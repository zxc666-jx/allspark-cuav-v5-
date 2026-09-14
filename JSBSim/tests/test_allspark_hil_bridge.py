"""Offline tests: no aircraft, COM ports or remote network endpoints."""

import json
import socket
import struct
import sys
import threading
import time
import unittest
import urllib.request
from pathlib import Path
from types import SimpleNamespace

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

from allspark_hil_bridge import AllsparkHILBridge, FAULT_PAYLOAD, RELAY_ACK, RELAY_HEADER
from fault_injection import FaultController, FaultSystemConfig
from hil_supervisor import JSBSimRawHILPublisher, JSBSIM_OUTPUT_PAYLOAD
import hil_supervisor as hil


def command(token=1, sysid=2, action=1, slot=0, fault_type=1, location=4,
            severity=1000, p1=5000, p2=0, ramp_in=0, delay=0, duration=10000):
    return RELAY_HEADER.pack(token, sysid, 128) + FAULT_PAYLOAD.pack(
        token & 65535, action, slot, fault_type, location, severity,
        p1, p2, ramp_in, 0, delay, duration,
    )


class MemoryPublisher(JSBSimRawHILPublisher):
    def __init__(self, faults):
        super().__init__(SimpleNamespace(), faults)
        self.incoming = b""
        self.sent = []

    def _read_available(self):
        incoming, self.incoming = self.incoming, b""
        return incoming

    def _write_frame(self, frame):
        self.sent.append(frame)

    def _transport_state(self):
        return {"transport": "memory-test"}


class AllsparkBridgeTests(unittest.TestCase):
    def setUp(self):
        self.faults = FaultController(FaultSystemConfig())
        self.bridge = AllsparkHILBridge(self.faults)

    def execute(self, payload, now=0.0):
        return RELAY_ACK.unpack(self.bridge.execute(payload, 2, now))[-2:]

    def test_wire_layout(self):
        self.assertEqual(FAULT_PAYLOAD.size, 28)
        self.assertEqual(len(command()), 34)
        self.assertEqual(RELAY_ACK.size, 9)

    def test_serial_loopback_routes_the_same_command(self):
        publisher = hil.JSBSimSerialHILPublisher(
            SimpleNamespace(device="loop://", baud=115200), self.faults,
        )
        worker = SimpleNamespace(config=SimpleNamespace(sysid=2),
                                 fdm=SimpleNamespace(get_sim_time=lambda: 0.0))
        try:
            publisher._port.write(publisher._pack_frame(b"JBF1", command()))
            publisher.receive_controls(worker)
            reply = publisher._port.read(publisher._port.in_waiting)
            self.assertEqual(reply[:4], b"JBA1")
            self.assertEqual(RELAY_ACK.unpack(reply[6:-2])[-2:], (0, 0))
            self.assertEqual(self.faults.status()["channels"][0]["level"], 0.5)
        finally:
            publisher.close()

    def test_shared_model_controller_and_ui_clear(self):
        self.assertEqual(self.execute(command()), (0, 0))
        # This is precisely the queue used by MP's HTTP clear endpoint.
        self.assertEqual(self.faults.status()["channels"][0]["fault_type"], "THRUST_LOSS")
        normal = dict(propulsion=0.8, aileron=0.2, elevator=0.1, rudder=0.0)
        self.assertAlmostEqual(self.faults.apply_actuators(normal)["propulsion"], 0.4)
        self.assertEqual(self.faults.transport_truth(0)["active_fault_count"], 1)
        ticket = self.faults.enqueue("fault.clear", {"slot": 0})
        self.faults.drain_commands(1.0)
        self.faults.update(1.0)
        self.assertEqual(ticket.result["status"], "applied")
        self.assertEqual(self.faults.apply_actuators(normal), normal)
        self.assertEqual(self.faults.transport_truth(0)["active_fault_count"], 0)

    def test_duplicate_does_not_restart_or_undo_ui_clear(self):
        payload = command(fault_type=2, ramp_in=10000)
        self.execute(payload)
        self.execute(payload, 5.0)
        self.faults.update(5.0)
        self.assertEqual(self.faults.status()["channels"][0]["factor"], 0.5)
        self.faults.enqueue("fault.reset", {})
        self.faults.drain_commands(6.0)
        self.execute(payload, 7.0)
        self.assertEqual(self.faults.status()["channels"], [])
        self.assertEqual(self.execute(command(p1=6000)), (3, 11))

    def test_wrong_aircraft_never_mutates_model(self):
        for sysid in (0, 1, 3, 10):
            self.assertEqual(self.execute(command(sysid=sysid)), (3, 4))
        self.assertEqual(self.faults.status()["channels"], [])

    def test_lost_ack_and_reconnected_transport_do_not_reinject(self):
        worker = SimpleNamespace(config=SimpleNamespace(sysid=2),
                                 fdm=SimpleNamespace(get_sim_time=lambda: 5.0))
        publisher = MemoryPublisher(self.faults)
        data = publisher._pack_frame(b"JBF1", command(fault_type=2, ramp_in=10000))
        publisher.incoming = data
        publisher.receive_controls(worker)
        publisher.sent.clear()  # Simulate an ACK lost on the serial/UDP link.
        self.faults.update(8.0)
        before = self.faults.status()["channels"][0]["configured_sim_time_s"]
        publisher.incoming = data  # Same transaction after temporary link loss.
        publisher.receive_controls(worker)
        self.assertEqual(RELAY_ACK.unpack(publisher.sent[-1][6:-2])[-2:], (0, 0))
        self.assertEqual(self.faults.status()["channels"][0]["configured_sim_time_s"], before)
        publisher.incoming = publisher._pack_frame(b"JBF1", command(token=2, action=0))
        publisher.receive_controls(worker)
        self.assertEqual(self.faults.status()["channels"], [])

    def test_bad_length_and_wrong_id_recover_to_next_valid_frame(self):
        publisher = MemoryPublisher(self.faults)
        worker = SimpleNamespace(config=SimpleNamespace(sysid=2),
                                 fdm=SimpleNamespace(get_sim_time=lambda: 0.0))
        publisher.incoming = (b"JBF1\xff\xffgarbage" +
                              publisher._pack_frame(b"JBF1", command(sysid=3)) +
                              publisher._pack_frame(b"JBF1", command()))
        publisher.receive_controls(worker)
        self.assertEqual([RELAY_ACK.unpack(reply[6:-2])[-2:] for reply in publisher.sent], [(3, 4), (0, 0)])
        self.assertEqual(len(self.faults.status()["channels"]), 1)

    def test_ten_aircraft_have_independent_controllers(self):
        controllers = [FaultController(FaultSystemConfig()) for _ in range(10)]
        for sysid, controller in enumerate(controllers, 1):
            reply = AllsparkHILBridge(controller).execute(command(sysid=sysid), sysid, 0.0)
            self.assertEqual(RELAY_ACK.unpack(reply)[-2:], (0, 0))
        controllers[1].enqueue("fault.reset", {})
        controllers[1].drain_commands(1.0)
        self.assertEqual([len(c.status()["channels"]) for c in controllers], [1, 0] + [1] * 8)

    def test_all_types_map_to_matching_truth(self):
        for kind in range(1, 14):
            faults = FaultController(FaultSystemConfig())
            location = 1 if kind in (3, 4, 5) else 0
            reply = AllsparkHILBridge(faults).execute(command(fault_type=kind, location=location), 2, 0.0)
            self.assertEqual(RELAY_ACK.unpack(reply)[-2:], (0, 0), kind)
            self.assertEqual(faults.transport_truth(0)["fault_type"], kind)

    def test_auto_slots_busy_update_reset_and_validation(self):
        for number in range(8):
            self.assertEqual(self.execute(command(token=number+1, slot=255)), (0, 0))
        self.assertEqual(self.execute(command(token=9, slot=255)), (3, 8))
        self.assertEqual(self.execute(command(token=10, slot=0)), (3, 8))
        self.assertEqual(self.execute(command(token=11, action=2, slot=7, p1=2500)), (0, 0))
        self.assertEqual(self.faults.status()["channels"][7]["level"], 0.25)
        self.assertEqual(self.execute(command(token=12, action=2, slot=7, p1=20000)), (3, 5))
        self.assertEqual(self.faults.status()["channels"][7]["level"], 0.25)
        self.assertEqual(self.execute(command(token=13, action=3)), (0, 0))
        self.assertEqual(self.faults.status()["channels"], [])

    def test_rejects_unsupported_or_ambiguous_inputs(self):
        for token, values in enumerate((dict(fault_type=14), dict(location=6),
                                       dict(severity=500), dict(ramp_in=100),
                                       dict(action=2, slot=255), dict(delay=3600001)), 1):
            self.assertEqual(self.execute(command(token=token, **values))[0], 3)
        self.assertEqual(self.faults.status()["channels"], [])
        disabled = AllsparkHILBridge(FaultController(FaultSystemConfig(enabled=False)))
        self.assertEqual(RELAY_ACK.unpack(disabled.execute(command(), 2, 0.0))[-2:], (3, 7))

    def test_fragmented_mixed_frames_crc_resync_and_reply(self):
        publisher = MemoryPublisher(self.faults)
        applied = []
        worker = SimpleNamespace(config=SimpleNamespace(sysid=2),
                                 fdm=SimpleNamespace(get_sim_time=lambda: 0.0),
                                 apply_pwm_outputs=lambda pwm, faults: applied.append(pwm))
        fault_frame = publisher._pack_frame(b"JBF1", command())
        bad = bytearray(fault_frame)
        bad[-1] ^= 1
        servo_frame = publisher._pack_frame(b"JBO1", JSBSIM_OUTPUT_PAYLOAD.pack(1, 1500, 1500, 1000, 1500))
        publisher.incoming = b"noise" + bytes(bad) + fault_frame[:11]
        publisher.receive_controls(worker)
        self.assertEqual(publisher.sent, [])
        publisher.incoming = fault_frame[11:] + servo_frame
        publisher.receive_controls(worker)
        self.assertEqual(len(publisher.sent), 1)
        reply = publisher.sent[0]
        self.assertEqual(reply[:6], b"JBA1\x09\x00")
        self.assertEqual(RELAY_ACK.unpack(reply[6:-2])[-2:], (0, 0))
        self.assertEqual(struct.unpack("<H", reply[-2:])[0], publisher._crc16_ccitt(reply[:-2]))
        self.assertEqual(applied, [(1500, 1500, 1000, 1500)])
        self.assertEqual(publisher.status_state()["allspark_fault_frames_rx"], 1)

    def test_real_jsbsim_udp_and_missionplanner_http_share_faults(self):
        # Load only model/spawn settings. Never open the configured remote radio.
        config = hil.load_config(Path(__file__).resolve().parents[1] / "config/vehicle03_udp_hil.yaml")
        worker = hil.JSBSimWorker(config)
        worker.initialize()
        sender = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        sender.bind(("127.0.0.1", 0))
        sender.settimeout(2.0)
        publisher = hil.JSBSimUdpHILPublisher(
            hil.UdpHILConfig(True, "127.0.0.1", 0, "127.0.0.1", sender.getsockname()[1], 100, 10),
            self.faults,
        )
        stop = threading.Event()
        server = hil.ControlAPIServer(hil.ControlAPIConfig(True, "127.0.0.1", 0),
                                      3, SimpleNamespace(status=lambda: {}), hil.RuntimeStatusStore(), stop.set, self.faults)
        errors = []

        def physics():
            try:
                while not stop.is_set():
                    now = worker.fdm.get_sim_time()
                    self.faults.drain_commands(now)
                    self.faults.update(now)
                    publisher.receive_controls(worker)
                    worker.step()
                    time.sleep(0.001)
            except Exception as exc:
                errors.append(exc)

        thread = threading.Thread(target=physics)
        try:
            server.start()
            thread.start()
            destination = publisher._socket.getsockname()
            sender.sendto(publisher._pack_frame(b"JBF1", command(sysid=3)), destination)
            reply = sender.recv(256)
            self.assertEqual(RELAY_ACK.unpack(reply[6:-2])[-2:], (0, 0))
            pwm = publisher._pack_frame(b"JBO1", JSBSIM_OUTPUT_PAYLOAD.pack(0, 1500, 1500, 1800, 1500))
            # Ordering barrier: next ACK follows PWM application in the same parser.
            sender.sendto(pwm + publisher._pack_frame(b"JBF1", command(sysid=3)), destination)
            sender.recv(256)
            api = f"http://127.0.0.1:{server._server.server_port}/api/v1/vehicles/3/faults"
            with urllib.request.urlopen(api, timeout=2.0) as response:
                snapshot = json.load(response)
            self.assertEqual(snapshot["channels"][0]["fault_type"], "THRUST_LOSS")
            self.assertAlmostEqual(snapshot["actuators"]["actual"]["propulsion"], 0.4)
            health_url = api.split("/api/")[0] + "/api/v1/health"
            with urllib.request.urlopen(health_url, timeout=2.0) as response:
                health = json.load(response)
            self.assertEqual(health["allspark_bridge"]["sensor_magic"], "JBS2")
            request = urllib.request.Request(api + "/0/clear", data=b"{}", method="POST",
                                             headers={"Content-Type": "application/json"})
            with urllib.request.urlopen(request, timeout=2.0) as response:
                self.assertEqual(json.load(response)["status"], "applied")
            sender.sendto(pwm + publisher._pack_frame(b"JBF1", command(sysid=3)), destination)
            sender.recv(256)
            # Reverse direction: MP HTTP injection must appear in JBS2 truth.
            mp_fault = {"fault_type": "SENSOR_BIAS", "target": "AIRSPEED", "profile": "step",
                        "level": 3.0, "duration_s": 0.0}
            request = urllib.request.Request(api + "/3", data=json.dumps(mp_fault).encode(), method="PUT",
                                             headers={"Content-Type": "application/json"})
            with urllib.request.urlopen(request, timeout=2.0) as response:
                self.assertEqual(json.load(response)["status"], "applied")
            stop.set()
            thread.join(2.0)
            self.assertEqual(errors, [])
            self.assertAlmostEqual(worker.fdm["fcs/throttle-cmd-norm"], 0.8)
            truth = self.faults.transport_truth(0)
            self.assertEqual((truth["active_fault_count"], truth["fault_slot"], truth["fault_type"]), (1, 3, 8))
            sensor_frame = publisher._sensor_frame(worker, worker.fdm.get_sim_time())
            packed_truth = hil.JSBSIM_TRUTH_PAYLOAD.unpack_from(sensor_frame, 6 + hil.JSBSIM_SENSOR_PAYLOAD.size)
            self.assertEqual(packed_truth[:6], (3, 1, 1 << 3, 3, 8, 5))
            self.assertEqual(packed_truth[8], 30000)
        finally:
            stop.set()
            if thread.ident is not None:
                thread.join(2.0)
            server.close()
            publisher.close()
            sender.close()


if __name__ == "__main__":
    unittest.main()

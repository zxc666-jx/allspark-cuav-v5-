import socket
import tempfile
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest import mock

import yaml


JSBSIM_DIR = Path(__file__).resolve().parents[1]

import sys

if str(JSBSIM_DIR) not in sys.path:
    sys.path.insert(0, str(JSBSIM_DIR))

import hil_fleet_supervisor as fleet
from hil_supervisor import (
    JSBSIM_OUTPUT_MAGIC,
    JSBSIM_OUTPUT_PAYLOAD,
    JSBSIM_SENSOR_MAGIC,
    JSBSIM_SENSOR_PAYLOAD,
    JSBSIM_TRUTH_PAYLOAD,
    JSBSimUdpHILPublisher,
    MavlinkConfig,
    MavlinkSensorPublisher,
    UdpHILConfig,
    load_config,
)


class FleetConfigurationTests(unittest.TestCase):
    def test_udp_smoke_configuration_loads(self):
        definition = fleet.load_fleet_config(JSBSIM_DIR / "config" / "fleet_udp_only.yaml")

        self.assertEqual(8750, definition.settings.port)
        self.assertEqual(1, len(definition.vehicles))
        self.assertEqual(3, definition.vehicles[0].config.sysid)
        self.assertEqual(
            "udp-jbs:0.0.0.0:14603->192.168.1.203:14603",
            definition.vehicles[0].transport,
        )

    def test_duplicate_sysid_is_rejected(self):
        vehicle_config = JSBSIM_DIR / "config" / "vehicle03_udp_hil.yaml"
        document = {
            "fleet": {"host": "127.0.0.1", "port": 18750},
            "vehicles": [
                {"name": "first", "config": str(vehicle_config)},
                {"name": "second", "config": str(vehicle_config)},
            ],
        }
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "fleet.yaml"
            path.write_text(yaml.safe_dump(document), encoding="utf-8")
            with self.assertRaisesRegex(ValueError, "duplicate vehicle sysid"):
                fleet.load_fleet_config(path)


class FleetAndUdpTests(unittest.TestCase):
    def test_jbs2_truth_extension_layout_is_stable(self):
        self.assertEqual(JSBSIM_SENSOR_MAGIC, b"JBS2")
        self.assertEqual(JSBSIM_SENSOR_PAYLOAD.size, 116)
        self.assertEqual(JSBSIM_TRUTH_PAYLOAD.size, 36)

    def test_udp_jbs_output_frame_is_applied_to_jsbsim(self):
        faults = mock.Mock()
        publisher = JSBSimUdpHILPublisher(
            UdpHILConfig(True, "127.0.0.1", 0, "127.0.0.1", 9, 100, 10),
            faults,
        )
        sender = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        try:
            payload = JSBSIM_OUTPUT_PAYLOAD.pack(123, 1100, 1200, 1300, 1400)
            frame = publisher._pack_frame(JSBSIM_OUTPUT_MAGIC, payload)
            sender.sendto(frame, publisher._socket.getsockname())
            worker = mock.Mock()

            publisher.receive_controls(worker)

            worker.apply_pwm_outputs.assert_called_once_with((1100, 1200, 1300, 1400), faults)
            self.assertEqual("udp-jsbsim", publisher.status_state()["transport"])
        finally:
            sender.close()
            publisher.close()

    def test_hil_actuator_controls_are_applied_to_jsbsim(self):
        faults = mock.Mock()
        publisher = MavlinkSensorPublisher(
            MavlinkConfig(False, "127.0.0.1", 14561, 100, 10, 1, 200),
            faults,
        )
        message = SimpleNamespace(controls=[0.5, -0.25, 0.75, -1.0])
        message.get_type = lambda: "HIL_ACTUATOR_CONTROLS"
        connection = mock.Mock()
        connection.recv_match.side_effect = [message, None]
        publisher._connection = connection
        worker = mock.Mock()

        publisher.receive_controls(worker)

        worker.apply_pwm_outputs.assert_called_once_with((1750, 1375, 1750, 1000), faults)

    def test_bluetooth_serial_port_is_rejected(self):
        config_path = JSBSIM_DIR / "config" / "vehicle02_serial_hil.yaml"
        spec = fleet.VehicleSpec("bluetooth mistake", config_path, load_config(config_path))

        with mock.patch.object(
            fleet,
            "_windows_serial_devices",
            return_value={spec.config.serial_hil.device.casefold(): r"\Device\BthModem5"},
        ):
            with self.assertRaisesRegex(ValueError, "Bluetooth modem"):
                fleet._validate_windows_serial_devices([spec])

    def test_missing_usb_serial_port_is_allowed_during_offline_validation(self):
        config_path = JSBSIM_DIR / "config" / "vehicle02_serial_hil.yaml"
        spec = fleet.VehicleSpec("unplugged adapter", config_path, load_config(config_path))

        with mock.patch.object(fleet, "_windows_serial_devices", return_value={}):
            fleet._validate_windows_serial_devices([spec])

    def test_more_than_ten_vehicle_slots_is_rejected(self):
        vehicle_config = JSBSIM_DIR / "config" / "vehicle03_udp_hil.yaml"
        document = {
            "fleet": {"host": "127.0.0.1", "port": 18750},
            "vehicles": [
                {"name": f"vehicle-{index}", "config": str(vehicle_config), "enabled": False}
                for index in range(11)
            ],
        }
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "fleet.yaml"
            path.write_text(yaml.safe_dump(document), encoding="utf-8")
            with self.assertRaisesRegex(ValueError, "at most 10 vehicles"):
                fleet.load_fleet_config(path)


if __name__ == "__main__":
    unittest.main()

import sys
import threading
import unittest
from pathlib import Path


JSBSIM_DIR = Path(__file__).resolve().parents[1]
if str(JSBSIM_DIR) not in sys.path:
    sys.path.insert(0, str(JSBSIM_DIR))

from fault_injection import FaultController, FaultSystemConfig


class NullLogger:
    def log_event(self, _event):
        pass


class FaultControllerTests(unittest.TestCase):
    def setUp(self):
        self.controller = FaultController(FaultSystemConfig(True, 8, 12345), NullLogger())

    def command(self, kind, payload, sim_time=0.0):
        result = []

        def submit():
            result.append(self.controller.submit(kind, payload))

        thread = threading.Thread(target=submit)
        thread.start()
        while self.controller._commands.empty():
            pass
        self.controller.drain_commands(sim_time)
        thread.join(timeout=1.0)
        self.assertEqual(result[0]["status"], "applied")

    def configure(self, slot, **values):
        payload = {
            "enabled": True,
            "fault_type": "THRUST_LOSS",
            "target": "PROPULSION",
            "profile": "step",
            "level": 0.3,
            "secondary": 0.0,
            "start_delay_s": 0.0,
            "ramp_in_s": 0.0,
            "duration_s": 10.0,
            "ramp_out_s": 0.0,
            "auto_recover": True,
        }
        payload.update(values)
        self.command("fault.configure", {"slot": slot, "channel": payload})

    def test_thrust_loss_ramp_changes_actual_not_normal(self):
        self.configure(0, profile="ramp", level=0.4, ramp_in_s=10.0)
        self.controller.update(5.0)
        normal = {"propulsion": 1.0, "aileron": 0.2, "elevator": 0.1, "rudder": -0.1}
        actual = self.controller.apply_actuators(normal)
        self.assertAlmostEqual(actual["propulsion"], 0.8)
        self.assertEqual(normal["propulsion"], 1.0)
        self.assertEqual(actual["aileron"], normal["aileron"])

    def test_compound_surface_and_thrust_losses_are_independent(self):
        self.configure(0, level=0.5)
        self.configure(
            1,
            fault_type="SURFACE_LOSS",
            target="ELEVATOR",
            level=0.25,
        )
        self.controller.update(1.0)
        normal = {"propulsion": 0.8, "aileron": 0.1, "elevator": 0.4, "rudder": 0.0}
        actual = self.controller.apply_actuators(normal)
        self.assertAlmostEqual(actual["propulsion"], 0.4)
        self.assertAlmostEqual(actual["elevator"], 0.3)

    def test_sensor_bias_does_not_change_raw_value(self):
        self.configure(
            0,
            fault_type="SENSOR_BIAS",
            target="AIRSPEED",
            level=5.0,
        )
        self.controller.update(1.0)
        measured, valid = self.controller.apply_sensor("AIRSPEED", 20.0, 1.0)
        self.assertEqual(measured, 25.0)
        self.assertTrue(valid)
        self.assertEqual(self.controller.status()["sensors"]["AIRSPEED"]["raw"], 20.0)

    def test_sensor_fail_freezes_then_reset_restores_updates(self):
        self.configure(0, fault_type="SENSOR_FAIL", target="ALTITUDE", level=0.0)
        self.controller.update(1.0)
        first, valid = self.controller.apply_sensor("ALTITUDE", 100.0, 1.0)
        second, valid_second = self.controller.apply_sensor("ALTITUDE", 120.0, 2.0)
        self.assertEqual(first, second)
        self.assertFalse(valid)
        self.assertFalse(valid_second)

        self.command("fault.reset", {}, sim_time=2.0)
        self.controller.update(2.1)
        restored, restored_valid = self.controller.apply_sensor("ALTITUDE", 120.0, 2.1)
        self.assertEqual(restored, 120.0)
        self.assertTrue(restored_valid)

    def test_noise_is_reproducible_for_same_seed(self):
        other = FaultController(FaultSystemConfig(True, 8, 12345), NullLogger())
        self.configure(0, fault_type="SENSOR_NOISE", target="AIRSPEED", level=2.0)

        payload = {
            "slot": 0,
            "channel": {
                "enabled": True,
                "fault_type": "SENSOR_NOISE",
                "target": "AIRSPEED",
                "profile": "step",
                "level": 2.0,
                "secondary": 0.0,
                "start_delay_s": 0.0,
                "ramp_in_s": 0.0,
                "duration_s": 10.0,
                "ramp_out_s": 0.0,
                "auto_recover": True,
            },
        }
        result = []
        thread = threading.Thread(target=lambda: result.append(other.submit("fault.configure", payload)))
        thread.start()
        while other._commands.empty():
            pass
        other.drain_commands(0.0)
        thread.join(timeout=1.0)
        self.controller.update(1.0)
        other.update(1.0)
        sequence_a = [self.controller.apply_sensor("AIRSPEED", 20.0, 1.0)[0] for _ in range(5)]
        sequence_b = [other.apply_sensor("AIRSPEED", 20.0, 1.0)[0] for _ in range(5)]
        self.assertEqual(sequence_a, sequence_b)

    def test_transport_truth_reports_actual_outputs_and_rotates_faults(self):
        self.configure(0, profile="ramp", level=0.4, ramp_in_s=10.0)
        self.configure(
            3,
            fault_type="SURFACE_JAM",
            target="ELEVATOR",
            level=5.0,
        )
        self.controller.update(5.0)
        self.controller.apply_actuators(
            {"propulsion": 1.0, "aileron": 0.2, "elevator": 0.1, "rudder": -0.1}
        )

        first = self.controller.transport_truth(0)
        second = self.controller.transport_truth(1)

        self.assertEqual(first["active_fault_count"], 2)
        self.assertEqual(first["active_fault_mask"], 0b1001)
        self.assertEqual(first["fault_slot"], 0)
        self.assertEqual(first["fault_type"], 2)  # 推力损失-缓变
        self.assertEqual(first["fault_location"], 4)
        self.assertEqual(first["fault_severity_permille"], 500)
        self.assertEqual(first["actual_throttle_permille"], (800, 0))
        self.assertEqual(second["fault_slot"], 3)
        self.assertEqual(second["fault_type"], 3)  # 舵面卡死
        self.assertEqual(second["fault_location"], 1)
        self.assertEqual(second["actual_surface_cd"][1], 500)
        self.assertEqual(first["valid_mask"], 0b1110)


if __name__ == "__main__":
    unittest.main()

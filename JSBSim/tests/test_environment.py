import json
import sys
import threading
import time
import unittest
import urllib.request
from pathlib import Path


JSBSIM_DIR = Path(__file__).resolve().parents[1]
if str(JSBSIM_DIR) not in sys.path:
    sys.path.insert(0, str(JSBSIM_DIR))

import hil_supervisor as hil


class NullLogger:
    def log_event(self, _event):
        pass


class FakeFDM(dict):
    def __init__(self):
        super().__init__()
        self.sim_time = 0.0
        self["atmosphere/total-wind-north-fps"] = 0.0
        self["atmosphere/total-wind-east-fps"] = 0.0
        self["atmosphere/total-wind-down-fps"] = 0.0

    def get_sim_time(self):
        return self.sim_time


def default_environment():
    return hil.EnvironmentConfig(
        hil.SteadyWindConfig(False, 0.0, 0.0, 0.0),
        hil.TurbulenceConfig(False, "light", 3, 7.62, 12345),
        hil.GustConfig(5.0, 0.0, 0.0, 1.0, 1.0, 1.0),
    )


class WindConversionTests(unittest.TestCase):
    def assertVectorAlmostEqual(self, actual, expected):
        for actual_value, expected_value in zip(actual, expected):
            self.assertAlmostEqual(actual_value, expected_value, places=9)

    def test_cardinal_wind_from_directions(self):
        self.assertVectorAlmostEqual(hil.wind_from_to_ned_mps(10.0, 0.0, 0.0), (-10.0, 0.0, 0.0))
        self.assertVectorAlmostEqual(hil.wind_from_to_ned_mps(10.0, 90.0, 0.0), (0.0, -10.0, 0.0))
        self.assertVectorAlmostEqual(hil.wind_from_to_ned_mps(10.0, 180.0, 0.0), (10.0, 0.0, 0.0))
        self.assertVectorAlmostEqual(hil.wind_from_to_ned_mps(10.0, 270.0, 0.0), (0.0, 10.0, 0.0))

    def test_positive_vertical_wind_is_negative_down(self):
        self.assertVectorAlmostEqual(hil.wind_from_to_ned_mps(0.0, 0.0, 3.5), (0.0, 0.0, -3.5))


class GustProfileTests(unittest.TestCase):
    def test_cosine_profile_is_continuous_and_returns_to_zero(self):
        gust = default_environment().gust
        self.assertEqual(hil.EnvironmentController.gust_profile(gust, 0.0), (0.0, "ramp_in"))
        factor, phase = hil.EnvironmentController.gust_profile(gust, 0.5)
        self.assertEqual(phase, "ramp_in")
        self.assertAlmostEqual(factor, 0.5)
        self.assertEqual(hil.EnvironmentController.gust_profile(gust, 1.0), (1.0, "hold"))
        self.assertEqual(hil.EnvironmentController.gust_profile(gust, 2.0), (1.0, "ramp_out"))
        factor, phase = hil.EnvironmentController.gust_profile(gust, 2.5)
        self.assertEqual(phase, "ramp_out")
        self.assertAlmostEqual(factor, 0.5)
        self.assertEqual(hil.EnvironmentController.gust_profile(gust, 3.0), (0.0, "idle"))


class EnvironmentControllerTests(unittest.TestCase):
    def setUp(self):
        self.fdm = FakeFDM()
        self.controller = hil.EnvironmentController(self.fdm, default_environment(), NullLogger())
        self.controller.initialize()

    def submit_and_drain(self, kind, payload, sim_time=2.5):
        result_holder = []

        def submit():
            result_holder.append(self.controller.submit(kind, payload, timeout_s=1.0))

        thread = threading.Thread(target=submit)
        thread.start()
        while self.controller._commands.empty():
            thread.join(0.001)
        self.controller.drain_commands(sim_time)
        thread.join(1.0)
        self.assertFalse(thread.is_alive())
        return result_holder[0]

    def test_apply_wind_and_turbulence_writes_jsbsim_properties(self):
        result = self.submit_and_drain(
            "environment.apply",
            {
                "steady_wind": {
                    "enabled": True,
                    "speed_mps": 12.0,
                    "direction_from_deg": 90.0,
                    "vertical_mps": 2.0,
                },
                "turbulence": {"enabled": True, "preset": "moderate", "random_seed": 99},
            },
        )
        self.assertEqual(result["status"], "applied")
        self.assertAlmostEqual(self.fdm["atmosphere/wind-north-fps"], 0.0, places=8)
        self.assertAlmostEqual(self.fdm["atmosphere/wind-east-fps"], -12.0 * hil.M_TO_FT)
        self.assertAlmostEqual(self.fdm["atmosphere/wind-down-fps"], -2.0 * hil.M_TO_FT)
        self.assertEqual(self.fdm["atmosphere/turb-type"], 3.0)
        self.assertEqual(self.fdm["atmosphere/turbulence/milspec/severity"], 4.0)
        self.assertAlmostEqual(
            self.fdm["atmosphere/turbulence/milspec/windspeed_at_20ft_AGL-fps"],
            15.24 * hil.M_TO_FT,
        )

    def test_reset_clears_all_environment_vectors(self):
        self.submit_and_drain(
            "environment.apply",
            {"steady_wind": {"enabled": True, "speed_mps": 10.0}},
        )
        self.submit_and_drain("environment.gust", {"magnitude_mps": 6.0})
        self.controller.update(3.0)
        result = self.submit_and_drain("environment.reset", {})
        self.assertEqual(result["status"], "applied")
        for name in ("wind-north-fps", "wind-east-fps", "wind-down-fps", "gust-north-fps",
                     "gust-east-fps", "gust-down-fps", "turb-north-fps", "turb-east-fps",
                     "turb-down-fps"):
            self.assertEqual(self.fdm["atmosphere/" + name], 0.0)
        self.assertEqual(self.fdm["atmosphere/turb-type"], 0.0)

    def test_invalid_severity_is_rejected_without_changing_settings(self):
        result = self.submit_and_drain(
            "environment.apply",
            {"turbulence": {"enabled": True, "preset": "custom", "severity": 8}},
        )
        self.assertEqual(result["status"], "rejected")
        self.assertFalse(self.controller.status()["settings"]["turbulence"]["enabled"])


class JSBSimDeterminismTests(unittest.TestCase):
    def turbulence_samples(self, seed):
        fdm = hil.jsbsim.FGFDMExec(None)
        fdm.set_debug_level(0)
        fdm.set_dt(1.0 / 400.0)
        self.assertTrue(fdm.load_model("c172x"))
        fdm.disable_output()
        fdm["ic/h-sl-ft"] = 1000.0
        fdm["ic/vc-kts"] = 70.0
        self.assertTrue(fdm.run_ic())
        config = hil.EnvironmentConfig(
            hil.SteadyWindConfig(False, 0.0, 0.0, 0.0),
            hil.TurbulenceConfig(True, "custom", 3, 7.62, seed),
            default_environment().gust,
        )
        controller = hil.EnvironmentController(fdm, config, NullLogger())
        controller.initialize()
        samples = []
        for _ in range(30):
            self.assertTrue(fdm.run())
            samples.append(round(fdm["atmosphere/turb-north-fps"], 10))
        return samples

    def test_same_seed_repeats_turbulence_sequence(self):
        self.assertEqual(self.turbulence_samples(24680), self.turbulence_samples(24680))


class ControlAPITests(unittest.TestCase):
    def setUp(self):
        self.fdm = FakeFDM()
        self.controller = hil.EnvironmentController(self.fdm, default_environment(), NullLogger())
        self.controller.initialize()
        self.faults = hil.FaultController(hil.FaultSystemConfig(True, 8, 12345), NullLogger())
        self.runtime = hil.RuntimeStatusStore()
        self.stop_requested = threading.Event()
        self.physics_stop = threading.Event()
        self.server = hil.ControlAPIServer(
            hil.ControlAPIConfig(True, "127.0.0.1", 0),
            1,
            self.controller,
            self.runtime,
            self.stop_requested.set,
            self.faults,
        )
        self.server.start()
        self.base_url = "http://127.0.0.1:%d" % self.server._server.server_port

        def physics_loop():
            while not self.physics_stop.is_set():
                self.fdm.sim_time += 0.0025
                self.controller.drain_commands(self.fdm.sim_time)
                self.controller.update(self.fdm.sim_time)
                self.controller.capture_actual_wind()
                self.faults.drain_commands(self.fdm.sim_time)
                self.faults.update(self.fdm.sim_time)
                time.sleep(0.001)

        self.physics_thread = threading.Thread(target=physics_loop)
        self.physics_thread.start()

    def tearDown(self):
        self.physics_stop.set()
        self.physics_thread.join(1.0)
        self.server.close()

    def request(self, method, path, body=None):
        data = None if body is None else json.dumps(body).encode("utf-8")
        request = urllib.request.Request(
            self.base_url + path,
            data=data,
            method=method,
            headers={"Content-Type": "application/json"},
        )
        with urllib.request.urlopen(request, timeout=2.0) as response:
            return response.status, json.loads(response.read().decode("utf-8"))

    def test_health_apply_gust_reset_and_shutdown_routes(self):
        status, health = self.request("GET", "/api/v1/health")
        self.assertEqual(status, 200)
        self.assertEqual(health["api"], "ready")
        self.assertEqual(health["sysid"], 1)

        status, result = self.request(
            "PUT",
            "/api/v1/vehicles/1/environment",
            {"steady_wind": {"enabled": True, "speed_mps": 8.0, "direction_from_deg": 180.0}},
        )
        self.assertEqual(status, 200)
        self.assertEqual(result["status"], "applied")

        status, result = self.request(
            "POST",
            "/api/v1/vehicles/1/environment/gust",
            {"magnitude_mps": 5.0, "ramp_in_s": 0.5, "hold_s": 0.5, "ramp_out_s": 0.5},
        )
        self.assertEqual(status, 200)
        self.assertEqual(result["status"], "applied")

        status, result = self.request("POST", "/api/v1/vehicles/1/environment/reset", {})
        self.assertEqual(status, 200)
        self.assertEqual(result["status"], "applied")

        status, result = self.request("POST", "/api/v1/shutdown", {})
        self.assertEqual(status, 202)
        self.assertEqual(result["status"], "stopping")
        self.assertTrue(self.stop_requested.wait(0.5))

    def test_fault_configure_status_clear_and_reset_routes(self):
        fault = {
            "enabled": True,
            "fault_type": "SURFACE_LOSS",
            "target": "ELEVATOR",
            "profile": "ramp",
            "level": 0.4,
            "secondary": 0.0,
            "start_delay_s": 0.0,
            "ramp_in_s": 2.0,
            "duration_s": 10.0,
            "ramp_out_s": 1.0,
            "auto_recover": True,
        }
        status, result = self.request("PUT", "/api/v1/vehicles/1/faults/2", fault)
        self.assertEqual(status, 200)
        self.assertEqual(result["status"], "applied")

        status, faults = self.request("GET", "/api/v1/vehicles/1/faults")
        self.assertEqual(status, 200)
        self.assertEqual(faults["channels"][0]["slot"], 2)
        self.assertEqual(faults["channels"][0]["target"], "ELEVATOR")

        status, result = self.request("POST", "/api/v1/vehicles/1/faults/2/clear", {})
        self.assertEqual(status, 200)
        self.assertEqual(result["status"], "applied")

        self.request("PUT", "/api/v1/vehicles/1/faults/0", fault)
        status, result = self.request("POST", "/api/v1/vehicles/1/faults/reset", {})
        self.assertEqual(status, 200)
        self.assertEqual(result["status"], "applied")
        self.assertEqual(self.faults.status()["channels"], [])


if __name__ == "__main__":
    unittest.main()

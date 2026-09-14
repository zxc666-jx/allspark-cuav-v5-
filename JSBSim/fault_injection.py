"""JSBSim 半实物仿真的线程安全故障注入控制器。

HTTP 线程只提交命令；配置应用、生命周期推进、执行机构和传感器故障计算
全部由 hil_supervisor 的固定频率物理线程完成。
"""

from __future__ import annotations

import math
import queue
import random
import threading
import uuid
from dataclasses import asdict, dataclass
from typing import Any


FAULT_TYPES = (
    "THRUST_LOSS",
    "SURFACE_LOSS",
    "SURFACE_JAM",
    "SENSOR_NOISE",
    "SENSOR_DRIFT",
    "SENSOR_BIAS",
    "SENSOR_FAIL",
)
SURFACE_TARGETS = ("AILERON", "ELEVATOR", "RUDDER")
SENSOR_TARGETS = ("AIRSPEED", "ALTITUDE")
TARGETS = ("PROPULSION",) + SURFACE_TARGETS + SENSOR_TARGETS

# JSBSim c172x 的归一化舵面输入换算为近似物理舵偏角。卡死故障以度为单位，
# 在执行机构层转换回归一化实际舵面量。
SURFACE_MAX_DEFLECTION_DEG = {
    "AILERON": 20.0,
    "ELEVATOR": 25.0,
    "RUDDER": 30.0,
}

# Allspark V1.1 的紧凑故障枚举。模型内部使用更容易理解的字符串，只有在
# JBS2 真值扩展帧出口处转换成飞控/Allspark 共用的数值。
PROTOCOL_FAULT_LOCATION = {
    "ELEVATOR": 1,
    "RUDDER": 2,
    "AILERON": 3,
    "PROPULSION": 4,
    "AIRSPEED": 5,
    "ALTITUDE": 6,
}
PROTOCOL_FAULT_STATE = {
    "pending": 2,
    "ramp_in": 3,
    "active": 4,
    "ramp_out": 5,
}


@dataclass(frozen=True)
class FaultSystemConfig:
    enabled: bool = True
    max_channels: int = 8
    random_seed: int = 24680


@dataclass(frozen=True)
class FaultChannelConfig:
    enabled: bool
    fault_type: str
    target: str
    profile: str
    level: float
    secondary: float
    start_delay_s: float
    ramp_in_s: float
    duration_s: float
    ramp_out_s: float
    auto_recover: bool


@dataclass
class FaultCommand:
    command_id: str
    kind: str
    payload: dict[str, Any]
    completed: threading.Event
    result: dict[str, Any] | None = None


@dataclass
class FaultChannelRuntime:
    config: FaultChannelConfig
    configured_sim_s: float
    phase: str = "pending"
    factor: float = 0.0
    last_phase: str = ""


def _finite(value: Any, name: str) -> float:
    number = float(value)
    if not math.isfinite(number):
        raise ValueError(f"{name} must be finite")
    return number


class FaultController:
    """管理独立故障通道，并提供执行机构/传感器测量变换。"""

    def __init__(self, config: FaultSystemConfig, logger: Any | None = None) -> None:
        self.config = config
        self._logger = logger
        self._commands: queue.Queue[FaultCommand] = queue.Queue(maxsize=128)
        self._channels: dict[int, FaultChannelRuntime] = {}
        self._status_lock = threading.Lock()
        self._latest_sim_s = 0.0
        self._random = {
            slot: random.Random(config.random_seed + slot * 1009)
            for slot in range(config.max_channels)
        }
        self._frozen_measurements: dict[str, float] = {}
        self._sensor_state: dict[str, dict[str, Any]] = {
            target: {"raw": None, "measured": None, "valid": True}
            for target in SENSOR_TARGETS
        }
        self._actuator_state: dict[str, dict[str, float]] = {
            "normal": {},
            "actual": {},
        }

    @staticmethod
    def _validate_channel(payload: dict[str, Any]) -> FaultChannelConfig:
        if not isinstance(payload, dict):
            raise ValueError("fault channel must be a JSON object")
        fault_type = str(payload.get("fault_type", "THRUST_LOSS")).upper()
        target = str(payload.get("target", "PROPULSION")).upper()
        profile = str(payload.get("profile", "step")).lower()
        if fault_type not in FAULT_TYPES:
            raise ValueError("unsupported fault_type")
        if target not in TARGETS:
            raise ValueError("unsupported fault target")
        if profile not in ("step", "ramp"):
            raise ValueError("profile must be step or ramp")

        if fault_type == "THRUST_LOSS" and target != "PROPULSION":
            raise ValueError("THRUST_LOSS target must be PROPULSION")
        if fault_type in ("SURFACE_LOSS", "SURFACE_JAM") and target not in SURFACE_TARGETS:
            raise ValueError("surface fault target must be AILERON, ELEVATOR or RUDDER")
        if fault_type.startswith("SENSOR_") and target not in SENSOR_TARGETS:
            raise ValueError("sensor fault target must be AIRSPEED or ALTITUDE")

        level = _finite(payload.get("level", 0.0), "level")
        secondary = _finite(payload.get("secondary", 0.0), "secondary")
        start_delay_s = _finite(payload.get("start_delay_s", 0.0), "start_delay_s")
        ramp_in_s = _finite(payload.get("ramp_in_s", 0.0), "ramp_in_s")
        duration_s = _finite(payload.get("duration_s", 10.0), "duration_s")
        ramp_out_s = _finite(payload.get("ramp_out_s", 0.0), "ramp_out_s")
        if any(value < 0.0 or value > 3600.0 for value in
               (start_delay_s, ramp_in_s, duration_s, ramp_out_s)):
            raise ValueError("fault times must be in the range 0..3600 seconds")
        if fault_type in ("THRUST_LOSS", "SURFACE_LOSS") and not 0.0 <= level <= 1.0:
            raise ValueError("loss level must be in the range 0..1")
        if fault_type == "SURFACE_JAM" and not -45.0 <= level <= 45.0:
            raise ValueError("surface jam angle must be in the range -45..45 degrees")
        if fault_type == "SENSOR_NOISE" and not 0.0 <= level <= 1000.0:
            raise ValueError("sensor noise sigma must be in the range 0..1000")
        if fault_type == "SENSOR_DRIFT" and not -100.0 <= level <= 100.0:
            raise ValueError("sensor drift rate must be in the range -100..100")
        if fault_type == "SENSOR_DRIFT" and not 0.0 <= secondary <= 10000.0:
            raise ValueError("sensor drift limit must be in the range 0..10000")
        if fault_type == "SENSOR_BIAS" and not -10000.0 <= level <= 10000.0:
            raise ValueError("sensor bias must be in the range -10000..10000")

        return FaultChannelConfig(
            enabled=bool(payload.get("enabled", True)),
            fault_type=fault_type,
            target=target,
            profile=profile,
            level=level,
            secondary=secondary,
            start_delay_s=start_delay_s,
            ramp_in_s=ramp_in_s,
            duration_s=duration_s,
            ramp_out_s=ramp_out_s,
            auto_recover=bool(payload.get("auto_recover", True)),
        )

    def enqueue(self, kind: str, payload: dict[str, Any]) -> FaultCommand:
        """Nonblocking entry shared by MissionPlanner HTTP and the HIL relay."""
        command = FaultCommand(str(uuid.uuid4()), kind, payload, threading.Event())
        try:
            self._commands.put_nowait(command)
        except queue.Full as exc:
            raise RuntimeError("fault command queue is full") from exc
        return command

    def submit(self, kind: str, payload: dict[str, Any], timeout_s: float = 1.0) -> dict[str, Any]:
        command = self.enqueue(kind, payload)
        if command.completed.wait(timeout_s):
            return command.result or {
                "command_id": command.command_id,
                "status": "failed",
                "error": "command completed without a result",
            }
        return {
            "command_id": command.command_id,
            "status": "accepted",
            "applied_sim_time_s": None,
            "error": None,
        }

    def drain_commands(self, sim_time_s: float) -> None:
        """只能从物理线程调用。"""
        while True:
            try:
                command = self._commands.get_nowait()
            except queue.Empty:
                return
            try:
                if not self.config.enabled:
                    raise ValueError("fault injection is disabled by configuration")
                if command.kind == "fault.configure":
                    slot = int(command.payload["slot"])
                    if not 0 <= slot < self.config.max_channels:
                        raise ValueError(f"slot must be in the range 0..{self.config.max_channels - 1}")
                    channel = self._validate_channel(command.payload.get("channel", command.payload))
                    self._channels[slot] = FaultChannelRuntime(channel, sim_time_s)
                    self._random[slot].seed(self.config.random_seed + slot * 1009)
                elif command.kind == "fault.clear":
                    slot = int(command.payload["slot"])
                    self._channels.pop(slot, None)
                elif command.kind == "fault.reset":
                    self._channels.clear()
                    self._frozen_measurements.clear()
                else:
                    raise ValueError(f"unsupported command kind: {command.kind}")
                command.result = {
                    "command_id": command.command_id,
                    "status": "applied",
                    "applied_sim_time_s": round(sim_time_s, 6),
                    "error": None,
                }
                self._log_event(command.kind, sim_time_s, command.payload, "applied")
            except (KeyError, TypeError, ValueError) as exc:
                command.result = {
                    "command_id": command.command_id,
                    "status": "rejected",
                    "applied_sim_time_s": None,
                    "error": str(exc),
                }
                self._log_event(command.kind, sim_time_s, command.payload, "rejected", str(exc))
            finally:
                command.completed.set()
                self._commands.task_done()

    @staticmethod
    def _lifecycle(runtime: FaultChannelRuntime, sim_time_s: float) -> tuple[str, float]:
        config = runtime.config
        if not config.enabled:
            return "disabled", 0.0
        elapsed = sim_time_s - runtime.configured_sim_s - config.start_delay_s
        if elapsed < 0.0:
            return "pending", 0.0
        if config.profile == "ramp" and config.ramp_in_s > 0.0 and elapsed < config.ramp_in_s:
            return "ramp_in", max(0.0, min(1.0, elapsed / config.ramp_in_s))
        elapsed -= config.ramp_in_s if config.profile == "ramp" else 0.0
        if not config.auto_recover or config.duration_s == 0.0:
            return "active", 1.0
        if elapsed < config.duration_s:
            return "active", 1.0
        elapsed -= config.duration_s
        if config.ramp_out_s > 0.0 and elapsed < config.ramp_out_s:
            return "ramp_out", max(0.0, min(1.0, 1.0 - elapsed / config.ramp_out_s))
        return "complete", 0.0

    def update(self, sim_time_s: float) -> None:
        self._latest_sim_s = sim_time_s
        for slot, runtime in self._channels.items():
            runtime.phase, runtime.factor = self._lifecycle(runtime, sim_time_s)
            if runtime.phase != runtime.last_phase:
                self._log_event(
                    "fault.phase",
                    sim_time_s,
                    {"slot": slot, "phase": runtime.phase, "channel": asdict(runtime.config)},
                    "applied",
                )
                runtime.last_phase = runtime.phase
        self._clear_unused_freezes()

    def _active(self) -> list[tuple[int, FaultChannelRuntime]]:
        return [
            (slot, runtime)
            for slot, runtime in sorted(self._channels.items())
            if runtime.factor > 0.0 and runtime.phase not in ("disabled", "complete", "pending")
        ]

    def apply_actuators(self, normal: dict[str, float]) -> dict[str, float]:
        """在飞控 PWM 解码后的实际执行机构层应用故障。"""
        actual = dict(normal)
        jammed: set[str] = set()
        for _slot, runtime in self._active():
            config = runtime.config
            factor = runtime.factor
            key = config.target.lower()
            if config.fault_type == "THRUST_LOSS":
                actual["propulsion"] *= max(0.0, 1.0 - config.level * factor)
            elif config.fault_type == "SURFACE_LOSS" and key not in jammed:
                actual[key] *= max(0.0, 1.0 - config.level * factor)
            elif config.fault_type == "SURFACE_JAM" and key not in jammed:
                max_angle = SURFACE_MAX_DEFLECTION_DEG[config.target]
                jam_value = max(-1.0, min(1.0, config.level / max_angle))
                # 渐入时从正常实际舵面连续过渡到卡死角度。
                actual[key] = normal[key] * (1.0 - factor) + jam_value * factor
                jammed.add(key)
        with self._status_lock:
            self._actuator_state = {"normal": dict(normal), "actual": dict(actual)}
        return actual

    def apply_sensor(self, target: str, raw_value: float, sim_time_s: float) -> tuple[float, bool]:
        """在传感器输出端应用噪声、漂移、偏差或冻结。"""
        target = target.upper()
        measured = float(raw_value)
        valid = True
        fail_active = False
        for slot, runtime in self._active():
            config = runtime.config
            if config.target != target or not config.fault_type.startswith("SENSOR_"):
                continue
            factor = runtime.factor
            if config.fault_type == "SENSOR_NOISE":
                measured += self._random[slot].gauss(0.0, config.level * factor)
            elif config.fault_type == "SENSOR_BIAS":
                measured += config.level * factor
            elif config.fault_type == "SENSOR_DRIFT":
                active_start = runtime.configured_sim_s + config.start_delay_s
                elapsed = max(0.0, sim_time_s - active_start)
                offset = config.level * elapsed
                if config.secondary > 0.0:
                    offset = max(-config.secondary, min(config.secondary, offset))
                measured += offset * factor
            elif config.fault_type == "SENSOR_FAIL":
                fail_active = True

        if fail_active:
            if target not in self._frozen_measurements:
                self._frozen_measurements[target] = measured
            measured = self._frozen_measurements[target]
            valid = False
        else:
            self._frozen_measurements.pop(target, None)

        with self._status_lock:
            self._sensor_state[target] = {
                "raw": round(raw_value, 6),
                "measured": round(measured, 6),
                "valid": valid,
            }
        return measured, valid

    def _clear_unused_freezes(self) -> None:
        active_failed_targets = {
            runtime.config.target
            for _slot, runtime in self._active()
            if runtime.config.fault_type == "SENSOR_FAIL"
        }
        for target in list(self._frozen_measurements):
            if target not in active_failed_targets:
                self._frozen_measurements.pop(target, None)

    def status(self) -> dict[str, Any]:
        with self._status_lock:
            sensors = {key: dict(value) for key, value in self._sensor_state.items()}
            actuators = {
                key: dict(value) for key, value in self._actuator_state.items()
            }
        channels = []
        for slot, runtime in sorted(self._channels.items()):
            channels.append(
                {
                    "slot": slot,
                    **asdict(runtime.config),
                    "phase": runtime.phase,
                    "factor": round(runtime.factor, 4),
                    "configured_sim_time_s": round(runtime.configured_sim_s, 6),
                }
            )
        return {
            "enabled": self.config.enabled,
            "max_channels": self.config.max_channels,
            "random_seed": self.config.random_seed,
            "sim_time_s": round(self._latest_sim_s, 6),
            "channels": channels,
            "actuators": actuators,
            "sensors": sensors,
        }

    @staticmethod
    def _protocol_fault_type(config: FaultChannelConfig) -> int:
        """把 JSBSim 模型故障映射到 Allspark V1.1 故障类型。"""
        if config.fault_type == "THRUST_LOSS":
            return 2 if config.profile == "ramp" else 1
        if config.fault_type == "SURFACE_JAM":
            return 3
        if config.fault_type == "SURFACE_LOSS":
            return 5 if config.profile == "ramp" else 4
        sensor_offset = 0 if config.target == "AIRSPEED" else 4
        return {
            "SENSOR_NOISE": 6 + sensor_offset,
            "SENSOR_DRIFT": 7 + sensor_offset,
            "SENSOR_BIAS": 8 + sensor_offset,
            "SENSOR_FAIL": 9 + sensor_offset,
        }.get(config.fault_type, 0)

    def transport_truth(self, frame_index: int) -> dict[str, Any]:
        """生成 JBS2 使用的紧凑模型真值快照。

        每帧只携带一个故障描述符；当存在复合故障时按帧轮转槽位，飞控端
        根据 active_fault_mask 缓存所有槽位。真实执行机构值来自已经施加故障
        后的 ``actual``，因此不会把飞控原始指令误当成故障真值。
        """
        with self._status_lock:
            actual = dict(self._actuator_state["actual"])

        live_channels = [
            (slot, runtime)
            for slot, runtime in sorted(self._channels.items())
            if 0 <= slot < 8
            and runtime.config.enabled
            and runtime.phase not in ("disabled", "complete")
        ]
        active_mask = 0
        for slot, _runtime in live_channels:
            active_mask |= 1 << slot

        selected = None
        if live_channels:
            selected = live_channels[int(frame_index) % len(live_channels)]

        def signed_centidegrees(key: str, target: str) -> int:
            value = actual.get(key, 0.0) * SURFACE_MAX_DEFLECTION_DEG[target] * 100.0
            return max(-32768, min(32767, int(round(value))))

        surfaces_valid = all(key in actual for key in ("aileron", "elevator", "rudder"))
        throttle_valid = "propulsion" in actual
        actual_surfaces_cd = (
            signed_centidegrees("aileron", "AILERON"),
            signed_centidegrees("elevator", "ELEVATOR"),
            signed_centidegrees("rudder", "RUDDER"),
            0,
        )
        throttle_permille = max(
            0,
            min(1000, int(round(actual.get("propulsion", 0.0) * 1000.0))),
        )

        truth = {
            "active_fault_count": len(live_channels),
            "active_fault_mask": active_mask,
            "fault_slot": 0xFF,
            "fault_type": 0,
            "fault_location": 0,
            "fault_state": 0,
            "fault_severity_permille": 0,
            "fault_parameter_1_q10000": 0,
            "fault_parameter_2_q10000": 0,
            "actual_surface_cd": actual_surfaces_cd,
            # 当前 c172x 模型只有一个油门；第二路保留为 0。
            "actual_throttle_permille": (throttle_permille, 0),
            # bit1=真实舵面，bit2=真实油门，bit3=故障描述有效。
            "valid_mask": (2 if surfaces_valid else 0) |
                          (4 if throttle_valid else 0) | 8,
        }
        if selected is not None:
            slot, runtime = selected
            config = runtime.config
            truth.update(
                {
                    "fault_slot": slot,
                    "fault_type": self._protocol_fault_type(config),
                    "fault_location": PROTOCOL_FAULT_LOCATION[config.target],
                    "fault_state": PROTOCOL_FAULT_STATE.get(runtime.phase, 0),
                    "fault_severity_permille": max(
                        0, min(1000, int(round(runtime.factor * 1000.0)))
                    ),
                    "fault_parameter_1_q10000": int(round(config.level * 10000.0)),
                    "fault_parameter_2_q10000": int(round(config.secondary * 10000.0)),
                }
            )
        return truth

    def _log_event(
        self,
        kind: str,
        sim_time_s: float,
        payload: dict[str, Any],
        status: str,
        error: str | None = None,
    ) -> None:
        if self._logger is not None:
            self._logger.log_event(
                {
                    "kind": kind,
                    "sim_time_s": round(sim_time_s, 6),
                    "payload": payload,
                    "status": status,
                    "error": error,
                }
            )

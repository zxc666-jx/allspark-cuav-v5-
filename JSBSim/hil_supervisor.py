#!/usr/bin/env python3
"""单架固定翼的 JSBSim 硬件在环监控程序。

程序以确定的固定步长推进飞机模型，与 ArduPlane 交换传感器和舵机帧，
并提供仅限本机访问的环境控制接口。所有 JSBSim 写操作都在固定频率物理线程完成。
"""

from __future__ import annotations

import argparse
import csv
import json
import math
import queue
import signal
import socket
import struct
import sys
import threading
import time
import uuid
from dataclasses import asdict, dataclass, replace
from datetime import datetime, timezone
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from typing import Any
from urllib.parse import urlsplit

import jsbsim
import yaml
from pymavlink import mavutil

from fault_injection import FaultController, FaultSystemConfig
from allspark_hil_bridge import AllsparkHILBridge, RELAY_SIZE, bridge_capabilities

try:
    import serial
except ImportError:  # 只有启用 USB 转 TTL 硬件在环链路时才必须安装此模块。
    serial = None


# JSBSim 的多数飞行状态量使用英制；MAVLink 传感器字段使用 SI 单位。
M_TO_FT = 3.280839895013123
MPS_TO_KTS = 1.9438444924406048
FPS_TO_MPS = 0.3048
FT_TO_M = 0.3048
# 1 psf = 47.88025898 Pa = 0.4788025898 hPa（mbar）。
PSF_TO_HPA = 0.4788025898
PSF_TO_PA = 47.88025898

# 真实气压计的压力值不会长期保持逐位完全相同。ArduPilot 检测到压力和温度
# 连续两秒都不变化时，会认为传感器冻结并把气压计标记为不健康。JSBSim 的
# 理想大气在稳定飞行时可能完全不变，因此加入极小且确定的压力波动（只相当于
# 数厘米高度变化），模拟传感器量化和噪声，同时保证各次运行能够复现。
BARO_PRESSURE_NOISE_PA = 0.35
BARO_PRESSURE_NOISE_HZ = 1.37
# 近地层标准大气压强尺度高度，用于把“高度测量偏差”换算为一致的静压偏差。
PRESSURE_SCALE_HEIGHT_M = 8434.5

# MAVLink HIL_SENSOR 的字段有效位：加速度、角速度、静压、差压、气压高度和温度。
# 磁力计尚未建模，故不置位 X/Y/Z 磁场的有效位；接收端应忽略其零值。
HIL_SENSOR_UPDATED_IMU_BARO_AIRSPEED = 0x1E3F
# GPS 时间以 1980-01-06 00:00:00 UTC 为起点，供 GPS_INPUT 的周数/周内毫秒换算。
GPS_EPOCH_UNIX_S = 315_964_800

# 与飞控端 AP_ExternalAHRS_JSBSim 共用的二进制串口协议。帧格式为：
# ``帧头标识 + uint16(载荷长度) + 载荷 + CRC16``。
JSBSIM_SENSOR_MAGIC = b"JBS2"
JSBSIM_OUTPUT_MAGIC = b"JBO1"
JSBSIM_FRAME_HEADER = struct.Struct("<4sH")
JSBSIM_FRAME_CRC = struct.Struct("<H")
JSBSIM_SENSOR_PAYLOAD = struct.Struct("<I" + "f" * 12 + "iii" + "f" * 6 + "BBHI" + "f" * 5)
# JBS2 在原 JBS1 传感器载荷后追加 36 字节模型真值。原 JBS1 接收仍由
# 飞控端保留，便于旧版上位机继续联调。
JSBSIM_TRUTH_PAYLOAD = struct.Struct("<BBHBBBBHiihh4h2HH")
JSBSIM_OUTPUT_PAYLOAD = struct.Struct("<I4H")


def simulated_baro_pressure_pa(fdm: Any, sim_time_s: float) -> float:
    """返回带有微小确定性传感器波动的静压值。"""
    ideal_pressure_pa = fdm["atmosphere/P-psf"] * PSF_TO_PA
    noise_pa = BARO_PRESSURE_NOISE_PA * math.sin(
        2.0 * math.pi * BARO_PRESSURE_NOISE_HZ * sim_time_s
    )
    return ideal_pressure_pa + noise_pa


def faulted_air_data(
    fdm: Any,
    sim_time_s: float,
    faults: FaultController,
) -> dict[str, float | bool]:
    """生成保留真值、只改变测量输出的空速和高度数据。"""
    true_airspeed_mps = fdm["velocities/vtrue-fps"] * FPS_TO_MPS
    measured_airspeed_mps, airspeed_valid = faults.apply_sensor(
        "AIRSPEED", true_airspeed_mps, sim_time_s
    )
    true_altitude_m = fdm["position/h-sl-ft"] * FT_TO_M
    measured_altitude_m, altitude_valid = faults.apply_sensor(
        "ALTITUDE", true_altitude_m, sim_time_s
    )

    raw_qbar_pa = max(0.0, fdm["aero/qbar-psf"] * PSF_TO_PA)
    if abs(true_airspeed_mps) > 1.0e-3:
        qbar_scale = (max(0.0, measured_airspeed_mps) / abs(true_airspeed_mps)) ** 2
        measured_qbar_pa = raw_qbar_pa * qbar_scale
    else:
        measured_qbar_pa = 0.0

    raw_pressure_pa = simulated_baro_pressure_pa(fdm, sim_time_s)
    altitude_delta_m = measured_altitude_m - true_altitude_m
    measured_pressure_pa = raw_pressure_pa * math.exp(
        -altitude_delta_m / PRESSURE_SCALE_HEIGHT_M
    )
    return {
        "true_airspeed_mps": true_airspeed_mps,
        "measured_airspeed_mps": max(0.0, measured_airspeed_mps),
        "airspeed_valid": airspeed_valid,
        "true_altitude_m": true_altitude_m,
        "measured_altitude_m": measured_altitude_m,
        "altitude_valid": altitude_valid,
        "dynamic_pressure_pa": max(0.0, measured_qbar_pa),
        "static_pressure_pa": max(100.0, measured_pressure_pa),
    }


@dataclass(frozen=True)
class SpawnConfig:
    latitude_deg: float
    longitude_deg: float
    altitude_msl_m: float
    terrain_elevation_m: float
    heading_deg: float
    airspeed_mps: float


@dataclass(frozen=True)
class SimulationConfig:
    rate_hz: float
    status_hz: float
    realtime: bool


@dataclass(frozen=True)
class MavlinkConfig:
    enabled: bool
    host: str
    port: int
    sensor_rate_hz: float
    gps_rate_hz: float
    system_id: int
    component_id: int


@dataclass(frozen=True)
class SerialHILConfig:
    """MicoAir743v2 硬件在环后端使用的专用 USB 转 TTL 链路。"""

    enabled: bool
    device: str
    baud: int
    sensor_rate_hz: float
    gps_rate_hz: float


@dataclass(frozen=True)
class UdpHILConfig:
    """通过串口转 UDP 网桥交换原始 JBS1/JBO1 帧。"""

    enabled: bool
    bind_host: str
    bind_port: int
    remote_host: str
    remote_port: int
    sensor_rate_hz: float
    gps_rate_hz: float


@dataclass(frozen=True)
class ControlAPIConfig:
    enabled: bool
    host: str
    port: int


@dataclass(frozen=True)
class SteadyWindConfig:
    enabled: bool
    speed_mps: float
    direction_from_deg: float
    vertical_mps: float


@dataclass(frozen=True)
class TurbulenceConfig:
    enabled: bool
    preset: str
    severity: int
    windspeed_at_20ft_mps: float
    random_seed: int


@dataclass(frozen=True)
class GustConfig:
    magnitude_mps: float
    direction_from_deg: float
    vertical_mps: float
    ramp_in_s: float
    hold_s: float
    ramp_out_s: float


@dataclass(frozen=True)
class EnvironmentConfig:
    steady_wind: SteadyWindConfig
    turbulence: TurbulenceConfig
    gust: GustConfig


@dataclass(frozen=True)
class VehicleConfig:
    sysid: int
    mesh_node: str
    model: str
    spawn: SpawnConfig
    simulation: SimulationConfig
    mavlink: MavlinkConfig
    serial_hil: SerialHILConfig
    udp_hil: UdpHILConfig
    control_api: ControlAPIConfig
    environment: EnvironmentConfig
    fault_injection: FaultSystemConfig


def _require(mapping: dict[str, Any], key: str, section: str) -> Any:
    """读取 YAML 的必填项，并在报错中保留完整配置路径。"""
    if key not in mapping:
        raise ValueError(f"missing required setting: {section}.{key}")
    return mapping[key]


TURBULENCE_PRESETS: dict[str, tuple[int, float]] = {
    "light": (3, 7.62),
    "moderate": (4, 15.24),
    "severe": (6, 22.86),
}


def _mapping(value: Any, section: str) -> dict[str, Any]:
    if value is None:
        return {}
    if not isinstance(value, dict):
        raise ValueError(f"{section} must be a YAML/JSON mapping")
    return value


def _steady_wind_config(raw: dict[str, Any]) -> SteadyWindConfig:
    config = SteadyWindConfig(
        enabled=bool(raw.get("enabled", False)),
        speed_mps=float(raw.get("speed_mps", 0.0)),
        direction_from_deg=float(raw.get("direction_from_deg", 0.0)) % 360.0,
        vertical_mps=float(raw.get("vertical_mps", 0.0)),
    )
    if not 0.0 <= config.speed_mps <= 100.0:
        raise ValueError("environment.steady_wind.speed_mps must be in the range 0..100")
    if not -50.0 <= config.vertical_mps <= 50.0:
        raise ValueError("environment.steady_wind.vertical_mps must be in the range -50..50")
    return config


def _turbulence_config(raw: dict[str, Any]) -> TurbulenceConfig:
    preset = str(raw.get("preset", "light")).lower()
    if preset not in ("light", "moderate", "severe", "custom"):
        raise ValueError("environment.turbulence.preset must be light, moderate, severe or custom")
    default_severity, default_speed = TURBULENCE_PRESETS.get(preset, (3, 7.62))
    config = TurbulenceConfig(
        enabled=bool(raw.get("enabled", False)),
        preset=preset,
        severity=int(raw.get("severity", default_severity)),
        windspeed_at_20ft_mps=float(raw.get("windspeed_at_20ft_mps", default_speed)),
        random_seed=int(raw.get("random_seed", 12345)),
    )
    if not 0 <= config.severity <= 7:
        raise ValueError("environment.turbulence.severity must be in the range 0..7")
    if not 0.0 <= config.windspeed_at_20ft_mps <= 100.0:
        raise ValueError("environment.turbulence.windspeed_at_20ft_mps must be in the range 0..100")
    if not 0 <= config.random_seed <= 2_147_483_647:
        raise ValueError("environment.turbulence.random_seed must be in the range 0..2147483647")
    return config


def _gust_config(raw: dict[str, Any]) -> GustConfig:
    config = GustConfig(
        magnitude_mps=float(raw.get("magnitude_mps", 0.0)),
        direction_from_deg=float(raw.get("direction_from_deg", 0.0)) % 360.0,
        vertical_mps=float(raw.get("vertical_mps", 0.0)),
        ramp_in_s=float(raw.get("ramp_in_s", 1.0)),
        hold_s=float(raw.get("hold_s", 1.0)),
        ramp_out_s=float(raw.get("ramp_out_s", 1.0)),
    )
    if not 0.0 <= config.magnitude_mps <= 100.0:
        raise ValueError("environment.gust.magnitude_mps must be in the range 0..100")
    if not -50.0 <= config.vertical_mps <= 50.0:
        raise ValueError("environment.gust.vertical_mps must be in the range -50..50")
    if any(value < 0.0 or value > 300.0 for value in (config.ramp_in_s, config.hold_s, config.ramp_out_s)):
        raise ValueError("environment.gust durations must be in the range 0..300 seconds")
    if config.ramp_in_s + config.hold_s + config.ramp_out_s <= 0.0:
        raise ValueError("environment.gust total duration must be positive")
    return config


def _environment_config(raw: dict[str, Any]) -> EnvironmentConfig:
    return EnvironmentConfig(
        steady_wind=_steady_wind_config(_mapping(raw.get("steady_wind"), "environment.steady_wind")),
        turbulence=_turbulence_config(_mapping(raw.get("turbulence"), "environment.turbulence")),
        gust=_gust_config(_mapping(raw.get("gust"), "environment.gust")),
    )


def _fault_system_config(raw: dict[str, Any]) -> FaultSystemConfig:
    config = FaultSystemConfig(
        enabled=bool(raw.get("enabled", True)),
        max_channels=int(raw.get("max_channels", 8)),
        random_seed=int(raw.get("random_seed", 24680)),
    )
    if not 1 <= config.max_channels <= 32:
        raise ValueError("fault_injection.max_channels must be in the range 1..32")
    if not 0 <= config.random_seed <= 2_147_483_647:
        raise ValueError("fault_injection.random_seed must be in the range 0..2147483647")
    return config

# 从 YAML 读取单机配置；此处仅完成类型转换，范围和频率关系在下方统一校验。
def load_config(path: Path) -> VehicleConfig:
    try:
        raw = yaml.safe_load(path.read_text(encoding="utf-8"))
    except OSError as exc:
        raise ValueError(f"cannot read config file {path}: {exc}") from exc
    except yaml.YAMLError as exc:
        raise ValueError(f"invalid YAML in {path}: {exc}") from exc

    if not isinstance(raw, dict):
        raise ValueError("config root must be a YAML mapping")

    vehicle = raw.get("vehicle", {})
    spawn = raw.get("spawn", {})
    simulation = raw.get("simulation", {})
    mavlink = raw.get("mavlink", {})
    serial_hil = raw.get("serial_hil", {})
    udp_hil = raw.get("udp_hil", {})
    control_api = raw.get("control_api", {})
    environment = raw.get("environment", {})
    fault_injection = raw.get("fault_injection", {})
    if not all(
        isinstance(item, dict)
        for item in (
            vehicle,
            spawn,
            simulation,
            mavlink,
            serial_hil,
            udp_hil,
            control_api,
            environment,
            fault_injection,
        )
    ):
        raise ValueError(
            "vehicle, spawn, simulation, mavlink, serial_hil, udp_hil, control_api, environment "
            "and fault_injection "
            "must be YAML mappings"
        )

    config = VehicleConfig(
        sysid=int(_require(vehicle, "sysid", "vehicle")),
        mesh_node=str(vehicle.get("mesh_node", "unassigned")),
        model=str(_require(vehicle, "model", "vehicle")),
        spawn=SpawnConfig(
            latitude_deg=float(_require(spawn, "latitude_deg", "spawn")),
            longitude_deg=float(_require(spawn, "longitude_deg", "spawn")),
            altitude_msl_m=float(_require(spawn, "altitude_msl_m", "spawn")),
            terrain_elevation_m=float(spawn.get("terrain_elevation_m", 0.0)),
            heading_deg=float(spawn.get("heading_deg", 0.0)),
            airspeed_mps=float(spawn.get("airspeed_mps", 0.0)),
        ),
        simulation=SimulationConfig(
            rate_hz=float(simulation.get("rate_hz", 400.0)),
            status_hz=float(simulation.get("status_hz", 5.0)),
            realtime=bool(simulation.get("realtime", True)),
        ),
        # MAVLink 发送端配置。host/port 指向接收端的 UDP 监听地址。
        # TODO(待对齐)：默认 IMU 为 200 Hz，而设计文档 §7 写的是 100 Hz；
        # 应在协议冻结后统一默认值，并在 YAML 中显式配置。
        mavlink=MavlinkConfig(
            enabled=bool(mavlink.get("enabled", False)),
            host=str(mavlink.get("host", "127.0.0.1")),
            port=int(mavlink.get("port", 14560)),
            sensor_rate_hz=float(mavlink.get("sensor_rate_hz", 200.0)),
            gps_rate_hz=float(mavlink.get("gps_rate_hz", 10.0)),
            system_id=int(mavlink.get("system_id", vehicle.get("sysid", 1))),
            component_id=int(mavlink.get("component_id", 200)),
        ),
        serial_hil=SerialHILConfig(
            enabled=bool(serial_hil.get("enabled", False)),
            device=str(serial_hil.get("device", "")),
            baud=int(serial_hil.get("baud", 921600)),
            sensor_rate_hz=float(serial_hil.get("sensor_rate_hz", 100.0)),
            gps_rate_hz=float(serial_hil.get("gps_rate_hz", 10.0)),
        ),
        udp_hil=UdpHILConfig(
            enabled=bool(udp_hil.get("enabled", False)),
            bind_host=str(udp_hil.get("bind_host", "0.0.0.0")),
            bind_port=int(udp_hil.get("bind_port", 14601)),
            remote_host=str(udp_hil.get("remote_host", "192.168.1.201")),
            remote_port=int(udp_hil.get("remote_port", 14601)),
            sensor_rate_hz=float(udp_hil.get("sensor_rate_hz", 100.0)),
            gps_rate_hz=float(udp_hil.get("gps_rate_hz", 10.0)),
        ),
        control_api=ControlAPIConfig(
            enabled=bool(control_api.get("enabled", False)),
            host=str(control_api.get("host", "127.0.0.1")),
            port=int(control_api.get("port", 8765)),
        ),
        environment=_environment_config(environment),
        fault_injection=_fault_system_config(fault_injection),
    )

    _validate_config(config)
    return config


def _validate_config(config: VehicleConfig) -> None:
    """在启动 JSBSim 前发现配置错误，避免运行中出现难以定位的链路问题。"""
    if not 1 <= config.sysid <= 255:
        raise ValueError("vehicle.sysid must be in the range 1..255")
    if not -90.0 <= config.spawn.latitude_deg <= 90.0:
        raise ValueError("spawn.latitude_deg must be in the range -90..90")
    if not -180.0 <= config.spawn.longitude_deg <= 180.0:
        raise ValueError("spawn.longitude_deg must be in the range -180..180")
    if config.simulation.rate_hz <= 0 or config.simulation.status_hz <= 0:
        raise ValueError("simulation rates must be positive")
    if config.simulation.status_hz > config.simulation.rate_hz:
        raise ValueError("simulation.status_hz cannot exceed simulation.rate_hz")
    if not 1 <= config.mavlink.port <= 65535:
        raise ValueError("mavlink.port must be in the range 1..65535")
    if not 1 <= config.mavlink.system_id <= 255:
        raise ValueError("mavlink.system_id must be in the range 1..255")
    if not 1 <= config.mavlink.component_id <= 255:
        raise ValueError("mavlink.component_id must be in the range 1..255")
    if config.mavlink.sensor_rate_hz <= 0 or config.mavlink.gps_rate_hz <= 0:
        raise ValueError("mavlink sensor and GPS rates must be positive")
    if config.mavlink.sensor_rate_hz > config.simulation.rate_hz:
        raise ValueError("mavlink.sensor_rate_hz cannot exceed simulation.rate_hz")
    if config.mavlink.gps_rate_hz > config.simulation.rate_hz:
        raise ValueError("mavlink.gps_rate_hz cannot exceed simulation.rate_hz")
    enabled_transports = sum(
        (config.mavlink.enabled, config.serial_hil.enabled, config.udp_hil.enabled)
    )
    if enabled_transports != 1:
        raise ValueError(
            "exactly one HIL transport must be enabled: mavlink, serial_hil or udp_hil"
        )
    if config.serial_hil.enabled and not config.serial_hil.device:
        raise ValueError("serial_hil.device is required when serial_hil.enabled is true")
    if config.serial_hil.baud <= 0:
        raise ValueError("serial_hil.baud must be positive")
    if config.serial_hil.sensor_rate_hz <= 0 or config.serial_hil.gps_rate_hz <= 0:
        raise ValueError("serial_hil sensor and GPS rates must be positive")
    if config.serial_hil.sensor_rate_hz > config.simulation.rate_hz:
        raise ValueError("serial_hil.sensor_rate_hz cannot exceed simulation.rate_hz")
    # AP_GPS 时间戳按照 5～20 Hz 接收机设计。频率更高的 GNSS 样本会被
    # 取整到 50 ms 周期，进而可能被报告为数据延迟。
    if not 5.0 <= config.serial_hil.gps_rate_hz <= 20.0:
        raise ValueError("serial_hil.gps_rate_hz must be in the range 5..20")
    if config.serial_hil.gps_rate_hz > config.serial_hil.sensor_rate_hz:
        raise ValueError("serial_hil.gps_rate_hz cannot exceed serial_hil.sensor_rate_hz")
    if not config.udp_hil.bind_host:
        raise ValueError("udp_hil.bind_host cannot be empty")
    if not config.udp_hil.remote_host:
        raise ValueError("udp_hil.remote_host cannot be empty")
    if not 1 <= config.udp_hil.bind_port <= 65535:
        raise ValueError("udp_hil.bind_port must be in the range 1..65535")
    if not 1 <= config.udp_hil.remote_port <= 65535:
        raise ValueError("udp_hil.remote_port must be in the range 1..65535")
    if config.udp_hil.sensor_rate_hz <= 0 or config.udp_hil.gps_rate_hz <= 0:
        raise ValueError("udp_hil sensor and GPS rates must be positive")
    if config.udp_hil.sensor_rate_hz > config.simulation.rate_hz:
        raise ValueError("udp_hil.sensor_rate_hz cannot exceed simulation.rate_hz")
    if not 5.0 <= config.udp_hil.gps_rate_hz <= 20.0:
        raise ValueError("udp_hil.gps_rate_hz must be in the range 5..20")
    if config.udp_hil.gps_rate_hz > config.udp_hil.sensor_rate_hz:
        raise ValueError("udp_hil.gps_rate_hz cannot exceed udp_hil.sensor_rate_hz")
    if config.control_api.host not in ("127.0.0.1", "localhost"):
        raise ValueError("control_api.host must be 127.0.0.1 or localhost")
    if not 1 <= config.control_api.port <= 65535:
        raise ValueError("control_api.port must be in the range 1..65535")


def apply_transport_overrides(config: VehicleConfig, args: argparse.Namespace) -> VehicleConfig:
    """应用 Mission Planner 启动界面传入的链路选择，不改写原始 YAML。"""
    transport = getattr(args, "transport", "config")
    if transport == "config":
        return config
    if transport == "serial-jbs":
        serial_config = replace(
            config.serial_hil,
            enabled=True,
            device=args.serial_device or config.serial_hil.device,
            baud=args.serial_baud or config.serial_hil.baud,
        )
        config = replace(
            config,
            mavlink=replace(config.mavlink, enabled=False),
            serial_hil=serial_config,
            udp_hil=replace(config.udp_hil, enabled=False),
        )
    elif transport == "udp-jbs":
        udp_config = replace(
            config.udp_hil,
            enabled=True,
            bind_host=args.udp_bind_host or config.udp_hil.bind_host,
            bind_port=args.udp_bind_port or config.udp_hil.bind_port,
            remote_host=args.udp_remote_host or config.udp_hil.remote_host,
            remote_port=args.udp_remote_port or config.udp_hil.remote_port,
        )
        config = replace(
            config,
            mavlink=replace(config.mavlink, enabled=False),
            serial_hil=replace(config.serial_hil, enabled=False),
            udp_hil=udp_config,
        )
    else:
        raise ValueError(f"unsupported HIL transport override: {transport}")
    _validate_config(config)
    return config

class JSBSimWorker:
    """管理一架飞机的 JSBSim 实例，并对外提供同一时刻的真值状态。"""

    def __init__(self, config: VehicleConfig) -> None:
        self.config = config
        self.fdm = jsbsim.FGFDMExec(None)
        self.fdm.set_debug_level(0)
        # 固定物理步长，例如 400 Hz 对应 dt=0.0025 s；采样率不能超过该频率。
        self.fdm.set_dt(1.0 / config.simulation.rate_hz)

    def initialize(self) -> None:
        if not self.fdm.load_model(self.config.model):
            raise RuntimeError(f"JSBSim could not load aircraft model {self.config.model!r}")

        # 一些 JSBSim 示例机型自带 CSV output directive。单机运行会持续覆盖
        # 工程目录中的 JSBout*.csv，多机并发时还会让多个进程争用同一文件。
        # 本项目已有按 sysid 隔离的结构化日志，因此关闭机型自带输出。
        self.fdm.disable_output()

        spawn = self.config.spawn
        # ``ic/*`` 是 JSBSim 初始条件属性。高度使用 ft，校准空速使用 kt。
        # 姿态和航向按 JSBSim 的真北、机体系约定设置。
        self.fdm["ic/lat-gc-deg"] = spawn.latitude_deg
        self.fdm["ic/long-gc-deg"] = spawn.longitude_deg
        self.fdm["ic/h-sl-ft"] = spawn.altitude_msl_m * M_TO_FT
        self.fdm["ic/terrain-elevation-ft"] = spawn.terrain_elevation_m * M_TO_FT
        self.fdm["ic/psi-true-deg"] = spawn.heading_deg
        self.fdm["ic/vc-kts"] = spawn.airspeed_mps * MPS_TO_KTS
        self.fdm["ic/gamma-deg"] = 0.0
        self.fdm["ic/phi-deg"] = 0.0
        self.fdm["ic/theta-deg"] = 0.0

        if not self.fdm.run_ic():
            raise RuntimeError("JSBSim rejected the configured initial conditions")

        # 以舵面中立、油门怠速开始。串口硬件在环接收器每收到一个有效的
        # JBO1 帧，都会用飞控的 SERVO1～4 输出覆盖这些初始指令。
        self.fdm["fcs/aileron-cmd-norm"] = 0.0
        self.fdm["fcs/elevator-cmd-norm"] = 0.0
        self.fdm["fcs/rudder-cmd-norm"] = 0.0
        self.fdm["fcs/throttle-cmd-norm"] = 0.0

        # 加载 c172x 后活塞发动机不会自动启动，仅给油门不会产生转速或推力。
        # 硬件在环启动时让发动机运行、混合比置浓并释放机轮刹车，使 ArduPlane
        # 的油门输出能够直接控制推力。
        self.fdm["fcs/mixture-cmd-norm"] = 1.0
        self.fdm["fcs/left-brake-cmd-norm"] = 0.0
        self.fdm["fcs/right-brake-cmd-norm"] = 0.0
        self.fdm["fcs/center-brake-cmd-norm"] = 0.0
        self.fdm["propulsion/set-running"] = -1.0

    def step(self) -> bool:
        return bool(self.fdm.run())

    def apply_pwm_outputs(
        self,
        pwm: tuple[int, int, int, int],
        faults: FaultController | None = None,
    ) -> None:
        """在下一物理步之前应用固定翼 SERVO1～4 的 PWM 输出。

        当前映射为：1=副翼、2=升降舵、3=油门、4=方向舵。机架必须使用相同的
        SERVOx_FUNCTION 分配；不同机型的反向设置以后应放入可配置映射层。
        """

        def surface_command(value: int) -> float:
            return max(-1.0, min(1.0, (value - 1500) / 500.0))

        # 飞控原始 PWM 先转换为正常执行机构输出；故障控制器只改变送入
        # JSBSim 的实际执行量，不回写也不修改飞控控制指令。
        normal = {
            "aileron": surface_command(pwm[0]),
            "elevator": -surface_command(pwm[1]),
            "propulsion": max(0.0, min(1.0, (pwm[2] - 1000) / 1000.0)),
            "rudder": -surface_command(pwm[3]),
        }
        actual = faults.apply_actuators(normal) if faults is not None else normal

        # ArduPlane 的正方向分别代表向右滚转、抬头和向右偏航。c172x 飞控系统
        # 的副翼符号相同，但升降舵和方向舵符号相反；符号已在 normal 层转换。
        self.fdm["fcs/aileron-cmd-norm"] = actual["aileron"]
        self.fdm["fcs/elevator-cmd-norm"] = actual["elevator"]
        self.fdm["fcs/throttle-cmd-norm"] = actual["propulsion"]
        self.fdm["fcs/rudder-cmd-norm"] = actual["rudder"]

    def truth_state(self, realtime_factor: float) -> dict[str, Any]:
        """生成低频监视 JSON；它是 JSBSim 真值，不能作为飞控估计状态使用。"""
        roll = math.degrees(self.fdm["attitude/phi-rad"])
        pitch = math.degrees(self.fdm["attitude/theta-rad"])
        yaw = math.degrees(self.fdm["attitude/psi-rad"]) % 360.0
        return {
            "sysid": self.config.sysid,
            "mesh_node": self.config.mesh_node,
            "model": self.config.model,
            "sim_time_s": round(self.fdm.get_sim_time(), 6),
            "realtime_factor": round(realtime_factor, 3),
            "latitude_deg": round(self.fdm["position/lat-gc-deg"], 8),
            "longitude_deg": round(self.fdm["position/long-gc-deg"], 8),
            "altitude_msl_m": round(self.fdm["position/h-sl-ft"] * FT_TO_M, 3),
            "altitude_agl_m": round(self.fdm["position/h-agl-ft"] * FT_TO_M, 3),
            "baro_pressure_pa": round(
                simulated_baro_pressure_pa(self.fdm, self.fdm.get_sim_time()), 3
            ),
            "groundspeed_mps": round(self.fdm["velocities/vg-fps"] * FPS_TO_MPS, 3),
            "true_airspeed_mps": round(self.fdm["velocities/vtrue-fps"] * FPS_TO_MPS, 3),
            "engine_running": self.fdm["propulsion/engine/set-running"] > 0.5,
            "engine_rpm": round(self.fdm["propulsion/engine/engine-rpm"], 1),
            "engine_thrust_lbf": round(self.fdm["propulsion/engine/thrust-lbs"], 2),
            "throttle_cmd_norm": round(self.fdm["fcs/throttle-cmd-norm"], 3),
            "roll_deg": round(roll, 3),
            "pitch_deg": round(pitch, 3),
            "yaw_deg": round(yaw, 3),
            "p_rad_s": round(self.fdm["velocities/p-rad_sec"], 5),
            "q_rad_s": round(self.fdm["velocities/q-rad_sec"], 5),
            "r_rad_s": round(self.fdm["velocities/r-rad_sec"], 5),
        }


def wind_from_to_ned_mps(
    horizontal_speed_mps: float,
    direction_from_deg: float,
    vertical_up_mps: float,
) -> tuple[float, float, float]:
    """把便于操作的气象风向转换为 JSBSim 的 NED 速度。

    ``direction_from_deg`` 从真北开始顺时针增加，表示风的来向；JSBSim 使用
    空气运动的去向，并规定垂直轴向下为正，因此必须显式转换这两种符号约定。
    """
    radians = math.radians(direction_from_deg % 360.0)
    return (
        -horizontal_speed_mps * math.cos(radians),
        -horizontal_speed_mps * math.sin(radians),
        -vertical_up_mps,
    )


class RuntimeStatusStore:
    """供 HTTP 线程读取的线程安全状态副本。"""

    def __init__(self) -> None:
        self._lock = threading.Lock()
        self._running = True
        self._status: dict[str, Any] = {}

    def update(self, status: dict[str, Any]) -> None:
        with self._lock:
            self._status = dict(status)

    def set_stopped(self) -> None:
        with self._lock:
            self._running = False

    def snapshot(self) -> dict[str, Any]:
        with self._lock:
            return {"running": self._running, "status": dict(self._status)}


class EnvironmentEventLogger:
    """记录可复现的环境命令和低频状态样本。"""

    STATUS_FIELDS = (
        "wall_time_utc",
        "sim_time_s",
        "latitude_deg",
        "longitude_deg",
        "altitude_msl_m",
        "groundspeed_mps",
        "true_airspeed_mps",
        "wind_north_mps",
        "wind_east_mps",
        "wind_down_mps",
        "gust_phase",
        "turbulence_enabled",
        "servo_rx_hz",
        "servo_link_ok",
    )

    def __init__(self, base_dir: Path, sysid: int) -> None:
        timestamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%S_%fZ")
        self.run_dir = base_dir / f"{timestamp}_sysid{sysid}"
        self.run_dir.mkdir(parents=True, exist_ok=False)
        self._events = (self.run_dir / "environment_events.jsonl").open(
            "a", encoding="utf-8", buffering=1
        )
        self._status_file = (self.run_dir / "environment_status.csv").open(
            "w", encoding="utf-8", newline=""
        )
        self._status_writer = csv.DictWriter(self._status_file, fieldnames=self.STATUS_FIELDS)
        self._status_writer.writeheader()
        self._status_file.flush()
        self._failed = False

    @staticmethod
    def _utc_now() -> str:
        return datetime.now(timezone.utc).isoformat(timespec="milliseconds")

    def log_event(self, event: dict[str, Any]) -> None:
        if self._failed:
            return
        record = {"wall_time_utc": self._utc_now(), **event}
        try:
            self._events.write(json.dumps(record, ensure_ascii=False) + "\n")
        except OSError as exc:
            self._disable_after_error(exc)

    def log_status(self, status: dict[str, Any], environment: dict[str, Any]) -> None:
        if self._failed:
            return
        actual = environment["actual_total_wind_mps"]
        settings = environment["settings"]
        try:
            self._status_writer.writerow(
                {
                    "wall_time_utc": self._utc_now(),
                    "sim_time_s": status.get("sim_time_s"),
                    "latitude_deg": status.get("latitude_deg"),
                    "longitude_deg": status.get("longitude_deg"),
                    "altitude_msl_m": status.get("altitude_msl_m"),
                    "groundspeed_mps": status.get("groundspeed_mps"),
                    "true_airspeed_mps": status.get("true_airspeed_mps"),
                    "wind_north_mps": actual["north"],
                    "wind_east_mps": actual["east"],
                    "wind_down_mps": actual["down"],
                    "gust_phase": environment["gust"]["phase"],
                    "turbulence_enabled": settings["turbulence"]["enabled"],
                    "servo_rx_hz": status.get("servo_rx_hz"),
                    "servo_link_ok": status.get("servo_link_ok"),
                }
            )
            self._status_file.flush()
        except OSError as exc:
            self._disable_after_error(exc)

    def _disable_after_error(self, exc: OSError) -> None:
        self._failed = True
        print(f"environment logging disabled after write error: {exc}", file=sys.stderr, flush=True)

    def close(self) -> None:
        try:
            self._events.close()
        finally:
            self._status_file.close()


class FaultEventLogger:
    """记录故障命令、通道阶段、正常执行量和故障后测量量。"""

    STATUS_FIELDS = (
        "wall_time_utc",
        "sim_time_s",
        "channels_json",
        "normal_actuators_json",
        "actual_actuators_json",
        "airspeed_raw_mps",
        "airspeed_measured_mps",
        "airspeed_valid",
        "altitude_raw_m",
        "altitude_measured_m",
        "altitude_valid",
    )

    def __init__(self, run_dir: Path) -> None:
        self._events = (run_dir / "fault_events.jsonl").open(
            "a", encoding="utf-8", buffering=1
        )
        self._status_file = (run_dir / "fault_status.csv").open(
            "w", encoding="utf-8", newline=""
        )
        self._status_writer = csv.DictWriter(self._status_file, fieldnames=self.STATUS_FIELDS)
        self._status_writer.writeheader()
        self._status_file.flush()
        self._failed = False

    @staticmethod
    def _utc_now() -> str:
        return datetime.now(timezone.utc).isoformat(timespec="milliseconds")

    def log_event(self, event: dict[str, Any]) -> None:
        if self._failed:
            return
        try:
            self._events.write(
                json.dumps({"wall_time_utc": self._utc_now(), **event}, ensure_ascii=False) + "\n"
            )
        except OSError as exc:
            self._disable_after_error(exc)

    def log_status(self, sim_time_s: float, faults: dict[str, Any]) -> None:
        if self._failed:
            return
        sensors = faults.get("sensors", {})
        airspeed = sensors.get("AIRSPEED", {})
        altitude = sensors.get("ALTITUDE", {})
        actuators = faults.get("actuators", {})
        try:
            self._status_writer.writerow(
                {
                    "wall_time_utc": self._utc_now(),
                    "sim_time_s": round(sim_time_s, 6),
                    "channels_json": json.dumps(faults.get("channels", []), ensure_ascii=False),
                    "normal_actuators_json": json.dumps(actuators.get("normal", {})),
                    "actual_actuators_json": json.dumps(actuators.get("actual", {})),
                    "airspeed_raw_mps": airspeed.get("raw"),
                    "airspeed_measured_mps": airspeed.get("measured"),
                    "airspeed_valid": airspeed.get("valid"),
                    "altitude_raw_m": altitude.get("raw"),
                    "altitude_measured_m": altitude.get("measured"),
                    "altitude_valid": altitude.get("valid"),
                }
            )
            self._status_file.flush()
        except OSError as exc:
            self._disable_after_error(exc)

    def _disable_after_error(self, exc: OSError) -> None:
        self._failed = True
        print(f"fault logging disabled after write error: {exc}", file=sys.stderr, flush=True)

    def close(self) -> None:
        try:
            self._events.close()
        finally:
            self._status_file.close()


@dataclass
class EnvironmentCommand:
    command_id: str
    kind: str
    payload: dict[str, Any]
    completed: threading.Event
    result: dict[str, Any] | None = None


class EnvironmentController:
    """确保所有 JSBSim 大气写操作都由固定频率物理线程执行。"""

    def __init__(
        self,
        fdm: Any,
        config: EnvironmentConfig,
        event_logger: EnvironmentEventLogger,
    ) -> None:
        self._fdm = fdm
        self._config = config
        self._logger = event_logger
        self._commands: queue.Queue[EnvironmentCommand] = queue.Queue(maxsize=128)
        self._status_lock = threading.Lock()
        self._gust_start_sim_s: float | None = None
        self._gust_phase = "idle"
        self._gust_factor = 0.0
        self._last_applied_sim_s = 0.0
        self._latest_sim_s = 0.0
        self._actual_total_wind_mps = {"north": 0.0, "east": 0.0, "down": 0.0}

    def initialize(self) -> None:
        self._apply_steady_wind(self._config.steady_wind)
        self._apply_turbulence(self._config.turbulence)
        self._write_gust((0.0, 0.0, 0.0))
        self.capture_actual_wind()

    @staticmethod
    def _merge_section(current: Any, update: Any, section: str) -> dict[str, Any]:
        result = asdict(current)
        result.update(_mapping(update, section))
        return result

    def _config_from_update(self, payload: dict[str, Any]) -> EnvironmentConfig:
        if not isinstance(payload, dict):
            raise ValueError("request body must be a JSON object")
        steady_raw = self._merge_section(
            self._config.steady_wind,
            payload.get("steady_wind", {}),
            "steady_wind",
        )
        turbulence_update = _mapping(payload.get("turbulence"), "turbulence")
        turbulence_raw = self._merge_section(
            self._config.turbulence,
            turbulence_update,
            "turbulence",
        )
        preset = str(turbulence_update.get("preset", turbulence_raw["preset"])).lower()
        if preset in TURBULENCE_PRESETS:
            preset_severity, preset_speed = TURBULENCE_PRESETS[preset]
            if "severity" not in turbulence_update:
                turbulence_raw["severity"] = preset_severity
            if "windspeed_at_20ft_mps" not in turbulence_update:
                turbulence_raw["windspeed_at_20ft_mps"] = preset_speed
        gust_raw = self._merge_section(self._config.gust, payload.get("gust", {}), "gust")
        return EnvironmentConfig(
            steady_wind=_steady_wind_config(steady_raw),
            turbulence=_turbulence_config(turbulence_raw),
            gust=_gust_config(gust_raw),
        )

    def submit(self, kind: str, payload: dict[str, Any], timeout_s: float = 1.0) -> dict[str, Any]:
        command = EnvironmentCommand(str(uuid.uuid4()), kind, payload, threading.Event())
        try:
            self._commands.put_nowait(command)
        except queue.Full as exc:
            raise RuntimeError("environment command queue is full") from exc
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
        while True:
            try:
                command = self._commands.get_nowait()
            except queue.Empty:
                return
            before = asdict(self._config)
            try:
                if command.kind == "environment.apply":
                    self._config = self._config_from_update(command.payload)
                    self._apply_steady_wind(self._config.steady_wind)
                    self._apply_turbulence(self._config.turbulence)
                elif command.kind == "environment.gust":
                    gust_payload = command.payload.get("gust", command.payload)
                    gust_raw = self._merge_section(self._config.gust, gust_payload, "gust")
                    gust = _gust_config(gust_raw)
                    self._config = EnvironmentConfig(
                        self._config.steady_wind,
                        self._config.turbulence,
                        gust,
                    )
                    self._gust_start_sim_s = sim_time_s
                    self._gust_phase = "ramp_in"
                    self._gust_factor = 0.0
                elif command.kind == "environment.reset":
                    self._config = EnvironmentConfig(
                        SteadyWindConfig(False, 0.0, 0.0, 0.0),
                        TurbulenceConfig(
                            False,
                            self._config.turbulence.preset,
                            self._config.turbulence.severity,
                            self._config.turbulence.windspeed_at_20ft_mps,
                            self._config.turbulence.random_seed,
                        ),
                        self._config.gust,
                    )
                    self._gust_start_sim_s = None
                    self._gust_phase = "idle"
                    self._gust_factor = 0.0
                    self._apply_steady_wind(self._config.steady_wind)
                    self._apply_turbulence(self._config.turbulence)
                    self._write_gust((0.0, 0.0, 0.0))
                else:
                    raise ValueError(f"unsupported command kind: {command.kind}")
                self._last_applied_sim_s = sim_time_s
                command.result = {
                    "command_id": command.command_id,
                    "status": "applied",
                    "applied_sim_time_s": round(sim_time_s, 6),
                    "error": None,
                }
                self._logger.log_event(
                    {
                        "command_id": command.command_id,
                        "kind": command.kind,
                        "sim_time_s": round(sim_time_s, 6),
                        "before": before,
                        "after": asdict(self._config),
                        "status": "applied",
                    }
                )
            except (TypeError, ValueError, KeyError) as exc:
                command.result = {
                    "command_id": command.command_id,
                    "status": "rejected",
                    "applied_sim_time_s": None,
                    "error": str(exc),
                }
                self._logger.log_event(
                    {
                        "command_id": command.command_id,
                        "kind": command.kind,
                        "sim_time_s": round(sim_time_s, 6),
                        "before": before,
                        "status": "rejected",
                        "error": str(exc),
                    }
                )
            finally:
                command.completed.set()
                self._commands.task_done()

    def _apply_steady_wind(self, config: SteadyWindConfig) -> None:
        vector = (
            wind_from_to_ned_mps(config.speed_mps, config.direction_from_deg, config.vertical_mps)
            if config.enabled
            else (0.0, 0.0, 0.0)
        )
        self._fdm["atmosphere/wind-north-fps"] = vector[0] * M_TO_FT
        self._fdm["atmosphere/wind-east-fps"] = vector[1] * M_TO_FT
        self._fdm["atmosphere/wind-down-fps"] = vector[2] * M_TO_FT

    def _apply_turbulence(self, config: TurbulenceConfig) -> None:
        # 先关闭湍流，使随机种子变化后 JSBSim 从确定状态重新开始，避免新旧
        # 随机样本混合。
        self._fdm["atmosphere/turb-type"] = 0.0
        self._fdm["atmosphere/turb-north-fps"] = 0.0
        self._fdm["atmosphere/turb-east-fps"] = 0.0
        self._fdm["atmosphere/turb-down-fps"] = 0.0
        self._fdm["atmosphere/randomseed"] = float(config.random_seed)
        self._fdm["atmosphere/turbulence/milspec/windspeed_at_20ft_AGL-fps"] = (
            config.windspeed_at_20ft_mps * M_TO_FT
        )
        self._fdm["atmosphere/turbulence/milspec/severity"] = float(config.severity)
        if config.enabled:
            self._fdm["atmosphere/turb-type"] = 3.0

    def _write_gust(self, vector_mps: tuple[float, float, float]) -> None:
        self._fdm["atmosphere/gust-north-fps"] = vector_mps[0] * M_TO_FT
        self._fdm["atmosphere/gust-east-fps"] = vector_mps[1] * M_TO_FT
        self._fdm["atmosphere/gust-down-fps"] = vector_mps[2] * M_TO_FT

    @staticmethod
    def gust_profile(config: GustConfig, elapsed_s: float) -> tuple[float, str]:
        if elapsed_s < 0.0:
            return 0.0, "pending"
        if config.ramp_in_s > 0.0 and elapsed_s < config.ramp_in_s:
            factor = 0.5 * (1.0 - math.cos(math.pi * elapsed_s / config.ramp_in_s))
            return factor, "ramp_in"
        elapsed_s -= config.ramp_in_s
        if elapsed_s < config.hold_s:
            return 1.0, "hold"
        elapsed_s -= config.hold_s
        if config.ramp_out_s > 0.0 and elapsed_s < config.ramp_out_s:
            factor = 0.5 * (1.0 + math.cos(math.pi * elapsed_s / config.ramp_out_s))
            return factor, "ramp_out"
        return 0.0, "idle"

    def update(self, sim_time_s: float) -> None:
        if self._gust_start_sim_s is None:
            return
        factor, phase = self.gust_profile(self._config.gust, sim_time_s - self._gust_start_sim_s)
        self._gust_factor = factor
        self._gust_phase = phase
        base = wind_from_to_ned_mps(
            self._config.gust.magnitude_mps,
            self._config.gust.direction_from_deg,
            self._config.gust.vertical_mps,
        )
        self._write_gust(tuple(component * factor for component in base))
        if phase == "idle":
            self._gust_start_sim_s = None

    def capture_actual_wind(self) -> None:
        actual = {
            "north": round(self._fdm["atmosphere/total-wind-north-fps"] * FPS_TO_MPS, 4),
            "east": round(self._fdm["atmosphere/total-wind-east-fps"] * FPS_TO_MPS, 4),
            "down": round(self._fdm["atmosphere/total-wind-down-fps"] * FPS_TO_MPS, 4),
        }
        with self._status_lock:
            self._latest_sim_s = self._fdm.get_sim_time()
            self._actual_total_wind_mps = actual

    def status(self) -> dict[str, Any]:
        with self._status_lock:
            actual = dict(self._actual_total_wind_mps)
        elapsed = None
        if self._gust_start_sim_s is not None:
            elapsed = max(0.0, self._latest_sim_s - self._gust_start_sim_s)
        return {
            "settings": asdict(self._config),
            "actual_total_wind_mps": actual,
            "gust": {
                "phase": self._gust_phase,
                "factor": round(self._gust_factor, 4),
                "elapsed_s": None if elapsed is None else round(elapsed, 3),
            },
            "last_applied_sim_time_s": round(self._last_applied_sim_s, 6),
        }


class EnvironmentRequestHandler(BaseHTTPRequestHandler):
    """仅允许本机访问的轻量 JSON 接口；JSBSim 操作仍由运行循环完成。"""

    server_version = "JSBSimHIL/1.0"

    @property
    def context(self) -> dict[str, Any]:
        return self.server.context  # type: ignore[attr-defined]

    def log_message(self, format_string: str, *args: Any) -> None:
        print(f"control API: {format_string % args}", file=sys.stderr)

    def _write_json(self, status: int, payload: dict[str, Any]) -> None:
        encoded = json.dumps(payload, ensure_ascii=False).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(encoded)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(encoded)

    def _read_json(self) -> dict[str, Any]:
        length = int(self.headers.get("Content-Length", "0"))
        if length > 65_536:
            raise ValueError("request body exceeds 65536 bytes")
        if length == 0:
            return {}
        try:
            value = json.loads(self.rfile.read(length).decode("utf-8"))
        except (UnicodeDecodeError, json.JSONDecodeError) as exc:
            raise ValueError(f"invalid JSON body: {exc}") from exc
        if not isinstance(value, dict):
            raise ValueError("request body must be a JSON object")
        return value

    def _vehicle_route(self) -> tuple[int, str] | None:
        parts = [part for part in urlsplit(self.path).path.split("/") if part]
        if len(parts) < 5 or parts[:3] != ["api", "v1", "vehicles"]:
            return None
        try:
            sysid = int(parts[3])
        except ValueError:
            return None
        suffix = "/".join(parts[4:])
        return sysid, suffix

    def do_GET(self) -> None:
        path = urlsplit(self.path).path
        if path == "/api/v1/health":
            health = self.context["runtime_status"].snapshot()
            health.update(
                {
                    "api": "ready",
                    "allspark_bridge": bridge_capabilities(),
                    "sysid": self.context["sysid"],
                    "environment": self.context["environment"].status(),
                    "faults": self.context["faults"].status(),
                }
            )
            self._write_json(200, health)
            return
        route = self._vehicle_route()
        if route == (self.context["sysid"], "environment"):
            self._write_json(200, self.context["environment"].status())
            return
        if route == (self.context["sysid"], "faults"):
            self._write_json(200, self.context["faults"].status())
            return
        self._write_json(404, {"error": "endpoint not found"})

    def do_PUT(self) -> None:
        route = self._vehicle_route()
        if route == (self.context["sysid"], "environment"):
            self._submit("environment", "environment.apply")
            return
        if route is not None and route[0] == self.context["sysid"]:
            parts = route[1].split("/")
            if len(parts) == 2 and parts[0] == "faults":
                try:
                    slot = int(parts[1])
                except ValueError:
                    self._write_json(400, {"status": "rejected", "error": "invalid fault slot"})
                    return
                self._submit("faults", "fault.configure", {"slot": slot})
                return
        self._write_json(404, {"error": "vehicle or endpoint not found"})

    def do_POST(self) -> None:
        path = urlsplit(self.path).path
        if path == "/api/v1/shutdown":
            self.context["request_stop"]()
            self._write_json(202, {"status": "stopping", "error": None})
            return
        route = self._vehicle_route()
        if route == (self.context["sysid"], "environment/gust"):
            self._submit("environment", "environment.gust")
            return
        if route == (self.context["sysid"], "environment/reset"):
            self._submit("environment", "environment.reset")
            return
        if route == (self.context["sysid"], "faults/reset"):
            self._submit("faults", "fault.reset")
            return
        if route is not None and route[0] == self.context["sysid"]:
            parts = route[1].split("/")
            if len(parts) == 3 and parts[0] == "faults" and parts[2] == "clear":
                try:
                    slot = int(parts[1])
                except ValueError:
                    self._write_json(400, {"status": "rejected", "error": "invalid fault slot"})
                    return
                self._submit("faults", "fault.clear", {"slot": slot})
                return
        self._write_json(404, {"error": "vehicle or endpoint not found"})

    def _submit(
        self,
        controller_name: str,
        kind: str,
        extra_payload: dict[str, Any] | None = None,
    ) -> None:
        try:
            payload = self._read_json()
            if extra_payload:
                payload.update(extra_payload)
            result = self.context[controller_name].submit(kind, payload)
            status_code = 202 if result["status"] == "accepted" else 200
            if result["status"] == "rejected":
                status_code = 400
            self._write_json(status_code, result)
        except (RuntimeError, ValueError) as exc:
            self._write_json(400, {"status": "rejected", "error": str(exc)})


class ControlAPIServer:
    def __init__(
        self,
        config: ControlAPIConfig,
        sysid: int,
        environment: EnvironmentController,
        runtime_status: RuntimeStatusStore,
        request_stop: Any,
        faults: FaultController | None = None,
    ) -> None:
        host = "127.0.0.1" if config.host == "localhost" else config.host
        self._server = ThreadingHTTPServer((host, config.port), EnvironmentRequestHandler)
        self._server.daemon_threads = True
        self._server.context = {
            "sysid": sysid,
            "environment": environment,
            "faults": faults or FaultController(FaultSystemConfig(enabled=False)),
            "runtime_status": runtime_status,
            "request_stop": request_stop,
        }
        self._thread = threading.Thread(
            target=self._server.serve_forever,
            name="jsbsim-control-api",
            daemon=True,
        )

    def start(self) -> None:
        self._thread.start()

    def close(self) -> None:
        self._server.shutdown()
        self._server.server_close()
        self._thread.join(timeout=2.0)


# 协议待对齐：当前发送标准 HIL_SENSOR(107) 和 GPS_INPUT(232)，而
# Docs_For_Maker/1_HIL_MAVLink_Protocol.md 定义的是私有
# HIL_IMU_COMPACT(42000) / HIL_NAV_COMPACT(42001)。若飞控不能接收标准消息，
# 需要同时替换本类的发送函数、MAVLink dialect 和飞控端解码逻辑。
class MavlinkSensorPublisher:
    """按仿真时间把 JSBSim 真值单播为 MAVLink 传感器报文。"""

    def __init__(self, config: MavlinkConfig, faults: FaultController) -> None:
        self.config = config
        self._faults = faults
        self._connection: Any | None = None
        # 两条传感器流独立调度；初值为 0，保证仿真启动后的首个物理步立即发送。
        self._next_sensor_time_s = 0.0
        self._next_gps_time_s = 0.0
        self._actuator_frames_rx = 0
        self._actuator_rate_window_start_s = time.perf_counter()
        self._actuator_rate_window_frames = 0
        self._actuator_rx_hz = 0.0
        self._latest_servo_pwm = (0, 0, 0, 0)
        self._last_actuator_rx_wall_s: float | None = None

        if config.enabled:
            # udpout 只负责向目标地址发送，不会等待接收端上线或建立连接。
            endpoint = f"udpout:{config.host}:{config.port}"
            self._connection = mavutil.mavlink_connection(
                endpoint,
                source_system=config.system_id,
                source_component=config.component_id,
                dialect="ardupilotmega",
            )

    def publish_due(self, worker: JSBSimWorker) -> None:
        if self._connection is None:
            return

        # 采样基准必须是仿真时钟，而非墙钟：这样 --no-realtime 和实时运行
        # 都能得到相同的仿真采样时刻。
        sim_time_s = worker.fdm.get_sim_time()
        if sim_time_s >= self._next_sensor_time_s:
            self._send_hil_sensor(worker, sim_time_s)
            self._next_sensor_time_s = self._advance_schedule(
                self._next_sensor_time_s,
                self.config.sensor_rate_hz,
                sim_time_s,
            )

        if sim_time_s >= self._next_gps_time_s:
            self._send_gps_input(worker, sim_time_s)
            self._next_gps_time_s = self._advance_schedule(
                self._next_gps_time_s,
                self.config.gps_rate_hz,
                sim_time_s,
            )

    @staticmethod
    def _advance_schedule(previous_s: float, rate_hz: float, now_s: float) -> float:
        """返回严格晚于当前时刻的下一采样点，跳过停顿期间的过期采样。"""
        period_s = 1.0 / rate_hz
        next_s = previous_s + period_s
        while next_s <= now_s:
            next_s += period_s
        return next_s

    @staticmethod
    def _sim_time_us(sim_time_s: float) -> int:
        """将 JSBSim 单调仿真时间转换为 MAVLink 的微秒时间戳。"""
        return int(round(sim_time_s * 1_000_000.0))

    # TODO(协议对齐)：本函数发送标准 HIL_SENSOR(107)；若切换到私有
    # HIL_IMU_COMPACT(42000)，字段量化方式和有效位定义都必须整体替换。
    def _send_hil_sensor(self, worker: JSBSimWorker, sim_time_s: float) -> None:
        fdm = worker.fdm
        measured = faulted_air_data(fdm, sim_time_s, self._faults)
        self._connection.mav.hil_sensor_send(
            self._sim_time_us(sim_time_s),
            # JSBSim 的 a-pilot 是机体系加速度（ft/s²），按 X 前、Y 右、Z 下发送。
            fdm["accelerations/a-pilot-x-ft_sec2"] * FT_TO_M,
            fdm["accelerations/a-pilot-y-ft_sec2"] * FT_TO_M,
            fdm["accelerations/a-pilot-z-ft_sec2"] * FT_TO_M,
            fdm["velocities/p-rad_sec"],
            fdm["velocities/q-rad_sec"],
            fdm["velocities/r-rad_sec"],
            # 磁场模型尚未接入；有效位也没有声明这三个字段。
            0.0,
            0.0,
            0.0,
            # HIL_SENSOR 的压强字段单位为 hPa（mbar），JSBSim 源数据为 psf。
            float(measured["static_pressure_pa"]) / 100.0,
            float(measured["dynamic_pressure_pa"]) / 100.0,
            float(measured["measured_altitude_m"]),
            (fdm["atmosphere/T-R"] - 491.67) * (5.0 / 9.0),
            HIL_SENSOR_UPDATED_IMU_BARO_AIRSPEED,
        )

    # TODO(协议对齐)：本函数发送标准 GPS_INPUT(232)；切换到私有
    # HIL_NAV_COMPACT(42001) 后需要采用文档指定的定点字段、目标 ID 和序号。
    def _send_gps_input(self, worker: JSBSimWorker, sim_time_s: float) -> None:
        fdm = worker.fdm
        measured = faulted_air_data(fdm, sim_time_s, self._faults)
        # GPS_INPUT 同时携带周数和周内毫秒。当前实现以墙钟为起点并叠加仿真时间；
        # 若要求严格可复现实验，应在启动时固定 GPS 起点，之后只累加 sim_time_s。
        gps_seconds = time.time() - GPS_EPOCH_UNIX_S + sim_time_s
        gps_week = int(gps_seconds // 604_800)
        gps_week_ms = int((gps_seconds - gps_week * 604_800) * 1000.0)

        self._connection.mav.gps_input_send(
            self._sim_time_us(sim_time_s),
            0,  # GPS 实例编号：第一套虚拟 GNSS
            0,  # ignore_flags=0：本帧提供的所有 GPS_INPUT 字段均有效
            gps_week_ms,
            gps_week,
            3,  # 三维定位
            # GPS 坐标以 degE7 表示；速度按 NED（北、东、下）且单位为 m/s。
            int(round(fdm["position/lat-geod-deg"] * 1e7)),
            int(round(fdm["position/long-gc-deg"] * 1e7)),
            float(measured["measured_altitude_m"]),
            0.7,  # 水平精度因子 HDOP
            1.2,  # 垂直精度因子 VDOP
            fdm["velocities/v-north-fps"] * FPS_TO_MPS,
            fdm["velocities/v-east-fps"] * FPS_TO_MPS,
            fdm["velocities/v-down-fps"] * FPS_TO_MPS,
            0.3,  # 速度精度，m/s
            0.8,  # 水平位置精度，m
            1.2,  # 垂直位置精度，m
            12,
        )

    def close(self) -> None:
        """关闭 UDP 句柄，确保异常退出时释放端口相关资源。"""
        if self._connection is not None:
            self._connection.close()

    @staticmethod
    def _surface_pwm(value: float) -> int:
        return int(round(1500.0 + max(-1.0, min(1.0, value)) * 500.0))

    @staticmethod
    def _throttle_pwm(value: float) -> int:
        return int(round(1000.0 + max(0.0, min(1.0, value)) * 1000.0))

    def receive_controls(self, worker: JSBSimWorker) -> None:
        """接收飞控的 UDP 执行机构输出，形成真正的闭环硬件在环。"""
        if self._connection is None:
            return
        # udpout 的首次发送会建立本地临时端口；飞控应把执行机构报文回复到
        # 该源地址。限制单个物理步的处理数量，避免异常网络流量饿死仿真线程。
        for _ in range(100):
            message = self._connection.recv_match(blocking=False)
            if message is None:
                break
            message_type = message.get_type()
            pwm: tuple[int, int, int, int] | None = None
            if message_type == "HIL_ACTUATOR_CONTROLS":
                controls = list(message.controls)
                if len(controls) >= 4:
                    pwm = (
                        self._surface_pwm(float(controls[0])),
                        self._surface_pwm(float(controls[1])),
                        self._throttle_pwm(float(controls[2])),
                        self._surface_pwm(float(controls[3])),
                    )
            elif message_type == "SERVO_OUTPUT_RAW":
                pwm = (
                    int(message.servo1_raw),
                    int(message.servo2_raw),
                    int(message.servo3_raw),
                    int(message.servo4_raw),
                )
            if pwm is None:
                continue
            worker.apply_pwm_outputs(pwm, self._faults)
            now_wall_s = time.perf_counter()
            self._latest_servo_pwm = pwm
            self._last_actuator_rx_wall_s = now_wall_s
            self._actuator_frames_rx += 1
            self._actuator_rate_window_frames += 1
            rate_elapsed_s = now_wall_s - self._actuator_rate_window_start_s
            if rate_elapsed_s >= 1.0:
                self._actuator_rx_hz = self._actuator_rate_window_frames / rate_elapsed_s
                self._actuator_rate_window_frames = 0
                self._actuator_rate_window_start_s = now_wall_s

    def status_state(self) -> dict[str, Any]:
        now_wall_s = time.perf_counter()
        actuator_age_s = (
            None
            if self._last_actuator_rx_wall_s is None
            else now_wall_s - self._last_actuator_rx_wall_s
        )
        return {
            "transport": "udp-mavlink",
            "udp_target": f"{self.config.host}:{self.config.port}",
            "actuator_frames_rx": self._actuator_frames_rx,
            "actuator_rx_hz": round(self._actuator_rx_hz, 1),
            "actuator_link_ok": actuator_age_s is not None and actuator_age_s <= 0.2,
            "actuator_age_ms": None if actuator_age_s is None else round(actuator_age_s * 1000.0, 1),
            "servo_pwm_us": list(self._latest_servo_pwm),
        }

# 桥接无人机的硬件在环代码
class JSBSimRawHILPublisher:
    """供 AP_ExternalAHRS_JSBSim 使用的双向原始 JBS2/JBO1 传输层。

    JBS2 在原 JBS1 传感器数据后附加模型故障与真实执行机构状态；配套
    固件使用 JBO1 帧返回 SERVO1～4 的 PWM 值，并在下一物理步之前将其
    应用到 JSBSim 飞控系统。飞控端仍兼容不含真值扩展的 JBS1。
    """

    def __init__(self, config: SerialHILConfig | UdpHILConfig, faults: FaultController) -> None:
        self.config = config
        self._faults = faults
        self._allspark_bridge = AllsparkHILBridge(faults)
        self._fault_commands_rx = 0
        self._last_fault_reply: dict[str, Any] | None = None
        self._next_sensor_time_s = 0.0
        self._next_gps_time_s = 0.0
        self._gps_week = 0
        self._gps_week_ms = 0
        self._rx = bytearray()
        self._sensor_frames_tx = 0
        self._sensor_rate_window_start_s = time.perf_counter()
        self._sensor_rate_window_frames = 0
        self._sensor_tx_hz = 0.0
        self._servo_frames_rx = 0
        self._servo_crc_errors = 0
        self._latest_servo_pwm = (0, 0, 0, 0)
        self._last_servo_rx_wall_s: float | None = None
        self._servo_rate_window_start_s = time.perf_counter()
        self._servo_rate_window_frames = 0
        self._servo_rx_hz = 0.0
        # 让 GPS 时间相对于仿真时钟保持确定，便于复现实验。
        self._gps_epoch_s = time.time() - GPS_EPOCH_UNIX_S

    @staticmethod
    def _crc16_ccitt(data: bytes) -> int:
        crc = 0
        for value in data:
            crc ^= value << 8
            for _ in range(8):
                crc = ((crc << 1) ^ 0x1021) & 0xFFFF if crc & 0x8000 else (crc << 1) & 0xFFFF
        return crc

    @classmethod
    def _pack_frame(cls, magic: bytes, payload: bytes) -> bytes:
        header = JSBSIM_FRAME_HEADER.pack(magic, len(payload))
        return header + payload + JSBSIM_FRAME_CRC.pack(cls._crc16_ccitt(header + payload))

    def publish_due(self, worker: JSBSimWorker) -> None:
        sim_time_s = worker.fdm.get_sim_time()
        if sim_time_s < self._next_sensor_time_s:
            return
        self._write_frame(self._sensor_frame(worker, sim_time_s))
        self._sensor_frames_tx += 1
        self._sensor_rate_window_frames += 1
        now_wall_s = time.perf_counter()
        sensor_rate_elapsed_s = now_wall_s - self._sensor_rate_window_start_s
        if sensor_rate_elapsed_s >= 1.0:
            self._sensor_tx_hz = self._sensor_rate_window_frames / sensor_rate_elapsed_s
            self._sensor_rate_window_frames = 0
            self._sensor_rate_window_start_s = now_wall_s
        self._next_sensor_time_s = MavlinkSensorPublisher._advance_schedule(
            self._next_sensor_time_s,
            self.config.sensor_rate_hz,
            sim_time_s,
        )

    def _sensor_frame(self, worker: JSBSimWorker, sim_time_s: float) -> bytes:
        fdm = worker.fdm
        measured = faulted_air_data(fdm, sim_time_s, self._faults)
        # JBS2 按 sensor_rate_hz 携带 IMU/状态数据，但 GPS 时间戳只按
        # gps_rate_hz 前进。固件通过 GPS 周数/周内时间是否变化来判断是否出现
        # 新的 GNSS 样本。这里加入很小的容差，避免二进制浮点累计误差把本应
        # 准时发送的样本推迟到下一传感器帧。
        if sim_time_s + 1.0e-9 >= self._next_gps_time_s:
            gps_seconds = self._gps_epoch_s + sim_time_s
            self._gps_week = int(gps_seconds // 604_800)
            self._gps_week_ms = int((gps_seconds - self._gps_week * 604_800) * 1000.0)
            self._next_gps_time_s = MavlinkSensorPublisher._advance_schedule(
                self._next_gps_time_s,
                self.config.gps_rate_hz,
                sim_time_s,
            )
        sensor_payload = JSBSIM_SENSOR_PAYLOAD.pack(
            int(round(sim_time_s * 1000.0)),
            fdm["attitude/phi-rad"], fdm["attitude/theta-rad"], fdm["attitude/psi-rad"],
            fdm["velocities/p-rad_sec"], fdm["velocities/q-rad_sec"], fdm["velocities/r-rad_sec"],
            fdm["accelerations/a-pilot-x-ft_sec2"] * FT_TO_M,
            fdm["accelerations/a-pilot-y-ft_sec2"] * FT_TO_M,
            fdm["accelerations/a-pilot-z-ft_sec2"] * FT_TO_M,
            0.0, 0.0, 0.0,  # JSBSim 磁场模型尚未接入。
            int(round(fdm["position/lat-geod-deg"] * 1e7)),
            int(round(fdm["position/long-gc-deg"] * 1e7)),
            int(round(float(measured["measured_altitude_m"]) * 100.0)),
            fdm["velocities/v-north-fps"] * FPS_TO_MPS,
            fdm["velocities/v-east-fps"] * FPS_TO_MPS,
            fdm["velocities/v-down-fps"] * FPS_TO_MPS,
            float(measured["static_pressure_pa"]),
            float(measured["dynamic_pressure_pa"]),
            (fdm["atmosphere/T-R"] - 491.67) * (5.0 / 9.0),
            3, 12, self._gps_week, self._gps_week_ms,
            0.8, 1.2, 0.3, 0.7, 1.2,
        )
        truth = self._faults.transport_truth(self._sensor_frames_tx)

        def signed_i16(value: float) -> int:
            return max(-32768, min(32767, int(round(value))))

        aoa_cd = signed_i16(float(fdm["aero/alpha-deg"]) * 100.0)
        sideslip_cd = signed_i16(float(fdm["aero/beta-deg"]) * 100.0)
        truth_valid_mask = int(truth["valid_mask"]) | 1  # bit0=迎角/侧滑角
        truth_payload = JSBSIM_TRUTH_PAYLOAD.pack(
            int(worker.config.sysid),
            int(truth["active_fault_count"]),
            int(truth["active_fault_mask"]),
            int(truth["fault_slot"]),
            int(truth["fault_type"]),
            int(truth["fault_location"]),
            int(truth["fault_state"]),
            int(truth["fault_severity_permille"]),
            int(truth["fault_parameter_1_q10000"]),
            int(truth["fault_parameter_2_q10000"]),
            aoa_cd,
            sideslip_cd,
            *truth["actual_surface_cd"],
            *truth["actual_throttle_permille"],
            truth_valid_mask,
        )
        return self._pack_frame(JSBSIM_SENSOR_MAGIC, sensor_payload + truth_payload)

    def receive_controls(self, worker: JSBSimWorker) -> None:
        received = self._read_available()
        if received:
            self._rx.extend(received)

        while True:
            indices = [index for magic in (JSBSIM_OUTPUT_MAGIC, b"JBF1")
                       if (index := self._rx.find(magic)) >= 0]
            if not indices:
                self._rx[:] = self._rx[-3:]
                return
            magic_index = min(indices)
            if magic_index:
                del self._rx[:magic_index]
            minimum_len = JSBSIM_FRAME_HEADER.size + JSBSIM_FRAME_CRC.size
            if len(self._rx) < minimum_len:
                return
            magic, payload_len = JSBSIM_FRAME_HEADER.unpack_from(self._rx)
            expected_len = RELAY_SIZE if magic == b"JBF1" else JSBSIM_OUTPUT_PAYLOAD.size
            if payload_len != expected_len:
                del self._rx[0]
                continue
            frame_len = JSBSIM_FRAME_HEADER.size + payload_len + JSBSIM_FRAME_CRC.size
            if len(self._rx) < frame_len:
                return
            frame = bytes(self._rx[:frame_len])
            received_crc = JSBSIM_FRAME_CRC.unpack_from(frame, frame_len - JSBSIM_FRAME_CRC.size)[0]
            if received_crc != self._crc16_ccitt(frame[:-JSBSIM_FRAME_CRC.size]):
                self._servo_crc_errors += 1
                del self._rx[0]  # Resynchronize even after a damaged length/header.
                continue
            if magic == b"JBF1":
                reply = self._allspark_bridge.execute(
                    frame[JSBSIM_FRAME_HEADER.size:-JSBSIM_FRAME_CRC.size],
                    worker.config.sysid, worker.fdm.get_sim_time(),
                )
                self._fault_commands_rx += 1
                token, target, source, result, error = struct.unpack("<IBBBH", reply)
                self._last_fault_reply = {
                    "token": token, "mav_sysid": target, "source": source,
                    "result": result, "error_code": error,
                }
                self._write_frame(self._pack_frame(b"JBA1", reply))
                del self._rx[:frame_len]
                continue
            _, *pwm = JSBSIM_OUTPUT_PAYLOAD.unpack_from(frame, JSBSIM_FRAME_HEADER.size)
            pwm_tuple = tuple(int(value) for value in pwm)
            worker.apply_pwm_outputs(pwm_tuple, self._faults)
            now_wall_s = time.perf_counter()
            self._latest_servo_pwm = pwm_tuple
            self._last_servo_rx_wall_s = now_wall_s
            self._servo_frames_rx += 1
            self._servo_rate_window_frames += 1
            rate_elapsed_s = now_wall_s - self._servo_rate_window_start_s
            if rate_elapsed_s >= 1.0:
                self._servo_rx_hz = self._servo_rate_window_frames / rate_elapsed_s
                self._servo_rate_window_frames = 0
                self._servo_rate_window_start_s = now_wall_s
            del self._rx[:frame_len]

    def status_state(self) -> dict[str, Any]:
        """返回地面端链路健康状态和最新飞控输出。"""
        now_wall_s = time.perf_counter()
        servo_age_s = (
            None
            if self._last_servo_rx_wall_s is None
            else now_wall_s - self._last_servo_rx_wall_s
        )
        result = {
            "sensor_frames_tx": self._sensor_frames_tx,
            "sensor_tx_hz": round(self._sensor_tx_hz, 1),
            "servo_frames_rx": self._servo_frames_rx,
            "servo_rx_hz": round(self._servo_rx_hz, 1),
            "servo_link_ok": servo_age_s is not None and servo_age_s <= 0.2,
            "servo_age_ms": None if servo_age_s is None else round(servo_age_s * 1000.0, 1),
            "servo_pwm_us": list(self._latest_servo_pwm),
            "servo_crc_errors": self._servo_crc_errors,
            "allspark_fault_frames_rx": self._fault_commands_rx,
            "allspark_last_fault_reply": self._last_fault_reply,
        }
        result.update(self._transport_state())
        return result

    def _write_frame(self, frame: bytes) -> None:
        raise NotImplementedError

    def _read_available(self) -> bytes:
        raise NotImplementedError

    def _transport_state(self) -> dict[str, Any]:
        raise NotImplementedError

    def close(self) -> None:
        raise NotImplementedError


class JSBSimSerialHILPublisher(JSBSimRawHILPublisher):
    """使用本机串口交换原始 JBS2/JBO1 帧。"""

    def __init__(self, config: SerialHILConfig, faults: FaultController) -> None:
        if serial is None:
            raise RuntimeError("serial HIL requires pyserial; install the JSBSim requirements first")
        super().__init__(config, faults)
        # ``serial_for_url`` 同时支持普通 COM 口和 pyserial 的 loop:// 地址，
        # 后者可用于不连接硬件的协议测试。
        self._port = serial.serial_for_url(
            config.device,
            baudrate=config.baud,
            timeout=0,
            write_timeout=0.2,
        )

    def _write_frame(self, frame: bytes) -> None:
        self._port.write(frame)

    def _read_available(self) -> bytes:
        waiting = self._port.in_waiting
        return self._port.read(waiting) if waiting else b""

    def _transport_state(self) -> dict[str, Any]:
        return {
            "transport": "serial-jsbsim",
            "serial_device": self.config.device,
            "serial_open": self._port.is_open,
        }

    def close(self) -> None:
        self._port.close()


class JSBSimUdpHILPublisher(JSBSimRawHILPublisher):
    """使用 UDP 单播交换原始 JBS2/JBO1 帧，适配 LQ-Mesh 串口转网桥。

    每架飞机使用独立的本地监听端口。该链路只承载原始 HIL 帧，不承载
    Mission Planner 的 MAVLink 遥测或控制消息。
    """

    def __init__(self, config: UdpHILConfig, faults: FaultController) -> None:
        super().__init__(config, faults)
        self._socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self._socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        try:
            self._socket.bind((config.bind_host, config.bind_port))
            self._remote = (socket.gethostbyname(config.remote_host), config.remote_port)
            # 不调用 connect()：部分串口转 UDP 网桥的回包源端口与配置的目标端口不同。
            # JBO1 魔数、长度和 CRC 仍由公共解析器校验，发送方向始终使用 _remote。
            self._socket.setblocking(False)
        except OSError:
            self._socket.close()
            raise
        self._last_sender: tuple[str, int] | None = None

    def _write_frame(self, frame: bytes) -> None:
        self._socket.sendto(frame, self._remote)

    def _read_available(self) -> bytes:
        chunks: list[bytes] = []
        while True:
            try:
                data, sender = self._socket.recvfrom(65535)
            except (BlockingIOError, InterruptedError):
                break
            if data:
                chunks.append(data)
                self._last_sender = (str(sender[0]), int(sender[1]))
        # 每个 socket 只服务一架飞机，按到达顺序拼接后可复用串口流式 JBO1 解析器。
        return b"".join(chunks)

    @staticmethod
    def _endpoint_text(endpoint: tuple[Any, ...]) -> str:
        return f"{endpoint[0]}:{endpoint[1]}"

    def _transport_state(self) -> dict[str, Any]:
        return {
            "transport": "udp-jsbsim",
            "udp_bind": self._endpoint_text(self._socket.getsockname()),
            "udp_remote": self._endpoint_text(self._remote),
            "udp_last_sender": None if self._last_sender is None else self._endpoint_text(self._last_sender),
        }

    def close(self) -> None:
        self._socket.close()


class FixedRateRunner:
    """以固定 JSBSim 步长运行物理、传感器发布和低频状态输出。"""
    def __init__(
        self,
        worker: JSBSimWorker,
        publisher: Any,
        environment: EnvironmentController,
        faults: FaultController,
        event_logger: EnvironmentEventLogger,
        fault_logger: FaultEventLogger,
        runtime_status: RuntimeStatusStore,
        realtime: bool,
    ) -> None:
        self.worker = worker
        self.publisher = publisher
        self.environment = environment
        self.faults = faults
        self.event_logger = event_logger
        self.fault_logger = fault_logger
        self.runtime_status = runtime_status
        self.realtime = realtime
        self.stop_requested = False

    def request_stop(self, _signum: int | None = None, _frame: Any = None) -> None:
        # 信号处理函数只设置标志位，避免在异步上下文中关闭套接字或操作 JSBSim。
        self.stop_requested = True

    def run(self, duration_s: float | None) -> int:
        simulation = self.worker.config.simulation
        dt = 1.0 / simulation.rate_hz
        status_period = 1.0 / simulation.status_hz
        # 墙钟仅用于实时节流与实时倍率统计，不参与物理积分或采样调度。
        start_wall = time.perf_counter()
        next_deadline = start_wall
        next_status_sim = 0.0

        while not self.stop_requested:
            if duration_s is not None and self.worker.fdm.get_sim_time() >= duration_s:
                break
            # HTTP 处理线程只把命令加入队列；大气属性在此处由持有 JSBSim
            # 实例的同一物理线程执行。
            current_sim_time = self.worker.fdm.get_sim_time()
            self.environment.drain_commands(current_sim_time)
            self.environment.update(current_sim_time)
            self.faults.drain_commands(current_sim_time)
            self.faults.update(current_sim_time)
            # 在当前 JSBSim 物理步之前应用最新飞控输出。
            self.publisher.receive_controls(self.worker)
            if not self.worker.step():
                print("JSBSim stopped the simulation", file=sys.stderr)
                return 1

            sim_time = self.worker.fdm.get_sim_time()
            self.environment.capture_actual_wind()
            self.publisher.publish_due(self.worker)
            wall_elapsed = max(time.perf_counter() - start_wall, 1.0e-9)
            realtime_factor = sim_time / wall_elapsed

            # 仅打印监控状态，不影响传感器频率；半个 dt 的容差抵消浮点累计误差。
            if sim_time + (dt * 0.5) >= next_status_sim:
                status = self.worker.truth_state(realtime_factor)
                status_state = getattr(self.publisher, "status_state", None)
                if status_state is not None:
                    status.update(status_state())
                environment_status = self.environment.status()
                fault_status = self.faults.status()
                status["environment"] = environment_status
                status["faults"] = fault_status
                self.runtime_status.update(status)
                self.event_logger.log_status(status, environment_status)
                self.fault_logger.log_status(sim_time, fault_status)
                print(json.dumps(status, ensure_ascii=False), flush=True)
                next_status_sim += status_period

            if self.realtime:
                # 截止时刻按固定步长累加，防止休眠误差持续累积到仿真时间中。
                next_deadline += dt
                remaining = next_deadline - time.perf_counter()
                if remaining > 0:
                    time.sleep(remaining)
                elif remaining < -0.25:
                    # 调试暂停或系统卡顿后不进行无限追赶，避免出现密集的突发报文。
                    next_deadline = time.perf_counter()

        return 0


def parse_args(argv: list[str]) -> argparse.Namespace:
    """解析运行参数；界面参数只覆盖链路，飞机和环境仍来自 YAML。"""
    default_config = Path(__file__).resolve().parent / "config" / "vehicle01.yaml"
    parser = argparse.ArgumentParser(description="Run one JSBSim vehicle at a fixed physics rate")
    parser.add_argument("--config", type=Path, default=default_config, help="vehicle YAML file")
    parser.add_argument("--duration", type=float, help="stop after this many simulated seconds")
    parser.add_argument(
        "--transport",
        choices=("config", "serial-jbs", "udp-jbs"),
        default="config",
        help="override the JBS1/JBO1 transport selected in YAML",
    )
    parser.add_argument("--serial-device", help="serial device used by serial-jbs")
    parser.add_argument("--serial-baud", type=int, help="serial baud used by serial-jbs")
    parser.add_argument("--udp-bind-host", help="local address used by udp-jbs")
    parser.add_argument("--udp-bind-port", type=int, help="local port used by udp-jbs")
    parser.add_argument("--udp-remote-host", help="LQ-Mesh/bridge address used by udp-jbs")
    parser.add_argument("--udp-remote-port", type=int, help="LQ-Mesh/bridge port used by udp-jbs")
    parser.add_argument(
        "--no-realtime",
        action="store_true",
        help="run as fast as possible while preserving the configured fixed simulation step",
    )
    args = parser.parse_args(argv)
    if args.duration is not None and args.duration <= 0:
        parser.error("--duration must be positive")
    return args


def main(argv: list[str] | None = None) -> int:
    args = parse_args(sys.argv[1:] if argv is None else argv)
    try:
        config = apply_transport_overrides(load_config(args.config.resolve()), args)
        worker = JSBSimWorker(config)
        worker.initialize()
    except (ValueError, RuntimeError, KeyError) as exc:
        print(f"configuration/startup error: {exc}", file=sys.stderr)
        return 2

    try:
        event_logger = EnvironmentEventLogger(Path(__file__).resolve().parent / "logs", config.sysid)
        fault_logger = FaultEventLogger(event_logger.run_dir)
        faults = FaultController(config.fault_injection, fault_logger)
        environment = EnvironmentController(worker.fdm, config.environment, event_logger)
        environment.initialize()
    except (OSError, KeyError, ValueError) as exc:
        print(f"environment startup error: {exc}", file=sys.stderr)
        return 2

    # 故障控制器创建后再建立发送端，使串口和 MAVLink 测量路径共用同一套
    # 传感器故障状态。
    publisher: Any
    if config.serial_hil.enabled:
        try:
            publisher = JSBSimSerialHILPublisher(config.serial_hil, faults)
        except (RuntimeError, OSError) as exc:
            fault_logger.close()
            event_logger.close()
            print(f"serial HIL startup error: {exc}", file=sys.stderr)
            return 2
    elif config.udp_hil.enabled:
        try:
            publisher = JSBSimUdpHILPublisher(config.udp_hil, faults)
        except OSError as exc:
            fault_logger.close()
            event_logger.close()
            print(f"UDP JBS HIL startup error: {exc}", file=sys.stderr)
            return 2
    else:
        publisher = MavlinkSensorPublisher(config.mavlink, faults)

    runtime_status = RuntimeStatusStore()
    runner = FixedRateRunner(
        worker,
        publisher,
        environment,
        faults,
        event_logger,
        fault_logger,
        runtime_status,
        realtime=config.simulation.realtime and not args.no_realtime,
    )
    signal.signal(signal.SIGINT, runner.request_stop)
    if hasattr(signal, "SIGTERM"):
        signal.signal(signal.SIGTERM, runner.request_stop)

    api_server: ControlAPIServer | None = None
    if config.control_api.enabled:
        try:
            api_server = ControlAPIServer(
                config.control_api,
                config.sysid,
                environment,
                runtime_status,
                runner.request_stop,
                faults,
            )
            api_server.start()
        except OSError as exc:
            publisher.close()
            fault_logger.close()
            event_logger.close()
            print(f"control API startup error: {exc}", file=sys.stderr)
            return 2

    print(
        f"HIL worker ready: sysid={config.sysid} model={config.model} "
        f"physics={config.simulation.rate_hz:g}Hz status={config.simulation.status_hz:g}Hz "
        f"realtime={runner.realtime} "
        f"transport={('serial-jsbsim:' + config.serial_hil.device) if config.serial_hil.enabled else (('udp-jsbsim:' + config.udp_hil.bind_host + ':' + str(config.udp_hil.bind_port) + '->' + config.udp_hil.remote_host + ':' + str(config.udp_hil.remote_port)) if config.udp_hil.enabled else ('udpout:' + config.mavlink.host + ':' + str(config.mavlink.port) if config.mavlink.enabled else 'disabled'))} "
        f"control_api={'http://' + config.control_api.host + ':' + str(config.control_api.port) if config.control_api.enabled else 'disabled'} "
        f"logs={event_logger.run_dir}",
        file=sys.stderr,
        flush=True,
    )
    try:
        return runner.run(args.duration)
    finally:
        runtime_status.set_stopped()
        if api_server is not None:
            api_server.close()
        # 无论正常结束、Ctrl+C 还是运行异常，都释放链路与日志句柄。
        publisher.close()
        fault_logger.close()
        event_logger.close()


if __name__ == "__main__":
    raise SystemExit(main())

"""Process supervisor for running multiple isolated JSBSim HIL vehicles."""

from __future__ import annotations

import argparse
import json
import os
import re
import signal
import subprocess
import sys
import threading
import time
import urllib.error
import urllib.request
from collections import deque
from dataclasses import dataclass
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from typing import Any, TextIO
from urllib.parse import urlsplit

import yaml

from hil_supervisor import VehicleConfig, load_config


MAX_FLEET_VEHICLES = 10


@dataclass(frozen=True)
class FleetConfig:
    host: str
    port: int
    status_hz: float
    shutdown_timeout_s: float


@dataclass(frozen=True)
class VehicleSpec:
    name: str
    config_path: Path
    config: VehicleConfig

    @property
    def control_url(self) -> str:
        host = "127.0.0.1" if self.config.control_api.host == "localhost" else self.config.control_api.host
        return f"http://{host}:{self.config.control_api.port}"

    @property
    def transport(self) -> str:
        if self.config.serial_hil.enabled:
            return f"serial:{self.config.serial_hil.device}"
        if self.config.udp_hil.enabled:
            return (
                f"udp-jbs:{self.config.udp_hil.bind_host}:{self.config.udp_hil.bind_port}"
                f"->{self.config.udp_hil.remote_host}:{self.config.udp_hil.remote_port}"
            )
        return f"udp-mavlink:{self.config.mavlink.host}:{self.config.mavlink.port}"


@dataclass(frozen=True)
class FleetDefinition:
    settings: FleetConfig
    vehicles: tuple[VehicleSpec, ...]


def _mapping(value: Any, name: str) -> dict[str, Any]:
    if not isinstance(value, dict):
        raise ValueError(f"{name} must be a mapping")
    return value


def load_fleet_config(path: Path) -> FleetDefinition:
    path = path.resolve()
    try:
        raw = yaml.safe_load(path.read_text(encoding="utf-8"))
    except OSError as exc:
        raise ValueError(f"cannot read fleet config {path}: {exc}") from exc
    except yaml.YAMLError as exc:
        raise ValueError(f"invalid fleet YAML {path}: {exc}") from exc

    root = _mapping(raw, "fleet config")
    fleet_raw = _mapping(root.get("fleet", {}), "fleet")
    settings = FleetConfig(
        host=str(fleet_raw.get("host", "127.0.0.1")),
        port=int(fleet_raw.get("port", 8750)),
        status_hz=float(fleet_raw.get("status_hz", 2.0)),
        shutdown_timeout_s=float(fleet_raw.get("shutdown_timeout_s", 5.0)),
    )
    if settings.host not in {"127.0.0.1", "localhost", "::1"}:
        raise ValueError("fleet.host must be a loopback address")
    if not 1 <= settings.port <= 65535:
        raise ValueError("fleet.port must be in the range 1..65535")
    if settings.status_hz <= 0:
        raise ValueError("fleet.status_hz must be positive")
    if settings.shutdown_timeout_s <= 0:
        raise ValueError("fleet.shutdown_timeout_s must be positive")

    vehicle_rows = root.get("vehicles")
    if not isinstance(vehicle_rows, list):
        raise ValueError("vehicles must be a list")
    if len(vehicle_rows) > MAX_FLEET_VEHICLES:
        raise ValueError(f"fleet supports at most {MAX_FLEET_VEHICLES} vehicles")

    vehicles: list[VehicleSpec] = []
    for index, row_value in enumerate(vehicle_rows, start=1):
        row = _mapping(row_value, f"vehicles[{index}]")
        if not bool(row.get("enabled", True)):
            continue
        name = str(row.get("name", "")).strip()
        relative_config = str(row.get("config", "")).strip()
        if not name:
            raise ValueError(f"vehicles[{index}].name is required")
        if not relative_config:
            raise ValueError(f"vehicles[{index}].config is required")
        config_path = (path.parent / relative_config).resolve()
        if not config_path.is_file():
            raise ValueError(f"vehicle {name!r} config does not exist: {config_path}")
        config = load_config(config_path)
        if not config.control_api.enabled:
            raise ValueError(f"vehicle {name!r} must enable control_api for fleet management")
        if config.control_api.host not in {"127.0.0.1", "localhost", "::1"}:
            raise ValueError(f"vehicle {name!r} control_api.host must be loopback")
        vehicles.append(VehicleSpec(name=name, config_path=config_path, config=config))

    if not vehicles:
        raise ValueError("fleet must contain at least one enabled vehicle")
    if len(vehicles) > MAX_FLEET_VEHICLES:
        raise ValueError(f"fleet supports at most {MAX_FLEET_VEHICLES} enabled vehicles")

    _validate_unique(vehicles, lambda item: item.name.casefold(), "vehicle name")
    _validate_unique(vehicles, lambda item: item.config.sysid, "vehicle sysid")
    _validate_unique(
        vehicles,
        lambda item: ("127.0.0.1" if item.config.control_api.host == "localhost" else item.config.control_api.host,
                      item.config.control_api.port),
        "control API endpoint",
    )
    fleet_endpoint = ("127.0.0.1" if settings.host == "localhost" else settings.host, settings.port)
    for item in vehicles:
        vehicle_endpoint = (
            "127.0.0.1" if item.config.control_api.host == "localhost" else item.config.control_api.host,
            item.config.control_api.port,
        )
        if vehicle_endpoint == fleet_endpoint:
            raise ValueError(f"vehicle {item.name!r} control API conflicts with fleet API")

    serial_vehicles = [item for item in vehicles if item.config.serial_hil.enabled]
    _validate_unique(
        serial_vehicles,
        lambda item: item.config.serial_hil.device.casefold(),
        "serial HIL device",
    )
    _validate_windows_serial_devices(serial_vehicles)
    udp_vehicles = [item for item in vehicles if item.config.udp_hil.enabled]
    # 每个子进程都会独占一个 UDP socket；JBS1/JBO1 本身没有 MAVLink SYSID，
    # 所以必须用唯一的本地端口和远端 LQ 端点隔离各架飞机的数据流。
    _validate_unique(
        udp_vehicles,
        lambda item: (
            item.config.udp_hil.bind_host.casefold(),
            item.config.udp_hil.bind_port,
        ),
        "UDP JBS local endpoint",
    )
    _validate_unique(
        udp_vehicles,
        lambda item: (
            item.config.udp_hil.remote_host.casefold(),
            item.config.udp_hil.remote_port,
        ),
        "UDP JBS remote endpoint",
    )
    return FleetDefinition(settings=settings, vehicles=tuple(vehicles))


def _validate_unique(items: list[VehicleSpec], key: Any, label: str) -> None:
    seen: dict[Any, str] = {}
    for item in items:
        value = key(item)
        if value in seen:
            raise ValueError(f"duplicate {label} {value!r}: {seen[value]!r} and {item.name!r}")
        seen[value] = item.name


def _windows_serial_devices() -> dict[str, str]:
    """Return COM name -> kernel device name without requiring pywin32."""
    if os.name != "nt":
        return {}
    try:
        import winreg

        with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"HARDWARE\DEVICEMAP\SERIALCOMM") as key:
            result: dict[str, str] = {}
            index = 0
            while True:
                try:
                    kernel_name, com_name, _ = winreg.EnumValue(key, index)
                except OSError:
                    break
                result[str(com_name).casefold()] = str(kernel_name)
                index += 1
            return result
    except OSError:
        return {}


def _validate_windows_serial_devices(vehicles: list[VehicleSpec]) -> None:
    devices = _windows_serial_devices()
    if not devices:
        return
    for item in vehicles:
        device = item.config.serial_hil.device
        kernel_name = devices.get(device.casefold())
        # 未插入的 USB 转串口不会出现在 SERIALCOMM 中；允许离线校验，
        # 真正启动时由 pyserial 给出端口不存在错误。已映射为蓝牙的端口则
        # 必须拒绝，避免像 COM6/COM7 一样打开成功后持续写超时。
        if kernel_name is not None and "bthmodem" in kernel_name.casefold():
            raise ValueError(
                f"vehicle {item.name!r} serial device {device!r} is a Bluetooth modem, not a USB TTL port"
            )


def _request_json(url: str, method: str = "GET", timeout: float = 0.5) -> dict[str, Any]:
    body = b"{}" if method != "GET" else None
    request = urllib.request.Request(url, data=body, method=method)
    request.add_header("Accept", "application/json")
    if body is not None:
        request.add_header("Content-Type", "application/json")
    with urllib.request.urlopen(request, timeout=timeout) as response:
        value = json.loads(response.read().decode("utf-8"))
    if not isinstance(value, dict):
        raise ValueError("API response must be a JSON object")
    return value


class VehicleProcess:
    def __init__(self, spec: VehicleSpec, script: Path, runtime_root: Path, shutdown_timeout_s: float) -> None:
        self.spec = spec
        self._script = script
        self._runtime_dir = runtime_root / self._safe_name(spec.name, spec.config.sysid)
        self._shutdown_timeout_s = shutdown_timeout_s
        self._lock = threading.RLock()
        self._process: subprocess.Popen[str] | None = None
        self._started_wall_s: float | None = None
        self._last_exit_code: int | None = None
        self._last_error: str | None = None
        self._last_output: deque[str] = deque(maxlen=30)
        self._latest_worker_status: dict[str, Any] | None = None

    @staticmethod
    def _safe_name(name: str, sysid: int) -> str:
        safe = re.sub(r"[^A-Za-z0-9_.-]+", "_", name).strip("_.") or "vehicle"
        return f"{sysid:03d}-{safe}"

    def start(self) -> dict[str, Any]:
        with self._lock:
            if self._process is not None and self._process.poll() is None:
                return self.snapshot(include_health=False)
            self._runtime_dir.mkdir(parents=True, exist_ok=True)
            environment = os.environ.copy()
            environment["PYTHONUTF8"] = "1"
            creation_flags = getattr(subprocess, "CREATE_NO_WINDOW", 0)
            try:
                process = subprocess.Popen(
                    [sys.executable, "-u", str(self._script), "--config", str(self.spec.config_path)],
                    cwd=str(self._runtime_dir),
                    env=environment,
                    stdout=subprocess.PIPE,
                    stderr=subprocess.PIPE,
                    text=True,
                    encoding="utf-8",
                    errors="replace",
                    bufsize=1,
                    creationflags=creation_flags,
                )
            except OSError as exc:
                self._last_error = str(exc)
                raise RuntimeError(f"cannot start {self.spec.name!r}: {exc}") from exc
            self._process = process
            self._started_wall_s = time.time()
            self._last_exit_code = None
            self._last_error = None
            self._latest_worker_status = None
            self._start_reader(process.stdout, "stdout")
            self._start_reader(process.stderr, "stderr")
            return self.snapshot(include_health=False)

    def _start_reader(self, stream: TextIO | None, channel: str) -> None:
        if stream is None:
            return
        thread = threading.Thread(
            target=self._read_stream,
            args=(stream, channel),
            name=f"fleet-{self.spec.config.sysid}-{channel}",
            daemon=True,
        )
        thread.start()

    def _read_stream(self, stream: TextIO, channel: str) -> None:
        try:
            for raw_line in stream:
                line = raw_line.rstrip("\r\n")
                if not line:
                    continue
                # 单机 worker 会按 status_hz 输出完整 JSON。多机时逐行转发会
                # 淹没 Mission Planner 控制台；只保留最新状态供总控 API 展示。
                if channel == "stdout" and line.startswith("{"):
                    try:
                        status = json.loads(line)
                    except json.JSONDecodeError:
                        status = None
                    if isinstance(status, dict) and status.get("sysid") == self.spec.config.sysid:
                        with self._lock:
                            self._latest_worker_status = status
                        continue
                with self._lock:
                    self._last_output.append(f"[{channel}] {line}")
                print(f"[{self.spec.name}][{channel}] {line}", flush=True)
        finally:
            stream.close()

    def stop(self) -> dict[str, Any]:
        with self._lock:
            process = self._process
        if process is None or process.poll() is not None:
            return self.snapshot(include_health=False)
        try:
            _request_json(f"{self.spec.control_url}/api/v1/shutdown", method="POST", timeout=1.0)
        except (OSError, ValueError, urllib.error.URLError):
            pass
        try:
            process.wait(timeout=self._shutdown_timeout_s)
        except subprocess.TimeoutExpired:
            process.terminate()
            try:
                process.wait(timeout=2.0)
            except subprocess.TimeoutExpired:
                process.kill()
                process.wait(timeout=2.0)
        with self._lock:
            self._last_exit_code = process.returncode
        return self.snapshot(include_health=False)

    def snapshot(self, include_health: bool = True) -> dict[str, Any]:
        with self._lock:
            process = self._process
            started_wall_s = self._started_wall_s
            last_error = self._last_error
            output = list(self._last_output)
            latest_worker_status = self._latest_worker_status
        pid = None if process is None else process.pid
        return_code = None if process is None else process.poll()
        state = "stopped" if process is None else ("starting" if return_code is None else "exited")
        health: dict[str, Any] | None = None
        if process is not None and return_code is None and include_health:
            try:
                health = _request_json(f"{self.spec.control_url}/api/v1/health")
                state = "running" if bool(health.get("running", True)) else "stopping"
                last_error = None
            except (OSError, ValueError, urllib.error.URLError) as exc:
                state = "starting" if started_wall_s and time.time() - started_wall_s < 10 else "unhealthy"
                last_error = str(exc)
        if return_code is not None:
            self._last_exit_code = return_code
        return {
            "name": self.spec.name,
            "sysid": self.spec.config.sysid,
            "state": state,
            "pid": pid,
            "exit_code": return_code,
            "transport": self.spec.transport,
            "config": str(self.spec.config_path),
            "control_api": self.spec.control_url,
            "started_wall_s": started_wall_s,
            "last_error": last_error,
            "last_output": output,
            "latest_worker_status": latest_worker_status,
            "health": health,
        }


class FleetManager:
    def __init__(self, definition: FleetDefinition, script: Path, runtime_root: Path) -> None:
        self.definition = definition
        self._vehicles = {
            item.config.sysid: VehicleProcess(
                item,
                script,
                runtime_root,
                definition.settings.shutdown_timeout_s,
            )
            for item in definition.vehicles
        }

    def start_all(self) -> dict[str, Any]:
        for vehicle in self._vehicles.values():
            try:
                vehicle.start()
            except RuntimeError as exc:
                print(f"fleet start error: {exc}", file=sys.stderr, flush=True)
        return self.snapshot()

    def stop_all(self) -> dict[str, Any]:
        for vehicle in reversed(tuple(self._vehicles.values())):
            vehicle.stop()
        return self.snapshot(include_health=False)

    def start_vehicle(self, sysid: int) -> dict[str, Any]:
        return self._vehicle(sysid).start()

    def stop_vehicle(self, sysid: int) -> dict[str, Any]:
        return self._vehicle(sysid).stop()

    def restart_vehicle(self, sysid: int) -> dict[str, Any]:
        vehicle = self._vehicle(sysid)
        vehicle.stop()
        return vehicle.start()

    def _vehicle(self, sysid: int) -> VehicleProcess:
        try:
            return self._vehicles[sysid]
        except KeyError as exc:
            raise ValueError(f"unknown vehicle sysid {sysid}") from exc

    def snapshot(self, include_health: bool = True) -> dict[str, Any]:
        vehicles = [item.snapshot(include_health=include_health) for item in self._vehicles.values()]
        return {
            "api": "ready",
            "running": any(item["state"] in {"starting", "running", "unhealthy"} for item in vehicles),
            "vehicle_count": len(vehicles),
            "vehicles": vehicles,
        }


class FleetRequestHandler(BaseHTTPRequestHandler):
    server_version = "JSBSimHILFleet/1.0"

    @property
    def manager(self) -> FleetManager:
        return self.server.manager  # type: ignore[attr-defined]

    @property
    def stop_event(self) -> threading.Event:
        return self.server.stop_event  # type: ignore[attr-defined]

    def log_message(self, format_string: str, *args: Any) -> None:
        print(f"fleet API: {format_string % args}", file=sys.stderr)

    def _write_json(self, status: int, payload: dict[str, Any]) -> None:
        encoded = json.dumps(payload, ensure_ascii=False).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(encoded)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(encoded)

    def _vehicle_action(self) -> tuple[int, str] | None:
        parts = [part for part in urlsplit(self.path).path.split("/") if part]
        if len(parts) != 6 or parts[:3] != ["api", "v1", "vehicles"]:
            return None
        try:
            sysid = int(parts[3])
        except ValueError:
            return None
        if parts[4] != "process":
            return None
        return sysid, parts[5]

    def do_GET(self) -> None:
        path = urlsplit(self.path).path
        if path in {"/api/v1/health", "/api/v1/fleet"}:
            self._write_json(200, self.manager.snapshot())
            return
        self._write_json(404, {"error": "endpoint not found"})

    def do_POST(self) -> None:
        path = urlsplit(self.path).path
        try:
            if path == "/api/v1/start-all":
                self._write_json(200, self.manager.start_all())
                return
            if path == "/api/v1/stop-all":
                self._write_json(200, self.manager.stop_all())
                return
            if path == "/api/v1/shutdown":
                self.stop_event.set()
                self._write_json(202, {"status": "stopping", "error": None})
                return
            action = self._vehicle_action()
            if action is not None:
                sysid, command = action
                if command == "start":
                    result = self.manager.start_vehicle(sysid)
                elif command == "stop":
                    result = self.manager.stop_vehicle(sysid)
                elif command == "restart":
                    result = self.manager.restart_vehicle(sysid)
                else:
                    self._write_json(404, {"error": "unknown process action"})
                    return
                self._write_json(200, result)
                return
            self._write_json(404, {"error": "endpoint not found"})
        except (RuntimeError, ValueError) as exc:
            self._write_json(400, {"error": str(exc)})


class FleetAPIServer:
    def __init__(self, definition: FleetDefinition, manager: FleetManager, stop_event: threading.Event) -> None:
        host = "127.0.0.1" if definition.settings.host == "localhost" else definition.settings.host
        self._server = ThreadingHTTPServer((host, definition.settings.port), FleetRequestHandler)
        self._server.daemon_threads = True
        self._server.manager = manager
        self._server.stop_event = stop_event
        self._thread = threading.Thread(target=self._server.serve_forever, name="fleet-control-api", daemon=True)

    def start(self) -> None:
        self._thread.start()

    def close(self) -> None:
        self._server.shutdown()
        self._server.server_close()
        self._thread.join(timeout=2.0)


def parse_args(argv: list[str]) -> argparse.Namespace:
    default_config = Path(__file__).resolve().parent / "config" / "fleet_example.yaml"
    parser = argparse.ArgumentParser(description="Run and supervise multiple isolated JSBSim HIL vehicles")
    parser.add_argument("--config", type=Path, default=default_config, help="fleet YAML file")
    parser.add_argument("--duration", type=float, help="stop after this many wall-clock seconds")
    parser.add_argument("--validate-only", action="store_true", help="validate configuration without starting workers")
    args = parser.parse_args(argv)
    if args.duration is not None and args.duration <= 0:
        parser.error("--duration must be positive")
    return args


def main(argv: list[str] | None = None) -> int:
    args = parse_args(sys.argv[1:] if argv is None else argv)
    try:
        definition = load_fleet_config(args.config)
    except (ValueError, KeyError) as exc:
        print(f"fleet configuration error: {exc}", file=sys.stderr)
        return 2

    if args.validate_only:
        print(
            json.dumps(
                {
                    "status": "valid",
                    "fleet_api": f"http://{definition.settings.host}:{definition.settings.port}",
                    "vehicles": [
                        {"name": item.name, "sysid": item.config.sysid, "transport": item.transport}
                        for item in definition.vehicles
                    ],
                },
                ensure_ascii=False,
            )
        )
        return 0

    script = Path(__file__).resolve().parent / "hil_supervisor.py"
    runtime_root = Path(__file__).resolve().parent / ".fleet_runtime"
    stop_event = threading.Event()
    manager = FleetManager(definition, script, runtime_root)
    try:
        api_server = FleetAPIServer(definition, manager, stop_event)
    except OSError as exc:
        print(f"fleet API startup error: {exc}", file=sys.stderr)
        return 2

    def request_stop(_signum: int | None = None, _frame: Any = None) -> None:
        stop_event.set()

    signal.signal(signal.SIGINT, request_stop)
    if hasattr(signal, "SIGTERM"):
        signal.signal(signal.SIGTERM, request_stop)

    api_server.start()
    manager.start_all()
    print(
        f"HIL fleet ready: vehicles={len(definition.vehicles)} "
        f"control_api=http://{definition.settings.host}:{definition.settings.port}",
        file=sys.stderr,
        flush=True,
    )
    started = time.monotonic()
    period = 1.0 / definition.settings.status_hz
    try:
        while not stop_event.wait(period):
            print(json.dumps(manager.snapshot(), ensure_ascii=False), flush=True)
            if args.duration is not None and time.monotonic() - started >= args.duration:
                break
    finally:
        manager.stop_all()
        api_server.close()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

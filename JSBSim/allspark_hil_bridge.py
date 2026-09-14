"""Allspark -> flight controller -> MissionPlanner's shared HITL fault controller.

Only the physics thread calls execute(). HTTP/UI commands and these commands
use the same queue, validation, slots, event log and lifecycle implementation.
No serial ports or network sockets are opened by this module.
"""

from __future__ import annotations

import struct
from collections import OrderedDict
from pathlib import Path

from fault_injection import FaultController


FAULT_PAYLOAD = struct.Struct("<HBBBBHiiHHII")
RELAY_HEADER = struct.Struct("<IBB")  # token, target MAV_SYSID, Allspark source
RELAY_ACK = struct.Struct("<IBBBH")   # token, target, source, result, error
RELAY_SIZE = RELAY_HEADER.size + FAULT_PAYLOAD.size
BRIDGE_VERSION = "2026.09.05.3"


def bridge_capabilities() -> dict:
    """Expose the loaded backend, not merely the files present on disk."""
    return {"version": BRIDGE_VERSION, "module_path": str(Path(__file__).resolve()),
            "command_magic": "JBF1", "ack_magic": "JBA1", "sensor_magic": "JBS2",
            "max_slots": 8, "transports": ["serial-jbs", "udp-jbs"]}


class RelayError(ValueError):
    def __init__(self, code: int, message: str):
        super().__init__(message)
        self.code = code


class AllsparkHILBridge:
    def __init__(self, faults: FaultController):
        self.faults = faults
        # Retransmission must not restart ramps, delays or random sequences.
        self._replies: OrderedDict[tuple[int, int, int], tuple[bytes, bytes]] = OrderedDict()

    def execute(self, payload: bytes, sysid: int, sim_time_s: float) -> bytes:
        """Return the JBA1 payload; caller has already checked length and CRC."""
        if len(payload) != RELAY_SIZE:
            raise ValueError("JBF1 payload must be 34 bytes")
        token, target, source = RELAY_HEADER.unpack_from(payload)
        key = (token, target, source)
        if target != sysid or target == 0:
            return RELAY_ACK.pack(token, target, source, 3, 4)
        if key in self._replies:
            previous, reply = self._replies[key]
            return reply if previous == payload else RELAY_ACK.pack(token, target, source, 3, 11)
        result, error = 0, 0
        try:
            # Preserve ordering with UI commands before selecting a free slot.
            self.faults.drain_commands(sim_time_s)
            self.faults.update(sim_time_s)
            kind, command = self._translate(payload[RELAY_HEADER.size:])
            command["origin"] = {
                "transport": "allspark-hil", "mav_sysid": target,
                "source": source, "token": token,
                "command_id": FAULT_PAYLOAD.unpack_from(payload, RELAY_HEADER.size)[0],
            }
            ticket = self.faults.enqueue(kind, command)
            self.faults.drain_commands(sim_time_s)
            if not ticket.result or ticket.result["status"] != "applied":
                raise RelayError(5, str(ticket.result))
            self.faults.update(sim_time_s)
        except RelayError as exc:
            result, error = 3, exc.code
        except (ValueError, TypeError, KeyError):
            result, error = 3, 5
        except RuntimeError:
            result, error = 4, 8
        reply = RELAY_ACK.pack(token, target, source, result, error)
        self._replies[key] = (payload, reply)
        if len(self._replies) > 128:
            self._replies.popitem(last=False)
        return reply

    def _translate(self, payload: bytes) -> tuple[str, dict]:
        (_command_id, action, slot, fault_type, location, severity,
         p1, p2, ramp_in, ramp_out, delay, duration) = FAULT_PAYLOAD.unpack(payload)
        if not self.faults.config.enabled:
            raise RelayError(7, "model fault injection disabled")
        if action == 3:
            return "fault.reset", {}
        if action not in (0, 1, 2):
            raise RelayError(5, "invalid action")
        channels = {item["slot"]: item for item in self.faults.status()["channels"]}
        occupied = {number for number, item in channels.items()
                    if item["phase"] not in ("complete", "disabled")}
        if slot == 255 and action == 1:
            slot = next((number for number in range(min(8, self.faults.config.max_channels))
                         if number not in occupied), 255)
            if slot == 255:
                raise RelayError(8, "no free model fault slots")
        if not 0 <= slot < min(8, self.faults.config.max_channels):
            raise RelayError(5, "slot must be 0..7; auto only valid for inject")
        if action == 0:
            return "fault.clear", {"slot": slot}
        if action == 1 and slot in occupied:
            raise RelayError(8, "slot occupied; use update or clear")
        if action == 2 and slot not in occupied:
            raise RelayError(5, "cannot update an inactive slot")
        # P1 specifies full fault magnitude, exactly as in the MP UI. The
        # incoming severity field is reserved at 1000; outgoing truth reports
        # the actual lifecycle factor. Reject other values instead of ignoring.
        if severity != 1000:
            raise RelayError(5, "severity must be 1000; set magnitude with P1")
        if fault_type in (1, 2):
            name, target, implied_location = "THRUST_LOSS", "PROPULSION", 4
        elif fault_type in (3, 4, 5):
            name = "SURFACE_JAM" if fault_type == 3 else "SURFACE_LOSS"
            target = {1: "ELEVATOR", 2: "RUDDER", 3: "AILERON"}.get(location)
            if target is None:
                raise RelayError(5, "surface location required")
            implied_location = location
        elif 6 <= fault_type <= 13:
            name = ("SENSOR_NOISE", "SENSOR_DRIFT", "SENSOR_BIAS", "SENSOR_FAIL")[(fault_type - 6) % 4]
            target, implied_location = ("AIRSPEED", 5) if fault_type <= 9 else ("ALTITUDE", 6)
        else:
            raise RelayError(9, "unsupported fault type")
        if location not in (0, implied_location):
            raise RelayError(5, "fault type and location disagree")
        if fault_type not in (2, 5) and ramp_in != 0:
            raise RelayError(5, "ramp_in requires a ramp fault type")
        channel = {
            "enabled": True, "fault_type": name, "target": target,
            "profile": "ramp" if fault_type in (2, 5) else "step",
            "level": p1 / 10000.0, "secondary": p2 / 10000.0,
            "start_delay_s": delay / 1000.0, "ramp_in_s": ramp_in / 1000.0,
            "ramp_out_s": ramp_out / 1000.0, "duration_s": duration / 1000.0,
            "auto_recover": duration != 0,
        }
        return "fault.configure", {"slot": slot, "channel": channel}

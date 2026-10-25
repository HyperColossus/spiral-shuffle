"""Start/stop only the isolated Spiral Shuffle processes on an Ubuntu host.

Never signals unrelated services or processes. All pidfiles belong to this
checkout's .runtime directory. Run as the unprivileged account that built it.
"""
from __future__ import annotations

import argparse
import ipaddress
import json
import os
import signal
import socket
import subprocess
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

from linux_runtime import has_framework, bundled_runtime_dependencies

ROOT = Path(__file__).resolve().parent
RUNTIME = ROOT / ".runtime"
LOGS = RUNTIME / "logs"
RUN = RUNTIME / "run"
SERVER = RUNTIME / "server"
REVISION = "V_r806919.Wizard_1_610"
PORTS = (12010, 12343, 12344, 12345, 12346, 12347, 12510, 8090, 13009, 12379)


def _pid_path(which: str) -> Path:
    return RUN / f"{which}.pid"


def _expected_argument(which: str) -> str:
    return str((ROOT / "host.py").resolve()) if which == "patch" else str((SERVER / "Imlight.Director.dll").resolve())


def configured_game_ip() -> str:
    config = SERVER / "Config" / "Imlight.ini"
    for line in config.read_text(encoding="utf-8-sig").splitlines():
        key, separator, value = line.partition("=")
        if separator and key.strip() == "GameServerIP":
            return str(ipaddress.ip_address(value.strip()))
    raise ValueError(f"GameServerIP is missing from {config}")


def owned_process(which: str) -> int | None:
    """Check PID plus exact executable argument to avoid killing another process."""
    pidfile = _pid_path(which)
    if not pidfile.exists():
        return None
    try:
        pid = int(pidfile.read_text().strip())
        cmdline = (Path("/proc") / str(pid) / "cmdline").read_bytes().split(b"\x00")
        expected = os.fsencode(_expected_argument(which))
        return pid if expected in cmdline else None
    except (FileNotFoundError, ValueError, ProcessLookupError, PermissionError):
        return None


def check_ports() -> list[int]:
    busy = []
    for port in PORTS:
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
                sock.settimeout(0.3)
                if sock.connect_ex(("127.0.0.1", port)) == 0:
                    busy.append(port)
        except OSError:
            busy.append(port)
    return busy


def spawn(which: str, argv: list[str], cwd: Path) -> int:
    LOGS.mkdir(parents=True, exist_ok=True)
    RUN.mkdir(parents=True, exist_ok=True)
    logfile = LOGS / f"{which}.log"
    # No shell, no inherited terminal process group, no interference with bot.
    with logfile.open("ab") as output:
        process = subprocess.Popen(argv, cwd=cwd, stdin=subprocess.DEVNULL,
                                   stdout=output, stderr=subprocess.STDOUT,
                                   start_new_session=True, close_fds=True)
    tmp = _pid_path(which).with_suffix(".pid.tmp")
    tmp.write_text(f"{process.pid}\n")
    tmp.replace(_pid_path(which))
    return process.pid


def start() -> int:
    if not (SERVER / "Imlight.Director.dll").is_file():
        print("Run setup_linux.py first; Imlight.Director.dll not built", file=sys.stderr)
        return 2
    if not has_framework(SERVER / "Imlight.Director.runtimeconfig.json"):
        print("Imlight .NET runtime configuration is not framework-dependent; "
              "run: python3 linux_runtime.py repair", file=sys.stderr)
        return 2
    deps = SERVER / "Imlight.Director.deps.json"
    if not deps.is_file():
        print("Missing Imlight dependency manifest; cannot start", file=sys.stderr)
        return 2
    try:
        if bundled_runtime_dependencies(deps):
            print("Imlight dependency manifest still references a bundled CoreCLR; "
                  "run: python3 linux_runtime.py repair-deps", file=sys.stderr)
            return 2
    except (OSError, ValueError) as exc:
        print(f"Invalid Imlight dependency manifest: {exc}", file=sys.stderr)
        return 2
    if owned_process("server") or owned_process("patch"):
        print("Spiral Shuffle is already running or partly running; use status/stop first", file=sys.stderr)
        return 2
    for which in ("server", "patch"):
        if _pid_path(which).exists():
            print(f"Stale/unowned pidfile {_pid_path(which)}; inspect and remove it manually", file=sys.stderr)
            return 2
    busy = check_ports()
    if busy:
        print(f"Cannot start: TCP ports already in use: {busy}", file=sys.stderr)
        return 2
    cfg = RUNTIME / "settings.json"
    if not cfg.is_file():
        print("Missing .runtime/settings.json; run setup_linux.py first", file=sys.stderr)
        return 2
    settings = json.loads(cfg.read_text())
    python = settings.get("python", sys.executable)
    dotnet = settings.get("dotnet", "dotnet")
    game_ip = configured_game_ip()
    patch_pid = spawn("patch", [python, _expected_argument("patch"), "--host", game_ip, "--port", "12379"], ROOT)
    try:
        url = f"http://{game_ip}:12379/{REVISION}/LatestFileList.bin"
        # host.py serves only patch assets on the configured game interface.
        for _ in range(30):
            if not owned_process("patch"):
                raise RuntimeError("Patch process exited; see .runtime/logs/patch.log")
            try:
                with urllib.request.urlopen(urllib.request.Request(url, method="HEAD"), timeout=1) as response:
                    if response.status == 200:
                        break
            except (urllib.error.URLError, OSError, TimeoutError):
                pass
            time.sleep(0.5)
        else:
            raise RuntimeError("Patch server was not ready; check .runtime/logs/patch.log")
        server_pid = spawn("server", [dotnet, _expected_argument("server")], SERVER)
        time.sleep(2)
        if not owned_process("server"):
            raise RuntimeError("Imlight exited; see .runtime/logs/server.log")
        print(f"Spiral Shuffle started (patch PID {patch_pid}, Imlight PID {server_pid}).")
        print("Inspect .runtime/logs/server.log and test client login; process running is not proof of game readiness.")
        return 0
    except Exception as exc:
        print(f"Start failed: {exc}", file=sys.stderr)
        stop_process("server")
        stop_process("patch")
        return 2


def stop_process(which: str) -> bool:
    pid = owned_process(which)
    if pid is None:
        return False
    os.kill(pid, signal.SIGTERM)
    for _ in range(50):
        if owned_process(which) is None:
            break
        time.sleep(0.1)
    else:
        print(f"{which} PID {pid} did not exit; inspect it manually, no force-kill performed")
        return False
    _pid_path(which).unlink(missing_ok=True)
    return True


def stop() -> int:
    for which in ("server", "patch"):
        pidfile = _pid_path(which)
        if pidfile.exists() and not owned_process(which):
            print(f"Unowned or stale pidfile {pidfile}; refusing to signal another process", file=sys.stderr)
            return 2
    stop_process("server")
    stop_process("patch")
    print("Stopped only processes owned by this Spiral Shuffle runtime (if running).")
    return 0


def status() -> int:
    for which in ("patch", "server"):
        print(f"{which}: {'running PID '+str(owned_process(which)) if owned_process(which) else 'not running'}")
    return 0


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("command", choices=("start", "stop", "status"))
    args = parser.parse_args(argv)
    if sys.platform != "linux":
        parser.error("This controller is Linux-only")
    return {"start": start, "stop": stop, "status": status}[args.command]()


if __name__ == "__main__":
    raise SystemExit(main())

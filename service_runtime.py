"""Supervise only this checkout's game processes and signal systemd readiness."""
from __future__ import annotations

import json
import os
from pathlib import Path
import signal
import socket
import threading
import time
from urllib.request import Request, urlopen

ROOT = Path(__file__).resolve().parent


def http_ready(url: str, method: str = "GET") -> bool:
    try:
        with urlopen(Request(url, method=method), timeout=3) as response:
            return response.status == 200
    except (OSError, ValueError):
        return False


def tcp_ready(port: int) -> bool:
    try:
        with socket.create_connection(("127.0.0.1", port), timeout=1):
            return True
    except OSError:
        return False


def game_ready() -> bool:
    return http_ready("http://127.0.0.1:12772/health") and all(
        tcp_ready(port) for port in (12010, *range(12343, 12349)))


def wait_ready(predicate, seconds=180):
    deadline = time.monotonic() + seconds
    while time.monotonic() < deadline:
        if predicate():
            return
        time.sleep(1)
    raise RuntimeError("Service readiness timeout")


def notify_ready():
    address = os.environ.get("NOTIFY_SOCKET")
    if address:
        if address.startswith("@"):
            address = "\0" + address[1:]
        with socket.socket(socket.AF_UNIX, socket.SOCK_DGRAM) as channel:
            channel.connect(address)
            channel.sendall(b"READY=1")


def clean_dead_pidfiles(ctl, started):
    """Only remove pidfiles recorded by this supervisor, after those PIDs exit."""
    for which, pid in started.items():
        path = ctl._pid_path(which)
        if not (Path("/proc") / str(pid)).exists() and path.exists():
            if path.read_text().strip() == str(pid):
                path.unlink()


def main():
    import linux_ctl as ctl
    stop = threading.Event()
    signal.signal(signal.SIGTERM, lambda *_: stop.set())
    signal.signal(signal.SIGINT, lambda *_: stop.set())
    started = {}
    record = ROOT / ".runtime/run/supervisor-owned.json"
    if record.is_file():
        recorded = json.loads(record.read_text())
        if set(recorded) <= {"server", "patch"} and all(type(pid) is int and pid > 1 for pid in recorded.values()):
            clean_dead_pidfiles(ctl, recorded)
    # Do not adopt an existing manual process or an unrelated port listener.
    if ctl.start() != 0:
        return 1
    for which in ("server", "patch"):
        pid = ctl.owned_process(which)
        if pid is not None:
            started[which] = pid
    temporary = record.with_suffix(".new")
    temporary.write_text(json.dumps(started))
    temporary.chmod(0o600)
    temporary.replace(record)
    try:
        if len(started) != 2:
            raise RuntimeError("Owned process startup was incomplete")
        deadline = time.monotonic() + 180
        while not stop.is_set():
            if any(ctl.owned_process(which) != pid for which, pid in started.items()):
                raise RuntimeError("An owned game process exited")
            if game_ready():
                notify_ready()
                print("Spiral Shuffle game and bridge are ready.", flush=True)
                break
            if time.monotonic() >= deadline:
                raise RuntimeError("Game startup readiness timeout")
            stop.wait(1)
        while not stop.wait(1):
            if any(ctl.owned_process(which) != pid for which, pid in started.items()):
                raise RuntimeError("An owned game process exited")
        return 0
    except Exception as error:
        print(f"Game supervision failed ({type(error).__name__}).", flush=True)
        return 1
    finally:
        # linux_ctl validates exact command arguments before signaling anything.
        for which in ("server", "patch"):
            ctl.stop_process(which)
        clean_dead_pidfiles(ctl, started)


if __name__ == "__main__":
    raise SystemExit(main())

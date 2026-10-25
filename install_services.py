"""One-time Ubuntu service installation; run as the existing game owner.

Uses sudo only for lingering, the existing restricted firewall worker and
the project's Caddy route. Never prints or replaces portal.env secrets.
"""
from __future__ import annotations

import getpass
import json
import os
from pathlib import Path
import re
import shutil
import signal
import subprocess
import sys
import tempfile
import time

ROOT = Path(__file__).resolve().parent
USER_UNITS = ("spiral-game.service", "spiral-portal.service", "spiral-auto-deploy.service", "spiral-auto-deploy.timer")


def run(*args, timeout=120):
    subprocess.run(list(args), cwd=ROOT, check=True, timeout=timeout)


def stop_existing_user_units():
    for name in ("spiral-auto-deploy.timer", "spiral-portal.service", "spiral-game.service"):
        state = subprocess.run(["systemctl", "--user", "show", name, "--property=LoadState", "--value"],
                               capture_output=True, text=True, timeout=10)
        if state.stdout.strip() not in ("", "not-found"):
            run("systemctl", "--user", "stop", name)


def stop_manual_api():
    """Signal only an API script owned by this UID and this exact checkout."""
    for entry in Path("/proc").iterdir():
        if not entry.name.isdigit() or int(entry.name) == os.getpid():
            continue
        try:
            if entry.stat().st_uid != os.getuid():
                continue
            args = (entry / "cmdline").read_bytes().split(b"\0")
            if len(args) < 2:
                continue
            if not re.fullmatch(r"python(?:[0-9.]+)?", Path(os.fsdecode(args[0])).name):
                continue
            cwd = (entry / "cwd").resolve()
            scripts = [os.fsdecode(arg) for arg in args[1:] if arg and not arg.startswith(b"-")]
            if not any(Path(script).name == "portal_api.py" and (cwd / script).resolve() == ROOT / "portal_api.py" for script in scripts):
                continue
            pid = int(entry.name)
            os.kill(pid, signal.SIGTERM)
            for _ in range(50):
                if not entry.exists():
                    break
                time.sleep(0.1)
            else:
                raise RuntimeError("The owned API did not stop")
        except (FileNotFoundError, ProcessLookupError, PermissionError):
            continue


def install_root_file(source, destination):
    run("sudo", "-n", "install", "-o", "root", "-g", "root", "-m", "0644", str(source), str(destination))


def configure_caddy():
    target = Path("/etc/caddy/spiral-registration.caddy")
    content = target.read_text()
    marker = next((line for line in content.splitlines() if line.strip().startswith("@signup path /register")), None)
    if marker is None:
        raise RuntimeError("Unexpected registration Caddy route; no automatic edit")
    if "/deployment" in marker.split():
        return
    changed = content.replace(marker, marker + " /deployment", 1)
    backup = target.with_name(target.name + ".before-services")
    if not backup.exists():
        run("sudo", "-n", "cp", "-a", str(target), str(backup))
    with tempfile.TemporaryDirectory() as folder:
        source = Path(folder) / "Caddyfile"
        source.write_text(changed)
        install_root_file(source, target)
        try:
            run("sudo", "-n", "caddy", "validate", "--config", "/etc/caddy/Caddyfile")
            run("sudo", "-n", "systemctl", "reload", "caddy")
        except Exception:
            source.write_text(content)
            install_root_file(source, target)
            raise


def install():
    if not sys.platform.startswith("linux") or os.geteuid() == 0:
        raise RuntimeError("Run as the existing non-root Ubuntu game owner")
    if not re.fullmatch(r"[A-Za-z0-9_./-]+", str(ROOT)):
        raise RuntimeError("Service checkout path contains unsupported characters")
    if ROOT != Path.home() / "spiral-shuffle":
        raise RuntimeError("Install in the existing ~/spiral-shuffle checkout")
    if not (ROOT / ".runtime/portal/portal.env").is_file():
        raise RuntimeError("Existing private portal environment is required")
    if subprocess.check_output(["git", "status", "--porcelain"], cwd=ROOT, text=True).strip():
        raise RuntimeError("Commit or preserve local checkout changes before service installation")
    import auto_deploy as deployer
    deployer.validate_origin(deployer.git("remote", "get-url", "origin"))
    # Dependency installation does not upgrade unrelated packages.
    if not shutil.which("node"):
        run("sudo", "-v")
        run("sudo", "-n", "apt-get", "install", "--yes", "nodejs", timeout=300)
    # Run checks before stopping any game/API processes.
    deployer.local_checks()
    run("sudo", "-v")
    os.environ.setdefault("XDG_RUNTIME_DIR", f"/run/user/{os.getuid()}")
    os.environ.setdefault("DBUS_SESSION_BUS_ADDRESS", f"unix:path=/run/user/{os.getuid()}/bus")
    run("sudo", "-n", "loginctl", "enable-linger", getpass.getuser())
    stop_existing_user_units()
    stop_manual_api()
    import linux_ctl
    if linux_ctl.stop() != 0 or linux_ctl.owned_process("server") or linux_ctl.owned_process("patch"):
        raise RuntimeError("Could not stop the owned game processes safely")
    active = ROOT / ".runtime/server/Imlight.Director.dll"
    backup = ROOT / ".runtime/deploy/install-backup"
    backup.mkdir(mode=0o700, parents=True, exist_ok=True)
    shutil.copy2(active, backup / active.name)
    units = Path.home() / ".config/systemd/user"
    units.mkdir(parents=True, exist_ok=True)
    for name in USER_UNITS:
        target = units / name
        if target.exists():
            saved = backup / name
            if not saved.exists():
                shutil.copy2(target, saved)
        target.write_text((ROOT / "portal" / name).read_text().replace("@ROOT@", str(ROOT)))
    run("systemctl", "--user", "daemon-reload")
    try:
        run(sys.executable, "portal_bridge.py", "install", timeout=300)
        run("systemctl", "--user", "start", "spiral-game.service", timeout=240)
        run("systemctl", "--user", "start", "spiral-portal.service")
        deployer.health()
    except Exception:
        run("systemctl", "--user", "stop", "spiral-portal.service", "spiral-game.service")
        shutil.copy2(backup / active.name, active)
        run("systemctl", "--user", "start", "spiral-game.service", timeout=240)
        run("systemctl", "--user", "start", "spiral-portal.service")
        raise
    # Root-owned firewall worker is stable; the website gets no sudo rights.
    run("sudo", "-n", "install", "-d", "-o", "root", "-g", "root", "-m", "0755", "/usr/local/lib/spiral-shuffle")
    install_root_file(ROOT / "portal/access_firewall.py", "/usr/local/lib/spiral-shuffle/access_firewall.py")
    install_root_file(ROOT / "portal/spiral-shuffle-registered", "/etc/ufw/applications.d/spiral-shuffle-registered")
    with tempfile.TemporaryDirectory() as folder:
        source = Path(folder) / "spiral-access.service"
        source.write_text((ROOT / "portal/spiral-access.service").read_text()
                          .replace("hypercolossus", getpass.getuser())
                          .replace(f"/home/{getpass.getuser()}/spiral-shuffle", str(ROOT)))
        install_root_file(source, "/etc/systemd/system/spiral-access.service")
    install_root_file(ROOT / "portal/spiral-access.timer", "/etc/systemd/system/spiral-access.timer")
    run("sudo", "-n", "systemctl", "daemon-reload")
    run("sudo", "-n", "systemctl", "start", "spiral-access.service")
    run("sudo", "-n", "systemctl", "enable", "--now", "spiral-access.timer")
    configure_caddy()
    deployer.save_state({"revision":deployer.git("rev-parse", "HEAD"), "status":"waiting_for_main"})
    run("systemctl", "--user", "enable", "spiral-game.service", "spiral-portal.service")
    run("systemctl", "--user", "enable", "--now", "spiral-auto-deploy.timer")
    print("Installed: persistent game/API, IP-access timer and tested-main deployment timer.")
    print("Deployment status: https://register.spiral.hypercoloss.us/deployment")
    print("No database, save, game asset or unrelated service was changed.")


if __name__ == "__main__":
    try:
        from auto_deploy import deployment_lock
        with deployment_lock():
            install()
    except Exception as error:
        print(f"Service installation failed ({type(error).__name__}). Existing private configuration was preserved.", file=sys.stderr)
        raise SystemExit(1)

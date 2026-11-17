"""One-time Ubuntu service installation; run with sudo from /opt/spiral-shuffle.

Installs the system units, root-owned helpers and the project's Caddy site
for the dedicated `spiral-shuffle` service user. The game, API, dashboard
and deployer run as that user; only the firewall sync runs as root, from a
root-owned copy. Never prints or replaces portal.env secrets.

Layout (see /opt/SERVER-LAYOUT.md on the server):
  code     /opt/spiral-shuffle (this checkout, owned by spiral-shuffle)
  data     $SPIRAL_DATA_DIR, default /var/lib/spiral-shuffle
           (runtime/ is linked from the checkout's .runtime, Revive101/ holds game data)
"""
from __future__ import annotations

import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile

from profile_paths import DATA_DIR

ROOT = Path(__file__).resolve().parent
SERVICE_USER = "spiral-shuffle"
UNIT_DIR = Path("/etc/systemd/system")
UNITS = ("spiral-shuffle-game.service", "spiral-shuffle-portal.service", "spiral-shuffle-dashboard.service",
         "spiral-shuffle-auto-deploy.service", "spiral-shuffle-auto-deploy.timer",
         "spiral-shuffle-access.service", "spiral-shuffle-access.timer")
ENABLED = ("spiral-shuffle-game.service", "spiral-shuffle-portal.service", "spiral-shuffle-dashboard.service",
           "spiral-shuffle-auto-deploy.timer", "spiral-shuffle-access.timer")
HELPER_DIR = Path("/usr/local/lib/spiral-shuffle")
CADDY_SITE = Path("/etc/caddy/sites/spiral-shuffle.caddy")


def run(*args, timeout=120):
    subprocess.run(list(args), cwd=ROOT, check=True, timeout=timeout)


def as_service_user(*args, timeout=300):
    run("/usr/sbin/runuser", "-u", SERVICE_USER, "--", "env", f"SPIRAL_DATA_DIR={DATA_DIR}", *args, timeout=timeout)


def render_unit(name, root=ROOT, data_dir=DATA_DIR):
    return ((ROOT / "portal" / name).read_text()
            .replace("@ROOT@", str(root)).replace("@DATA_DIR@", str(data_dir)))


def install_root_file(source, destination, mode="0644"):
    run("install", "-o", "root", "-g", "root", "-m", mode, str(source), str(destination))


def install_text(text, destination, mode="0644"):
    with tempfile.TemporaryDirectory() as folder:
        source = Path(folder) / Path(destination).name
        source.write_text(text)
        install_root_file(source, destination, mode)


def check_layout():
    import pwd
    if not sys.platform.startswith("linux") or os.geteuid() != 0:
        raise RuntimeError("Run with sudo on the Ubuntu server")
    for path in (ROOT, DATA_DIR):
        if not re.fullmatch(r"/[A-Za-z0-9_./-]+", str(path)):
            raise RuntimeError("Installation paths contain unsupported characters")
    owner = pwd.getpwnam(SERVICE_USER)
    if owner.pw_uid == 0 or ROOT.stat().st_uid != owner.pw_uid:
        raise RuntimeError("The checkout must be owned by the spiral-shuffle service user")
    runtime = ROOT / ".runtime"
    if not runtime.is_symlink() or runtime.resolve() != (DATA_DIR / "runtime").resolve():
        raise RuntimeError("The checkout's .runtime must link to the data directory's runtime")
    if not (DATA_DIR / "runtime/portal/portal.env").is_file():
        raise RuntimeError("Existing private portal environment is required")


def install_helpers():
    # The firewall sync runs as root, so it must run a root-owned copy.
    run("install", "-d", "-o", "root", "-g", "root", "-m", "0755", str(HELPER_DIR))
    install_root_file(ROOT / "portal/access_firewall.py", HELPER_DIR / "access_firewall.py")
    install_root_file(ROOT / "portal/spiral-shuffle-registered", "/etc/ufw/applications.d/spiral-shuffle-registered")
    install_root_file(ROOT / "portal/spiral-shuffle.rules", "/etc/polkit-1/rules.d/50-spiral-shuffle.rules")
    install_root_file(ROOT / "portal/spiral-shuffle-logs", "/usr/local/sbin/spiral-shuffle-logs", "0755")
    with tempfile.TemporaryDirectory() as folder:
        sudoers = Path(folder) / "spiral-shuffle-logs"
        shutil.copy(ROOT / "portal/spiral-shuffle-logs.sudoers", sudoers)
        run("visudo", "-cf", str(sudoers))
        install_root_file(sudoers, "/etc/sudoers.d/spiral-shuffle-logs", "0440")


def configure_caddy():
    """The site file is the operator's; only check it is imported and valid."""
    if not CADDY_SITE.is_file():
        raise RuntimeError(f"Expected the registration site in {CADDY_SITE}")
    if f"import {CADDY_SITE}" not in Path("/etc/caddy/Caddyfile").read_text():
        raise RuntimeError("The Caddyfile must import the Spiral Shuffle site file")
    run("caddy", "validate", "--config", "/etc/caddy/Caddyfile", "--adapter", "caddyfile")
    run("systemctl", "reload", "caddy")


def install():
    check_layout()
    import auto_deploy as deployer
    deployer.validate_origin(subprocess.check_output(
        ["/usr/sbin/runuser", "-u", SERVICE_USER, "--", "git", "remote", "get-url", "origin"], cwd=ROOT, text=True).strip())
    # Checks run as the service user before any service is (re)started.
    as_service_user(sys.executable, "-c", "import auto_deploy; auto_deploy.local_checks()")
    install_helpers()
    for name in UNITS:
        install_text(render_unit(name), UNIT_DIR / name)
    run("systemctl", "daemon-reload")
    as_service_user(sys.executable, "portal_bridge.py", "install")
    run("systemctl", "enable", "--now", *ENABLED, timeout=300)
    as_service_user(sys.executable, "-c", "import auto_deploy; auto_deploy.health()", timeout=240)
    configure_caddy()
    run("systemctl", "start", "spiral-shuffle-access.service")
    revision = subprocess.check_output(["/usr/sbin/runuser", "-u", SERVICE_USER, "--", "git", "rev-parse", "HEAD"],
                                       cwd=ROOT, text=True).strip()
    as_service_user(sys.executable, "-c",
                    f"import auto_deploy; auto_deploy.save_state({{'revision':'{revision}','status':'current'}})")
    print("Installed: game, API, dashboard, IP-access sync and tested-main deployment as system units.")
    print("Deployment status: https://register.spiral.hypercoloss.us/deployment")
    print("No database, save, game asset or unrelated service was changed.")


if __name__ == "__main__":
    try:
        install()
    except Exception as error:
        print(f"Service installation failed ({type(error).__name__}: {error}). "
              "Existing private configuration was preserved.", file=sys.stderr)
        raise SystemExit(1)

"""Unprivileged pull deployment of CI-approved main commits, with rollback.

Never changes databases, saves, generated game assets, root-owned firewall
helpers, Caddy configuration or unrelated processes. Installation is separate.
"""
from __future__ import annotations

from contextlib import contextmanager
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import re
import shutil
import subprocess
import sys
import time
from urllib.request import Request, urlopen

from service_runtime import game_ready, http_ready, wait_ready

ROOT = Path(__file__).resolve().parent
PRIVATE = ROOT / ".runtime/deploy"
STATE = PRIVATE / "state.json"
REPOSITORY = "HyperColossus/spiral-shuffle"
WORKFLOW = ".github/workflows/deploy-gate.yml"
SHA = re.compile(r"[0-9a-f]{40}")
GAME_UNIT = "spiral-game.service"
API_UNIT = "spiral-portal.service"


class RecoveryFailed(RuntimeError):
    pass


def command(*args, timeout=120):
    # No shell or interpolated commands; output remains in the private journal.
    return subprocess.run(list(args), cwd=ROOT, check=True, capture_output=True,
                          text=True, timeout=timeout).stdout.strip()


def git(*args):
    return command("git", *args)


def systemctl(*args):
    return command("systemctl", "--user", *args, timeout=240)


def save_state(state):
    PRIVATE.mkdir(mode=0o700, parents=True, exist_ok=True)
    PRIVATE.chmod(0o700)
    state["updated_at"] = int(time.time())
    temporary = PRIVATE / "state.new"
    with temporary.open("w", encoding="utf-8") as output:
        os.chmod(temporary, 0o600)
        json.dump(state, output)
    temporary.replace(STATE)


@contextmanager
def deployment_lock():
    import fcntl
    PRIVATE.mkdir(mode=0o700, parents=True, exist_ok=True)
    with (PRIVATE / "lock").open("a") as lock:
        fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
        yield


def validate_origin(remote):
    if remote not in (f"https://github.com/{REPOSITORY}.git", f"https://github.com/{REPOSITORY}", f"git@github.com:{REPOSITORY}.git"):
        raise ValueError("Unexpected deployment origin")


def gate_result(runs, candidate):
    matching = [run for run in runs if run.get("head_sha") == candidate and
                run.get("head_branch") == "main" and run.get("event") == "push" and
                run.get("path") == WORKFLOW]
    if not matching:
        return "pending"
    latest = max(matching, key=lambda run: (run.get("run_number", 0), run.get("run_attempt", 0)))
    if latest.get("status") != "completed":
        return "pending"
    return "passed" if latest.get("conclusion") == "success" else "failed"


def check_gate(candidate):
    request = Request(f"https://api.github.com/repos/{REPOSITORY}/actions/runs?head_sha={candidate}&per_page=100",
                      headers={"Accept":"application/vnd.github+json", "User-Agent":"spiral-shuffle-deployer"})
    with urlopen(request, timeout=20) as response:
        result = json.load(response)
    return gate_result(result.get("workflow_runs", []), candidate)


def deployment_kind(paths):
    """Explicit scope: website/API/bridge operations, never generated game data."""
    api = bridge = False
    allowed_api = {"portal_api.py", "portal_access.py"}
    allowed_bridge = {"PortalRegistration.cs", "portal_bridge.py"}
    allowed_ops = {"auto_deploy.py", "service_runtime.py", "install_services.py"}
    for value in paths:
        path = PurePosixPath(value)
        if path.is_absolute() or ".." in path.parts:
            raise ValueError("Invalid deployment path")
        if value in allowed_api:
            api = True
        elif value in allowed_bridge:
            bridge = True
        elif value in allowed_ops:
            # Running supervisors may be older than this checkout. Explicit
            # operator reinstall is required to change their service contract.
            raise ValueError("Service supervisor update requires installation")
        elif (value.startswith(("docs/", "tests/", ".github/")) or value.startswith("README") or
              value in (".gitignore", ".gitattributes")):
            continue
        else:
            raise ValueError("Change outside automatic API/bridge deployment scope")
    return "bridge" if bridge else "api" if api else "website"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def health():
    wait_ready(lambda: game_ready() and http_ready("http://127.0.0.1:12771/health"), seconds=180)


def local_checks():
    command(sys.executable, "-m", "unittest", "discover", "-s", "tests", "-p", "test_portal*.py", "-q", timeout=120)
    command(sys.executable, "-m", "unittest", "discover", "-s", "tests", "-p", "test_operations.py", "-q", timeout=60)
    command("node", "tests/portal_ui.mjs", timeout=30)


def apply_candidate(previous, candidate, kind):
    binary = ROOT / ".runtime/server/Imlight.Director.dll"
    core = ROOT / ".runtime/server/Imlight.CoreLib.dll"
    backup = PRIVATE / "backups" / candidate
    backup.mkdir(mode=0o700, parents=True, exist_ok=False)
    shutil.copy2(binary, backup / binary.name)
    shutil.copy2(core, backup / core.name)
    core_before = digest(core)
    api_stopped = game_stopped = False
    try:
        git("checkout", "--detach", candidate)
        local_checks()
        if kind != "website":
            systemctl("stop", API_UNIT)
            api_stopped = True
            if kind == "bridge":
                systemctl("stop", GAME_UNIT)
                game_stopped = True
                command(sys.executable, "portal_bridge.py", "install", timeout=300)
                if digest(core) != core_before:
                    raise RuntimeError("Unexpected CoreLib modification")
                systemctl("start", GAME_UNIT)
            systemctl("start", API_UNIT)
        health()
    except Exception as original:
        # Stop only this project's services. Restore code and Director together.
        try:
            if api_stopped:
                systemctl("stop", API_UNIT)
            if game_stopped:
                systemctl("stop", GAME_UNIT)
                shutil.copy2(backup / binary.name, binary)
                if digest(core) != core_before:
                    shutil.copy2(backup / core.name, core)
            git("checkout", "--detach", previous)
            if game_stopped:
                systemctl("start", GAME_UNIT)
            if api_stopped:
                systemctl("start", API_UNIT)
            health()
        except Exception as recovery:
            raise RecoveryFailed("Automatic rollback could not restore health") from recovery
        raise original


def recover_interrupted(state):
    """A reboot/crash during deployment must not silently accept a partial update."""
    previous, candidate = state["revision"], state.get("candidate", "")
    if not SHA.fullmatch(previous) or not SHA.fullmatch(candidate):
        raise RecoveryFailed("Invalid deployment journal")
    if git("status", "--porcelain") or git("rev-parse", "HEAD") not in (previous, candidate):
        raise RecoveryFailed("Checkout changed during deployment")
    kind = state.get("kind")
    systemctl("stop", API_UNIT)
    if kind == "bridge":
        systemctl("stop", GAME_UNIT)
        backup = PRIVATE / "backups" / candidate
        if backup.is_dir():
            for name in ("Imlight.Director.dll", "Imlight.CoreLib.dll"):
                shutil.copy2(backup / name, ROOT / ".runtime/server" / name)
    git("checkout", "--detach", previous)
    if kind == "bridge":
        systemctl("start", GAME_UNIT)
    systemctl("start", API_UNIT)
    health()
    state.update(status="rolled_back", failed_revision=candidate)
    save_state(state)


def deploy():
    if not sys.platform.startswith("linux") or os.geteuid() == 0:
        raise RuntimeError("Deployment must run as the non-root Ubuntu game owner")
    if not STATE.is_file():
        raise RuntimeError("Run install_services.py once first")
    state = json.loads(STATE.read_text())
    previous = state["revision"]
    if not SHA.fullmatch(previous):
        raise ValueError("Invalid deployed revision")
    validate_origin(git("remote", "get-url", "origin"))
    if state.get("status") == "deploying":
        recover_interrupted(state)
        return
    if git("status", "--porcelain") or git("rev-parse", "HEAD") != previous:
        state["status"] = "blocked_local_changes"
        save_state(state)
        return
    try:
        # Public HTTPS fetch needs no deploy key or interactive Git credentials.
        git("fetch", "--no-tags", f"https://github.com/{REPOSITORY}.git", "main")
    except (OSError, subprocess.SubprocessError):
        state["status"] = "check_failed"
        save_state(state)
        raise
    candidate = git("rev-parse", "FETCH_HEAD")
    if not SHA.fullmatch(candidate):
        raise ValueError("Invalid candidate revision")
    state["candidate"] = candidate
    if candidate == previous:
        state["status"] = "current"
        save_state(state)
        return
    # No downgrade to today's old main while this feature is still a draft.
    try:
        if git("show", f"{candidate}:deploy-contract.json") != '{"version":1,"scope":"portal-bridge"}':
            raise ValueError("Unsupported deployment contract")
    except (subprocess.CalledProcessError, ValueError):
        state["status"] = "waiting_for_main"
        save_state(state)
        return
    if state.get("failed_revision") == candidate:
        state["status"] = "blocked_failed_revision"
        save_state(state)
        return
    try:
        gate = check_gate(candidate)
    except (OSError, ValueError):
        state["status"] = "check_failed"
        save_state(state)
        raise
    if gate != "passed":
        state["status"] = "waiting_for_checks" if gate == "pending" else "blocked_failed_checks"
        save_state(state)
        return
    try:
        kind = deployment_kind(git("diff", "--name-only", previous, candidate).splitlines())
    except ValueError:
        state["status"] = "blocked_scope"
        save_state(state)
        return
    state["status"] = "deploying"
    state["kind"] = kind
    save_state(state)
    try:
        apply_candidate(previous, candidate, kind)
    except Exception as error:
        state.update(status="recovery_failed" if isinstance(error, RecoveryFailed) else "rolled_back",
                     failed_revision=candidate, failure_type=type(error).__name__)
        save_state(state)
        raise
    state.update(revision=candidate, status="current")
    state.pop("failed_revision", None)
    state.pop("failure_type", None)
    save_state(state)


def main():
    try:
        with deployment_lock():
            deploy()
    except BlockingIOError:
        return 0
    except Exception as error:
        print(f"Spiral deployment failed ({type(error).__name__}); inspect private service journal.", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

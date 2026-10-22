"""Repair the Linux Imlight runtime configuration without rebuilding assets.

The Revive101 Linux bundle can carry a self-contained runtimeconfig.json,
while Spiral Shuffle launches rebuilt framework-dependent DLLs via dotnet.
Only the fork's private .runtime/server is modified.
"""
from __future__ import annotations

import argparse
import json
import os
import shutil
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent
CONFIG = ROOT / ".runtime/server/Imlight.Director.runtimeconfig.json"
FRAMEWORK = {"name": "Microsoft.NETCore.App", "version": "10.0.0"}


def has_framework(path: Path) -> bool:
    """True if the app selects an installed shared-framework family."""
    try:
        options = json.loads(path.read_text(encoding="utf-8"))["runtimeOptions"]
        frameworks = options.get("frameworks", [])
        if isinstance(options.get("framework"), dict):
            frameworks = [*frameworks, options["framework"]]
        return any(isinstance(entry, dict) and entry.get("name") == "Microsoft.NETCore.App"
                   and str(entry.get("version", "")).startswith("10.")
                   for entry in frameworks)
    except (OSError, ValueError, KeyError, TypeError):
        return False


def repair_runtimeconfig(path: Path = CONFIG) -> bool:
    """Add a .NET 10 runtime framework reference. Return True if modified.

    Preserves configProperties, and makes a one-time backup. Does not touch
    the original Revive101 distribution, the player database, or game assets.
    """
    if not path.with_name("Imlight.Director.dll").is_file():
        raise FileNotFoundError(f"Imlight.Director.dll is missing beside {path}")
    if has_framework(path):
        return False
    settings = json.loads(path.read_text(encoding="utf-8")) if path.exists() else {}
    if not isinstance(settings, dict):
        raise ValueError("Invalid runtimeconfig root object")
    options = settings.setdefault("runtimeOptions", {})
    if not isinstance(options, dict):
        raise ValueError("Invalid runtimeOptions in runtimeconfig")
    if options.get("frameworks") or options.get("framework"):
        raise ValueError("Existing framework definition requires manual review")
    options.pop("includedFrameworks", None)  # self-contained deployment marker
    options["tfm"] = "net10.0"
    options["framework"] = FRAMEWORK.copy()

    if path.exists():
        backup = path.with_name("Imlight.Director.runtimeconfig.before-linux-fix.json")
        if not backup.exists():
            shutil.copy2(path, backup)
    path.parent.mkdir(parents=True, exist_ok=True)
    fd, name = tempfile.mkstemp(prefix=".runtimeconfig-", suffix=".tmp", dir=path.parent)
    try:
        with os.fdopen(fd, "w", encoding="utf-8") as file:
            json.dump(settings, file, indent=2)
            file.write("\n")
            file.flush()
            os.fsync(file.fileno())
        os.chmod(name, 0o600)
        os.replace(name, path)
    finally:
        if os.path.exists(name):
            os.unlink(name)
    return True


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["check", "repair"])
    args = parser.parse_args()
    if args.action == "check":
        print("Runtime configuration: OK" if has_framework(CONFIG)
              else "Runtime configuration missing .NET 10 framework reference; run: python3 linux_runtime.py repair")
        return 0 if has_framework(CONFIG) else 2
    changed = repair_runtimeconfig()
    print("Updated the Linux .NET 10 runtime configuration." if changed
          else "Linux .NET 10 runtime configuration is already correct.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

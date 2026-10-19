"""Paths for the private local runtime."""
import json
from pathlib import Path
from typing import TypedDict, cast


class Settings(TypedDict):
    revive_path: str
    client_path: str
    python: str
    dotnet: str
    seed: str


ROOT = Path(__file__).resolve().parent
RUNTIME = ROOT / ".runtime"
REVISION = "V_r806919.Wizard_1_610"


def load_settings() -> Settings:
    path = RUNTIME / "settings.json"
    if not path.is_file():
        raise RuntimeError("Run Setup.cmd first.")
    return cast(Settings, json.loads(path.read_text(encoding="utf-8")))


PATCH = RUNTIME / "patch"
PATCH_VERSION = PATCH / REVISION
GENERATED = RUNTIME / "generated"
NATIVE = RUNTIME / "native"
SERVER = RUNTIME / "server"
CLIENT = RUNTIME / "client"


def base_version() -> Path:
    return Path(load_settings()["revive_path"]) / "Aurorium/data" / REVISION

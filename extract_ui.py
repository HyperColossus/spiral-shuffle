"""Read the native menu prototypes from the supplied game archive."""
import zlib
from pathlib import Path

from pack_assets import index_archive
from profile_paths import ROOT, base_version


def main() -> None:
    archive = base_version() / "Data/GameData/Root.wad"
    _, records = index_archive(archive)
    entries = {entry.name: entry for entry in records}
    destination = ROOT / ".build/research"
    destination.mkdir(parents=True, exist_ok=True)
    filenames = {
        "GUI/CharacterSelectWindow.gui": "CharacterSelectWindow.gui.bin",
        "GUI/ToolTip.gui": "ToolTip.gui",
    }
    with archive.open("rb") as stream:
        for source, filename in filenames.items():
            entry = entries[source]
            stream.seek(entry.offset)
            data = stream.read(entry.compressed_size if entry.compressed else entry.size)
            raw = zlib.decompress(data) if entry.compressed else data
            if len(raw) != entry.size:
                raise ValueError(f"Truncated GUI prototype: {source}")
            (destination / filename).write_bytes(raw)


if __name__ == "__main__":
    main()

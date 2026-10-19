"""Build an isolated WAD and matching patch metadata without changing travel data."""
import hashlib
import json
import os
import struct
import xml.etree.ElementTree as ET
import zipfile
import zlib
from dataclasses import dataclass
from pathlib import Path

from profile_paths import GENERATED, NATIVE, PATCH_VERSION, base_version


ROOT = Path(__file__).resolve().parent
BASE = base_version()
OUTPUT = Path(os.environ.get("SPIRAL_PATCH_DIRECTORY", str(PATCH_VERSION)))


@dataclass(frozen=True)
class Entry:
    name: str
    offset: int
    size: int
    compressed_size: int
    compressed: int
    crc: int


def index_archive(path: Path) -> tuple[int, list[Entry]]:
    with path.open("rb") as stream:
        if stream.read(5) != b"KIWAD":
            raise ValueError("Invalid WAD")
        version, count = struct.unpack("<II", stream.read(8))
        if version >= 2:
            stream.read(1)
        entries: list[Entry] = []
        for _ in range(count):
            offset, size, compressed_size, compressed, crc, name_size = struct.unpack("<IIIBII", stream.read(21))
            name = stream.read(name_size).decode("utf-8").rstrip("\0")
            entries.append(Entry(name, offset, size, compressed_size, compressed, crc))
    return version, entries


def crc32(data: bytes) -> int:
    return zlib.crc32(data, 0xFFFFFFFF) ^ 0xFFFFFFFF


def build_wad(source: Path, destination: Path, overrides: dict[str, bytes]) -> dict[str, int]:
    version, old_entries = index_archive(source)
    existing = {entry.name for entry in old_entries}
    names = [entry.name for entry in old_entries] + sorted(overrides.keys() - existing)
    header_size = 13 + int(version >= 2) + sum(22 + len(name.encode()) for name in names)
    header_size = (header_size + 511) // 512 * 512
    entries: list[Entry] = []
    destination.parent.mkdir(parents=True, exist_ok=True)
    with source.open("rb") as original, destination.open("w+b") as target:
        target.seek(header_size)
        old = {entry.name: entry for entry in old_entries}
        for name in names:
            offset = target.tell()
            if name in overrides:
                raw = overrides[name]
                packed = zlib.compress(raw)
                compressed = int(len(packed) < len(raw))
                payload = packed if compressed else raw
                entry = Entry(name, offset, len(raw), len(packed) if compressed else 0, compressed, crc32(payload))
            else:
                prior = old[name]
                original.seek(prior.offset)
                payload = original.read(prior.compressed_size if prior.compressed else prior.size)
                entry = Entry(name, offset, prior.size, prior.compressed_size, prior.compressed, prior.crc)
            target.write(payload)
            entries.append(entry)
        size = target.tell()
        target.seek(0)
        target.write(b"KIWAD" + struct.pack("<II", version, len(entries)))
        if version >= 2:
            target.write(b"\0")
        for entry in entries:
            name = entry.name.encode() + b"\0"
            target.write(struct.pack("<IIIBII", entry.offset, entry.size, entry.compressed_size,
                                     entry.compressed, entry.crc, len(name)) + name)
        target.write(b"\0" * (header_size - target.tell()))
        target.seek(0)
        header = target.read(header_size)
        checksum = 0xFFFFFFFF
        target.seek(0)
        while chunk := target.read(1024 * 1024):
            checksum = zlib.crc32(chunk, checksum)
    return {"Size": size, "HeaderSize": header_size, "CompressedHeaderSize": len(zlib.compress(header)) + 12,
            "CRC": checksum ^ 0xFFFFFFFF, "HeaderCRC": crc32(header)}


def update_lists(metadata: dict[str, int]) -> None:
    xml = ET.parse(BASE / "LatestFileList.xml")
    records = [record for record in xml.iter("RECORD") if record.findtext("SrcFileName") == "Data/GameData/Root.wad"]
    if len(records) != 1:
        raise ValueError("Root patch record missing or duplicated")
    record = records[0]
    original = {name: int(record.findtext(name, "0")) for name in metadata}
    for name, value in metadata.items():
        record.find(name).text = str(value)
    xml.write(OUTPUT / "LatestFileList.xml", encoding="utf-8", xml_declaration=True)
    binary = bytearray((BASE / "LatestFileList.bin").read_bytes())
    filename = b"Data/GameData/Root.wad"
    location = binary.find(filename)
    if location < 0 or binary.find(filename, location + 1) >= 0:
        raise ValueError("Unexpected binary patch record layout")
    offset = location + len(filename)
    fields = ("FileType", "Size", "HeaderSize", "CompressedHeaderSize", "CRC", "HeaderCRC")
    expected = struct.pack("<H6I", 0, 3, *(original[name] for name in fields[1:]))
    if binary[offset:offset + len(expected)] != expected:
        raise ValueError("Binary record did not match the XML metadata")
    binary[offset:offset + len(expected)] = struct.pack("<H6I", 0, 3, *(metadata[name] for name in fields[1:]))
    (OUTPUT / "LatestFileList.bin").write_bytes(binary)


def main() -> None:
    with zipfile.ZipFile(GENERATED / "overrides.zip") as archive:
        overrides = {name: archive.read(name) for name in archive.namelist()}
    overrides["GUI/RandomizerMenu.gui"] = (NATIVE / "RandomizerMenu.gui").read_bytes()
    inspector = NATIVE / "RandomizerEnemyInspector.gui"
    if inspector.exists():
        overrides["GUI/RandomizerEnemyInspector.gui"] = inspector.read_bytes()
    overrides["Scripts/Randomizer/EnemyInspector.lua"] = (ROOT / "EnemyInspector.lua").read_bytes()
    overrides["GUI/RandomizerSpellReward.gui"] = (NATIVE / "RandomizerSpellReward.gui").read_bytes()
    overrides["Scripts/Randomizer/SpellRewards.lua"] = (ROOT / "SpellRewards.lua").read_bytes()
    for mode in [*range(1, 7), *range(17, 23)]:
        overrides[f"Scripts/Randomizer/Menu{mode}.lua"] = (NATIVE / f"Menu{mode}.lua").read_bytes()
    forbidden = [name for name in overrides if name != "TemplateManifest.xml" and
                 not name.startswith(("ObjectData/", "Spells/", "GUI/Randomizer", "Scripts/Randomizer/"))]
    if forbidden:
        raise ValueError(f"Unexpected assets changed: {forbidden}")
    metadata = build_wad(BASE / "Data/GameData/Root.wad", OUTPUT / "Data/GameData/Root.wad", overrides)
    update_lists(metadata)
    manifest = {"Metadata": metadata, "ChangedAssets": sorted(overrides), "TravelDataChanged": False,
                "RootSha256": hashlib.file_digest((OUTPUT / "Data/GameData/Root.wad").open("rb"), "sha256").hexdigest()}
    (GENERATED / "asset-manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    print(f"Packed {len(overrides)} overrides; Root.wad is {metadata['Size']:,} bytes. Travel data unchanged.")


if __name__ == "__main__":
    main()

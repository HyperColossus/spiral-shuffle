"""Verify archive integrity and prove that travel and spawn assets are unchanged."""
import hashlib
import json
import xml.etree.ElementTree as ET
import zlib
from pathlib import Path

from pack_assets import BASE, OUTPUT, ROOT, crc32, index_archive
from profile_paths import GENERATED, NATIVE, SERVER


def read_payload(path: Path, offset: int, size: int) -> bytes:
    with path.open("rb") as stream:
        stream.seek(offset)
        return stream.read(size)


def main() -> None:
    original = BASE / "Data/GameData/Root.wad"
    patched = OUTPUT / "Data/GameData/Root.wad"
    _, old_entries = index_archive(original)
    _, new_entries = index_archive(patched)
    old = {entry.name: entry for entry in old_entries}
    new = {entry.name: entry for entry in new_entries}
    manifest = json.loads((GENERATED / "asset-manifest.json").read_text())
    changed = set(manifest["ChangedAssets"])
    verified = 0
    with original.open("rb") as source, patched.open("rb") as target:
        for name, entry in new.items():
            target.seek(entry.offset)
            payload = target.read(entry.compressed_size if entry.compressed else entry.size)
            assert len(payload) == (entry.compressed_size if entry.compressed else entry.size), name
            assert crc32(payload) == entry.crc, name
            if name in changed:
                raw = zlib.decompress(payload) if entry.compressed else payload
                assert len(raw) == entry.size, name
            else:
                prior = old[name]
                source.seek(prior.offset)
                assert payload == source.read(prior.compressed_size if prior.compressed else prior.size), name
                verified += 1
    assert not (old.keys() - new.keys())
    assert not any(name in changed for name in old if any(token in name.lower() for token in ("gateway", "worldhubzones", "zonetransfer", "spawnpoint")))
    gui = ET.parse(NATIVE / "RandomizerMenu.gui")
    controls = {element.findtext("m_sName") for element in gui.iter("Class")}
    assert {"shuffleMode1", "shuffleMode2", "shuffleMode3", "shuffleNew", "shuffleContinue", "shuffleCreatures"} <= controls
    config = (SERVER / "Config/Imlight.ini").read_text()
    assert "RandomizeGateways = false" in config and "StartingLevel = 1" in config
    with original.open("rb") as stream:
        original_hash = hashlib.file_digest(stream, "sha256").hexdigest()
    report = {"UnchangedAssetsVerified": verified, "ModifiedAssetsCrcVerified": len(changed),
              "TravelAndSpawnAssetsUnchanged": True, "NativeMenuControlsVerified": True,
              "OriginalRootSha256": original_hash, "ModRootSha256": manifest["RootSha256"]}
    (GENERATED / "verification.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()

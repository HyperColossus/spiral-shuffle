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
    assert {"shuffleMode1", "shuffleMode2", "shuffleMode3", "shuffleNew", "shuffleContinue", "shuffleCreatures", "shuffleVendors"} <= controls
    assert {"shuffleSpellDrops", "shuffleDropLess", "shuffleDropMore", "shuffleDropRate"} <= controls
    assert {"shuffleDropMode", "shuffleDropInput", "shuffleDropApply"} <= controls
    assert "shuffleItemCards" in controls
    assert "shuffleInfiniteMana" in controls
    inspector = ET.parse(NATIVE / "RandomizerEnemyInspector.gui")
    assert "enemySpellDrop" in {element.findtext("m_sName") for element in inspector.iter("Class")}
    reward = ET.parse(NATIVE / "RandomizerSpellReward.gui")
    assert {"rewardTitle", "rewardCardHolder", "rewardSchool", "rewardCost", "rewardAccuracy", "rewardDismiss"} <= {
        element.findtext("m_sName") for element in reward.iter("Class")
    }
    assert "Scripts/Randomizer/SpellRewards.lua" in new
    catalog = json.loads((GENERATED / "catalog.json").read_text(encoding="utf-8"))
    assert all(f"GUI/Randomizer/SpellDrop{spell_id}.gui" in new
               for spell in catalog["DropSpells"] for spell_id in spell["Variants"])
    for mode in [*range(1, 7), *range(17, 23)]:
        name = f"Scripts/Randomizer/Menu{mode}.lua"
        entry = new[name]
        payload = read_payload(patched, entry.offset, entry.compressed_size if entry.compressed else entry.size)
        script = zlib.decompress(payload) if entry.compressed else payload
        assert script == (NATIVE / f"Menu{mode}.lua").read_bytes(), name
        assert f"local initialChoice = {mode};".encode() in script, name
        assert b"INITIAL_MODE" not in script and b'FindNamedWindow("shuffleVendors")' in script, name
        assert b"SpiralSpellDrops" in script and b"shuffleSpellDrops" in script, name
        assert b"SpiralItemCards" in script and b"shuffleItemCards" in script, name
        assert b"SpiralInfiniteMana" in script and b"shuffleInfiniteMana" in script, name
    config = (SERVER / "Config/Imlight.ini").read_text()
    assert "RandomizeGateways = false" in config and "StartingLevel = 1" in config
    with original.open("rb") as stream:
        original_hash = hashlib.file_digest(stream, "sha256").hexdigest()
    report = {"UnchangedAssetsVerified": verified, "ModifiedAssetsCrcVerified": len(changed),
              "TravelAndSpawnAssetsUnchanged": True, "NativeMenuControlsVerified": True,
              "MenuChoicesVerified": 12,
              "OriginalRootSha256": original_hash, "ModRootSha256": manifest["RootSha256"]}
    (GENERATED / "verification.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()

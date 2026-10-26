import json
import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"
SPIRALDB = Path("/home/hypercolossus/Revive101/Imlight/spiraldb-cache")


class DormitoryTests(unittest.TestCase):
    def test_zone_service_dorm_route(self):
        """Verify MSG_GOTODORM routes to WC_Housing_Dorm_Interior privately."""
        zs = (SOURCE / "Game/Services/ZoneService.cs").read_text(encoding="utf-8")
        self.assertIn('DestinationZone = "WizardCity/Interiors/WC_Housing_Dorm_Interior"', zs)
        self.assertIn('IsPrivate = true', zs)
        self.assertIn('OwnerCharId = wizard.CharId', zs)

    def test_access_pass_allows_spiraldb_zones(self):
        """Verify AccessPassManager allows zones from SpiralDB."""
        ap = (SOURCE / "Game/World/AccessPassManager.cs").read_text(encoding="utf-8")
        self.assertIn("ZoneDataCollection.GetZoneData(zoneName) is not null", ap)

    def test_local_wad_cache_disk_fallback(self):
        """Verify LocalWadCache checks disk when wad is not in LiteDB."""
        lw = (SOURCE / "Shared/Resources/LocalWadCache.cs").read_text(encoding="utf-8")
        self.assertIn("ResolveDiskWadPath", lw)
        self.assertIn("ArchiveParser.Parse(memStream)", lw)

    def test_dorm_spiraldb_data(self):
        """Verify Dormitory zone transfer data connects back to Ravenwood."""
        dorm_json = SPIRALDB / "ZoneTransfer/WizardZoneDatas_993-A.json"
        self.assertTrue(dorm_json.is_file(), "Dorm data file must exist in SpiralDB")
        with open(dorm_json, "r", encoding="utf-8") as f:
            data = json.load(f)
        self.assertEqual(data["ZoneName"], "WizardCity/Interiors/WC_Housing_Dorm_Interior")
        teleports = {t["TriggerName"]: t["Teleport"] for t in data["Teleports"]}
        self.assertIn("FemaleTeleport", teleports)
        self.assertIn("MaleTeleport", teleports)
        self.assertEqual(teleports["FemaleTeleport"]["m_destinationZone"], "WizardCity/WC_Ravenwood")
        self.assertEqual(teleports["MaleTeleport"]["m_destinationZone"], "WizardCity/WC_Ravenwood")


if __name__ == "__main__":
    unittest.main()

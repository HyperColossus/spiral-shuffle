import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class HousingFeaturesTests(unittest.TestCase):
    def test_housing_feature_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingFeatureService)", factory)

    def test_housing_teleporters(self):
        svc = (SOURCE / "Game/Services/HousingFeatureService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_SETHOUSETELEPORTER", svc)
        self.assertIn("MSG_REQUESTTELEPORTERHOUSINGZONE", svc)

    def test_housing_music_and_aquariums(self):
        svc = (SOURCE / "Game/Services/HousingFeatureService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_EQUIPHOUSEMUSIC", svc)
        self.assertIn("MSG_SELECTHOUSEMUSIC", svc)
        self.assertIn("MSG_REMOVEHOUSEMUSIC", svc)
        self.assertIn("MSG_ADDFISHTOAQUARIUM", svc)
        self.assertIn("MSG_REMOVEFISHFROMAQUARIUM", svc)

if __name__ == "__main__":
    unittest.main()

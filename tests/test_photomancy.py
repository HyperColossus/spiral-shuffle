import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class PhotomancyTests(unittest.TestCase):
    def test_photomancy_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PhotomancyService)", factory)

    def test_photomancy_actions_and_filters(self):
        svc = (SOURCE / "Game/Services/PhotomancyService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PHOTOMANCYUSETOME", svc)
        self.assertIn("MSG_PHOTOTAKEN", svc)
        self.assertIn("MSG_UPDATEPHOTOFILTERS", svc)

if __name__ == "__main__":
    unittest.main()

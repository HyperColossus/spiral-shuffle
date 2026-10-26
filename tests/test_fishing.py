import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class FishingTests(unittest.TestCase):
    def test_fishing_service_registered(self):
        """Verify FishingService is registered in GameServiceFactory."""
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(FishingService)", factory)

    def test_begin_and_cast_lure_implemented(self):
        """Verify fishing lure casting consumes energy and shows cast animation."""
        fish = (SOURCE / "Game/Services/FishingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_BEGINFISHINGCAST", fish)
        self.assertIn("MSG_FISHINGSPELLCAST", fish)
        self.assertIn("wizard.UpdateEnergy", fish)
        self.assertIn("MSG_SHOWFISHINGCAST", fish)

    def test_catch_fish_implemented(self):
        """Verify MSG_CATCHFISH awards fish, XP, gold, and sends display messages."""
        fish = (SOURCE / "Game/Services/FishingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CATCHFISH", fish)
        self.assertIn("MSG_DISPLAYCATCHFISH", fish)
        self.assertIn("MSG_DISPLAYCAUGHTFISH", fish)
        self.assertIn("MSG_CATCHSUCCESS", fish)
        self.assertIn("wizard.GameStats.m_fishingXP += 50", fish)
        self.assertIn("MSG_FISHINGLEVELUP", fish)
        self.assertIn("MSG_UPDATEFISHINGXP", fish)
        self.assertIn("MSG_UPDATEGOLD", fish)


if __name__ == "__main__":
    unittest.main()

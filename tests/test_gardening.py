import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class GardeningTests(unittest.TestCase):
    def test_gardening_service_registered(self):
        """Verify GardeningService is registered in GameServiceFactory."""
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(GardeningService)", factory)

    def test_gardening_cast_spell_implemented(self):
        """Verify MSG_GARDENINGCASTSPELL consumes energy and awards XP & levelup."""
        garden = (SOURCE / "Game/Services/GardeningService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_GARDENINGCASTSPELL", garden)
        self.assertIn("wizard.UpdateEnergy", garden)
        self.assertIn("wizard.GameStats.m_gardeningXP += 25", garden)
        self.assertIn("MSG_UPDATEGARDENINGXP", garden)
        self.assertIn("MSG_GARDENLEVELUP", garden)
        self.assertIn("MSG_GARDENINGCOMMANDRESPONSE", garden)

    def test_gardening_harvest_implemented(self):
        """Verify MSG_GARDENINGHARVESTPLANT awards gold, harvest XP and sends response."""
        garden = (SOURCE / "Game/Services/GardeningService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_GARDENINGHARVESTPLANT", garden)
        self.assertIn("wizard.AddGold(150)", garden)
        self.assertIn("wizard.GameStats.m_gardeningXP += 50", garden)
        self.assertIn("MSG_UPDATEGOLD", garden)


if __name__ == "__main__":
    unittest.main()

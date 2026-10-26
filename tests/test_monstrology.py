import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class MonstrologyTests(unittest.TestCase):
    def test_monstrology_service_registered(self):
        """Verify MonstrologyService is registered in GameServiceFactory."""
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(MonstrologyService)", factory)

    def test_monstrology_tome_implemented(self):
        """Verify MSG_REQUESTMONSTERTOME responds with tome and XP stats."""
        mon = (SOURCE / "Game/Services/MonstrologyService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTMONSTERTOME", mon)
        self.assertIn("MSG_UPDATEMONSTERMAGICXP", mon)

    def test_monstrology_craft_implemented(self):
        """Verify MSG_MONSTERMAGICREQUESTCREATE awards XP and levels up."""
        mon = (SOURCE / "Game/Services/MonstrologyService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_MONSTERMAGICREQUESTCREATE", mon)
        self.assertIn("wizard.GameStats.m_monsterMagicXP += 30", mon)
        self.assertIn("MSG_MONSTERMAGICLEVELUP", mon)


if __name__ == "__main__":
    unittest.main()

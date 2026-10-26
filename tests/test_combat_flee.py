import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class CombatFleeTests(unittest.TestCase):
    def test_combat_flee_and_afk_handlers(self):
        svc = (SOURCE / "Game/Services/CombatService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_COMBATFLEE", svc)
        self.assertIn("MSG_COMBATAFK", svc)
        self.assertIn("MSG_BATTLECARDTRASH", svc)
        self.assertIn("MSG_BATTLECARDTRASHCONFIRM", svc)

    def test_combat_flee_teleports_and_sets_health(self):
        svc = (SOURCE / "Game/Services/CombatService.cs").read_text(encoding="utf-8")
        self.assertIn("wizard.IsInDuel = false", svc)
        self.assertIn("wizard.GameStats.m_currentHitpoints = 1", svc)
        self.assertIn("MSG_SENDTOHUB", svc)

if __name__ == "__main__":
    unittest.main()

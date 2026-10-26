import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class ArchmasteryTests(unittest.TestCase):
    def test_deck_archmastery_handled(self):
        spellbook = (SOURCE / "Game/Services/SpellBookService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_SetDeckArchmastery", spellbook)
        self.assertIn("MSG_UPDATEARCHMASTERY", spellbook)

    def test_combat_archmastery_pip_conversion(self):
        circle = (SOURCE / "Game/Combat/CombatDuelSubCircle.cs").read_text(encoding="utf-8")
        self.assertIn("ConvertArchmasteryPip", circle)
        self.assertIn("AdvanceShadowPips", circle)
        self.assertIn("m_archmasterySchool", circle)
        self.assertIn("m_firePips", circle)
        self.assertIn("m_stormPips", circle)

    def test_school_pip_deduction(self):
        circle = (SOURCE / "Game/Combat/CombatDuelSubCircle.cs").read_text(encoding="utf-8")
        self.assertIn("TryDeductSchoolPip", circle)
        self.assertIn("TotalSchoolPips", circle)


if __name__ == "__main__":
    unittest.main()

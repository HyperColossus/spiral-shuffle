import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class SpellementsTests(unittest.TestCase):
    def test_tiered_spell_upgrade_handler_exists(self):
        svc = (SOURCE / "Game/Services/SpellBookService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_TIEREDSPELLUPGRADE", svc)
        self.assertIn("ReceiveTieredSpellUpgrade", svc)

    def test_spellements_upgrade_logic(self):
        svc = (SOURCE / "Game/Services/SpellBookService.cs").read_text(encoding="utf-8")
        self.assertIn("spellbook.RemoveSpellFromBook(message.SpellID)", svc)
        self.assertIn("SpellFactory.GetSpell(message.SpellIDNew)", svc)
        self.assertIn("spellbook.AddSpellToBook(newSpell)", svc)
        self.assertIn("MSG_ADDSPELLTOBOOK", svc)
        self.assertIn("MSG_TIEREDSPELLUPGRADERESP", svc)

if __name__ == "__main__":
    unittest.main()

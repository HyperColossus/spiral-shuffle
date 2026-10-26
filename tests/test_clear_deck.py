import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class ClearDeckTests(unittest.TestCase):
    def test_clear_equipped_deck_handler(self):
        svc = (SOURCE / "Game/Services/SpellBookService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CLEAR_EQUIPPED_DECK", svc)
        self.assertIn("ReceiveClearEquippedDeck", svc)

if __name__ == "__main__":
    unittest.main()

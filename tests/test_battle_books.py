import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class BattleBookTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(BattleBookService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/BattleBookService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_BattleBookAdd", svc)
        self.assertIn("MSG_BattleBookRemove", svc)
        self.assertIn("MSG_BattleBookClear", svc)
        self.assertIn("MSG_BattleBookRating", svc)

if __name__ == "__main__":
    unittest.main()

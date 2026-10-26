import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class SecondChanceChestTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(SecondChanceChestService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/SecondChanceChestService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PAID_LOOT_ROLL_PROMPT", svc)
        self.assertIn("MSG_PAID_LOOT_ROLL_RESPONSE", svc)
        self.assertIn("MSG_PAID_LOOT_ROLL_RESULT", svc)
        self.assertIn("MSG_PAID_LOOT_CROWNS_BALANCE", svc)
        self.assertIn("MSG_PAID_LOOT_ROLL_ERROR", svc)

if __name__ == "__main__":
    unittest.main()

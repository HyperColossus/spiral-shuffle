import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class BadgeTests(unittest.TestCase):
    def test_badge_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(BadgeService)", factory)

    def test_badge_service_handlers(self):
        svc = (SOURCE / "Game/Services/BadgeService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTNEWBADGE", svc)
        self.assertIn("MSG_REQUESTPLAYERBADGE", svc)
        self.assertIn("MSG_SELECT_BADGE", svc)
        self.assertIn("MSG_BADGES", svc)

if __name__ == "__main__":
    unittest.main()

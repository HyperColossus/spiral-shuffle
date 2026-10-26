import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class ScrollOfFortuneTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(ScrollOfFortuneService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/ScrollOfFortuneService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_BUYSEASONPASS", svc)
        self.assertIn("MSG_ADVANCESEASONPASS", svc)
        self.assertIn("MSG_CROWNSREWARDSTIERREACHED", svc)
        self.assertIn("MSG_REMOVECROWNSREWARDSEVENT", svc)

if __name__ == "__main__":
    unittest.main()

import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class HenchmanTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HenchmanService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/HenchmanService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CROWNSHOPOFFER", svc)
        self.assertIn("MSG_LOGCROWNSHOPOFFER", svc)
        self.assertIn("MSG_CLAIMPERIODICOFFERS", svc)
        self.assertIn("MSG_LOGOFFER", svc)

if __name__ == "__main__":
    unittest.main()

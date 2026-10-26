import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class DailyPvPTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(DailyPvPService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/DailyPvPService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_DAILYPVPOPEN", svc)
        self.assertIn("MSG_DailyPvPUpdate", svc)
        self.assertIn("MSG_DAILYPVPCOMPLETED", svc)

if __name__ == "__main__":
    unittest.main()

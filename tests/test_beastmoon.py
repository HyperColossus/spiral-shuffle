import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class BeastmoonTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(BeastmoonService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/BeastmoonService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_BATTLEGROUNDQUEUEPLAYER", svc)
        self.assertIn("MSG_BATTLEGROUNDQUEUEUPDATE", svc)
        self.assertIn("MSG_BATTLEGROUNDREQUESTMATCHDATA", svc)
        self.assertIn("MSG_BATTLEGROUNDTELEPORTPLAYER", svc)
        self.assertIn("MSG_BATTLEGROUNDUPDATEPOINTS", svc)
        self.assertIn("MSG_BATTLEGROUNDPOLYMORPHIMPROVEREQ", svc)
        self.assertIn("MSG_BATTLEGROUNDPOLYMORPHIMPROVERESP", svc)
        self.assertIn("MSG_BATTLEGROUNDEND", svc)
        self.assertIn("MSG_BATTLEGROUNDPENALTY", svc)

if __name__ == "__main__":
    unittest.main()

import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class EmoteTests(unittest.TestCase):
    def test_emote_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(EmoteService)", factory)

    def test_emote_handlers(self):
        svc = (SOURCE / "Game/Services/EmoteService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTPIIRADIALMENUPLAYEMOTE", svc)
        self.assertIn("MSG_PIIRADIALMENUPLAYEMOTE", svc)
        self.assertIn("MSG_CORE_EMOTE", svc)
        self.assertIn("MSG_UPDATECUSTOMEMOTES", svc)

if __name__ == "__main__":
    unittest.main()

import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class PixiePostTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PixiePostService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/PixiePostService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PIXIEPOSTMAIL", svc)
        self.assertIn("MSG_PIXIEPOSTMESSAGE", svc)
        self.assertIn("MSG_REQUESTPIXIEPOSTMESSAGE", svc)
        self.assertIn("MSG_PLAYPIXIEPOSTANIMATION", svc)
        self.assertIn("MSG_PIXIEPOSTGUILDMAIL", svc)

if __name__ == "__main__":
    unittest.main()

import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestInteractiveMusic(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(InteractiveMusicService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/InteractiveMusicService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTINTERACTIVEMUSIC", svc)
        self.assertIn("MSG_PLAYINTERACTIVEMUSIC", svc)
        self.assertIn("MSG_PLAYINTERACTIVEMUSIC2", svc)
        self.assertIn("MSG_PLAYINTERACTIVEMUSICLOOP", svc)
        self.assertIn("MSG_STOPINTERACTIVEMUSICLOOP", svc)
        self.assertIn("MSG_SETMUSICLOOP", svc)
        self.assertIn("MSG_DELETEMUSICLOOP", svc)
        self.assertIn("MSG_SELECTMUSICSTYLE", svc)

if __name__ == "__main__":
    unittest.main()

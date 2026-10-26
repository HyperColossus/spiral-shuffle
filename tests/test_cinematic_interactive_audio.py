import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCinematicInteractiveAudio(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CinematicAndInteractiveAudioService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CinematicAndInteractiveAudioService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PLAYCINEMATIC", svc)
        self.assertIn("MSG_PLAYGRAPHIC", svc)
        self.assertIn("MSG_PLAYMUSIC", svc)
        self.assertIn("MSG_PLAYINTERACTIVEMUSICLOOPING", svc)
        self.assertIn("MSG_PLAYINTERACTIVEMUSICLOOPING2", svc)

if __name__ == "__main__":
    unittest.main()

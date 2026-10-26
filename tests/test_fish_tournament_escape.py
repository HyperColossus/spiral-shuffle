import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestFishTournamentEscape(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(FishTournamentEscapeService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/FishTournamentEscapeService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_FISHESCAPED", svc)
        self.assertIn("MSG_FISHINGCSRRESULTS", svc)
        self.assertIn("MSG_FISHTOURNAMENTOPEN", svc)
        self.assertIn("MSG_EXITBENEFICIALOBJECT", svc)

if __name__ == "__main__":
    unittest.main()

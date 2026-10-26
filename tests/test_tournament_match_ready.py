import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestTournamentMatchReady(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(TournamentMatchReadyService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/TournamentMatchReadyService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_MATCHINVITE", svc)
        self.assertIn("MSG_MATCHMAKERUPDATE", svc)
        self.assertIn("MSG_MATCHREADY", svc)
        self.assertIn("MSG_MATCHRESULT", svc)
        self.assertIn("MSG_NOT_AFK", svc)

if __name__ == "__main__":
    unittest.main()

import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestLadderRankingsSession(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(LadderRankingsAndSessionService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/LadderRankingsAndSessionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_GETLADDER", svc)
        self.assertIn("MSG_GET_RANKINGS", svc)
        self.assertIn("MSG_FIND_SESSIONS", svc)
        self.assertIn("MSG_FIND_SESSION_PLAYERS", svc)
        self.assertIn("MSG_GUILDUNLOAD", svc)
        self.assertIn("MSG_GENERICGAME", svc)

if __name__ == "__main__":
    unittest.main()

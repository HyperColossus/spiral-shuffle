import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestLeaderboardTournamentLadder(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(LeaderboardAndTournamentLadderService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/LeaderboardAndTournamentLadderService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_LEADERBOARDFRIENDREQUEST", svc)
        self.assertIn("MSG_LEADERBOARDREQUEST", svc)
        self.assertIn("MSG_LEADERBOARDRESPONSE", svc)
        self.assertIn("MSG_LADDER", svc)
        self.assertIn("MSG_MATCHAWARD", svc)

if __name__ == "__main__":
    unittest.main()

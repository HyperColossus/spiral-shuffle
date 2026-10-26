import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class CastleGamesTests(unittest.TestCase):
    def test_castle_game_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CastleGameService)", factory)

    def test_castle_games_handlers(self):
        svc = (SOURCE / "Game/Services/CastleGameService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLEGAMESREQUEST", svc)
        self.assertIn("MSG_CASTLEGAMESREQUESTGAMEDATA", svc)
        self.assertIn("MSG_STARTHOUSINGGAME", svc)
        self.assertIn("MSG_HOUSINGGAMEUSEPOWERUP", svc)

if __name__ == "__main__":
    unittest.main()

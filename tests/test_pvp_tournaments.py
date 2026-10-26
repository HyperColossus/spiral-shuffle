import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class PvPTournamentTests(unittest.TestCase):
    def test_pvp_tournament_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PvPTournamentService)", factory)

    def test_tournaments_and_elo(self):
        svc = (SOURCE / "Game/Services/PvPTournamentService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PVPCONFIRMTOURNEY", svc)
        self.assertIn("MSG_PVP5THAGEKIOSKREQUEST", svc)
        self.assertIn("MSG_AdvPvPEloData", svc)
        self.assertIn("MSG_PvPStatueInspect", svc)

if __name__ == "__main__":
    unittest.main()

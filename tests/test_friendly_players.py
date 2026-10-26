import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class FriendlyPlayersTests(unittest.TestCase):
    def test_friendly_player_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(FriendlyPlayerService)", factory)

    def test_friendly_player_volunteering(self):
        svc = (SOURCE / "Game/Services/FriendlyPlayerService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTFRIENDLYPLAYERS", svc)
        self.assertIn("MSG_SETVOLUNTEERFLAG", svc)
        self.assertIn("MSG_REQUESTVOLUNTEERINFO", svc)
        self.assertIn("MSG_UPDATEVOLUNTEERINFO", svc)

    def test_volunteer_team_help(self):
        svc = (SOURCE / "Game/Services/FriendlyPlayerService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_VOLUNTEERREQUEST", svc)
        self.assertIn("MSG_VOLUNTEERTEAMHELPJOIN", svc)
        self.assertIn("MSG_VOLUNTEERTEAMHELPCOMPLETE", svc)

if __name__ == "__main__":
    unittest.main()

import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class TeamUpTests(unittest.TestCase):
    def test_team_up_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(TeamUpService)", factory)

    def test_team_up_kiosk_and_queues(self):
        svc = (SOURCE / "Game/Services/TeamUpService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTTEAMUPINFO", svc)
        self.assertIn("MSG_REQUESTTEAMUP", svc)
        self.assertIn("MSG_TEAMUPREMOVEPLAYER", svc)
        self.assertIn("MSG_UPDATETEAMUPREADY", svc)

    def test_team_up_client_events(self):
        svc = (SOURCE / "Game/Services/TeamUpService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_TEAMUPADDPLAYERTOCLIENT", svc)
        self.assertIn("MSG_TEAMUPREMOVEPLAYERFROMCLIENT", svc)
        self.assertIn("MSG_TEAMUPTELEPORTPLAYER", svc)

if __name__ == "__main__":
    unittest.main()

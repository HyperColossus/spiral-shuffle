import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestTeamUpKiosk(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(TeamUpService)", factory)

    def test_kiosk_protocol_handlers(self):
        svc = (SOURCE / "Game/Services/TeamUpService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTTEAMHELPINFO", svc)
        self.assertIn("MSG_REQUESTTEAMHELPJOIN", svc)
        self.assertIn("MSG_REQUESTTEAMHELPWORLDINFO", svc)
        self.assertIn("MSG_CLIENTREQUESTTEAMHELPWORLDINFO", svc)
        self.assertIn("MSG_TEAMUPADDPLAYER", svc)
        self.assertIn("MSG_REQUESTTEAMHELPFAIL", svc)

if __name__ == "__main__":
    unittest.main()

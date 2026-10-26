import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestFriendlyPlayerQuestHelp(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(FriendlyPlayerQuestHelpService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/FriendlyPlayerQuestHelpService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTACTIVEMAPQUESTS", svc)
        self.assertIn("MSG_REQUESTFRIENDLYPLAYERQUEST", svc)
        self.assertIn("MSG_REQUESTFRIENDLYPLAYERQUEST2", svc)
        self.assertIn("MSG_REMOVEGOAL", svc)
        self.assertIn("MSG_REINTERACT", svc)
        self.assertIn("MSG_REQUESTTEAMHELPJOINFAIL", svc)

if __name__ == "__main__":
    unittest.main()

import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestAdventurePartyBulletin(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(AdventurePartyBulletinService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/AdventurePartyBulletinService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CANACQUIREGROUPELIXIR", svc)
        self.assertIn("MSG_SHOWCLIENTGROUPELIXIRPOPUP", svc)
        self.assertIn("MSG_POSTPARTYMESSAGE", svc)
        self.assertIn("MSG_REMOVEPARTYMESSAGE", svc)
        self.assertIn("MSG_CHECKPARTYNAME", svc)
        self.assertIn("MSG_CANCELEDPARTYMESSAGE", svc)
        self.assertIn("MSG_CHANGERESPONSEPARTYMESSAGE", svc)
        self.assertIn("MSG_PARTYSUBMITMEMBERZONES", svc)
        self.assertIn("MSG_PARTYZONEREQUESTRESPONSE", svc)

if __name__ == "__main__":
    unittest.main()

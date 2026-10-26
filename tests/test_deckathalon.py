import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestDeckathalon(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(DeckathalonService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/DeckathalonService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTCLASSPROJECTINFO", svc)
        self.assertIn("MSG_CLASSPROJECTUSETOKEN", svc)
        self.assertIn("MSG_CLASSPROJECTUSECROWNS", svc)
        self.assertIn("MSG_CLASSPROJECTREQUESTTELEPORT", svc)
        self.assertIn("MSG_CLASSPROJECTCLAIMREWARD", svc)
        self.assertIn("MSG_CLASSPROJECTREQUESTLEADERBOARD", svc)
        self.assertIn("MSG_CLASSPROJECTSREQUESTFRIENDS", svc)
        self.assertIn("MSG_UPDATECLASSPROJECTBUTTON", svc)
        self.assertIn("MSG_UPDATECLASSPROJECTSTATUS", svc)

if __name__ == "__main__":
    unittest.main()

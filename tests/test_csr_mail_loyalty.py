import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCSRMailLoyalty(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CSRMailAndLoyaltyDataService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CSRMailAndLoyaltyDataService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CSR_MAIL_DATA", svc)
        self.assertIn("MSG_CSR_RECEIVE_GIFTS", svc)
        self.assertIn("MSG_CSRDELETEGIFT", svc)
        self.assertIn("MSG_CSRSETFRIENDSTATUS", svc)
        self.assertIn("MSG_CSRLOYALTYPAGEDATA", svc)
        self.assertIn("MSG_CLASSPROJECTSCSRRESULTS", svc)
        self.assertIn("MSG_CLASSPROJECTSTARTTIME", svc)

if __name__ == "__main__":
    unittest.main()

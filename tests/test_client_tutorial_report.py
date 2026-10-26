import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestClientTutorialReport(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(ClientTutorialAndReportService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/ClientTutorialAndReportService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CLIENTTUTORIALEVENT", svc)
        self.assertIn("MSG_TUTORIALEVENT", svc)
        self.assertIn("MSG_DISMISSTUTORIALTIP", svc)
        self.assertIn("MSG_READTUTORIALTIP", svc)
        self.assertIn("MSG_REQUESTTUTORIALTIPLOG", svc)
        self.assertIn("MSG_UPDATETUTORIALTIPLOG", svc)
        self.assertIn("MSG_SERVERTUTORIALCOMMAND", svc)
        self.assertIn("MSG_TUTORIALS", svc)
        self.assertIn("MSG_BUGSUBMITRESPONSE", svc)
        self.assertIn("MSG_FORCE_DISCONNECT", svc)
        self.assertIn("MSG_CLIENT_ERROR", svc)

if __name__ == "__main__":
    unittest.main()

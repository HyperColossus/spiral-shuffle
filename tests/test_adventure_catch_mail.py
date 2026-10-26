import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestAdventureCatchMail(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(AdventureCatchAndMailService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/AdventureCatchAndMailService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTADVENTUREPARTY", svc)
        self.assertIn("MSG_REQUESTCATCHSUCCESS", svc)
        self.assertIn("MSG_REQUESTCHATSTATS", svc)
        self.assertIn("MSG_REQUESTDELETEMESSAGE", svc)
        self.assertIn("MSG_REPORTADTAKEOVER", svc)
        self.assertIn("MSG_REGISTRAR", svc)

if __name__ == "__main__":
    unittest.main()

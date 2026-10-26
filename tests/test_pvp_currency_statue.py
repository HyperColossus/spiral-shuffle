import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPvPCurrencyStatue(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PvPCurrencyAndStatueService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PvPCurrencyAndStatueService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_UPDATEPVPCURRENCY", svc)
        self.assertIn("MSG_UPDATEPVPTOURNEYCURRENCY", svc)
        self.assertIn("MSG_PvPStatueData", svc)
        self.assertIn("MSG_PVPNOMATCHIGNORELIST", svc)
        self.assertIn("MSG_PVPBLOCKED", svc)
        self.assertIn("MSG_PVPDISABLED", svc)
        self.assertIn("MSG_RESPONSESHOWPVPOPTION", svc)
        self.assertIn("MSG_PVPBLOCK", svc)
        self.assertIn("MSG_PVPREGISTERFAILED", svc)

if __name__ == "__main__":
    unittest.main()

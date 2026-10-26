import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestPvPKiosk(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PvPKioskService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/PvPKioskService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTPVPKIOSK", svc)
        self.assertIn("MSG_PVPMATCHREQUEST", svc)
        self.assertIn("MSG_PVPUPDATEREQUEST", svc)
        self.assertIn("MSG_REQUESTSHOWPVPOPTION", svc)
        self.assertIn("MSG_UPDATESHOWPVPOPTION", svc)

if __name__ == "__main__":
    unittest.main()

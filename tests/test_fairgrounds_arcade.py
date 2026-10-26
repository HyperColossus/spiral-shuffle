import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestFairgroundsArcade(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(FairgroundsArcadeService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/FairgroundsArcadeService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ENTERMINIGAME", svc)
        self.assertIn("MSG_LEAVEMINIGAME", svc)
        self.assertIn("MSG_REQUESTMINIGAME", svc)
        self.assertIn("MSG_SOBLOCKS_SETLEVEL", svc)
        self.assertIn("MSG_SOBLOCKS_TIMEDDROP", svc)
        self.assertIn("MSG_SOBLOCKS_ROCKDROP", svc)

if __name__ == "__main__":
    unittest.main()

import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHatchmakingService(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HatchmakingService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HatchmakingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTHATCHMAKING", svc)
        self.assertIn("MSG_REQUESTHATCHMAKINGPETS", svc)
        self.assertIn("MSG_REQUESTHATCHMAKINGSEARCHPETS", svc)
        self.assertIn("MSG_REQUESTHATCHMAKINGADDPET", svc)
        self.assertIn("MSG_REQUESTHATCHMAKINGREMOVEPET", svc)
        self.assertIn("MSG_REQUESTHATCHMAKINGHATCH2", svc)
        self.assertIn("MSG_HATCHMAKINGUSETOME", svc)
        self.assertIn("MSG_HATCHMAKINGSUPDATEHATCHDAYPET", svc)
        self.assertIn("MSG_REQUESTDONATEDPETLIST", svc)

if __name__ == "__main__":
    unittest.main()

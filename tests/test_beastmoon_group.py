import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestBeastmoonGroup(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(BeastmoonGroupMatchmakingService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/BeastmoonGroupMatchmakingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_BATTLEGROUNDQUEUEGROUP", svc)
        self.assertIn("MSG_BATTLEGROUNDQUEUEGROUPCHECK", svc)
        self.assertIn("MSG_BATTLEGROUNDQUEUEGROUPJOINED", svc)
        self.assertIn("MSG_BATTLEGROUNDQUEUEGROUPSTATUS", svc)
        self.assertIn("MSG_BATTLEGROUNDQUEUESTATS", svc)
        self.assertIn("MSG_BATTLEGROUNDPOLYMORPHIMPROVEMENT", svc)

if __name__ == "__main__":
    unittest.main()

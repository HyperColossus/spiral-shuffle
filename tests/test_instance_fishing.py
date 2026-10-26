import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestInstanceFishing(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(InstanceFishingService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/InstanceFishingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_SETINSTANCEFISH", svc)
        self.assertIn("MSG_ADDINSTANCEFISH", svc)
        self.assertIn("MSG_SETFISHINGPLAYERS", svc)
        self.assertIn("MSG_DELETEFISH", svc)
        self.assertIn("MSG_CSRREQUESTFISHLIST", svc)

if __name__ == "__main__":
    unittest.main()

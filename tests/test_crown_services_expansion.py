import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCrownServicesExpansion(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CrownServicesAndExpansionService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CrownServicesAndExpansionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_BUYENERGYCONFIRM", svc)
        self.assertIn("MSG_BUYISLANDSPACECONFIRM", svc)
        self.assertIn("MSG_CROWNSBUYCONFIRM", svc)
        self.assertIn("MSG_CROWNSERVICESOPEN", svc)
        self.assertIn("MSG_CSRCROWNBALANCE", svc)
        self.assertIn("MSG_ADCLICKTHROUGH", svc)

if __name__ == "__main__":
    unittest.main()

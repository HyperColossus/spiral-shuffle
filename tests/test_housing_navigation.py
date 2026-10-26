import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHousingNavigation(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingNavigationService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HousingNavigationService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CHANGEBREADCRUMBREQUEST", svc)
        self.assertIn("MSG_PATCHHOUSINGBREADCRUMB", svc)
        self.assertIn("MSG_REQUESTISLANDSWITCH", svc)
        self.assertIn("MSG_REQUESTDEEDZONE", svc)

if __name__ == "__main__":
    unittest.main()

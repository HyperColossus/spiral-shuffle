import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHousingCustomization(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingCustomizationService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HousingCustomizationService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PATCHTEXTUREREMAP", svc)
        self.assertIn("MSG_PATCHUPDATEHOUSINGOBJECT", svc)
        self.assertIn("MSG_PATCHHOUSE", svc)
        self.assertIn("MSG_AUDITATTICRESULTS", svc)
        self.assertIn("MSG_SETATTICID", svc)
        self.assertIn("MSG_PATCHTINTHOUSINGOBJECT", svc)
        self.assertIn("MSG_PATCHUPEXTENDEDTILE", svc)

if __name__ == "__main__":
    unittest.main()

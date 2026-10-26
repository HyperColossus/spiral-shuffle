import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHousingVaultsInteractive(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingInteractiveVaultService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HousingInteractiveVaultService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_HOUSINGOBJECTPROXIMITY", svc)
        self.assertIn("MSG_HOUSINGOBJECTNOPICKUP", svc)
        self.assertIn("MSG_HIDEHOUSINGOBJECT", svc)
        self.assertIn("MSG_LEAVEHOUSINGLOT", svc)
        self.assertIn("MSG_PATCHHOUSINGGARDENINGSHED", svc)
        self.assertIn("MSG_PATCHHOUSINGGEARVAULT", svc)
        self.assertIn("MSG_PATCHHOUSINGJEWELVAULT", svc)
        self.assertIn("MSG_PATCHEQUIPHOUSINGOBJECT", svc)

if __name__ == "__main__":
    unittest.main()

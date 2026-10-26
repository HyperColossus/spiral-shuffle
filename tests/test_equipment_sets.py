import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class EquipmentSetsTests(unittest.TestCase):
    def test_equipment_set_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(EquipmentSetService)", factory)

    def test_equipment_set_handlers(self):
        svc = (SOURCE / "Game/Services/EquipmentSetService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CREATENEWEQUIPMENTSET", svc)
        self.assertIn("MSG_EQUIPSET", svc)
        self.assertIn("MSG_DELETEEQUIPMENTSET", svc)
        self.assertIn("MSG_MOVEEQUIPMENTSET", svc)
        self.assertIn("MSG_EQUIPMENTSETCREATED", svc)

if __name__ == "__main__":
    unittest.main()

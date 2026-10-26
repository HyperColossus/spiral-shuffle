import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestJewelSocketEquipment(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(JewelSocketAndEquipmentService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/JewelSocketAndEquipmentService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_EQUIPNEWJEWELTOITEM", svc)
        self.assertIn("MSG_EQUIPMENTBEHAVIOR_UNEQUIPITEM", svc)
        self.assertIn("MSG_GETITEMINFO", svc)
        self.assertIn("MSG_GETSUBSCRIBERONLYITEMS", svc)
        self.assertIn("MSG_HOLIDAYDATA", svc)
        self.assertIn("MSG_GETNEXTWORLD", svc)

if __name__ == "__main__":
    unittest.main()

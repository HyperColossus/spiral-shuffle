import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class HousingDecoratingTests(unittest.TestCase):
    def test_housing_service_registered(self):
        """Verify HousingService is registered in GameServiceFactory."""
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingService)", factory)

    def test_place_housing_object_implemented(self):
        """Verify MSG_PLACEHOUSINGOBJECT removes item from inventory and updates housing zone."""
        housing = (SOURCE / "Game/Services/HousingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PLACEHOUSINGOBJECT", housing)
        self.assertIn("wizard.RemoveItemFromInventory(message.ObjectID)", housing)
        self.assertIn("MSG_INVENTORYBEHAVIOR_REMOVEITEM", housing)
        self.assertIn("MSG_UPDATEHOUSINGOBJECT", housing)
        self.assertIn("MSG_PATCHADDHOUSINGOBJECT", housing)

    def test_update_housing_object_implemented(self):
        """Verify MSG_UPDATEHOUSINGOBJECT updates position, yaw and scale."""
        housing = (SOURCE / "Game/Services/HousingService.cs").read_text(encoding="utf-8")
        self.assertIn("ReceiveUpdateHousingObject", housing)
        self.assertIn("Scale = message.Scale", housing)

    def test_delete_housing_object_pickup_implemented(self):
        """Verify MSG_PATCHDELETEHOUSINGOBJECT returns item back to inventory."""
        housing = (SOURCE / "Game/Services/HousingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PATCHDELETEHOUSINGOBJECT", housing)
        self.assertIn("wizard.AddItemToInventory", housing)
        self.assertIn("MSG_INVENTORYBEHAVIOR_ADDITEM", housing)

    def test_attic_management_implemented(self):
        """Verify attic request, move to attic, and move from attic are handled."""
        housing = (SOURCE / "Game/Services/HousingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTATTIC", housing)
        self.assertIn("MSG_MOVETOATTIC", housing)
        self.assertIn("MSG_MOVEFROMATTIC", housing)
        self.assertIn("MSG_UPDATEATTICCOUNT", housing)


if __name__ == "__main__":
    unittest.main()

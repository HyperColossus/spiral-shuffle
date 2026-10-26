import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class JewelSocketingTests(unittest.TestCase):
    def test_equip_jewel_implemented(self):
        """Verify ReceiveEquipJewelRequest consumes jewel and attaches socket behavior."""
        inv = (SOURCE / "Game/Services/InventoryService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_EQUIPJEWELREQUEST", inv)
        self.assertIn("MSG_EQUIPJEWELTOITEM", inv)
        self.assertIn("ClientJewelSocketBehavior", inv)
        self.assertIn("PublicSocketJewelPair", inv)
        self.assertIn("wizard.RemoveItemFromInventory(message.JewelGID)", inv)
        self.assertIn("WizardItemCollection.UpdateItem(targetItem)", inv)

    def test_unsocket_jewel_implemented(self):
        """Verify ReceiveUnsocketJewelRequest shatters jewel and clears socket."""
        inv = (SOURCE / "Game/Services/InventoryService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_UNSOCKETJEWELREQUEST", inv)
        self.assertIn("MSG_UNEQUIPSOCKETEDJEWEL", inv)
        self.assertIn("SocketDeleted = 1", inv)

    def test_unlock_sockets_implemented(self):
        """Verify ReceiveUnlockSockets handles socket unlocking."""
        inv = (SOURCE / "Game/Services/InventoryService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_UNLOCKSOCKETS", inv)
        self.assertIn("MSG_UNLOCKSOCKETSCONFIRM", inv)
        self.assertIn("MSG_JEWELSOCKETLOCKUPDATE", inv)
        self.assertIn("m_bSocketsLocked = false", inv)

    def test_wizard_item_collection_update_item(self):
        """Verify WizardItemCollection has UpdateItem helper."""
        coll = (SOURCE / "WizardData/Collections/WizardItemCollection.cs").read_text(encoding="utf-8")
        self.assertIn("public static bool UpdateItem(WizClientObjectItem item)", coll)


if __name__ == "__main__":
    unittest.main()

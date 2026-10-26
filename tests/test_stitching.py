import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class StitchingTests(unittest.TestCase):
    def test_seamstress_component_implemented(self):
        """Verify InteractSeamstressComponent attaches to Eloise/Seamstresses and gives options."""
        comp = (SOURCE / "Game/Zone/Components/InteractSeamstressComponent.cs").read_text(encoding="utf-8")
        self.assertIn("InteractSeamstressComponent", comp)
        self.assertIn("SeamstressOption", comp)
        self.assertIn("UnstitchOption", comp)
        self.assertIn("MSG_SEAMSTRESSOPEN", comp)
        self.assertIn("MSG_UNSTITCHOPEN", comp)

    def test_stitch_items_handler_implemented(self):
        """Verify ReceiveStitchItems transfers appearance and removes display item."""
        shop = (SOURCE / "Game/Services/ShopService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_STITCHITEMS", shop)
        self.assertIn("wizard.Account.SetCrowns", shop)
        self.assertIn("statItem.m_displayID =", shop)
        self.assertIn("wizard.RemoveItemFromInventory(displayItem.m_globalID)", shop)
        self.assertIn("WizardItemCollection.UpdateItem(statItem)", shop)
        self.assertIn("MSG_STITCHITEMSCONFIRM", shop)

    def test_unstitch_items_handler_implemented(self):
        """Verify ReceiveUnstitchItems clears displayID and restores item."""
        shop = (SOURCE / "Game/Services/ShopService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_UNSTITCHITEMS", shop)
        self.assertIn("item.m_displayID = 0", shop)
        self.assertIn("wizard.AddItemToInventory(restoredItem)", shop)
        self.assertIn("MSG_STITCHITEMSCONFIRM", shop)


if __name__ == "__main__":
    unittest.main()

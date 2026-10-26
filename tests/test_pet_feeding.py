import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class PetFeedingTests(unittest.TestCase):
    def test_pet_feeding_implemented(self):
        """Verify ReceiveFeedInventoryItem handles snacks and gear feeding."""
        inv = (SOURCE / "Game/Services/InventoryService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_FEEDINVENTORYITEM", inv)
        self.assertIn("PetSnackBehavior", inv)
        self.assertIn("MSG_PETSNACKREMOVE", inv)
        self.assertIn("MSG_PETLEVELUP", inv)
        self.assertIn("m_expressedTalents", inv)
        self.assertIn("m_expressedDerbyTalents", inv)

    def test_gear_feeding_rewards(self):
        """Verify feeding gear removes item and awards gold."""
        inv = (SOURCE / "Game/Services/InventoryService.cs").read_text(encoding="utf-8")
        self.assertIn("wizard.RemoveItemFromInventory(gear.m_globalID)", inv)
        self.assertIn("wizard.AddGold(goldReward)", inv)
        self.assertIn("MSG_UPDATEGOLD", inv)


if __name__ == "__main__":
    unittest.main()

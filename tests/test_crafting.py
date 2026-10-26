import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class CraftingTests(unittest.TestCase):
    def test_crafting_service_registered(self):
        """Verify CraftingService is registered in GameServiceFactory."""
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CraftingService)", factory)

    def test_use_recipe_implemented(self):
        """Verify ReceiveUseRecipe checks ingredients, deducts reagents, awards item and sets slot."""
        craft = (SOURCE / "Game/Services/CraftingService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_USERECIPE", craft)
        self.assertIn("m_ingredients", craft)
        self.assertIn("wizard.AlchemyBehavior.RemoveReagent", craft)
        self.assertIn("MSG_REAGENTREMOVE", craft)
        self.assertIn("wizard.AddItemToInventory(craftedItem)", craft)
        self.assertIn("MSG_INVENTORYBEHAVIOR_ADDITEM", craft)
        self.assertIn("MSG_CRAFTINGSLOTADD", craft)
        self.assertIn("MSG_CLEARALLCRAFTINGSLOTS", craft)


if __name__ == "__main__":
    unittest.main()

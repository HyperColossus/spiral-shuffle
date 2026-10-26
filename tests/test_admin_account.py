import json
import unittest
import urllib.request


class AdminAccountTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        url = "http://127.0.0.1:18080/databases/Playerdata/docs?id=accounts/66-A"
        req = urllib.request.Request(url)
        with urllib.request.urlopen(req) as resp:
            cls.account = json.loads(resp.read().decode('utf-8'))['Results'][0]

        url = "http://127.0.0.1:18080/databases/Playerdata/docs?id=wizards/66-A"
        req = urllib.request.Request(url)
        with urllib.request.urlopen(req) as resp:
            cls.wizard = json.loads(resp.read().decode('utf-8'))['Results'][0]

    def test_account_admin_privileges(self):
        """Verify account test123 has Administrator AuthLevel and high crowns."""
        self.assertEqual(self.account.get("AuthLevel"), "Administrator")
        self.assertGreaterEqual(self.account.get("Crowns", 0), 999999)

    def test_wizard_max_stats(self):
        """Verify character is level 170 with max gold, health, mana, energy."""
        magic = self.wizard.get("MagicSchoolBehavior", {})
        self.assertEqual(magic.get("Level"), 170)
        self.assertGreaterEqual(magic.get("TrainingPoints", 0), 100)

        stats = self.wizard.get("GameStats", {})
        self.assertGreaterEqual(stats.get("m_currentGold", 0), 1000000)
        self.assertGreaterEqual(stats.get("m_currentHitpoints", 0), 12500)
        self.assertGreaterEqual(stats.get("m_currentMana", 0), 950)
        self.assertTrue(stats.get("m_shadowMagicUnlocked", False))
        self.assertGreaterEqual(stats.get("m_gardeningLevel", 0), 20)
        self.assertGreaterEqual(stats.get("m_fishingLevel", 0), 20)

    def test_wizard_spells_and_cards(self):
        """Verify character has high-level spells and treasure cards loaded."""
        spellbook = self.wizard.get("SpellbookBehavior", {})
        learned = spellbook.get("LearnedSpellTemplateIds", [])
        self.assertGreaterEqual(len(learned), 100)
        tcs = spellbook.get("TreasureCardTemplateIds", [])
        self.assertGreaterEqual(len(tcs), 50)

    def test_wizard_snacks_and_reagents(self):
        """Verify snacks and reagents bags are heavily stocked."""
        snacks = self.wizard.get("PetSnackBehavior", {}).get("SnackItemIds", [])
        self.assertGreaterEqual(len(snacks), 5)
        reagents = self.wizard.get("AlchemyBehavior", {}).get("ReagentItemIds", [])
        self.assertGreaterEqual(len(reagents), 10)

    def test_wizard_gear_inventory(self):
        """Verify character has top gear, pets, and mounts."""
        items = self.wizard.get("InventoryBehavior", {}).get("InventoryItemIds", [])
        self.assertGreaterEqual(len(items), 10)
        pet = self.wizard.get("PetOwnerBehavior", {})
        self.assertEqual(pet.get("EquippedPetTemplateId"), 100334)


if __name__ == "__main__":
    unittest.main()

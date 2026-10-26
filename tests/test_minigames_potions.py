import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class MinigamesAndPotionsTests(unittest.TestCase):
    def test_minigame_rewards_implemented(self):
        """Verify MSG_MINIGAMEREWARDS refills potions and awards gold."""
        mg = (SOURCE / "Game/Services/MinigameService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_MINIGAMEREWARDS", mg)
        self.assertIn("wizard.GameStats.m_potionCharge = wizard.GameStats.m_potionMax", mg)
        self.assertIn("MSG_UPDATEPOTIONS", mg)
        self.assertIn("wizard.AddGold(goldReward)", mg)
        self.assertIn("MSG_UPDATEGOLD", mg)

    def test_use_potion_implemented(self):
        """Verify MSG_USEPOTION restores health and mana, consumes charge."""
        mg = (SOURCE / "Game/Services/MinigameService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_USEPOTION", mg)
        self.assertIn("wizard.GameStats.m_currentHitpoints = wizard.GameStats.m_baseHitpoints", mg)
        self.assertIn("wizard.GameStats.m_currentMana = wizard.GameStats.m_baseMana", mg)
        self.assertIn("MSG_UPDATEHEALTH", mg)
        self.assertIn("MSG_UPDATEMANA", mg)

    def test_potion_buy_implemented(self):
        """Verify MSG_POTIONBUYREQUEST buys potion charges with gold."""
        mg = (SOURCE / "Game/Services/MinigameService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_POTIONBUYREQUEST", mg)
        self.assertIn("wizard.AddGold(-potionCost)", mg)
        self.assertIn("MSG_POTIONBUYCONFIRM", mg)

    def test_pet_games_implemented(self):
        """Verify Pet Pavilion minigames consume energy and send snack feeding success."""
        pet = (SOURCE / "Game/Services/PetService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PETGAMEEND", pet)
        self.assertIn("MSG_PETGAMESKIP", pet)
        self.assertIn("MSG_PETGAMESNACKFEEDSUCCESS", pet)


if __name__ == "__main__":
    unittest.main()

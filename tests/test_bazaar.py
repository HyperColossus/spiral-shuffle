import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class BazaarTests(unittest.TestCase):
    def test_bazaar_commands_implemented(self):
        """Verify AuctionHouseService handles Command 4 (sell quote), 5 (search), 6 (bulk sell), 9 (close)."""
        ah = (SOURCE / "Game/Services/AuctionHouseService.cs").read_text(encoding="utf-8")
        self.assertIn("case 4:", ah)
        self.assertIn("ConfirmSellToAuctionHouse(message.itemGlobalID, message.key);", ah)
        self.assertIn("case 5:", ah)
        self.assertIn("SendAuctionHouseContents(message.npcGlobalID, message.category, message.key);", ah)
        self.assertIn("case 6:", ah)
        self.assertIn("SellToAuctionHouse(message.itemGlobalID, message.key);", ah)
        self.assertIn("case 9:", ah)

    def test_bazaar_initial_stock_seeder(self):
        """Verify AuctionHouseCollection seeds starter stock when empty."""
        ah_coll = (SOURCE / "WizardData/Collections/AuctionHouseCollection.cs").read_text(encoding="utf-8")
        self.assertIn("starterTemplates", ah_coll)
        self.assertIn("session.Store(entry)", ah_coll)


if __name__ == "__main__":
    unittest.main()

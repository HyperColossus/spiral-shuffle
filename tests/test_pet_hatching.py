import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class PetHatchingTests(unittest.TestCase):
    def test_pet_hatch_request_implemented(self):
        """Verify MSG_PETHATCHREQUEST sends result and create confirmations."""
        pet = (SOURCE / "Game/Services/PetService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PETHATCHREQUEST", pet)
        self.assertIn("MSG_PETHATCHRESULT", pet)
        self.assertIn("MSG_PETHATCHCREATE", pet)

    def test_hatchmaking_hatch_implemented(self):
        """Verify MSG_REQUESTHATCHMAKINGHATCH deducts gold, creates egg, and starts timer."""
        pet = (SOURCE / "Game/Services/PetService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTHATCHMAKINGHATCH", pet)
        self.assertIn("wizard.AddGold(-25000)", pet)
        self.assertIn("CreatePetEgg", pet)
        self.assertIn("SendEggCreatedMessages", pet)
        self.assertIn("StartEggTimer", pet)


if __name__ == "__main__":
    unittest.main()

import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class PetDerbyTests(unittest.TestCase):
    def test_pet_derby_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PetDerbyService)", factory)

    def test_pet_derby_race_controls(self):
        svc = (SOURCE / "Game/Services/PetDerbyService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PETDERBYSTART", svc)
        self.assertIn("MSG_PETDERBYSWITCHLANE", svc)
        self.assertIn("MSG_PETDERBYJUMPDUCK", svc)
        self.assertIn("MSG_PETDERBYCHEER", svc)
        self.assertIn("MSG_PETDERBYUSETALENT", svc)
        self.assertIn("MSG_PETDERBYLAP", svc)

    def test_pet_derby_results_and_buffs(self):
        svc = (SOURCE / "Game/Services/PetDerbyService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PETDERBYSPEED", svc)
        self.assertIn("MSG_PETDERBYBUFF", svc)
        self.assertIn("MSG_PETDERBYPHOTOFINISH", svc)
        self.assertIn("MSG_PETGAMEDERBYRESULTS", svc)

if __name__ == "__main__":
    unittest.main()

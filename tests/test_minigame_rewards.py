import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class MinigameRewardTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(MinigameRewardService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/MinigameRewardService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_DOODLEDOUG_REWARDS", svc)
        self.assertIn("MSG_SKULLRIDERS_REWARDS", svc)
        self.assertIn("MSG_MG1_REWARDS", svc)
        self.assertIn("MSG_MG3_REWARDS", svc)
        self.assertIn("MSG_MG4_REWARDS", svc)
        self.assertIn("MSG_MG5_REWARDS", svc)
        self.assertIn("MSG_MG6_REWARDS", svc)

if __name__ == "__main__":
    unittest.main()

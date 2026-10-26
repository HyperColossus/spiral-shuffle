import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class MultiplayerMountsTests(unittest.TestCase):
    def test_multiplayer_mount_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(MultiplayerMountService)", factory)

    def test_multiplayer_mount_and_ride_handlers(self):
        svc = (SOURCE / "Game/Services/MultiplayerMountService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PARTYMULTIPLAYERMOUNTEQUIP", svc)
        self.assertIn("MSG_PARTYMULTIPLAYERMOUNTJOIN", svc)
        self.assertIn("MSG_STARTRIDE", svc)

if __name__ == "__main__":
    unittest.main()

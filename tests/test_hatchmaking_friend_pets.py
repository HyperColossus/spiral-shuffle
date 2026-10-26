import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHatchmakingFriendPets(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HatchmakingFriendPetQueryService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HatchmakingFriendPetQueryService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTHATCHMAKINGFRIENDPETS", svc)
        self.assertIn("MSG_REQUESTHATCHMAKINGHATCHEDPET", svc)
        self.assertIn("MSG_REQUESTHATCHMAKINGPETDATA", svc)
        self.assertIn("MSG_REQUESTHATCHMAKINGPETTYPES", svc)

if __name__ == "__main__":
    unittest.main()

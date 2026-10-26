import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestLevelUpElixirs(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(LevelUpElixirService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/LevelUpElixirService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_DISETLEVELUPELIXIRTELEPORT", svc)
        self.assertIn("MSG_DISETCHECKSUBSCRIPTIONPURCHASE", svc)
        self.assertIn("MSG_SETELIXIRTIMER", svc)
        self.assertIn("MSG_SETRENTALTIMER", svc)

if __name__ == "__main__":
    unittest.main()

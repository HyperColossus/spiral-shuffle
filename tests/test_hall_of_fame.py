import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHallOfFame(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HallOfFameService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HallOfFameService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLETOURSREMOVEHALLOFFAME", svc)
        self.assertIn("MSG_CASTLETOURSPREADD", svc)
        self.assertIn("MSG_RATEMYSTITCHCLAIMJUDGESCHOICE", svc)

if __name__ == "__main__":
    unittest.main()

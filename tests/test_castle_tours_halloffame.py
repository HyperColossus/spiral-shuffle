import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCastleToursHallOfFame(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CastleToursHallOfFameService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CastleToursHallOfFameService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLETOURSFAVORITEINFO", svc)
        self.assertIn("MSG_CASTLETOURSFAVORITEINFO2", svc)
        self.assertIn("MSG_CASTLETOURSHALLOFFAME", svc)
        self.assertIn("MSG_CASTLETOURSHALLOFFAMERATINGS", svc)
        self.assertIn("MSG_CASTLETOURSENABLERATINGDISPLAY", svc)
        self.assertIn("MSG_CASTLETOURSRATINGDISPLAY", svc)

if __name__ == "__main__":
    unittest.main()

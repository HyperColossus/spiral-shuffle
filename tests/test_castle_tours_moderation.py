import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCastleToursModeration(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CastleToursModerationService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CastleToursModerationService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLETOURSCSRRESULTS", svc)
        self.assertIn("MSG_CASTLETOURSDELETECHARACTER", svc)
        self.assertIn("MSG_CASTLETOURSGOTOFRIEND", svc)
        self.assertIn("MSG_CASTLETOURSLEADERBOARDRESPONSE", svc)
        self.assertIn("MSG_CASTLETOURSPLAYERBANNED", svc)
        self.assertIn("MSG_CASTLETOURSREMOVERESULT", svc)
        self.assertIn("MSG_CASTLETOURSVISITIGNORED", svc)
        self.assertIn("MSG_CASTLEGAMESTELEPORTREJECTED", svc)

if __name__ == "__main__":
    unittest.main()

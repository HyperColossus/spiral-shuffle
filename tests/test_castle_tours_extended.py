import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCastleToursExtended(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CastleToursService)", factory)

    def test_extended_protocol_handlers(self):
        svc = (SOURCE / "Game/Services/CastleToursService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLETOURSREQUESTLEADERBOARD", svc)
        self.assertIn("MSG_CASTLETOURSREQUESTNEXTLEADERBOARD", svc)
        self.assertIn("MSG_CASTLETOURSREQUESTMYCASTLES", svc)
        self.assertIn("MSG_CASTLETOURSREQUESTMYCASTLEDATA", svc)
        self.assertIn("MSG_CASTLETOURSREQUESTFRIENDS", svc)
        self.assertIn("MSG_CASTLETOURSADD", svc)
        self.assertIn("MSG_CASTLETOURSREMOVE", svc)
        self.assertIn("MSG_CASTLETOURSPOSTRATEHOUSE", svc)
        self.assertIn("MSG_CASTLETOURSADDFAVORITE", svc)
        self.assertIn("MSG_CASTLETOURSREMOVEFAVORITE", svc)

if __name__ == "__main__":
    unittest.main()

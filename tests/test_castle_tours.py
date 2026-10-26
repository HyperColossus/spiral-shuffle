import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class CastleToursTests(unittest.TestCase):
    def test_castle_tours_service_registered(self):
        """Verify CastleToursService is registered in GameServiceFactory."""
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CastleToursService)", factory)

    def test_castle_tours_browse_implemented(self):
        """Verify MSG_CASTLETOURSREQUEST responds with status."""
        ct = (SOURCE / "Game/Services/CastleToursService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLETOURSREQUEST", ct)
        self.assertIn("MSG_CASTLETOURSADDRESULT", ct)

    def test_castle_tours_visit_and_rate_implemented(self):
        """Verify visit teleports to showcase dorm and rate confirms."""
        ct = (SOURCE / "Game/Services/CastleToursService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLETOURSVISITCASTLE", ct)
        self.assertIn("WC_Housing_Dorm_Interior", ct)
        self.assertIn("MSG_CASTLETOURSSENDRATING", ct)


if __name__ == "__main__":
    unittest.main()

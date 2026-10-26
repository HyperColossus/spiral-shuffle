import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class RateMyStitchTests(unittest.TestCase):
    def test_rate_my_stitch_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(RateMyStitchService)", factory)

    def test_rate_my_stitch_requests_and_voting(self):
        svc = (SOURCE / "Game/Services/RateMyStitchService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_RATEMYSTITCHREQUEST", svc)
        self.assertIn("MSG_RATEMYSTITCHREQUESTOUTFITS", svc)
        self.assertIn("MSG_RATEMYSTITCHREQUESTLEADERBOARD", svc)
        self.assertIn("MSG_RATEMYSTITCHRATE", svc)
        self.assertIn("MSG_RATEMYSTITCHRATERESPONSE", svc)
        self.assertIn("MSG_RATEMYSTITCHPOSTRATE", svc)
        self.assertIn("MSG_RATEMYSTITCHEREQUESTREMOVE", svc)
        self.assertIn("MSG_RATEMYSTITCHADD", svc)

if __name__ == "__main__":
    unittest.main()

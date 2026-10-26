import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class SocialModerationTests(unittest.TestCase):
    def test_social_moderation_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(SocialModerationService)", factory)

    def test_ignore_and_reporting(self):
        svc = (SOURCE / "Game/Services/SocialModerationService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_IGNOREADD", svc)
        self.assertIn("MSG_IGNOREDROP", svc)
        self.assertIn("MSG_IGNORELIST", svc)
        self.assertIn("MSG_BUGREPORT", svc)
        self.assertIn("MSG_HARASSMENTREPORT", svc)
        self.assertIn("MSG_REPORTHOUSE", svc)

    def test_privacy_options(self):
        svc = (SOURCE / "Game/Services/SocialModerationService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTPRIVACYOPTIONS", svc)
        self.assertIn("MSG_RESPONSEPRIVACYOPTIONS", svc)
        self.assertIn("MSG_UPDATEPRIVACYOPTIONS", svc)

if __name__ == "__main__":
    unittest.main()

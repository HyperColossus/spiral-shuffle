import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCastleMagicAndSigns(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CastleMagicAndSignsService)", factory)

    def test_magic_and_signs_handlers(self):
        svc = (SOURCE / "Game/Services/CastleMagicAndSignsService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_CASTLEMAGICREQUESTPVPSTATE", svc)
        self.assertIn("MSG_CASTLEMAGICREQUESTRANDOM", svc)
        self.assertIn("MSG_SETHOUSESIGN", svc)
        self.assertIn("MSG_SETHOUSECUSTOMIZATION", svc)
        self.assertIn("MSG_SETHOUSINGPALETTE", svc)
        self.assertIn("MSG_SETTILEWALLPAPER", svc)

if __name__ == "__main__":
    unittest.main()

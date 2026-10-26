import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestCantripExtensions(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(CantripExtensionService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/CantripExtensionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_REQUESTMAGICWEAVINGTOME", svc)
        self.assertIn("MSG_CANTRIPAFFECTEDPLAYER", svc)
        self.assertIn("MSG_CANTRIPENDLOOP", svc)
        self.assertIn("MSG_CANTRIPLEVELUP", svc)
        self.assertIn("MSG_UPDATECANTRIPXP", svc)
        self.assertIn("MSG_UPDATERITUALOBJECT", svc)
        self.assertIn("MSG_CASTLEMAGICCANTRIP", svc)

if __name__ == "__main__":
    unittest.main()

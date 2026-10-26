import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHousingInteractiveNetwork(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingInteractiveNetworkService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HousingInteractiveNetworkService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_PATCHHOUSINGSIGNOBJECT", svc)
        self.assertIn("MSG_PATCHHOUSINGTELEPORTEROBJECT", svc)
        self.assertIn("MSG_REQUESTTELEPORTERHOUSINGCONFIRM", svc)
        self.assertIn("MSG_PATCHHOUSINGMUSICOBJECT", svc)
        self.assertIn("MSG_PATCHMUSICLOOP", svc)
        self.assertIn("MSG_SENDHOUSINGOBJECTSTATES", svc)
        self.assertIn("MSG_PATCHHOUSINGTRAIN", svc)
        self.assertIn("MSG_PLACEHOUSINGFISH", svc)

if __name__ == "__main__":
    unittest.main()

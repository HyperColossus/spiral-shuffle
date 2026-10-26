import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHousingItemFinder(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingItemFinderService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/HousingItemFinderService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ITEMFINDERHOUSECOMPLETE", svc)
        self.assertIn("MSG_ISFRIENDSWITHHOUSEOWNER", svc)
        self.assertIn("MSG_REQUESTHOUSEOWNERCHARACTERID", svc)
        self.assertIn("MSG_REQUESTHOUSETRANSFERLIST", svc)

if __name__ == "__main__":
    unittest.main()

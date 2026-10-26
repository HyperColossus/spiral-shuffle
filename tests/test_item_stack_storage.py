import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestItemStackStorage(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(ItemStackAndStorageSyncService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/ItemStackAndStorageSyncService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_COMBINEINVENTORYITEMS", svc)
        self.assertIn("MSG_REMOVETREASURESPELLFROMVAULT", svc)
        self.assertIn("MSG_STORAGECLIENTREMOVE", svc)
        self.assertIn("MSG_WIZINVENTORYCLIENTREMOVE", svc)
        self.assertIn("MSG_ADDPARTICLE", svc)
        self.assertIn("MSG_REMOVEPARTICLE", svc)
        self.assertIn("MSG_REMOVEDSHOPPER", svc)
        self.assertIn("MSG_RECIPEREMOVE", svc)
        self.assertIn("MSG_DERBYEFFECTREMOVE", svc)

if __name__ == "__main__":
    unittest.main()

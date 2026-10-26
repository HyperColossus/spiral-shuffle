import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class PetProgressionTests(unittest.TestCase):
    def test_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(PetProgressionService)", factory)

    def test_message_handlers(self):
        svc = (SOURCE / "Game/Services/PetProgressionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_BUYPETELIXIRCONFIRM", svc)
        self.assertIn("MSG_DONATEPET", svc)
        self.assertIn("MSG_BLOCKPETSPAWN", svc)
        self.assertIn("MSG_DISPLAYPETONLYMESSAGE", svc)
        self.assertIn("MSG_DERBYPETENERGYINFO", svc)

if __name__ == "__main__":
    unittest.main()

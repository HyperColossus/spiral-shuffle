import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestHousingVaults(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(HousingVaultService)", factory)

    def test_vault_protocol_handlers(self):
        svc = (SOURCE / "Game/Services/HousingVaultService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_MOVEGEARTOGEARVAULT", svc)
        self.assertIn("MSG_MOVEGEARFROMGEARVAULT", svc)
        self.assertIn("MSG_MOVEJEWELTOJEWELVAULT", svc)
        self.assertIn("MSG_MOVEJEWELFROMJEWELVAULT", svc)
        self.assertIn("MSG_MOVESEEDTOGARDENINGSHED", svc)
        self.assertIn("MSG_MOVESEEDFROMGARDENINGSHED", svc)
        self.assertIn("MSG_MOVETCTOTCVAULT", svc)
        self.assertIn("MSG_MOVETCFROMTCVAULT", svc)
        self.assertIn("MSG_DELETEFROMATTIC", svc)
        self.assertIn("MSG_PATCHADDATTIC", svc)
        self.assertIn("MSG_PATCHDELETEATTIC", svc)

if __name__ == "__main__":
    unittest.main()

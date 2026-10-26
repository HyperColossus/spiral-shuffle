import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestFishFailureVaultPatch(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(FishFailureAndVaultPatchService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/FishFailureAndVaultPatchService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_MISSFISH", svc)
        self.assertIn("MSG_NOFISHSPACE", svc)
        self.assertIn("MSG_PATCHAQUARIUM", svc)
        self.assertIn("MSG_PATCHTREASURECARDVAULT", svc)
        self.assertIn("MSG_PATCHMONSTERARENA", svc)

if __name__ == "__main__":
    unittest.main()

import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestGardeningLifecycle(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(GardeningLifecycleService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/GardeningLifecycleService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_GARDENINGHARVESTPLANTSECONDSPRING", svc)
        self.assertIn("MSG_GARDENINGSPELLINSPECT", svc)
        self.assertIn("MSG_GARDENINGPELLFIZZLE", svc)
        self.assertIn("MSG_PATCHGARDENING", svc)
        self.assertIn("MSG_GARDENINGCSRRESULTS", svc)

if __name__ == "__main__":
    unittest.main()

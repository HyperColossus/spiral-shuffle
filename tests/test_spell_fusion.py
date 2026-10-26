import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestSpellFusion(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(SpellFusionService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/SpellFusionService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ADDSPELLFUSIONTODECK", svc)
        self.assertIn("MSG_ADDSPELLTOBOOK", svc)
        self.assertIn("MSG_ADDTREASURESPELLTOBOOK", svc)
        self.assertIn("MSG_UPDATENEWSPELLBOOKLAYOUTWARNING", svc)

if __name__ == "__main__":
    unittest.main()

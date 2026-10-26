import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / ".build/source"

class TestNpcSpeechPersona(unittest.TestCase):
    def test_service_factory_registration(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(NpcSpeechAndPersonaService)", factory)

    def test_protocol_handlers_exist(self):
        svc = (SOURCE / "Game/Services/NpcSpeechAndPersonaService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_NPCINFO", svc)
        self.assertIn("MSG_NPCSPEECH", svc)
        self.assertIn("MSG_PERSONAINFO", svc)
        self.assertIn("MSG_OBJECT_INFO_QUERY", svc)
        self.assertIn("MSG_PARTYLEVELUP", svc)
        self.assertIn("MSG_OPENCLASSPROJECTPORTAL", svc)

if __name__ == "__main__":
    unittest.main()

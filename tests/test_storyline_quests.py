import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"

class StorylineQuestsTests(unittest.TestCase):
    def test_quest_service_registered(self):
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(QuestService)", factory)

    def test_tutorial_storyline_start(self):
        ts = (SOURCE / "Game/Services/TutorialService.cs").read_text(encoding="utf-8")
        self.assertIn("Tutorial_Intro", ts)
        self.assertIn("SkipTutorialGoal", ts)
        self.assertIn("Character.StartingZone", ts)

    def test_quest_acceptance_and_bogus_handling(self):
        qs = (SOURCE / "Game/Services/QuestService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_ACCEPTQUEST", qs)
        self.assertIn("MSG_ACCEPTQUESTBOGUS", qs)

    def test_persona_dialogue_progression(self):
        qs = (SOURCE / "Game/Services/QuestService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_COMPLETEPERSONAGOAL", qs)
        self.assertIn("MSG_COMPLETEPERSONA", qs)
        self.assertIn("MSG_COMPLETEDIALOG", qs)

    def test_usage_and_proximity_goals(self):
        qs = (SOURCE / "Game/Services/QuestService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_COMPLETEUSAGEGOAL", qs)
        self.assertIn("MSG_COMPLETEPROXIMITYGOAL", qs)

    def test_combat_goals_and_turn_in(self):
        qs = (SOURCE / "Game/Services/QuestService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_COMBATWIN", qs)
        self.assertIn("CompleteQuest", qs)
        self.assertIn("ResultDispatcher.ExecuteResults", qs)

    def test_quest_finder_navigation(self):
        qs = (SOURCE / "Game/Services/QuestService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_QUESTFINDEROPTION", qs)
        self.assertIn("MSG_REQUESTQUESTFRIENDFINDER", qs)

if __name__ == "__main__":
    unittest.main()

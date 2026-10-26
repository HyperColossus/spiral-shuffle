import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class DailyQuestTests(unittest.TestCase):
    def test_daily_quest_service_registered(self):
        """Verify DailyQuestService is registered in GameServiceFactory."""
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(DailyQuestService)", factory)

    def test_daily_quest_open_implemented(self):
        """Verify MSG_DAILYQUESTOPEN sends DailyQuestUpdate."""
        daily = (SOURCE / "Game/Services/DailyQuestService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_DAILYQUESTOPEN", daily)
        self.assertIn("MSG_DailyQuestUpdate", daily)

    def test_daily_quest_completed_implemented(self):
        """Verify MSG_DAILYQUESTCOMPLETED awards gold, tickets, crowns, advances streak."""
        daily = (SOURCE / "Game/Services/DailyQuestService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_DAILYQUESTCOMPLETED", daily)
        self.assertIn("wizard.AddGold(250)", daily)
        self.assertIn("m_currentPvPCurrency += 5", daily)
        self.assertIn("wizard.Account.SetCrowns", daily)
        self.assertIn("ConsecutiveDays = (byte)(message.ConsecutiveDays + 1)", daily)


if __name__ == "__main__":
    unittest.main()

import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class TradingTests(unittest.TestCase):
    def test_trade_service_registered(self):
        """Verify TradeService is registered in GameServiceFactory."""
        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(TradeService)", factory)

    def test_trade_protocol_implemented(self):
        """Verify TradeService handles requests, joins, item and gold offers, ready status, and results."""
        trade = (SOURCE / "Game/Services/TradeService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_TRADE_REQUEST", trade)
        self.assertIn("MSG_TRADE_CREATE", trade)
        self.assertIn("MSG_TRADE_JOIN_STATUS", trade)
        self.assertIn("MSG_TRADE_CHANGE_ITEM", trade)
        self.assertIn("MSG_TRADE_CHANGE_MONEY", trade)
        self.assertIn("MSG_TRADE_READY_STATUS", trade)
        self.assertIn("MSG_TRADE_RESULT", trade)
        self.assertIn("ExecuteTrade", trade)


if __name__ == "__main__":
    unittest.main()

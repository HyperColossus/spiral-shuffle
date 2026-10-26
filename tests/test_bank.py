import unittest
from pathlib import Path

ROOT = Path("/home/hypercolossus/spiral-shuffle")
SOURCE = ROOT / ".build/source"


class BankSystemTests(unittest.TestCase):
    def test_bank_service_exists_and_registered(self):
        """Verify BankService exists and is registered in GameServiceFactory."""
        bank_file = SOURCE / "Game/Services/BankService.cs"
        self.assertTrue(bank_file.is_file(), "BankService.cs must be copied into Game/Services")

        factory = (SOURCE / "Game/GameServiceFactory.cs").read_text(encoding="utf-8")
        self.assertIn("typeof(BankService)", factory)

    def test_bank_handlers_in_bank_service(self):
        """Verify all essential bank message handlers are implemented."""
        bank_code = (SOURCE / "Game/Services/BankService.cs").read_text(encoding="utf-8")
        self.assertIn("MSG_OPENBANK", bank_code)
        self.assertIn("MSG_MOVEINVTOBANK", bank_code)
        self.assertIn("MSG_MOVEBANKTOINV", bank_code)
        self.assertIn("MSG_MOVEBANKTOBANK", bank_code)
        self.assertIn("MSG_BANKDELETE", bank_code)

    def test_account_shared_bank_models(self):
        """Verify Account and AccountCollection support SharedBank."""
        acc_model = (SOURCE / "WizardData/Models/Player/Account.cs").read_text(encoding="utf-8")
        self.assertIn("SharedBankItemIds", acc_model)
        self.assertIn("SharedBankItems", acc_model)

        acc_coll = (SOURCE / "WizardData/Collections/AccountCollection.cs").read_text(encoding="utf-8")
        self.assertIn("UpdateSharedBank", acc_coll)
        self.assertIn("account.SharedBankItems =", acc_coll)

    def test_wizard_bank_models(self):
        """Verify Wizard and WizardCollection support character Bank."""
        wiz_model = (SOURCE / "WizardData/Models/Player/Wizard.cs").read_text(encoding="utf-8")
        self.assertIn("BankItemIds", wiz_model)
        self.assertIn("BankItems", wiz_model)

        wiz_coll = (SOURCE / "WizardData/Collections/WizardCollection.cs").read_text(encoding="utf-8")
        self.assertIn("wizard.BankItems =", wiz_coll)

        item_coll = (SOURCE / "WizardData/Collections/WizardItemCollection.cs").read_text(encoding="utf-8")
        self.assertIn("UpdateItemCharacter", item_coll)


if __name__ == "__main__":
    unittest.main()

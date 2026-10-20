"""Connect equipment card settings to owned gear, rewards and shops."""
from collections.abc import Callable


def patch_item_cards(replace: Callable[[str, str, str], None]) -> None:
    replace("Game/Spells/SpellFactory.cs", "        var stringHash = StringHash.Compute(spellName);", """        var stringHash = StringHash.Compute(spellName);
        if (Imlight.CoreLib.Game.EquipmentItemCards.IsSpellId(stringHash)) {
            return GetSpell(stringHash);
        }""")
    replace("Game/Spells/SpellFactory.cs", "    internal static uint GetTemplateIdByHash(uint spellHash) {", """    internal static uint GetTemplateIdByHash(uint spellHash) {
        if (Imlight.CoreLib.Game.EquipmentItemCards.IsSpellId(spellHash)) {
            return spellHash;
        }""")
    replace("Shared/Character/CharacterHelper.cs", "        wizard.GameEffects = [];", """        wizard.GameEffects = [];
        Imlight.CoreLib.Game.EquipmentItemCards.PrepareRecalculation(wizard);""")
    replace("Login/Services/CharacterService.cs", """                 Imlight.CoreLib.Game.SpellDrops.HandleMenuSetting(account.AccountId, message.Message, out var response)) {
            SendToSocket(new GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS {
                JobID = Imlight.CoreLib.Game.RandomizerRuntime.MenuJobId, Message = response
            });
        }""", """                 Imlight.CoreLib.Game.SpellDrops.HandleMenuSetting(account.AccountId, message.Message, out var response)) {
            SendToSocket(new GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS {
                JobID = Imlight.CoreLib.Game.RandomizerRuntime.MenuJobId, Message = response
            });
        }
        else if (account is not null && message.JobID == Imlight.CoreLib.Game.RandomizerRuntime.MenuJobId &&
                 Imlight.CoreLib.Game.EquipmentItemCards.HandleMenuSetting(account.AccountId, message.Message, out var itemCardResponse)) {
            SendToSocket(new GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS {
                JobID = Imlight.CoreLib.Game.RandomizerRuntime.MenuJobId, Message = itemCardResponse
            });
        }""")
    replace("WizardData/Models/Player/Wizard.cs", "    internal void AfterDatabaseLoad() {", """    internal void AfterDatabaseLoad() {
        Imlight.CoreLib.Game.EquipmentItemCards.Apply(this);""")
    replace("WizardData/Models/Player/Wizard.cs", "        CoreObjectFactory.InitializeCoreObjectBehaviors(item, item.m_templateID);", """        Imlight.CoreLib.Game.EquipmentItemCards.Apply(this, item);
        CoreObjectFactory.InitializeCoreObjectBehaviors(item, item.m_templateID);""")
    replace("WizardData/Models/Player/Wizard.cs", "            CoreObjectFactory.InitializeCoreObjectBehaviors(cObj, templateId);", """            Imlight.CoreLib.Game.EquipmentItemCards.Apply(this, cObj);
            CoreObjectFactory.InitializeCoreObjectBehaviors(cObj, cObj.m_templateID);""")
    replace("WizardData/Models/Player/Wizard.cs", "        var template = ItemHelper.GetItemTemplate(inventoryItem);", """        Imlight.CoreLib.Game.EquipmentItemCards.Apply(this, inventoryItem);
        var template = ItemHelper.GetItemTemplate(inventoryItem);""")
    replace("Game/Zone/Components/InteractVendorComponent.cs",
            "=> Imlight.CoreLib.Game.VendorRandomization.Items(wizard, Entity.ActiveGameObject.m_templateID, _inventory);",
            "=> Imlight.CoreLib.Game.EquipmentItemCards.ShopItems(wizard, Imlight.CoreLib.Game.VendorRandomization.Items(wizard, Entity.ActiveGameObject.m_templateID, _inventory));")
    replace("Game/DropTables/LootGranter.cs", "        UpdateCharacterItems(playerActor, wizard, results);", """        Imlight.CoreLib.Game.EquipmentItemCards.ApplyLoot(wizard, results);
        UpdateCharacterItems(playerActor, wizard, results);""")
    replace("Game/Services/ShopService.cs", """    private void ProcessSuccessfulPurchase(Wizard playerWizard, WizClientObjectItem item, uint itemTemplateID, int goldCost) {
""", """    private void ProcessSuccessfulPurchase(Wizard playerWizard, WizClientObjectItem item, uint itemTemplateID, int goldCost) {
        Imlight.CoreLib.Game.EquipmentItemCards.Apply(playerWizard, item);
        itemTemplateID = (uint)item.m_templateID;
""")
    replace("Game/Services/AuctionHouseService.cs", "        var item = (WizClientObjectItem) CoreObjectFactory.FinalizeCoreObject(templateId);", """        var item = (WizClientObjectItem) CoreObjectFactory.FinalizeCoreObject(templateId);
        Imlight.CoreLib.Game.EquipmentItemCards.Apply(GetActiveWizard(), item);""")

"""Keep spellbook previews and combat on the same difficulty without changing learned IDs."""
from collections.abc import Callable


def patch_spell_numbers(replace: Callable[[str, str, str], None]) -> None:
    replace("Game/WizardObjectLoader.cs", "        return clientObject;",
            "        return SpellPresentation.ForClient(clientObject, character.AccountId);")
    replace("WizardData/Models/Player/Wizard.cs", "    public bool AddSpellToDeck(uint spellTemplateId, ulong deckId) {",
            """    public bool AddSpellToDeck(uint spellTemplateId, ulong deckId) {
        spellTemplateId = Imlight.CoreLib.Game.RandomizerRuntime.OriginalSpellId(spellTemplateId);""")
    replace("WizardData/Models/Player/Wizard.cs", "    public bool RemoveSpellFromDeck(uint spellTemplateId, ulong deckId) {",
            """    public bool RemoveSpellFromDeck(uint spellTemplateId, ulong deckId) {
        spellTemplateId = Imlight.CoreLib.Game.RandomizerRuntime.OriginalSpellId(spellTemplateId);""")
    replace("Game/Spells/SpellFactory.cs", "if (Imlight.CoreLib.Game.EquipmentItemCards.IsSpellId(spellHash)) {",
            "if (Imlight.CoreLib.Game.EquipmentItemCards.IsSpellId(spellHash) || Imlight.CoreLib.Game.RandomizerRuntime.IsSpellVariant(spellHash)) {")
    replace("Game/Spells/SpellTeacher.cs", "            SpellID = (int) spellTemplateId,",
            "            SpellID = (int) Imlight.CoreLib.Game.RandomizerRuntime.CombatSpellId(spellTemplateId, Imlight.CoreLib.Game.RandomizerRuntime.GetMode(wizard.AccountId)),")
    replace("Game/Services/TrainService.cs", "            SpellID = (int) spellEntry.TemplateID",
            "            SpellID = (int) Imlight.CoreLib.Game.RandomizerRuntime.CombatSpellId((uint) spellEntry.TemplateID, Imlight.CoreLib.Game.RandomizerRuntime.GetMode(wizard.AccountId))")

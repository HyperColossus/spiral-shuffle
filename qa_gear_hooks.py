"""Keep QA equipment out of starter kits and existing randomizer inventories."""
from collections.abc import Callable


def patch_qa_gear(replace: Callable[[str, str, str], None]) -> None:
    replace("WizardData/Models/Player/Wizard.cs", "foreach (var templateId in templateIds) {",
            "foreach (var templateId in templateIds.Where(id => !Imlight.CoreLib.Game.RandomizerRuntime.Enabled ||\n"
            "                     !Imlight.CoreLib.Game.QaGear.IsQa(id))) {")
    replace("WizardData/Collections/WizardCollection.cs", "        // `AlchemyBehavior` only keeps track of the reagent global IDs.",
            """        if (Imlight.CoreLib.Game.RandomizerRuntime.Enabled && Imlight.CoreLib.Game.QaGear.RemoveFrom(wizard)) {
            UpdateCharacterItems(wizard);
        }

        // `AlchemyBehavior` only keeps track of the reagent global IDs.""")

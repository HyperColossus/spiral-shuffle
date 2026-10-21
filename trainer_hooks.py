"""Keep trainer eligibility, costs and spellbook notifications consistent."""
from collections.abc import Callable


def patch_trainers(replace: Callable[[str, str, str], None]) -> None:
    path = "Game/Services/TrainService.cs"
    replace(path, "var spellCost = wizard.MagicSchoolBehavior.MagicSchool.ToString() == spellTemplate.m_sMagicSchoolName ? 0 : 1;",
            "var spellCost = Imlight.CoreLib.Game.TrainerRules.Cost(wizard, spellEntry);")
    replace(path, "if (wizard.MagicSchoolBehavior.TrainingPoints < spellCost)",
            "if (!Imlight.CoreLib.Game.TrainerRules.CanTrain(wizard, spellEntry))")
    replace(path, "            SpellID = spellEntry.TemplateID,",
            "            SpellID = Imlight.CoreLib.Game.TrainerRules.ClientId(wizard, spellEntry.TemplateID),")

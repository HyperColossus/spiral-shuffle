using Imcodec.ObjectProperty.TypeCache;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Models.Player;
using Imlight.CoreLib.WizardData.Models.World;

namespace Imlight.CoreLib.Game;

internal static class TrainerRules {
    internal static uint ClientId(Wizard wizard, ulong original) =>
        RandomizerRuntime.CombatSpellId((uint)original, RandomizerRuntime.GetMode(wizard.AccountId));

    internal static int Cost(Wizard wizard, NPCSpellEntry entry) {
        var template = (SpellTemplate)CoreObjectFactory.GetCoreTemplate(entry.TemplateID);
        string school = RandomizerRuntime.TrainerSpellSchool((uint)entry.TemplateID, template.m_sMagicSchoolName);
        return wizard.MagicSchoolBehavior.MagicSchool.ToString() == school ? 0 : 1;
    }

    internal static bool CanTrain(Wizard wizard, NPCSpellEntry entry) =>
        !wizard.SpellbookBehavior.HasSpell((uint)entry.TemplateID) &&
        wizard.MagicSchoolBehavior.Level >= entry.Level &&
        wizard.MagicSchoolBehavior.TrainingPoints >= Cost(wizard, entry) &&
        (entry.RequiredSpellID == 0 || wizard.SpellbookBehavior.HasSpell((uint)entry.RequiredSpellID));
}

using Imcodec.ObjectProperty.TypeCache;

namespace Imlight.CoreLib.Game.Requirements.Handlers;

internal sealed class RandomizerLevelRequirement : BaseRequirementHandler<ReqMagicLevel> {
    public override bool Evaluate(IRequirementContext context) {
        var wizard = context.GetWizard();
        return wizard is not null && NumericRequirement.Compare(Requirement, wizard.MagicSchoolBehavior.Level);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.CoreLib.Shared.Resources;

namespace Imlight.CoreLib.Game.Spells;

internal static class SpellEffectFilter {
    internal static List<Spell> FilterSpellsByOutgoingDamage(List<Spell> spells) => Filter(spells,
        (effect, target) => IsEnemyTarget(target) && effect.m_effectType is
            kSpellEffects.kDamage or kSpellEffects.kDamageNoCrit or kSpellEffects.kStealHealth or
            kSpellEffects.kDamageOverTime or kSpellEffects.kDamagePerTotalPipPower);

    internal static List<Spell> FilterSpellsByHealing(List<Spell> spells) => Filter(spells,
        (effect, target) => IsFriendlyTarget(target) && effect.m_effectType is
            kSpellEffects.kHeal or kSpellEffects.kHealOverTime or kSpellEffects.kHealByWard or
            kSpellEffects.kHealPercent or kSpellEffects.kMaxHealthHeal or kSpellEffects.kSetHealPercent);

    internal static List<Spell> FilterSpellsByBuff(List<Spell> spells) => Filter(spells,
        (effect, target) => IsFriendlyTarget(target) && IsModifier(effect));

    internal static List<Spell> FilterSpellsByDebuff(List<Spell> spells) => Filter(spells,
        (effect, target) => IsEnemyTarget(target) && IsModifier(effect));

    private static List<Spell> Filter(IEnumerable<Spell> spells, Func<SpellEffect, kEffectTarget, bool> predicate) =>
        spells.Where(spell => CoreObjectFactory.GetCoreTemplate(spell.m_templateID) is SpellTemplate template &&
            Effects(template.m_effects).Any(value => predicate(value.Effect, value.Target))).ToList();

    private static IEnumerable<(SpellEffect Effect, kEffectTarget Target)> Effects(
        IEnumerable<SpellEffect> effects, kEffectTarget inherited = kEffectTarget.kInvalidTarget) {
        foreach (SpellEffect effect in effects) {
            kEffectTarget target = effect.m_effectTarget == kEffectTarget.kInvalidTarget ? inherited : effect.m_effectTarget;
            IEnumerable<SpellEffect> children = effect switch {
                RandomSpellEffect random => random.m_effectList,
                VariableSpellEffect variable => variable.m_effectList,
                EffectListSpellEffect list => list.m_effectList,
                CountBasedSpellEffect count => count.m_effectList,
                ConditionalSpellEffect conditional => conditional.m_elements.Select(element => element.m_pEffect),
                TargetCountSpellEffect targetCount => targetCount.m_effectLists,
                HangingConversionSpellEffect conversion => conversion.m_outputEffect,
                _ => null
            };
            if (children is null) {
                yield return (effect, target);
            }
            else {
                foreach (var nested in Effects(children, target)) {
                    yield return nested;
                }
            }
        }
    }

    private static bool IsEnemyTarget(kEffectTarget target) => target is
        kEffectTarget.kAtLeastOneEnemy or kEffectTarget.kEnemyTeam or kEffectTarget.kEnemyTeamAllAtOnce or
        kEffectTarget.kEnemySingle or kEffectTarget.kMultiTargetEnemy or kEffectTarget.kPreselectedEnemySingle or kEffectTarget.kEnemyMinion;

    private static bool IsFriendlyTarget(kEffectTarget target) => target is
        kEffectTarget.kFriendlyMinion or kEffectTarget.kFriendlyTeam or kEffectTarget.kFriendlyTeamAllAtOnce or
        kEffectTarget.kFriendlySingle or kEffectTarget.kMultiTargetFriendly or kEffectTarget.kFriendlySingleNotMe or
        kEffectTarget.kSelf or kEffectTarget.kInvalidTarget or kEffectTarget.kCasterMinion;

    private static bool IsModifier(SpellEffect effect) => effect.m_effectType is
        kSpellEffects.kModifyIncomingDamage or kSpellEffects.kModifyIncomingDamageFlat or
        kSpellEffects.kModifyIncomingDamageOverTime or kSpellEffects.kModifyIncomingHeal or
        kSpellEffects.kModifyIncomingHealFlat or kSpellEffects.kModifyIncomingHealOverTime or
        kSpellEffects.kModifyIncomingArmorPiercing or kSpellEffects.kModifyOutgoingDamage or
        kSpellEffects.kModifyOutgoingDamageFlat or kSpellEffects.kModifyOutgoingHeal or
        kSpellEffects.kModifyOutgoingHealFlat or kSpellEffects.kModifyOutgoingArmorPiercing or
        kSpellEffects.kModifyAccuracy or kSpellEffects.kAbsorbDamage or kSpellEffects.kStunBlock or kSpellEffects.kStunResist;
}

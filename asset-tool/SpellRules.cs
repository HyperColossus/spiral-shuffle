using Imcodec.ObjectProperty.TypeCache;

internal static class SpellRules {
    internal static IEnumerable<SpellEffect> Effects(IEnumerable<SpellEffect> effects) {
        foreach (SpellEffect effect in effects) {
            yield return effect;
            IEnumerable<SpellEffect> nested = effect switch {
                RandomSpellEffect random => random.m_effectList,
                VariableSpellEffect variable => variable.m_effectList,
                EffectListSpellEffect list => list.m_effectList,
                CountBasedSpellEffect count => count.m_effectList,
                ConditionalSpellEffect conditional => conditional.m_elements.Select(element => element.m_pEffect),
                TargetCountSpellEffect targetCount => targetCount.m_effectLists,
                HangingConversionSpellEffect conversion => conversion.m_outputEffect,
                _ => []
            };
            foreach (SpellEffect child in Effects(nested)) {
                yield return child;
            }
        }
    }

    internal static string Role(SpellTemplate spell) {
        SpellEffect[] effects = Effects(spell.m_effects).ToArray();
        string[] kinds = effects.Select(effect => effect.m_effectType.ToString()).ToArray();
        if (kinds.Any(kind => new[] { "Summon", "SpawnCreature", "Polymorph", "Teleport", "Enchant" }
            .Any(protectedKind => kind.Contains(protectedKind, StringComparison.Ordinal)))) {
            return "protected";
        }

        if (kinds.Any(kind => kind is "kDamage" or "kStealHealth" or "kDamageOverTime")) {
            return effects.Any(effect => effect.m_effectTarget.ToString().Contains("All", StringComparison.Ordinal))
                ? "aoe" : "attack";
        }

        if (kinds.Any(kind => kind is "kHeal" or "kHealOverTime")) {
            return "heal";
        }
        if (kinds.Contains("kModifyOutgoingDamage")) {
            return "blade";
        }
        if (kinds.Contains("kModifyIncomingDamage")) {
            return effects.Any(effect => effect.m_effectType == kSpellEffects.kModifyIncomingDamage && effect.m_effectParam > 0)
                ? "trap" : "shield";
        }
        if (kinds.Contains("kModifyAccuracy")) {
            return "accuracy";
        }
        if (kinds.Contains("kModifyCardDamage")) {
            return "damage-enchant";
        }
        return "protected";
    }

    internal static bool Compatible(SpellTemplate original, SpellTemplate candidate) =>
        candidate.m_displayName != original.m_displayName &&
        (Role(original) is not ("attack" or "aoe" or "heal") || candidate.m_spellBase != original.m_spellBase) &&
        Role(candidate) == Role(original) &&
        (original.m_spellRank.m_spellRank <= 1
            ? candidate.m_spellRank.m_spellRank == original.m_spellRank.m_spellRank
            : Math.Abs(candidate.m_spellRank.m_spellRank - original.m_spellRank.m_spellRank) <= 1);
}

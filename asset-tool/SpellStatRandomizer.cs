using Imcodec.ObjectProperty.TypeCache;

internal static class SpellStatRandomizer {
    internal static double Multiplier(int difficulty, double unit) {
        double direction = unit < 0.5 ? -1 : 1;
        double magnitude = 0.05 + 0.95 * Math.Abs(2 * unit - 1);
        double width = difficulty switch {
            3 => direction < 0 ? 0.50 : 1,
            2 => 0.50,
            _ => 0.25
        };
        return 1 + direction * width * magnitude;
    }

    internal static void Apply(BindDocument document, SpellTemplate template, double multiplier, bool modifiers = true) {
        BindProperty effects = document.Value.Get<SpellTemplate>("m_effects");
        document.Value.Set<SpellTemplate>("m_effects", ScaleEffects(effects, template.m_effects, multiplier, modifiers));
    }

    private static BindProperty ScaleEffects(BindProperty property, IReadOnlyList<SpellEffect> effects, double multiplier, bool modifiers) {
        List<BindObject> values = BindDocument.ReadVector(property);
        if (values.Count != effects.Count) {
            throw new InvalidDataException("Spell effect structure does not match its binary data.");
        }

        for (int index = 0; index < effects.Count; index++) {
            ScaleEffect(values[index], effects[index], multiplier, modifiers);
        }

        return BindDocument.WriteVector(property, values);
    }

    private static void ScaleEffect(BindObject value, SpellEffect effect, double multiplier, bool modifiers) {
        switch (effect) {
            case RandomSpellEffect random:
                value.Set<RandomSpellEffect>("m_effectList", ScaleEffects(
                    value.Get<RandomSpellEffect>("m_effectList"), random.m_effectList, multiplier, modifiers));
                break;
            case VariableSpellEffect variable:
                value.Set<VariableSpellEffect>("m_effectList", ScaleEffects(
                    value.Get<VariableSpellEffect>("m_effectList"), variable.m_effectList, multiplier, modifiers));
                break;
            case EffectListSpellEffect list:
                value.Set<EffectListSpellEffect>("m_effectList", ScaleEffects(
                    value.Get<EffectListSpellEffect>("m_effectList"), list.m_effectList, multiplier, modifiers));
                break;
            case CountBasedSpellEffect count:
                value.Set<CountBasedSpellEffect>("m_effectList", ScaleEffects(
                    value.Get<CountBasedSpellEffect>("m_effectList"), count.m_effectList, multiplier, modifiers));
                break;
            case ConditionalSpellEffect conditional:
                ScaleConditional(value, conditional, multiplier, modifiers);
                break;
            case TargetCountSpellEffect targetCount:
                value.Set<TargetCountSpellEffect>("m_effectLists", ScaleEffects(
                    value.Get<TargetCountSpellEffect>("m_effectLists"), targetCount.m_effectLists.Cast<SpellEffect>().ToArray(), multiplier, modifiers));
                break;
            case HangingConversionSpellEffect conversion:
                value.Set<HangingConversionSpellEffect>("m_outputEffect", ScaleEffects(
                    value.Get<HangingConversionSpellEffect>("m_outputEffect"), conversion.m_outputEffect, multiplier, modifiers));
                break;
        }

        if (effect.m_effectType is kSpellEffects.kDamage or kSpellEffects.kDamageNoCrit or kSpellEffects.kStealHealth or
            kSpellEffects.kDamageOverTime or kSpellEffects.kHeal or kSpellEffects.kHealOverTime) {
            ScaleParameter(value, "m_effectParam", effect.m_effectParam, multiplier);
            ScaleParameter(value, "m_paramPerRound", effect.m_paramPerRound, multiplier);
        }
        else if (modifiers && effect.m_effectType is kSpellEffects.kModifyOutgoingDamage or
                 kSpellEffects.kModifyIncomingDamage or kSpellEffects.kModifyAccuracy or
                 kSpellEffects.kModifyOutgoingHeal or kSpellEffects.kModifyIncomingHeal or
                 kSpellEffects.kModifyOutgoingArmorPiercing or kSpellEffects.kModifyIncomingArmorPiercing or
                 kSpellEffects.kModifyCardAccuracy or kSpellEffects.kModifyCardArmorPiercing or
                 kSpellEffects.kModifyPowerPipChance or kSpellEffects.kModifyCardOutgoingDamage or
                 kSpellEffects.kModifyCardOutgoingAccuracy or kSpellEffects.kModifyCardOutgoingHeal or
                 kSpellEffects.kModifyCardOutgoingArmorPiercing or kSpellEffects.kModifyCardIncomingDamage) {
            int amount = (int)Math.Clamp(Math.Round(effect.m_effectParam * multiplier), -95, 200);
            if (amount == 0 && effect.m_effectParam != 0) {
                amount = Math.Sign(effect.m_effectParam);
            }
            BindProperty parameter = value.Get<SpellEffect>("m_effectParam");
            value.Set<SpellEffect>("m_effectParam", parameter with { Data = BitConverter.GetBytes(amount) });
        }
        else if (modifiers && effect.m_effectType is kSpellEffects.kAbsorbDamage or
                 kSpellEffects.kAbsorbHeal or kSpellEffects.kModifyCardDamage or kSpellEffects.kModifyCardHeal or
                 kSpellEffects.kModifyOutgoingDamageFlat or kSpellEffects.kModifyIncomingDamageFlat or
                 kSpellEffects.kModifyOutgoingHealFlat or kSpellEffects.kModifyIncomingHealFlat or
                 kSpellEffects.kCritBoost or kSpellEffects.kCritBlock or kSpellEffects.kCritBoostSchoolSpecific) {
            ScaleParameter(value, "m_effectParam", effect.m_effectParam, multiplier);
        }
    }

    private static void ScaleConditional(BindObject value, ConditionalSpellEffect effect, double multiplier, bool modifiers) {
        BindProperty property = value.Get<ConditionalSpellEffect>("m_elements");
        List<BindObject> elements = BindDocument.ReadVector(property);
        for (int index = 0; index < elements.Count; index++) {
            BindProperty child = elements[index].Get<ConditionalSpellElement>("m_pEffect");
            BindObject nested = BindObject.Read(child.Data);
            ScaleEffect(nested, effect.m_elements[index].m_pEffect, multiplier, modifiers);
            elements[index].Set<ConditionalSpellElement>("m_pEffect", child with { Data = nested.Encode() });
        }
        value.Set<ConditionalSpellEffect>("m_elements", BindDocument.WriteVector(property, elements));
    }

    private static void ScaleParameter(BindObject value, string name, int original, double multiplier) {
        if (original <= 0) {
            return;
        }

        int result = (int)Math.Clamp(Math.Round(original * multiplier), 1, int.MaxValue);
        BindProperty property = value.Get<SpellEffect>(name);
        value.Set<SpellEffect>(name, property with { Data = BitConverter.GetBytes(result) });
    }
}

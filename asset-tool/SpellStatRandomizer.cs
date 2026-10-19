using Imcodec.ObjectProperty.TypeCache;

internal static class SpellStatRandomizer {
    internal static double Multiplier(int difficulty, double unit) {
        double direction = unit < 0.5 ? -1 : 1;
        double magnitude = 0.05 + 0.95 * Math.Abs(2 * unit - 1);
        double width = difficulty switch {
            3 => direction < 0 ? 0.75 : 1,
            2 => 0.50,
            _ => 0.25
        };
        return 1 + direction * width * magnitude;
    }

    internal static void Apply(BindDocument document, SpellTemplate template, double multiplier, bool modifiers = false) {
        BindProperty effects = document.Value.Get<SpellTemplate>("m_effects");
        document.Value.Set<SpellTemplate>("m_effects", ScaleEffects(effects, template.m_effects, multiplier, modifiers));
    }

    private static BindProperty ScaleEffects(BindProperty property, IReadOnlyList<SpellEffect> effects, double multiplier, bool modifiers) {
        List<BindObject> values = BindDocument.ReadVector(property);
        if (values.Count != effects.Count) {
            throw new InvalidDataException("Spell effect structure does not match its binary data.");
        }

        for (int index = 0; index < effects.Count; index++) {
            SpellEffect effect = effects[index];
            BindObject value = values[index];
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
            }

            if (effect.m_effectType is kSpellEffects.kDamage or kSpellEffects.kStealHealth or
                kSpellEffects.kDamageOverTime or kSpellEffects.kHeal or kSpellEffects.kHealOverTime) {
                ScaleParameter(value, "m_effectParam", effect.m_effectParam, multiplier);
                ScaleParameter(value, "m_paramPerRound", effect.m_paramPerRound, multiplier);
            }
            else if (modifiers && effect.m_effectType is kSpellEffects.kModifyOutgoingDamage or
                     kSpellEffects.kModifyIncomingDamage or kSpellEffects.kModifyAccuracy or
                     kSpellEffects.kModifyOutgoingHeal or kSpellEffects.kModifyIncomingHeal) {
                int amount = (int)Math.Clamp(Math.Round(effect.m_effectParam * multiplier), -95, 200);
                if (amount == 0 && effect.m_effectParam != 0) {
                    amount = Math.Sign(effect.m_effectParam);
                }
                BindProperty parameter = value.Get<SpellEffect>("m_effectParam");
                value.Set<SpellEffect>("m_effectParam", parameter with { Data = BitConverter.GetBytes(amount) });
            }
            else if (modifiers && effect.m_effectType is kSpellEffects.kAbsorbDamage or
                     kSpellEffects.kModifyOutgoingHealFlat or kSpellEffects.kModifyIncomingHealFlat) {
                ScaleParameter(value, "m_effectParam", effect.m_effectParam, multiplier);
            }
        }

        return BindDocument.WriteVector(property, values);
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

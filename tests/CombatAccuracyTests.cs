using System.Reflection;
using System.Runtime.CompilerServices;
using Imcodec.Cryptography;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.CoreLib.Game.Combat;

internal static class CombatAccuracyTests {
    internal static int Run(Assembly assembly) {
        Near(75, CombatAccuracy.FromStats(80, -0.05f, 0));
        Near(85, CombatAccuracy.FromStats(80, 0.05f, 0));
        Near(65, CombatAccuracy.FromStats(70, 0.10f, 0.15f));
        Near(79.9, CombatAccuracy.FromStats(80, -0.001f, 0));
        Near(110, CombatAccuracy.FromStats(90, 0.20f, 0));
        Near(80, CombatAccuracy.FromStats(90, 0.20f, 0) - 30);
        int checks = 6;
        foreach (double chance in new[] { -30d, 0, 1, 55, 65, 75, 79.9, 85, 99, 100, 150 }) {
            int hits = Enumerable.Range(0, 10000).Count(roll => CombatAccuracy.Hits(chance, (roll + 0.5) / 10000d));
            Require(hits == (int)Math.Round(Math.Clamp(chance, 0, 100) * 100), "Accuracy and hit rate disagree.");
            Require(!CombatAccuracy.Hits(chance, -0.001) && !CombatAccuracy.Hits(chance, 1), "Invalid accuracy roll accepted.");
            checks += 2;
        }
        Require(!CombatAccuracy.Hits(0, 0), "A zero-accuracy spell could hit.");
        Require(CombatAccuracy.Hits(100, 0.999999999), "A guaranteed spell could fizzle.");
        checks += VerifyResolver(assembly);
        Console.WriteLine($"Passed {checks + 2} accuracy checks: penalties, bonuses, fractional odds, and zero/100% boundaries.");
        return checks + 2;
    }

    private static int VerifyResolver(Assembly assembly) {
        uint school = StringHash.Compute("Fire");
        var circle = (CombatDuelSubCircle)RuntimeHelpers.GetUninitializedObject(typeof(CombatDuelSubCircle));
        WizGameStats stats = new() { m_accBonusPercentAll = -0.05f };
        SpellEffect firePenalty = new() { m_effectType = kSpellEffects.kModifyAccuracy, m_sDamageType = "Fire", m_damageType = school, m_effectParam = -20 };
        SpellEffect universalBonus = new() { m_effectType = kSpellEffects.kModifyAccuracy, m_sDamageType = "All", m_effectParam = 5 };
        SpellEffect otherSchool = new() { m_effectType = kSpellEffects.kModifyAccuracy, m_sDamageType = "Ice", m_effectParam = -90 };
        CombatParticipant participant = new() { m_pGameStats = stats, m_hangingEffects = [firePenalty, universalBonus, otherSchool] };
        typeof(CombatDuelSubCircle).GetProperty("CombatParticipant", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(circle, participant);
        System.Type resolver = assembly.GetType("Imlight.CoreLib.Game.Combat.CombatResolver")!;
        MethodInfo consume = resolver.GetMethod("ConsumeHangingAccuracyEffects", BindingFlags.Static | BindingFlags.NonPublic)!;
        Near(65, (double)consume.Invoke(null, [80d, circle, school])!);
        Require(participant.m_hangingEffects.Count == 1 && participant.m_hangingEffects[0] == otherSchool,
            "Accuracy effects were not consumed once for the matching school.");
        participant.m_hangingEffects.Clear();
        MethodInfo hits = resolver.GetMethod("SpellHits", BindingFlags.Static | BindingFlags.NonPublic)!;
        Spell card = new() { m_magicSchoolID = school, m_accuracy = 80 };
        int landed = Enumerable.Range(0, 2000).Count(_ => (bool)hits.Invoke(null, [circle, card])!);
        Require(landed > 1300 && landed < 1700, "A small negative modifier made nearly every cast fizzle.");
        card.m_accuracy = 100;
        stats.m_accBonusPercentAll = 0;
        Require(Enumerable.Range(0, 100).All(_ => (bool)hits.Invoke(null, [circle, card])!), "Guaranteed casts fizzled in the resolver.");
        card.m_accuracy = 0;
        Require(Enumerable.Range(0, 100).All(_ => !(bool)hits.Invoke(null, [circle, card])!), "Zero-accuracy casts hit in the resolver.");
        return 5;
    }

    private static void Near(double expected, double actual) {
        Require(Math.Abs(expected - actual) < 0.00001, $"Expected accuracy {expected}, received {actual}.");
    }

    private static void Require(bool condition, string message) {
        if (!condition) { throw new InvalidDataException(message); }
    }
}

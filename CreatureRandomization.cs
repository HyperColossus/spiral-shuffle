using System;
using System.Collections.Generic;
using Imlight.CoreLib.Shared.Behaviors;

namespace Imlight.CoreLib.Game;

internal sealed record CreatureRoll(double Health, float Damage, float Resistance, float Accuracy, float Rating) {
    internal int ScaleHealth(int health) => (int)Math.Clamp(Math.Round(health * Health), 1, int.MaxValue);
}


internal static class CreatureRandomization {
    internal static int Difficulty(int mode) => (Math.Clamp(mode, 1, 6) - 1) % 3 + 1;

    internal static string RealmName(int mode) => mode switch {
        2 => "Chaos",
        3 => "Nightmare",
        4 => "ShuffleCreatures",
        5 => "ChaosCreatures",
        6 => "NightmareCreatures",
        _ => "Shuffle"
    };

    internal static int ModeForPath(string actorPath, int basePort) {
        for (int mode = 1; mode <= 6; mode++) {
            if (actorPath.Contains($"/Imlight.Game.{RealmName(mode)}/", StringComparison.Ordinal) ||
                actorPath.Contains($"/Imlight.Game.{RealmName(mode)}.{basePort + mode - 1}/", StringComparison.Ordinal) ||
                actorPath.Contains($"/Imlight.Game.{basePort + mode - 1}/", StringComparison.Ordinal)) {
                return mode;
            }
        }

        return 1;
    }

    internal static CreatureRoll Create(int mode, Random random) {
        int difficulty = Difficulty(mode);
        (double healthMin, double healthMax, double baseline, float damageMin, float damageMax,
            float resistanceMax, float accuracyMax, float ratingRange) = difficulty switch {
            3 => (0.50, 1.75, 2.25, 0.50f, 1.85f, 0.20f, 0.15f, 0.50f),
            2 => (0.65, 1.50, 1.40, 0.65f, 1.50f, 0.12f, 0.10f, 0.35f),
            _ => (0.80, 1.20, 1.00, 0.80f, 1.20f, 0.05f, 0.05f, 0.20f)
        };
        return new CreatureRoll(
            (healthMin + random.NextDouble() * (healthMax - healthMin)) * baseline,
            Between(random, damageMin, damageMax),
            Between(random, 0, resistanceMax),
            Between(random, -accuracyMax, accuracyMax),
            Between(random, 1 - ratingRange, 1 + ratingRange));
    }

    internal static void Apply(ServerWizGameStats stats, CreatureRoll roll) {
        stats.m_dmgBonusPercentAll = (1 + stats.m_dmgBonusPercentAll) * roll.Damage - 1;
        Scale(stats.m_dmgBonusPercent, roll.Damage);
        float resistance = stats.m_dmgReducePercentAll + roll.Resistance;
        if (stats.m_dmgReducePercent is { } reductions) {
            for (int index = 0; index < reductions.Count; index++) {
                reductions[index] = Math.Clamp(reductions[index] + resistance, -1f, 0.95f) -
                    Math.Clamp(resistance, -1f, 0.95f);
            }
        }

        stats.m_dmgReducePercentAll = Math.Clamp(resistance, -1f, 0.95f);
        stats.m_accBonusPercentAll += roll.Accuracy;
        stats.m_criticalHitRatingAll *= roll.Rating;
        Scale(stats.m_criticalHitRatingBySchool, roll.Rating);
        stats.m_blockRatingAll *= roll.Rating;
        Scale(stats.m_blockRatingBySchool, roll.Rating);
    }

    private static float Between(Random random, float minimum, float maximum) =>
        minimum + (float)random.NextDouble() * (maximum - minimum);

    private static void Scale(List<float>? values, float factor) {
        if (values is null) {
            return;
        }

        for (int index = 0; index < values.Count; index++) {
            values[index] *= factor;
        }
    }
}

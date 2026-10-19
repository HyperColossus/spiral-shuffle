using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;

namespace Imlight.CoreLib.Game;

internal sealed record DropSpell(uint Original, uint[] Variants);

internal static class SpellDropRules {
    internal const int Scale = 1_000_000;
    internal const int DefaultSetting = -100;
    internal const int FixedOffset = 100000;

    internal static bool IsValidSetting(int setting) {
        long value = Math.Abs((long)setting);
        return value is >= 10 and <= 10000 or >= FixedOffset + 10 and <= FixedOffset + 10000;
    }

    internal static int Rate(int setting) => (int)(Math.Abs((long)setting) % FixedOffset);

    internal static bool IsFixed(int setting) => Math.Abs((long)setting) >= FixedOffset;

    internal static double NewEncounterUnit() => RandomNumberGenerator.GetInt32(Scale) / (double)Scale;

    internal static int Chance(int setting, double encounterUnit) {
        if (setting <= 0 || !IsValidSetting(setting)) {
            return 0;
        }

        int maximum = Rate(setting) * 100;
        if (IsFixed(setting)) {
            return maximum;
        }

        double unit = Math.Clamp(encounterUnit, 0, 1);
        return (int)Math.Round(1000 + (maximum - 1000) * unit * unit * unit);
    }

    internal static bool Wins(int chance, int roll) => chance > 0 && roll >= 0 && roll < Math.Min(chance, Scale);

    internal static string Percent(int chance)
        => (chance / 10000.0).ToString("0.####", CultureInfo.InvariantCulture) + "%";

    internal static uint[] Available(IReadOnlyList<DropSpell> pool, ISet<uint> learned, int difficulty)
        => pool.Where(spell => spell.Variants.Length == 3 && !learned.Contains(spell.Original) &&
                !spell.Variants.Any(learned.Contains))
            .Select(spell => spell.Variants[Math.Clamp(difficulty, 1, 3) - 1]).Distinct().ToArray();
}

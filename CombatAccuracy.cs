using System;

namespace Imlight.CoreLib.Game.Combat;

internal static class CombatAccuracy {
    internal static double FromStats(int cardAccuracy, float bonus, float reduction)
        => cardAccuracy + 100d * bonus - 100d * reduction;

    internal static bool Hits(double accuracy, double roll)
        => roll >= 0 && roll < 1 && roll < Math.Clamp(accuracy, 0, 100) / 100;
}

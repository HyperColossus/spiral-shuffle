using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;

namespace Imlight.CoreLib.Game;

internal sealed record SpellDropRewards(uint[] SpellIds, int Difficulty);

internal sealed record RewardSpell(uint TemplateId, string DisplayName, string School, string Cost, int Accuracy, int Difficulty);

internal sealed record SpellRewardNotice(int Token, RewardSpell Spell, int Number, int Total) {
    internal string Payload => string.Join("|", "SpellReward", Token, Spell.TemplateId,
        Uri.EscapeDataString(Spell.DisplayName), Uri.EscapeDataString(Spell.School), Uri.EscapeDataString(Spell.Cost),
        Spell.Accuracy, Math.Clamp(Spell.Difficulty, 1, 3), Number, Total);
}

internal sealed class SpellRewardQueue {
    private readonly Queue<SpellRewardNotice> _pending = new();

    internal SpellRewardNotice? Current => _pending.TryPeek(out SpellRewardNotice? notice) ? notice : null;

    internal void Enqueue(IReadOnlyList<RewardSpell> spells) {
        RewardSpell[] unique = spells.DistinctBy(spell => spell.TemplateId).ToArray();
        for (int index = 0; index < unique.Length; index++) {
            _pending.Enqueue(new(RandomNumberGenerator.GetInt32(1, int.MaxValue), unique[index], index + 1, unique.Length));
        }
    }

    internal bool Acknowledge(int token) {
        if (Current?.Token != token) {
            return false;
        }

        _pending.Dequeue();
        return true;
    }

    internal static string PipCost(int pips, bool variable, int shadowPips, IReadOnlyDictionary<string, int> schoolPips) {
        List<string> costs = [variable ? "X" : pips.ToString(CultureInfo.InvariantCulture)];
        if (shadowPips > 0) { costs.Add(shadowPips.ToString(CultureInfo.InvariantCulture) + " shadow"); }
        costs.AddRange(schoolPips.Where(pair => pair.Value > 0)
            .Select(pair => pair.Value.ToString(CultureInfo.InvariantCulture) + " " + pair.Key));
        return string.Join(" + ", costs);
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Math;
using Imlight.CoreLib.Game.Combat;
using Imlight.CoreLib.Game.Spells;
using Imlight.CoreLib.Game.Effects;
using Imlight.CoreLib.Shared.Behaviors;
using Imlight.CoreLib.Shared.Networking;

namespace Imlight.CoreLib.Game;

internal sealed record EnemyInspectorQuery(ulong PlayerId, int SpellDropSetting = 0) : IServerMessage {
    public byte ServiceID => 102;
    public byte MessageOrder => 240;
}


internal sealed record EnemyInspectorSnapshot(string Payload);


internal static class EnemyInspectorData {
    private static readonly MagicSchool[] Schools = [MagicSchool.Fire, MagicSchool.Ice, MagicSchool.Storm,
        MagicSchool.Myth, MagicSchool.Life, MagicSchool.Death, MagicSchool.Balance, MagicSchool.Shadow];

    internal static string PlanningCamera(Vector3 center, float yaw, int playerSlot) => string.Join("|", "Planning",
        Number(center.X), Number(center.Y), Number(center.Z), yaw.ToString("R", CultureInfo.InvariantCulture),
        playerSlot >= 4 ? "1" : "-1");

    internal static string Snapshot(CoreObject creature, string name, MagicSchool school, int level,
        ServerWizGameStats stats, int spellDropChance = 0) {
        string identity = string.Join("|", ((ulong)creature.m_globalID).ToString(CultureInfo.InvariantCulture), Number(creature.m_location.X),
            Number(creature.m_location.Y), Number(creature.m_location.Z), name.Replace("|", " "),
            school.ToString(), level.ToString(CultureInfo.InvariantCulture),
            stats.m_currentHitpoints.ToString(CultureInfo.InvariantCulture),
            stats.m_baseHitpoints.ToString(CultureInfo.InvariantCulture));
        string[] rows = Schools.Select(value => string.Join(",", value.ToString(),
            Number(100 * Total(stats.m_dmgBonusPercent, stats.m_dmgBonusPercentAll, value)),
            Number(Total(stats.m_dmgBonusFlat, stats.m_dmgBonusFlatAll, value)),
            Number(100 * Total(stats.m_dmgReducePercent, stats.m_dmgReducePercentAll, value)),
            Number(Total(stats.m_dmgReduceFlat, stats.m_dmgReduceFlatAll, value)),
            Number(100 * Total(stats.m_accBonusPercent, stats.m_accBonusPercentAll, value)),
            Number(100 * Total(stats.m_apBonusPercent, stats.m_apBonusPercentAll, value)),
            Number(Total(stats.m_criticalHitRatingBySchool, stats.m_criticalHitRatingAll, value)),
            Number(Total(stats.m_blockRatingBySchool, stats.m_blockRatingAll, value)),
            Number(100 * CriticalChance(Total(stats.m_criticalHitRatingBySchool, stats.m_criticalHitRatingAll, value), level)),
            Number(100 * CriticalChance(Total(stats.m_blockRatingBySchool, stats.m_blockRatingAll, value), level)))).ToArray();
        return "Snapshot|" + identity + "|" + string.Join(";", rows) + "|" + string.Join(",",
            Number(100 * (stats.m_powerPipBase + stats.m_powerPipBonusPercentAll)),
            Number(100 * stats.m_stunResistancePercent), Number(100 * stats.m_healBonusPercentAll),
            Number(100 * stats.m_healIncBonusPercentAll), Number(stats.m_shadowPipRating)) + "|" +
            (spellDropChance > 0 ? SpellDropRules.Percent(spellDropChance) : "OFF");
    }

    private static float Total(List<float>? values, float universal, MagicSchool school) {
        int index = (int)MagicSchools.GetMagicSchool(school.ToString()).m_schoolIndex;
        return universal + (values is not null && index < values.Count ? values[index] : 0);
    }

    private static float CriticalChance(float rating, int level) =>
        level < WizStatisticEffectConfigLoader.GetCriticalHitLevelThreshold() ? 0 : CombatCriticals.RatingToChance(rating, level);

    private static string Number(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);
}

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Akka.Actor;
using Imcodec.MessageLayer.Generated;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.Common;
using Imlight.CoreLib.Game.Combat;
using Imlight.CoreLib.Game.Zone.Core;
using Imlight.CoreLib.Shared.Behaviors;
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.Shared.Resources;
using Type = System.Type;

internal static class CreatureTurnTests {
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static int _checks;

    internal static int Run(Assembly assembly) {
        var cache = (ConcurrentDictionary<ulong, CoreTemplate>)typeof(CoreObjectFactory).GetField("s_templateCache", Static)!.GetValue(null)!;
        const uint attackId = 3_900_000_001;
        const uint buffId = attackId + 1;
        const uint healId = attackId + 2;
        cache[attackId] = new SpellTemplate { m_effects = [new EffectListSpellEffect {
            m_effectList = [new RandomSpellEffect { m_effectList = [new SpellEffect {
                m_effectType = kSpellEffects.kStealHealth, m_effectTarget = kEffectTarget.kEnemySingle }] }] }] };
        cache[buffId] = new SpellTemplate { m_effects = [new SpellEffect {
            m_effectType = kSpellEffects.kModifyOutgoingDamage, m_effectTarget = kEffectTarget.kFriendlySingle }] };
        cache[healId] = new SpellTemplate { m_effects = [new SpellEffect {
            m_effectType = kSpellEffects.kHeal, m_effectTarget = kEffectTarget.kFriendlySingle }] };
        try {
            ZoneEntity entity = (ZoneEntity)RuntimeHelpers.GetUninitializedObject(typeof(ZoneEntity));
            Set(entity, "ActiveGameObject", new CoreObject { m_globalID = 77UL });
            Set(entity, "SelfRef", ActorRefs.Nobody);
            Type duelType = assembly.GetType("Imlight.CoreLib.Game.Zone.Components.CombatDuelComponent")!;
            object duel = Activator.CreateInstance(duelType, Instance, null, [entity], null)!;
            CombatDuelSubCircle[] circles = Enumerable.Range(0, 8).Select(slot => Circle(slot)).ToArray();
            Set(circles[0], "ParticipantObject", new CoreObject { m_templateID = 2U });
            Set(circles[3], "ParticipantObject", new CoreObject { m_templateID = 3U });
            Set(circles[4], "ParticipantObject", new CoreObject { m_templateID = 1U });
            Set(duel, "SubCircles", circles);
            Type aiType = assembly.GetType("Imlight.CoreLib.Game.Zone.Components.CombatCreatureAIComponent")!;
            object ai = Activator.CreateInstance(aiType, Instance, null, [entity], null)!;
            Field(ai, "_currentDuelComponent", duel);
            Field(ai, "_currentSubCircle", circles[3]);
            Require((float)aiType.GetField("_healingThreshold", Instance)!.GetValue(ai)! == 0.75f,
                "The legacy healing threshold setting was ignored.");
            Field(ai, "_healingPercentChance", 1f);
            aiType.GetMethod("InitiatizeHateTable", Instance)!.Invoke(ai, null);
            aiType.GetMethod("InitiatizeHateTable", Instance)!.Invoke(ai, null);
            Spell attack = Card(attackId, 1);
            Field(ai, "_roundHand", new Hand { m_spellList = [attack] });
            MethodInfo turn = aiType.GetMethod("DetermineTurnAction", Instance)!;
            foreach (bool aggressive in new[] { true, false }) {
                Field(ai, "_determinedAggressiveThisTurn", aggressive);
                for (int index = 0; index < 100; index++) {
                    var move = (COMBAT_106_PROTOCOL.MSG_ACTORCOMBATMOVE)turn.Invoke(ai, null)!;
                    Require(move.MoveType == (byte)CombatMoveType.Attack && move.SpellSelection == 0 && move.SpellTarget == 4,
                        "A creature passed with a usable nested drain, or targeted the wrong team.");
                }
            }
            attack.m_pipCost.m_spellRank = 4;
            Require(((COMBAT_106_PROTOCOL.MSG_ACTORCOMBATMOVE)turn.Invoke(ai, null)!).MoveType == (byte)CombatMoveType.Pass,
                "A creature cast a spell it could not afford.");
            Field(ai, "_roundHand", new Hand { m_spellList = [Card(buffId, 0)] });
            Field(ai, "_determinedSelfishThisTurn", false);
            for (int index = 0; index < 100; index++) {
                var move = (COMBAT_106_PROTOCOL.MSG_ACTORCOMBATMOVE)turn.Invoke(ai, null)!;
                Require(move.MoveType == (byte)CombatMoveType.Attack && move.SpellSelection == 0 && move.SpellTarget is 0 or 3,
                    "A creature passed with a buff available, or offset the friendly target slot.");
            }
            Stats(circles[0]).m_currentHitpoints = 20;
            Field(ai, "_roundHand", new Hand { m_spellList = [Card(healId, 0)] });
            var healing = (COMBAT_106_PROTOCOL.MSG_ACTORCOMBATMOVE)turn.Invoke(ai, null)!;
            Require(healing.MoveType == (byte)CombatMoveType.Attack && healing.SpellTarget == 0,
                "A heal selected a healthy ally or used a one-based slot.");
            Set(circles[3], "CombatParticipant", new CombatParticipant { m_stunned = 1, m_pipCount = new PipCount { m_genericPips = 1 } });
            Require(((COMBAT_106_PROTOCOL.MSG_ACTORCOMBATMOVE)turn.Invoke(ai, null)!).MoveType == (byte)CombatMoveType.Pass,
                "A stunned creature acted.");
            Console.WriteLine($"Passed {_checks} creature turn checks: nested drains, affordable casts, buffs, healing and slot three targeting.");
            return _checks;
        }
        finally {
            foreach (uint id in new[] { attackId, buffId, healId }) {
                cache.TryRemove(id, out _);
            }
        }
    }

    private static CombatDuelSubCircle Circle(int slot) {
        var circle = (CombatDuelSubCircle)RuntimeHelpers.GetUninitializedObject(typeof(CombatDuelSubCircle));
        Set(circle, "SlotIndex", slot);
        Set(circle, "ParticipantGameStats", new ServerWizGameStats(MagicSchool.Death, 1) { m_currentHitpoints = 100, m_baseHitpoints = 100 });
        Set(circle, "CombatParticipant", new CombatParticipant { m_pipCount = new PipCount { m_genericPips = 1 } });
        return circle;
    }

    private static ServerWizGameStats Stats(CombatDuelSubCircle circle) =>
        (ServerWizGameStats)typeof(CombatDuelSubCircle).GetProperty("ParticipantGameStats", Instance)!.GetValue(circle)!;

    private static Spell Card(uint id, byte pips) => new() { m_templateID = id, m_magicSchoolID = (uint)MagicSchool.Death,
        m_pipCost = new SpellRank { m_spellRank = pips } };

    private static void Set(object target, string name, object value) => target.GetType().GetProperty(name, Instance)!.SetValue(target, value);

    private static void Field(object target, string name, object value) => target.GetType().GetField(name, Instance)!.SetValue(target, value);

    private static void Require(bool condition, string message) {
        if (!condition) { throw new InvalidDataException(message); }
        _checks++;
    }
}

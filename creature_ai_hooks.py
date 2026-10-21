"""Keep creature turns actionable and use the duel's zero-based target slots."""
from collections.abc import Callable


def patch_creature_ai(replace: Callable[[str, str, str], None]) -> None:
    path = "Game/Zone/Components/CombatCreatureAIComponent.cs"
    replace(path, "using System.Collections.Generic;", "using System.Collections.Generic;\nusing System.Globalization;")
    replace(path, 'ConfigurationManager.Settings["Combat.HealingThreshold"].AsFloat()',
            'float.Parse(ConfigurationManager.GetSetting("Combat.HealingThreshold") ??\n'
            '        ConfigurationManager.GetSetting("Combat.HealingThreshhold") ?? "0.75", CultureInfo.InvariantCulture)')
    replace(path, '    private readonly float _preparePassChance    = ConfigurationManager.Settings["Combat.PreparePassChance"].AsFloat();\n', "")
    replace(path, "x => x.OccupiedTeam == _currentSubCircle.OccupiedTeam", "x => x.IsAlive && x.OccupiedTeam == _currentSubCircle.OccupiedTeam")
    for prefix in ["_friendlySubcircles.Any(x => ", ".OrderBy(x => "]:
        replace(path, prefix + "x.ParticipantGameStats.m_currentHitpoints / x.ParticipantGameStats.m_baseHitpoints",
                prefix + "(double)x.ParticipantGameStats.m_currentHitpoints / Math.Max(1, x.ParticipantGameStats.m_baseHitpoints)")
    replace(path, "_stats.Stats.m_currentHitpoints / _stats.Stats.m_baseHitpoints",
            "(double)_currentSubCircle.ParticipantGameStats.m_currentHitpoints / Math.Max(1, _currentSubCircle.ParticipantGameStats.m_baseHitpoints)")
    for comment in ["There's a very low chance that we just pass on a prepare turn.",
                    "We have buffs or debuffs available. There's a small chance we pass anyway."]:
        trailing = "            \n" if comment.startswith("There's") else ""
        replace(path, f"""        // {comment}
        if (_random.NextDouble() < _preparePassChance) {{
            Logger.Debug("Duel {{0}} | Slot {{1}} | Preparing, but passing.",
                Logger.Args(_currentDuelComponent.SigilId, _currentSubCircle.SlotIndex));

            msg.MoveType = (byte) CombatMoveType.Pass;
{trailing}            return msg;
        }}

""", "")
    replace(path, "if (coinFlip < 0.5 && buffSpells.Count > 0)",
            "if (buffSpells.Count > 0 && (debuffSpells.Count == 0 || coinFlip < 0.5))")
    replace(path, "(randomTeammate.SlotIndex + 1)", "randomTeammate.SlotIndex")
    replace(path, "(lowestHealthTeammate.SlotIndex + 1)", "lowestHealthTeammate.SlotIndex")
    replace(path, "        // Create the hate table.\n", "        _hateTable.Clear();\n")
    for variable in ["start", "end"]:
        replace(path, f"int {variable} = (_currentSubCircle.SlotIndex < 3)",
                f"int {variable} = (_currentSubCircle.SlotIndex < _currentDuelComponent.SubCircles.Length / 2)")
    replace(path, """        // Otherwise, choose a random healing spell.
        var randomIdx = _random.Next(castableHealingSpells.Count);
        var selectedSpell = castableHealingSpells[randomIdx];
        msg.SpellSelection = (byte) _roundHand.m_spellList.IndexOf(selectedSpell);""", """        if (!_determinedSmartThisTurn) {
            var selectedSpell = castableHealingSpells[_random.Next(castableHealingSpells.Count)];
            msg.SpellSelection = (byte)_roundHand.m_spellList.IndexOf(selectedSpell);
        }""")
    replace("Game/Zone/Components/CombatDuelComponent.cs", """            CombatResolver.AddCombatMove(CombatMoveType.Pass, caster, null, null);
        }

        // Find the target sub circle.""", """            CombatResolver.AddCombatMove(CombatMoveType.Pass, caster, null, null);
            return;
        }

        // Find the target sub circle.""")

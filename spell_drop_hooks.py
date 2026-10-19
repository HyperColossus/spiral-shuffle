"""Connect spell-drop settings, combat rewards and enemy inspection."""
from collections.abc import Callable


def patch_spell_drops(replace: Callable[[str, str, str], None]) -> None:
    replace("Game/Services/CombatService.cs", "using System;", "using System;\nusing System.Linq;")
    replace("Login/Services/CharacterService.cs", """            Imlight.CoreLib.Game.VendorRandomization.SetEnabled(account.AccountId, vendors);
        }
""", """            Imlight.CoreLib.Game.VendorRandomization.SetEnabled(account.AccountId, vendors);
        }
        else if (account is not null && message.JobID == Imlight.CoreLib.Game.RandomizerRuntime.MenuJobId &&
                 Imlight.CoreLib.Game.SpellDrops.HandleMenuSetting(account.AccountId, message.Message, out var response)) {
            SendToSocket(new GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS {
                JobID = Imlight.CoreLib.Game.RandomizerRuntime.MenuJobId, Message = response
            });
        }
""")
    replace("Game/Combat/CombatDuelSubCircle.cs", "    internal ServerWizGameStats ParticipantGameStats { get; private set; }",
            "    internal ServerWizGameStats ParticipantGameStats { get; private set; }\n    internal double SpellDropUnit { get; private set; }")
    replace("Game/Combat/CombatDuelSubCircle.cs", "        ParticipantGameStats = creatureStats.GameStats;",
            "        SpellDropUnit = Imlight.CoreLib.Game.SpellDropRules.NewEncounterUnit();\n        ParticipantGameStats = creatureStats.GameStats;")
    replace("Shared/Packets/COMBAT_106_PROTOCOL.cs", "        public ulong[] MobTemplateIds;",
            "        public ulong[] MobTemplateIds;\n        public double[] SpellDropUnits = [];")
    replace("Game/Zone/Components/CombatDuelComponent.cs", "        var templateIdsOfDefeatedMobs = new List<ulong>();",
            "        var templateIdsOfDefeatedMobs = new List<ulong>();\n        var spellDropUnits = new List<double>();")
    replace("Game/Zone/Components/CombatDuelComponent.cs", "                templateIdsOfDefeatedMobs.Add(gameObjectTemplate.m_templateID);",
            "                templateIdsOfDefeatedMobs.Add(gameObjectTemplate.m_templateID);\n                spellDropUnits.Add(circle.SpellDropUnit);")
    replace("Game/Zone/Components/CombatDuelComponent.cs", "                MobTemplateIds = [.. templateIdsOfDefeatedMobs],",
            "                MobTemplateIds = [.. templateIdsOfDefeatedMobs],\n                SpellDropUnits = [.. spellDropUnits],")
    replace("Game/Services/CombatService.cs", "GrantMobLoot(message.MobTemplateIds);",
            "GrantMobLoot(message.MobTemplateIds, message.SpellDropUnits);")
    replace("Game/Services/CombatService.cs", "private void GrantMobLoot(ulong[] defeatedMobTemplateIds)",
            "private void GrantMobLoot(ulong[] defeatedMobTemplateIds, double[] encounterUnits)")
    replace("Game/Services/CombatService.cs", """        foreach (var templateId in defeatedMobTemplateIds) {
            if (!NpcDropTableCollection.TryGetDropTable(templateId, out var npcDropTable)) {
                continue;
            }

            var loot = DropTableRoller.Roll(npcDropTable.DropTableNames.ToArray(), playerRef, playerObj, wizard);
            LootGranter.GrantAndDisplay(playerRef, wizard, loot);
        }""", """        var learnedSpells = new System.Collections.Generic.List<uint>();
        for (int index = 0; index < defeatedMobTemplateIds.Length; index++) {
            var loot = NpcDropTableCollection.TryGetDropTable(defeatedMobTemplateIds[index], out var npcDropTable)
                ? DropTableRoller.Roll(npcDropTable.DropTableNames.ToArray(), playerRef, playerObj, wizard)
                : new Imlight.CoreLib.WizardData.Models.World.DropTableResult();
            double unit = index < encounterUnits.Length ? encounterUnits[index]
                : Imlight.CoreLib.Game.SpellDropRules.NewEncounterUnit();
            Imlight.CoreLib.Game.SpellDrops.AddCreatureDrop(wizard, loot, unit);
            LootGranter.GrantAndDisplay(playerRef, wizard, loot);
            learnedSpells.AddRange(loot.SpellIds.Where(id =>
                Imlight.CoreLib.Game.RandomizerRuntime.DropSpellPool.Any(spell => spell.Variants.Contains(id))));
        }
        if (learnedSpells.Count > 0) {
            playerRef.Tell(new Imlight.CoreLib.Game.SpellDropRewards([.. learnedSpells],
                Imlight.CoreLib.Game.CreatureRandomization.Difficulty(
                    Imlight.CoreLib.Game.RandomizerRuntime.GetMode(wizard.AccountId))));
        }""")
    replace("Game/Zone/Components/CombatDuelComponent.cs", "                    circle.CombatParticipant.m_mobLevel, circle.ParticipantGameStats)));",
            """                    circle.CombatParticipant.m_mobLevel, circle.ParticipantGameStats,
                    Imlight.CoreLib.Game.SpellDropRules.Chance(query.SpellDropSetting, circle.SpellDropUnit))));""")

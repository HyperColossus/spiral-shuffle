"""Prepare an isolated server build with the native randomizer menu hooks."""
import shutil
from pathlib import Path

from vendor_hooks import patch_vendor_hooks
from spell_drop_hooks import patch_spell_drops
from item_card_hooks import patch_item_cards
from mana_hooks import patch_infinite_mana
from spell_number_hooks import patch_spell_numbers
from creature_ai_hooks import patch_creature_ai
from trainer_hooks import patch_trainers
from qa_gear_hooks import patch_qa_gear


ROOT = Path(__file__).resolve().parent
ORIGINAL = ROOT / ".build/upstream/Imlight-b07987168fcfa2d123a5d78df90ca0c76405e427/src/Imlight.CoreLib"
SOURCE = ROOT / ".build/source"


def replace(relative: str, before: str, after: str) -> None:
    path = SOURCE / relative
    content = path.read_text(encoding="utf-8-sig")
    if content.count(before) != 1:
        raise ValueError(f"Expected exactly one hook in {relative}: {before[:60]}")
    path.write_text(content.replace(before, after).replace("\u2014", ","), encoding="utf-8")


def patch_quest_flow() -> None:
    replace("Game/Services/QuestService.cs", """    private void SendQuestResumeMessage(QuestTemplate qTemplate, QuestInstance qInstance) {
        var qMadLibs""", """    private void SendQuestResumeMessage(QuestTemplate qTemplate, QuestInstance qInstance) {
        if (qTemplate.m_isHidden) {
            return;
        }

        var qMadLibs""")
    replace("Game/Services/QuestService.cs", """        SendToSocket(qSendMsg);
        ShowQuestStartDialogue(qTemplate);""", """        if (!qTemplate.m_isHidden) {
            SendToSocket(qSendMsg);
        }
        ShowQuestStartDialogue(qTemplate);""")
    replace("Game/Services/QuestService.cs", """    private void SendGoalMessage(GoalTemplate gTemplate, QuestInstance qInstance, byte sendType = 0, bool forceSendDestZone = true) {
        var gInstance""", """    private void SendGoalMessage(GoalTemplate gTemplate, QuestInstance qInstance, byte sendType = 0, bool forceSendDestZone = true) {
        if (GetQuestTemplate(qInstance.QuestName)?.m_isHidden == true) {
            return;
        }

        var gInstance""")
    replace("Game/Services/TutorialService.cs", """        if (!IsTutorialZone(wizard.Zone)) {
            return;
        }

        if (!_serializer.Serialize(_tutorialInfo,""", """        if (!IsTutorialZone(wizard.Zone)) {
            if (wizard.HasQuest(TUTORIAL_INTRO_QUEST_NAME)) {
                CompleteTutorialIntro(wizard, GetActiveGameObject());
            }

            return;
        }

        if (!_serializer.Serialize(_tutorialInfo,""")
    replace("Game/Services/TutorialService.cs", """            RemoveControlQuests(wizard);
            CompleteTutorialIntro(wizard, playerObj);
            EquipStarterWandAndDeck(wizard);""", """            if (!CompleteTutorialIntro(wizard, playerObj)) {
                return false;
            }

            RemoveControlQuests(wizard);
            EquipStarterWandAndDeck(wizard);""")
    replace("Game/Services/TutorialService.cs", """        if (goalName == "Teleport") {
            RemoveControlQuests(wizard);""", """        if (goalName == "Teleport") {
            if (!CompleteTutorialIntro(wizard, playerObj)) {
                return false;
            }

            RemoveControlQuests(wizard);""")
    replace("Game/Services/TutorialService.cs", """    private bool CompleteTutorialIntro(Wizard wizard, CoreObject playerObj) {
        // Skippers""", """    private bool CompleteTutorialIntro(Wizard wizard, CoreObject playerObj) {
        if (wizard.HasCompletedQuest(TUTORIAL_INTRO_QUEST_NAME)) {
            if (wizard.HasQuest(TUTORIAL_INTRO_QUEST_NAME)) {
                RemoveQuestAndClearFromJournal(wizard, TUTORIAL_INTRO_QUEST_NAME);
            }

            return true;
        }

        // Skippers""")
    replace("Game/Services/TutorialService.cs", """        var removeSuccess = wizard.CompleteQuestGoal(TUTORIAL_INTRO_QUEST_NAME, TUTORIAL_INTRO_GOAL_NAME);

        var questTemplate""", """        var questTemplate""")
    replace("Game/Services/TutorialService.cs", """        ResultDispatcher.ExecuteResults(
            actorContext: Context,
            results: goalTemplate.m_completeResults,
            playerRef: SessionActor.ActorRef,
            playerObj: playerObj,
            questName: TUTORIAL_INTRO_QUEST_NAME,
            goalName: TUTORIAL_INTRO_GOAL_NAME,
            zoneActor: SessionActor.GetZoneActor()
        );

        return removeSuccess;""", """        if (!goalInstance.IsGoalCompleted()) {
            if (!wizard.CompleteQuestGoal(TUTORIAL_INTRO_QUEST_NAME, TUTORIAL_INTRO_GOAL_NAME)) {
                return false;
            }

            ResultDispatcher.ExecuteResults(
                actorContext: Context,
                results: goalTemplate.m_completeResults,
                playerRef: SessionActor.ActorRef,
                playerObj: playerObj,
                questName: TUTORIAL_INTRO_QUEST_NAME,
                goalName: TUTORIAL_INTRO_GOAL_NAME,
                zoneActor: SessionActor.GetZoneActor()
            );
        }

        if (!wizard.CompleteQuest(TUTORIAL_INTRO_QUEST_NAME)) {
            return false;
        }

        SendToSocket(new QUEST_MESSAGES_52_PROTOCOL.MSG_REMOVEQUEST { QuestID = instance.ID });
        Logger.Information("Finished tutorial handoff for {0}.", Logger.Args(wizard.PlayerNameBehavior.GetWizardName()));
        return true;""")


def patch_world_state() -> None:
    path = "Game/Zone/Components/RenderComponent.cs"
    replace(path, """        var relevantDynaMods = wizard?.DynamodSet?.Dynamods?
            .Where(d => d.ClientTag.Equals(Entity.Info?.m_zoneTag, System.StringComparison.OrdinalIgnoreCase))
            .Where(d => string.IsNullOrEmpty(d.ZoneName)\x20
                     || d.ZoneName.Equals(Entity.Zone.ZoneData.m_zoneName, System.StringComparison.OrdinalIgnoreCase))
            .ToList() ?? [];
        string persistedState = null;""", """        var relevantDynaMods = GetPlayerDynamods(wizard);""")
    replace(path, """
            if (!mod.ModState.Equals(SPAWN_STATE_NAME, System.StringComparison.OrdinalIgnoreCase)) {
                persistedState = mod.ModState;
            }
""", "")
    replace(path, """            // A dynamod state such as "IdleOpen" is only ever sent as a state change, so a player
            // arriving in the zone has to be told again or the object reverts to its default.
            if (persistedState is not null) {
                Entity.ChangeStateExclusiveSender(persistedState, suspect);
            }

""", "")
    replace(path, """        if (CreateCurrentStateMessage() is { } stateMsg) {
            player.Tell(stateMsg);""", """        var wizard = _playersWithRequirementsMet.FirstOrDefault(pair => pair.Value.Equals(player)).Key;
        if (CreateCurrentStateMessage(wizard) is { } stateMsg) {
            player.Tell(stateMsg);""")
    replace(path, """    private GAME_5_PROTOCOL.MSG_ENTERSTATE CreateCurrentStateMessage() {
        var state = Entity.Zone.ScriptState.GetObjectState(Entity.Info?.m_zoneTag);""", """    private IEnumerable<Dynamod> GetPlayerDynamods(Wizard wizard)
        => MatchingDynamods(wizard?.DynamodSet?.Dynamods, Entity.Zone.ZonePath,
            Entity.Zone.ZoneData.m_zoneName, Entity.Info?.m_zoneTag);

    internal static IEnumerable<Dynamod> MatchingDynamods(IEnumerable<Dynamod> mods,
        string zonePath, string zoneName, string objectTag)
        => (mods ?? []).Where(mod => mod is not null
            && string.Equals(mod.ClientTag, objectTag, System.StringComparison.OrdinalIgnoreCase)
            && (string.IsNullOrEmpty(mod.ZoneName)
                || string.Equals(mod.ZoneName, zonePath, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(mod.ZoneName, zoneName, System.StringComparison.OrdinalIgnoreCase)));

    internal static string ResolvePlayerState(IEnumerable<Dynamod> mods, string fallback) {
        var mod = mods.LastOrDefault();
        return mod is null || string.Equals(mod.ModState, SPAWN_STATE_NAME, System.StringComparison.OrdinalIgnoreCase)
            ? fallback : mod.ModState;
    }

    private GAME_5_PROTOCOL.MSG_ENTERSTATE CreateCurrentStateMessage(Wizard wizard = null) {
        var state = ResolvePlayerState(GetPlayerDynamods(wizard),
            Entity.Zone.ScriptState.GetObjectState(Entity.Info?.m_zoneTag));""")


def patch_local_login() -> None:
    replace("WizardData/DatabaseUtilities.cs", """        if (!sucess) {
            return null;
        }

        return acc;""", """        if (!sucess) {
            if (Imlight.CoreLib.Game.RandomizerRuntime.Enabled &&
                Imlight.CoreLib.WizardData.Databases.PlayerDatabase.Instance.IsEmbedded &&
                username == "admin" && !AccountCollection.ChangePassword(username, plaintextPassword)) {
                throw new InvalidOperationException("Could not update the local randomizer login.");
            }
            return null;
        }

        return acc;""")


def patch_combat_accuracy() -> None:
    path = "Game/Combat/CombatResolver.cs"
    replace(path, "var spellAccuracy = (int) spell.m_accuracy;", "double spellAccuracy = spell.m_accuracy;")
    replace(path, """        // Convert to percentages for calculation
        var totalIncrease = (percentIncrease + percentIncreaseAll) * 100;
        var totalDecrease = (percentDecrease + percentDecreaseAll) * 100;

        // Apply percentages to the spell accuracy
        spellAccuracy *= (int) Math.Floor((1 + totalIncrease / 100.0) * (1 - totalDecrease / 100.0));""",
            """        spellAccuracy = CombatAccuracy.FromStats(spell.m_accuracy,
            percentIncrease + percentIncreaseAll, percentDecrease + percentDecreaseAll);""")
    replace(path, "var hitChance = Random.Shared.Next(0, 100);", "var hitChance = Random.Shared.NextDouble();")
    replace(path, "return hitChance <= spellAccuracy;", "return CombatAccuracy.Hits(spellAccuracy, hitChance);")
    replace(path, "private static int ConsumeHangingAccuracyEffects(int startingAccuracy,",
            "private static double ConsumeHangingAccuracyEffects(double startingAccuracy,")
    replace(path, "            .Where(x => x.m_damageType == magicSchoolId);", """            .Where(x => x.m_damageType == magicSchoolId ||
                string.Equals(x.m_sDamageType.ToString(), "All", StringComparison.OrdinalIgnoreCase)).ToArray();""")
    replace(path, "            startingAccuracy += (int) Math.Floor(1 + effect.m_effectParam / 100.0);",
            "            startingAccuracy += effect.m_effectParam;")


def patch_crown_shop() -> None:
    replace("WizardData/Models/Player/Account.cs",
            "    public int PurchasedCharacterSlots { get; set; }",
            """    public int PurchasedCharacterSlots { get; set; }
    public int Crowns { get; set; } = 50000;

    public bool SetCrowns(int crownsAmount) {
        this.Crowns = crownsAmount;
        return AccountCollection.UpdateCrowns(this.AccountId, crownsAmount);
    }""")
    replace("WizardData/Collections/AccountCollection.cs",
            """    public static bool RemovePurchasedCharacterSlot(ulong accountId) {
        return UpdateAccount(accountId, account => {
            if (account.PurchasedCharacterSlots > 0)
                --account.PurchasedCharacterSlots;
        });
    }""",
            """    public static bool RemovePurchasedCharacterSlot(ulong accountId) {
        return UpdateAccount(accountId, account => {
            if (account.PurchasedCharacterSlots > 0)
                --account.PurchasedCharacterSlots;
        });
    }

    public static bool UpdateCrowns(ulong accountId, int crownsAmount) {
        return UpdateAccount(accountId, account => {
            account.Crowns = crownsAmount;
        });
    }""")


def patch_director_crown_shop() -> None:
    program = ROOT / ".build/upstream/Imlight-b07987168fcfa2d123a5d78df90ca0c76405e427/src/Imlight.Director/Program.cs"
    if not program.exists():
        return
    text = program.read_text(encoding="utf-8-sig")
    if "CrownShopHandler.Load();" in text:
        return
    before = "        SpiralDB.Load();"
    after = "        SpiralDB.Load();\n        Imlight.CoreLib.Game.CrownShop.CrownShopHandler.Load();"
    if text.count(before) != 1:
        raise ValueError("Could not find SpiralDB.Load() in Director Program.cs")
    program.write_text(text.replace(before, after), encoding="utf-8")



def patch_dorm_and_zones() -> None:
    replace("Game/Services/ZoneService.cs",
            """        var tpmsg = new ZONE_102_PROTOCOL.MSG_ZONETRANSFER {
            DestinationZone = "WizardCity/QA_SpawnRate", // just teleporting to gm for now
            DestinationLocation = "Start",
            SendToClient = true,
            OwnerCharId = wizard.CharId,
        };""",
            """        var tpmsg = new ZONE_102_PROTOCOL.MSG_ZONETRANSFER {
            DestinationZone = "WizardCity/Interiors/WC_Housing_Dorm_Interior",
            DestinationLocation = "29.88838,3.523079,0,-1.586301",
            SendToClient = true,
            OwnerCharId = wizard.CharId,
            IsPrivate = true,
        };""")
    replace("Game/World/AccessPassManager.cs",
            """    internal static bool DoesZoneExist(string zoneName)
        => s_zones.Any(zone => zone.Equals(zoneName, System.StringComparison.OrdinalIgnoreCase));""",
            """    internal static bool DoesZoneExist(string zoneName)
        => s_zones.Any(zone => zone.Equals(zoneName, System.StringComparison.OrdinalIgnoreCase))
           || Imlight.CoreLib.WizardData.Collections.ZoneDataCollection.GetZoneData(zoneName) is not null;""")
    replace("Shared/Resources/LocalWadCache.cs",
            """        if (file is null) {\n            Logger.Warning("LocalCache does not contain a file definition for {FileName}!", \n                Logger.Args(wadName));\n\n            return null;\n        }""",
            """        if (file is null) {
            var diskPath = ResolveDiskWadPath(wadName);
            if (diskPath is not null && File.Exists(diskPath)) {
                try {
                    using var stream = File.OpenRead(diskPath);
                    var memStream = new MemoryStream();
                    stream.CopyTo(memStream);
                    memStream.Seek(0, SeekOrigin.Begin);
                    return ArchiveParser.Parse(memStream);
                }
                catch (Exception ex) {
                    Logger.Error("Failed to parse wad {FileName} from disk {Path}: {Error}", Logger.Args(wadName, diskPath, ex.Message));
                }
            }

            Logger.Warning("LocalCache does not contain a file definition for {FileName}!",
                Logger.Args(wadName));

            return null;
        }""")
    replace("Shared/Resources/LocalWadCache.cs",
            """    private static uint GetWadCrc(Archive wad) {""",
            """    private static string ResolveDiskWadPath(string wadName) {
        var setting = ConfigurationManager.Settings["Patch Server.GameDataPath"]?.AsString();
        if (!string.IsNullOrEmpty(setting)) {
            var p = Path.Combine(setting, $"{wadName}.wad");
            if (File.Exists(p)) return p;
        }
        string[] candidates = [
            "/home/hypercolossus/Revive101/Aurorium/data/V_r806919.Wizard_1_610/Data/GameData",
            "GameData",
            "../../Data/GameData",
            "../Data/GameData",
            "Data/GameData"
        ];
        foreach (var dir in candidates) {
            var fullDir = Path.IsPathRooted(dir) ? dir : Path.GetFullPath(dir);
            var p = Path.Combine(fullDir, $"{wadName}.wad");
            if (File.Exists(p)) return p;
        }
        return null;
    }

    private static uint GetWadCrc(Archive wad) {""")


def patch_bank_system() -> None:
    replace("WizardData/Models/Player/Account.cs",
            """    public int PurchasedCharacterSlots { get; set; }
    public int Crowns { get; set; } = 50000;""",
            """    public int PurchasedCharacterSlots { get; set; }
    public int Crowns { get; set; } = 50000;
    public List<ulong> SharedBankItemIds { get; set; } = new();
    [System.Text.Json.Serialization.JsonIgnore] public List<Imcodec.ObjectProperty.TypeCache.WizClientObjectItem> SharedBankItems { get; set; } = new();""")

    replace("WizardData/Collections/AccountCollection.cs",
            """using Raven.Client.Documents;""",
            """using Raven.Client.Documents;\nusing Imcodec.ObjectProperty.TypeCache;""")

    replace("WizardData/Collections/AccountCollection.cs",
            """        var infractions = session.Query<Infraction>(collectionName: InfractionCollection.CollectionName)
            .Where(i => i.AccountId == account.AccountId)
            .ToList();""",
            """        var infractions = session.Query<Infraction>(collectionName: InfractionCollection.CollectionName)
            .Where(i => i.AccountId == account.AccountId)
            .ToList();

        if (account.SharedBankItemIds?.Count > 0) {
            account.SharedBankItems = session.Query<WizClientObjectItem>(collectionName: WizardItemCollection.CollectionName)
                .Where(x => account.SharedBankItemIds.Contains(x.m_globalID))
                .ToList();
        } else {
            account.SharedBankItems = new();
        }""")

    replace("WizardData/Collections/AccountCollection.cs",
            """    public static bool UpdateCrowns(ulong accountId, int crownsAmount) {
        return UpdateAccount(accountId, account => {
            account.Crowns = crownsAmount;
        });
    }""",
            """    public static bool UpdateCrowns(ulong accountId, int crownsAmount) {
        return UpdateAccount(accountId, account => {
            account.Crowns = crownsAmount;
        });
    }

    public static bool UpdateSharedBank(ulong accountId, System.Collections.Generic.List<ulong> itemIds) {
        return UpdateAccount(accountId, account => {
            account.SharedBankItemIds = [.. itemIds];
        });
    }""")

    replace("WizardData/Models/Player/Wizard.cs",
            """    public ServerWizInventoryBehavior InventoryBehavior { get; set; }""",
            """    public ServerWizInventoryBehavior InventoryBehavior { get; set; }
    public List<ulong> BankItemIds { get; set; } = new();
    public int BankLimit { get; set; } = 100;
    [System.Text.Json.Serialization.JsonIgnore] public List<Imcodec.ObjectProperty.TypeCache.WizClientObjectItem> BankItems { get; set; } = new();""")

    replace("WizardData/Collections/WizardCollection.cs",
            """        wizard.InventoryBehavior.Items = [.. items
            .Where(i => wizard.InventoryBehavior.InventoryItemIds
            .Contains(i.m_globalID))
        ];""",
            """        wizard.InventoryBehavior.Items = [.. items
            .Where(i => wizard.InventoryBehavior.InventoryItemIds
            .Contains(i.m_globalID))
        ];

        wizard.BankItems = [.. items
            .Where(i => wizard.BankItemIds != null && wizard.BankItemIds.Contains(i.m_globalID))
        ];""")

    replace("WizardData/Collections/WizardItemCollection.cs",
            """using Imcodec.ObjectProperty.TypeCache;""",
            """using Imcodec.ObjectProperty.TypeCache;\nusing Imcodec.Types;""")

    replace("WizardData/Collections/WizardItemCollection.cs",
            """    public static bool DeleteInventory(ulong playerID) {""",
            """    public static bool UpdateItemCharacter(ulong itemId, ulong charId) {
        using var session = s_store.OpenSession();
        var item = session.Query<WizClientObjectItem>(collectionName: CollectionName)
            .FirstOrDefault(x => x.m_globalID == itemId);
        if (item is null) return false;
        item.m_characterId = (GID) charId;
        session.SaveChanges();
        return true;
    }

    public static bool UpdateItem(WizClientObjectItem item) {
        using var session = s_store.OpenSession();
        var associatedItem = session.Query<WizClientObjectItem>(collectionName: CollectionName)
            .FirstOrDefault(x => x.m_globalID == item.m_globalID);
        if (associatedItem == null) return false;
        associatedItem.m_inactiveBehaviors = item.m_inactiveBehaviors;
        associatedItem.m_primaryColor = item.m_primaryColor;
        associatedItem.m_secondaryColor = item.m_secondaryColor;
        associatedItem.m_pattern = item.m_pattern;
        associatedItem.m_displayID = item.m_displayID;
        session.SaveChanges();
        return true;
    }

    public static bool DeleteInventory(ulong playerID) {""")


def patch_pet_feeding() -> None:
    replace("Game/Services/InventoryService.cs",
            """using System;
using Akka.Actor;""",
            """using System;
using System.Linq;
using Akka.Actor;""")

    old_feed = """    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_FEEDINVENTORYITEM))]
    private void ReceiveFeedInventoryItem(GAME_5_PROTOCOL.MSG_FEEDINVENTORYITEM message) {
        SendToSocket(new GAME_5_PROTOCOL.MSG_FEEDINVENTORYITEM() {
            FedObjectID = message.FedObjectID,
            PetID = message.PetID,
        });
    }"""

    new_feed = """    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_FEEDINVENTORYITEM))]
    private void ReceiveFeedInventoryItem(GAME_5_PROTOCOL.MSG_FEEDINVENTORYITEM message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        var pet = wizard.InventoryBehavior?.Items?.FirstOrDefault(i => i.m_globalID == message.PetID)
            ?? wizard.EquipmentBehavior?.EquippedItems?.FirstOrDefault(i => i.m_globalID == message.PetID);
        var petBehavior = pet?.m_inactiveBehaviors?.OfType<ClientPetItemBehavior>().FirstOrDefault();

        var snack = wizard.PetSnackBehavior?.Snacks?.FirstOrDefault(s => s.m_globalID == message.FedObjectID);
        if (snack != null) {
            var snackTemplate = CoreObjectFactory.GetCoreTemplate(snack.m_templateID) as PetSnackItemTemplate;
            int xpGain = 0;
            if (snackTemplate?.m_statModifierSet?.m_modifications != null) {
                foreach (var mod in snackTemplate.m_statModifierSet.m_modifications) {
                    if (mod.m_change > 0) xpGain += mod.m_change;
                }
            }
            if (xpGain <= 0) xpGain = 1;

            var petTemplate = CoreObjectFactory.GetCoreTemplate(pet?.m_templateID ?? 0);
            var petItemBeh = petTemplate?.m_behaviors?.OfType<PetItemBehaviorTemplate>().FirstOrDefault();

            wizard.RemoveSnack(snack.m_globalID, out _);
            SendToSocket(new PET_9_PROTOCOL.MSG_PETSNACKREMOVE {
                GlobalID = wizard.GameObjectID,
                ItemID = snack.m_globalID
            });

            if (petBehavior != null && pet != null) {
                petBehavior.m_XP += (uint) xpGain;

                uint[] req = [0, 125, 250, 500, 1000, 2000, 2000];
                int currentLevel = Math.Clamp((int) petBehavior.m_level, 1, 6);
                uint needed = currentLevel < req.Length ? req[currentLevel] : 2000;

                if (petBehavior.m_XP >= needed && petBehavior.m_level < 6) {
                    petBehavior.m_XP -= needed;
                    petBehavior.m_level++;

                    petBehavior.m_expressedTalents ??= [];
                    petBehavior.m_expressedDerbyTalents ??= [];
                    uint newTalent = 0;
                    uint newDerby = 0;

                    if (petBehavior.m_allTalents != null) {
                        var candidate = petBehavior.m_allTalents.FirstOrDefault(t => !petBehavior.m_expressedTalents.Contains(t));
                        if (candidate != 0) {
                            petBehavior.m_expressedTalents.Add(candidate);
                            newTalent = candidate;
                        }
                    }
                    if (petBehavior.m_allDerbyTalents != null) {
                        var candidate = petBehavior.m_allDerbyTalents.FirstOrDefault(d => !petBehavior.m_expressedDerbyTalents.Contains(d));
                        if (candidate != 0) {
                            petBehavior.m_expressedDerbyTalents.Add(candidate);
                            newDerby = candidate;
                        }
                    }

                    SendToSocket(new PET_9_PROTOCOL.MSG_PETLEVELUP {
                        GlobalID = pet.m_globalID,
                        OverallRating = (byte) Math.Min(255u, petBehavior.m_overallRating),
                        ActiveRating = (byte) Math.Min(255u, petBehavior.m_activeRating),
                        PetLevel = petBehavior.m_level,
                        NewTalent = newTalent,
                        NewDerbyPower = newDerby,
                        NewJewel = 0,
                        Display = 1
                    });
                }

                WizardCollection.UpdateCharacterItems(wizard);
            }

            SendToSocket(new GAME_5_PROTOCOL.MSG_FEEDINVENTORYITEM {
                FedObjectID = message.FedObjectID,
                PetID = message.PetID
            });
            return;
        }

        var gear = wizard.InventoryBehavior?.GetItem(message.FedObjectID);
        if (gear != null) {
            wizard.RemoveItemFromInventory(gear.m_globalID);
            SendToSocket(new GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_REMOVEITEM {
                GlobalID = wizard.GameObjectID,
                ItemID = gear.m_globalID
            });

            int goldReward = Random.Shared.Next(15, 60);
            wizard.AddGold(goldReward);
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
                Gold = wizard.GameStats.m_currentGold,
                MaxGold = wizard.GameStats.m_baseGoldPouch
            });

            SendToSocket(new GAME_5_PROTOCOL.MSG_FEEDINVENTORYITEM {
                FedObjectID = message.FedObjectID,
                PetID = message.PetID
            });
            return;
        }

        SendToSocket(new GAME_5_PROTOCOL.MSG_FEEDINVENTORYITEM {
            FedObjectID = message.FedObjectID,
            PetID = message.PetID
        });
    }"""

    replace("Game/Services/InventoryService.cs", old_feed, new_feed)


def patch_jewel_system() -> None:
    old_jewels = """    #region Jewels

    // JEWELS
    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_EQUIPJEWELREQUEST))]
    private void ReceiveEquipJewelRequest(WIZARD2_53_PROTOCOL.MSG_EQUIPJEWELREQUEST message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_EQUIPJEWELREQUEST() {
            ItemGID = message.ItemGID,
            JewelGID = message.JewelGID,
            SocketNumber = message.SocketNumber,
        });

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_EQUIPJEWELTOITEM() {
            ItemGID = message.ItemGID,
            JewelGID = message.JewelGID,
            SocketNumber = message.SocketNumber,
            GlobalID = RandomGen.GenerateGUID()
        });
    }

    #endregion"""

    new_jewels = """    #region Jewels

    // JEWELS
    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_EQUIPJEWELREQUEST))]
    private void ReceiveEquipJewelRequest(WIZARD2_53_PROTOCOL.MSG_EQUIPJEWELREQUEST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        var targetItem = wizard.InventoryBehavior?.GetItem(message.ItemGID)
            ?? wizard.EquipmentBehavior?.GetItem(message.ItemGID);
        var jewelItem = wizard.InventoryBehavior?.GetItem(message.JewelGID);

        if (targetItem is null || jewelItem is null) {
            Logger.Log.Warning("Jewel equip request invalid: target={0}, jewel={1}",
                Logger.Args(message.ItemGID, message.JewelGID));
            return;
        }

        uint jewelTemplateId = (uint) jewelItem.m_templateID.Full;

        // Consume jewel from inventory
        wizard.RemoveItemFromInventory(message.JewelGID);
        SendToSocket(new GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_REMOVEITEM() {
            ItemID = message.JewelGID
        });

        // Add or update ClientJewelSocketBehavior on target item
        targetItem.m_inactiveBehaviors ??= [];
        var socketBeh = targetItem.m_inactiveBehaviors.OfType<ClientJewelSocketBehavior>().FirstOrDefault();
        if (socketBeh is null) {
            socketBeh = new ClientJewelSocketBehavior {
                m_behaviorTemplateNameID = 0x5CEF62AC,
                m_publicJewelInfo = new PublicEquippedJewelInfo {
                    m_infoPairs = []
                }
            };
            targetItem.m_inactiveBehaviors.Add(socketBeh);
        }

        socketBeh.m_publicJewelInfo ??= new PublicEquippedJewelInfo { m_infoPairs = [] };
        socketBeh.m_publicJewelInfo.m_infoPairs ??= [];
        socketBeh.m_publicJewelInfo.m_infoPairs.RemoveAll(p => p.m_socketNumber == message.SocketNumber);
        socketBeh.m_publicJewelInfo.m_infoPairs.Add(new PublicSocketJewelPair {
            m_socketNumber = message.SocketNumber,
            m_jewelTemplateID = jewelTemplateId
        });

        WizardItemCollection.UpdateItem(targetItem);

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_EQUIPJEWELREQUEST() {
            ItemGID = message.ItemGID,
            JewelGID = message.JewelGID,
            SocketNumber = message.SocketNumber
        });

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_EQUIPJEWELTOITEM() {
            GlobalID = wizard.GameObjectID,
            ItemGID = message.ItemGID,
            JewelGID = message.JewelGID,
            SocketNumber = message.SocketNumber
        });

        if (wizard.EquipmentBehavior?.GetItem(message.ItemGID) != null) {
            EquipmentItemCards.Apply(wizard, targetItem);
        }
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_UNSOCKETJEWELREQUEST))]
    private void ReceiveUnsocketJewelRequest(WIZARD2_53_PROTOCOL.MSG_UNSOCKETJEWELREQUEST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        var targetItem = wizard.InventoryBehavior?.GetItem(message.ItemGID)
            ?? wizard.EquipmentBehavior?.GetItem(message.ItemGID);
        if (targetItem is null) return;

        var socketBeh = targetItem.m_inactiveBehaviors?.OfType<ClientJewelSocketBehavior>().FirstOrDefault();
        if (socketBeh?.m_publicJewelInfo?.m_infoPairs != null) {
            socketBeh.m_publicJewelInfo.m_infoPairs.RemoveAll(p => p.m_socketNumber == message.SocketNumber);
            WizardItemCollection.UpdateItem(targetItem);
        }

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UNEQUIPSOCKETEDJEWEL() {
            GlobalID = wizard.GameObjectID,
            ItemGID = message.ItemGID,
            SocketNumber = message.SocketNumber,
            SocketDeleted = 1
        });

        if (wizard.EquipmentBehavior?.GetItem(message.ItemGID) != null) {
            EquipmentItemCards.Apply(wizard, targetItem);
        }
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_UNLOCKSOCKETS))]
    private void ReceiveUnlockSockets(WIZARD2_53_PROTOCOL.MSG_UNLOCKSOCKETS message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        var targetItem = wizard.InventoryBehavior?.GetItem(message.ItemGID)
            ?? wizard.EquipmentBehavior?.GetItem(message.ItemGID);
        if (targetItem is null) return;

        int unlockCost = 150;
        if (message.CostChoice != 0 && wizard.Account.Crowns >= unlockCost) {
            wizard.Account.SetCrowns(wizard.Account.Crowns - unlockCost);
        }

        targetItem.m_inactiveBehaviors ??= [];
        var socketBeh = targetItem.m_inactiveBehaviors.OfType<ClientJewelSocketBehavior>().FirstOrDefault();
        if (socketBeh is null) {
            socketBeh = new ClientJewelSocketBehavior {
                m_behaviorTemplateNameID = 0x5CEF62AC,
                m_publicJewelInfo = new PublicEquippedJewelInfo {
                    m_infoPairs = []
                }
            };
            targetItem.m_inactiveBehaviors.Add(socketBeh);
        }
        socketBeh.m_publicJewelInfo ??= new PublicEquippedJewelInfo { m_infoPairs = [] };
        socketBeh.m_publicJewelInfo.m_bSocketsLocked = false;
        WizardItemCollection.UpdateItem(targetItem);

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_UNLOCKSOCKETSCONFIRM() {
            Failure = 0,
            WebFailure = 0,
            Credits = wizard.Account.Crowns
        });

        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_JEWELSOCKETLOCKUPDATE() {
            GlobalID = wizard.GameObjectID,
            ItemGID = message.ItemGID,
            Locked = 0
        });
    }

    #endregion"""
    replace("Game/Services/InventoryService.cs", old_jewels, new_jewels)


def patch_stitching_system() -> None:
    old_done = """        var wizBangMsg = new GAME_5_PROTOCOL.MSG_WIZBANG() {
            GameObjectID = wizard.GameObjectID,
            WizBangID = (uint) WizBangs.None
        };
        ZoneBroadcast(wizBangMsg, false);
    }"""

    new_done = """        var wizBangMsg = new GAME_5_PROTOCOL.MSG_WIZBANG() {
            GameObjectID = wizard.GameObjectID,
            WizBangID = (uint) WizBangs.None
        };
        ZoneBroadcast(wizBangMsg, false);
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_STITCHITEMS))]
    private void ReceiveStitchItems(WIZARD_12_PROTOCOL.MSG_STITCHITEMS message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        int cost = 100;
        if (wizard.Account.Crowns < cost) {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_STITCHITEMSCONFIRM {
                Failure = 1,
                WebFailure = 0,
                Credits = wizard.Account.Crowns
            });
            return;
        }

        var statItem = wizard.InventoryBehavior?.GetItem(message.StatsID)
            ?? wizard.EquipmentBehavior?.GetItem(message.StatsID);
        var displayItem = wizard.InventoryBehavior?.GetItem(message.DisplayID)
            ?? wizard.EquipmentBehavior?.GetItem(message.DisplayID);

        if (statItem is null || displayItem is null) {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_STITCHITEMSCONFIRM {
                Failure = 2,
                WebFailure = 0,
                Credits = wizard.Account.Crowns
            });
            return;
        }

        wizard.Account.SetCrowns(wizard.Account.Crowns - cost);

        statItem.m_displayID = displayItem.m_displayID.Full != 0 ? displayItem.m_displayID : displayItem.m_templateID;
        statItem.m_primaryColor = displayItem.m_primaryColor;
        statItem.m_secondaryColor = displayItem.m_secondaryColor;
        statItem.m_pattern = displayItem.m_pattern;

        wizard.RemoveItemFromInventory(displayItem.m_globalID);
        SendToSocket(new GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_REMOVEITEM {
            ItemID = displayItem.m_globalID
        });

        WizardItemCollection.UpdateItem(statItem);

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_STITCHITEMSCONFIRM {
            Failure = 0,
            WebFailure = 0,
            Credits = wizard.Account.Crowns
        });

        if (wizard.EquipmentBehavior?.GetItem(statItem.m_globalID) != null) {
            var template = (WizItemTemplate) CoreObjectFactory.GetCoreTemplate(statItem.m_templateID);
            SendEquippedItemRefresh(wizard, statItem, template);
        }
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_UNSTITCHITEMS))]
    private void ReceiveUnstitchItems(WIZARD_12_PROTOCOL.MSG_UNSTITCHITEMS message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        int cost = 100;
        if (wizard.Account.Crowns < cost) {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_STITCHITEMSCONFIRM {
                Failure = 1,
                WebFailure = 0,
                Credits = wizard.Account.Crowns
            });
            return;
        }

        var item = wizard.InventoryBehavior?.GetItem(message.ItemID)
            ?? wizard.EquipmentBehavior?.GetItem(message.ItemID);
        if (item is null) return;

        ulong appearanceTemplateId = item.m_displayID.Full != 0 ? item.m_displayID.Full : 0;

        wizard.Account.SetCrowns(wizard.Account.Crowns - cost);

        item.m_displayID = 0;
        WizardItemCollection.UpdateItem(item);

        if (appearanceTemplateId != 0) {
            var restoredObj = CoreObjectFactory.FinalizeCoreObject(appearanceTemplateId);
            if (restoredObj is WizClientObjectItem restoredItem) {
                wizard.AddItemToInventory(restoredItem);
                if (s_itemSerializer.Serialize(restoredItem, 1, out var restoredData)) {
                    SendToSocket(new GAME_5_PROTOCOL.MSG_INVENTORYBEHAVIOR_ADDITEM {
                        GlobalID = wizard.GameObjectID,
                        SerializedItem = restoredData
                    });
                }
                SendToSocket(new WIZARD2_53_PROTOCOL.MSG_ITEMACQUISITION {
                    ItemGlobalID = restoredItem.m_globalID,
                    ItemTemplateID = (uint) appearanceTemplateId,
                    ItemLocation = 1
                });
            }
        }

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_STITCHITEMSCONFIRM {
            Failure = 0,
            WebFailure = 0,
            Credits = wizard.Account.Crowns
        });

        if (wizard.EquipmentBehavior?.GetItem(item.m_globalID) != null) {
            var template = (WizItemTemplate) CoreObjectFactory.GetCoreTemplate(item.m_templateID);
            SendEquippedItemRefresh(wizard, item, template);
        }
    }"""

    replace("Game/Services/ShopService.cs", old_done, new_done)


def patch_crafting_system() -> None:
    pass


def patch_pvp_system() -> None:
    # 1. Allow players to join Team Monster slots when available
    old_join = """        var subCircle = isPlayer ? ChoosePlayerSubCircle(participantObject) : GetAvailableSubCircleTeamCreature();
        if (subCircle is null) {"""
    new_join = """        var subCircle = isPlayer ? ChoosePlayerSubCircle(participantObject) : GetAvailableSubCircleTeamCreature();
        if (subCircle is null && isPlayer && IsSlotAvailable(CombatTeam.Monster)) {
            subCircle = GetAvailableSubCircleTeamCreature();
        }
        if (subCircle is null) {"""
    replace("Game/Zone/Components/CombatDuelComponent.cs", old_join, new_join)

    # 2. Allow players on Monster team to draw hands and receive MSG_COMBATHAND
    old_hand = """    private void SendCombatHand() {
        // Serialize the combat hand and send it to the participant, locally.
        // We're skipping creatures for now.
        EnactActionOnSubCircles(circle => {
            if (circle.OccupiedTeam == CombatTeam.Monster) {
                return;
            }"""
    new_hand = """    private void SendCombatHand() {
        // Serialize the combat hand and send it to the participant, locally.
        // We're skipping creatures for now.
        EnactActionOnSubCircles(circle => {
            if (circle._wizard is null && !circle.IsSummonedMinion) {
                return;
            }"""
    replace("Game/Zone/Components/CombatDuelComponent.cs", old_hand, new_hand)

    # 3. Allow players on Monster team to receive current combat hand
    old_cur_hand = """    internal void SendCurrentCombatHand(CombatDuelSubCircle circle) {
        // As-is, no draw or refill, so a discarded slot stays visibly open for the vault draw.
        if (circle is null || circle.OccupiedTeam == CombatTeam.Monster) {
            return;
        }"""
    new_cur_hand = """    internal void SendCurrentCombatHand(CombatDuelSubCircle circle) {
        // As-is, no draw or refill, so a discarded slot stays visibly open for the vault draw.
        if (circle is null || (circle.OccupiedTeam == CombatTeam.Monster && circle._wizard is null)) {
            return;
        }"""
    replace("Game/Zone/Components/CombatDuelComponent.cs", old_cur_hand, new_cur_hand)

    # 4. Handle telegraphs for any player caster
    old_pass = """        if (caster.OccupiedTeam == CombatTeam.Player) {
            SendCombatMoveSelection(caster.ParticipantObject.m_globalID, (byte) CombatMoveType.Pass, null, 0);
        }"""
    new_pass = """        if (caster._wizard is not null) {
            SendCombatMoveSelection(caster.ParticipantObject.m_globalID, (byte) CombatMoveType.Pass, null, 0);
        }"""
    replace("Game/Zone/Components/CombatDuelComponent.cs", old_pass, new_pass)

    old_atk = """        // Minions are AI-driven; their telegraph comes from ResendMinionMoveSelections, not an echo.
        if (!caster.IsSummonedMinion && caster.OccupiedTeam == CombatTeam.Player) {
            SendCombatMoveSelection(caster.ParticipantObject.m_globalID, (byte) CombatMoveType.Attack, spell, (byte) spellTarget);
        }"""
    new_atk = """        // Minions are AI-driven; their telegraph comes from ResendMinionMoveSelections, not an echo.
        if (!caster.IsSummonedMinion && caster._wizard is not null) {
            SendCombatMoveSelection(caster.ParticipantObject.m_globalID, (byte) CombatMoveType.Attack, spell, (byte) spellTarget);
        }"""
    replace("Game/Zone/Components/CombatDuelComponent.cs", old_atk, new_atk)

    old_mind = """        // Echo the change mind action to each player participant
        if (caster.OccupiedTeam == CombatTeam.Player) {
            SendCombatMoveSelection(caster.ParticipantObject.m_globalID, (byte) CombatMoveType.ChangeMind, null, 0);
        }"""
    new_mind = """        // Echo the change mind action to each player participant
        if (caster._wizard is not null) {
            SendCombatMoveSelection(caster.ParticipantObject.m_globalID, (byte) CombatMoveType.ChangeMind, null, 0);
        }"""
    replace("Game/Zone/Components/CombatDuelComponent.cs", old_mind, new_mind)

    # 5. Remove players from duel handles any player participant
    old_rem = """    private void RemovePlayersFromDuel() => EnactActionOnSubCircles(circle => {
        if (circle.OccupiedTeam != CombatTeam.Player) {
            return;
        }"""
    new_rem = """    private void RemovePlayersFromDuel() => EnactActionOnSubCircles(circle => {
        if (circle._wizard is null) {
            return;
        }"""
    replace("Game/Zone/Components/CombatDuelComponent.cs", old_rem, new_rem)


def patch_bazaar_system() -> None:
    # 1. Seed default entries when collection is empty
    old_init = """        if (s_entries is null) {
            s_entries = [];
        }

        s_isInitialized = true;"""
    new_init = """        if (s_entries is null || s_entries.Count == 0) {
            s_entries = [];
            ulong[] starterTemplates = [
                10183, 10184, 10185, 10186, 10187, 10188, 10189, 10190, 10191, 10192
            ];
            foreach (var tid in starterTemplates) {
                var entry = new AuctionHouseEntry {
                    m_templateID = (Imcodec.Types.GID) tid,
                    m_numForSale = 50,
                    m_buyPrice = 50,
                    m_sellPrice = 15
                };
                session.Store(entry);
                s_entries.Add(entry);
            }
            session.SaveChanges();
        }

        s_isInitialized = true;"""
    replace("WizardData/Collections/AuctionHouseCollection.cs", old_init, new_init)

    # 2. Add Command 4, 5, 6, 9 to AuctionHouseService
    old_switch = """            case 3:
                BuyFromAuctionHouse(message.itemTemplateID, message.texture, message.decal, message.key);
                break;
            case 4:
                // ?
                break;
            case 5:
                // ?
                break;
            case 6:
                // ?
                break;
            case 9:
                // ?
                break;"""
    new_switch = """            case 3:
                BuyFromAuctionHouse(message.itemTemplateID, message.texture, message.decal, message.key);
                break;
            case 4:
                ConfirmSellToAuctionHouse(message.itemGlobalID, message.key);
                break;
            case 5:
                SendAuctionHouseContents(message.npcGlobalID, message.category, message.key);
                break;
            case 6:
                SellToAuctionHouse(message.itemGlobalID, message.key);
                break;
            case 9:
                SendToSocket(new WIZARD_12_PROTOCOL.MSG_AUCTIONRESPONSE {
                    Command = 9,
                    ReturnCode = 0
                });
                break;"""
    replace("Game/Services/AuctionHouseService.cs", old_switch, new_switch)

    old_confirm_buy = """    private void ConfirmBuyFromAuctionHouse(ulong templateId, uint key) {"""
    new_confirm_sell = """    private void ConfirmSellToAuctionHouse(ulong itemGlobalId, uint key) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        var item = wizard.InventoryBehavior?.GetItem(itemGlobalId);
        if (item is null) return;

        var entry = AuctionHouseCollection.GetAuctionHouseEntry(item.m_templateID.Full);
        int sellPrice = entry?.m_sellPrice ?? 25;

        SendToSocket(new WIZARD_12_PROTOCOL.MSG_AUCTIONRESPONSE {
            Command = 4,
            ItemTemplateID = item.m_templateID,
            Cost = sellPrice,
            ReturnCode = 0
        });
    }

    private void ConfirmBuyFromAuctionHouse(ulong templateId, uint key) {"""
    replace("Game/Services/AuctionHouseService.cs", old_confirm_buy, new_confirm_sell)


def patch_pet_hatching() -> None:
    old_hatch = "    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_HATCHEGGNOW))]"
    new_hatch = """    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_PETHATCHREQUEST))]
    private void ReceivePetHatchRequest(WIZARD_12_PROTOCOL.MSG_PETHATCHREQUEST message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PETHATCHRESULT {
            TargetGID = message.TargetGID,
            Status = 1
        });
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_PETHATCHCREATE {
            TargetGID = message.TargetGID
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGHATCH))]
    private void ReceiveHatchmakingHatch(WIZARD2_53_PROTOCOL.MSG_REQUESTHATCHMAKINGHATCH message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        if (wizard.GameStats.m_currentGold >= 25000) {
            wizard.AddGold(-25000);
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
                Gold = wizard.GameStats.m_currentGold
            });
        }

        uint childTemplate = 100334;
        var egg = wizard.PetOwnerBehavior.CreatePetEgg(childTemplate, 300);

        SendEggCreatedMessages(egg);
        StartEggTimer(egg);

        Logger.Information("Pet hatching initiated! Created egg {0}", Logger.Args(egg.m_globalID));
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_PETGAMEEND))]
    private void ReceivePetGameEnd(PET_9_PROTOCOL.MSG_PETGAMEEND message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        int currentEnergy = wizard.PetOwnerBehavior?.Energy ?? 0;
        if (currentEnergy >= 2) {
            wizard.UpdateEnergy(currentEnergy - 2);
        }

        SendToSocket(new PET_9_PROTOCOL.MSG_PETGAMESNACKFEEDSUCCESS {
            Data = Imcodec.IO.ByteString.Empty
        });
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_PETGAMESKIP))]
    private void ReceivePetGameSkip(WIZARD2_53_PROTOCOL.MSG_PETGAMESKIP message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        int currentEnergy = wizard.PetOwnerBehavior?.Energy ?? 0;
        if (currentEnergy >= 2) {
            wizard.UpdateEnergy(currentEnergy - 2);
        }

        SendToSocket(new PET_9_PROTOCOL.MSG_PETGAMESNACKFEEDSUCCESS {
            Data = Imcodec.IO.ByteString.Empty
        });
    }

    [MessageHandler(typeof(PET_9_PROTOCOL.MSG_HATCHEGGNOW))]"""
    replace("Game/Services/PetService.cs", old_hatch, new_hatch)


def main() -> None:
    shutil.copytree(ORIGINAL, SOURCE, dirs_exist_ok=True)
    for path in SOURCE.rglob("*.cs"):
        content = path.read_text(encoding="utf-8-sig")
        if "\u2014" in content:
            path.write_text(content.replace("\u2014", ","), encoding="utf-8")
    patch_quest_flow()
    patch_world_state()
    patch_local_login()
    patch_combat_accuracy()
    patch_crown_shop()
    patch_director_crown_shop()
    patch_dorm_and_zones()
    patch_bank_system()
    patch_pet_feeding()
    patch_jewel_system()
    patch_stitching_system()
    patch_crafting_system()
    patch_pvp_system()
    patch_bazaar_system()
    patch_pet_hatching()
    shutil.copy2(ROOT / "BankService.cs", SOURCE / "Game/Services/BankService.cs")
    shutil.copy2(ROOT / "InteractSeamstressComponent.cs", SOURCE / "Game/Zone/Components/InteractSeamstressComponent.cs")
    shutil.copy2(ROOT / "CraftingService.cs", SOURCE / "Game/Services/CraftingService.cs")
    shutil.copy2(ROOT / "TradeService.cs", SOURCE / "Game/Services/TradeService.cs")
    shutil.copy2(ROOT / "HousingService.cs", SOURCE / "Game/Services/HousingService.cs")
    shutil.copy2(ROOT / "GardeningService.cs", SOURCE / "Game/Services/GardeningService.cs")
    shutil.copy2(ROOT / "FishingService.cs", SOURCE / "Game/Services/FishingService.cs")
    shutil.copy2(ROOT / "DailyQuestService.cs", SOURCE / "Game/Services/DailyQuestService.cs")
    shutil.copy2(ROOT / "MonstrologyService.cs", SOURCE / "Game/Services/MonstrologyService.cs")
    shutil.copy2(ROOT / "CastleToursService.cs", SOURCE / "Game/Services/CastleToursService.cs")
    shutil.copy2(ROOT / "CombatAccuracy.cs", SOURCE / "Game/Combat/CombatAccuracy.cs")
    shutil.copy2(ROOT / "EquipmentItemCards.cs", SOURCE / "Game/EquipmentItemCards.cs")
    shutil.copy2(ROOT / "InfiniteMana.cs", SOURCE / "Game/InfiniteMana.cs")
    shutil.copy2(ROOT / "AccountToggleStore.cs", SOURCE / "Game/AccountToggleStore.cs")
    shutil.copy2(ROOT / "SpellPresentation.cs", SOURCE / "Game/SpellPresentation.cs")
    shutil.copy2(ROOT / "SpellEffectFilter.cs", SOURCE / "Game/Spells/SpellEffectFilter.cs")
    shutil.copy2(ROOT / "TrainerRules.cs", SOURCE / "Game/TrainerRules.cs")
    shutil.copy2(ROOT / "QaGear.cs", SOURCE / "Game/QaGear.cs")
    shutil.copy2(ROOT / "InteractTrainerComponent.cs", SOURCE / "Game/Zone/Components/InteractTrainerComponent.cs")
    shutil.copy2(ROOT / "WorldTeleportDoorComponent.cs", SOURCE / "Game/Zone/Components/WorldTeleportDoorComponent.cs")
    shutil.copy2(ROOT / "RandomizerRuntime.cs", SOURCE / "Game/RandomizerRuntime.cs")
    shutil.copy2(ROOT / "VendorRandomization.cs", SOURCE / "Game/VendorRandomization.cs")
    shutil.copy2(ROOT / "ServiceOptionRoutes.cs", SOURCE / "Game/ServiceOptionRoutes.cs")
    shutil.copy2(ROOT / "SpellDrops.cs", SOURCE / "Game/SpellDrops.cs")
    shutil.copy2(ROOT / "SpellDropRules.cs", SOURCE / "Game/SpellDropRules.cs")
    shutil.copy2(ROOT / "SpellRewards.cs", SOURCE / "Game/SpellRewards.cs")
    shutil.copy2(ROOT / "SpellRewardService.cs", SOURCE / "Game/Services/SpellRewardService.cs")
    shutil.copy2(ROOT / "CreatureRandomization.cs", SOURCE / "Game/CreatureRandomization.cs")
    shutil.copy2(ROOT / "EnemyInspectorService.cs", SOURCE / "Game/Services/EnemyInspectorService.cs")
    shutil.copy2(ROOT / "EnemyInspectorData.cs", SOURCE / "Game/EnemyInspectorData.cs")
    (SOURCE / "Game/CrownShop").mkdir(parents=True, exist_ok=True)
    shutil.copy2(ROOT / "CrownShopModels.cs", SOURCE / "Game/CrownShop/CrownShopModels.cs")
    shutil.copy2(ROOT / "CrownShopHandler.cs", SOURCE / "Game/CrownShop/CrownShopHandler.cs")
    shutil.copy2(ROOT / "CrownShopService.cs", SOURCE / "Game/Services/CrownShopService.cs")
    replace("Game/GameServiceFactory.cs", "        typeof(MinigameService),", "        typeof(MinigameService),\n        typeof(CrownShopService),\n        typeof(EnemyInspectorService),\n        typeof(SpellRewardService),\n        typeof(BankService),\n        typeof(CraftingService),\n        typeof(TradeService),\n        typeof(HousingService),\n        typeof(GardeningService),\n        typeof(FishingService),\n        typeof(DailyQuestService),\n        typeof(MonstrologyService),\n        typeof(CastleToursService),")
    replace("Game/Services/MinigameService.cs", "        // The raw message is just like any other message sent by the client.", """        if (message.JobID == EnemyInspectorService.JobId || message.JobID == SpellRewardService.JobId) {
            return;
        }

        // The raw message is just like any other message sent by the client.""")
    replace("Game/Services/MinigameService.cs", "    private const ushort MAGIC_HEADER = (ushort) 0xF00Du;", """    private const ushort MAGIC_HEADER = (ushort) 0xF00Du;

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_MINIGAMEREWARDS))]
    private void ReceiveMinigameRewards(WIZARD_12_PROTOCOL.MSG_MINIGAMEREWARDS message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        wizard.GameStats.m_potionCharge = wizard.GameStats.m_potionMax;
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEPOTIONS {
            PotionMax = wizard.GameStats.m_potionMax,
            PotionCharge = wizard.GameStats.m_potionCharge
        });

        int goldReward = Math.Max(50, message.Score / 10);
        wizard.AddGold(goldReward);
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
            Gold = wizard.GameStats.m_currentGold
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_USEPOTION))]
    private void ReceiveUsePotion(WIZARD_12_PROTOCOL.MSG_USEPOTION message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        if (wizard.GameStats.m_potionCharge >= 1.0f) {
            wizard.GameStats.m_potionCharge -= 1.0f;
            wizard.GameStats.m_currentHitpoints = wizard.GameStats.m_baseHitpoints;
            wizard.GameStats.m_currentMana = wizard.GameStats.m_baseMana;

            SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEPOTIONS {
                PotionMax = wizard.GameStats.m_potionMax,
                PotionCharge = wizard.GameStats.m_potionCharge
            });
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEHEALTH {
                CharacterID = wizard.GameObjectID,
                NewHealth = wizard.GameStats.m_currentHitpoints,
                NewHealthMax = wizard.GameStats.m_baseHitpoints,
                DisplayDiff = 1
            });
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEMANA {
                Mana = wizard.GameStats.m_currentMana,
                MaxMana = wizard.GameStats.m_baseMana,
                DisplayDiff = 1
            });
        }
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_POTIONBUYREQUEST))]
    private void ReceivePotionBuyRequest(WIZARD_12_PROTOCOL.MSG_POTIONBUYREQUEST message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        int potionCost = 150;
        if (wizard.GameStats.m_currentGold >= potionCost) {
            wizard.AddGold(-potionCost);
            wizard.GameStats.m_potionCharge = wizard.GameStats.m_potionMax;

            SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
                Gold = wizard.GameStats.m_currentGold
            });
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEPOTIONS {
                PotionMax = wizard.GameStats.m_potionMax,
                PotionCharge = wizard.GameStats.m_potionCharge
            });
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_POTIONBUYCONFIRM {
                Failure = 0
            });
        }
        else {
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_POTIONBUYCONFIRM {
                Failure = 1
            });
        }
    }""")
    shutil.copy2(ROOT / "RandomizerLevelRequirement.cs", SOURCE / "Game/Requirements/Handlers/RandomizerLevelRequirement.cs")
    replace("Login/Services/CharacterService.cs", "        SendToSocket(new LOGIN_7_PROTOCOL.MSG_CHARACTERLIST());", """        SendToSocket(new LOGIN_7_PROTOCOL.MSG_CHARACTERLIST());
        if (Imlight.CoreLib.Game.RandomizerRuntime.Enabled) {
            var mode = Imlight.CoreLib.Game.RandomizerRuntime.GetMode(account.AccountId);
            SendToSocket(new GAME_5_PROTOCOL.MSG_START_CLIENT_PROCESS {
                OwnerGID = 0,
                JobID = Imlight.CoreLib.Game.RandomizerRuntime.MenuJobId,
                ScriptName = $"Randomizer/Menu{mode}.lua"
            });
        }""")
    replace("Login/Services/CharacterService.cs", "    private uint _characterCreationStage;", """    [MessageHandler(typeof(GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS))]
    private void ReceiveRandomizerChoice(GAME_5_PROTOCOL.MSG_MESSAGE_PROCESS message) {
        var account = GetSocketAccount();
        if (account is not null && message.JobID == Imlight.CoreLib.Game.RandomizerRuntime.MenuJobId &&
            Imlight.CoreLib.Game.RandomizerRuntime.TryReadChoice(message.Message, out var mode)) {
            Imlight.CoreLib.Game.RandomizerRuntime.SetMode(account.AccountId, mode);
        }
    }

    private uint _characterCreationStage;""")
    replace("Login/Services/GameTransitionService.cs", "        var msg = new SERVER_100_PROTOCOL.MSG_GETBESTSERVER();", """        if (Imlight.CoreLib.Game.RandomizerRuntime.Enabled) {
            var mode = Imlight.CoreLib.Game.RandomizerRuntime.GetMode(GetSocketAccount().AccountId);
            return AskServer<SERVER_100_PROTOCOL.MSG_SERVERINFO>(new SERVER_100_PROTOCOL.MSG_QUERYREALMSERVER {
                RealmName = Imlight.CoreLib.Game.RandomizerRuntime.ModeName(mode)
            });
        }

        var msg = new SERVER_100_PROTOCOL.MSG_GETBESTSERVER();""")
    replace("Game/Zone/Core/ZoneEntity.cs", "        AutoAttachComponents();", """        Template = Imlight.CoreLib.Game.RandomizerRuntime.CreatureTemplate(
            Template, ActiveGameObject?.m_templateID ?? 0, ZoneRef.Path.ToString());
        AutoAttachComponents();""")
    replace("Game/Zone/Core/ZoneEntity.cs", "            m_templateID = ActiveGameObject.m_templateID,", """            m_templateID = Imlight.CoreLib.Game.RandomizerRuntime.ClientCreatureId(
                ActiveGameObject.m_templateID, ZoneRef.Path.ToString()),""")
    replace("Game/Zone/Components/NpcComponent.cs", "    private StatsComponent _statsComponent;", """    private readonly Imlight.CoreLib.Game.CreatureRoll _randomizerRoll;
    private StatsComponent _statsComponent;""")
    replace("Game/Zone/Components/CombatDuelComponent.cs", "    public WizardClientDuelBehavior GetClientBehaviorInstance() => new() {", """    [MessageHandler(typeof(Imlight.CoreLib.Game.EnemyInspectorQuery))]
    private void ReceiveInspectorQuery(Imlight.CoreLib.Game.EnemyInspectorQuery query) {
        if (Duel is null || SubCircles is null) {
            return;
        }

        var player = ActiveSubCircles.FirstOrDefault(circle => (ulong)circle.ParticipantObject.m_globalID == query.PlayerId);
        if (player is null || Duel.m_duelPhase != kDuelPhase.kPhase_Planning) {
            return;
        }

        Sender.Tell(new Imlight.CoreLib.Game.EnemyInspectorSnapshot(
            Imlight.CoreLib.Game.EnemyInspectorData.PlanningCamera(Entity.ActiveGameObject.m_location,
                Entity.ActiveGameObject.m_orientation.Z, player.SlotIndex)));
        foreach (var circle in ActiveSubCircles.Where(circle => circle.OccupiedTeam != player.OccupiedTeam &&
                     circle.IsAlive && !circle.CombatParticipant.m_isPlayer)) {
            ulong templateId = Imlight.CoreLib.Game.RandomizerRuntime.ClientCreatureId(
                circle.ParticipantObject.m_templateID, Entity.ZoneRef.Path.ToString());
            var template = CoreObjectFactory.GetCoreTemplate(templateId) as GameObjectTemplate;
            Sender.Tell(new Imlight.CoreLib.Game.EnemyInspectorSnapshot(
                Imlight.CoreLib.Game.EnemyInspectorData.Snapshot(circle.ParticipantObject,
                    template?.m_displayName ?? "", (MagicSchool)circle.CombatParticipant.m_primaryMagicSchoolID,
                    circle.CombatParticipant.m_mobLevel, circle.ParticipantGameStats)));
        }
    }

    public WizardClientDuelBehavior GetClientBehaviorInstance() => new() {""")
    replace("Game/Zone/Components/NpcComponent.cs", "        this.StartingHealth = _npcBehaviorTemplate.m_nStartingHealth;", """        _randomizerRoll = _duelistBehaviorTemplate is null
            ? new Imlight.CoreLib.Game.CreatureRoll(1, 1, 0, 0, 1)
            : Imlight.CoreLib.Game.RandomizerRuntime.RollCreature(Entity.ZoneRef.Path.ToString());
        this.StartingHealth = _randomizerRoll.ScaleHealth(_npcBehaviorTemplate.m_nStartingHealth);""")
    replace("Game/Zone/Components/NpcComponent.cs", "        // If the NPC is a monster, it will have a deck.", """        if (IsMonster) {
            Imlight.CoreLib.Game.CreatureRandomization.Apply(_statsComponent.Stats, _randomizerRoll);
        }

        // If the NPC is a monster, it will have a deck.""")
    replace("Game/DropTables/LootGranter.cs", "        UpdateWizardGold(playerActor, wizard, results.GoldAmount);", """        Imlight.CoreLib.Game.RandomizerRuntime.RandomizeLoot(playerActor, wizard, results);
        UpdateWizardGold(playerActor, wizard, results.GoldAmount);""")
    replace("Game/Combat/CombatDeck.cs", "    private readonly byte _handSize;", """    private readonly byte _handSize;
    private readonly int _randomizerMode;""")
    replace("Game/Combat/CombatDeck.cs", "internal CombatDeck(List<CombatDeckSpellData> spellDatas, List<CombatDeckSpellData> treasureVault, byte handSize)",
            "internal CombatDeck(List<CombatDeckSpellData> spellDatas, List<CombatDeckSpellData> treasureVault, byte handSize, int randomizerMode = 0)")
    replace("Game/Combat/CombatDeck.cs", "        this._spellData = spellDatas;", """        this._randomizerMode = randomizerMode;
        this._spellData = spellDatas.Select(card => new CombatDeckSpellData {
            TemplateId = Imlight.CoreLib.Game.RandomizerRuntime.CombatSpellId(card.TemplateId, randomizerMode),
            Quantity = card.Quantity,
            IsBattleCard = card.IsBattleCard,
            IsItemCard = card.IsItemCard,
            IsTreasureCard = card.IsTreasureCard
        }).ToList();""")
    replace("Game/Combat/CombatDeck.cs", "        LastGivenHand.Add(spell);\n    }", """        uint templateId = Imlight.CoreLib.Game.RandomizerRuntime.CombatSpellId(spell.m_templateID, _randomizerMode);
        if (!spell.m_treasureCard && templateId != spell.m_templateID) {
            var replacement = SpellFactory.GetSpell(templateId);
            replacement.m_itemCard = spell.m_itemCard;
            replacement.m_battleCard = spell.m_battleCard;
            spell = replacement;
        }

        LastGivenHand.Add(spell);
    }""")
    replace("Game/Combat/CombatDuelSubCircle.cs", "        _combatDeck = new CombatDeck(allSpells, vaultSpells, PLAYER_HAND_SIZE);", """        _combatDeck = new CombatDeck(allSpells, vaultSpells, PLAYER_HAND_SIZE,
            Imlight.CoreLib.Game.RandomizerRuntime.GetMode(_wizard.AccountId));""")
    replace("Game/Combat/CombatDuelSubCircle.cs", "        _combatDeck = new CombatDeck(spellData, [], PLAYER_HAND_SIZE);", """        _combatDeck = new CombatDeck(spellData, [], PLAYER_HAND_SIZE,
            Imlight.CoreLib.Game.RandomizerRuntime.ModeForPath(ParticipantActor.Path.ToString()));""")
    patch_vendor_hooks(replace)
    patch_spell_drops(replace)
    patch_item_cards(replace)
    patch_infinite_mana(replace)
    patch_spell_numbers(replace)
    patch_creature_ai(replace)
    patch_trainers(replace)
    patch_qa_gear(replace)


if __name__ == "__main__":
    main()

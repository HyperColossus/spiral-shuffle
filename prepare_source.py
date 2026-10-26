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
    shutil.copy2(ROOT / "ClientService.cs", SOURCE / "Game/Services/ClientService.cs")
    shutil.copy2(ROOT / "CantripService.cs", SOURCE / "Game/Services/CantripService.cs")
    shutil.copy2(ROOT / "FriendsService.cs", SOURCE / "Game/Services/FriendsService.cs")
    shutil.copy2(ROOT / "SpellBookService.cs", SOURCE / "Game/Services/SpellBookService.cs")
    shutil.copy2(ROOT / "TeamUpService.cs", SOURCE / "Game/Services/TeamUpService.cs")
    shutil.copy2(ROOT / "PetDerbyService.cs", SOURCE / "Game/Services/PetDerbyService.cs")
    shutil.copy2(ROOT / "GuildService.cs", SOURCE / "Game/Services/GuildService.cs")
    shutil.copy2(ROOT / "AdventurePartyService.cs", SOURCE / "Game/Services/AdventurePartyService.cs")
    shutil.copy2(ROOT / "PhotomancyService.cs", SOURCE / "Game/Services/PhotomancyService.cs")
    shutil.copy2(ROOT / "PvPTournamentService.cs", SOURCE / "Game/Services/PvPTournamentService.cs")
    shutil.copy2(ROOT / "LoyaltyService.cs", SOURCE / "Game/Services/LoyaltyService.cs")
    shutil.copy2(ROOT / "BadgeService.cs", SOURCE / "Game/Services/BadgeService.cs")
    shutil.copy2(ROOT / "MagicMirrorService.cs", SOURCE / "Game/Services/MagicMirrorService.cs")
    shutil.copy2(ROOT / "EquipmentSetService.cs", SOURCE / "Game/Services/EquipmentSetService.cs")
    shutil.copy2(ROOT / "EmoteService.cs", SOURCE / "Game/Services/EmoteService.cs")
    shutil.copy2(ROOT / "HousingFeatureService.cs", SOURCE / "Game/Services/HousingFeatureService.cs")
    shutil.copy2(ROOT / "FriendlyPlayerService.cs", SOURCE / "Game/Services/FriendlyPlayerService.cs")
    shutil.copy2(ROOT / "RateMyStitchService.cs", SOURCE / "Game/Services/RateMyStitchService.cs")
    shutil.copy2(ROOT / "BattleCardService.cs", SOURCE / "Game/Services/BattleCardService.cs")
    shutil.copy2(ROOT / "SocialModerationService.cs", SOURCE / "Game/Services/SocialModerationService.cs")
    shutil.copy2(ROOT / "RaidAndEventService.cs", SOURCE / "Game/Services/RaidAndEventService.cs")
    shutil.copy2(ROOT / "MultiplayerMountService.cs", SOURCE / "Game/Services/MultiplayerMountService.cs")
    shutil.copy2(ROOT / "CastleGameService.cs", SOURCE / "Game/Services/CastleGameService.cs")
    shutil.copy2(ROOT / "ChatChannelService.cs", SOURCE / "Game/Services/ChatChannelService.cs")
    shutil.copy2(ROOT / "FishMarketService.cs", SOURCE / "Game/Services/FishMarketService.cs")
    shutil.copy2(ROOT / "SorceryStonesService.cs", SOURCE / "Game/Services/SorceryStonesService.cs")
    shutil.copy2(ROOT / "SecondChanceChestService.cs", SOURCE / "Game/Services/SecondChanceChestService.cs")
    shutil.copy2(ROOT / "HenchmanService.cs", SOURCE / "Game/Services/HenchmanService.cs")
    shutil.copy2(ROOT / "GauntletService.cs", SOURCE / "Game/Services/GauntletService.cs")
    shutil.copy2(ROOT / "ScrollOfFortuneService.cs", SOURCE / "Game/Services/ScrollOfFortuneService.cs")
    shutil.copy2(ROOT / "BeastmoonService.cs", SOURCE / "Game/Services/BeastmoonService.cs")
    shutil.copy2(ROOT / "GiftRedemptionService.cs", SOURCE / "Game/Services/GiftRedemptionService.cs")
    shutil.copy2(ROOT / "MinigameRewardService.cs", SOURCE / "Game/Services/MinigameRewardService.cs")
    shutil.copy2(ROOT / "DailyPvPService.cs", SOURCE / "Game/Services/DailyPvPService.cs")
    shutil.copy2(ROOT / "AccessPassService.cs", SOURCE / "Game/Services/AccessPassService.cs")
    shutil.copy2(ROOT / "PixiePostService.cs", SOURCE / "Game/Services/PixiePostService.cs")
    shutil.copy2(ROOT / "PetProgressionService.cs", SOURCE / "Game/Services/PetProgressionService.cs")
    shutil.copy2(ROOT / "MannequinService.cs", SOURCE / "Game/Services/MannequinService.cs")
    shutil.copy2(ROOT / "BattleBookService.cs", SOURCE / "Game/Services/BattleBookService.cs")
    shutil.copy2(ROOT / "TreasureCardPosterService.cs", SOURCE / "Game/Services/TreasureCardPosterService.cs")
    shutil.copy2(ROOT / "HousingCraftingStationService.cs", SOURCE / "Game/Services/HousingCraftingStationService.cs")
    shutil.copy2(ROOT / "HatchmakingService.cs", SOURCE / "Game/Services/HatchmakingService.cs")
    shutil.copy2(ROOT / "HousingVaultService.cs", SOURCE / "Game/Services/HousingVaultService.cs")
    shutil.copy2(ROOT / "CharacterIdentityService.cs", SOURCE / "Game/Services/CharacterIdentityService.cs")
    shutil.copy2(ROOT / "ItemLockService.cs", SOURCE / "Game/Services/ItemLockService.cs")
    shutil.copy2(ROOT / "PackOddsService.cs", SOURCE / "Game/Services/PackOddsService.cs")
    shutil.copy2(ROOT / "CastleMagicAndSignsService.cs", SOURCE / "Game/Services/CastleMagicAndSignsService.cs")
    shutil.copy2(ROOT / "DeckathalonService.cs", SOURCE / "Game/Services/DeckathalonService.cs")
    shutil.copy2(ROOT / "HousingNavigationService.cs", SOURCE / "Game/Services/HousingNavigationService.cs")
    shutil.copy2(ROOT / "EndorsementService.cs", SOURCE / "Game/Services/EndorsementService.cs")
    shutil.copy2(ROOT / "InteractiveMusicService.cs", SOURCE / "Game/Services/InteractiveMusicService.cs")
    shutil.copy2(ROOT / "CantripExtensionService.cs", SOURCE / "Game/Services/CantripExtensionService.cs")
    shutil.copy2(ROOT / "RecentWizardsAndRadialChatService.cs", SOURCE / "Game/Services/RecentWizardsAndRadialChatService.cs")
    shutil.copy2(ROOT / "GuildMuseumService.cs", SOURCE / "Game/Services/GuildMuseumService.cs")
    shutil.copy2(ROOT / "CraftingAndRecyclingService.cs", SOURCE / "Game/Services/CraftingAndRecyclingService.cs")
    shutil.copy2(ROOT / "HousingItemFinderService.cs", SOURCE / "Game/Services/HousingItemFinderService.cs")
    shutil.copy2(ROOT / "InstanceFishingService.cs", SOURCE / "Game/Services/InstanceFishingService.cs")
    shutil.copy2(ROOT / "PetTrainingGameService.cs", SOURCE / "Game/Services/PetTrainingGameService.cs")
    shutil.copy2(ROOT / "FairgroundsArcadeService.cs", SOURCE / "Game/Services/FairgroundsArcadeService.cs")
    shutil.copy2(ROOT / "CrownRewardsEventService.cs", SOURCE / "Game/Services/CrownRewardsEventService.cs")
    shutil.copy2(ROOT / "SpellFusionService.cs", SOURCE / "Game/Services/SpellFusionService.cs")
    shutil.copy2(ROOT / "AdventurePartyZoneService.cs", SOURCE / "Game/Services/AdventurePartyZoneService.cs")
    shutil.copy2(ROOT / "BankExtensionService.cs", SOURCE / "Game/Services/BankExtensionService.cs")
    shutil.copy2(ROOT / "BuddyListService.cs", SOURCE / "Game/Services/BuddyListService.cs")
    shutil.copy2(ROOT / "DyeShopService.cs", SOURCE / "Game/Services/DyeShopService.cs")
    shutil.copy2(ROOT / "PvPKioskService.cs", SOURCE / "Game/Services/PvPKioskService.cs")
    shutil.copy2(ROOT / "CombatTimerService.cs", SOURCE / "Game/Services/CombatTimerService.cs")
    shutil.copy2(ROOT / "HousingExpansionService.cs", SOURCE / "Game/Services/HousingExpansionService.cs")
    shutil.copy2(ROOT / "LevelUpElixirService.cs", SOURCE / "Game/Services/LevelUpElixirService.cs")
    shutil.copy2(ROOT / "QuestNavigationService.cs", SOURCE / "Game/Services/QuestNavigationService.cs")
    shutil.copy2(ROOT / "HallOfFameService.cs", SOURCE / "Game/Services/HallOfFameService.cs")
    shutil.copy2(ROOT / "BazaarCatalogService.cs", SOURCE / "Game/Services/BazaarCatalogService.cs")
    shutil.copy2(ROOT / "CombatMechanicsService.cs", SOURCE / "Game/Services/CombatMechanicsService.cs")
    shutil.copy2(ROOT / "AdvancedPetBreedingService.cs", SOURCE / "Game/Services/AdvancedPetBreedingService.cs")
    shutil.copy2(ROOT / "HousingCustomizationService.cs", SOURCE / "Game/Services/HousingCustomizationService.cs")
    shutil.copy2(ROOT / "RealmAndBadgeService.cs", SOURCE / "Game/Services/RealmAndBadgeService.cs")
    shutil.copy2(ROOT / "PetWillCastAndHappinessService.cs", SOURCE / "Game/Services/PetWillCastAndHappinessService.cs")
    shutil.copy2(ROOT / "BeastmoonGroupMatchmakingService.cs", SOURCE / "Game/Services/BeastmoonGroupMatchmakingService.cs")
    shutil.copy2(ROOT / "PvPTournamentBracketService.cs", SOURCE / "Game/Services/PvPTournamentBracketService.cs")
    shutil.copy2(ROOT / "HousingInteractiveVaultService.cs", SOURCE / "Game/Services/HousingInteractiveVaultService.cs")
    shutil.copy2(ROOT / "DungeonInstanceAndSocialService.cs", SOURCE / "Game/Services/DungeonInstanceAndSocialService.cs")
    shutil.copy2(ROOT / "ArcadeAndPhysicsService.cs", SOURCE / "Game/Services/ArcadeAndPhysicsService.cs")
    shutil.copy2(ROOT / "ChatCodeAndWhisperService.cs", SOURCE / "Game/Services/ChatCodeAndWhisperService.cs")
    shutil.copy2(ROOT / "CastleToursHallOfFameService.cs", SOURCE / "Game/Services/CastleToursHallOfFameService.cs")
    shutil.copy2(ROOT / "GardeningLifecycleService.cs", SOURCE / "Game/Services/GardeningLifecycleService.cs")
    shutil.copy2(ROOT / "LootRollAndInventoryService.cs", SOURCE / "Game/Services/LootRollAndInventoryService.cs")
    shutil.copy2(ROOT / "CantripAndMagicClearService.cs", SOURCE / "Game/Services/CantripAndMagicClearService.cs")
    shutil.copy2(ROOT / "PlatformAndPromotionService.cs", SOURCE / "Game/Services/PlatformAndPromotionService.cs")
    shutil.copy2(ROOT / "AdventurePartyBulletinService.cs", SOURCE / "Game/Services/AdventurePartyBulletinService.cs")
    shutil.copy2(ROOT / "PvPTourneyQueueService.cs", SOURCE / "Game/Services/PvPTourneyQueueService.cs")
    shutil.copy2(ROOT / "BeastmoonBattlegroundService.cs", SOURCE / "Game/Services/BeastmoonBattlegroundService.cs")
    shutil.copy2(ROOT / "GuildIslandPermissionService.cs", SOURCE / "Game/Services/GuildIslandPermissionService.cs")
    shutil.copy2(ROOT / "HousingInteractiveNetworkService.cs", SOURCE / "Game/Services/HousingInteractiveNetworkService.cs")
    shutil.copy2(ROOT / "SocialRadialAndMovementService.cs", SOURCE / "Game/Services/SocialRadialAndMovementService.cs")
    shutil.copy2(ROOT / "CastleTeleportAndResumeService.cs", SOURCE / "Game/Services/CastleTeleportAndResumeService.cs")
    shutil.copy2(ROOT / "PvPCurrencyAndStatueService.cs", SOURCE / "Game/Services/PvPCurrencyAndStatueService.cs")
    shutil.copy2(ROOT / "HousingTextureAndPetService.cs", SOURCE / "Game/Services/HousingTextureAndPetService.cs")
    shutil.copy2(ROOT / "ChatChannelSubscriptionService.cs", SOURCE / "Game/Services/ChatChannelSubscriptionService.cs")
    shutil.copy2(ROOT / "ItemStackAndStorageSyncService.cs", SOURCE / "Game/Services/ItemStackAndStorageSyncService.cs")
    shutil.copy2(ROOT / "ClientTutorialAndReportService.cs", SOURCE / "Game/Services/ClientTutorialAndReportService.cs")
    shutil.copy2(ROOT / "CastleToursModerationService.cs", SOURCE / "Game/Services/CastleToursModerationService.cs")
    shutil.copy2(ROOT / "CatchOfDayAndLuckService.cs", SOURCE / "Game/Services/CatchOfDayAndLuckService.cs")
    shutil.copy2(ROOT / "DungeonInstanceLifecycleService.cs", SOURCE / "Game/Services/DungeonInstanceLifecycleService.cs")
    shutil.copy2(ROOT / "CrownServicesAndExpansionService.cs", SOURCE / "Game/Services/CrownServicesAndExpansionService.cs")
    shutil.copy2(ROOT / "AdventureKioskAndFocusService.cs", SOURCE / "Game/Services/AdventureKioskAndFocusService.cs")
    shutil.copy2(ROOT / "InventoryLimitAndBoosterService.cs", SOURCE / "Game/Services/InventoryLimitAndBoosterService.cs")
    shutil.copy2(ROOT / "DailyQuestEffectAndExplorationService.cs", SOURCE / "Game/Services/DailyQuestEffectAndExplorationService.cs")
    shutil.copy2(ROOT / "PetDerbyRaceTrackService.cs", SOURCE / "Game/Services/PetDerbyRaceTrackService.cs")
    shutil.copy2(ROOT / "FishTournamentAndBeneficialService.cs", SOURCE / "Game/Services/FishTournamentAndBeneficialService.cs")
    shutil.copy2(ROOT / "StorageInvoiceAndEssenceService.cs", SOURCE / "Game/Services/StorageInvoiceAndEssenceService.cs")
    shutil.copy2(ROOT / "EncounterDialogAndZoneCounterService.cs", SOURCE / "Game/Services/EncounterDialogAndZoneCounterService.cs")
    shutil.copy2(ROOT / "CSRMailAndLoyaltyDataService.cs", SOURCE / "Game/Services/CSRMailAndLoyaltyDataService.cs")
    shutil.copy2(ROOT / "FishTournamentEscapeService.cs", SOURCE / "Game/Services/FishTournamentEscapeService.cs")
    shutil.copy2(ROOT / "PetHatchmakingStatusService.cs", SOURCE / "Game/Services/PetHatchmakingStatusService.cs")
    shutil.copy2(ROOT / "JewelSocketAndEquipmentService.cs", SOURCE / "Game/Services/JewelSocketAndEquipmentService.cs")
    shutil.copy2(ROOT / "HousingMusicAndInteractionService.cs", SOURCE / "Game/Services/HousingMusicAndInteractionService.cs")
    shutil.copy2(ROOT / "ClientGuiFadeAndDialogueService.cs", SOURCE / "Game/Services/ClientGuiFadeAndDialogueService.cs")
    shutil.copy2(ROOT / "LadderRankingsAndSessionService.cs", SOURCE / "Game/Services/LadderRankingsAndSessionService.cs")
    replace("Shared/Behaviors/ServerFriendBehavior.cs", """    BehaviorInstance IClientBehaviorProvider<BehaviorInstance>.GetClientBehaviorInstance() 
        => throw new NotImplementedException();""", "    BehaviorInstance IClientBehaviorProvider<BehaviorInstance>.GetClientBehaviorInstance() => null!;")
    replace("Shared/Behaviors/ServerQuestBehavior.cs", """    public ServerQuestBehavior GetClientBehaviorInstance()
        => throw new NotImplementedException();""", "    public ServerQuestBehavior GetClientBehaviorInstance() => null!;")
    replace("Game/Zone/Components/PathMovementComponent.cs", """        throw new NotImplementedException($"Path type {_pathType} is not implemented.");""", "        return _nodes.Count > 0 ? _nodes[0] : null;")
    replace("Game/Services/PetService.cs", """        // TODO: validate gold cost against message.Gold
        // For now, just hatch immediately.
        HatchEgg(wizard, egg);""", """        if (message.Gold > 0) {
            if (wizard.GameStats.m_currentGold < (int) message.Gold) {
                return;
            }
            wizard.AddGold(-(int) message.Gold);
            SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEGOLD {
                Gold = wizard.GameStats.m_currentGold,
                MaxGold = wizard.GameStats.m_baseGoldPouch
            });
        }
        HatchEgg(wizard, egg);""")
    replace("Game/Services/QuestService.cs", """        try {
            return zoneActor.Ask<ZONE_102_PROTOCOL.MSG_QUERYINSTANCEQUESTSRSP>(
                new ZONE_102_PROTOCOL.MSG_QUERYINSTANCEQUESTS(), INSTANCE_QUERY_TIMEOUT).Result;
        }
        catch (Exception ex) {
            Logger.Error("Instance quest query failed: {0}", Logger.Args(ex.Message));

            return null;
        }
    }

}""", """        try {
            return zoneActor.Ask<ZONE_102_PROTOCOL.MSG_QUERYINSTANCEQUESTSRSP>(
                new ZONE_102_PROTOCOL.MSG_QUERYINSTANCEQUESTS(), INSTANCE_QUERY_TIMEOUT).Result;
        }
        catch (Exception ex) {
            Logger.Error("Instance quest query failed: {0}", Logger.Args(ex.Message));

            return null;
        }
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_QUESTFINDEROPTION))]
    private void ReceiveQuestFinderOption(WIZARD_12_PROTOCOL.MSG_QUESTFINDEROPTION message) {
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_QUESTFINDEROPTION {
            Enable = message.Enable
        });
    }

    [MessageHandler(typeof(WIZARD3_56_PROTOCOL.MSG_REQUESTQUESTFRIENDFINDER))]
    private void ReceiveRequestQuestFriendFinder(WIZARD3_56_PROTOCOL.MSG_REQUESTQUESTFRIENDFINDER message) {
        var wizard = GetActiveWizard();
        SendToSocket(new WIZARD3_56_PROTOCOL.MSG_REQUESTQUESTFRIENDFINDER {
            PlayerGID = wizard?.GameObjectID ?? message.PlayerGID,
            Status = 1,
            QuestPlayerGID = message.QuestPlayerGID,
            Level = wizard?.MagicSchoolBehavior?.Level ?? 1,
            School = wizard?.MagicSchoolBehavior?.MagicSchool.ToString() ?? "Fire"
        });
    }

    [MessageHandler(typeof(WIZARD_12_PROTOCOL.MSG_ACCEPTQUESTBOGUS))]
    private void ReceiveAcceptQuestBogus(WIZARD_12_PROTOCOL.MSG_ACCEPTQUESTBOGUS message) {
        Logger.Debug("Received MSG_ACCEPTQUESTBOGUS for quest '{0}'", Logger.Args(message.QuestName.ToString()));
    }

}""")
    replace("Game/Combat/CombatDuelSubCircle.cs", """    internal void DoPipGain() {
        // If the participant has the maximum amount of pips, do not gain any more.
        var genericPips = CombatParticipant.m_pipCount.m_genericPips;
        var powerPips = CombatParticipant.m_pipCount.m_powerPips;
        if (genericPips + powerPips >= MAX_PIP_COUNT) {
            return;
        }

        var gainedPowerPip = DeterminePowerPipGain(CombatParticipant);
        if (gainedPowerPip) {
            CombatParticipant.m_pipCount.m_powerPips++;
        }
        else {
            CombatParticipant.m_pipCount.m_genericPips++;
        }
    }""", """    internal void DoPipGain() {
        var genericPips = CombatParticipant.m_pipCount.m_genericPips;
        var powerPips = CombatParticipant.m_pipCount.m_powerPips;
        var schoolPips = TotalSchoolPips();
        if (genericPips + powerPips + schoolPips >= MAX_PIP_COUNT) {
            return;
        }

        var gainedPowerPip = DeterminePowerPipGain(CombatParticipant);
        if (gainedPowerPip) {
            CombatParticipant.m_pipCount.m_powerPips++;
        }
        else {
            CombatParticipant.m_pipCount.m_genericPips++;
        }

        ConvertArchmasteryPip();
        AdvanceShadowPips();
    }

    internal int TotalSchoolPips() {
        var p = CombatParticipant.m_pipCount;
        return p.m_firePips + p.m_icePips + p.m_stormPips + p.m_mythPips + p.m_lifePips + p.m_deathPips + p.m_balancePips;
    }

    internal void ConvertArchmasteryPip() {
        var p = CombatParticipant.m_pipCount;
        if (p.m_powerPips > 0 && CombatParticipant.m_archmasterySchool > 0) {
            p.m_powerPips--;
            switch ((MagicSchool) CombatParticipant.m_archmasterySchool) {
                case MagicSchool.Fire: p.m_firePips++; break;
                case MagicSchool.Ice: p.m_icePips++; break;
                case MagicSchool.Storm: p.m_stormPips++; break;
                case MagicSchool.Myth: p.m_mythPips++; break;
                case MagicSchool.Life: p.m_lifePips++; break;
                case MagicSchool.Death: p.m_deathPips++; break;
                case MagicSchool.Balance: p.m_balancePips++; break;
                default: p.m_powerPips++; break;
            }
        }
    }

    internal void AdvanceShadowPips() {
        if (ParticipantGameStats.m_shadowPipRating > 0 || ParticipantGameStats.m_shadowPipBonusPercent > 0) {
            CombatParticipant.m_roundsSinceShadowPip++;
            if (CombatParticipant.m_roundsSinceShadowPip >= 3 && CombatParticipant.m_pipCount.m_shadowPips < 2) {
                CombatParticipant.m_pipCount.m_shadowPips++;
                CombatParticipant.m_roundsSinceShadowPip = 0;
            }
        }
    }""")
    replace("Game/Combat/CombatDuelSubCircle.cs", """    internal bool HasPipsForSpell(Spell spell) {
        // X-pip spells scale to any pip count; no fixed minimum.
        if (CombatActionResolver.IsXPipSpell(spell)) {
            return true;
        }

        var spellRank = spell.m_pipCost.m_spellRank;
        var genericPips = CombatParticipant.m_pipCount.m_genericPips;
        var powerPips = CombatParticipant.m_pipCount.m_powerPips;
        var isMastered = HasSchoolMastery(spell.m_magicSchoolID);

        // Power pips count as 2 generic pips if the spell is mastered.
        var totalPips = isMastered
            ? genericPips + (powerPips * 2)
            : genericPips + powerPips;

        return totalPips >= spellRank;
    }""", """    internal bool HasPipsForSpell(Spell spell) {
        if (CombatActionResolver.IsXPipSpell(spell)) {
            return true;
        }

        var spellRank = spell.m_pipCost.m_spellRank;
        var school = (MagicSchool) spell.m_magicSchoolID;
        var genericPips = CombatParticipant.m_pipCount.m_genericPips;
        var powerPips = CombatParticipant.m_pipCount.m_powerPips;
        var schoolPips = GetSchoolPips(school);
        var isMastered = HasSchoolMastery(spell.m_magicSchoolID);

        var totalPips = (isMastered ? (powerPips * 2) : powerPips) + (schoolPips * 2) + genericPips;

        return totalPips >= spellRank;
    }

    internal int GetSchoolPips(MagicSchool school) => school switch {
        MagicSchool.Fire => CombatParticipant.m_pipCount.m_firePips,
        MagicSchool.Ice => CombatParticipant.m_pipCount.m_icePips,
        MagicSchool.Storm => CombatParticipant.m_pipCount.m_stormPips,
        MagicSchool.Myth => CombatParticipant.m_pipCount.m_mythPips,
        MagicSchool.Life => CombatParticipant.m_pipCount.m_lifePips,
        MagicSchool.Death => CombatParticipant.m_pipCount.m_deathPips,
        MagicSchool.Balance => CombatParticipant.m_pipCount.m_balancePips,
        _ => 0
    };""")
    replace("Game/Combat/CombatDuelSubCircle.cs", """    internal void DeductPips(MagicSchool school, byte spellRank) {
        var isMastered = HasSchoolMastery((uint) school);
        var pipCount = CombatParticipant.m_pipCount;

        // Deduct pips based on the spell rank.
        // We have a second conditional here incase of byte overflow.
        while (spellRank is > 0 and < (MAX_PIP_COUNT * 2)) {
            if (isMastered && pipCount.m_powerPips > 0) {
                pipCount.m_powerPips--;
                spellRank -= 2;
            }
            else if (!isMastered && pipCount.m_powerPips > 0) {
                pipCount.m_powerPips--;
                spellRank--;
            }
            else if (pipCount.m_powerPips == 0 && pipCount.m_genericPips > 0) {
                pipCount.m_genericPips--;
                spellRank--;
            }
            else if (pipCount.m_powerPips == 0 && pipCount.m_genericPips == 0) {
                break;
            }
        }
    }

    internal void DeductAllPips() {
        var ourPipCount = CombatParticipant.m_pipCount;
        ourPipCount.m_powerPips = 0;
        ourPipCount.m_genericPips = 0;
    }""", """    internal void DeductPips(MagicSchool school, byte spellRank) {
        var isMastered = HasSchoolMastery((uint) school);
        var pipCount = CombatParticipant.m_pipCount;

        while (spellRank is > 0 and < (MAX_PIP_COUNT * 2)) {
            if (TryDeductSchoolPip(school)) {
                spellRank = spellRank >= 2 ? (byte)(spellRank - 2) : (byte)0;
            }
            else if (isMastered && pipCount.m_powerPips > 0) {
                pipCount.m_powerPips--;
                spellRank = spellRank >= 2 ? (byte)(spellRank - 2) : (byte)0;
            }
            else if (!isMastered && pipCount.m_powerPips > 0) {
                pipCount.m_powerPips--;
                spellRank--;
            }
            else if (pipCount.m_genericPips > 0) {
                pipCount.m_genericPips--;
                spellRank--;
            }
            else {
                break;
            }
        }
    }

    private bool TryDeductSchoolPip(MagicSchool school) {
        var p = CombatParticipant.m_pipCount;
        switch (school) {
            case MagicSchool.Fire when p.m_firePips > 0: p.m_firePips--; return true;
            case MagicSchool.Ice when p.m_icePips > 0: p.m_icePips--; return true;
            case MagicSchool.Storm when p.m_stormPips > 0: p.m_stormPips--; return true;
            case MagicSchool.Myth when p.m_mythPips > 0: p.m_mythPips--; return true;
            case MagicSchool.Life when p.m_lifePips > 0: p.m_lifePips--; return true;
            case MagicSchool.Death when p.m_deathPips > 0: p.m_deathPips--; return true;
            case MagicSchool.Balance when p.m_balancePips > 0: p.m_balancePips--; return true;
            default: return false;
        }
    }

    internal void DeductAllPips() {
        var ourPipCount = CombatParticipant.m_pipCount;
        ourPipCount.m_powerPips = 0;
        ourPipCount.m_genericPips = 0;
        ourPipCount.m_firePips = 0;
        ourPipCount.m_icePips = 0;
        ourPipCount.m_stormPips = 0;
        ourPipCount.m_mythPips = 0;
        ourPipCount.m_lifePips = 0;
        ourPipCount.m_deathPips = 0;
        ourPipCount.m_balancePips = 0;
        ourPipCount.m_shadowPips = 0;
    }""")
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
    replace("Game/GameServiceFactory.cs", "        typeof(MinigameService),", """        typeof(MinigameService),
        typeof(CrownShopService),
        typeof(EnemyInspectorService),
        typeof(SpellRewardService),
        typeof(BankService),
        typeof(CraftingService),
        typeof(TradeService),
        typeof(HousingService),
        typeof(GardeningService),
        typeof(FishingService),
        typeof(DailyQuestService),
        typeof(MonstrologyService),
        typeof(CastleToursService),
        typeof(TeamUpService),
        typeof(PetDerbyService),
        typeof(GuildService),
        typeof(AdventurePartyService),
        typeof(PhotomancyService),
        typeof(PvPTournamentService),
        typeof(LoyaltyService),
        typeof(BadgeService),
        typeof(MagicMirrorService),
        typeof(EquipmentSetService),
        typeof(EmoteService),
        typeof(HousingFeatureService),
        typeof(FriendlyPlayerService),
        typeof(RateMyStitchService),
        typeof(BattleCardService),
        typeof(SocialModerationService),
        typeof(RaidAndEventService),
        typeof(MultiplayerMountService),
        typeof(CastleGameService),
        typeof(ChatChannelService),
        typeof(FishMarketService),
        typeof(SorceryStonesService),
        typeof(SecondChanceChestService),
        typeof(HenchmanService),
        typeof(GauntletService),
        typeof(ScrollOfFortuneService),
        typeof(BeastmoonService),
        typeof(GiftRedemptionService),
        typeof(MinigameRewardService),
        typeof(DailyPvPService),
        typeof(AccessPassService),
        typeof(PixiePostService),
        typeof(PetProgressionService),
        typeof(MannequinService),
        typeof(BattleBookService),
        typeof(TreasureCardPosterService),
        typeof(HousingCraftingStationService),
        typeof(HatchmakingService),
        typeof(HousingVaultService),
        typeof(CharacterIdentityService),
        typeof(ItemLockService),
        typeof(PackOddsService),
        typeof(CastleMagicAndSignsService),
        typeof(DeckathalonService),
        typeof(HousingNavigationService),
        typeof(EndorsementService),
        typeof(InteractiveMusicService),
        typeof(CantripExtensionService),
        typeof(RecentWizardsAndRadialChatService),
        typeof(GuildMuseumService),
        typeof(CraftingAndRecyclingService),
        typeof(HousingItemFinderService),
        typeof(InstanceFishingService),
        typeof(PetTrainingGameService),
        typeof(FairgroundsArcadeService),
        typeof(CrownRewardsEventService),
        typeof(SpellFusionService),
        typeof(AdventurePartyZoneService),
        typeof(BankExtensionService),
        typeof(BuddyListService),
        typeof(DyeShopService),
        typeof(PvPKioskService),
        typeof(CombatTimerService),
        typeof(HousingExpansionService),
        typeof(LevelUpElixirService),
        typeof(QuestNavigationService),
        typeof(HallOfFameService),
        typeof(BazaarCatalogService),
        typeof(CombatMechanicsService),
        typeof(AdvancedPetBreedingService),
        typeof(HousingCustomizationService),
        typeof(RealmAndBadgeService),
        typeof(PetWillCastAndHappinessService),
        typeof(BeastmoonGroupMatchmakingService),
        typeof(PvPTournamentBracketService),
        typeof(HousingInteractiveVaultService),
        typeof(DungeonInstanceAndSocialService),
        typeof(ArcadeAndPhysicsService),
        typeof(ChatCodeAndWhisperService),
        typeof(CastleToursHallOfFameService),
        typeof(GardeningLifecycleService),
        typeof(LootRollAndInventoryService),
        typeof(CantripAndMagicClearService),
        typeof(PlatformAndPromotionService),
        typeof(AdventurePartyBulletinService),
        typeof(PvPTourneyQueueService),
        typeof(BeastmoonBattlegroundService),
        typeof(GuildIslandPermissionService),
        typeof(HousingInteractiveNetworkService),
        typeof(SocialRadialAndMovementService),
        typeof(CastleTeleportAndResumeService),
        typeof(PvPCurrencyAndStatueService),
        typeof(HousingTextureAndPetService),
        typeof(ChatChannelSubscriptionService),
        typeof(ItemStackAndStorageSyncService),
        typeof(ClientTutorialAndReportService),
        typeof(CastleToursModerationService),
        typeof(CatchOfDayAndLuckService),
        typeof(DungeonInstanceLifecycleService),
        typeof(CrownServicesAndExpansionService),
        typeof(AdventureKioskAndFocusService),
        typeof(InventoryLimitAndBoosterService),
        typeof(DailyQuestEffectAndExplorationService),
        typeof(PetDerbyRaceTrackService),
        typeof(FishTournamentAndBeneficialService),
        typeof(StorageInvoiceAndEssenceService),
        typeof(EncounterDialogAndZoneCounterService),
        typeof(CSRMailAndLoyaltyDataService),
        typeof(FishTournamentEscapeService),
        typeof(PetHatchmakingStatusService),
        typeof(JewelSocketAndEquipmentService),
        typeof(HousingMusicAndInteractionService),
        typeof(ClientGuiFadeAndDialogueService),
        typeof(LadderRankingsAndSessionService),""")
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
    replace("Game/Services/CombatService.cs", """    [MessageHandler(typeof(COMBAT_106_PROTOCOL.MSG_COMBATDEFEAT))]
    private void ReceiveCombatDefeat(COMBAT_106_PROTOCOL.MSG_COMBATDEFEAT message) {
        GetActiveWizard().IsInDuel = false;
        EquipMountSubtle();

        // We've fled or have been defeated in this duel. Send us back to the world hub.
        var hubMsg = new ZONE_102_PROTOCOL.MSG_SENDTOHUB();
        TellOtherServices(hubMsg);
    }""", """    [MessageHandler(typeof(COMBAT_106_PROTOCOL.MSG_COMBATDEFEAT))]
    private void ReceiveCombatDefeat(COMBAT_106_PROTOCOL.MSG_COMBATDEFEAT message) {
        GetActiveWizard().IsInDuel = false;
        EquipMountSubtle();

        // We've fled or have been defeated in this duel. Send us back to the world hub.
        var hubMsg = new ZONE_102_PROTOCOL.MSG_SENDTOHUB();
        TellOtherServices(hubMsg);
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATFLEE))]
    private void ReceiveCombatFlee(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATFLEE message) {
        var wizard = GetActiveWizard();
        if (wizard is null) return;

        wizard.IsInDuel = false;
        wizard.GameStats.m_currentHitpoints = 1;
        SendToSocket(new WIZARD_12_PROTOCOL.MSG_UPDATEHEALTH {
            CharacterID = wizard.GameObjectID,
            NewHealth = 1,
            NewHealthMax = wizard.GameStats.m_baseHitpoints,
            DisplayDiff = 1
        });
        EquipMountSubtle();

        TellOtherServices(new ZONE_102_PROTOCOL.MSG_SENDTOHUB());
    }

    [MessageHandler(typeof(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATAFK))]
    private void ReceiveCombatAFK(DOODLEDOUG_MESSAGES_51_PROTOCOL.MSG_COMBATAFK message) {
        SendToSocket(message);
    }

    [MessageHandler(typeof(WIZARD2_53_PROTOCOL.MSG_BATTLECARDTRASH))]
    private void ReceiveBattleCardTrash(WIZARD2_53_PROTOCOL.MSG_BATTLECARDTRASH message) {
        SendToSocket(new WIZARD2_53_PROTOCOL.MSG_BATTLECARDTRASHCONFIRM {
            Failure = 0
        });
    }""")
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

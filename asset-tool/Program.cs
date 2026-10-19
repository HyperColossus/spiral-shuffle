using System.IO.Compression;
using System.Text.Json;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Cryptography;
using Imcodec.Wad;

internal sealed record Creature(uint Id, string Name, int Level, int Health, bool Boss);
internal sealed record Equipment(uint Id, string Name, string Slot, string School, int Level);
internal sealed record Swap(uint From, uint To, string Source, string Target);
internal sealed record CreatureSwap(uint From, uint To, string Source, string Target, uint Alias);
internal sealed record SpellVariant(uint Original, int Difficulty, uint TemplateId, double Multiplier);

internal static class Program {
    private static readonly BindSerializer Codec = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly JsonDocumentOptions JsonReadOptions = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };

    private static int Main(string[] args) {
        if (args.Length != 3) {
            Console.Error.WriteLine("Usage: AssetTool INSTALL_ROOT OUTPUT_DIRECTORY SEED");
            return 2;
        }

        string root = Path.GetFullPath(args[0]);
        string output = Path.GetFullPath(args[1]);
        string seed = args[2];
        Directory.CreateDirectory(output);
        string data = Path.Combine(root, "Aurorium/data/V_r806919.Wizard_1_610/Data/GameData");
        using FileStream stream = File.OpenRead(Path.Combine(data, "Root.wad"));
        Archive archive = ArchiveParser.Parse(stream) ?? throw new InvalidDataException("Invalid Root.wad");
        TemplateManifest manifest = Read<TemplateManifest>(archive, "TemplateManifest.xml")!;
        Dictionary<uint, TemplateLocation> locations = manifest.m_serializedTemplates
            .GroupBy(location => location.m_id).ToDictionary(group => group.Key, group => group.First());
        HashSet<uint> playerSpells = PlayerSpells(root);
        Dictionary<string, WizardStatTable> statTables = archive.Files.Keys
            .Where(path => path.StartsWith("GameEffectRuleData/", StringComparison.Ordinal) && path.EndsWith(".xml"))
            .Select(path => Read<WizardStatTable>(archive, path)).OfType<WizardStatTable>()
            .GroupBy(table => table.m_tableName.ToString()).ToDictionary(group => group.Key, group => group.First());
        GameEffectTemplateList effectList = Read<GameEffectTemplateList>(archive, "GameEffectData/CanonicalStatEffects.xml")!;
        Dictionary<string, string> effectTables = effectList.m_effectTemplates.OfType<WizStatisticEffectTemplate>()
            .ToDictionary(effect => effect.m_effectName.ToString(), effect => effect.m_statTableName.ToString());
        List<Creature> creatures = [];
        List<Equipment> equipment = [];
        List<GearAsset> gear = [];
        Dictionary<uint, (SpellTemplate Template, string Path)> spells = [];
        List<Swap> spellSwaps = [];
        List<CreatureSwap> creatureSwaps = [];
        List<SpellVariant> spellVariants = [];
        BindDocument rawManifest = ReadBinary(archive, "TemplateManifest.xml");
        BindProperty locationProperty = rawManifest.Value.Get<TemplateManifest>("m_serializedTemplates");
        List<BindObject> rawLocations = BindDocument.ReadVector(locationProperty);
        Dictionary<uint, BindObject> locationRecords = manifest.m_serializedTemplates.Zip(rawLocations)
            .GroupBy(pair => pair.First.m_id).ToDictionary(group => group.Key, group => group.First().Second);
        uint nextAlias = 2_000_000_000;
        HashSet<uint> occupiedIds = locations.Keys.ToHashSet();
        int gearRolls = 0;
        int rejected = 0;
        string zipPath = Path.Combine(output, "overrides.zip");
        if (File.Exists(zipPath)) {
            File.Move(zipPath, zipPath + ".previous", true);
        }
        using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create)) {
            foreach ((uint id, TemplateLocation location) in locations.OrderBy(pair => pair.Key)) {
                string path = location.m_filename.ToString();
                if (!path.StartsWith("ObjectData/", StringComparison.Ordinal) && !playerSpells.Contains(id)) {
                    continue;
                }

                CoreTemplate? template;
                try {
                    template = Read<CoreTemplate>(archive, path);
                }
                catch (Exception) {
                    rejected++;
                    continue;
                }

                if (template is SpellTemplate spell && playerSpells.Contains(id) && IsPlayerSpell(spell)) {
                    spells[id] = (spell, path);
                }

                if (template is not GameObjectTemplate gameObject) {
                    continue;
                }

                NPCBehaviorTemplate? npc = gameObject.m_behaviors.OfType<NPCBehaviorTemplate>().FirstOrDefault();
                bool combat = gameObject.m_behaviors.OfType<DuelistBehaviorTemplate>().Any();
                if (npc is not null && combat && npc.m_nStartingHealth > 0 && npc.m_nLevel > 0 &&
                    !gameObject.m_behaviors.Any(behavior => behavior is not null && IsServiceBehavior(behavior.GetType().Name))) {
                    creatures.Add(new(id, gameObject.m_objectName.ToString(), npc.m_nLevel,
                        npc.m_nStartingHealth, npc.m_bossMob));
                }

                if (template is not WizItemTemplate item || item.m_avatarInfo is not WizAvatarItemInfo avatar) {
                    continue;
                }

                string slot = avatar.m_slotName.ToString();
                if (!new[] { "Hat", "Robe", "Shoes", "Wand", "Athame", "Amulet", "Ring", "Deck" }.Contains(slot)) {
                    continue;
                }

                int level = RequiredLevel(item.m_equipRequirements);
                equipment.Add(new(id, item.m_displayName.ToString(), slot, item.m_school.ToString(), level));
                gear.Add(new(id, path, item, level));
            }

            GearRandomizer gearRandomizer = new(gear, effectTables, statTables,
                path => ReadBinary(archive, path), key => Unit(seed, key));
            foreach ((string path, BindDocument document) in gearRandomizer.Build()) {
                Write(zip, path, document);
                gearRolls++;
            }

            HashSet<string> itemSpellNames = gear.SelectMany(item => item.Template.m_equipEffects)
                .OfType<ProvideSpellEffectInfo>().Select(effect => effect.m_spellName.ToString()).ToHashSet();
            foreach ((uint id, TemplateLocation location) in locations.Where(pair =>
                !spells.ContainsKey(pair.Key) && pair.Value.m_filename.ToString().StartsWith("Spells/", StringComparison.Ordinal))) {
                SpellTemplate? spell = Read<SpellTemplate>(archive, location.m_filename.ToString());
                if (spell is not null && itemSpellNames.Contains(spell.m_name.ToString()) && IsPlayerSpell(spell)) {
                    spells[id] = (spell, location.m_filename.ToString());
                }
            }

            foreach ((uint id, (SpellTemplate original, string path)) in spells.OrderBy(pair => pair.Key)) {
                string role = SpellRules.Role(original);
                if (role == "protected") {
                    continue;
                }

                var candidates = spells.Where(pair => pair.Key != id &&
                    (!playerSpells.Contains(id) || playerSpells.Contains(pair.Key)) &&
                    SpellRules.Compatible(original, pair.Value.Template))
                    .OrderBy(pair => pair.Key).ToArray();
                if (candidates.Length == 0) {
                    continue;
                }

                var chosen = candidates[(int)(Unit(seed, $"spell:{id}") * candidates.Length)];
                BindDocument replacement = ReadBinary(archive, path);
                BindDocument donorSpell = ReadBinary(archive, chosen.Value.Path);
                foreach (string property in new[] {
                    "m_description", "m_advancedDescription", "m_displayName", "m_spellBase", "m_effects",
                    "m_sMagicSchoolName", "m_sTypeName", "m_accuracy", "m_imageIndex", "m_imageName",
                    "m_boosterPackIcon", "m_descriptionTrainer", "m_descriptionCombatHUD", "m_behaviors"
                }) {
                    replacement.Value.Set<SpellTemplate>(property, donorSpell.Value.Find<SpellTemplate>(property));
                }

                for (int difficulty = 1; difficulty <= 3; difficulty++) {
                    double multiplier = SpellStatRandomizer.Multiplier(difficulty, Unit(seed, $"spell-stats:{id}"));
                    BindDocument variant = BindDocument.Read(replacement.Encode());
                    SpellStatRandomizer.Apply(variant, chosen.Value.Template, multiplier);
                    uint variantId = id;
                    string variantPath = path;
                    if (difficulty > 1) {
                        string variantName = $"SpiralShuffle{difficulty}_{id}";
                        variantId = StringHash.Compute(variantName);
                        for (int suffix = 1; !occupiedIds.Add(variantId); suffix++) {
                            variantName = $"SpiralShuffle{difficulty}_{id}_{suffix}";
                            variantId = StringHash.Compute(variantName);
                        }

                        BindProperty spellName = variant.Value.Get<SpellTemplate>("m_name");
                        variant.Value.Set<SpellTemplate>("m_name", spellName.WithString(variantName));
                        variantPath = $"ObjectData/Randomizer/Spell{variantId}.xml";
                        rawLocations.Add(AliasLocation(locationRecords[id], variantId, variantPath));
                    }

                    Write(zip, variantPath, variant);
                    spellVariants.Add(new(id, difficulty, variantId, multiplier));
                }

                spellSwaps.Add(new(id, chosen.Key, original.m_name.ToString(), chosen.Value.Template.m_name.ToString()));
            }

            foreach (Creature original in creatures) {
                Creature[] candidates = creatures.Where(candidate => candidate.Id != original.Id &&
                    candidate.Boss == original.Boss && Math.Abs(candidate.Level - original.Level) <= 8 &&
                    candidate.Health >= original.Health * 0.65 && candidate.Health <= original.Health * 1.5)
                    .OrderBy(candidate => candidate.Id).ToArray();
                if (candidates.Length == 0) {
                    continue;
                }

                Creature donor = candidates[(int)(Unit(seed, $"creature:{original.Id}") * candidates.Length)];
                string path = locations[original.Id].m_filename.ToString();
                GameObjectTemplate modified = Read<GameObjectTemplate>(archive, path)!;
                GameObjectTemplate source = Read<GameObjectTemplate>(archive, locations[donor.Id].m_filename.ToString())!;
                BindDocument rawModified = ReadBinary(archive, path);
                BindDocument rawSource = ReadBinary(archive, locations[donor.Id].m_filename.ToString());
                BindProperty behaviorProperty = rawModified.Value.Get<GameObjectTemplate>("m_behaviors");
                List<BindObject> rawBehaviors = BindDocument.ReadVector(behaviorProperty);
                List<BindObject> donorBehaviors = BindDocument.ReadVector(rawSource.Value.Get<GameObjectTemplate>("m_behaviors"));
                for (int index = 0; index < modified.m_behaviors.Count; index++) {
                    BehaviorTemplate? behavior = modified.m_behaviors[index];
                    if (behavior is null) {
                        continue;
                    }

                    string type = behavior.GetType().Name;
                    if (type is "RenderBehaviorTemplate" or "EquipmentBehaviorTemplate" or "MobDeckBehaviorTemplate" or "DeckBehaviorTemplate" ||
                        type.Contains("Animation", StringComparison.Ordinal)) {
                        BehaviorTemplate? replacement = source.m_behaviors.FirstOrDefault(candidate =>
                            candidate is not null && candidate.GetType() == behavior.GetType());
                        if (replacement is not null) {
                            rawBehaviors[index] = donorBehaviors.First(candidate => candidate.Hash == replacement.GetHash());
                        }
                    }
                }

                uint npcHash = modified.m_behaviors.OfType<NPCBehaviorTemplate>().First().GetHash();
                BindObject npc = rawBehaviors.First(behavior => behavior.Hash == npcHash);
                BindObject donorNpc = donorBehaviors.First(behavior => behavior.Hash == npcHash);
                npc.Set<NPCBehaviorTemplate>("m_schoolOfFocus", donorNpc.Get<NPCBehaviorTemplate>("m_schoolOfFocus"));
                rawModified.Value.Set<GameObjectTemplate>("m_behaviors", BindDocument.WriteVector(behaviorProperty, rawBehaviors));
                rawModified.Value.Set<GameObjectTemplate>("m_displayName", rawSource.Value.Get<GameObjectTemplate>("m_displayName"));
                rawModified.Value.Set<GameObjectTemplate>("m_sIcon", rawSource.Value.Get<GameObjectTemplate>("m_sIcon"));
                uint alias = AllocateAlias(occupiedIds, ref nextAlias);
                string aliasPath = $"ObjectData/Randomizer/Creature{alias}.xml";
                BindProperty templateId = rawModified.Value.Get<GameObjectTemplate>("m_templateID");
                rawModified.Value.Set<GameObjectTemplate>("m_templateID", templateId.WithUnsigned(alias));
                Write(zip, aliasPath, rawModified);
                rawLocations.Add(AliasLocation(locationRecords[original.Id], alias, aliasPath));
                creatureSwaps.Add(new(original.Id, donor.Id, original.Name, donor.Name, alias));
            }

            rawManifest.Value.Set<TemplateManifest>("m_serializedTemplates", BindDocument.WriteVector(locationProperty, rawLocations));
            Write(zip, "TemplateManifest.xml", rawManifest);
        }

        File.WriteAllText(Path.Combine(output, "catalog.json"), JsonSerializer.Serialize(new {
            Seed = seed, Creatures = creatures, Equipment = equipment, SpellSwaps = spellSwaps, CreatureSwaps = creatureSwaps,
            SpellVariants = spellVariants, GearRolls = gearRolls, RejectedTemplates = rejected,
        }, JsonOptions));
        Console.WriteLine($"Built {spellSwaps.Count} spell swaps, {gearRolls} gear rolls, " +
            $"{creatureSwaps.Count} creature swaps, {equipment.Count} loot candidates.");
        return 0;
    }

    private static uint AllocateAlias(ISet<uint> occupiedIds, ref uint nextAlias) {
        while (!occupiedIds.Add(nextAlias)) {
            nextAlias++;
        }

        return nextAlias++;
    }

    private static BindObject AliasLocation(BindObject source, uint id, string path) {
        BindObject result = BindObject.Read(source.Encode());
        result.Set<TemplateLocation>("m_id", result.Get<TemplateLocation>("m_id").WithUnsigned(id));
        result.Set<TemplateLocation>("m_filename", result.Get<TemplateLocation>("m_filename").WithString(path));
        return result;
    }

    private static bool IsServiceBehavior(string name) =>
        new[] { "Shop", "Trainer", "Vendor", "Quest", "Service", "Minigame", "Teleport", "Pet" }
            .Any(part => name.Contains(part, StringComparison.OrdinalIgnoreCase));

    private static bool IsPlayerSpell(SpellTemplate spell) => !spell.m_Treasure && !spell.m_PvP &&
        !spell.m_battlegroundsOnly && !spell.m_alwaysFizzle && !spell.m_spellRank.m_xPipSpell &&
        spell.m_spellRank.m_shadowPips == 0 && !spell.m_name.ToString().Contains("PvP", StringComparison.OrdinalIgnoreCase) &&
        !SpellRules.Effects(spell.m_effects).Any(effect => effect is VariableSpellEffect);

    private static int RequiredLevel(RequirementList? requirements) {
        if (requirements is null) {
            return 0;
        }

        return requirements.m_requirements.OfType<ReqMagicLevel>()
            .Select(requirement => (int)requirement.m_numericValue).DefaultIfEmpty(0).Max();
    }

    private static HashSet<uint> PlayerSpells(string root) {
        HashSet<uint> ids = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "Imlight/spiraldb-cache/NpcSpellInventory"), "*.json")) {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(file), JsonReadOptions);
            foreach (JsonElement spell in document.RootElement.GetProperty("Spells").EnumerateArray()) {
                ids.Add(spell.GetProperty("TemplateID").GetUInt32());
            }
        }

        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "Imlight/spiraldb-cache/QuestTemplates"), "*.json")) {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(file), JsonReadOptions);
            AddQuestSpells(document.RootElement, ids);
        }

        return ids;
    }

    private static void AddQuestSpells(JsonElement value, ISet<uint> ids) {
        if (value.ValueKind == JsonValueKind.Array) {
            foreach (JsonElement child in value.EnumerateArray()) {
                AddQuestSpells(child, ids);
            }
        }
        else if (value.ValueKind == JsonValueKind.Object) {
            if (value.TryGetProperty("$type", out JsonElement type) &&
                type.GetString()?.Split(',')[0].EndsWith(".ResLearnSpell", StringComparison.Ordinal) == true &&
                value.TryGetProperty("m_templateID", out JsonElement id)) {
                ids.Add(id.GetUInt32());
            }

            foreach (JsonProperty child in value.EnumerateObject()) {
                AddQuestSpells(child.Value, ids);
            }
        }
    }

    private static T? Read<T>(Archive archive, string path) where T : PropertyClass {
        if (!archive.Files.ContainsKey(path)) {
            return null;
        }

        Memory<byte>? bytes = archive.OpenFile(path);
        return bytes.HasValue && Codec.Deserialize(bytes.Value.ToArray(), PropertyFlags.Prop_Save, out T value) ? value : null;
    }

    private static BindDocument ReadBinary(Archive archive, string path) =>
        BindDocument.Read(archive.OpenFile(path)!.Value.ToArray());

    private static void Write(ZipArchive zip, string path, BindDocument value) {
        byte[] bytes = value.Encode();
        if (!Codec.Deserialize(bytes, PropertyFlags.Prop_Save, out PropertyClass _)) {
            throw new InvalidDataException($"Failed round trip: {path}");
        }

        using Stream stream = zip.CreateEntry(path, CompressionLevel.Fastest).Open();
        stream.Write(bytes);
    }

    private static double Unit(string seed, string key) {
        byte[] bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(seed + ":" + key));
        return BitConverter.ToUInt32(bytes, 0) / 4294967296.0;
    }
}

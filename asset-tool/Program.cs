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
internal sealed record DropSpellAsset(uint Original, string Name, string School, uint[] Variants, double[] Multipliers);

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
        HashSet<uint> dropSpellIds = DropSpellCandidates(root, playerSpells);
        foreach ((uint id, TemplateLocation location) in locations) {
            if (new[] { "Spells/Tiered Spells/", "Spells/Trained Spells/", "Spells/Shadow Spells/" }
                .Any(prefix => location.m_filename.ToString().StartsWith(prefix, StringComparison.Ordinal))) {
                dropSpellIds.Add(id);
            }
        }
        HashSet<uint> shuffleSpellIds = new(playerSpells);
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
        List<DropSpellAsset> dropSpells = [];
        List<EquipmentCardAsset> equipmentCards = [];
        Dictionary<string, BindDocument> randomizedGear = [];
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
                if (!path.StartsWith("ObjectData/", StringComparison.Ordinal) && !path.StartsWith("Spells/", StringComparison.Ordinal)) {
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

                if (template is SpellTemplate spell && IsNumberedPlayerSpell(spell, path)) {
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
                randomizedGear.Add(path, document);
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

            SpellDescriptions descriptions = new(archive);
            dropSpells = BuildDropSpells(archive, locations, dropSpellIds, occupiedIds, rawLocations, locationRecords, zip, seed, descriptions);
            EquipmentSpellSource[] itemCardSpells = dropSpells.Select(spell => new EquipmentSpellSource(spell.Original,
                locations[spell.Original].m_filename, Read<SpellTemplate>(archive, locations[spell.Original].m_filename)!)).ToArray();
            EquipmentCardBuilder itemCards = new(gear, randomizedGear, itemCardSpells,
                path => ReadBinary(archive, path), key => Unit(seed, key), name => AllocateSpellName(name, occupiedIds));
            foreach (EquipmentCardBuild build in itemCards.Build()) {
                equipmentCards.Add(build.Card);
                foreach (EquipmentCardFile file in build.Files) {
                    if (build.Card.Spells.Contains(file.Id)) {
                        descriptions.Apply(file.Document);
                    }
                    Write(zip, file.Path, file.Document);
                    rawLocations.Add(AliasLocation(locationRecords[file.Prototype], file.Id, file.Path));
                }
            }

            foreach ((uint id, (SpellTemplate original, string path)) in spells.OrderBy(pair => pair.Key)) {
                var candidates = (shuffleSpellIds.Contains(id) || itemSpellNames.Contains(original.m_name)) &&
                    IsPlayerSpell(original) && SpellRules.Role(original) != "protected"
                    ? spells.Where(pair => pair.Key != id && (shuffleSpellIds.Contains(pair.Key) || itemSpellNames.Contains(pair.Value.Template.m_name)) && IsPlayerSpell(pair.Value.Template) &&
                        (!shuffleSpellIds.Contains(id) || shuffleSpellIds.Contains(pair.Key)) &&
                        SpellRules.Compatible(original, pair.Value.Template)).OrderBy(pair => pair.Key).ToArray()
                    : [];
                var chosen = candidates.Length == 0
                    ? new KeyValuePair<uint, (SpellTemplate Template, string Path)>(id, (original, path))
                    : candidates[(int)(Unit(seed, $"spell:{id}") * candidates.Length)];
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
                    SpellStatRandomizer.Apply(variant, chosen.Value.Template, multiplier, true);
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

                    descriptions.Apply(variant);
                    Write(zip, variantPath, variant);
                    spellVariants.Add(new(id, difficulty, variantId, multiplier));
                }

                if (chosen.Key != id) {
                    spellSwaps.Add(new(id, chosen.Key, original.m_name.ToString(), chosen.Value.Template.m_name.ToString()));
                }
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

            descriptions.Write(zip);
            rawManifest.Value.Set<TemplateManifest>("m_serializedTemplates", BindDocument.WriteVector(locationProperty, rawLocations));
            Write(zip, "TemplateManifest.xml", rawManifest);
        }

        File.WriteAllText(Path.Combine(output, "catalog.json"), JsonSerializer.Serialize(new {
            Seed = seed, Creatures = creatures, Equipment = equipment, SpellSwaps = spellSwaps, CreatureSwaps = creatureSwaps,
            SpellVariants = spellVariants, DropSpells = dropSpells, EquipmentCards = equipmentCards,
            GearRolls = gearRolls, RejectedTemplates = rejected,
        }, JsonOptions));
        Console.WriteLine($"Built {spellSwaps.Count} spell swaps, {gearRolls} gear rolls, " +
            $"{creatureSwaps.Count} creature swaps, {equipment.Count} loot candidates, {dropSpells.Count} spell-drop candidates, " +
            $"{equipmentCards.Count} equipment item-card rolls.");
        return 0;
    }

    private static uint AllocateAlias(ISet<uint> occupiedIds, ref uint nextAlias) {
        while (!occupiedIds.Add(nextAlias)) {
            nextAlias++;
        }

        return nextAlias++;
    }

    private static (uint Id, string Name) AllocateSpellName(string name, ISet<uint> occupiedIds) {
        string candidate = name;
        uint id = StringHash.Compute(candidate);
        for (int suffix = 1; !occupiedIds.Add(id); suffix++) {
            candidate = $"{name}_{suffix}";
            id = StringHash.Compute(candidate);
        }

        return (id, candidate);
    }

    private static BindObject AliasLocation(BindObject source, uint id, string path) {
        BindObject result = BindObject.Read(source.Encode());
        result.Set<TemplateLocation>("m_id", result.Get<TemplateLocation>("m_id").WithUnsigned(id));
        result.Set<TemplateLocation>("m_filename", result.Get<TemplateLocation>("m_filename").WithString(path));
        return result;
    }

    private static List<DropSpellAsset> BuildDropSpells(Archive archive, Dictionary<uint, TemplateLocation> locations,
        HashSet<uint> playerSpells, HashSet<uint> occupiedIds, List<BindObject> rawLocations,
        Dictionary<uint, BindObject> locationRecords, ZipArchive zip, string seed, SpellDescriptions descriptions) {
        List<DropSpellAsset> result = [];
        HashSet<(string School, string Name)> seen = [];
        foreach (uint originalId in playerSpells.Order()) {
            if (!locations.TryGetValue(originalId, out TemplateLocation? location) ||
                !location.m_filename.ToString().StartsWith("Spells/", StringComparison.Ordinal)) {
                continue;
            }

            SpellTemplate? spell = Read<SpellTemplate>(archive, location.m_filename);
            if (spell is null || spell.m_Treasure || spell.m_PvP || spell.m_battlegroundsOnly ||
                spell.m_alwaysFizzle || spell.m_effects is not { Count: > 0 } || spell.m_displayName.IsEmpty ||
                new[] { "MOB", "Boss", "Test", "CHEAT", "QA_", "BG_", "Debug", "100K" }
                    .Any(token => spell.m_name.ToString().Contains(token, StringComparison.OrdinalIgnoreCase)) ||
                new[] { "Battleground", "Fight Specific", "/Mobs/", "CombatTrigger", "Minion Specific" }
                    .Any(token => location.m_filename.ToString().Contains(token, StringComparison.OrdinalIgnoreCase)) ||
                !seen.Add((spell.m_sMagicSchoolName, spell.m_displayName))) {
                continue;
            }

            uint[] variants = new uint[3];
            double[] multipliers = new double[3];
            for (int difficulty = 1; difficulty <= 3; difficulty++) {
                string name = $"SpiralDrop{difficulty}_{originalId}";
                uint id = StringHash.Compute(name);
                for (int suffix = 1; !occupiedIds.Add(id); suffix++) {
                    name = $"SpiralDrop{difficulty}_{originalId}_{suffix}";
                    id = StringHash.Compute(name);
                }

                BindDocument variant = ReadBinary(archive, location.m_filename);
                variant.Value.Set<SpellTemplate>("m_name", variant.Value.Get<SpellTemplate>("m_name").WithString(name));
                double multiplier = SpellStatRandomizer.Multiplier(difficulty, Unit(seed, $"drop-stats:{originalId}"));
                SpellStatRandomizer.Apply(variant, spell, multiplier, true);
                string path = $"ObjectData/Randomizer/DropSpell{id}.xml";
                descriptions.Apply(variant);
                Write(zip, path, variant);
                rawLocations.Add(AliasLocation(locationRecords[originalId], id, path));
                variants[difficulty - 1] = id;
                WriteRewardCard(zip, id, name);
                multipliers[difficulty - 1] = multiplier;
            }

            result.Add(new(originalId, spell.m_name, spell.m_sMagicSchoolName, variants, multipliers));
        }

        return result;
    }

    private static bool IsServiceBehavior(string name) =>
        new[] { "Shop", "Trainer", "Vendor", "Quest", "Service", "Minigame", "Teleport", "Pet" }
            .Any(part => name.Contains(part, StringComparison.OrdinalIgnoreCase));

    private static bool IsNumberedPlayerSpell(SpellTemplate spell, string path) =>
        !spell.m_Treasure && !spell.m_PvP && !spell.m_battlegroundsOnly && !spell.m_alwaysFizzle &&
        !new[] { "MOB", "Boss", "Test", "CHEAT", "QA_", "BG_", "Debug", "100K" }
            .Any(token => spell.m_name.ToString().Contains(token, StringComparison.OrdinalIgnoreCase)) &&
        !new[] { "Battleground", "Fight Specific", "/Mobs/", "CombatTrigger", "Minion Specific" }
            .Any(token => path.Contains(token, StringComparison.OrdinalIgnoreCase));

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

    private static HashSet<uint> DropSpellCandidates(string root, HashSet<uint> playerSpells) {
        HashSet<uint> ids = [.. playerSpells];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "Imlight/spiraldb-cache/DropTables"), "*.json")) {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(file), JsonReadOptions);
            if (!document.RootElement.TryGetProperty("Items", out JsonElement items)) {
                continue;
            }

            foreach (JsonElement item in items.EnumerateArray()) {
                if (item.TryGetProperty("ItemId", out JsonElement value) && uint.TryParse(value.GetString(), out uint id)) {
                    ids.Add(id);
                }
            }
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

    private static void WriteRewardCard(ZipArchive zip, uint id, string name) {
        string gui = "<Objects><Class Name=\"class GraphicalSpellWindow\">" +
            "<m_sName>rewardCard</m_sName><m_Style>HAS_NO_BORDER|DO_NOT_CAPTURE_MOUSE</m_Style>" +
            "<m_Flags>VISIBLE</m_Flags><m_Window>0,0,194,272</m_Window>" +
            $"<m_spellName>{name}</m_spellName><m_grayscale>0</m_grayscale></Class></Objects>";
        using Stream stream = zip.CreateEntry($"GUI/Randomizer/SpellDrop{id}.gui", CompressionLevel.Fastest).Open();
        stream.Write(System.Text.Encoding.UTF8.GetBytes(gui));
    }

    private static double Unit(string seed, string key) {
        byte[] bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(seed + ":" + key));
        return BitConverter.ToUInt32(bytes, 0) / 4294967296.0;
    }
}

using Imcodec.ObjectProperty.TypeCache;

internal sealed record EquipmentCardAsset(uint Original, uint Donor, uint[] Items, uint[] Spells, double[] Multipliers);
internal sealed record EquipmentSpellSource(uint Id, string Path, SpellTemplate Template);
internal sealed record EquipmentCardFile(uint Id, uint Prototype, string Path, BindDocument Document);
internal sealed record EquipmentCardBuild(EquipmentCardAsset Card, IReadOnlyList<EquipmentCardFile> Files);

internal sealed class EquipmentCardBuilder(
    IReadOnlyList<GearAsset> gear,
    IReadOnlyDictionary<string, BindDocument> randomizedGear,
    IReadOnlyList<EquipmentSpellSource> spells,
    Func<string, BindDocument> read,
    Func<string, double> roll,
    Func<string, (uint Id, string Name)> allocate) {
    internal IEnumerable<EquipmentCardBuild> Build() {
        if (spells.Count == 0) {
            throw new InvalidDataException("No spells are available for equipment item cards.");
        }

        BindObject effectPrototype = FindProvideSpellEffect();
        foreach (GearAsset item in gear.OrderBy(item => item.Id)) {
            EquipmentSpellSource donor = spells[(int)(roll($"item-card:{item.Id}:spell") * spells.Count)];
            BindDocument sourceSpell = read(donor.Path);
            BindDocument sourceItem = randomizedGear[item.Path];
            BindProperty effectProperty = sourceItem.Value.Get<WizItemTemplate>("m_equipEffects");
            List<BindObject> effects = BindDocument.ReadVector(effectProperty)
                .Where(effect => effect.Hash != effectPrototype.Hash).ToList();
            uint[] items = new uint[3];
            uint[] cards = new uint[3];
            double[] multipliers = new double[3];
            List<EquipmentCardFile> files = [];
            for (int difficulty = 1; difficulty <= 3; difficulty++) {
                (uint spellId, string spellName) = allocate($"SpiralGearSpell{difficulty}_{item.Id}");
                BindDocument spell = SpellCardAssets.Independent(BindDocument.Read(sourceSpell.Encode()));
                spell.Value.Set<SpellTemplate>("m_name", spell.Value.Get<SpellTemplate>("m_name").WithString(spellName));
                double multiplier = SpellStatRandomizer.Multiplier(difficulty, roll($"item-card:{item.Id}:numbers"));
                SpellStatRandomizer.Apply(spell, donor.Template, multiplier, true);
                files.Add(new(spellId, donor.Id, $"ObjectData/Randomizer/GearSpell{spellId}.xml", spell));

                (uint itemId, _) = allocate($"SpiralGearItem{difficulty}_{item.Id}");
                BindDocument variant = BindDocument.Read(sourceItem.Encode());
                BindObject effect = BindObject.Read(effectPrototype.Encode());
                effect.Set<ProvideSpellEffectInfo>("m_spellName", effect.Get<ProvideSpellEffectInfo>("m_spellName").WithString(spellName));
                BindProperty count = effect.Get<ProvideSpellEffectInfo>("m_numSpells");
                effect.Set<ProvideSpellEffectInfo>("m_numSpells", count with { Data = BitConverter.GetBytes(1) });
                variant.Value.Set<WizItemTemplate>("m_equipEffects", BindDocument.WriteVector(effectProperty, [.. effects, effect]));
                variant.Value.Set<GameObjectTemplate>("m_templateID",
                    variant.Value.Get<GameObjectTemplate>("m_templateID").WithUnsigned(itemId));
                files.Add(new(itemId, item.Id, $"ObjectData/Randomizer/GearItem{itemId}.xml", variant));
                items[difficulty - 1] = itemId;
                cards[difficulty - 1] = spellId;
                multipliers[difficulty - 1] = multiplier;
            }

            yield return new(new(item.Id, donor.Id, items, cards, multipliers), files);
        }
    }

    private BindObject FindProvideSpellEffect() {
        foreach (GearAsset item in gear) {
            int index = item.Template.m_equipEffects.FindIndex(effect => effect is ProvideSpellEffectInfo);
            if (index >= 0) {
                return BindDocument.ReadVector(read(item.Path).Value.Get<WizItemTemplate>("m_equipEffects"))[index];
            }
        }

        throw new InvalidDataException("No native item-card effect was found.");
    }
}

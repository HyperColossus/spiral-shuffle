using Imcodec.ObjectProperty.TypeCache;

internal sealed record GearAsset(uint Id, string Path, WizItemTemplate Template, int Level);
internal sealed record StatRoll(string Name, int Level, double Value, BindObject Effect);


internal sealed class GearRandomizer(
    IReadOnlyList<GearAsset> gear,
    IReadOnlyDictionary<string, string> effectTables,
    IReadOnlyDictionary<string, WizardStatTable> statTables,
    Func<string, BindDocument> read,
    Func<string, double> roll) {
    internal IEnumerable<(string Path, BindDocument Document)> Build() {
        List<StatRoll> stats = [];
        foreach (GearAsset item in gear) {
            BindDocument document = read(item.Path);
            List<BindObject> effects = BindDocument.ReadVector(document.Value.Get<WizItemTemplate>("m_equipEffects"));
            for (int index = 0; index < item.Template.m_equipEffects.Count; index++) {
                if (item.Template.m_equipEffects[index] is StatisticEffectInfo effect && TryTable(effect, out WizardStatTable? table) &&
                    effect.m_lookupIndex >= 0 && effect.m_lookupIndex < table.m_statVector.Count && table.m_statVector[effect.m_lookupIndex] > 0) {
                    stats.Add(new(effect.m_effectName.ToString(), item.Level, table.m_statVector[effect.m_lookupIndex], effects[index]));
                }
            }
        }

        foreach (GearAsset item in gear) {
            BindDocument document = read(item.Path);
            BindProperty property = document.Value.Get<WizItemTemplate>("m_equipEffects");
            List<BindObject> effects = BindDocument.ReadVector(property);
            List<BindObject> result = effects.Where((effectObject, index) =>
                item.Template.m_equipEffects[index] is not StatisticEffectInfo effect || !TryTable(effect, out _)).ToList();
            HashSet<string> originalNames = item.Template.m_equipEffects.OfType<StatisticEffectInfo>()
                .Select(effect => effect.m_effectName.ToString()).ToHashSet();
            StatRoll[] pool = stats.Where(stat => stat.Level <= Math.Max(10, item.Level + 10) &&
                stat.Level >= Math.Max(0, item.Level - 20)).ToArray();
            if (pool.Length == 0) {
                pool = stats.Where(stat => stat.Level <= item.Level).ToArray();
            }

            HashSet<string> selected = [];
            int count = 1 + (int)(roll($"gear:{item.Id}:count") * 5);
            for (int index = 0; index < count; index++) {
                string[] names = pool.Select(stat => stat.Name).Distinct().Where(name => !selected.Contains(name))
                    .OrderBy(name => name, StringComparer.Ordinal).ToArray();
                if (index == 0 && names.Any(name => !originalNames.Contains(name))) {
                    names = names.Where(name => !originalNames.Contains(name)).ToArray();
                }
                if (names.Length == 0) {
                    break;
                }

                string name = names[(int)(roll($"gear:{item.Id}:{index}:type") * names.Length)];
                StatRoll[] choices = pool.Where(stat => stat.Name == name).ToArray();
                StatRoll donor = choices[(int)(roll($"gear:{item.Id}:{index}:donor") * choices.Length)];
                BindObject effect = BindObject.Read(donor.Effect.Encode());
                WizardStatTable table = statTables[effectTables[name]];
                double desired = donor.Value * (0.5 + roll($"gear:{item.Id}:{index}:value") * 1.25);
                int lookup = Enumerable.Range(0, table.m_statVector.Count)
                    .Where(candidate => table.m_statVector[candidate] > 0)
                    .MinBy(candidate => Math.Abs(table.m_statVector[candidate] - desired));
                BindProperty old = effect.Get<StatisticEffectInfo>("m_lookupIndex");
                effect.Set<StatisticEffectInfo>("m_lookupIndex", old with { Data = BitConverter.GetBytes(lookup) });
                result.Add(effect);
                selected.Add(name);
            }

            document.Value.Set<WizItemTemplate>("m_equipEffects", BindDocument.WriteVector(property, result));
            yield return (item.Path, document);
        }
    }

    private bool TryTable(StatisticEffectInfo effect, out WizardStatTable table) {
        table = null!;
        return effectTables.TryGetValue(effect.m_effectName.ToString(), out string? name) && statTables.TryGetValue(name, out table!);
    }
}

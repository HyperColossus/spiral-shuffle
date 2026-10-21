using Imcodec.ObjectProperty.TypeCache;

internal sealed record ShuffleSpell(uint Id, SpellTemplate Template);

internal static class SpellShufflePlan {
    internal static Dictionary<uint, uint> Build(IReadOnlyList<ShuffleSpell> spells, ISet<uint> trainerIds, Func<string, double> roll) {
        Dictionary<uint, ShuffleSpell[]> candidates = spells.ToDictionary(source => source.Id, source => spells
            .Where(donor => donor.Id != source.Id && SpellRules.Compatible(source.Template, donor.Template))
            .OrderBy(donor => donor.Template.m_sMagicSchoolName == source.Template.m_sMagicSchoolName ? 1 : 0)
            .ThenBy(donor => roll($"spell:{source.Id}:{donor.Id}"))
            .ThenBy(donor => donor.Id).DistinctBy(Identity).ToArray());
        Dictionary<string, uint> owners = [];
        Dictionary<uint, uint> result = [];
        foreach (ShuffleSpell source in spells.OrderBy(spell => trainerIds.Contains(spell.Id) ? 0 : 1)
                     .ThenBy(spell => spell.Template.m_spellRank.m_spellRank).ThenBy(spell => spell.Id)) {
            if (!Assign(source.Id, candidates, owners, result, new HashSet<string>()) && candidates[source.Id].Length > 0) {
                result[source.Id] = candidates[source.Id][0].Id;
            }
        }
        return result;
    }

    private static bool Assign(uint source, IReadOnlyDictionary<uint, ShuffleSpell[]> candidates,
        IDictionary<string, uint> owners, IDictionary<uint, uint> result, ISet<string> visited) {
        foreach (ShuffleSpell donor in candidates[source]) {
            string identity = Identity(donor);
            if (!visited.Add(identity)) {
                continue;
            }
            if (!owners.TryGetValue(identity, out uint prior) || Assign(prior, candidates, owners, result, visited)) {
                owners[identity] = source;
                result[source] = donor.Id;
                return true;
            }
        }
        return false;
    }

    private static string Identity(ShuffleSpell spell) => $"{spell.Template.m_sMagicSchoolName}:{spell.Template.m_displayName}";
}

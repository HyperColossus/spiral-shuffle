using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Wad;

internal sealed class SpellDescriptions(Archive archive) {
    private const string Table = "Spell";
    private static readonly Regex Amount = new(@"(?:#-?\d+:)?\$eA(\d*)\$", RegexOptions.CultureInvariant);
    private static readonly string[] Fields = ["m_description", "m_advancedDescription", "m_descriptionTrainer", "m_descriptionCombatHUD"];
    private int _nextKey;
    private readonly BindSerializer _codec = new();
    private readonly string[] _languages = archive.Files.Keys.Where(path => path.StartsWith("Locale/", StringComparison.Ordinal))
        .Select(path => path.Split('/')[1]).Distinct().Order().ToArray();
    private readonly Dictionary<string, Dictionary<string, string>> _sources = [];
    private readonly Dictionary<string, Dictionary<string, string>> _entries = [];

    internal void Apply(BindDocument document) {
        if (!_codec.Deserialize(document.Encode(), PropertyFlags.Prop_Save, out SpellTemplate spell)) {
            throw new InvalidDataException("Cannot read a randomized spell description.");
        }

        for (int index = 0; index < Fields.Length; index++) {
            string field = Fields[index];
            string reference = typeof(SpellTemplate).GetProperty(field)!.GetValue(spell)!.ToString()!;
            if (reference.Length == 0) {
                continue;
            }

            // Match the eight-digit IDs used by the native locale tables.
            if (_nextKey == 0) {
                foreach (string language in _languages) {
                    Resolve(language, "Spell_00000000");
                }
                _nextKey = _sources.Where(table => table.Key.EndsWith("/Spell.lang", StringComparison.Ordinal))
                    .SelectMany(table => table.Value.Keys).Select(key => int.TryParse(key, out int number) ? number : 0).DefaultIfEmpty().Max() + 1;
            }
            string key = (_nextKey++).ToString("D8", CultureInfo.InvariantCulture);
            bool written = false;
            foreach (string language in _languages) {
                string? source = Resolve(language, reference) ?? Resolve("en-US", reference);
                if (source is null) {
                    continue;
                }

                if (!_entries.TryGetValue(language, out Dictionary<string, string>? entries)) {
                    entries = _entries[language] = [];
                }
                entries.Add(key, Render(source, spell.m_effects));
                written = true;
            }

            if (written) {
                BindProperty property = document.Value.Get<SpellTemplate>(field);
                document.Value.Set<SpellTemplate>(field, property.WithString($"{Table}_{key}"));
            }
        }
    }

    internal void Write(ZipArchive output) {
        foreach ((string language, Dictionary<string, string> entries) in _entries) {
            using Stream stream = output.CreateEntry($"Locale/{language}/{Table}.lang", CompressionLevel.Optimal).Open();
            using StreamWriter writer = new(stream, Encoding.Unicode) { NewLine = "\r\n" };
            writer.WriteLine($"1:{Table}");
            string path = $"Locale/{language}/{Table}.lang";
            foreach ((string key, string value) in _sources.GetValueOrDefault(path, []).Concat(entries)) {
                writer.WriteLine(key);
                writer.WriteLine();
                writer.WriteLine(value);
            }
        }
    }

    internal static string Render(string description, IReadOnlyList<SpellEffect> effects) {
        SpellEffect[] values = NumericEffects(effects).ToArray();
        return Amount.Replace(description, match => {
            int index = match.Groups[1].Length == 0 ? 0 : int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) - 1;
            return index >= 0 && index < values.Length && values[index].m_effectType != kSpellEffects.kInvalidSpellEffect
                ? Math.Abs((long)values[index].m_effectParam).ToString(CultureInfo.InvariantCulture) : match.Value;
        });
    }

    private static IEnumerable<SpellEffect> NumericEffects(IEnumerable<SpellEffect> effects) {
        foreach (SpellEffect effect in effects) {
            IEnumerable<SpellEffect>? children = effect switch {
                RandomSpellEffect random => random.m_effectList,
                VariableSpellEffect variable => variable.m_effectList,
                EffectListSpellEffect list => list.m_effectList,
                CountBasedSpellEffect count => count.m_effectList,
                ConditionalSpellEffect conditional => conditional.m_elements.Select(element => element.m_pEffect),
                TargetCountSpellEffect targetCount => targetCount.m_effectLists,
                HangingConversionSpellEffect conversion => conversion.m_outputEffect,
                _ => null
            };
            if (children is null) {
                yield return effect;
            }
            else {
                foreach (SpellEffect child in NumericEffects(children)) {
                    yield return child;
                }
            }
        }
    }

    private string? Resolve(string language, string reference) {
        int separator = reference.IndexOf('_');
        if (separator < 0) {
            return null;
        }

        string path = $"Locale/{language}/{reference[..separator]}.lang";
        if (!_sources.TryGetValue(path, out Dictionary<string, string>? entries)) {
            entries = _sources[path] = [];
            if (archive.OpenFile(path) is { } bytes) {
                using StringReader reader = new(Encoding.Unicode.GetString(bytes.Span).TrimStart('\uFEFF'));
                reader.ReadLine();
                while (reader.ReadLine() is { } key && reader.ReadLine() is not null && reader.ReadLine() is { } value) {
                    entries[key] = value;
                }
            }
        }

        return entries.GetValueOrDefault(reference[(separator + 1)..]);
    }
}

using System.IO.Compression;
using System.Text.Json;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imcodec.Wad;

if (args.Length == 1 && args[0] == "test") {
    uint priceHash = BindMetadata.Hash(typeof(WizItemTemplate), "m_creditsCost");
    uint goldHash = BindMetadata.Hash(typeof(WizItemTemplate), "m_baseCost");
    byte[] header = [.. "BINd"u8, 7, 0, 0, 0];
    var gold = new BindProperty(goldHash, BitConverter.GetBytes(900f), 32);
    var unknown = new BindProperty(123456789, [1, 2, 3, 4], 32);
    var doc = new BindDocument(header, new(123, [new(priceHash, BitConverter.GetBytes(4500f), 32), gold, unknown]));
    if (!CrownPricing.Apply(doc)) throw new Exception("Paid Crown item unchanged");
    if (CrownPricing.Apply(doc)) throw new Exception("Zero price was changed twice");
    if (!doc.Value.Properties.Single(p => p.Hash == goldHash).Data.AsSpan().SequenceEqual(gold.Data) ||
        !doc.Value.Properties.Single(p => p.Hash == unknown.Hash).Data.AsSpan().SequenceEqual(unknown.Data))
        throw new Exception("Unrelated price or unknown field changed");
    var normal = new BindDocument(header, new(123, [gold, unknown]));
    byte[] before = normal.Encode();
    if (CrownPricing.Apply(normal) || !normal.Encode().AsSpan().SequenceEqual(before)) throw new Exception("Gold-only item changed");
    if (CrownPricing.Apply(BindDocument.Read(doc.Encode()))) throw new Exception("Zero price did not survive encoding");
    Console.WriteLine("PASS: Crown price zero, gold and unknown fields preserved, missing prices untouched, idempotent BIND round trip.");
    return;
}
if (args.Length != 3 || args[0] is not ("build" or "verify"))
    throw new ArgumentException("Usage: CrownTool build|verify INPUT_ROOT_WAD OUTPUT_ZIP_OR_BASELINE_WAD");
using var stream = File.OpenRead(args[1]);
Archive archive = ArchiveParser.Parse(stream) ?? throw new InvalidDataException("Invalid Root.wad");
if (args[0] == "build") {
    if (File.Exists(args[2])) throw new IOException("Output already exists.");
    using var zip = ZipFile.Open(args[2], ZipArchiveMode.Create);
    int changed = 0, scanned = 0;
    foreach (string path in archive.Files.Keys.Where(p => p.StartsWith("ObjectData/", StringComparison.Ordinal) && p.EndsWith(".xml", StringComparison.Ordinal))) {
        byte[] bytes = archive.OpenFile(path)!.Value.ToArray();
        BindDocument document = BindDocument.Read(bytes);
        scanned++;
        if (!CrownPricing.Apply(document)) continue;
        byte[] replacement = document.Encode();
        // Raw-field verification also supports item behaviors unknown to Imcodec.
        if (CrownPricing.Apply(BindDocument.Read(replacement)))
            throw new InvalidDataException($"Crown price verification failed: {path}");
        using var entry = zip.CreateEntry(path, CompressionLevel.Fastest).Open();
        entry.Write(replacement);
        changed++;
    }
    Console.WriteLine(JsonSerializer.Serialize(new { Scanned = scanned, CrownItemsChanged = changed }));
} else {
    using var baselineStream = File.OpenRead(args[2]);
    Archive baseline = ArchiveParser.Parse(baselineStream) ?? throw new InvalidDataException("Invalid baseline");
    if (!archive.Files.Keys.ToHashSet().SetEquals(baseline.Files.Keys)) throw new InvalidDataException("Archive entries changed.");
    int changed = 0, untouched = 0;
    foreach (string path in archive.Files.Keys) {
        byte[] before = baseline.OpenFile(path)!.Value.ToArray();
        byte[] after = archive.OpenFile(path)!.Value.ToArray();
        if (path.StartsWith("ObjectData/", StringComparison.Ordinal) && path.EndsWith(".xml", StringComparison.Ordinal) &&
            CrownPricing.Apply(BindDocument.Read(after))) throw new InvalidDataException($"Nonzero Crown price remains: {path}");
        if (before.AsSpan().SequenceEqual(after)) { untouched++; continue; }
        if (!path.StartsWith("ObjectData/", StringComparison.Ordinal)) throw new InvalidDataException($"Non-item changed: {path}");
        BindDocument expected = BindDocument.Read(before);
        if (!CrownPricing.Apply(expected) || !expected.Encode().AsSpan().SequenceEqual(after))
            throw new InvalidDataException($"Unexpected item changes: {path}");
        if (CrownPricing.Apply(BindDocument.Read(after)))
            throw new InvalidDataException($"Nonzero Crown price: {path}");
        changed++;
    }
    Console.WriteLine(JsonSerializer.Serialize(new { VerifiedCrownItems = changed, UnchangedAssets = untouched, OnlyCrownPriceFieldsChanged = true }));
}

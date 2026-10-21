using Imcodec.IO;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;

internal static class SpellCardAssets {
    private static readonly BindObject Prototype = CreatePrototype();
    private static readonly HashSet<uint> Fields = Prototype.Properties.Select(property => property.Hash).ToHashSet();

    internal static BindDocument Independent(BindDocument source) => new(
        [.. "BINd"u8, 7, 0, 0, 0],
        new BindObject(Prototype.Hash, source.Value.Properties.Where(property => Fields.Contains(property.Hash)).ToList()));

    private static BindObject CreatePrototype() {
        ObjectSerializer codec = new(true, (SerializerFlags)7);
        if (!codec.Serialize(new SpellTemplate(), PropertyFlags.Prop_Save, out ByteString bytes)) {
            throw new InvalidDataException("Cannot read the native spell fields.");
        }
        return BindObject.Read(((byte[])bytes)[4..]);
    }
}

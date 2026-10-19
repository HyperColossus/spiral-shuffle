using System.Buffers.Binary;
using System.Collections;
using System.IO.Compression;
using System.Reflection;
using Imcodec.IO;
using Imcodec.ObjectProperty;

internal sealed record BindProperty(uint Hash, byte[] Data, int DataBits) {
    internal BindProperty WithUnsigned(ulong value) {
        byte[] data = Data.Length switch {
            4 => BitConverter.GetBytes(checked((uint)value)),
            8 => BitConverter.GetBytes(value),
            _ => throw new InvalidDataException("Expected a 32-bit or 64-bit unsigned BIND field.")
        };
        return this with { Data = data, DataBits = data.Length * 8 };
    }

    internal BindProperty WithString(string value) {
        byte[] body = System.Text.Encoding.UTF8.GetBytes(value);
        if (body.Length >= 128) {
            throw new InvalidDataException("This compact BIND string must be shorter than 128 bytes.");
        }

        byte[] data = [(byte)(body.Length << 1), .. body];
        return this with { Data = data, DataBits = data.Length * 8 };
    }

    internal byte[] Encode(int padding) {
        byte[] output = new byte[8 + Data.Length];
        BinaryPrimitives.WriteUInt32LittleEndian(output, checked((uint)(padding + 64 + DataBits)));
        BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(4), Hash);
        Data.CopyTo(output, 8);
        return output;
    }
}


internal sealed class BindObject(uint hash, List<BindProperty> properties) {
    internal uint Hash { get; } = hash;
    internal List<BindProperty> Properties { get; } = properties;

    internal BindProperty Get<T>(string name) where T : PropertyClass =>
        Find<T>(name) ?? throw new InvalidDataException($"Missing BIND field: {typeof(T).Name}.{name}");

    internal BindProperty? Find<T>(string name) where T : PropertyClass =>
        Properties.SingleOrDefault(property => property.Hash == PropertyHash<T>(name));

    internal void Set<T>(string name, BindProperty? value) where T : PropertyClass {
        uint hash = PropertyHash<T>(name);
        int index = Properties.FindIndex(property => property.Hash == hash);
        if (value is null) {
            if (index >= 0) {
                Properties.RemoveAt(index);
            }
        }
        else if (index < 0) {
            Properties.Add(value with { Hash = hash });
        }
        else {
            Properties[index] = value with { Hash = hash };
        }
    }

    internal byte[] Encode() {
        if (Hash == 0) {
            return new byte[4];
        }

        List<byte[]> encoded = [];
        int position = 64;
        foreach (BindProperty property in Properties) {
            int padding = (8 - position % 8) % 8;
            encoded.Add(property.Encode(padding));
            position += padding + 64 + property.DataBits;
        }

        byte[] output = new byte[8 + encoded.Sum(property => property.Length)];
        BinaryPrimitives.WriteUInt32LittleEndian(output, Hash);
        BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(4), checked((uint)(position - 32)));
        int offset = 8;
        foreach (byte[] property in encoded) {
            property.CopyTo(output, offset);
            offset += property.Length;
        }

        return output;
    }

    internal static BindObject Read(byte[] input) {
        if (input.Length == 4 && BinaryPrimitives.ReadUInt32LittleEndian(input) == 0) {
            return new(0, []);
        }

        if (input.Length < 8) {
            throw new InvalidDataException("Truncated BIND object.");
        }

        uint hash = BinaryPrimitives.ReadUInt32LittleEndian(input);
        int end = checked(32 + (int)BinaryPrimitives.ReadUInt32LittleEndian(input.AsSpan(4)));
        int position = 64;
        List<BindProperty> properties = [];
        while (position < end) {
            int offset = (position + 7) / 8;
            int padding = offset * 8 - position;
            int size = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(input.AsSpan(offset)));
            uint propertyHash = BinaryPrimitives.ReadUInt32LittleEndian(input.AsSpan(offset + 4));
            if (size < padding + 64 || position + size > end) {
                throw new InvalidDataException($"Invalid BIND property size: {propertyHash}, offset {offset}, size {size}, end {end}");
            }

            properties.Add(new(propertyHash, input[(offset + 8)..((position + size + 7) / 8)], size - padding - 64));
            position += size;
        }

        BindObject result = new(hash, properties);
        if (!result.Encode().AsSpan().SequenceEqual(input)) {
            throw new InvalidDataException("BIND object did not preserve its original bytes.");
        }

        return result;
    }

    private static uint PropertyHash<T>(string name) where T : PropertyClass => BindMetadata.Hash(typeof(T), name);
}


internal static class BindMetadata {
    // This codec build omits field attributes, so inspect disposable objects for property hashes only.
    private static readonly Dictionary<(Type Type, string Name), uint> Hashes = [];
    private static readonly ObjectSerializer Encoder = new(true, (SerializerFlags)7);

    internal static uint Hash(Type type, string name) {
        if (Hashes.TryGetValue((type, name), out uint hash)) {
            return hash;
        }

        PropertyInfo property = type.GetProperty(name) ?? throw new InvalidDataException($"Missing property: {name}");
        PropertyClass value = (PropertyClass)Activator.CreateInstance(type)!;
        BindObject before = EncodeMetadata(value);
        object? current = property.GetValue(value);
        object? replacement;
        if (property.PropertyType == typeof(ByteString)) {
            replacement = new ByteString(new byte[] { 65 });
        }
        else if (typeof(IList).IsAssignableFrom(property.PropertyType)) {
            IList list = (IList)Activator.CreateInstance(property.PropertyType)!;
            list.Add(Activator.CreateInstance(property.PropertyType.GenericTypeArguments[0]));
            replacement = list;
        }
        else if (typeof(PropertyClass).IsAssignableFrom(property.PropertyType)) {
            replacement = current is null ? Activator.CreateInstance(property.PropertyType) : null;
        }
        else {
            replacement = Convert.ChangeType(1, property.PropertyType);
        }

        property.SetValue(value, replacement);
        BindObject after = EncodeMetadata(value);
        hash = after.Properties.Single(candidate => !candidate.Data.AsSpan().SequenceEqual(
            before.Properties.Single(original => original.Hash == candidate.Hash).Data)).Hash;
        Hashes[(type, name)] = hash;
        return hash;
    }

    private static BindObject EncodeMetadata(PropertyClass value) {
        if (!Encoder.Serialize(value, PropertyFlags.Prop_Save, out ByteString body)) {
            throw new InvalidDataException("Could not inspect BIND property metadata.");
        }

        return BindObject.Read(((byte[])body)[4..]);
    }
}


internal sealed class BindDocument(byte[] header, BindObject value) {
    internal BindObject Value { get; } = value;

    internal byte[] Encode() => [.. header, .. Value.Encode()];

    internal static BindDocument Read(byte[] input) {
        if (!input.AsSpan(0, 4).SequenceEqual("BINd"u8)) {
            throw new InvalidDataException("Expected the original string-enum BIND format.");
        }

        uint flags = BinaryPrimitives.ReadUInt32LittleEndian(input.AsSpan(4));
        if (flags == 15 && input[8] == 1) {
            using MemoryStream compressed = new(input[13..]);
            using ZLibStream inflater = new(compressed, CompressionMode.Decompress);
            using MemoryStream body = new();
            inflater.CopyTo(body);
            if (body.Length != BinaryPrimitives.ReadUInt32LittleEndian(input.AsSpan(9))) {
                throw new InvalidDataException("Invalid compressed BIND length.");
            }

            return new([.. "BINd"u8, 7, 0, 0, 0], BindObject.Read(body.ToArray()));
        }

        if (flags != 7) {
            throw new InvalidDataException($"Unsupported BIND flags: {flags}.");
        }

        return new(input[..8], BindObject.Read(input[8..]));
    }

    internal static List<BindObject> ReadVector(BindProperty property) {
        byte[] data = property.Data;
        int prefix = (data[0] & 1) == 0 ? 1 : 4;
        uint size = prefix == 1 ? (uint)(data[0] >> 1) : BinaryPrimitives.ReadUInt32LittleEndian(data) >> 1;
        int offset = prefix;
        List<BindObject> values = [];
        for (uint index = 0; index < size; index++) {
            uint hash = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset));
            int bits = hash == 0 ? 0 : checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset + 4)));
            int length = 4 + (bits + 7) / 8;
            values.Add(BindObject.Read(data[offset..(offset + length)]));
            offset += length;
        }

        if (offset != data.Length) {
            throw new InvalidDataException("Unexpected trailing BIND vector data.");
        }

        return values;
    }

    internal static BindProperty WriteVector(BindProperty original, List<BindObject> values) {
        byte[] prefix = values.Count < 128 ? [(byte)(values.Count << 1)] : BitConverter.GetBytes(((uint)values.Count << 1) | 1);
        byte[][] encoded = values.Select(value => value.Encode()).ToArray();
        byte[] data = [.. prefix, .. encoded.SelectMany(value => value)];
        int bits = data.Length * 8;
        if (values.Count > 0 && values[^1].Hash != 0) {
            byte[] last = encoded[^1];
            bits -= last.Length * 8 - 32 - checked((int)BinaryPrimitives.ReadUInt32LittleEndian(last.AsSpan(4)));
        }

        return original with { Data = data, DataBits = bits };
    }
}

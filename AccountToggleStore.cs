using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Imlight.CoreLib.Game;

internal sealed class AccountToggleStore(string path) {
    private readonly object _lock = new();
    private readonly Dictionary<ulong, bool> _accounts = File.Exists(path)
        ? JsonConvert.DeserializeObject<Dictionary<ulong, bool>>(File.ReadAllText(path))
            ?? throw new InvalidDataException("Invalid account toggle settings.")
        : [];

    internal bool Get(ulong accountId) {
        lock (_lock) {
            return _accounts.GetValueOrDefault(accountId);
        }
    }

    internal void Set(ulong accountId, bool enabled) {
        lock (_lock) {
            _accounts[accountId] = enabled;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.WriteAllText(path + ".new", JsonConvert.SerializeObject(_accounts));
            File.Move(path + ".new", path, true);
        }
    }
}

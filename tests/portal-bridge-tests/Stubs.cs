// Test-only RavenDB and account storage seams. Production bridge source is
// compiled unchanged; NativeDatabaseUtilities.cs is the pinned upstream helper.
using System.Linq;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Raven.Client.Documents {
    public sealed class QueryOptions {
        public void WaitForNonStaleResults(TimeSpan timeout) { }
    }
    public static class QueryExtensions {
        public static IQueryable<T> Customize<T>(this IQueryable<T> source, Action<QueryOptions> configure) {
            configure(new QueryOptions());
            return source;
        }
    }
}
namespace Raven.Client.Documents.Session { public interface IDocumentSession { } }
namespace Imlight.CoreLib.WizardData.Databases {
    public sealed class PlayerDatabase {
        public static PlayerDatabase Instance { get; } = new();
        public FakeStore Store { get; } = new();
    }
    public sealed class FakeStore { public FakeSession OpenSession() => new(); }
    public sealed class FakeSession : IDisposable {
        public IQueryable<T> Query<T>(string collectionName) =>
            Collections.AccountCollection.Accounts.Values.Cast<T>().AsQueryable();
        public void Dispose() { }
    }
}
namespace Imlight.CoreLib.WizardData.Models.Player {
    public enum AuthLevel { None, Administrator }
    public sealed class InfractionHistory { public bool IsCurrentlyBanned { get; set; } }
    public sealed class Account(string username, string email, string passwordHash) {
        public string Username { get; } = username;
        public string PasswordHash { get; set; } = passwordHash;
        public AuthLevel AuthLevel { get; set; }
        public bool IsLocked { get; set; }
        public InfractionHistory? InfractionHistory { get; set; } = new();
    }
}
namespace Imlight.CoreLib.WizardData.Collections {
    public static class AccountCollection {
        public const string CollectionName = "Accounts";
        public static readonly Dictionary<string, Account> Accounts = new();
        public static bool CreateAccount(Account account) => Accounts.TryAdd(account.Username, account);
        public static Account? GetAccount(string username) => Accounts.GetValueOrDefault(username);
    }
    public static class InfractionCollection {
        public static readonly HashSet<string> BannedIps = new();
        public static bool IsIpBanned(string ip) => BannedIps.Contains(ip);
    }
}

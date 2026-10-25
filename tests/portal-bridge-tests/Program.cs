// Real loopback HTTP tests against the unchanged PortalRegistration.cs.
// Only RavenDB/account persistence is replaced with an in-memory seam.
using System.Net;
using System.Net.Http.Json;
using Imlight.Director;
using Imlight.CoreLib.WizardData;
using Imlight.CoreLib.WizardData.Collections;
using Imlight.CoreLib.WizardData.Models.Player;

internal static class Program {
    private static readonly HttpClient Client = new() { BaseAddress = new Uri("http://127.0.0.1:12772"), Timeout = TimeSpan.FromSeconds(10) };
    private const string Token = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private static int passed;
    private static string? LastCode;

    private static void Check(bool okay, string name) {
        if (!okay) throw new Exception("FAIL: " + name);
        Console.WriteLine("PASS: " + name);
        passed++;
    }
    private static async Task<int> Enter(string username, string password, string ip = "8.8.8.8", string token = Token, string path = "/enter") {
        using var request = new HttpRequestMessage(HttpMethod.Post, path) {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new { Username = username, Password = password, ClientIp = ip }), System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Spiral-Token", token);
        using var response = await Client.SendAsync(request);
        var payload = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        LastCode = payload.GetProperty("code").GetString();
        return (int)response.StatusCode;
    }
    private static async Task Main() {
        var directory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "portal"));
        Directory.CreateDirectory(directory);
        var tokenFile = Path.Combine(directory, "bridge-token");
        await File.WriteAllTextAsync(tokenFile, Token);
        try {
            PortalRegistration.Start();
            using var health = await Client.GetAsync("/health");
            Check(health.StatusCode == HttpStatusCode.OK, "bridge starts on loopback");
            Check(await Enter("new_player", "ExamplePassword123!") == 201, "new credentials create account");
            var account = AccountCollection.GetAccount("new_player")!;
            var originalHash = account.PasswordHash;
            Check(account.AuthLevel == AuthLevel.None && originalHash == DatabaseUtilities.CreateHashedPassword("ExamplePassword123!"), "native password hash and normal privileges");
            Check(await Enter("new_player", "ExamplePassword123!") == 200, "same credentials verify existing account");
            Check(await Enter("new_player", "WrongPassword123!") == 403, "wrong password is rejected");
            Check(LastCode == "wrong_password", "incorrect password receives actionable reason");
            Check(account.PasswordHash == originalHash && AccountCollection.Accounts.Count == 1, "login never overwrites account or password");
            account.IsLocked = true;
            Check(await Enter("new_player", "WrongPassword123!") == 403 && LastCode == "wrong_password", "lock status is not revealed to a wrong password");
            Check(await Enter("new_player", "ExamplePassword123!") == 403, "locked account rejected");
            Check(LastCode == "account_locked", "correct password receives locked-account reason");
            account.IsLocked = false;
            account.InfractionHistory!.IsCurrentlyBanned = true;
            Check(await Enter("new_player", "ExamplePassword123!") == 403, "banned account rejected");
            Check(LastCode == "account_banned", "correct password receives banned-account reason");
            account.InfractionHistory.IsCurrentlyBanned = false;
            InfractionCollection.BannedIps.Add("8.8.8.8");
            Check(await Enter("new_player", "ExamplePassword123!") == 403, "banned IP rejected for existing account");
            Check(LastCode == "network_blocked", "blocked network receives a connection reason");
            Check(await Enter("other_player", "ExamplePassword123!") == 403 && AccountCollection.Accounts.Count == 1, "banned IP cannot auto-create account");
            InfractionCollection.BannedIps.Clear();
            Check(await Enter("new_player", "ExamplePassword123!", token: "bad") == 403, "private bridge token enforced");
            Check(await Enter("new_player", "ExamplePassword123!", ip: "not-an-address") == 400, "malformed connection address rejected");
            Check(await Enter("short_player", "short") == 400 && AccountCollection.GetAccount("short_player") == null, "short new passwords cannot create account");
            Check(LastCode == "new_password_requirements", "short new password receives actual creation requirements");
            Check(await Enter("newline_player", "ExamplePassword123!\n") == 400, "trailing whitespace is rejected");
            DatabaseUtilities.CreateEmbeddedDatabaseAccount("old_player", "", "legacy");
            Check(await Enter("old_player", "legacy") == 200, "existing legacy password can still authenticate");
            Check(await Enter("admin", "ExamplePassword123!") == 400, "reserved usernames cannot auto-create");
            Check(LastCode == "username_reserved", "reserved username receives actionable reason");
            Check(await Enter("new_player", "ExamplePassword123!", path: "/register") == 409, "old registration route stays create-only");
            Console.WriteLine($"{passed} bridge checks passed.");
        } finally {
            File.Delete(tokenFile);
            Client.Dispose();
        }
    }
}

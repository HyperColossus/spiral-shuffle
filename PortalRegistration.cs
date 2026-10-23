// Optional loopback-only game account registration bridge for Spiral Shuffle.
// AGPL-3.0-or-later. No public socket and no database endpoint exposed.
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Imlight.CoreLib.WizardData;
using Imlight.CoreLib.WizardData.Collections;
using Imlight.CoreLib.WizardData.Databases;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.Director;

internal static class PortalRegistration {
    private const string LocalPrefix = "http://127.0.0.1:12772/";
    private static readonly object CreationLock = new();
    private static readonly Regex UsernameRule = new("^[a-z][a-z0-9_]{2,19}$", RegexOptions.CultureInvariant);
    private static readonly Regex PasswordRule = new("^[!-~]{12,32}$", RegexOptions.CultureInvariant);

    private sealed class RegistrationRequest {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    internal static void Start() {
        // Creating the private bridge-token file is an explicit operator opt-in.
        var tokenFile = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "portal", "bridge-token"));
        if (!File.Exists(tokenFile)) return;
        var token = File.ReadAllText(tokenFile).Trim();
        if (token.Length != 64 || !token.All(Uri.IsHexDigit)) {
            Console.Error.WriteLine("Portal bridge disabled: invalid private bridge token.");
            return;
        }
        try {
            var listener = new HttpListener();
            listener.Prefixes.Add(LocalPrefix);
            listener.Start();
            _ = Task.Run(() => AcceptLoop(listener, token));
            Console.WriteLine("Portal registration bridge listening on loopback 127.0.0.1:12772.");
        } catch (Exception ex) {
            Console.Error.WriteLine($"Portal bridge disabled: {ex.GetType().Name}");
        }
    }

    private static async Task AcceptLoop(HttpListener listener, string token) {
        while (listener.IsListening) {
            HttpListenerContext context;
            try { context = await listener.GetContextAsync(); }
            catch (HttpListenerException) { break; }
            catch (ObjectDisposedException) { break; }
            try { await Respond(context, token); }
            catch (Exception exception) {
                // Never log raw body, password, token, or account name.
                Console.Error.WriteLine($"Portal bridge request error: {exception.GetType().Name}");
                try { await Reply(context, 503, "Registration temporarily unavailable."); }
                catch { try { context.Response.Close(); } catch { } }
            }
        }
    }

    private static bool SecretMatches(string? supplied, string expected) {
        if (supplied == null) return false;
        var a = Encoding.ASCII.GetBytes(supplied);
        var b = Encoding.ASCII.GetBytes(expected);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }

    private static async Task Respond(HttpListenerContext context, string token) {
        var request = context.Request;
        if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/health") {
            await Reply(context, 200, "Registration bridge ready.");
            return;
        }
        if (request.HttpMethod != "POST" || request.Url?.AbsolutePath != "/register") {
            await Reply(context, 404, "Not found.");
            return;
        }
        if (!SecretMatches(request.Headers["X-Spiral-Token"], token)) {
            await Reply(context, 403, "Forbidden.");
            return;
        }
        if (request.ContentLength64 < 2 || request.ContentLength64 > 1024 ||
            request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) != true) {
            await Reply(context, 400, "Invalid registration data.");
            return;
        }
        string body;
        using (var reader = new StreamReader(request.InputStream, Encoding.UTF8)) {
            body = await reader.ReadToEndAsync();
        }
        RegistrationRequest? form;
        try { form = JsonSerializer.Deserialize<RegistrationRequest>(body); }
        catch (JsonException) { await Reply(context, 400, "Invalid registration data."); return; }
        var username = form?.Username?.Trim().ToLowerInvariant() ?? "";
        var password = form?.Password ?? "";
        if (!UsernameRule.IsMatch(username) || !PasswordRule.IsMatch(password) ||
            username is "admin" or "administrator" or "moderator" or "support" or "system" or "staff" or "gamemaster") {
            await Reply(context, 400, "Invalid username or password.");
            return;
        }

        bool created;
        lock (CreationLock) {
            // Wait for the username lookup's RavenDB index rather than using a stale result.
            using var session = PlayerDatabase.Instance.Store.OpenSession();
            bool exists = session.Query<Account>(collectionName: AccountCollection.CollectionName)
                .Customize(query => query.WaitForNonStaleResults(TimeSpan.FromSeconds(10)))
                .Any(a => a.Username == username);
            created = !exists && DatabaseUtilities.CreateEmbeddedDatabaseAccount(
                username, "", password, AuthLevel.None) is not null;
        }
        await Reply(context, created ? 201 : 409,
            created ? "Account created. Sign in to Spiral Shuffle." : "Username unavailable.");
    }

    private static async Task Reply(HttpListenerContext context, int status, string message) {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new { message });
        var response = context.Response;
        response.StatusCode = status;
        response.ContentType = "application/json; charset=utf-8";
        response.Headers["Cache-Control"] = "no-store";
        response.ContentLength64 = json.Length;
        await response.OutputStream.WriteAsync(json);
        response.Close();
    }
}

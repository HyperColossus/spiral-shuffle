# Spiral Shuffle — GitHub Pages registration backend

**Status:** Source code committed; requires installation and real-account testing on the owner's Ubuntu server. Public signups are disabled until configured.

The **website stays on GitHub Pages** at https://spiral.hypercoloss.us/ . This add-on supplies only the server-side API that GitHub Pages cannot run.

Optional signup-based firewall access is documented in [README-IP-ACCESS.md](README-IP-ACCESS.md).
It is disabled by default. Enable it only after the HTTPS API and separate firewall worker are tested.
Players use one game username/password form: new usernames create normal accounts;
existing usernames must verify the existing password before updating IP access.
No additional service account or player access key is required.

**Docker RavenDB database:** To start with new empty accounts (the selected option), use the **Fresh installation** instructions in [docker/README.md](docker/README.md). To preserve existing characters, use the separate import procedure instead. Complete the appropriate database switch **before installing the registration bridge**. The website and Imlight must use the same `Playerdata` database. Do not point the public website at RavenDB.

## Architecture

GitHub Pages (HTTPS static form) -> your registration HTTPS hostname, such as `https://register.example.com/register` -> **Caddy** -> `127.0.0.1:12771` (`portal_api.py`) -> authenticated `127.0.0.1:12772` (`PortalRegistration.cs` inside Imlight) -> **existing Imlight PlayerDatabase/RavenDB**.

Game clients still connect separately to **YOUR GAME SERVER public IP**, port `12010` and realms `12343–12348`. The players' IPs are not their login identities. No admin passwords, bridge tokens, CAPTCHA secrets or database access are ever added to GitHub Pages.

The bridge uses Imlight's own password hashing and `DatabaseUtilities.CreateEmbeddedDatabaseAccount(..., AuthLevel.None)`. It writes to the **database currently configured in Imlight.ini**, so finish selecting the permanent Docker database before inviting players.

The API binds **127.0.0.1:12771**, the bridge binds **127.0.0.1:12772**, and Docker RavenDB stays at **127.0.0.1:18080** (the old embedded instance used port `8090`). Do not forward any of these ports on the router or add UFW public allow rules for them.

## A. Install locally on Ubuntu (no asset rebuild)

Do this from SSH, while the server is available. It changes only the compiled, staged `Imlight.Director.dll` and the pinned Director source; it does **not** rerun randomized asset generation or touch saves, Docker, Discord, SSH, Tailscale, or any unrelated processes.

```bash
cd ~/spiral-shuffle
git pull --ff-only
python3 -m unittest discover -s tests -p 'test_portal_api.py' -v
python3 portal_bridge.py init
./Stop-Linux.sh
python3 portal_bridge.py install
./Play-Linux.sh
sleep 20
python3 linux_ctl.py status
curl -fsS http://127.0.0.1:12772/health
```

The last command should return `{"message":"Registration bridge ready."}`. The .NET bridge has **not yet been compiled on your Ubuntu installation**; if compilation fails, the installer restores the startup source and leaves the existing live Director DLL untouched. Run `./Play-Linux.sh` to restart the old game server if needed; share the compiler error without passwords.

If the newly compiled Director cannot start, stop just Spiral Shuffle and restore from `.runtime/server/Imlight.Director.before-portal.dll` (saved on first successful installation). Never delete `.runtime/saves`.

If you used the earlier `admin / 123` temporary test database, **do not open public signups yet**. Choose your permanent database and change the administrator password. The backend deliberately refuses public mode when its Imlight config contains a short admin password. Changing `Imlight.ini` alone does not necessarily update already-persisted credentials; update the game account and its config consistently.

## B. Start the registration API in local test mode

First confirm the bridge health check above succeeds. On Ubuntu:

```bash
cd ~/spiral-shuffle
set -a
source .runtime/portal/portal.env
set +a
python3 portal_api.py
```

The default private `portal.env` has `SPIRAL_PORTAL_PUBLIC=0`. Keep this process running for the local test. From a **second Ubuntu terminal**:

```bash
curl -fsS http://127.0.0.1:12771/health
curl -i -X OPTIONS http://127.0.0.1:12771/register \
  -H 'Origin: https://spiral.hypercoloss.us' \
  -H 'Access-Control-Request-Method: POST' \
  -H 'Access-Control-Request-Headers: content-type'
```

Expected results: JSON health response and **HTTP 204** for the trusted GitHub Pages origin. Wrong origins return 403 without a CORS allow header.

To create one real test account **without writing the password to your shell history**:

```bash
python3 - <<'PY'
import getpass, json, urllib.request
username = input("Test username (3-20 lowercase letters/digits/_): ").strip()
password = getpass.getpass("New password (12-32 ASCII, no spaces): ")
req = urllib.request.Request(
    "http://127.0.0.1:12771/register",
    data=json.dumps({"username": username, "password": password}).encode(),
    headers={"Origin": "https://spiral.hypercoloss.us",
             "Content-Type": "application/json"},
    method="POST")
with urllib.request.urlopen(req, timeout=30) as response:
    print(response.status, response.read().decode())
PY
```

A response of **201** means Imlight reports account creation; **200** means an existing account's credentials were verified. The private bridge uses `/enter` for this combined flow. Wrong existing passwords return 403 and never change an IP grant or persisted password. Log into the game with those same credentials to verify end-to-end behavior. Only share response codes and errors; do not paste credentials into chat.

## C. Set up public HTTPS (after the local test works)

Because you already have another website, choose a **separate DNS subdomain**, e.g. `register.yourdomain.com`. Do not overwrite any existing site's files or web-server configuration.

1. Configure DNS so the new subdomain points to the public web/reverse-proxy endpoint. If using direct Caddy, verify the existing router/web-server situation first: another service may already own TCP 80/443. **Do not install a second conflicting web server.** Add `portal/Caddyfile.example` as a separate site block in the *existing* Caddy configuration, adjusting its hostname; adapt equivalently if using Nginx, Apache or a managed HTTPS tunnel.
2. Create a Cloudflare **Turnstile** widget, allowing the hostname **`spiral.hypercoloss.us`**, and with action **`register`**. Turnstile's **site key** is public (for the GitHub Pages page); its **secret key** must remain in a server-only environment file.
3. Edit `.runtime/portal/portal.env` (mode 0600) on Ubuntu:

```ini
SPIRAL_PORTAL_PUBLIC=1
SPIRAL_ALLOWED_ORIGIN=https://spiral.hypercoloss.us
SPIRAL_TURNSTILE_SECRET=YOUR_PRIVATE_TURNSTILE_SECRET
SPIRAL_GAME_HOST=YOUR_PUBLIC_GAME_HOST
```

`SPIRAL_GAME_HOST` documents your host; the active backend does not depend on it. Keep the secret **off GitHub**, and do not paste it in chat. Public mode checks the admin credential length in Imlight config, requires HTTPS origin, and validates Turnstile tokens server-side with hostname/action checks. Signup rate limits use hashed IPs. The optional IP-access feature additionally stores private raw IPv4 grants; it never stores passwords.

4. Restart the API after updating its environment. When your HTTPS endpoint is actually reachable, edit `docs/index.html` in your GitHub fork, near `window.SPIRAL_SITE_CONFIG`:

```js
window.SPIRAL_SITE_CONFIG = {
  gameHost: "YOUR_PUBLIC_GAME_IP_OR_HOST",
  gamePort: 12010,
  registrationApi: "https://register.yourdomain.com/register",
  turnstileSiteKey: "YOUR_PUBLIC_TURNSTILE_SITE_KEY"
};
```

The GitHub Pages account form stays **disabled** if either `registrationApi` or `turnstileSiteKey` is blank. Use only the PUBLIC Turnstile site key in GitHub. Never put the secret, admin password, bridge token or internal IPs in `docs/index.html`.

5. Test registration through the actual website from a different internet connection and sign in with a new account before announcing public availability.

**Caddy requirement:** `portal/Caddyfile.example` deliberately **overwrites** `X-Real-IP` with `{remote_host}`; this prevents a client from supplying a fake rate-limit identity. If using a CDN in front, configure trusted proxy addresses carefully or many visitors will share one rate limit.

## D. Run as a persistent user service

After you finish local tests and configure the portal environment:

```bash
mkdir -p ~/.config/systemd/user
cp ~/spiral-shuffle/portal/spiral-portal.service ~/.config/systemd/user/
systemctl --user daemon-reload
systemctl --user enable --now spiral-portal.service
systemctl --user status spiral-portal.service
```

This user service starts **only the registration API**; it does not manage the game server or other containers. If you want it to survive logging out and aren't already using user service lingering, see `loginctl` documentation before changing host settings. Keep `.runtime/portal/portal.env` and token mode `0600`.

## Security and limitations

- **Experimental, not independently security audited.** A publicly reachable game emulator presents additional risks. Prefer an isolated VM/VPS or dedicated nonprivileged account to host public services. Back up `.runtime/saves`.
- Ordinary users get `AuthLevel.None`; no website command can grant admin rights. Account creation is disabled if no bridge token was installed.
- The API needs Cloudflare Turnstile for public use and validates it via Siteverify; browser-only CAPTCHA is insufficient.
- Each signup is limited to a lowercase `[a-z][a-z0-9_]{2,19}` username and a **12–32-character ASCII** password without whitespace. Accounts may require administrator help for password recovery; this project does not collect email addresses.
- For security, never ask players to use their official Wizard101 credentials.
- The registration website does **not** provide or redistribute copyrighted game client files. Players need their own compatible revision and private installation.
- Public website and public gameplay routing are separate: making the site accessible does not automatically expose game TCP ports.

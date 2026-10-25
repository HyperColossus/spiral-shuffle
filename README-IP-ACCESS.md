# Game username/password access

Players enter a game username and password on the website and press **Continue**.
A new username creates a normal game account automatically. Existing usernames
require the correct game password. Either success queues the current connection's
public IPv4 for game access for 30 days. Players use those same credentials in
the launcher. There are no extra service accounts, access keys or recovery codes.

Turnstile protects the request; its Managed widget usually completes without
interaction but can require a challenge. The public homepage stays disabled until
real HTTPS/Turnstile and external gameplay tests pass.

## How it works

HTTPS website -> loopback Python API -> authenticated Imlight `/enter` bridge ->
existing Playerdata database. The bridge creates missing accounts with
`AuthLevel.None`, or compares existing credentials using the pinned native
`DatabaseUtilities.CreateHashedPassword` function and a constant-time comparison.
Wrong passwords and locked/banned accounts are rejected. IP bans are also checked.
No password is changed and no game session is created by website verification.

Only after the bridge accepts the credentials does Python write the account's
IP grant to `.runtime/portal/access.sqlite3`. This separate private database
contains no passwords. Its legacy column names remain for compatibility with
installed workers; old access keys are no longer accepted by any endpoint.
A verified return visit consolidates that account's old grants and replaces its
address atomically. Other accounts sharing the old public IP retain their access.

The root-owned worker reads the database as the non-root game owner and manages
only the dedicated `SpiralShuffleRegistered` UFW profile. The API has no sudo
permission. This profile allows TCP 12010, 12343–12348, 12510 and 12379 only.
Existing SSH, Tailscale and other manual rules remain unchanged.

## Upgrade the previously installed bridge

On Ubuntu, while on the branch tracking `origin/codex/signup-ip-access`:

```bash
cd ~/spiral-shuffle
git status --short
git pull --ff-only
python3 -m unittest discover -s tests -p 'test_portal*.py' -v
```

If Git reports conflicting changes, stop; do not discard them. Stop the old
foreground API with Ctrl+C in its own terminal, or stop its dedicated user
service if that is how it runs. Keep the private environment and secret intact.

The bridge needs a brief game restart:

```bash
python3 linux_ctl.py stop
python3 portal_bridge.py install
```

If installation fails, run `python3 linux_ctl.py start` to restart the old server
and inspect the compiler error. The installer restores the startup source on
build failure. On success:

```bash
python3 linux_ctl.py start
curl -fsS http://127.0.0.1:12772/health
```

The installer stages only the Director DLL. It does not replace the live CoreLib,
regenerate assets, change databases or delete saves, so the game-entry fix stays
in place. Start the updated API using its existing private environment, including
`SPIRAL_PORTAL_PUBLIC=1` and `SPIRAL_IP_ACCESS=1`. The public proxy needs only
`/register`; the old `/access/refresh` route now returns 404.

If a credential request returns 503 after account creation, retry the same
credentials: it will verify the existing account and retry saving the grant.
There is no need to delete an account or save directory.

## First-time firewall worker installation

Review the files and current `sudo ufw status verbose` / `sudo ufw show added`.
Require active UFW with default-deny incoming. Existing broad game-port allow
rules bypass this allowlist; inspect those without resetting the firewall.
For owner `hypercolossus` and checkout `/home/hypercolossus/spiral-shuffle`:

```bash
sudo install -d -o root -g root -m 0755 /usr/local/lib/spiral-shuffle
sudo install -o root -g root -m 0644 portal/access_firewall.py /usr/local/lib/spiral-shuffle/access_firewall.py
sudo install -o root -g root -m 0644 portal/spiral-shuffle-registered /etc/ufw/applications.d/spiral-shuffle-registered
sudo install -o root -g root -m 0644 portal/spiral-access.service /etc/systemd/system/spiral-access.service
sudo install -o root -g root -m 0644 portal/spiral-access.timer /etc/systemd/system/spiral-access.timer
sudo systemctl daemon-reload
```

Adjust owner/database arguments in the root-owned service if needed. Never run
root Python from the web-writable checkout. API startup creates the private
access database. Run one sync before enabling the timer:

```bash
sudo systemctl start spiral-access.service
sudo systemctl show spiral-access.service -p Result -p ExecMainStatus
sudo journalctl -u spiral-access.service -n 10 --no-pager
```

After an empty-grant check and a controlled grant add/remove test succeed:

```bash
sudo systemctl enable --now spiral-access.timer
```

If the worker was already installed and tested, it does not need reinstalling for
this credential-flow update. The timer runs 30 seconds after each completed sync.
API success means **queued**, not that a firewall rule has already been applied.

## Public deployment checks

Follow [README-PORTAL.md](README-PORTAL.md) for HTTPS and the secret environment.
The direct Caddy example overwrites `X-Real-IP` with its connection peer. A CDN
or tunnel needs separately validated trusted-proxy handling. Never accept a
browser-supplied IP or arbitrary forwarded header. The registration hostname
must be reachable by IPv4 (A record, no AAAA for this version).

1. Confirm missing and invalid Turnstile proofs fail from an outside network.
2. With the updated backend installed, configure the isolated test page using
   the public site key and HTTPS API URL; leave homepage registration disabled.
3. Submit unused credentials. Expect HTTP 201, a normal game account and an
   active grant for the actual browser IP. No password/key appears in responses.
4. Run the worker manually and confirm exactly that IP gains the dedicated rule.
5. Submit the same username with a wrong password from another network. Expect
   HTTP 403 and no grant change. Repeat with the correct password: expect HTTP
   200, no duplicate account and replacement of that account's old IP.
6. Verify normal game login and world entry from that network, plus refusal of
   an unregistered outside connection. Public game routing/advertised addresses
   must already be set correctly; website registration alone does not do that.

## Limits and rollback

IP grants last 30 days. Re-entering valid game credentials renews access. New
passwords require 12–32 printable ASCII characters without spaces; existing
1–32 character printable passwords can still be verified. IP and account attempt
budgets are 5/hour and 15/day; account attempts count after valid bot verification.
Game username/password checks still decide who may actually play. People behind
the same public NAT share network reachability. IP access is not DDoS protection.

A worker failure preserves existing rules, including expired ones until a
successful reconciliation. Existing TCP sessions may survive rule removal.
Check `journalctl -u spiral-access.service` for worker failures. Incremental UFW
updates recover on the next successful run. Do not delete the access database
or profile before withdrawing its rules.

To revoke an account's network grants, set `active = 0` for its username in the
private access database as the game owner and run a sync. If the account must
remain barred from reauthorizing, lock/ban it in the game database as well.
To withdraw all grants, deactivate all rows, run a successful sync, then stop
the timer and disable the API feature. Stopping the timer alone keeps rules.
Never reset UFW, flush unrelated rules, switch Playerdata or delete volumes.

Offline coverage includes Python HTTP/storage tests, a DOM/fetch/Turnstile UI
harness, and compiled tests of the real bridge source with in-memory persistence
and a pinned upstream hashing fixture. Real RavenDB behavior, the installed
Director build and end-to-end external connectivity require deployment tests.

## Signing in from the server's home network

Some routers rewrite hairpin requests to a private router address. For this
installation, the direct Caddy connection check reported `192.168.86.0` from
the owner's home connection, while outside friends had public IPv4 peers.
If needed, explicitly add this to the private portal environment:

```ini
SPIRAL_LOCAL_SIGNUP_PEERS=192.168.86.0
```

Restart only the Python API after updating it. This permits account creation or
native password verification from that exact observed private peer; CAPTCHA,
rate limits and game-account lock/ban checks still apply. No public-IP grant is
written, no public address is guessed and any existing public grant for that
account is preserved. Turnstile's optional remote-IP field is omitted for this
case. Home gameplay must continue using an already permitted LAN/Tailscale
route; this setting does not add or widen game firewall rules.

The default is empty. Only exact RFC1918 IPv4 addresses are accepted, not
subnets, public addresses, loopback or caller-supplied body fields. This depends
on the loopback-only API and Caddy overwriting `X-Real-IP` from its connection
peer. Do not add CDN/proxy addresses here to bypass client-IP configuration.

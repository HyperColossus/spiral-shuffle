# Registered-player IP access

Implementation is opt-in and has offline regression coverage. Ubuntu UFW,
systemd and real HTTPS signup still require deployment testing. The repository
does not enable services, open ports or turn on website signup by itself.

## Flow

Successful HTTPS signup -> game account in the existing Playerdata database ->
active grant in `.runtime/portal/access.sqlite3` -> root-owned timer applies
only the `SpiralShuffleRegistered` UFW application rules.

The access database stores account names, public IPv4 addresses, expiry times
and SHA-256 hashes of random 256-bit access keys. It contains no passwords.
It is private operational data: do not commit, publish or paste it into chat.
SQLite is separate from RavenDB; this feature never switches the game database
or modifies existing characters or saves.

Players receive a key once at signup. The website's **Update Game Access** form
uses that key plus Turnstile to replace the player's previous address with the
current connection's address. No caller-supplied IP is accepted. Keys expire
after 90 days; network grants last 30 days after signup or refresh. Save the
key in a password manager. The website does not persist it in browser storage.
Lost or expired keys and existing accounts require operator assistance in this
first version; password-based recovery and self-service key rotation are not
implemented. The game still authenticates usernames and passwords normally.

The browser must remain open until the returned key is saved. A lost signup
response may require operator recovery because account creation and the local
grant database are separate transactions. Failed/duplicate registrations never
activate a grant. If activation fails after account creation, the response
includes `accessPending: true` and the saved key; investigate the inactive row
for that account rather than deleting/recreating the game account.

## Prerequisites

- Complete and test the existing registration bridge and HTTPS API from
  [README-PORTAL.md](README-PORTAL.md), using the current Playerdata database.
- Confirm router WAN/public IPv4 routing. Forward only login TCP 12010, realm
  TCP 12343–12348, patch TCP 12510 and patch downloads TCP 12379 to Ubuntu.
  Update the server's advertised addresses and listeners separately; never
  bind a process directly to a router's public IP if it is not on the host.
- Keep UFW active with default-deny incoming. Inspect existing rules first;
  broad public allows for these game ports bypass this allowlist. Do not reset
  UFW or change SSH, Docker, Tailscale or unrelated service rules.
- Keep RavenDB, API and bridge ports private. Do not forward 8090, 18080,
  12771 or 12772.
- First version supports public IPv4 only. Use an IPv4-reachable registration
  hostname (A record, no AAAA), and connect from the same network as the game.
  A VPN, browser proxy or separate browser egress can produce a different IP.
- The example Caddy configuration assumes **direct** HTTPS ingress: Caddy
  overwrites `X-Real-IP` with the connection peer. A CDN or tunnel requires
  separately configured trusted-proxy handling; otherwise every signup could
  record the proxy address. Never trust arbitrary forwarded headers.

## Install the isolated firewall worker on Ubuntu

These commands assume owner `hypercolossus` and checkout
`/home/hypercolossus/spiral-shuffle`. Adjust the root-owned service's `--owner`
and `--database` arguments if your paths differ. Inspect the files before
installing. No game restart or asset rebuild is needed for this worker.

```bash
cd ~/spiral-shuffle
python3 -m unittest discover -s tests -p 'test_portal*.py' -v
sudo ufw status verbose
sudo ufw show added
sudo install -d -o root -g root -m 0755 /usr/local/lib/spiral-shuffle
sudo install -o root -g root -m 0644 portal/access_firewall.py /usr/local/lib/spiral-shuffle/access_firewall.py
sudo install -o root -g root -m 0644 portal/spiral-shuffle-registered /etc/ufw/applications.d/spiral-shuffle-registered
sudo install -o root -g root -m 0644 portal/spiral-access.service /etc/systemd/system/spiral-access.service
sudo install -o root -g root -m 0644 portal/spiral-access.timer /etc/systemd/system/spiral-access.timer
sudo systemctl daemon-reload
```

The API has no sudo permission. A root-owned executable reads SQLite through
an unprivileged subprocess, revalidates every address and expiry, then invokes
UFW using fixed argument lists with no shell. Its UFW profile permits only
the four TCP port groups above. Unexpected rules using the reserved application
name abort synchronization; existing manual/Tailscale/SSH rules stay intact.
Never run a root timer against Python files in a web-writable checkout.

## Enable and test before enabling public website signup

1. Add `SPIRAL_IP_ACCESS=1` to the private `.runtime/portal/portal.env` along
   with the tested public HTTPS/Turnstile settings (`SPIRAL_PORTAL_PUBLIC=1`).
   Restart the API using its existing service. Both `/register` and
   `/access/refresh` must reach the same loopback API through Caddy. Local
   test mode deliberately refuses IP granting.
2. API startup creates the private access database, initially empty. Start
   and inspect a single sync before enabling the timer:

   ```bash
   sudo systemctl start spiral-access.service
   sudo systemctl status spiral-access.service --no-pager
   sudo journalctl -u spiral-access.service -n 20 --no-pager
   ```

3. Once the service succeeds, start the timer:

   ```bash
   sudo systemctl enable --now spiral-access.timer
   sudo systemctl list-timers spiral-access.timer --no-pager
   ```

4. Set `ipAccessEnabled: true` in `window.SPIRAL_SITE_CONFIG` only when the
   worker and new API are ready. Keep registration API/site-key settings blank
   on the public site until real HTTPS/Turnstile testing is ready. Use a private
   staging page on the configured Turnstile hostname to test before launch.
5. From an outside connection, register a test account, save its access key,
   and wait for a successful timer run. Inspect `sudo ufw show added` locally:
   only that connection's public IPv4 should receive a rule with application
   `SpiralShuffleRegistered` and comment `spiral-signup-v1`.
6. Confirm an unregistered outside connection cannot reach game ports and
   the registered connection can log in and enter the world. Move to a second
   public network and use **Update Game Access**. Confirm the old rule disappears
   unless another active account shares that IP, and the new rule appears.

## Operations and limitations

The timer runs 30 seconds after each completed sync. Access is queued, not
confirmed by the API; normal updates take about a minute at small scale.
Monitor failures with `journalctl -u spiral-access.service`. On a read or
validation failure, the worker preserves existing rules and exits unsuccessfully;
expired rules are therefore not removed until the worker is healthy again.
UFW changes are incremental; a command failure can leave partial progress and
the next successful run reconciles it. Existing TCP sessions may outlive rule
removal because UFW tracks established connections; revocation controls new
connections and does not log an account out of the game.

Access is per public IP, so people sharing a home or carrier NAT share network
reachability. IP access supplements account authentication. It is not identity
verification or DDoS protection. Changing an IP revokes it only when no active
grant needs it, and manually allowed addresses remain manually allowed.

For a lost key or an existing game account, verify ownership privately before
issuing a replacement through `portal_access.AccessStore`. Do not provide a
public endpoint that accepts only a username. To revoke an account's grants,
set `active = 0` for its username in the private access database as the game
owner, then run a sync; no RavenDB data needs to change.

To pause updates, stop `spiral-access.timer`; this leaves existing rules intact.
To withdraw all signup grants, deactivate rows in the private access database,
run a successful sync, then stop the timer and disable the API feature.
Do not delete the access database or application profile before cleanup.
Keep the API and worker versions updated together. Rules are reconstructed
from `ufw show added` rather than rule numbers, so interrupted runs can recover.
Never use `ufw reset`, flush unrelated rules or delete database volumes.

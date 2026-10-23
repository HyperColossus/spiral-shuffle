# Spiral Shuffle — Docker RavenDB player database

**Status:** staged in GitHub, NOT running on your Ubuntu machine until you run the commands.
**Scope:** database only. The six Imlight realms and Python signup API still run on Ubuntu. The website stays on GitHub Pages at https://spiral.hypercoloss.us/.

Imlight's pinned code explicitly supports an external RavenDB connection via `[Database] PlayerDatabaseUrl`. This is **not PostgreSQL or MySQL**: Imlight stores `Accounts`, `Wizards`, and related documents in RavenDB. Docker is a container runtime, not a database type.

## Why the new container is isolated

- Separate Compose project: `spiral-shuffle-ravendb`.
- Official, fixed-version RavenDB image: `ravendb/ravendb:7.2.6-ubuntu.24.04-x64` (matching the RavenDB 7.2 family of the Imlight 7.2.4 client).
- Dedicated persistent named volume `spiral-shuffle-ravendb_spiral_data`.
- Database bound ONLY to `127.0.0.1:18080` on the Ubuntu host. No router forwarding, Cloudflare routing, UFW public rule or external browser access to the DB.
- Your existing embedded RavenDB on `127.0.0.1:8090` stays untouched during migration.
- No edits to any existing Docker Compose file, bot, SSH, Tailscale, or portfolio.

**Unsecured local HTTP is for single-host development only.** Restrict local machine access to trusted users. If you later move the DB to a separate host, replace it with RavenDB TLS and certificate-based authentication; NEVER publish port 18080.

### Why RavenDB requires PublicNetwork inside this container

RavenDB 7.2 returns **HTTP 503** if `RAVEN_ServerUrl=http://0.0.0.0:8080` is combined with `Security.UnsecuredAccessAllowed=PrivateNetwork`. The container must bind its own network interface so Docker can forward the port, so this Compose project uses `RAVEN_Security_UnsecuredAccessAllowed=PublicNetwork` **inside Docker only**. That setting does not provide authentication: any host or container able to reach the RavenDB service can read/write its data. The Docker **host** port remains bound to `127.0.0.1:18080`, not the Ubuntu LAN or public IP, and Compose gives this service a separate project network. Do not attach untrusted containers, open host firewall/router ports, or proxy this database. For a less isolated or production deployment, provision TLS and RavenDB client certificates instead.

If you already started the older Compose file and get 503, update safely:

```bash
cd ~/spiral-shuffle
git pull --ff-only
sudo docker compose -f docker/ravendb.compose.yml up -d
curl -sS -i --max-time 10 http://127.0.0.1:18080/build/version
```

This recreates the service if the configuration changed but **does not delete its named volume**. Never use `docker compose down -v`.


## 1. Inspect Docker and start the dedicated service

On Ubuntu:

```bash
cd ~/spiral-shuffle
git pull --ff-only
docker compose -f docker/ravendb.compose.yml config
docker ps --format 'table {{.Names}}\t{{.Ports}}'
sudo ss -ltnp | grep ':18080' || true
```

Make sure TCP **18080** is not used by another service. The new container should not affect other running containers.

```bash
docker compose -f docker/ravendb.compose.yml up -d
docker compose -f docker/ravendb.compose.yml ps
docker compose -f docker/ravendb.compose.yml logs --tail 40
curl -I http://127.0.0.1:18080
```

If RavenDB requires license/EULA acceptance, review and accept those terms yourself through its documented setup; this project does not accept them on your behalf.

**Do not switch Imlight.ini yet.** At this stage a new RavenDB instance is empty.

## 2. Export existing player records first

You already have a working embedded RavenDB for Spiral Shuffle (normally at `127.0.0.1:8090`). Make sure you know which database is active: if you used the earlier temporary `admin / 123` setup, you might still be pointing to a separate test save directory. Do not migrate the wrong one.

Arrange a short maintenance window: prevent players from logging in during export and switch, so no progress is written after the export.

On your Windows computer, open PowerShell and create an SSH tunnel that grants your own browser access to both private databases:

```powershell
ssh -N -L 18090:127.0.0.1:8090 -L 18080:127.0.0.1:18080 hypercolossus@192.168.95.76
```

This tunnels the database ports to your Windows PC only. Do not expose them to the internet. Keep the PowerShell window open.

Open your browser:
- **Existing database:** `http://127.0.0.1:18090`
- **New Docker database:** `http://127.0.0.1:18080`

In **existing RavenDB Studio**, select database **Playerdata**. Use **Tasks → Export Database** to download a complete `.ravendbdump` file containing documents and related data, including attachments, counters, time series and indexes if offered. Handle the export as a private backup; it contains player records and password hashes. Verify the export file exists before continuing.

Official RavenDB export documentation: https://docs.ravendb.net/7.2/studio/database/tasks/export-database

## 3. Import into Docker RavenDB

In the **new Docker RavenDB Studio**, create a database named exactly **Playerdata** (capitalization matters). Use **Tasks → Import Data → From file (.ravendbdump)** to import the saved export from step 2.

After import, compare document counts and verify the **Accounts** and **Wizards** collections have the expected records. Do not rely on document count alone. Keep the original export file and embedded save directory for rollback.

Official docs: https://docs.ravendb.net/7.2/studio/database/tasks/import-data/import-data-file

Verify Ubuntu can read the new database:

```bash
cd ~/spiral-shuffle
python3 docker/db_config.py check
```

It refuses an empty/unreachable database. This is a preflight, not proof the complete migration succeeded; check collection counts in Studio.

## 4. Switch only Spiral Shuffle after verified migration

On Ubuntu, **once the import and counts have been verified**:

```bash
cd ~/spiral-shuffle
./Stop-Linux.sh
python3 docker/db_config.py switch --confirm-imported
./Play-Linux.sh
python3 linux_ctl.py status
```

The helper saves a timestamped, private `Imlight.ini.before-docker.*` backup. It changes only `PlayerDatabaseUrl` and `PlayerDatabaseCertificatePath`, directing the game to `http://127.0.0.1:18080` and explicitly avoiding a TLS certificate for this loopback-only HTTP connection. It **does not delete, move, or rewrite the embedded database**, and never changes other Docker containers. The no-certificate path is deliberately nonexistent because this pinned Imlight build otherwise calls .NET `Path.GetFullPath("")` on an empty certificate setting.

Test logging in with an **existing character** and make sure account/character state matches the export. If it fails, stop Spiral Shuffle and restore the saved config backup:

```bash
./Stop-Linux.sh
cp -p .runtime/server/Config/Imlight.ini.before-docker.YOUR_TIMESTAMP .runtime/server/Config/Imlight.ini
./Play-Linux.sh
```

Substitute the exact backup filename printed by the helper. Do not delete any database directories or use `docker compose down -v`.

## 5. Enable website account creation (separate backend)

Once existing game logins work against Docker RavenDB, follow [README-PORTAL.md](../README-PORTAL.md) to install the localhost-only `PortalRegistration.cs` bridge and run the Python registration API.

The flow is:

```text
spiral.hypercoloss.us  (GitHub Pages / HTTPS)
  -> register.hypercoloss.us (HTTPS registration API; to be deployed)
     -> 127.0.0.1:12771 (Python API)
        -> 127.0.0.1:12772 (token-protected Imlight bridge)
           -> Imlight's document store
              -> 127.0.0.1:18080 (Docker RavenDB)
```

Use the same `Playerdata` database for game login and registration. No Docker-to-host database port needs to be publicly reachable.

## 6. Verify backup/recovery

Keep periodic logical RavenDB backups OUTSIDE the live named volume, and test restoring them. Never rely only on copying RavenDB database files while it is running: RavenDB recommends consistent logical backups or snapshots.

Also keep your old embedded database and initial exported `.ravendbdump` until the new service has been validated.

RavenDB docs: https://docs.ravendb.net/7.2/backup/overview

## If a command fails

Do **not** reinstall Spiral Shuffle or delete any Docker volumes. Send the command's error/output (with passwords, tokens and private IDs removed). We'll fix that specific step.

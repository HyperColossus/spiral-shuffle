# Spiral Shuffle on Linux (experimental Ubuntu server port)

These files are an **add-on to a fork of [ljste/spiral-shuffle](https://github.com/ljste/spiral-shuffle)**. They reuse that project's build scripts, C# hooks, and game asset tools. They are not a standalone game and do not contain a Wizard101 client, copyrighted assets, or server binaries.

**Status:** Linux setup and controller are implemented and tested with synthetic fixtures. **Not yet verified building Imlight or running a real client on Ubuntu** because the required .NET 10 SDK, Revive101 install, and licensed game assets were unavailable in the build environment. It may need revision-specific adjustments.

The upstream project is licensed **AGPL-3.0**. Preserve upstream authorship, history, and license in the fork. These additions are intended to be distributed under the same license.

## What changes on Linux

- Uses an existing **Linux** Revive101/Imlight package, matching the release `r806919.Wizard_1_610`, rather than demanding `Imlight.Director.exe` and a Windows client on the server.
- Reuses the original repository's source patching and .NET asset tool to build randomized C# and a separate patched `Root.wad`. No Windows scripts are modified.
- Runs the game server through `dotnet Imlight.Director.dll` and the private patch host through Python, with dedicated PID files and logs.
- Keeps installed Imlight and Aurorium game files separate from `.runtime/` generated files.
- Makes a random administrator password and session salt; never uses the public `admin / compsci` default.
- Pins SpiralDB to a local copy so changes upstream do not silently change an ongoing run.
- Checks port conflicts before startup; never searches for or kills unrelated processes (including the preexisting Discord bot).
- Has a separate, explicit option to **locally** create a private client-patch ZIP from user-provided game assets; do not publish or upload it.

## Before starting

1. **Fork the upstream repo** using GitHub's Fork control: <https://github.com/ljste/spiral-shuffle/fork>. This overlay does **not** perform the fork automatically.
2. Clone your fork to your Ubuntu laptop: `git clone https://github.com/HyperColossus/spiral-shuffle.git` (change path if the fork name differs).
3. Copy these files to the root of your fork, keeping their names, and commit them to a new branch.
4. Install Python **3.12+** and the **.NET 10 SDK** on Ubuntu. Run as a regular unprivileged user, not root.
5. Obtain a legitimate, compatible Linux Revive101/Imlight installation and an Aurorium cache for the **same** game revision. Expected layout:

   ```text
   /path/to/Revive101/
     Imlight/
       Imlight.Director.dll
       Imlight.Director.deps.json
       Imlight.Director.runtimeconfig.json
       Imlight.CoreLib.dll
       Imcodec.ObjectProperty.dll
       RavenDBServer/
       spiraldb-cache/QuestTemplates/
       spiraldb-cache/NpcSpellInventory/
     Aurorium/data/V_r806919.Wizard_1_610/
       Data/GameData/Root.wad
       LatestFileList.bin
       LatestFileList.xml
   ```

   Matching releases: <https://github.com/Revive101/Imlight/releases/tag/r806919.Wizard_1_610> and <https://github.com/Revive101/Aurorium/releases>. Aurorium cache files are required and are not included in this port.

## Check, build, start

```bash
# Inspect prerequisites without touching the machine:
./Setup-Linux.sh --revive-path /path/to/Revive101 --check-only

# Build in an isolated .runtime/ directory, using a seed you choose:
./Setup-Linux.sh --revive-path /path/to/Revive101 --seed SPIRAL-101

# Run the server and loopback patch host (no GUI client is launched):
./Play-Linux.sh

python3 linux_ctl.py status
# Readiness and errors:
tail -n 100 .runtime/logs/server.log

# Stop ONLY this project's server and patch-host processes:
./Stop-Linux.sh
```

Login details are stored in `.runtime/login.json` (file mode `0600`); treat that file as a secret. **Don't commit `.runtime` or credentials**. If an existing login has the public default password, the Linux setup refuses to reuse it.

### Allowing a Windows client on the local network (advanced, unverified)

The **server runs on Ubuntu**, but the game client can stay on a Windows computer. Run with the Ubuntu laptop's **actual LAN IP**, for example:

```bash
./Setup-Linux.sh --revive-path /path/to/Revive101 \
  --seed SPIRAL-101 --game-ip 192.168.1.50 --allow-lan
```

The game server will advertise that address. The local Python patch host remains loopback-only; you'll need a **separately validated client setup** appropriate to the game revision before remote clients can connect. Do not port-forward publicly, expose RavenDB, or claim that an untested Windows client will work immediately. Review UFW/firewall rules for the login and game ports separately.

To package your **own** generated patch for a separate, legally obtained and compatible Windows client, add `--export-client-patch /private/path/ClientPatch.zip` to setup. This ZIP includes proprietary user-owned content; **never commit, publish, or redistribute it**. How to integrate it with a particular Windows client still needs an in-person validation of the release.

### No client on Ubuntu required

These files intentionally do **not** use Wine or execute `WizardGraphicalClient.exe`. A local Ubuntu game client under Wine might be possible but is a different, untested project. For the first server milestone, use a separate compatible Windows client.

## Safety and troubleshooting

- `.runtime/` must remain ignored by Git (already in upstream `.gitignore`, verify your fork).
- If `--check-only` reports missing files, fix the Imlight/Aurorium data layout rather than bypassing checks.
- The build may take significant RAM/disk due to asset processing; do not run it while your existing laptop workload is resource-constrained.
- If the server build fails, review the .NET output. The wrapper intentionally does not patch binaries of unrelated projects.
- If `Play-Linux.sh` says a port is in use, **do not stop your Discord bot**. Inspect it with `sudo ss -lntup`, and resolve the port conflict explicitly.
- If the controller reports a stale/unowned PID file, manually inspect the PID with `ps` rather than force-killing it.
- If you update Imlight or game assets to a different revision, rebuild and revalidate the C# patch anchors, assembly dependencies, WAD metadata, and full story quests.
- This is a **Linux server port**, not a claim that NPC, portal, or level-up Wildcard systems from your other project have been merged yet.

## Checks

Run the isolated Linux wrapper tests (synthetic fixtures, no assets needed):

```bash
python3 -m unittest discover -s tests -p 'test_linux_port.py' -v
bash -n Setup-Linux.sh Play-Linux.sh Stop-Linux.sh
```

# Persistent services and automatic tested-main deployment

One initial Ubuntu installation replaces the repeated terminal commands.
The game and API survive logout/reboot and restart after process failures.
Every two minutes, an unprivileged pull service checks GitHub `main`. It deploys
only the exact commit that passes the always-running **Spiral deployment gate**
push workflow. Pull-request checks, an older successful run, another repository
or a different revision cannot authorize deployment.

No GitHub token, deploy SSH key, inbound admin port, website secret or sudo
permission is given to the web API/deployer. GitHub is contacted outbound. The
public `/deployment` endpoint exposes only revision, status and update time;
it never exposes credentials, private logs, paths or account data.

## One-time setup on Ubuntu

Run as the existing `hypercolossus` game owner from the current checkout.
The feature branch must include these files and pass its checks first:

```bash
cd ~/spiral-shuffle
git pull --ff-only && python3 install_services.py
```

This requires your sudo password once; never send it to chat. Do not run the
installer as root. If Git reports local changes/conflicts, preserve them and
inspect rather than resetting. The installation requires the existing private
portal environment, generated game runtime, patched source and compatible .NET
SDK. If Node is absent, only the Node package needed for offline checks is added.

The installer runs portal/operations/website checks first, enables lingering
for the game owner, and stops only the owned game/API processes. It installs
user services, builds the current registration bridge, starts the game and
waits for bridge/game readiness before starting the API. It preserves the live
CoreLib, generated assets, database configuration, portal environment and saves.
It also installs/starts the already tested dedicated root-owned IP-access worker.
The only web-server edit adds `/deployment` to the existing project proxy route;
the old site configuration is backed up and validated before reload.

While the feature is still a draft, automatic updates report `waiting_for_main`.
The service contract file prevents the older current main from replacing the
working test installation. After this feature is merged, the exact main commit
must pass the new push gate before any update is applied. Merging is separate
from installation; the installer never merges PRs or changes GitHub branches.

## What gets deployed

- Website/docs/test-only revisions: checkout update; no game/API restart.
- Python API or IP-grant storage code: restart only the API after local checks.
- Registration bridge code: stop API/game, build and install only Director,
  restart the game, wait for readiness, then start the API.

Generated game assets, arbitrary CoreLib/game changes, database/container files,
privileged firewall helpers, service-unit changes and supervisor self-updates
pause with `blocked_scope`. They require a reviewed installation rather than
running arbitrary migration/asset-rebuild commands. This initial automation is
for the repeated website/API/registration-bridge tasks in this project.

## Failure behavior

Each update keeps the previous Director and CoreLib plus its Git revision. It
checks out the exact approved candidate, runs offline checks before stopping
services, and verifies game TCP ports, bridge health and API health afterward.
If installation/startup/health fails, it restores the old code and binary and
starts the previous services. CoreLib changes during a bridge install are
detected and restored, preserving the prior game-entry repair. No database,
volume, save directory or generated WAD is deleted or switched.

A deployment journal also recovers an interrupted update after reboot. The
same failed revision is not retried repeatedly; a new tested main commit can
retry the fix. A recovery that cannot restore health reports `recovery_failed`,
which requires investigation; it is never reported as a successful rollback.
Tracked/untracked checkout changes block deployment rather than being discarded.
Successful deploys leave the checkout detached at the deployed commit; the timer
fetches main itself, so there is no need to run `git pull` after installation.

Health checks establish process/network readiness, not full in-game correctness.
Account creation, Turnstile, world entry and game behavior still need real-client
validation for changes affecting them. Privileged firewall/network changes are
not automatically applied by Python deployment code.

## Status and recovery controls

The assistant can read:

```text
https://register.spiral.hypercoloss.us/deployment
```

Private Ubuntu status/logs when deeper diagnosis is needed:

```bash
systemctl --user status spiral-game spiral-portal spiral-auto-deploy.timer
journalctl --user -u spiral-auto-deploy.service -n 30 --no-pager
sudo journalctl -u spiral-access.service -n 20 --no-pager
```

To pause automatic deployments without stopping the game:

```bash
systemctl --user disable --now spiral-auto-deploy.timer
```

To resume:

```bash
systemctl --user enable --now spiral-auto-deploy.timer
```

No unrelated Docker, Discord, SSH, Tailscale or portfolio service is restarted.

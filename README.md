# Spiral Shuffle

A Wizard101 randomizer for Revive101. Spells, gear stats, drops, and enemies get shuffled. Pick Shuffle, Chaos, or Nightmare in the in-game menu. You can keep creature appearances and randomize just their stats. Hover over enemies during card selection to see their rolls.

Travel and spawn points stay normal.

Spell numbers roll for attacks, drains, heals, blades, traps, shields, and enchants. Power ranges are 75-125% in Shuffle, 50-150% in Chaos, and 50-200% in Nightmare.

Your usual school trainer's spells stay free, even when they shuffle into another school. Trainer rolls include utility spells and avoid repeated starter offers.

Optional vendor shuffle gives each wizard their own random item and treasure-card stock, saved across reloads.

Random Item Cards puts one random spell on every hat, robe, pair of boots, wand, athame, amulet, ring, and deck. Any school, with fresh number rolls for each piece. Turn it off to restore the usual item cards.

Infinite Mana keeps your mana full, including after casting, marking a location, or fleeing. Toggle it in the book menu.

Optional spell drops permanently teach new spells from creature kills. Type a rate from 0.1%-100% and press Set. Random mode rolls separate odds per enemy, usually low; Fixed uses your exact rate. Fixed at 100% guarantees a new spell per creature until you know them all. Hover to see the odds. A parchment popup reveals your cards after the fight.

## Setup

Windows, Python 3.12+, .NET 10 SDK, and a working Revive101 install with the full **r806919.Wizard_1_610** Aurorium files. Allow about 25 GB for the separate client.

1. Download this repo and extract it.
2. Open a terminal in its folder and run:
   ```powershell
   .\Setup.cmd "C:\path\to\Revive101"
   ```
3. Open `Play.cmd` and log in with **admin** / **compsci**.
4. Pick a difficulty, then create a level 1 wizard or choose an existing one.

If your game client isn't in the usual location, add `-ClientPath "C:\path\to\Wizard101"` to setup. Close the game, then run `Stop.cmd` when you're done.

To update, run `Stop.cmd`, replace the source files, and rerun `Setup.cmd` with the same install path. Keep `.runtime` to preserve your wizards. Existing local admin accounts are updated to the same login.

This uses its own save. Game files and personal account data aren't included in the repo. Early main-quest kill credit was tested; a full story run hasn't been tested. Nightmare can be rough at level 1.

AGPL-3.0. Built on [Imlight](https://github.com/Revive101/Imlight/tree/b07987168fcfa2d123a5d78df90ca0c76405e427) and [Imcodec](https://github.com/Jooty/Imcodec/tree/r806919.Wizard_1_610). Not affiliated with KingsIsle.

## Linux server (experimental)

This fork includes an **Ubuntu/Linux server-side build and process controller**. It reuses Spiral Shuffle's existing Imlight patches and asset generation, without launching a Windows game client on the server. It requires a matching Linux Imlight release, Aurorium game data, Python 3.12+, and .NET 10. Its full server/client workflow has **not** been validated yet.

See [README-LINUX.md](README-LINUX.md) for prerequisites, commands, network cautions and tests. The original Windows `Setup.cmd` / `Play.cmd` workflow remains intact.

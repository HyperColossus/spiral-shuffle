# Spiral Shuffle

A Wizard101 randomizer for Revive101. Spells, gear stats, drops, and enemies get shuffled. Pick Shuffle, Chaos, or Nightmare in the in-game menu. You can keep creature appearances and randomize just their stats. Hover over enemies during card selection to see their rolls.

Travel and spawn points stay normal.

## Setup

Windows, Python 3.12+, .NET 10 SDK, and a working Revive101 install with the full **r806919.Wizard_1_610** Aurorium files. Allow about 25 GB for the separate client.

1. Download this repo and extract it.
2. Open a terminal in its folder and run:
   ```powershell
   .\Setup.cmd "C:\path\to\Revive101"
   ```
3. Open `Play.cmd`. Your new local login is in `.runtime/login.txt`.
4. Pick a difficulty, then create a level 1 wizard or choose an existing one.

If your game client isn't in the usual location, add `-ClientPath "C:\path\to\Wizard101"` to setup. Close the game, then run `Stop.cmd` when you're done.

This uses its own save. Game files, accounts, and passwords aren't included in the repo. Early main-quest kill credit was tested; a full story run hasn't been tested. Nightmare can be rough at level 1.

AGPL-3.0. Built on [Imlight](https://github.com/Revive101/Imlight/tree/b07987168fcfa2d123a5d78df90ca0c76405e427) and [Imcodec](https://github.com/Jooty/Imcodec/tree/r806919.Wizard_1_610). Not affiliated with KingsIsle.

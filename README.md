<!-- SPDX-License-Identifier: LicenseRef-GDE-PolyForm-NC-1.0.0-plus-terms -->
<!-- Modified by GustavoARodri on 2026-10-27: Prepared the public project overview and Windows usage guide. -->

# Guitar Hero PC

A native Windows launcher and PC reimplementation for **Guitar Hero (PlayStation 2)**. The game reads the disc image supplied by the player; this repository contains no game image, songs, models, textures, or other original game data.

**Based on Guitar Dream Engine, https://github.com/mbracali/prj-gd-engine**

> This is an independent, non-commercial project. It is not affiliated with or endorsed by the owners of Guitar Hero or RedOctane.

## Quick start

1. Download or clone this repository on Windows x64.
2. Run `Guitar Hero.exe`.
3. Choose your legally obtained Guitar Hero (PS2) ISO when prompted.
4. Select resolution, frame-rate cap (60 or 120 FPS), window mode, and optional split keyboard, then start the game.

Keep `Guitar Hero.exe` next to the `gd01` folder. The launcher and engine require the included `gd01/SDL3.dll`. The 120 FPS option sets a cap; actual performance depends on the PC and game scene.

## Controls

- Menus: arrow keys and Enter; Esc goes back.
- Frets: 1–5 or F1–F5; strum: Up/Down.
- Star Power: Space; whammy: W; pause: Esc.
- For two-player keyboard mode, enable split keyboard in the launcher. Player 2 uses H/J/K/L/; for frets, N/M to strum, B for Star Power, and O for Start.
- USB guitars and gamepads can be configured in the in-game GAMEPADS screen.

## Source and building

- Engine source and platform build instructions: [`gd-game-01/`](gd-game-01/README.md) and [`gd-game-01/BUILD_INSTRUCTIONS.md`](gd-game-01/BUILD_INSTRUCTIONS.md).
- Windows launcher source: [`launcher-src/`](launcher-src/). Build it on Windows with PowerShell: `powershell -ExecutionPolicy Bypass -File .\launcher-src\Build.ps1`.
- Changes from the upstream base: [`CHANGES.md`](CHANGES.md).

The engine is an alpha reimplementation. The PAL intro path and two-player gameplay flow have been exercised; physical two-controller multiplayer still needs validation on a PC with both controllers connected. See the source project's backlog for known engine limitations.

## Licences and attribution

The engine is distributed under the **PolyForm Noncommercial License 1.0.0 plus Guitar Dream Engine's additional licence terms**. Both [`LICENSE`](LICENSE) and [`LICENSE-ADDITIONAL-TERMS.md`](LICENSE-ADDITIONAL-TERMS.md) apply and must stay unchanged. This software cannot be sold or put behind a paywall. Third-party components keep their own licences; see their included files and `gd01/licenses/`.

`SDL3.dll` is accompanied by its upstream licence at [`gd01/licenses/SDL3-LICENSE.txt`](gd01/licenses/SDL3-LICENSE.txt).

The original game is not included. You must supply your own game media.
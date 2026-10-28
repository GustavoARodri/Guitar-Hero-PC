<!-- SPDX-License-Identifier: LicenseRef-GDE-PolyForm-NC-1.0.0-plus-terms -->
<!-- Modified by GustavoARodri on 2026-10-27: Recorded every project change against the upstream base. -->

# Changes

Date: 2026-10-27  
Upstream base: Guitar Dream Engine, game 01, commit `af83561fa26779fa8fb65a8a476024b2fd14eb8b`.

## Engine source changes

- `gd-game-01/engine/src/app.cpp`: Added fallback lookup for PAL intro movies; corrected multiplayer track reveal timing and player-specific input/event routing; ignored chart actions before song start and after song end.
- `gd-game-01/engine/src/game/chart.cpp`: Prevented notes owned by different face-off players from being merged into one chord.
- `gd-game-01/engine/src/game/guitar_walk.cpp`: Added the required C standard library include for the Windows build.
- `gd-game-01/engine/src/render/track_renderer.cpp`: Corrected face-off viewport projection/framing, sustain-tail scaling, and multiplayer rail warning state.
- `gd-game-01/engine/src/scene/scene.cpp`: Normalized archive paths and added fallback loading for shared textures used by localized UI panels.
- `gd-game-01/engine/src/ui/career_objects.cpp`: Added the required C standard library include for the Windows build.
- `gd-game-01/engine/src/ui/panel.cpp`: Preserved the localized directory when resolving panel textures.
- `gd-game-01/engine/src/ui/select_panels.cpp`: Added the required C standard library include for the Windows build.
- `gd-game-01/engine/src/ui/ui_system.cpp`: Added localized Options panel lookup, registered the end-of-song panel, and matched the Enhanced Settings button style to other selectable entries.
- `gd-game-01/engine/third_party/fidelityfx/ffx_vk_volk.cpp`: Added the mutex header required by the bundled FidelityFX Vulkan backend.

## Added project files

- `launcher-src/Program.cs`, `Launcher.UI.cs`, and `Launcher.Run.cs`: Native Windows launcher with ISO selection/validation, resolution and frame-rate settings, split-keyboard option, launch logging, and icon integration.
- `launcher-src/Build.ps1`: Builds the Windows launcher and its multi-size icon using the .NET Framework compiler supplied with Windows.
- `launcher-src/guitar-hero.ico` and its licence notice: Custom Guitar Hero-style guitar icon for the launcher.
- `gd01/README.md`: Runtime instructions and notice for the packaged Windows build.
- Root `README.md` and `.gitignore`: Public project overview, startup/build steps, and exclusions for personal configuration, logs, and game media.
- `gd01/licenses/SDL3-LICENSE.txt`: Licence notice for the packaged SDL3 runtime DLL.

The repository also contains the Windows launcher and engine binaries built from these sources. No ISO or original game assets are included.
# Be Stiff – MonoGame port

Port of the original XNA 4.0 game to MonoGame 3.8.5 (DesktopGL, .NET 8).
The C# sources were recovered by decompiling the shipped binaries with ILSpy,
since the original project folder was lost.

## Layout

- `BeStiff/` – the game (MonoGame DesktopGL project). Content lives in
  `BeStiff/Content/`: the original compiled `.xnb` assets are used as-is,
  while shaders (`Effects/*.fx`) and music (`Audio/Music/*.ogg`) are rebuilt
  through the MonoGame content pipeline (`Content.mgcb`).
  Game code is grouped by area, one namespace per folder
  (`Be_Stiff.<Folder>`, imported project-wide in `GlobalUsings.cs`):
  - root – `Program`, `Game1`, `Globals` (options and save games),
    `GameSettings`, `DebugKeys`, `CaseInsensitiveContentManager`
  - `AI/` – enemy brains and their states, squads, path finding
  - `Audio/` – music/sound manager, noise events heard by enemies
  - `Characters/` – `Human`, `Hero`, enemies; `Characters/States/` holds
    the hero state machine
  - `Graphics/` – sprites, camera, particles, tiles, HUD;
    `Graphics/Backgrounds/` the parallax layers
  - `Input/` – keyboard/gamepad controls
  - `Levels/` – level loading, zones, portals, score, and
    `GameElementsControl` (the per-level world, split into partial files:
    core, `.Content`, `.Rendering`, `.Debug`)
  - `Physics/` – Farseer helpers: ray casts, contact handling, unit
    conversion, collision filters
  - `Screens/` – screen manager and all menus/screens
  - `Utils/` – small math, vector and string helpers
  - `Weapons/`, `Weapons/Projectiles/`
  - `WorldObjects/` – base classes and interfaces; `Props/`, `Goals/`,
    `Pickups/` for the concrete level objects
- `Libs/` – the third-party XNA libraries the game depends on, recompiled
  against MonoGame: Farseer Physics, DebugViewXNA, OgmoXNA, SKAnimation,
  Krypton, ProjectMercury. `EasyStorage` is a rewrite on plain file IO
  (the original used the removed XNA Storage/GamerServices APIs); saves go
  to `~/.local/share/BeStiff/`.
- `Tools/fxccs/` – tiny Windows-side helper that MonoGame 3.8.5's effect
  compiler invokes inside Wine but does not ship. Build output is copied to
  `~/.winemonogame/drive_c/fxccs.dll`.
- `Tools/dx9dis.py` – DirectX 9 shader bytecode disassembler used to recover
  the game's pixel shaders from the compiled effects.

## Building on Linux

One-time setup (already done on this machine):

1. `sudo dnf install dotnet-sdk-8.0 wine-core p7zip`
2. Run MonoGame's `mgfxc_wine_setup.sh` (creates `~/.winemonogame` with a
   Windows .NET SDK and `d3dcompiler_47.dll`).
3. `winepath` wrapper in `~/.local/bin/winepath`:
   `#!/bin/sh` / `exec wine winepath "$@"`
4. `cd Tools/fxccs && dotnet build -c Release && cp bin/Release/net8.0/fxccs.* ~/.winemonogame/drive_c/`
5. `export MGFXC_WINE_PATH=$HOME/.winemonogame` (also appended to `~/.profile`).

Then:

```
# from the repository root
export MGFXC_WINE_PATH=$HOME/.winemonogame
dotnet build BeStiff
dotnet run --project BeStiff
```

Debug shortcut to skip the menus and load a level directly (also
auto-confirms the "get ready" box):

```
BESTIFF_LEVEL=Tutorial1 dotnet run --project BeStiff
```

Available levels: `Tutorial1`, `Tutorial2`, `TestLevel`.

`F12` in game saves `~/bestiff_shotN.png` plus the intermediate render
targets (`~/bestiff_shotN_*.png`) and prints the hero's state.

Debug switches (environment variables; "set" means any value):

| Variable | Effect |
|---|---|
| `BESTIFF_LEVEL=<name>` | Skip the menus, load that level, auto-confirm the "get ready" box |
| `BESTIFF_HERO_POS=x,y` | Teleport the hero to display (pixel) coordinates after the level loads |
| `BESTIFF_KEYS=<spec>` | Scripted input: hold keys during frame ranges, e.g. `100-150:D;200-230:Space,A` (frames counted from the first update, key names from `Keys`) |
| `BESTIFF_SHOT=<png>` | Save a screenshot (plus render targets, like `F12`) to that path at frame `BESTIFF_SHOT_FRAME` |
| `BESTIFF_SHOT_FRAME=<n>` | Frame for `BESTIFF_SHOT` (default 180) |
| `BESTIFF_DUMP` | Print diagnostics to stderr: level/grid scale, sprite scales, bones, texture regions, enemies, decals, bad platform/elevator velocities, first Krypton light passes |
| `BESTIFF_NO_CANNON` | Don't spawn wall cannons |
| `BESTIFF_JUMP_FRAME=<n>` | Apply an upward impulse to the hero at physics frame n |
| `BESTIFF_KILL_FRAME=<n>` | Kill the hero (ragdoll death) at physics frame n |
| `BESTIFF_KILL_ENEMY_FRAME=<n>` | Kill every enemy (ragdoll death) at physics frame n |
| `BESTIFF_DEATH_LOG` | Log the dead hero's body state every 10 physics frames |
| `BESTIFF_PERF` | Frame profiler: logs frames over 25 ms (split into update / draw steps / present wait), garbage collections, content loaded from disk, and a summary every 300 frames (allocation per frame, heap size) |
| `BESTIFF_STATE_LOG` | Log every hero state change (time, state names, body rotation, position, energy) |
| `BESTIFF_GIRDER_LOG` | Log every girder's body and rope state every 15 physics frames |
| `BESTIFF_KRYPTON_DUMP=<prefix>` | Save the Krypton light map at frame 60 as `<prefix>_postlight.png`, `_blurH.png`, `_postblur.png` |
| `BESTIFF_KR_LOG` | Log the first two light passes' render state |
| `BESTIFF_KR_NOSTENCIL` | Draw lights without the shadow stencil test |
| `BESTIFF_KR_NOSCISSOR` | Draw lights without the scissor rectangle |
| `BESTIFF_KR_FLIPSCISSOR` | Flip the light scissor rectangle vertically |
| `BESTIFF_KR_NOCULL` | Draw lights with culling off |

## Porting notes

- The shipped build was mid-migration: the November 2011 project doubled
  character/object sizes (48 px grid) but `Tutorial2` and `TestLevel` were
  still exported against the August project (24 px grid), regular-enemy
  sprites, enemy animations, the fat guy skeleton and several object sprites
  were left at the old size. All hard-coded body sizes in the code match the old
  art at 24 display pixels per simulation unit, while the current project
  uses 48. The port detects such levels (grid bitmap vs. project grid) and
  plays them at 24 px/unit: pixel coordinates kept, catwalk tiles at 24 px,
  24 px collision grid, template-derived object sizes halved, hero skeleton
  and art at half, regular enemies with old-scale skeleton lengths, and the
  twelve sprites that were redrawn at 2x (slopes, exit door, barrels,
  chairs, table) drawn at half. Current-scale levels (Tutorial1) use 48
  px/unit with regular-enemy art at 2x and the fat guy skeleton scaled 2x.
  An old build lives in `~/Downloads/Be Stiff_old` and the Ogmo project in
  `~/Downloads/Maps` for reference.
- Krypton on OpenGL: MonoGame effect passes replace the whole rasterizer
  state, so every pass in `KryptonEffect.fx` sets `CullMode = None`
  explicitly (XNA kept the device state). The light map is cleared with
  alpha 0 and the shadow pass writes alpha 1, which the line-of-sight
  overlay (`AlphaShadow.fx`) uses as its fog mask.
- Farseer: bodies that come out of the solver non-finite are restored to
  their pre-step position (degenerate contacts under strict 32-bit floats).

- Asset names in the code use Windows casing and backslashes;
  `CaseInsensitiveContentManager` resolves them against the real files.
- The original effects only contained compiled `ps_2_0` bytecode. They were
  disassembled and rewritten as HLSL (`Content/Effects/*.fx`). Krypton's
  effect is the original source from the Krypton project (via the OUYA
  MonoGame port), which matches the compiled version's techniques.
- WMA music was converted to Ogg Vorbis with ffmpeg.

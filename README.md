# Be Stiff – MonoGame port

Port of the original XNA 4.0 game to MonoGame 3.8.5 (DesktopGL, .NET 8).
The C# sources were recovered by decompiling the shipped binaries with ILSpy,
since the original project folder was lost.

## Layout

- `BeStiff/` – the game (MonoGame DesktopGL project). Content lives in
  `BeStiff/Content/`: the original compiled `.xnb` assets are used as-is,
  while shaders (`Effects/*.fx`) and music (`Audio/Music/*.ogg`) are rebuilt
  through the MonoGame content pipeline (`Content.mgcb`).
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
cd MonoGame
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

Other debug switches: `F12` in game saves `~/bestiff_shotN.png` plus the
intermediate render targets; `BESTIFF_SHOT=<png>` / `BESTIFF_SHOT_FRAME=<n>`
do the same unattended; `BESTIFF_DUMP=1` prints level, bone and physics
diagnostics; `BESTIFF_HERO_POS=x,y` teleports the hero after load.

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

# Class icons (CustomClasses)

PNG icon for each class — shown next to the **player name** (deploy / character / online, via
`ChatSpecialIcon`) and in the class "seal" (menu and Skills screen). The icon is a white
silhouette **tinted with the class `nameColor` at runtime**.

## How it works

- A class points to its icon via `"iconFile": "<name>.png"` in its JSON
  (`modded/Server/config/classes/<class>.jsonc`).
- The build (`/compile-mod`) copies every `*.png` in this folder to
  `BepInEx/plugins/CustomClasses/icons/` on the SPT install.
- At runtime the client loads the PNG from that folder by file name only
  (path characters are stripped for safety).

## Editing / adding an icon

1. Drop a `*.png` here using the **same name** referenced by the class `iconFile`.
2. Run `/compile-mod CustomClasses` (or just copy the PNG to the install folder).
3. **Restart the game** (the icon is cached on load).

No recompile of the DLL is needed to change the artwork — only the PNG file.

## Format

- **PNG** with transparency (the only raster format the game decodes; SVG is not supported).
- **Square** recommended (e.g. 128×128 or 256×256). Non-square works — the icon is shown
  with `preserveAspect` (no distortion), fit into the seal's icon box.
- Keep it small/clean — it renders at ~40–48 px in-game.
- Missing/invalid file → the class shows the **name only** (no icon), no error.

## Current icons

The shipped PNGs are **white silhouettes** (alpha mask) derived from
[game-icons.net](https://game-icons.net/) — see [ATTRIBUTION.md](./ATTRIBUTION.md) (CC BY 3.0).
They are **tinted with the class `nameColor` at runtime**, so the icon always follows the class
color (icon + name match) — mirroring how EFT tints its edition icons (Unheard, EOD, …).

The art is generated, not hand-edited: the vendored SVGs live in `scripts/icon-sources/` and
`scripts/build-icons.mjs` rasterizes them to 256×256 white PNGs here.

To change an icon:

1. Replace the matching `.svg` in `scripts/icon-sources/` (a white-on-transparent game-icons SVG).
2. From `scripts/`: `npm install` (first time), then `npm run build:icons`.
3. Update [ATTRIBUTION.md](./ATTRIBUTION.md) with the new icon/author.
4. `/compile-mod CustomClasses` and **restart the game**.

> Tip: `scripts/preview-icons.mjs` renders a tinted contact sheet (`scripts/preview/`) so the
> white silhouettes can be reviewed over a dark background before shipping.

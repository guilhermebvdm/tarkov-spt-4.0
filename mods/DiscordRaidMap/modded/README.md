# Discord Raid Map

Client mod that posts a raid map image to Discord while a raid is running. Works on headless fine (I think).

## Displayed

- Players (alive and dead)
- Player-killed enemies
- Player-killed bosses
- Airdrop
- Active extracts
- Unlockable extracts
- Raid time remaining

## How to use
- Set `Discord > Webhook Url` in the generated BepInEx config.
- Add `.ttf` or `.otf` files to `Assets\Fonts`, restart the game, then choose one with `Map Text > Map Text Font`. `Default` uses the built-in bitmap font.
- The mod creates one webhook message at raid start, edits it on an interval, and deletes it at raid end.
- Marker images are loaded from `Assets\Markers`. Replace `player.png`, `skull_enemy.png`, `skull_boss.png`, `airdrop.png`, `extract.png`, or `extract_requirements.png` to customize them.
- `Markers > Marker Display Size` controls how large markers appear on the map. Source PNGs can be larger than this value for sharper downsampling.

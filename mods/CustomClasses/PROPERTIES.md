# PROPERTIES.md — CustomClasses (client F12 / BepInEx)

Plugin: `customclasses.mdj.client` ("CustomClasses") — see [modded/Client/Plugin.cs](modded/Client/Plugin.cs) and [modded/Client/PerksConfig.cs](modded/Client/PerksConfig.cs). PT version: [PROPRIEDADES.md](PROPRIEDADES.md).

Properties exposed in the configuration menu (F12 / ConfigurationManager). None is **(Advanced)**.

> **Layout (reorg 2026-07-10):** one **section per class** (perks + drawbacks together). The numeric prefix (`0 ·`, `1 ·`…) forces the F12 order — ConfigurationManager sorts sections alphabetically. Order: system (`0`/`1`), then the 6 classes in roster order (`2`–`7`), then global fixes (`8`).
>
> **Language:** the F12 is a **BepInEx** plugin, not part of EFT — it does **not** follow the game language (strings are fixed at `Awake`, before EFT loads its locale). So **section/property names stay in English** and the **descriptions (tooltips) are bilingual `PT / EN`** on the same line. This file (EN) and [PROPRIEDADES.md](PROPRIEDADES.md) (PT) document per language.
>
> **Split shared perks:** Pack Mule (Scavenger + Tank) and Loud Operator (Rifleman + Tank) have their **own per-class config** — each section shows its own, with independent values.

---

## Section `0 · General`

| Property | Type | Default | What it does |
|---|---|---|---|
| `EnableSkillMultipliers` | bool | `true` | Toggle the per-class skill XP-gain scaling. |
| `ShowMultiplierOnSkills` | bool | `true` | Multiplier highlight on skills (colored border + ±X% arrow + class tooltip). |
| `ShowClassOnPlayerName` | bool | `true` | Class icon + name (gradient) on the player's name (deploy, character, online list). (item 015) |
| `ShowClassIdentity` | bool | `false` | Separate class seal in the menu and at the top of the Skills screen. (item 012) |
| `ShowSkillsButton` | bool | `true` | SKILLS button in the menu (below CHARACTER) that opens the Skills screen. (item 013) |
| `ShowLevelUpFlavor` | bool | `true` | Level-up notification `EASILY` (buff) / `FINALLY` (debuff) on skills with a multiplier. (item 014) |
| `Raid-start perks notification` | bool | `true` | Raid-start notification listing the class's perks (green) and drawbacks (red). |
| `Perk Diagnostics overlay` | bool | `false` | Live overlay of the properties affected by your player's perks (validation). |

## Section `1 · Interface & Position`

> Class-identity UI offsets (px). `SkillsClassPos*` are **sliders** that apply in real time (with the Skills screen open).

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `SkillsClassPosX` | float | `0` | −1000..1000 | Class seal (Skills screen) — horizontal offset from center. |
| `SkillsClassPosY` | float | `-20` | −1000..1000 | Class seal (Skills screen) — vertical offset from top. |
| `ClassIconRatio` | float | `1.35` | 0.8..2.5 | Icon size = name font × ratio (keeps the icon:font proportion across screens). |
| `DeployNameScale` | float | `1.2` | 1.0..4.0 | Scale of the player icon+name on the deploy/loading screen (icon and name grow together). |
| `Class Tab — X offset` | float | `0` | −400..400 | Fine-tune the CLASS tab button horizontal position. |
| `Class Detail on Loading Screen` | bool | `true` | — | Class detail (perks/drawbacks) on your name in the raid loading screen (FIKA). (item 055) |
| `Class Detail — Loading panel scale` | float | `0.75` | 0.5..1.0 | Scale (zoom-out) of the loading-screen class popover (same footprint, smaller content). |
| `Weight Marker — X offset` | float | `-107.0423` | −600..600 | Horizontal position of the `▲ +X%` weight marker (Health tab). Default calibrated in-game. (item 056) |
| `Weight Marker — Y offset` | float | `50.70423` | −600..600 | Vertical position of the `▲ +X%` weight marker (positive = up). Default calibrated in-game. (item 056) |

## Section `2 · Combat Medic`

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Efficient Metabolism — Enabled` | bool | `true` | — | Perk: slower hunger/thirst drain. |
| `Efficient Metabolism — Hunger/thirst drain` | float | `0.85` | 0.5..1 | Hunger/thirst drain (0.85 = 15% slower). |
| `Shaky Hands — Enabled` | bool | `false` | — | Drawback: more recoil (shaky hands). **Off by default** (balance B1). |
| `Shaky Hands — Recoil mult` | float | `1.25` | 1..2 | Recoil (1.25 = +25%). |

## Section `3 · Rifleman`

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Cool Under Fire — Enabled` | bool | `true` | — | Perk: less flinch when hit. |
| `Cool Under Fire — Flinch mult` | float | `0.5` | 0..1 | Camera flinch when hit (0.5 = −50%). |
| `Cool Under Fire — Malfunction chance mult` | float | `0.5` | 0..1 | Weapon malfunction chance (0.5 = −50%, anti-jam). |
| `Adrenaline — Enabled` | bool | `true` | — | Perk: dealing/taking damage opens a window with better recoil/reload/ADS. |
| `Adrenaline — Window (s)` | float | `25` | 5..120 | Window duration (renewed on each new damage). |
| `Adrenaline — Cooldown (s)` | float | `120` | 0..600 | Cooldown after the window before it can re-trigger. |
| `Adrenaline — Recoil mult` | float | `0.7` | 0.3..1 | Recoil during the window (0.7 = −30%). |
| `Adrenaline — Reload time mult` | float | `0.8` | 0.3..1 | Reload during the window (0.8 = 20% faster). |
| `Adrenaline — ADS time mult` | float | `0.8` | 0.3..1 | ADS during the window (0.8 = 20% faster). |
| `Loud Operator — Enabled` | bool | `true` | — | Drawback: increases the audibility radius of your movement sounds. |
| `Loud Operator — Sound radius mult` | float | `1.3` | 1..2 | Movement-sound radius (1.3 = +30%). |

## Section `4 · Hunter`

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Stalker — Enabled` | bool | `true` | — | Perk: reduces the audibility radius of your movement sounds (stalking). Weaker sibling of the Stealth's Ghost Step. |
| `Stalker — Sound radius mult` | float | `0.8` | 0.1..1 | Movement-sound radius (0.80 = **−20%**; the Stealth gets −30%). |
| `Sharpshooter — Enabled` | bool | `true` | — | Perk: faster ADS. |
| `Sharpshooter — ADS time mult` | float | `0.85` | 0.5..1 | ADS time (0.85 = 15% faster). |
| `Iron Lungs — Enabled` | bool | `true` | — | Perk: holds breath longer. |
| `Iron Lungs — Breath drain mult` | float | `0.667` | 0.2..1 | Hold-breath O2 drain (0.667 → +50% duration). |
| `Steady Arms — Enabled` | bool | `true` | — | Perk: slower arm fatigue while aiming (**requires the stances mod**). |
| `Steady Arms — ADS arm drain mult` | float | `0.65` | 0.2..1 | ADS arm drain (0.65 = 35% slower). |
| `Rooted — Enabled` | bool | `true` | — | Drawback: slower movement while aiming. |
| `Rooted — ADS move speed` | float | `0.85` | 0.5..1 | Move speed while aiming (0.85 = −15%). |

## Section `5 · Stealth`

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Execution — Melee move speed Enabled` | bool | `true` | — | Perk: +move speed with the melee in hand. |
| `Execution — Melee move speed` | float | `1.1` | 1..1.5 | Move speed with melee in hand (1.1 = +10%). |
| `Execution — Melee damage Enabled` | bool | `true` | — | Perk: multiplies knife melee damage. |
| `Execution — Melee damage mult` | float | `5` | 1..10 | Melee damage (5.0 = 5×, execution). |
| `Ghost Step — Enabled` | bool | `true` | — | Perk: reduces the audibility radius of your movement sounds. |
| `Ghost Step — Sound radius mult` | float | `0.7` | 0.1..1 | Movement-sound radius (0.7 = −30%). |
| `Rattled — Enabled` | bool | `true` | — | Drawback: stronger aim-punch when hit. |
| `Rattled — Aim-punch mult` | float | `1.5` | 1..3 | Aim-punch when hit (1.5 = +50%). |

## Section `6 · Scavenger`

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Quick Hands — Enabled` | bool | `true` | — | Perk: search **two containers at once**, from the start. This is the Search skill's **elite** bonus (level 51) granted early — not a new mechanic. No double effect if Search reaches elite. (item 061) |
| `Silent Looter — Enabled` | bool | `true` | — | Perk: quieter interaction/loot sounds. |
| `Silent Looter — Volume mult` | float | `0.4` | 0.1..1 | Interaction/loot volume (0.4 = −60%). |
| `Pack Mule — Enabled` | bool | `true` | — | Perk: +carry limit (floor, does not stack with Strength). |
| `Pack Mule — Carry limit bonus` | float | `0.3` | 0..1 | Carry-limit bonus (0.3 = +30%). |
| `Overladen — Enabled` | bool | `true` | — | Drawback: inertia scales more with weight. |
| `Overladen — Inertia mult` | float | `1.5` | 1..3 | Inertia (1.5 = +50% over the weight-scaled inertia). |

## Section `7 · Tank`

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Bulwark — Enabled` | bool | `true` | — | Perk: reduces incoming health damage. |
| `Bulwark — Damage taken` | float | `0.85` | 0.5..1 | Incoming damage (0.85 = −15%). |
| `Bunker — Enabled` | bool | `true` | — | Perk: with a heavy weapon (LMG/HMG/GL), less recoil and more ergonomics. |
| `Bunker — Heavy weapon recoil mult` | float | `0.85` | 0.5..1 | Heavy-weapon recoil (0.85 = −15%). |
| `Bunker — Heavy weapon ergo mult` | float | `1.15` | 1..1.5 | Heavy-weapon ergonomics (1.15 = +15%). |
| `Tireless Arms — Enabled` | bool | `true` | — | Perk: no arm fatigue with a heavy weapon (**requires the stances mod**). |
| `Tireless Arms — Heavy arm drain mult` | float | `0` | 0..1 | Heavy-weapon arm drain (0 = no drain). |
| `Heavy Frame — Enabled` | bool | `true` | — | Drawback: slower movement (heavy frame). |
| `Heavy Frame — Move speed` | float | `0.9` | 0.5..1 | Move speed (0.9 = −10%). |
| `Heavy Frame — Hunger/thirst drain` | float | `1.3` | 1..2 | Hunger/thirst drain (1.3 = +30% faster). |
| `Pack Mule — Enabled` | bool | `true` | — | Perk: +carry limit (floor, does not stack with Strength). |
| `Pack Mule — Carry limit bonus` | float | `0.3` | 0..1 | Carry-limit bonus (0.3 = +30%). |
| `Loud Operator — Enabled` | bool | `true` | — | Drawback: increases the audibility radius of your movement sounds. |
| `Loud Operator — Sound radius mult` | float | `1.3` | 1..2 | Movement-sound radius (1.3 = +30%). |

## Section `8 · Vanilla Skill Fixes`

> Fixes/activations for vanilla skill mechanics that are inert in EFT. Today: Weapon Mastery (item 058).

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Weapon Mastery — Enabled` | bool | `true` | — | Enables inert masteries: underbarrel XP per shot + per-level bonuses (SMG/LMG/Launcher/Underbarrel). |
| `Underbarrel XP per shot` | float | `0.5` | 0..1 | Underbarrel Launchers XP per GP-25/M203 shot (0.5 = effort parity with SMG). |
| `Recoil bonus per level` | float | `0.004` | 0..0.02 | Recoil reduction per mastery level of the held weapon (0.004 = −0.4%/level). |
| `Ergo bonus per level` | float | `0.002` | 0..0.02 | Ergonomics increase per mastery level (0.002 = +0.2%/level). |

---

> Notes (i18n — item 008):
> - **In-game** text (class name, tooltips, SKILLS button, perk cards) follows the **EFT language** (`"po"` = Portuguese → pt; otherwise English). The **F12** is the exception — it does not follow (see the note at the top).
> - Swapping the client `.dll` requires **restarting the game** (BepInEx plugin). Changing a **default** in code does NOT alter an existing `.cfg` — BepInEx only writes the default when the entry is first created.

## History

| Date | Change |
|---|---|
| 2026-07-10 | Created (EN mirror of PROPRIEDADES.md) alongside the full F12 reorg: 9 sections (one per class, numeric-prefixed, EN), Pack Mule/Loud Operator split per class, bilingual `PT / EN` descriptions, 7 orphan entries removed. |

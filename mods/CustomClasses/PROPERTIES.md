# PROPERTIES.md — CustomClasses (client F12 / BepInEx)

Plugin: `customclasses.mdj.client` ("CustomClasses") — see [modded/Client/Plugin.cs](modded/Client/Plugin.cs) and [modded/Client/PerksConfig.cs](modded/Client/PerksConfig.cs). PT version: [PROPRIEDADES.md](PROPRIEDADES.md).

Properties exposed in the configuration menu (F12 / ConfigurationManager). One is **(Advanced)**: `Perk Diagnostics overlay` (hidden behind the F12 *Advanced* filter).

> **Layout (reorg 2026-07-10):** one **section per class** (perks + drawbacks together). The numeric prefix (`0 ·`, `1 ·`…) forces the F12 order — ConfigurationManager sorts sections alphabetically. Order: system (`0`/`1`), then the 6 classes in roster order (`2`–`7`), then **Naked** (`8`), then global fixes (`9`).
>
> **Order WITHIN a section (property review 2026-08-01):** now follows the **bind order in code** — the `BindOrdered` helper injects a descending `Order`, so the F12 mirrors the code (each perk's `— Enabled` first, the color last). Previously ConfigurationManager sorted alphabetically by key, pushing the `— Enabled` to the middle and splitting the color pair.
>
> **Language:** the F12 is a **BepInEx** plugin, not part of EFT — it does **not** follow the game language (strings are fixed at `Awake`, before EFT loads its locale). So **section/property names stay in English** and the **descriptions (tooltips) are bilingual `PT / EN`** on the same line. This file (EN) and [PROPRIEDADES.md](PROPRIEDADES.md) (PT) document per language.
>
> **Split shared perks (own per-class config):** Pack Mule (Scavenger + Tank) and Loud Operator (Rifleman + Tank) have their **own per-class config** — each section shows its own, with independent values.
>
> **Single-config shared perks:** Light Frame (Hunter + Stealth) and Quick Draw (Hunter + Rifleman + Stealth) have **a single config**, in the Hunter section (`4`), applied to every class that has them. They appear **below** the Hunter color pair because they are bound later in the code (physically in the Scavenger block). Loud Looter (`3` Rifleman) has the same ordering side effect.

---

## Section `0 · General`

| Property | Type | Default | What it does |
|---|---|---|---|
| `Skill XP scaling — Enabled` | bool | `true` | Toggle the per-class skill XP-gain scaling. |
| `Skill multiplier highlight` | bool | `true` | Multiplier highlight on skills (colored border + ±X% arrow + class tooltip). |
| `Class identity on player name` | bool | `true` | Class icon + name (gradient) on the player's name (deploy, character, online list). (item 015) |
| `Class seal (menu + Skills)` | bool | `false` | Separate class seal in the menu and at the top of the Skills screen. (item 012) |
| `SKILLS menu button` | bool | `true` | SKILLS button in the menu (below CHARACTER) that opens the Skills screen. (item 013) |
| `Level-up flavor text` | bool | `true` | Level-up notification `EASILY` (buff) / `FINALLY` (debuff) on skills with a multiplier. (item 014) |
| `Raid-start perks notification` | bool | `true` | Raid-start notification listing the class's perks (green) and drawbacks (red). |
| `Perk Diagnostics overlay` | bool | `false` | **(Advanced)** Live overlay of the properties affected by your player's perks + a log of the SOUND perks applied to peers (coop; `LogOutput.log`). Validation only. |
| `Recoil floor — Enabled` | bool | `true` | **Floor for the COMBINED recoil multiplier** (mastery × perks). They stack multiplicatively and the product had no floor. (balance B15) |
| `Recoil floor — Min combined mult` | float | `0.60` | Minimum recoil as a fraction of the original (0.60 = never below −40% combined). Range 0.3..1. (balance B15) |

## Section `1 · Interface & Position`

> Class-identity UI offsets (px). `Class seal — X/Y offset` are **sliders** that apply in real time (with the Skills screen open).

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Class seal — X offset` | float | `0` | −1000..1000 | Class seal (Skills screen) — horizontal offset from center. |
| `Class seal — Y offset` | float | `-20` | −1000..1000 | Class seal (Skills screen) — vertical offset from top (negative = down). |
| `Class icon size ratio` | float | `1.35` | 0.8..2.5 | Icon size = name font × ratio (keeps the icon:font proportion across screens). |
| `Deploy name scale` | float | `1.2` | 1.0..4.0 | Scale of the player icon+name on the deploy/loading screen (icon and name grow together). |
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
| `Rapid Care — Enabled` | bool | `true` | — | Perk (**072**): faster heals/stabilizations — **effect AND animation** together. |
| `Rapid Care — Use time mult` | float | `0.75` | 0.3..1 | Medical item use time (0.75 = 25% faster). Does **not** apply to the surgery kit (see Swift Surgeon). |
| `Swift Surgeon — Enabled` | bool | `true` | — | Perk (**072**): much faster surgery (CMS/Surv12). |
| `Swift Surgeon — Surgery time mult` | float | `0.75` | 0.3..1 | Surgery time (0.75 = 25% faster). The player's Surgery skill **still** stacks on top. |
| `Restorative Surgery — Enabled` | bool | `true` | — | Perk (**076**): surgery restores the limb to ~80% of max HP (vanilla: CMS 25–45%, Surv12 60–72%). Applies to own surgery + ally via ICM. |
| `Restorative Surgery — Restored max HP` | float | `0.90` | 0..1 | **Floor** of the retained max-HP fraction (0.80 = 80%). Never worse than vanilla; the Surgery skill pushes **beyond** this floor. |
| `Unskilled — Enabled` | bool | `true` | — | Drawback (**079**, formerly `Shaky Hands`): more recoil from lack of firearm skill. Now Combat Medic **and** Scavenger, and **ON** by default (was off). |
| `Unskilled — Recoil mult` | float | `1.25` | 1..2 | Recoil (1.25 = +25%). |
| `Override color` | bool | `false` | — | Override this class's name/icon color with 'Class color' (off = server color). (item 067) |
| `Class color` | Color | `#6f9455` | — | Class name/icon color — only when 'Override color' is on; alpha ignored (always opaque). (item 067) |

## Section `3 · Rifleman`

> `Loud Looter` is bound later in the code (Scavenger block) → it appears **below** the color pair.

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Cool Under Fire — Enabled` | bool | `true` | — | Perk: less flinch when hit. |
| `Cool Under Fire — Flinch mult` | float | `0.5` | 0..1 | Camera flinch when hit (0.5 = −50%). |
| `Cool Under Fire — Malfunction chance mult` | float | `0.5` | 0..1 | Weapon malfunction chance (0.5 = −50%, anti-jam). |
| `Adrenaline — Enabled` | bool | `true` | — | Perk: dealing/taking damage opens a window with better recoil/reload/ADS. |
| `Adrenaline — Window (s)` | float | `25` | 5..120 | Window duration (renewed on each new damage). |
| `Adrenaline — Cooldown (s)` | float | `120` | 0..600 | Cooldown after the window before it can re-trigger. |
| `Adrenaline — Recoil mult` | float | `0.7` | 0.3..1 | Recoil during the window (0.7 = −30%). |
| `Adrenaline — Reload time mult` | float | `0.7` | 0.3..1 | Reload during the window (0.7 = 30% faster). |
| `Adrenaline — ADS time mult` | float | `0.7` | 0.3..1 | ADS during the window (0.7 = 30% faster). |
| `Loud Operator — Enabled` | bool | `true` | — | Drawback: increases the audibility radius of your movement sounds. |
| `Loud Operator — Sound radius mult` | float | `1.3` | 1..2 | Movement-sound radius (1.3 = +30%). |
| `Override color` | bool | `false` | — | Override this class's name/icon color with 'Class color' (off = server color). (item 067) |
| `Class color` | Color | `#b0573a` | — | Class name/icon color — only when 'Override color' is on; alpha ignored (always opaque). (item 067) |
| `Loud Looter — Enabled` | bool | `true` | — | Drawback (**079**): **LOUDER** interaction/loot sound (AI hears more; the AI channel needs SAIN). |
| `Loud Looter — Volume mult` | float | `1.3` | 1..2 | Interaction/loot volume (1.3 = +30%). |

## Section `4 · Hunter`

> `Light Frame` and `Quick Draw` are **shared** (Hunter+Stealth and Hunter+Rifleman+Stealth) with **a single config**, here. They are bound later in the code → they appear **below** the color pair.

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Stalker — Enabled` | bool | `true` | — | Perk: reduces the audibility radius of your movement sounds (stalking). Weaker sibling of the Stealth's Ghost Step. |
| `Stalker — Sound radius mult` | float | `0.8` | 0.1..1 | Movement-sound radius (0.80 = **−20%**; the Stealth gets −30%). |
| `Sharpshooter — Enabled` | bool | `true` | — | Perk: faster ADS. |
| `Sharpshooter — ADS time mult` | float | `0.90` | 0.5..1 | ADS time (0.85 = 15% faster). |
| `Iron Lungs — Enabled` | bool | `true` | — | Perk: holds breath longer. |
| `Iron Lungs — Breath drain mult` | float | `0.7` | 0.2..1 | Hold-breath O2 drain (0.7 → +43% duration). |
| `Steady Arms — Enabled` | bool | `true` | — | Perk: slower arm fatigue while aiming (**requires the stances mod**). |
| `Steady Arms — ADS arm drain mult` | float | `0.65` | 0.2..1 | ADS arm drain (0.65 = 35% slower). |
| `Calm Sights — Enabled` | bool | `true` | — | Perk (**072**): less weapon sway. ⚠️ Affects **aim/movement** sway; **breathing** sway is a different system (that one is Iron Lungs). |
| `Calm Sights — Sway mult` | float | `0.7` | 0.3..1 | Weapon sway (0.7 = 30% less). |
| `Rooted — Enabled` | bool | `true` | — | Drawback: slower movement while aiming. |
| `Rooted — ADS move speed` | float | `0.85` | 0.5..1 | Move speed while aiming (0.85 = −15%). |
| `Override color` | bool | `false` | — | Override this class's name/icon color with 'Class color' (off = server color). (item 067) |
| `Class color` | Color | `#c2973f` | — | Class name/icon color — only when 'Override color' is on; alpha ignored (always opaque). (item 067) |
| `Light Frame — Enabled` | bool | `true` | — | Drawback (**079**, Hunter + Stealth): reduced carry limit (light frame — carries less loot). |
| `Light Frame — Carry limit penalty` | float | `-0.10` | −0.5..0 | Carry-limit reduction (−0.20 = −20%). **Negative** value (cap, not floor). |
| `Quick Draw — Enabled` | bool | `true` | — | Perk (**080/087/088**, Hunter + Rifleman + Stealth): faster SWAP to the Holster weapon (put away the previous + draw the holster one). |
| `Quick Draw — Draw-in time mult (phase 3)` | float | `0.90` | 0.3..1 | Phase 3 — time to **DRAW** the holster weapon (0.65 = 35% faster; 1.0 = off). |
| `Quick Draw — Put-away time mult (phase 1)` | float | `0.90` | 0.3..1 | Phase 1 — time to **PUT AWAY** the previous weapon (0.75 = 25% faster; 1.0 = off). |

## Section `5 · Stealth`

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Execution Speed — Enabled` | bool | `true` | — | Perk: +move speed with the melee in hand. |
| `Execution Speed — Move speed mult` | float | `1.1` | 1..1.5 | Move speed with melee in hand (1.1 = +10%). |
| `Execution Melee — Enabled` | bool | `true` | — | Perk: multiplies knife melee damage. |
| `Execution Melee — Damage mult` | float | `5` | 1..10 | Melee damage (3.5x, execution). Was `5` (trivial one-shot). (balance B7) |
| `Ghost Step — Enabled` | bool | `true` | — | Perk: reduces the audibility radius of your movement sounds. |
| `Ghost Step — Sound radius mult` | float | `0.7` | 0.1..1 | Movement-sound radius (0.7 = −30%). |
| `Rattled — Enabled` | bool | `true` | — | Drawback: stronger aim-punch when hit. |
| `Rattled — Aim-punch mult` | float | `1.5` | 1..3 | Aim-punch when hit (1.5 = +50%). |
| `Silent Knife — Enabled` | bool | `true` | — | Perk (**083**): the knife makes no sound (drawing, swinging and hitting are all silent). |
| `Override color` | bool | `false` | — | Override this class's name/icon color with 'Class color' (off = server color). (item 067) |
| `Class color` | Color | `#8b8fa3` | — | Class name/icon color — only when 'Override color' is on; alpha ignored (always opaque). (item 067) |

## Section `6 · Scavenger`

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Quick Hands — Enabled` | bool | `true` | — | Perk: search **two containers at once**, from the start. This is the Search skill's **elite** bonus (level 51) granted early — not a new mechanic. No double effect if Search reaches elite. (item 061) |
| `Silent Looter — Enabled` | bool | `true` | — | Perk: quieter interaction/loot sounds. |
| `Silent Looter — Volume mult` | float | `0.4` | 0.1..1 | Interaction/loot volume (0.4 = −60%). |
| `Pack Mule — Enabled` | bool | `true` | — | Perk: +carry limit (floor, does not stack with Strength). |
| `Pack Mule — Carry limit bonus` | float | `0.3` | 0..1 | Carry-limit bonus (0.3 = +30%). |
| `Hare — Enabled` | bool | `true` | — | Perk (**081**, formerly `Lebre`): +move speed while **NOT** overweight (no overweight/anvil icon). |
| `Hare — Move speed mult` | float | `1.3` | 1..1.5 | Move speed while light (1.3 = +30%). Auto-off when overweight. |
| `Nervous — Enabled` | bool | `true` | — | Drawback (**082**, formerly `Medroso`): shaky hands (tremor) when shot **OR** suppressed (bullet fly-by). |
| `Nervous — Tremor duration (s)` | float | `6` | 1..20 | Tremor duration (seconds). |
| `Nervous — Cooldown (s)` | float | `8` | 0..30 | Cooldown before the tremor can re-trigger. |
| `Nervous — Suppression distance (m)` | float | `4` | 0..20 | Bullet fly-by distance (m) counting as suppression (0 = only when hit). |
| `Override color` | bool | `false` | — | Override this class's name/icon color with 'Class color' (off = server color). (item 067) |
| `Class color` | Color | `#c4ad45` | — | Class name/icon color — only when 'Override color' is on; alpha ignored (always opaque). (item 067) |

## Section `7 · Tank`

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Bulwark — Enabled` | bool | `true` | — | Perk: reduces incoming health damage. |
| `Bulwark — Damage taken` | float | `0.85` | 0.5..1 | Incoming damage (0.85 = −15%). |
| `Bulwark — Require heavy armor` | bool | `true` | — | **CONDITIONAL Bulwark**: only applies while wearing heavy armor (it used to be unconditional — it applied even naked). (balance B6) |
| `Bulwark — Min armor class` | int | `4` | 1..6 | Minimum equipped armor class for Bulwark to apply. (balance B6) |
| `Bunker — Enabled` | bool | `true` | — | Perk: with a heavy weapon (LMG/HMG/GL), less recoil and more ergonomics. |
| `Bunker — Heavy weapon recoil mult` | float | `0.7` | 0.5..1 | Heavy-weapon recoil (0.7 = −30%). |
| `Bunker — Heavy weapon ergo mult` | float | `1.15` | 1..1.5 | Heavy-weapon ergonomics (1.15 = +15%). |
| `Tireless Arms — Enabled` | bool | `true` | — | Perk: very slow arm fatigue with a heavy weapon (**requires the stances mod**). |
| `Tireless Arms — Heavy arm drain mult` | float | `0.5` | 0..1 | Heavy-weapon arm drain (0.5 = 2x slower; 0 = no drain). Was `0.20` → `0.5`. (balance B16) |
| `Heavy Frame — Enabled` | bool | `true` | — | Drawback: slower movement (heavy frame). |
| `Heavy Frame — Move speed` | float | `0.9` | 0.5..1 | Move speed (0.9 = −10%). |
| `Heavy Frame — Hunger/thirst drain` | float | `1.15` | 1..2 | Hunger/thirst drain (1.15 = +15% faster). |
| `Pack Mule — Enabled` | bool | `true` | — | Perk: +carry limit (floor, does not stack with Strength). |
| `Pack Mule — Carry limit bonus` | float | `0.3` | 0..1 | Carry-limit bonus (0.3 = +30%). |
| `Loud Operator — Enabled` | bool | `true` | — | Drawback: increases the audibility radius of your movement sounds. |
| `Loud Operator — Sound radius mult` | float | `1.3` | 1..2 | Movement-sound radius (1.3 = +30%). |
| `Shotgun Reload — Enabled` | bool | `true` | — | Perk (**084**): faster tube-fed (shell-by-shell) shotgun reload. Does not affect detachable-magazine shotguns (Saiga). |
| `Shotgun Reload — Reload time mult` | float | `0.6` | 0.4..1 | Shotgun reload time (0.6 = 40% faster). |
| `Override color` | bool | `false` | — | Override this class's name/icon color with 'Class color' (off = server color). (item 067) |
| `Class color` | Color | `#6b7280` | — | Class name/icon color — only when 'Override color' is on; alpha ignored (always opaque). (item 067) |

## Section `8 · Naked`

> The Naked (Peladão) has no perks — this section exists for its color (item 067) and, later, its merit text (item 068).

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Override color` | bool | `false` | — | Override this class's name/icon color with 'Class color' (off = server color). (item 067) |
| `Class color` | Color | `#c28a60` | — | Class name/icon color — only when 'Override color' is on; alpha ignored (always opaque). (item 067) |

## Section `9 · Vanilla Skill Fixes`

> Fixes/activations for vanilla skill mechanics that are inert in EFT. Today: Weapon Mastery (item 058). The 4 keys are now prefixed `Weapon Mastery — `.

| Property | Type | Default | Range | What it does |
|---|---|---|---|---|
| `Weapon Mastery — Enabled` | bool | `true` | — | Enables inert masteries: underbarrel XP per shot + per-level bonuses (SMG/LMG/Launcher/Underbarrel). |
| `Weapon Mastery — Underbarrel XP per shot` | float | `0.5` | 0..1 | Underbarrel Launchers XP per GP-25/M203 shot (0.5 = effort parity with SMG). |
| `Weapon Mastery — Recoil bonus per level` | float | `0.004` | 0..0.02 | Recoil reduction per mastery level of the held weapon (0.004 = −0.4%/level). |
| `Weapon Mastery — Ergo bonus per level` | float | `0.002` | 0..0.02 | Ergonomics increase per mastery level (0.002 = +0.2%/level). |

---

> Notes (i18n — item 008):
> - **In-game** text (class name, tooltips, SKILLS button, perk cards) follows the **EFT language** (`"po"` = Portuguese → pt; otherwise English). The **F12** is the exception — it does not follow (see the note at the top).
> - Swapping the client `.dll` requires **restarting the game** (BepInEx plugin). Changing a **default** in code does NOT alter an existing `.cfg` — BepInEx only writes the default when the entry is first created. Renaming a **key** (e.g. `Shaky Hands` → `Unskilled`) creates a new entry with the default → the old value is "lost" (resets).

## History

| Date | Change |
|---|---|
| 2026-07-10 | Created (EN mirror of PROPRIEDADES.md) alongside the full F12 reorg: 9 sections (one per class, numeric-prefixed, EN), Pack Mule/Loud Operator split per class, bilingual `PT / EN` descriptions, 7 orphan entries removed. |
| 2026-07-17 | Item 067 — per-class `Override color` + `Class color` (F12) on the 6 classes; new `8 · Naked` section (color only); Vanilla Skill Fixes renumbered `8`→`9`. |
| 2026-08-01 | **Regenerated from the code (items 079–088 + property review).** New perks: Silent Knife (083), Hare (081), Nervous (082), Light Frame (079), Loud Looter (079), Quick Draw (080/087/088, 2 phase sliders), Shotgun Reload (084). Removed: Mobile Surgery, Overladen (→ Hare), Shaky Hands (→ Unskilled). Keys renamed in sections `0`/`1`/`5`/`9`; `Perk Diagnostics overlay` is now **(Advanced)**; intra-section order now follows the code (`BindOrdered`). Defaults adjusted: Rapid Care 0.7→0.75, Swift Surgeon 0.5→0.75, Adrenaline reload/ADS 0.8→0.7, Iron Lungs 0.667→0.7, Bunker recoil 0.85→0.7, Tireless Arms 0.20→0.5, Heavy Frame hunger/thirst 1.3→1.15. |

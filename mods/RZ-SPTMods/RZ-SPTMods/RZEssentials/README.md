# RZEssentials {.tabset}

## 👁️

> ⭐ RZEssentials started as a collection of separate mods, each built around the same philosophy, then merged into a single coherent package. Every system is independent, optional, and disabled by default. Install it as-is and nothing changes in your game.

> ⭐ This mod is built for players who want to craft their own experience and aren't afraid to dig into config files. That said, it's designed to be approachable : the configs are well organized and clearly commented. Most features ship with sensible default, enabling them is often enough to get a solid base without touching anything else.

> ⚠️ All my previous mods are deprecated and replaced by this one. Make sure to remove them before installing RZEssentials, or you'll have conflicts.

*This mod was designed around a flea-market-disabled server. Flea-related features are limited and not a development priority. Most systems have nothing to do with the flea market either way.*

*Some features overlap with SVM.*

---

### 💠 Getting Started

Each feature lives in its own subfolder under `config/`. The folder structure maps directly to the tabs in this description : if you're reading the **Profiles** tab, the relevant files are in `config/profiles/`. No hidden dependencies, no cascading side effects between systems.

Most features have a master toggle at the top of their config file. Flip it to `true` and the feature is live. Leave everything off and the mod does absolutely nothing.

---

### 💠 About

This mod has been extensively tested in multiplayer through a Fika headless server setup, with a small group of 3-4 people playing almost every evening. I can confidently say that 95% of the features work perfectly, at least the ones I personally use. If you run into bugs, don't hesitate to report them.

I built this for myself first and I'm posting it hoping it'll be useful to others.

---

[![Team](https://i.imgur.com/OQa1Bwd.png)](https://i.imgur.com/OQa1Bwd.png)

## 🔍 Item Context

> 📌 Enriches item descriptions and names with contextual information. All sections are independently toggleable. Fully compatible with modded crafts, barters, quests, and hideout requirements, whether added by this mod or any external mod.

> ⚡ **TL;DR** : To enable this feature, open `RZEssentials/config/itemContext/itemContextConfig.json` and set `EnableItemContext` to `true`.

---

**Ammo name enrichment** — Appends color-coded `[dmg:x/pen:x]` stats directly to ammo item names. Colors and thresholds are fully configurable.

**Base price display** — Appends a color-coded `[x₽]` price suffix to item names. Colors and thresholds are fully configurable.

**Description cleanup** — Clears vanilla descriptions for selected categories.

**Crafting Components** — Lists crafting recipes where this item is required as an ingredient.

**Crafting Tools** — Lists crafting recipes where this item is required as a tool.

**Hideout** — Lists hideout construction stages that require this item. Specific areas can be excluded.

**Barters** — Lists barter trades where this item is part of the payment. The loyalty level display is toggleable.

**Quests** — Lists quests that require this item.

**Size** — Shows container capacity and slot efficiency.

**Audio** — Shows headset boost and suppression stats.

**Armor plates** — Shows class and effective durability in the item description, and appends a `[class/eff. durability]` suffix to the item name.

**Key** — Shows a custom description for keys : what the key opens, what loot to expect, and any related quests.

> ⚠️ Streets of Tarkov keys are not covered. There are also very likely errors in the listed keys, I can't be bothered to verify all of them. If you feel like doing a proper pass and want to submit a corrected version, send it my way and I'll include it.

---

[![1](https://i.imgur.com/rHdlmMX.png)](https://i.imgur.com/rHdlmMX.png)

[![2](https://i.imgur.com/w7chykJ.png)](https://i.imgur.com/w7chykJ.png)

[![3](https://i.imgur.com/N6HPEba.png)](https://i.imgur.com/N6HPEba.png)

## 🎨 Item Tiers

> 📌 Colors the background of items in your inventory based on their usefulness. Most categories are left untouched : weapons, armor, and meds are already readable at a glance and don't need it. The coloring is reserved for categories where a quick visual signal actually helps : ammo, armor plates, keys, and a handful of high-value items. The system is intentionally small : 6 tiers, and if something is glowing red, it genuinely means best-in-class.

> ⚡ **TL;DR** : To enable this feature, open `RZEssentials/config/itemTiers/itemTiersConfig.json` and set `EnableItemTiers` to `true`.

---

⚫ **Default** — Unassigned or not worth highlighting.

🔵 **Notable** — Ammo & armor plates : decent option. *- OR -* Other items : valued 30k–60k roubles.

🟣 **Superior** — Ammo & armor plates : good, reliable choice. *- OR -* Other items : valued 60k–90k roubles.

🟡 **Excellent** — Ammo & armor plates : high-end, strong performer. *- OR -* Other items : valued 90k–200k roubles.

🔴 **Elite** — Ammo & armor plates : best-in-class, hard capped at 1 or 2 per category. *- OR -* Other items : valued 200k+ roubles.

⚪ **Situational** — Ammo only : high damage but low pen aka leg meta (white/grey).

🟢 **Quest** — Required for a quest.

---

> 💡 Quest-only items are colored automatically. Quest keys for Customs and Reserve have been manually added on top of that. Other maps are not covered yet, if you want to add them, drop the TPLs in `config/keys.json`.

> 💡 Situational tier colors are intentionally slightly lighter than Default. These rounds are niche enough that new players don't need to pay attention to them, so they must stay visually quiet.

> 💡 Includes several color themes !

[![1](https://i.imgur.com/tI3S0US.png)](https://i.imgur.com/tI3S0US.png)

[![2](https://i.imgur.com/2c9nQGC.jpeg)](https://i.imgur.com/2c9nQGC.jpeg)

---

---

---

#### `itemTiersConfig.json`

Defines the color for each tier name and the global price thresholds used for automatic tiering. Supports multiple color presets via the `Theme` field : set to `dark`, `bright` or `verybright`.

---

#### `categoryRules.json` — Pass 1

Maps item category IDs to tier names. Walks the DB parent chain, so assigning a category also covers all its sub-categories automatically. The full vanilla category hierarchy is included as comments for reference.

By default, no category is assigned a meaningful tier, every root category is mapped to `Default`. This means the file currently serves as a blanket reset, but you can assign any tier to any category if you want to color entire item families at once.

Quest items flagged by Tarkov's `QuestItem` property are automatically assigned the Quest tier at this pass. However, this does not cover quest-required keys, which are not flagged as quest items in the DB : those are handled manually in `config/keys.json`.

---

#### `priceRules.json` — Pass 2

Lists category IDs whose tier should be assigned automatically based on handbook price, using the thresholds defined in `masterConfig.json`. Items below the lowest threshold fall back to `Default`. Uses the same DB category IDs as `categoryRules.json`.

---

#### Override files - Pass 3

`ammo.json`, `armorplates.json`, and any other `*.json` in `config/` map individual item TPLs to tier names with absolute priority. Set `"Enabled": false` to disable a file without deleting it.

The per-TPL overrides are intentionally split across multiple files rather than lumped into one. This makes updates easier to manage : if you've built your own custom config, you can replace or ignore individual files without touching everything else. It also makes it easier to contribute to specific categories. If you know the ammo meta better than I do and want to submit a corrected `ammo.json`, you're welcome to contribute.

---

#### Ammo boxes - Pass 4

Automatically inherits the color of the corresponding loose round. No configuration required.

## ⚙️ Item Stats

> 📌 Lets you change almost any stat on any item in the game : damage, penetration, armor class, durability, use time, stack size, ergonomics, speed penalty, and more. No hardcoded list of supported items or properties : if the stat exists on the item template, you can change it. Changes can be applied to an entire category at once, or to individual items by TPL, with per-item values always taking priority over category values. Also supports resizing container grids : stash size, secure container dimensions, backpack capacity, cases, etc.

---

The mod ships with several pre-configured files as a starting point (all disabled by default) :

- **`containerSizes.json`** — Resizes pockets, stashes, secure containers, storage cases, backpacks, vests and armored rigs.
- **`stackSizes.json`** — Adjusts ammo and currency stack sizes.
- **`medical.json`** — Tweaks use counts on medkits and injectors. Defaults restore vanilla use counts.
- **`gearPenalties.json`** — Tweaks speed, mouse, and ergonomics penalties on armored equipment, vests and backpacks.
- **`keys.json`** — Tweaks use count of mechanical keys and keycards.
- **`itemWeights.json`** — Halves the weight of every item in the game.
- **`faceCovers.json`** — Ensures all skin variants of each ballistic mask share the same armor values as their base version.

All `*.json` files in the folder are loaded and merged automatically. Each file can be disabled independently with `"Enabled": false` without deleting it.

> ⚠️ Grid resizing, known limitation : Grid overrides work correctly for most items (containers, cases, pouches, stash). For backpacks specifically, some have a GridLayoutComponent with a layout baked into a Unity prefab. For those the inventory UI may still display the original layout. The mod BetterBackpacks solves this with a BepInEx client patch. RZEssentials does not include a client component and this will not be fixed.

> ⚠️ Increasing the stack size of non-stackable items is not supported. Tarkov's inventory system is not designed for it, and the results range from balance-breaking to outright broken mechanics. Better that way I guess, it's not Minecraft after all.

> 💡 The one limitation worth knowing : this system works cleanly on flat numeric and boolean properties. Nested structures like attachment filter lists require dedicated code and are not supported, except container grids, which are explicitly handled.

---

Each property supports three operations :
- **`set`** — Sets the value directly. This is the default, `"Op"` can be omitted entirely.
- **`add`** — Adds to the current value.
- **`multiply`** — Multiplies the current value.

```jsonc
// Mechanical keys - set to unlimited uses (0 = unlimited) (Op omitted).
"5c99f98d86f7745c314214b3": {
    "Ops": {
        "MaximumNumberOfUsage": { "Value": 0 }
    }
},

// Keycards - double the number of uses.
"5c164d2286f774194c5e69fa": {
    "Ops": {
        "MaximumNumberOfUsage": { "Op": "multiply", "Value": 2 }
    }
}
```

---

[![1](https://i.imgur.com/fNv6wcc.png)](https://i.imgur.com/fNv6wcc.png)

## 👤 Profiles

> 📌 Define custom profile editions that appear in the SPT launcher at character creation, each with their own starting items, trader standings and more.

---

### 💠 How it works

Registers your custom profiles into SPT's profile template system at server startup. When a player creates a new character in the launcher, your editions appear alongside (or instead of) the vanilla SPT ones.

Each profile is defined by a separate `.json` file in the `profiles/templates/` folder. All files are loaded automatically, just drop and go.

---

### 💠 Main Config ( *profiles/profilesConfig.json* )

- **`EnabledBaseProfiles`** — List of vanilla SPT profiles to keep visible in the launcher alongside your custom ones.
- **`ExaminedCategoryBlacklist`** — Excludes all items belonging to the listed categories.
- **`ExaminedBlacklist`** — Excludes specific items by TPL.

> ⚠️ Removing the Standard SPT profile from EnabledBaseProfiles has been known to cause issues in certain edge cases.

---

### 💠 Profile Files

#### Identity

- **`Enabled`** — Whether this profile is registered and visible in the launcher.
- **`BaseProfile`** — Vanilla SPT profile to clone as a base (same indices as `EnabledBaseProfiles`).
- **`Name`** — Profile name shown in the launcher.****
- **`Description`** — Profile description shown in the launcher.

#### Progression

- **`AllItemsExamined`** — Examine all handbook items on character creation. Respects `ExaminedCategoryBlacklist` and `ExaminedBlacklist`.
- **`MaxLevel`** — Start at max level.
- **`StartingLevel`** — Start at a specific level (1 to max). Ignored if `MaxLevel` is true. `null` = leave unchanged from base profile.
- **`StartingPrestigeLevel`** — Starting prestige level (1–4). `null` = no prestige applied.
- **`MaxSkills`** — Max out all skills.
- **`SkillOverrides`** — Override specific skills individually (values 0–51). Ignored if `MaxSkills` is true.

#### Inventory

- **`ClearEquipment`** — Wipe equipped items from the base profile.
- **`ClearStash`** — Wipe stash items from the base profile before injecting additional items.
- **`SecureContainer`** — Override the starting secure container.
- **`AdditionalStartingItems`** — Items injected into the stash on character creation. If `ClearStash` is false, they are added on top of whatever the base profile already contains. For items with required slots (weapons, armor...), missing children are resolved automatically from the database.

#### Traders

- **`TradersLoyalty`** — Per-trader standing and sales sum, keyed by trader ID.

#### Hideout

- **`HideoutStartingLevels`** — Starting level for each hideout area.

---

[![Profiles](https://i.imgur.com/46GzkXV.png)](https://i.imgur.com/46GzkXV.png)

## ⚔️ Raids

> 📌 Patches applied at the raid level.

---

### 💠 Misc raid settings ( *raids/raidsConfig.json* )

**Raid times** — Override raid duration per map. Only maps explicitly listed are affected.

**No run-through** — Removes the time and XP requirements for a raid to count as a survival.

**Remove raid restrictions** — Removes in-raid item carry restrictions (GP coin limits, etc.).

**Free special extracts** — Removes requirements on special extractions (car extract payment, co-op extract partner, etc.). Train and Saferoom extracts are left untouched.

**Magazine Speed Multiplier** — Magazine load/unload speed multipliers.

---

### 💠 Weather ( *raids/weatherConfig.json* )

Controls season selection and weather conditions per raid. Both systems are independent and can be enabled separately.

#### Season

A season is drawn randomly at the start of each raid using configurable weights. Set a season's weight to 0 to exclude it entirely.

#### Weather Presets

Defines named weather presets with weighted random selection. Each preset controls rain, fog, cloud cover, and wind independently. Any field left as null is left to SPT to decide.

**FixedForRaid: true** — One preset is drawn per raid, conditions stay consistent from start to finish.

**FixedForRaid: false** — One preset is drawn per weather period (~15–30 min), producing vanilla-style variation within the raid.

## 🏪 Trader Settings

> 📌 The largest system in the mod. Controls everything players see at traders : what's for sale, at what price, in what quantities, what traders will buy back, and more. The three main features that control trader stock are **Default Trades**, **Routed Trades**, and **Manual Trades**. They are fully independent and can run in any combination.

---

### 💠 Default Trades ( *traders/defaultTradesConfig.json* )

Patches vanilla trader assorts without replacing them. Useful for cleaning up barter schemes, applying price multipliers, or blacklisting specific items, while keeping the rest of the vanilla stock intact. Setting `ClearAllDefaultTrades` to true wipes all non-Fence assorts entirely as a clean slate for the other systems.

---

### 💠 Routed Trades ( *traders/routedTradesConfig.json* )

Rebuilds trader assorts from scratch based on the handbook. Every item gets automatically assigned to a trader according to category routes you define in config. The routing follows the item categories hierarchy : assign a route to a root category and every sub-category inherits it automatically.

Supports routing only modded items, only vanilla items, or everything. Prices are derived from handbook values.

---

### 💠 Manual Trades ( *traders/manualTradesConfig.json* )

Define specific offers for specific traders with full control over every parameter : item, price, currency, barter, stack size, loyalty level, durability, and attachments. Completely independent from the other two systems. Required children (armor plates, etc.) are resolved automatically from the database.

---

### 💠 Fence ( *traders/fenceConfig.json* )

Fence is handled separately from other traders internally. Controls the native SPT item pool generation (counts, price multipliers) and a custom weighted pool of offers that are injected directly into Fence's assort on every refresh.

---

### 💠 Buyback ( *traders/buybackConfig.json* )

Controls what each trader will accept from the player. Four modes per trader : leave vanilla behavior untouched, disable buyback entirely, restrict to specific handbook categories, or accept everything except a blacklist.

---

### 💠 Supply ( *traders/supplyConfig.json* )

Controls trader restock timers and stock amounts per restock cycle. Timers are clamped immediately on server start if the configured interval is shorter than the current one. Stock multipliers can be applied per trader, per handbook category, or both simultaneously.

## 👩 Custom Trader

> 📌 Registers a new trader (Sable). Fully configurable, sells all keys in the game by default.

---

To enable her, open *config/traders/customTraderConfig.json* and set **EnableCustomTrader** to true.

Stock is configured through the same systems as any other trader : **Routed Trades** for automatic category-based stock, and **Manual Trades** for specific hand-crafted offers. Both are covered in the **Traders** tab. In short :

- Open `routedTradesConfig.json`, find the Sable entry under `CategoryRoutes`, and enable the categories you want her to sell.
- Add specific offers in `manualTradesConfig.json` under her trader ID for anything that needs custom pricing, barter schemes, or durability.

---

[![1](https://i.imgur.com/q9gWEEy.png)](https://i.imgur.com/q9gWEEy.png)

[![2](https://i.imgur.com/4U3mHjC.png)](https://i.imgur.com/4U3mHjC.png)

[![3](https://i.imgur.com/rD7hSeA.jpeg)](https://i.imgur.com/rD7hSeA.jpeg)

## 📈 Economy

> 📌 Controls the economy systems that aren't directly tied to trader assorts.

---

### 💠 Handbook Prices ( *economy/handbookPricesConfig.json* )

Override the handbook price of any item by TPL. Handbook prices are the foundation of several SPT systems : routed trade prices, trader buyback prices, and more all derive from them. Changing a price here has broad downstream effects.

### 💠 Flea Market ( *economy/fleaMarketConfig.json* )

Controls dynamic flea market offer generation. Trader flea offers are not affected. Supports disabling all dynamic offers entirely, or selectively forcing specific items in or out of the dynamic pool.

### 💠 Insurance ( *economy/insurance.json* )

Disable insurance globally, or selectively by handbook category and individual TPL. Categories and TPL blacklists are cumulative.

## 🏠 Hideout

### 💠 Hideout Areas ( *hideout/hideoutAreasConfig.json* )

Full control over hideout area requirements, construction times, and the bitcoin farm. Each area is configured independently. Each requirement type (items, areas, skills, trader loyalty) is controlled by its own toggle : enabling a toggle clears the vanilla requirements of that type and injects your own. Leaving a toggle off leaves vanilla requirements completely untouched.

Ships with a default config that matches vanilla exactly, enabling the feature without changing anything produces zero difference in-game.

---

### 💠 Hideout Bonuses ( *hideout/hideoutBonusesConfig.json* )

Overrides the bonus values granted by hideout areas at each stage.

The file ships with all vanilla values pre-filled as a ready-to-edit baseline. Enabling it without changing anything should produces zero difference in-game.

**What you can safely do**

- Change `Value` on any existing entry : this is the intended use case.
- Set a bonus `Value` to `0` to effectively disable it without removing the entry.
- Set `IsVisible` to `false` to hide a bonus from the UI without disabling it.

> ⚠️ This is exactly the kind of section where you want to go slow. Make one change at a time, launch the game, confirm it works, then make the next one.

### 💠 Hideout Misc ( *hideout/hideoutMiscConfig.json* )

Smaller settings that apply globally across the hideout.

**RequireFoundInRaid** — Forces or removes the Found In Raid requirement on every item requirement across all areas and all stages.

**UnlockHideoutCustomizations** — Unlocks hideout customization items by category. Supported categories : Wall, Floor, Ceiling, Light, MannequinPose, ShootingRangeMark.

---

### 💠 Crafting ( *hideout/craftingConfig.json* )

Define custom crafting recipes per hideout area, and optionally wipe all existing recipes for specific areas before injecting your own. Recipes support area level requirements, production time, fuel requirements, and flexible input lists of items, tools, and area requirements.

Ships with a default config that matches vanilla exactly.

> ⚠️Modifying recipes on a server where crafts are already in progress is generally a bad idea, even if the changes don't affect those specific crafts. Finish or cancel all active crafts before making changes.

## 💰 Loot

[![Loot](https://i.imgur.com/EalCJHR.png)](https://i.imgur.com/EalCJHR.png)

> 📌 Injects items directly into bot pockets after generation. Supports bosses, boss followers, PMCs, and Scavs, each with their own independent loot pool configurable separately.
>
> Why not just edit the loot pool ?
>
> SPT's native loot system assigns weights to items in a pool and rolls randomly when generating a bot. The higher the weight, the more likely an item is to appear, but it's never guaranteed.
This mod sidesteps that entirely by injecting items directly into bot pockets after generation, so you get exactly what you configured every time.

---

### ⚙️ How it works

The mod runs after the bot is fully generated and injects items in two passes:

1. **Free slots first** — empty pocket slots are filled first.
2. **Overwrite SPT items** — if all slots are full, SPT-generated items are replaced.

This guarantees your configured items always end up in the pockets, regardless of what SPT put there.

> ⚠️ Currently covers pocket injection only. Backpack and vest support may come later, but it's not a priority right now.

> ⚠️ Only **1x1 items** are supported. There are no safeguards but anything larger will most likely cause issues.

---

### 🔧 Configuration

#### Roles

Bot roles are fully configurable directly in the config file via `BossRoles` and `FollowerRoles`. Any role listed in `BossRoles` gets boss loot, any role in `FollowerRoles` gets follower loot. The Goon followers (Big Pipe, Birdeye) are in `BossRoles` by default.

#### Boss config

Bosses support two modes controlled by `UseGlobalConfig`:

- **`true`** — one config applies to all bosses
- **`false`** — each boss has its own pocket loot list via `PerBoss`

If `UseGlobalConfig` is false and a boss has no `PerBoss` entry, it falls back to `Global` automatically.

#### Per-item config

```jsonc
{ "Tpl": "59faff1d86f7746c51718c9c", "Chance": 100, "MinStack": 1, "MaxStack": 1 }
```

- `Chance` — 0 to 100. 100 = guaranteed, 0 = never drops.
- `MinStack` / `MaxStack` — Stack size range. A random value between the two is picked on each injection. Defaults to 1/1 if not specified. Automatically clamped to the item's max stack size.

Items are processed **in order, top to bottom**. A failed chance roll does **not** consume a slot, the next item in the list gets a chance to fill it. Put your highest priority items first.

#### Overwrite priority

When all pocket slots are full and the mod needs to overwrite SPT items, it always targets the **cheapest item first** based on handbook price. This preserves the most valuable SPT loot as much as possible.

#### Overwrite blacklist

Items protected from being overwritten when the mod needs to make room. Two ways to blacklist:

- **`Tpls`** — Exact TPL match.
- **`Categories`** — Any item whose parent chain contains the category ID is protected.

> 💡 Blacklisted items are never touched, regardless of their handbook price.

---

### 🔀 How the sequential logic works

Say you have 4 pocket slots and configure 3 items, with SPT already filling 3 of them:
```jsonc
"Pockets": [
  { "Tpl": "Bitcoin",    "Chance": 100 },
  { "Tpl": "GP Coin",    "Chance": 50  },
  { "Tpl": "Lega Medal", "Chance": 30  }
]
```

1. **Bitcoin** — rolls 100%, passes. One free slot available → injected. ✓
2. **GP Coin** — rolls 50%, fails. Slot is **not consumed** → next item gets a shot.
3. **Lega Medal** — rolls 30%, passes. No free slots left → overwrites an SPT item. ✓

A failed roll never wastes a slot. Only a successful roll claims one.

> 💡 The injection loop stops as soon as all slots are claimed, either by your configured items or by SPT items that are blacklisted. If a bot has 4 pocket slots and 4 blacklisted SPT items, none of your configured items will be injected.

## 🪖  Character

> 📌 Patches applied to character configuration.

---

### 💠 Health ( *character/healthConfig.json* )

**BaseEnergyRegeneration / BaseHydrationRegeneration** — Base regeneration rates for energy and hydration out of raid.

**Body parts** — Set the maximum HP for each body part independently. Any field left as `null` is left unchanged from the base profile.

**ApplyToExistingSaves** — When enabled, HP overrides are also written to existing save files on every server start. When disabled, changes only apply to new characters created after activation.

> ⚠️ Disabling this option later will not restore the previous HP values in existing saves.

---

### 💠 Weight Limits ( *character/weightLimitsConfig.json* )

Overrides the stamina weight thresholds that control when penalties start applying.

- **BaseOverweightLimits** — General threshold where stamina penalties begin.
- **WalkOverweightLimits** — Threshold where walk speed starts being reduced.
- **SprintOverweightLimits** — Threshold where sprinting becomes impossible.
- **WalkSpeedOverweightLimits** — Threshold where walk speed is fully capped.

**X** is the weight where the penalty starts, **Y** is where it reaches its maximum.

---

### 💠 Skills ( *character/skillsConfig.json* )

Overrides the progression parameters of any skill in the game. Each entry targets a named skill block (e.g. `Crafting`, `Endurance`, `HideoutManagement`) and sets individual properties within it.

The mod ships with a full pre-configured file covering every skill. The vanilla values are preserved by default, enable it with "Enabled": true and adjust from there.

---

### 💠 Equipment Conflicts ( *character/equipmentConflictsConfig.json* )

Removes the blocking relationships between equipment slots, things like headwear preventing face covers.

The default config already ships with sensible settings : most slot combinations are unlocked, but broken ones (full helmet + face mask simultaneously, etc...) are kept blocked.

If you don't want to dig into the details, just set `Enabled` to `true` at the top of the config and you're done. The full breakdown of what each setting does is documented inside the config file itself.

> ⚠️ Not every possible equipment combination has been tested. If you run into a combo that should work but doesn't, or one that shouldn't work but does, feel free to report it.

---

### 💠 Special Slots ( *character/specialSlotsConfig.json* )

Controls which items can go into the 3 special slots on all pocket templates. Each entry has a `Tpl` and an `Enabled` toggle :

- `true` — Adds the item to the special slot filter, and sets it as non-discardable and non-insurable.
- `false` — Removes the item from the special slot filter.

The config ships with a handful of pre-configured entries. Toggle them on or off as needed, or add your own TPLs.

---

### 💠 Holster Slot ( *character/holsterSlotConfig.json* )

Expands the holster slot to accept additional weapon types. Each category is toggled independently, and sub-categories are included automatically. Individual TPLs can also be added directly for items outside any enabled category.

**Categories** — One toggle per weapon type.

**AllowedTpls** — Individual item TPLs added on top of whatever categories are enabled.

Both lists are combined : an item is allowed if it matches either one.

## 🖥️ UI

### 💠 Locale Overrides ( *ui/localesConfig.json* )

**Force English items** — Replaces all item names, short names, and descriptions with their English equivalents for non-English clients. A great way to teach your friends English without them realizing it xD

**Force English maps** — Same, for map names and descriptions.

**Force English hideout** — Same, for hideout area names and descriptions.

**Force English traders** — Same, for trader locale strings.

**Locale overrides** — Override any locale key with a custom string, applied to all languages including English. Keys can be anything found in `SPT_Data\database\locales\global\en.json`.

**Trader locale overrides** — Override trader names, nicknames, locations, and descriptions for all languages including English. Empty fields are ignored.

> 💡 Locale overrides take priority over Force English options and are applied last.

---

### 💠 Avatar Overrides ( *ui/avatarsConfig.json* )

Remap trader portrait images. Image files are placed in the `db/` folder of the mod.

> 💡 After changing portraits, press **Clean Temp Files** in the SPT launcher before restarting. Without this step portraits won't update.

---

### 💠 Client Config ( *ui/clientConfig.json* )

Controls various client-side UI behaviours. All options apply to every player on the server.

**Skip side selection screen** — Skips the PMC/Scav selection screen before raids.

**Skip insurance screen** — Skips the insurance selection screen before raids.

**Skip raid settings screen** — Skips the raid settings screen before raids.

**Skip experience screen** — Skips the experience summary screen after raids.

**Category sort / Item sort / Alphabetical sort** — Controls trader inventory sorting behaviour. Category order and item order can be defined explicitly via their respective lists.

> 💡 Client config is intentionally managed server-side rather than through a BepInEx F12 menu. The mod is designed around Fika : regular players have no business touching F12 config, and keeping everything in files makes it easier to push changes without asking everyone to reinstall anything. The only tradeoff is that a game restart is required after any change, which is fine.

> ⚠️ Category sort was put together quickly and may not be practical for every setup — it's worth testing before committing to it. Item sort and alphabetical sort work reliably.

[![Skip](https://i.imgur.com/pac4sQ3.gif)](https://i.imgur.com/pac4sQ3.gif)

## 🔧 Misc Settings

### 💠 Misc Settings ( *miscSettings/miscSettingsConfig.json* )

A collection of smaller quality of life patches.

**Free post-raid heal** — Removes the healing cost between raids.

**Unlock all outfits** — Unlocks all Ragman outfits without requiring quests or purchases.

**Disable PMC mail responses** — Suppresses the automatic chat messages from PMCs after kills.

**Disable starting gifts** — Disables the starting gifts sent by traders on character creation.

---

### 💠 Repair ( *miscSettings/repairConfig.json* )

**No repair degradation** — Removes max durability loss when repairing. Items are restored to their template maximum rather than their degraded maximum, completely eliminating the durability penalty that accumulates over successive repairs.

**Repair kit buffs** — Controls the bonus effects applied when using repair kits. Each equipment type (armor, vest, headwear, weapon) is configured independently. Two rarity tiers (Common/Rare) each support their own bonus types, value ranges, and active durability window.

For armor, vest and headwear the available bonus type is `DamageReduction`. For weapons : `WeaponSpread`, `MalfunctionProtections`, and `WeaponDamage`.

- `RarityWeight` — Relative probability of rolling Common vs Rare.
- `BonusTypeWeight` — Relative probability of each bonus type being selected.
- `ValuesMinMax` — The min/max range of the bonus multiplier value.
- `ActiveDurabilityPercentMinMax` — The durability window (% of max) within which the bonus is active.

> 💡 This section hasn't been thoroughly tested yet.

---

### 💠 Log Config ( *miscSettings/logConfig.json* )

Controls which log channels are active. All channels are enabled by default. Once your config is dialed in, you can disable channels here to reduce console spam.

## 📝 Notes

### 💠 The `extras/` folder

The `extras/` folder contains two reference files dumped from the SPT database :

- **`itemList.json`** — All item templates.
- **`categoryList.json`** — The full category hierarchy.

These are useful any time you need to look up a specific item TPL or category ID for use in config files. In many cases this is faster than using [ItemFinder](https://db.sp-tarkov.com/), since you can just `Ctrl+F` directly in the file and copy paste the tpl and the handbook price.

The dumps also include the full contents of **WTT - Content Backport**, so modded items and categories from that pack are covered as well.

---

> ⚠️ Use Notepad++ or VSCode to edit config files. The default Windows Notepad won't warn you about syntax errors.

> 💡 My personal config is available on the repository if you want more concrete examples of what the mod can do.

## 🐱

![cat](https://media4.giphy.com/media/v1.Y2lkPTZjMDliOTUyejBvc3JvcDFjeGVnemRlbHpjaWJ2dGdsNWIzNTlibmtkbXc2bHN4bCZlcD12MV9naWZzX3NlYXJjaCZjdD1n/qZgHBlenHa1zKqy6Zn/giphy.gif)

{.endtabset}

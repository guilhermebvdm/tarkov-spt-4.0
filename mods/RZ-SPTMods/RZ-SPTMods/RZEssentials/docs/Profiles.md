- Added `DisableStartingGifts` option to suppress Prapor and Mechanic starting gifts on new profiles.
- Fixed `MaxSkills` incorrectly setting bot-only skills (BotReload, BotSound) on player profiles. (omg)
- Fixed `MaxSkills` and `SkillOverrides` being reset to 0 when `StartingPrestigeLevel` is set. (omggggg)
- This mod is cursed and I wish anyone but me would have done this before.

> Define custom profile editions that appear in the SPT launcher at character creation, each with their own starting items, trader standings and more.

*This is part of my personal suite of mods that I use to run my server.*

*All in all a pretty boring mod, but the basics deserve a clean foundation, so here it is.*

---

---

---

## ⚫ How it works

The mod runs at server startup and registers your custom profiles into SPT's profile template system. When a player creates a new character in the launcher, your editions appear alongside (or instead of) the vanilla SPT ones.

Each profile is defined by a separate `.json` file dropped in the `profiles/` folder. All files are loaded automatically, just drop and go.

Global settings (unlocks, blacklists, enabled base profiles) are controlled via `masterConfig.json`.

---

---

---

## ⚫ masterConfig.json

*Applied at server start, globally for all profiles.*

- **`EnabledBaseProfiles`** — List of vanilla SPT profiles to keep visible in the launcher alongside your custom ones.
- **`UnlockAllOutfits`** — Unlocks all Ragman outfits.
- **`UnlockHideoutCustomizations`** — Unlocks hideout customization items by category. Set a category to `false` to leave it locked.
- **`UnlockJaeger`** — Unlocks Jaeger by default.
- **`UnlockRef`** — Unlocks Ref by default.
- **`ExaminedCategoryBlacklist`** — Excludes all items belonging to the listed categories.
- **`ExaminedBlacklist`** — Excludes specific items by TPL.

---

---

---

## ⚫ Profile files

### Identity

- **`Enabled`** — Whether this profile is registered and visible in the launcher.
- **`BaseProfile`** — Vanilla SPT profile to clone as a base (same indices as `EnabledBaseProfiles`).
- **`Name`** — Profile name shown in the launcher.
- **`Description`** — Profile description shown in the launcher.

---

### Progression

- **`AllItemsExamined`** — Examine all handbook items on character creation. Respects `ExaminedCategoryBlacklist` and `ExaminedBlacklist`.
- **`MaxLevel`** — Start at max level.
- **`StartingLevel`** — Start at a specific level (1 to max). Ignored if `MaxLevel` is true. `null` = leave unchanged from base profile.
- **`StartingPrestigeLevel`** — Starting prestige level (1–4). `null` = no prestige applied.
- **`MaxSkills`** — Max out all skills.
- **`SkillOverrides`** — Override specific skills individually (values 0–51). Ignored if `MaxSkills` is true.

---

### Inventory

- **`ClearEquipment`** — Wipe equipped items from the base profile.
- **`ClearStash`** — Wipe stash items from the base profile before injecting additional items.
- **`SecureContainer`** — Override the starting secure container.
- **`AdditionalStartingItems`** — Items injected into the stash on character creation. If `ClearStash` is false, they are added on top of whatever the base profile already contains. For items with required slots (weapons, armor...), missing children are resolved automatically from the database.

---

### Traders

- **`TradersLoyalty`** — Per-trader standing and sales sum, keyed by trader ID.

---

### Hideout

- **`HideoutStartingLevels`** — Starting level for each hideout area.

> 💡 `HideoutStartingLevels` entries are optional : delete any area you don't want to override.

---

---

---

*If you made it this far without falling asleep, you earned a cat picture.*

![cat](https://media1.popsugar-assets.com/files/thumbor/lWUURoKYv9B2XqBJ8WGzUsllkLs=/0x0:2003x2003/fit-in/1584x1584/filters:format_auto():upscale()/2019/09/23/864/n/1922243/74b4f2275d89208a0f2ad4.00493766_.jpg)

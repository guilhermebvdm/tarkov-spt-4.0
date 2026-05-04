# RZCustomEconomy {.tabset}

## 🔎 Overview

> Full control over SPT's economy through config files. Every feature is independently toggleable and fully configurable : pick what you need, ignore the rest. Whether you want to tweak a few trader stocks or rebuild the economy from scratch, it's all just config.

---

---

---

### Getting Started

> **⚠️ By default, every feature in this mod is disabled.** Install it as-is and nothing changes in your game - it's a clean slate.

Each feature has a dedicated on/off switch in `masterConfig.json`. Flip it to `true` and the feature is live. That's it. Every switch maps directly to one of the tabs in this description, which contains the full documentation and config reference for that feature.

- 🏷️ **`EnableDefaultTrades`** — 🟢 On by default → see the **Default Trades** tab
- 🔀 **`EnableRoutedTrades`** — 🔴 Off by default → see the **Routed Trades** tab
- 🛒 **`EnableManualTrades`** — 🔴 Off by default → see the **Manual Trades** tab
- 👤 **`EnableFenceConfig`** — 🔴 Off by default → see the **Fence** tab
- 🏪 **`EnableBuybackConfig`** — 🔴 Off by default → see the **Buyback** tab
- 📦 **`EnableSupplyConfig`** — 🔴 Off by default → see the **Supply** tab
- 📝 **`EnableHandbookPricesConfig`** — 🔴 Off by default → see the **Handbook Prices** tab
- 📈 **`EnableFleaMarketConfig`** — 🔴 Off by default → see the **Flea Market** tab
- 🛡️ **`EnableInsuranceConfig`** — 🔴 Off by default → see the **Insurance** tab
- 🏠 **`EnableHideoutConfig`** — 🔴 Off by default → see the **Hideout** tab
- ⚗️ **`EnableCraftingConfig`** — 🔴 Off by default → see the **Crafting** tab

Pick what you need, leave the rest off. Features are fully independent - run any combination you want.

---

The three main features that control what players see at traders are **Default Trades**, **Routed Trades** and **Manual Trades**. It's worth understanding the difference before diving in.

🏷️ **EnableDefaultTrades** keeps vanilla trader assorts intact and applies patches on top of them, for instance replacing barter requirements with a straight cash price. Setting it to false wipes all vanilla assorts entirely and ignores *defaultTradesConfig.json*.

🔀 **EnableRoutedTrades** takes a completely different approach : it rebuilds trader offers from scratch based on the handbook. Every item gets automatically assigned to a trader according to category rules you define. If set to false, *routedTradesConfig.json* will be entirely ignored.

🛒 **EnableManualTrades** lets you define specific offers for specific traders with full control over every parameter : item, price, currency, barter, durability, attachments. Completely independent from the other two. If set to false, *manualTradesConfig.json* will be entirely ignored.

---

---

---

> ⚠️ Use Notepad++ or VSCode to edit your config files, the default Windows Notepad will mess up your formatting and won't warn you about syntax errors. ----->
> [Download Notepad++](https://notepad-plus-plus.org/downloads/v8.9.2/) - [Download VSCode](https://code.visualstudio.com/)

> 💡 The mod ships with an `extras` folder containing `category_dump.json` and `item_dump.json` : direct dumps of the database. If you're just looking for a specific item or category ID, these are significantly more conveninent to copy&paste from than [ItemFinder](https://db.sp-tarkov.com/) : open the file, Ctrl+F, done. `item_dump.json` includes all items added by WTT - Content Backport.

## 🏷️ Default Trades

> Patches vanilla trader assorts without replacing them entirely. Useful when you want to keep the default stock but clean up how trades work.

---

---

---

#### ⚙️ Configuration : `defaultTradesConfig.json`

**`Blacklist`** — List of TPLs to remove from all trader assorts unconditionally. Applied before any other patch.

**`PriceMultipliers`** — Per-trader cash price multiplier. Only affects cash-only offers (rub/usd/eur) — barter schemes are ignored. Keyed by trader ID.

**`NoBarterTraders`** — Replaces barter schemes with a straight cash payment at handbook price, per trader. Each trader entry supports:

- **`Enabled`** — whether this trader's barters are converted
- **`Currency`** — currency to use for the converted price : `"rub"` | `"eur"` | `"usd"`. EUR and USD amounts are calculated from the handbook rouble price using the in-game exchange rate.
- **`ExcludedBarterTpls`** — list of item TPLs to exclude. If every item in a barter scheme belongs to this list, the scheme is left untouched. Useful to preserve GP coin or lega medal trades selectively.

## 🔀 Routed Trades

> Reads every item in the handbook at runtime and automatically assigns it to a trader based on a category map you define in config. The routing system follows the handbook's category hierarchy : define a route for "Weapons" and every sub-category inherits it automatically. Ships with a full pre-built map covering all vanilla item categories.

**For modded content** — `RouteModdedItemsOnly` detects items not present in the vanilla handbook and routes only those to traders. Every item added by every mod you have installed shows up at a trader immediately, at handbook price. The vanilla_items.json file in the mod's root folder is used as the reference to determine what's vanilla and what isn't.

**For visibility** — `ForceRouteAll` bypasses all filters and routes every item in the handbook. Combined with `AllItemsExamined`, nothing is hidden.

**For economy overhauls** — instead of hand-editing dozens of assort json files, you maintain a single config that describes the intent and the mod handles the rest.

---

---

---

#### ⚙️ Configuration : `routedTradesConfig.json`

- **`ForceRouteAll`** — Route every handbook item regardless of category routes and blacklists.
- **`RouteModdedItemsOnly`** — Only route items not present in vanilla_items.json (modded items). Mutually exclusive with RouteVanillaItemsOnly.
- **`RouteVanillaItemsOnly`** — Only route items present in vanilla_items.json (vanilla items). Mutually exclusive with RouteModdedItemsOnly.
- **`FallbackTrader`** — When ForceRouteAll is true, items with no matching category route go here. Always priced in roubles.
- **`TraderCurrencies`** — Currency used for routed offers, per trader.
- **`Blacklist`** — Items that are never routed to any trader, regardless of category routes.

## 🛒 Manual Trades

> Completely independent from auto-routing. Define specific trades for specific traders with full control over every parameter.

---

---

---

#### ⚙️ Configuration : `manualTradesConfig.json`

Each offer supports:
- Rouble price or barter payment (or both combined)
- Stack count (`-1` for unlimited)
- Loyalty level requirement
- Durability for weapons and armor
- Manual children (explicit attachments)
- Auto-resolved required children — for items with required slots (armor plates etc.), the mod automatically injects the correct child items by reading the template from the DB, recursively.

## 👤 Fence

> Controls Fence's item generation and offer injection. Fence is handled differently from other traders internally : this feature is independent from `EnableDefaultTrades`, `EnableRoutedTrades`, and `EnableManualTrades`.

#### ⚙️ Configuration : `fenceConfig.json`

The config is split into three sub-sections.

---

---

---

### ⚫ *Default Item Pool*

SPT's native Fence generation system. On each refresh, SPT randomly picks items from a built-in pool and generates offers from them.

- **`EnableDefaultItemPool`** — Master switch for default item pool. If false, zeroes all native generation counts entirely and also disables Default Item Pool Additions, regardless of `EnableDefaultItemPoolAdditions`.
- **`AssortSize`** — Number of regular items (ammo, meds...) per refresh.
- **`WeaponPresetMinMax`** — Number of preconfigured weapons with mods per refresh.
- **`EquipmentPresetMinMax`** — Number of preconfigured armor rigs per refresh.
- **`ItemPriceMult`** — Price multiplier applied to all regular items.
- **`PresetPriceMult`** — Price multiplier applied to all weapon and equipment presets.

All of the above also have a `Discount*` variant that applies to the secondary discount tab, unlocked at Fence rep level 6.

---

---

---

### ⚫ *Default Item Pool Additions*

> 🚨 EXPERIMENTAL, not battle-tested.
This feature was added mostly out of curiosity and hasn't received much attention since. I don't personally use it and have no plans to in the near future, so I haven't invested time in making it robust. It will likely misbehave with weapons or armors. If you just want to throw a couple of simple items into the vanilla pool it might do the job, but if you actually care about controlling what Fence stocks, just set EnableDefaultItemPool: false and use the Custom Item Pool : it's more reliable and more flexible.

Extends the built-in pool with extra items of your choice. Only active if `EnableDefaultItemPool` is also true.

- **`EnableDefaultItemPoolAdditions`** — switch for this sub-section only.
- **`DefaultItemPoolAdditions`** — list of items to inject into the native pool. Each entry supports:
    - **`Tpl`** — item template ID
    - **`RoublePrice`** — price in roubles
    - **`LoyaltyLevel`** — Fence rep level required (`1` = always visible)

---

---

---

### ⚫ *Custom Item Pool*

Completely independent from the default pool. Defines a weighted pool of offers that are injected directly into Fence's assort on every refresh, on top of whatever the default pool generates.

- **`CustomItemCount`** — How many offers to draw from the pool per refresh.
- **`CustomItemPriceMultiplier`** — Global price multiplier applied to all custom offers. Useful to scale the entire pool up or down without editing each entry individually.
- **`CustomItemPool`** — the pool to draw from. Each entry supports:
    - **`Tpl`** — item template ID
    - **`Weight`** — relative draw chance. Higher values mean the item is more likely to be picked. Items with `Weight: 0` are ignored.
    - **`StackSize`****** — quantity per offer slot
    - **`CurrencyTpl`** — currency TPL (roubles, dollars, euros, GP coin, Lega medal, or any other item)
    - **`Price`** — price in the chosen currency, before `CustomItemPriceMultiplier` is applied
    - **`LoyaltyLevel`** — Fence rep level required (`1` = always visible)

> 💡 The weighted draw means you can mix rare and common items in the same pool — a weapon with `Weight: 2` is twice as likely to appear as one with `Weight: 1`. Set extreme values (e.g. `Weight: 100` vs `Weight: 1`) to simulate truly rare drops.

> 💡 Example config included. The default fenceConfig.json ships with a ready-to-use custom pool listing every weapon in the game — including all weapons added by WTT - Content Backport — all priced in GP coins. Feel free to use it as a starting point or strip it down to whatever you actually need.

## 🏪 Buyback

> Controls what each trader will accept from the player. Each trader can be configured independently — or left untouched if you want to keep vanilla behavior for specific ones.

---

---

---

#### ⚙️ Configuration : `buybackConfig.json`

Each trader can be set to one of four modes:

- **Default** — leaves the vanilla buy policy untouched
- **Disabled** — trader refuses to buy anything from the player
- **Categories** — trader only accepts items from the specified handbook category IDs
- **AllWithBlacklist** — trader accepts all handbook items except those explicitly blacklisted

## 📦 Supply

> Controls trader restock timers and stock amounts per restock cycle.

---

---

---

#### ⚙️ Configuration : `supplyConfig.json`

**`RestockTimes`** — Restock interval in seconds for each trader. If the configured interval is shorter than the trader's current timer, the timer is clamped immediately on server start — no need to wait out the old one.

**`StockMultipliers`** — Multiplies the purchasable quantity (`BuyRestrictionMax`) of trader items per restock cycle. Items with no buy restriction (truly unlimited) are not affected.

- **`EnableByTrader`** — Apply a flat multiplier per trader.
- **`EnableByCategory`** — Apply a multiplier per handbook category. Sub-categories inherit the parent multiplier automatically.

> 💡 Both modes can be active simultaneously — multipliers are combined.

> ⚠️ **Stock multipliers are not compatible with routed trades.** Routed offers are automatically injected as unlimited stock, which means they have no `BuyRestrictionMax` to multiply, the supply patcher skips them entirely. This is a known limitation with no planned fix for now, as unlimited stock is the intended behavior for routed trades. If you need stock restrictions on specific items, use manual trades instead.

## 📝 Handbook Prices

> Overrides the handbook price of any item by TPL.

---

---

---

#### ⚙️ Configuration : `handbookPricesConfig.json`

A flat dict of `TPL → price in roubles`. Any item in the game can be overridden here.
```jsonc
"62330c18744e5e31df12f516": 186, // .357 Magnum JHP
"59faff1d86f7746c51718c9c": 35000 // Physical bitcoin
```

Handbook prices are the foundation of several SPT systems, this feature has broad effects beyond just what's visible at traders :

- **Routed trades** — routed offer prices are derived from handbook price, so overriding a price here directly changes what it costs at the trader.
- **Trader sell prices** — what traders pay when buying items from the player is also handbook-based.

## 📈 Flea Market

> Controls dynamic flea market offer generation. Trader flea offers are not affected by any of these settings.

---

---

---

#### ⚙️ Configuration : `fleaMarketConfig.json`

- **`Disable`** — Disables all dynamic flea market offer generation entirely. Both blocks new offer generation and purges any offers already present at server start. Trader offers are not affected.

- **`DynamicForceDisable`** — List of TPLs to remove from dynamic flea offers. Sets `CanSellOnRagfair = false` on matching item templates. Nothing outside the listed TPLs is touched.

- **`DynamicForceEnable`** — List of TPLs to force into dynamic flea offers. Sets `CanSellOnRagfair = true` on matching item templates. Nothing outside the listed TPLs is touched.

> Both lists are applied independently and can coexist. If the same TPL appears in both, `ForceEnable` wins as it runs last.

> The flea market is, in my opinion, a leftover from the live game that has no place in a balanced solo/coop experience. It adds a lot of clicking for not much actual fun, and there are better ways to introduce randomness into the economy. That said, I'll add the occasional feature for it if needed.

## 🛡️ Insurance

> Disable insurance globally or selectively by category and item TPL.

---

#### ⚙️ Configuration : `insuranceConfig.json`

- **`DisableAll`** — Disables insurance on every item in the game. If true, the blacklists below are ignored.
- **`CategoryBlacklist`** — Disables insurance on all items belonging to the specified handbook categories. Sub-categories are included automatically. Ships with the full vanilla category list pre-filled, all disabled by default.
- **`TplBlacklist`** — Disables insurance on specific items by TPL.

> 💡 `CategoryBlacklist` and `TplBlacklist` are cumulative, an item is blacklisted if it matches either one.

> 📝 All other insurance-related settings are already covered by ServerValueModifier. No point duplicating them here.

## 🏠 Hideout

> Full control over hideout area requirements, construction times, and the bitcoin farm through config files.

> 💡 The default `hideoutConfig.json` ships with every area pre-configured to match vanilla exactly. Enabling the feature without changing anything produces zero difference in-game, it's just a clean starting point. Edit only what you want to change, leave the rest as-is.

---

---

---

#### ⚙️ Configuration : `hideoutConfig.json`

---

#### Area fields

- **`RemoveFromDb`** — removes the area from the database entirely.
- **`Enabled`** — whether the area is available to the player.
- **`DisplayLevel`** — whether the current level is displayed in the UI.

---

#### Construction time

- **`UseCustomConstructionTime`** — master toggle. If false, vanilla construction times are left untouched.
- **`ConstructionTime`** — dict of stage level → time in seconds. Stages not listed default to `0`. Use an empty dict `{}` to set everything to instant.

---

#### Requirements

Each requirement type (`Item`, `Area`, `Skill`, `TraderLoyalty`) is controlled independently via a `UseCustom*` toggle and a matching dict.

- When **true** : vanilla requirements of that type are **cleared** on every stage first, then the dict is injected per stage.
- When **false** : vanilla requirements of that type are left completely untouched.
- To **only clear** without injecting anything, set the toggle to true and leave the dict empty `{}`.

Dict keys are stage level strings (`"1"`, `"2"`, `"3"`...). Stages not listed are left as-is after the clear.

Each requirement type expects the following fields per entry :

- **`ItemRequirements`** — `ItemTpl`, `ItemCount`
- **`AreaRequirements`** — `AreaName` (matches `HideoutAreas` enum, case-insensitive), `AreaLevel`
- **`SkillRequirements`** — `SkillName`, `SkillLevel`
- **`TraderRequirements`** — `TraderName` (trader nickname, case-insensitive), `TraderLoyalty`

---

#### Found in Raid

- **`RequireFoundInRaid`** — forces `IsSpawnedInSession` on every item requirement across all areas and all stages, after all custom patches have been applied. `true` = FIR required everywhere, `false` = FIR removed everywhere, `null` = untouched.

---

#### Bitcoin farm

- **`ProductionSpeedMultiplier`** — divides the base production time. e.g. `2.0` = twice as fast. (default: `1.0` → 145000s per bitcoin)
- **`MaxCapacity`** — maximum number of bitcoins that can accumulate. (default: `3`)
- **`GpuBoostRate`** — GPU boost rate per card. (default: `0.041225`)

## ⚗️ Crafting

> Define custom crafting recipes per hideout area, and optionally clear all existing recipes for specific areas before injecting your own.


> 💡 The default `craftingConfig.json` ships with all vanilla recipes pre-configured. Enabling the feature without changing anything produces zero difference in-game; it's just a clean starting point. Add your own recipes, remove ones you don't want, or wipe entire areas and rebuild them from scratch.

---

#### ⚙️ Configuration : `craftingConfig.json`

- **`ClearAreas`** — list of hideout areas whose vanilla recipes will be wiped before injection
- **`Recipes`** — recipes grouped by hideout area. Each recipe supports area level requirement, end product, count, production time, fuel requirement, and a list of input requirements (items or area levels)

## 🔌 Compatibility

Most features work out of the box with modded content. Auto-routing, manual offers, buyback rules, `AllItemsExamined`, and handbook price overrides all operate on TPLs — as long as you know the correct TPL for a modded item, you can reference it anywhere in the config exactly like a vanilla one. I haven't done extensive compatibility testing though, and this is something I'll look to improve in future updates.

**Tested compatible mods:**
- [More Energy Drinks](https://forge.sp-tarkov.com/mod/1688/more-energy-drinks) by Hood
- [WTT - Content Backport](https://forge.sp-tarkov.com/mod/2512/wtt-content-backport) by GrooveypenguinX

> ⚠️ Any mod that touches the same systems as RZCustomEconomy will conflict. This includes mods that modify trader assorts, flea market offers, hideout requirements or production times, crafting recipes, or trader buyback policies.

## 🔞 Hot Anime Girls

![Rick](https://i.redd.it/i8ijkqb0v2cf1.gif)

{.endtabset}

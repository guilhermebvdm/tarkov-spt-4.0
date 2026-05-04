> ### Why not just edit the loot pool?
> SPT's native loot system assigns weights to items in a pool and rolls randomly when generating a bot. The higher the weight, the more likely an item is to appear, but it's never guaranteed.
This mod sidesteps that entirely by injecting items directly into bot pockets after generation, so you get exactly what you configured every time.

---

---

---

## ⚙️ How it works

The mod runs after the bot is fully generated and injects items in two passes:

1. **Free slots first** — empty pocket slots are filled first.
2. **Overwrite SPT items** — if all slots are full, SPT-generated items are replaced.

This guarantees your configured items always end up in the pockets, regardless of what SPT put there.

> ⚠️ Currently covers pocket injection only. Backpack and vest support may come later, but it's not a priority right now.

> ⚠️ Only **1x1 items** are supported. There are no safeguards but anything larger will most likely cause issues.

> ⚠️ Since the mod runs at the start of each raid, a config validator runs at server startup and logs warnings for any invalid entries : unknown TPLs, incorrect stack ranges, items larger than 1x1, chance values out of range.

---

---

---

## 🔧 Configuration

### Roles

Bot roles are fully configurable directly in the config file via `BossRoles` and `FollowerRoles`. Any role listed in `BossRoles` gets boss loot, any role in `FollowerRoles` gets follower loot. The Goon followers (Big Pipe, Birdeye) are in `BossRoles` by default.

```jsonc
  "BossRoles": [
    "bosstagilla",     // Tagilla
    "bossknight",      // Knight (goon)
    "followerbigpipe", // Big Pipe (goon)
    "followerbirdeye"  // Birdeye (goon)
    ...
  ],

  "FollowerRoles": [
    "followergluharscout",
    "followergluharassault",
    "followergluharprotect",
    ...
  ],
```

### Boss config

Bosses support two modes controlled by `UseGlobalConfig`:

- **`true`** — one config applies to all bosses
- **`false`** — each boss has its own pocket loot list via `PerBoss`

If `UseGlobalConfig` is false and a boss has no `PerBoss` entry, it falls back to `Global` automatically.

### Per-item config

```jsonc
{ "Tpl": "59faff1d86f7746c51718c9c", "Chance": 100, "MinStack": 1, "MaxStack": 1 }
```

- `Chance` — 0 to 100. 100 = guaranteed, 0 = never drops.
- `MinStack` / `MaxStack` — Stack size range. A random value between the two is picked on each injection. Defaults to 1/1 if not specified. Automatically clamped to the item's max stack size.

Items are processed **in order, top to bottom**. A failed chance roll does **not** consume a slot, the next item in the list gets a chance to fill it. Put your highest priority items first.

### Overwrite priority

When all pocket slots are full and the mod needs to overwrite SPT items, it always targets the **cheapest item first** based on handbook price. This preserves the most valuable SPT loot as much as possible.

### Overwrite blacklist

Items protected from being overwritten when the mod needs to make room. Two ways to blacklist:

- **`Tpls`** — Exact TPL match.
- **`Categories`** — Any item whose parent chain contains the category ID is protected.

> 💡 Blacklisted items are never touched, regardless of their handbook price.

---

---

---

## 🔀 How the sequential logic works

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

---

---

---

## 🔌 Compatibility

- Safe to use alongside other loot mod, because it runs after all vanilla loot generation is complete.
- **Fika** — Should work if the mod is installed server side, but this hasn't been tested yet. Please let me know.

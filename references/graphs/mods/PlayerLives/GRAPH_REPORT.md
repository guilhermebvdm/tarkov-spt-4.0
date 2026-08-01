# Graph Report - mods\PlayerLives\modded  (2026-08-01)

## Corpus Check
- 16 files · ~5,026 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 148 nodes · 194 edges · 17 communities (16 shown, 1 thin omitted)
- Extraction: 88% EXTRACTED · 12% INFERRED · 0% AMBIGUOUS · INFERRED: 23 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `b8dae51a`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- [[_COMMUNITY_Community 0|Community 0]]
- [[_COMMUNITY_Community 1|Community 1]]
- [[_COMMUNITY_Community 2|Community 2]]
- [[_COMMUNITY_Community 3|Community 3]]
- [[_COMMUNITY_Community 4|Community 4]]
- [[_COMMUNITY_Community 5|Community 5]]
- [[_COMMUNITY_Community 6|Community 6]]
- [[_COMMUNITY_Community 7|Community 7]]
- [[_COMMUNITY_Community 8|Community 8]]
- [[_COMMUNITY_Community 9|Community 9]]
- [[_COMMUNITY_Community 10|Community 10]]
- [[_COMMUNITY_Community 11|Community 11]]
- [[_COMMUNITY_Community 12|Community 12]]
- [[_COMMUNITY_Community 13|Community 13]]
- [[_COMMUNITY_Community 14|Community 14]]

## God Nodes (most connected - your core abstractions)
1. `RevivalFeatures` - 13 edges
2. `RevivalFeatures` - 11 edges
3. `RevivalFeatures` - 9 edges
4. `ConfigurationManagerAttributes` - 9 edges
5. `RevivalFeatures` - 8 edges
6. `ConfigurationManagerAttributes` - 8 edges
7. `Player` - 7 edges
8. `RevivalFeatures` - 6 edges
9. `Plugin` - 6 edges
10. `Player` - 5 edges

## Surprising Connections (you probably didn't know these)
- `RevivalFeatures` --inherits--> `ModulePatch`  [EXTRACTED]
  Features/Features.cs →   _Bridges community 5 → community 4_
- `RaidStartPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Patches/RaidStartPatch.cs →   _Bridges community 4 → community 6_

## Import Cycles
- None detected.

## Communities (17 total, 1 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.18
Nodes (8): PatchPostfix, Player, Dictionary, float, int, Player, PlayerLives.Features, RevivalFeatures

### Community 1 - "Community 1"
Cohesion: 0.19
Nodes (6): Color, ENotificationDurationType, ENotificationIconType, Player, PlayerLives.Features, RevivalFeatures

### Community 2 - "Community 2"
Cohesion: 0.30
Nodes (4): Player, string, PlayerLives.Features, RevivalFeatures

### Community 3 - "Community 3"
Cohesion: 0.17
Nodes (12): Attribute, CustomHotkeyDrawerFunc, ConfigurationManagerAttributes, bool, ConfigEntryBase>, Func<object, string>, Func<string, object>, int (+4 more)

### Community 4 - "Community 4"
Cohesion: 0.18
Nodes (8): ActiveHealthController, FieldInfo, ModulePatch, EDamageType, MethodBase, DeathPatch, PlayerLives.Patches, PatchPrefix

### Community 5 - "Community 5"
Cohesion: 0.20
Nodes (6): Dictionary, EDamageType, float, MethodBase, PlayerLives.Features, RevivalFeatures

### Community 6 - "Community 6"
Cohesion: 0.25
Nodes (5): GameWorld, MethodBase, PatchPostfix, PlayerLives.Patches, RaidStartPatch

### Community 7 - "Community 7"
Cohesion: 0.22
Nodes (8): ConfigurationManagerAttributes, bool, ConfigEntryBase>, Func<object, string>, Func<string, object>, int, object, string

### Community 8 - "Community 8"
Cohesion: 0.25
Nodes (6): BaseUnityPlugin, ManualLogSource, bool, int, PlayerLives, Plugin

### Community 10 - "Community 10"
Cohesion: 0.29
Nodes (4): PainKiller, PlayerLives.Features, GClass3008, GInterface358

### Community 11 - "Community 11"
Cohesion: 0.33
Nodes (4): ConfigEntry, ConfigFile, PlayerLives.Helpers, Settings

### Community 12 - "Community 12"
Cohesion: 0.40
Nodes (3): Player, PlayerLives.Features, RevivalFeatures

### Community 13 - "Community 13"
Cohesion: 0.33
Nodes (6): net472, BepInEx.Analyzers (1.*), BepInEx.Core (5.*), BepInEx.PluginInfoProps (1.*), PlayerLives, Microsoft.NET.Sdk

### Community 14 - "Community 14"
Cohesion: 0.33
Nodes (4): Player, string, FikaReviveDetector, PlayerLives.Helpers

## Knowledge Gaps
- **58 isolated node(s):** `PlayerLives.Features`, `Dictionary`, `float`, `MethodBase`, `Player` (+53 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RevivalFeatures` connect `Community 5` to `Community 0`, `Community 4`?**
  _High betweenness centrality (0.200) - this node is a cross-community bridge._
- **Why does `RevivalFeatures` connect `Community 9` to `Community 1`, `Community 10`?**
  _High betweenness centrality (0.073) - this node is a cross-community bridge._
- **What connects `PlayerLives.Features`, `Dictionary`, `float` to the rest of the system?**
  _58 weakly-connected nodes found - possible documentation gaps or missing edges._
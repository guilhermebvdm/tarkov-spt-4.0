# Graph Report - mods\SPT-Menu-Overhaul\modded  (2026-06-12)

## Corpus Check
- 22 files · ~46,379 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 349 nodes · 590 edges · 18 communities
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c3e8df24`
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
- [[_COMMUNITY_Community 15|Community 15]]
- [[_COMMUNITY_Community 17|Community 17]]

## God Nodes (most connected - your core abstractions)
1. `PlayerProfileFeaturesPatch` - 51 edges
2. `LayoutHelpers` - 38 edges
3. `MenuOverhaulPatch` - 18 edges
4. `ButtonHelpers` - 16 edges
5. `ButtonHoverEffects` - 16 edges
6. `GameObject` - 13 edges
7. `GameObject` - 12 edges
8. `PlayerProfileTransformController` - 12 edges
9. `Settings` - 12 edges
10. `LightHelpers` - 11 edges

## Surprising Connections (you probably didn't know these)
- `PlayerProfileFeaturesPatch` --implements--> `ICleanupPatch`  [EXTRACTED]
  Patches/PlayerProfileFeaturesPatch.cs → Patches/ICleanupPatch.cs
- `MenuOverhaulPatch` --implements--> `ICleanupPatch`  [EXTRACTED]
  Patches/MenuOverhaulPatch.cs → Patches/ICleanupPatch.cs

## Import Cycles
- None detected.

## Communities (18 total, 0 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.08
Nodes (15): bool, EventArgs, GameObject, MatchmakerPlayerControllerClass, MenuScreen, MethodBase, PatchPostfix, PlayerModelDragRotator (+7 more)

### Community 1 - "Community 1"
Cohesion: 0.08
Nodes (19): bool, Color, DefaultUIButtonAnimation, Dictionary, GameObject, int, List, MenuScreen (+11 more)

### Community 2 - "Community 2"
Cohesion: 0.06
Nodes (21): ModulePatch, MethodBase, PatchPostfix, MoxoPixel.MenuOverhaul.Patches, OnGameEndedPatch, MethodBase, PatchPostfix, MoxoPixel.MenuOverhaul.Patches (+13 more)

### Community 3 - "Community 3"
Cohesion: 0.10
Nodes (13): EnvironmentObjects, ICleanupPatch, MoxoPixel.MenuOverhaul.Patches, bool, EventArgs, MatchmakerPlayerControllerClass, MenuScreen, MethodBase (+5 more)

### Community 4 - "Community 4"
Cohesion: 0.21
Nodes (12): ButtonBaseState, Ease, ButtonHoverEffects, Color, DefaultUIButtonAnimation, Dictionary, float, Image (+4 more)

### Community 5 - "Community 5"
Cohesion: 0.17
Nodes (9): ButtonHelpers, FieldInfo, float, GameObject, MenuScreen, string, Task, Vector3 (+1 more)

### Community 6 - "Community 6"
Cohesion: 0.20
Nodes (6): float, GameObject, PlayerModelDragRotator, Transform, MoxoPixel.MenuOverhaul.Helpers, PlayerProfileTransformController

### Community 7 - "Community 7"
Cohesion: 0.18
Nodes (9): ConfigEntry, ConfigFile, bool, EventArgs, float, List, string, MoxoPixel.MenuOverhaul.Utils (+1 more)

### Community 8 - "Community 8"
Cohesion: 0.21
Nodes (5): EEftScreenType, bool, GameObject, MenuVisibilityController, MoxoPixel.MenuOverhaul.Helpers

### Community 9 - "Community 9"
Cohesion: 0.22
Nodes (5): Action, GameObject, LightHelpers, MoxoPixel.MenuOverhaul.Helpers, Light

### Community 10 - "Community 10"
Cohesion: 0.29
Nodes (7): Action, GameObject, Image, Profile, Transform, MoxoPixel.MenuOverhaul.Helpers, PlayerProfileStatsController

### Community 11 - "Community 11"
Cohesion: 0.17
Nodes (10): ConfigEntryBase>, CustomHotkeyDrawerFunc, Func<object, string>, Func<string, object>, object, MoxoPixel.MenuOverhaul.Patches, ConfigurationManagerAttributes, bool (+2 more)

### Community 12 - "Community 12"
Cohesion: 0.29
Nodes (10): BottomFieldUi, string, Environment, MenuButtons, MenuOverhaulConstants, MenuScreen, MoxoPixel.MenuOverhaul.Utils, PlayerModel (+2 more)

### Community 13 - "Community 13"
Cohesion: 0.20
Nodes (4): bool, float, MoxoPixel.MenuOverhaul.Utils, Utility

### Community 14 - "Community 14"
Cohesion: 0.28
Nodes (4): BaseUnityPlugin, List, MoxoPixel.MenuOverhaul, Plugin

### Community 15 - "Community 15"
Cohesion: 0.22
Nodes (6): bool, float, Transform, MoxoPixel.MenuOverhaul.Helpers, PlayerModelDragRotator, MonoBehaviour

### Community 17 - "Community 17"
Cohesion: 0.67
Nodes (3): netstandard2.1, Microsoft.NET.Sdk, SPT-MenuOverhaul

## Knowledge Gaps
- **92 isolated node(s):** `MoxoPixel.MenuOverhaul.Helpers`, `float`, `string`, `Task`, `Vector3` (+87 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PlayerProfileFeaturesPatch` connect `Community 0` to `Community 3`, `Community 2`, `Community 11`?**
  _High betweenness centrality (0.105) - this node is a cross-community bridge._
- **Why does `MenuOverhaulPatch` connect `Community 3` to `Community 2`?**
  _High betweenness centrality (0.049) - this node is a cross-community bridge._
- **Why does `ICleanupPatch` connect `Community 3` to `Community 0`?**
  _High betweenness centrality (0.022) - this node is a cross-community bridge._
- **What connects `MoxoPixel.MenuOverhaul.Helpers`, `float`, `string` to the rest of the system?**
  _92 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.08287961282516637 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.08078431372549019 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.06060606060606061 - nodes in this community are weakly interconnected._
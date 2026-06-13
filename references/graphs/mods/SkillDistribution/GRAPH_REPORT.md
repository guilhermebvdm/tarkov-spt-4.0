# Graph Report - mods\SkillDistribution\modded  (2026-06-12)

## Corpus Check
- 23 files · ~8,951 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 169 nodes · 191 edges · 20 communities (17 shown, 3 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 5 edges (avg confidence: 0.8)
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
- [[_COMMUNITY_Community 14|Community 14]]
- [[_COMMUNITY_Community 15|Community 15]]
- [[_COMMUNITY_Community 16|Community 16]]
- [[_COMMUNITY_Community 17|Community 17]]
- [[_COMMUNITY_Community 18|Community 18]]
- [[_COMMUNITY_Community 20|Community 20]]

## God Nodes (most connected - your core abstractions)
1. `SkillDistributions` - 8 edges
2. `Settings` - 7 edges
3. `SkillHelper` - 7 edges
4. `Plugin` - 6 edges
5. `ServerConfig` - 5 edges
6. `SkillClass` - 5 edges
7. `AbstractSkillPatch` - 5 edges
8. `SkillUnsubscribePatch` - 5 edges
9. `AbstractSkillUnsubscribePatch` - 5 edges
10. `WorkoutBehaviourPatch` - 5 edges

## Surprising Connections (you probably didn't know these)
- `AbstractSkillPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  SkillDistributionClient/Patches/AbstractSkillPatch.cs →   _Bridges community 7 → community 0_
- `OnGameStartedPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  SkillDistributionClient/Patches/OnGameStartedPatch.cs →   _Bridges community 0 → community 12_
- `ProfileSelectionPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  SkillDistributionClient/Patches/ProfileSelectionPatch.cs →   _Bridges community 0 → community 15_
- `SkillPanelPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  SkillDistributionClient/Patches/SkillPanelPatch.cs →   _Bridges community 0 → community 9_
- `SkillTooltipPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  SkillDistributionClient/Patches/SkillTooltipPatch.cs →   _Bridges community 0 → community 8_

## Import Cycles
- 1-file cycle: `build.mjs -> build.mjs`

## Communities (20 total, 3 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.14
Nodes (10): ModulePatch, AbstractSkillUnsubscribePatch, SkillDistribution.Patches, SkillProgressUnsubscribePatch, SkillUnsubscribePatch, AbstractSkillClass, bool, MethodBase (+2 more)

### Community 1 - "Community 1"
Cohesion: 0.32
Nodes (6): ECompareMode, SkillDistribution.Helpers, SkillDistributions, int, List, SkillClass

### Community 2 - "Community 2"
Cohesion: 0.26
Nodes (8): ESkillId, float, SkillDistribution.Helpers, SkillHelper, bool, List, SkillClass, SkillManager

### Community 3 - "Community 3"
Cohesion: 0.18
Nodes (9): SkillDistribution.Patches, WorkoutBehaviourPatch, QteHandleData, MethodBase, PatchPrefix, SkillClass, SkillManager, SkillExperienceMultiplierData (+1 more)

### Community 4 - "Community 4"
Cohesion: 0.18
Nodes (8): net9.0, netstandard2.1, Lib.Harmony (2.4.2), SPTarkov.Common (4.0.0), SPTarkov.DI (4.0.0), SPTarkov.Server.Core (4.0.0), Microsoft.NET.Sdk, Microsoft.NET.Sdk

### Community 5 - "Community 5"
Cohesion: 0.20
Nodes (7): ConfigCategory, ConfigFile, Settings, SkillDistribution.Helpers, ConfigEntry, Dictionary, List

### Community 6 - "Community 6"
Cohesion: 0.22
Nodes (7): JsonUtil, RouteAction, List, SkillDistribution, SkillDistributionRouter, StaticRouter, ValueTask

### Community 7 - "Community 7"
Cohesion: 0.22
Nodes (6): AbstractSkillPatch, SkillDistribution.Patches, AbstractSkillClass, MethodBase, PatchPostfix, PatchPrefix

### Community 8 - "Community 8"
Cohesion: 0.22
Nodes (6): CodeInstruction, IEnumerable, SkillDistribution.Patches, SkillTooltipPatch, PatchTranspiler, MethodBase

### Community 9 - "Community 9"
Cohesion: 0.22
Nodes (6): GameObject, SkillDistribution.Patches, SkillPanelPatch, MethodBase, PatchPostfix, SkillClass

### Community 10 - "Community 10"
Cohesion: 0.28
Nodes (5): ServerConfig, SkillDistribution.Helpers, ConfigEntry, Dictionary, T

### Community 11 - "Community 11"
Cohesion: 0.25
Nodes (5): BaseUnityPlugin, ManualLogSource, SkillManager, Plugin, SkillDistribution

### Community 12 - "Community 12"
Cohesion: 0.25
Nodes (5): GameWorld, OnGameStartedPatch, SkillDistribution.Patches, MethodBase, PatchPostfix

### Community 14 - "Community 14"
Cohesion: 0.29
Nodes (5): IOnLoad, ModHelper, SkillDisctributionMod, SkillDistribution, Task

### Community 15 - "Community 15"
Cohesion: 0.29
Nodes (4): ProfileSelectionPatch, SkillDistribution.Patches, MethodBase, PatchPostfix

### Community 16 - "Community 16"
Cohesion: 0.40
Nodes (3): ENotificationIconType, Notifications, SkillDistribution.Helpers

## Knowledge Gaps
- **74 isolated node(s):** `SkillDistribution.Helpers`, `ENotificationIconType`, `SkillDistribution.Helpers`, `Dictionary`, `T` (+69 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `WorkoutBehaviourPatch` connect `Community 3` to `Community 0`?**
  _High betweenness centrality (0.054) - this node is a cross-community bridge._
- **Why does `AbstractSkillPatch` connect `Community 7` to `Community 0`?**
  _High betweenness centrality (0.042) - this node is a cross-community bridge._
- **Why does `SkillPanelPatch` connect `Community 9` to `Community 0`?**
  _High betweenness centrality (0.038) - this node is a cross-community bridge._
- **What connects `SkillDistribution.Helpers`, `ENotificationIconType`, `SkillDistribution.Helpers` to the rest of the system?**
  _74 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.13970588235294118 - nodes in this community are weakly interconnected._
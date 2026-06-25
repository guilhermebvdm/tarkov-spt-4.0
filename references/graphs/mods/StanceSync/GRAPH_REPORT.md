# Graph Report - mods\StanceSync\modded  (2026-06-22)

## Corpus Check
- 7 files · ~2,122 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 48 nodes · 45 edges · 7 communities
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `dd0c533f`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- [[_COMMUNITY_Community 0|Community 0]]
- [[_COMMUNITY_Community 1|Community 1]]
- [[_COMMUNITY_Community 2|Community 2]]
- [[_COMMUNITY_Community 3|Community 3]]
- [[_COMMUNITY_Community 4|Community 4]]
- [[_COMMUNITY_Community 5|Community 5]]

## God Nodes (most connected - your core abstractions)
1. `ConfigurationManagerAttributes` - 9 edges
2. `Camera` - 8 edges
3. `Plugin` - 6 edges
4. `OnLeanPatchPostfix` - 4 edges
5. `PlayerCameraControllerPatchPrefix` - 4 edges
6. `Player` - 2 edges
7. `StanceSync` - 2 edges
8. `hazelify.StanceSync` - 1 edges
9. `Player` - 1 edges
10. `SharedGameSettingsClass` - 1 edges

## Surprising Connections (you probably didn't know these)
- `PlayerCameraControllerPatchPrefix` --inherits--> `ModulePatch`  [EXTRACTED]
  Patches/PlayerCameraControllerPatchPrefix.cs →   _Bridges community 3 → community 4_

## Import Cycles
- None detected.

## Communities (7 total, 0 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.20
Nodes (9): bool, ConfigEntryBase>, ConfigurationManagerAttributes, CustomHotkeyDrawerFunc, Func<object, string>, Func<string, object>, int, object (+1 more)

### Community 1 - "Community 1"
Cohesion: 0.20
Nodes (7): Camera, Player, hazelify.StanceSync, FirearmController, ItemHandsController, MonoBehaviour, SharedGameSettingsClass

### Community 2 - "Community 2"
Cohesion: 0.29
Nodes (5): BaseUnityPlugin, ConfigEntry, ManualLogSource, hazelify.StanceSync, Plugin

### Community 3 - "Community 3"
Cohesion: 0.29
Nodes (5): ModulePatch, MethodBase, Player, OnLeanPatchPostfix, PatchPostfix

### Community 4 - "Community 4"
Cohesion: 0.25
Nodes (5): MethodBase, hazelify.StanceSync.Patches, PlayerCameraControllerPatchPrefix, PatchPrefix, PlayerCameraController

### Community 5 - "Community 5"
Cohesion: 0.67
Nodes (3): netstandard2.1, Microsoft.NET.Sdk, StanceSync

## Knowledge Gaps
- **24 isolated node(s):** `hazelify.StanceSync`, `Player`, `SharedGameSettingsClass`, `ItemHandsController`, `FirearmController` (+19 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `PlayerCameraControllerPatchPrefix` connect `Community 4` to `Community 3`?**
  _High betweenness centrality (0.067) - this node is a cross-community bridge._
- **What connects `hazelify.StanceSync`, `Player`, `SharedGameSettingsClass` to the rest of the system?**
  _24 weakly-connected nodes found - possible documentation gaps or missing edges._
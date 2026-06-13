# Graph Report - mods\AutoGym\modded  (2026-06-12)

## Corpus Check
- 3 files · ~1,454 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 51 nodes · 69 edges · 7 communities (6 shown, 1 thin omitted)
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

## God Nodes (most connected - your core abstractions)
1. `WorkoutBodySkinSwap` - 7 edges
2. `WorkoutGearVisibility` - 6 edges
3. `ShrinkingCircleQtePatch` - 6 edges
4. `Plugin` - 5 edges
5. `GameObjectVisibilityState` - 5 edges
6. `RendererVisibilityState` - 5 edges
7. `HideoutPlayerOwner` - 4 edges
8. `IVisibilityState` - 4 edges
9. `ShrinkingCircleQTE` - 3 edges
10. `HideoutPlayerOwnerPrepareWorkoutPatch` - 2 edges

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Import Cycles
- None detected.

## Communities (7 total, 1 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.33
Nodes (4): Dictionary, EquipmentSlot, HideoutPlayerOwner, WorkoutGearVisibility

### Community 1 - "Community 1"
Cohesion: 0.24
Nodes (8): bool, GameObject, GameObjectVisibilityState, HideoutPlayerOwnerPrepareWorkoutPatch, HideoutPlayerOwnerStopWorkoutPatch, IVisibilityState, RendererVisibilityState, Renderer

### Community 2 - "Community 2"
Cohesion: 0.24
Nodes (6): GClass1661, int, PlayerBody, HideoutPlayerOwner, Task, WorkoutBodySkinSwap

### Community 3 - "Community 3"
Cohesion: 0.39
Nodes (5): FieldInfo, Task, ShrinkingCircleQtePatch, ShrinkingCircleQTE, T

### Community 4 - "Community 4"
Cohesion: 0.50
Nodes (3): HashSet, IVisibilityState, List

### Community 5 - "Community 5"
Cohesion: 0.40
Nodes (4): BaseUnityPlugin, ConfigEntry, ManualLogSource, Plugin

## Knowledge Gaps
- **14 isolated node(s):** `netstandard2.1`, `Microsoft.NET.Sdk`, `ManualLogSource`, `ConfigEntry`, `EquipmentSlot` (+9 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ShrinkingCircleQtePatch` connect `Community 3` to `Community 1`?**
  _High betweenness centrality (0.202) - this node is a cross-community bridge._
- **Why does `WorkoutGearVisibility` connect `Community 0` to `Community 1`, `Community 4`?**
  _High betweenness centrality (0.156) - this node is a cross-community bridge._
- **Why does `Plugin` connect `Community 5` to `Community 1`?**
  _High betweenness centrality (0.113) - this node is a cross-community bridge._
- **What connects `netstandard2.1`, `Microsoft.NET.Sdk`, `ManualLogSource` to the rest of the system?**
  _14 weakly-connected nodes found - possible documentation gaps or missing edges._
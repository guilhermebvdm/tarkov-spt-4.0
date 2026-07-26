# Graph Report - mods\TRL-Fixes\modded  (2026-07-26)

## Corpus Check
- 6 files · ~1,422 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 37 nodes · 31 edges · 7 communities (4 shown, 3 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `8fc70a61`
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
1. `Patch_FikaReviveHitbox` - 5 edges
2. `Plugin` - 4 edges
3. `FlashbangBotPatch` - 3 edges
4. `FlashbangRadiusPatch` - 3 edges
5. `Patch_PoolManagerCreateItem` - 3 edges
6. `TRLFixes.Patches` - 1 edges
7. `TRLFixes.Patches` - 1 edges
8. `IExplosiveItem` - 1 edges
9. `Vector3` - 1 edges
10. `TRLFixes.Patches` - 1 edges

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Import Cycles
- None detected.

## Communities (7 total, 3 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.29
Nodes (4): FieldInfo, Patch_FikaReviveHitbox, TRLFixes.Patches, string

### Community 1 - "Community 1"
Cohesion: 0.29
Nodes (4): IExplosiveItem, FlashbangRadiusPatch, TRLFixes.Patches, Vector3

### Community 2 - "Community 2"
Cohesion: 0.33
Nodes (4): BaseUnityPlugin, ManualLogSource, Plugin, TRLFixes

### Community 3 - "Community 3"
Cohesion: 0.40
Nodes (4): ECameraType, GameObject, IPlayer, Item

## Knowledge Gaps
- **16 isolated node(s):** `TRLFixes.Patches`, `TRLFixes.Patches`, `IExplosiveItem`, `Vector3`, `TRLFixes.Patches` (+11 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Patch_PoolManagerCreateItem` connect `Community 5` to `Community 3`?**
  _High betweenness centrality (0.027) - this node is a cross-community bridge._
- **What connects `TRLFixes.Patches`, `TRLFixes.Patches`, `IExplosiveItem` to the rest of the system?**
  _16 weakly-connected nodes found - possible documentation gaps or missing edges._
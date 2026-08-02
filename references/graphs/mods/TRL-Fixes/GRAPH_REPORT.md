# Graph Report - mods\TRL-Fixes\modded  (2026-08-02)

## Corpus Check
- 6 files · ~1,650 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 35 nodes · 29 edges · 6 communities (4 shown, 2 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `8981b11e`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- [[_COMMUNITY_Community 0|Community 0]]
- [[_COMMUNITY_Community 1|Community 1]]
- [[_COMMUNITY_Community 2|Community 2]]
- [[_COMMUNITY_Community 3|Community 3]]
- [[_COMMUNITY_Community 4|Community 4]]
- [[_COMMUNITY_Community 6|Community 6]]

## God Nodes (most connected - your core abstractions)
1. `PickupAimingSafetyPatch` - 6 edges
2. `FixFikaReviveRagdollPatch` - 4 edges
3. `FlashbangBotPatch` - 3 edges
4. `FlashbangRadiusPatch` - 3 edges
5. `Plugin` - 3 edges
6. `TRLFixes.Patches` - 1 edges
7. `FieldInfo` - 1 edges
8. `TRLFixes.Patches` - 1 edges
9. `TRLFixes.Patches` - 1 edges
10. `IExplosiveItem` - 1 edges

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Import Cycles
- None detected.

## Communities (6 total, 2 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.33
Nodes (3): FieldInfo, FixFikaReviveRagdollPatch, TRLFixes.Patches

### Community 1 - "Community 1"
Cohesion: 0.29
Nodes (4): IExplosiveItem, FlashbangRadiusPatch, TRLFixes.Patches, Vector3

### Community 2 - "Community 2"
Cohesion: 0.40
Nodes (3): BaseUnityPlugin, Plugin, TRLFixes

### Community 3 - "Community 3"
Cohesion: 0.22
Nodes (6): bool, Exception, float, int, PickupAimingSafetyPatch, TRLFixes.Patches

## Knowledge Gaps
- **14 isolated node(s):** `TRLFixes.Patches`, `FieldInfo`, `TRLFixes.Patches`, `TRLFixes.Patches`, `IExplosiveItem` (+9 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **2 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **What connects `TRLFixes.Patches`, `FieldInfo`, `TRLFixes.Patches` to the rest of the system?**
  _14 weakly-connected nodes found - possible documentation gaps or missing edges._
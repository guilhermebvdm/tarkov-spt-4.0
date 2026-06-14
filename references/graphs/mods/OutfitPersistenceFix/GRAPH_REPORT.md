# Graph Report - mods\OutfitPersistenceFix\modded  (2026-06-13)

## Corpus Check
- 4 files · ~1,057 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 29 nodes · 28 edges · 6 communities (5 shown, 1 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `2968ee9c`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- [[_COMMUNITY_Community 0|Community 0]]
- [[_COMMUNITY_Community 1|Community 1]]
- [[_COMMUNITY_Community 2|Community 2]]
- [[_COMMUNITY_Community 3|Community 3]]
- [[_COMMUNITY_Community 4|Community 4]]

## God Nodes (most connected - your core abstractions)
1. `OutfitPersistenceFixMod` - 6 edges
2. `ProfileFixerCustomizationPatch` - 5 edges
3. `PmcData` - 2 edges
4. `Snapshot` - 2 edges
5. `net9.0` - 1 edges
6. `SPTarkov.Server.Core (4.0.0)` - 1 edges
7. `SPTarkov.DI (4.0.0)` - 1 edges
8. `SPTarkov.Common (4.0.0)` - 1 edges
9. `Microsoft.NET.Sdk` - 1 edges
10. `DatabaseService` - 1 edges

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Import Cycles
- None detected.

## Communities (6 total, 1 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.25
Nodes (6): bool, DatabaseService, IOnLoad, ISptLogger, OutfitPersistenceFixMod, Task

### Community 1 - "Community 1"
Cohesion: 0.33
Nodes (5): HarmonyPostfix, HarmonyPrefix, HarmonyPriority, PmcData, Snapshot

### Community 2 - "Community 2"
Cohesion: 0.33
Nodes (5): net9.0, SPTarkov.Common (4.0.0), SPTarkov.DI (4.0.0), SPTarkov.Server.Core (4.0.0), Microsoft.NET.Sdk

### Community 3 - "Community 3"
Cohesion: 0.50
Nodes (3): CustomizationItem, Dictionary, MongoId

## Knowledge Gaps
- **16 isolated node(s):** `net9.0`, `SPTarkov.Server.Core (4.0.0)`, `SPTarkov.DI (4.0.0)`, `SPTarkov.Common (4.0.0)`, `Microsoft.NET.Sdk` (+11 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ProfileFixerCustomizationPatch` connect `Community 4` to `Community 1`, `Community 3`?**
  _High betweenness centrality (0.087) - this node is a cross-community bridge._
- **What connects `net9.0`, `SPTarkov.Server.Core (4.0.0)`, `SPTarkov.DI (4.0.0)` to the rest of the system?**
  _16 weakly-connected nodes found - possible documentation gaps or missing edges._
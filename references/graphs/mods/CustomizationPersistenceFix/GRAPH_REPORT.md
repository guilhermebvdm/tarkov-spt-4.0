# Graph Report - mods\CustomizationPersistenceFix\modded  (2026-06-12)

## Corpus Check
- 4 files · ~592 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 22 nodes · 20 edges · 4 communities
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

## God Nodes (most connected - your core abstractions)
1. `CustomizationPersistenceFixMod` - 5 edges
2. `ProfileFixerCustomizationPatch` - 3 edges
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

## Communities (4 total, 0 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.32
Nodes (5): HarmonyPostfix, HarmonyPrefix, PmcData, ProfileFixerCustomizationPatch, Snapshot

### Community 1 - "Community 1"
Cohesion: 0.29
Nodes (5): bool, DatabaseService, IOnLoad, CustomizationPersistenceFixMod, Task

### Community 2 - "Community 2"
Cohesion: 0.33
Nodes (5): net9.0, SPTarkov.Common (4.0.0), SPTarkov.DI (4.0.0), SPTarkov.Server.Core (4.0.0), Microsoft.NET.Sdk

## Knowledge Gaps
- **10 isolated node(s):** `net9.0`, `SPTarkov.Server.Core (4.0.0)`, `SPTarkov.DI (4.0.0)`, `SPTarkov.Common (4.0.0)`, `Microsoft.NET.Sdk` (+5 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **What connects `net9.0`, `SPTarkov.Server.Core (4.0.0)`, `SPTarkov.DI (4.0.0)` to the rest of the system?**
  _10 weakly-connected nodes found - possible documentation gaps or missing edges._
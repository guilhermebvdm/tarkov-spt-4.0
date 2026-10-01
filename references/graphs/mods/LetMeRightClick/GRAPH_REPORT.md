# Graph Report - modded  (2026-09-06)

## Corpus Check
- 4 files · ~258 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 20 nodes · 19 edges · 6 communities (4 shown, 1 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 1 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `5ddac638`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- LMRC_Plugin
- .Transpile
- LMRC_Plugin.cs
- ItemUiContext_GetItemContextInteractions_Patch
- LetMeRightClick

## God Nodes (most connected - your core abstractions)
1. `ItemUiContext_GetItemContextInteractions_Patch` - 5 edges
2. `LMRC_Plugin` - 4 edges
3. `LetMeRightClick` - 2 edges
4. `LetMeRightClick.Patches` - 2 edges
5. `LetMeRightClick` - 1 edges
6. `netstandard2.1` - 1 edges
7. `Microsoft.NET.Sdk` - 1 edges

## Surprising Connections (you probably didn't know these)
- None detected - all connections are within the same source files.

## Import Cycles
- None detected.

## Communities (6 total, 1 thin omitted)

### Community 0 - "LMRC_Plugin"
Cohesion: 0.50
Nodes (3): BaseUnityPlugin, LMRC_Plugin, ManualLogSource

### Community 1 - ".Transpile"
Cohesion: 0.50
Nodes (3): CodeInstruction, IEnumerable, PatchTranspiler

### Community 3 - "ItemUiContext_GetItemContextInteractions_Patch"
Cohesion: 0.50
Nodes (3): MethodBase, ModulePatch, ItemUiContext_GetItemContextInteractions_Patch

### Community 4 - "LetMeRightClick"
Cohesion: 0.67
Nodes (3): netstandard2.1, LetMeRightClick, Microsoft.NET.Sdk

## Knowledge Gaps
- **3 isolated node(s):** `LetMeRightClick`, `netstandard2.1`, `Microsoft.NET.Sdk`
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 10 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ItemUiContext_GetItemContextInteractions_Patch` connect `ItemUiContext_GetItemContextInteractions_Patch` to `LMRC_Plugin`, `.Transpile`, `LMRC_Plugin.cs`?**
  _High betweenness centrality (0.427) - this node is a cross-community bridge._
- **Why does `LMRC_Plugin` connect `LMRC_Plugin` to `LMRC_Plugin.cs`?**
  _High betweenness centrality (0.219) - this node is a cross-community bridge._
- **What connects `LetMeRightClick`, `netstandard2.1`, `Microsoft.NET.Sdk` to the rest of the system?**
  _3 weakly-connected nodes found - possible documentation gaps or missing edges._
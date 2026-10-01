# Graph Report - modded  (2026-09-15)

## Corpus Check
- 6 files · ~2,042 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 70 nodes · 117 edges · 7 communities
- Extraction: 99% EXTRACTED · 1% INFERRED · 0% AMBIGUOUS · INFERRED: 1 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `d3bda652`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- DynamicExternalResolutionPatches
- DynamicExternalResolution
- DynamicExternalResolutionConfig
- PatchManager
- ConfigurationManagerAttributes
- DynamicExternalResolution

## God Nodes (most connected - your core abstractions)
1. `DynamicExternalResolutionConfig` - 14 edges
2. `DynamicExternalResolutionPatches` - 13 edges
3. `DynamicExternalResolution` - 7 edges
4. `PatchManager` - 7 edges
5. `ConfigurationManagerAttributes` - 4 edges
6. `OpticSightOnEnablePath` - 4 edges
7. `OpticSightOnDisablePath` - 4 edges
8. `ClientFirearmControllerChangeAimingModePath` - 4 edges
9. `DynamicExternalResolution.Configs` - 3 edges
10. `Patcher` - 3 edges

## Surprising Connections (you probably didn't know these)
- `PatchManager` --references--> `ModulePatch`  [EXTRACTED]
  mods/SPT-Dynamic-External-Resolution/modded/DynamicExternalResolutionPatches.cs →   _Bridges community 3 → community 1_

## Import Cycles
- None detected.

## Communities (7 total, 0 thin omitted)

### Community 0 - "DynamicExternalResolutionPatches"
Cohesion: 0.28
Nodes (9): CameraManager, EDLSSMode, EFSR2Mode, EFSR3Mode, DynamicExternalResolutionPatches, EAntialiasingMode, FieldInfo, PropertyInfo (+1 more)

### Community 1 - "DynamicExternalResolution"
Cohesion: 0.18
Nodes (9): BaseUnityPlugin, DynamicExternalResolution, ClientFirearmControllerChangeAimingModePath, OpticSightOnDisablePath, OpticSightOnEnablePath, MethodBase, ModulePatch, PatchPostfix (+1 more)

### Community 2 - "DynamicExternalResolutionConfig"
Cohesion: 0.19
Nodes (13): ConfigEntry, EDLSSMode, EFSR2Mode, EFSR3Mode, DLSSModeQuality, DynamicExternalResolutionConfig, DLSSMode, EnableMod (+5 more)

### Community 3 - "PatchManager"
Cohesion: 0.22
Nodes (5): DynamicExternalResolution.Configs, DynamicExternalResolution, Patcher, PatchManager, List

### Community 4 - "ConfigurationManagerAttributes"
Cohesion: 0.29
Nodes (4): ConfigEntryBase&gt;, ConfigFile, ConfigurationManagerAttributes, Func

### Community 5 - "DynamicExternalResolution"
Cohesion: 0.67
Nodes (3): DynamicExternalResolution, net471, Microsoft.NET.Sdk

## Knowledge Gaps
- **7 isolated node(s):** `net471`, `Microsoft.NET.Sdk`, `EnableMod`, `SuperSampling`, `DLSSMode` (+2 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 17 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `DynamicExternalResolutionConfig` connect `DynamicExternalResolutionConfig` to `PatchManager`, `ConfigurationManagerAttributes`?**
  _High betweenness centrality (0.306) - this node is a cross-community bridge._
- **Why does `DynamicExternalResolutionPatches` connect `DynamicExternalResolutionPatches` to `DynamicExternalResolution`, `PatchManager`?**
  _High betweenness centrality (0.279) - this node is a cross-community bridge._
- **Why does `DynamicExternalResolution` connect `DynamicExternalResolution` to `DynamicExternalResolutionPatches`, `PatchManager`, `ConfigurationManagerAttributes`?**
  _High betweenness centrality (0.232) - this node is a cross-community bridge._
- **What connects `net471`, `Microsoft.NET.Sdk`, `EnableMod` to the rest of the system?**
  _7 weakly-connected nodes found - possible documentation gaps or missing edges._
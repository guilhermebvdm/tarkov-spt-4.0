# Graph Report - references\spt-bigbrain  (2026-07-28)

## Corpus Check
- 24 files · ~5,622 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 223 nodes · 352 edges · 14 communities
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 15 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `b6ce1700`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- DrakiaXYZ.BigBrain.Internal
- CustomLayerWrapper
- BrainManager
- ConfigurationManagerAttributes
- .splitForSettingsGeneric
- CustomLogic
- ExcludeLayerHelpers
- AbstractLayerInfo
- BotBaseBrainActivateLayerPatch
- .GetTargetMethod
- DrakiaXYZ-BigBrain

## God Nodes (most connected - your core abstractions)
1. `BrainManager` - 26 edges
2. `CustomLayerWrapper` - 15 edges
3. `CustomLayer` - 14 edges
4. `DrakiaXYZ.BigBrain.Internal` - 14 edges
5. `ExcludeLayerHelpers` - 11 edges
6. `DrakiaXYZ.BigBrain.Brains` - 10 edges
7. `CustomLogic` - 8 edges
8. `AbstractLayerInfo` - 8 edges
9. `ConfigurationManagerAttributes` - 8 edges
10. `CustomLogicWrapper` - 7 edges

## Surprising Connections (you probably didn't know these)
- `LayerInfo` --inherits--> `AbstractLayerInfo`  [EXTRACTED]
  references/spt-bigbrain/Brains/BrainManager.cs → references/spt-bigbrain/Internal/AbstractLayerInfo.cs
- `ExcludeLayerInfo` --inherits--> `AbstractLayerInfo`  [EXTRACTED]
  references/spt-bigbrain/Brains/BrainManager.cs → references/spt-bigbrain/Internal/AbstractLayerInfo.cs
- `CustomLayerWrapper` --references--> `CustomLayer`  [EXTRACTED]
  references/spt-bigbrain/Internal/CustomLayerWrapper.cs → references/spt-bigbrain/Brains/CustomLayer.cs
- `CustomLogicWrapper` --references--> `CustomLogic`  [EXTRACTED]
  references/spt-bigbrain/Internal/CustomLogicWrapper.cs → references/spt-bigbrain/Brains/CustomLogic.cs

## Import Cycles
- None detected.

## Communities (14 total, 0 thin omitted)

### Community 0 - "DrakiaXYZ.BigBrain.Internal"
Cohesion: 0.06
Nodes (27): AICoreLogicAgentClass, BotLogicDecision, CustomBrain, DrakiaXYZ.BigBrain.Patches, DrakiaXYZ.BigBrain.Brains, DrakiaXYZ.BigBrain.Internal, ModulePatch, MethodBase (+19 more)

### Community 1 - "CustomLayerWrapper"
Cohesion: 0.08
Nodes (18): Action, AICoreActionEndStruct, BaseLogicLayerSimpleAbstractClass, ActionData, BotOwner, StringBuilder, Type, Action (+10 more)

### Community 2 - "BrainManager"
Cohesion: 0.17
Nodes (16): AICoreLogicLayerClass, BotOwner, ExcludeLayerInfo, FieldInfo, int, List, Type, WildSpawnType (+8 more)

### Community 3 - "ConfigurationManagerAttributes"
Cohesion: 0.09
Nodes (19): Attribute, BaseUnityPlugin, ManualLogSource, BigBrainPlugin, bool, ConfigEntryBase, ConfigEntryBase&gt;, ConfigFile (+11 more)

### Community 4 - ".splitForSettingsGeneric"
Cohesion: 0.20
Nodes (12): IEnumerable, CollectionIntersection, ExcludeLayerInfo, Func, IEnumerable, List, Predicate, string (+4 more)

### Community 5 - "CustomLogic"
Cohesion: 0.15
Nodes (7): BaseNodeAbstractClass, ActionData, BotOwner, StringBuilder, CustomLogic, GClass26, CustomLogicWrapper

### Community 6 - "ExcludeLayerHelpers"
Cohesion: 0.28
Nodes (7): IList, BotOwner, ExcludeLayerInfo, IEnumerable, Predicate, WildSpawnType, ExcludeLayerHelpers

### Community 7 - "AbstractLayerInfo"
Cohesion: 0.16
Nodes (8): BotOwner, IEnumerable, List, WildSpawnType, AbstractLayerInfo, BotOwner, ExcludeLayerInfo, BrainHelpers

### Community 8 - "BotBaseBrainActivateLayerPatch"
Cohesion: 0.25
Nodes (5): AICoreLogicLayerClass, FieldInfo, MethodBase, PatchPrefix, BotBaseBrainActivateLayerPatch

### Community 9 - ".GetTargetMethod"
Cohesion: 0.40
Nodes (3): MethodBase, FieldInfo, Type

### Community 10 - "DrakiaXYZ-BigBrain"
Cohesion: 0.67
Nodes (3): DrakiaXYZ-BigBrain, netstandard2.1, Microsoft.NET.Sdk

## Knowledge Gaps
- **3 isolated node(s):** `CustomBrain`, `netstandard2.1`, `Microsoft.NET.Sdk`
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `DrakiaXYZ.BigBrain.Internal` connect `DrakiaXYZ.BigBrain.Internal` to `BotBaseBrainActivateLayerPatch`, `.splitForSettingsGeneric`, `CustomLogic`, `AbstractLayerInfo`?**
  _High betweenness centrality (0.405) - this node is a cross-community bridge._
- **Why does `BrainManager` connect `BrainManager` to `DrakiaXYZ.BigBrain.Internal`?**
  _High betweenness centrality (0.183) - this node is a cross-community bridge._
- **Why does `CustomLayerWrapper` connect `CustomLayerWrapper` to `DrakiaXYZ.BigBrain.Internal`?**
  _High betweenness centrality (0.178) - this node is a cross-community bridge._
- **What connects `CustomBrain`, `netstandard2.1`, `Microsoft.NET.Sdk` to the rest of the system?**
  _3 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `DrakiaXYZ.BigBrain.Internal` be split into smaller, more focused modules?**
  _Cohesion score 0.05507246376811594 - nodes in this community are weakly interconnected._
- **Should `CustomLayerWrapper` be split into smaller, more focused modules?**
  _Cohesion score 0.07899159663865546 - nodes in this community are weakly interconnected._
- **Should `ConfigurationManagerAttributes` be split into smaller, more focused modules?**
  _Cohesion score 0.08666666666666667 - nodes in this community are weakly interconnected._
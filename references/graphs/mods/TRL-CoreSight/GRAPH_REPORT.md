# Graph Report - modded  (2026-09-20)

## Corpus Check
- 24 files · ~20,166 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 488 nodes · 822 edges · 24 communities (22 shown, 1 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 35 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `d3bda652`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- ModConfig
- ShellSpawnCullingPatch
- PerformanceManager
- TRLCoreSight.Core
- DeclutterManager
- NaturalConcealmentManager
- AimVegetationInspector
- .IsProtectedTarkovComponent
- CpuTopologyManager
- LootCullingManager
- AmbientAudioCullingManager
- LocalLightShadowManager
- ConcealmentVisualizer
- VegetationSpatialIndex
- .DumpActiveSceneVegetation
- BotPerformanceLimiter
- ModConfig.cs
- ModulePatch
- GameWorldOnDestroyPatch
- TRLCoreSight.Patches
- WeaponShotGCPatch
- LootRegisterPatch
- .Awake

## God Nodes (most connected - your core abstractions)
1. `ModConfig` - 71 edges
2. `NaturalConcealmentManager` - 42 edges
3. `DeclutterManager` - 27 edges
4. `PerformanceManager` - 25 edges
5. `LootCullingManager` - 23 edges
6. `TRLCoreSight.Core` - 20 edges
7. `LocalLightShadowManager` - 19 edges
8. `TRLCoreSight.Configuration` - 18 edges
9. `AmbientAudioCullingManager` - 18 edges
10. `ConcealmentVisualizer` - 18 edges

## Surprising Connections (you probably didn't know these)
- `PerformanceManager` --references--> `AimVegetationInspector`  [EXTRACTED]
  mods/TRL-CoreSight/modded/Core/PerformanceManager.cs → mods/TRL-CoreSight/modded/Core/AimVegetationInspector.cs
- `PerformanceManager` --references--> `AmbientAudioCullingManager`  [EXTRACTED]
  mods/TRL-CoreSight/modded/Core/PerformanceManager.cs → mods/TRL-CoreSight/modded/Core/AmbientAudioCullingManager.cs
- `PerformanceManager` --references--> `BotPerformanceLimiter`  [EXTRACTED]
  mods/TRL-CoreSight/modded/Core/PerformanceManager.cs → mods/TRL-CoreSight/modded/Core/BotPerformanceLimiter.cs
- `ConcealmentVisualizer` --references--> `NaturalConcealmentManager`  [EXTRACTED]
  mods/TRL-CoreSight/modded/Core/ConcealmentVisualizer.cs → mods/TRL-CoreSight/modded/Core/NaturalConcealmentManager.cs
- `PerformanceManager` --references--> `DeclutterManager`  [EXTRACTED]
  mods/TRL-CoreSight/modded/Core/PerformanceManager.cs → mods/TRL-CoreSight/modded/Core/DeclutterManager.cs

## Import Cycles
- None detected.

## Communities (24 total, 1 thin omitted)

### Community 0 - "ModConfig"
Cohesion: 0.03
Nodes (67): ConfigEntry, ModConfig, AimLODBias, AimOffsetNerfIntensity, AudioCullingMargin, BaseLODBias, BotOcclusionCheckInterval, BreakProximityDistance (+59 more)

### Community 1 - "ShellSpawnCullingPatch"
Cohesion: 0.25
Nodes (6): MethodBase, PatchPrefix, WeaponManagerClass, ShellSpawnAllCullingPatch, ShellSpawnCullingPatch, ShellSpawnJamCullingPatch

### Community 2 - "PerformanceManager"
Cohesion: 0.07
Nodes (15): FrametimeBenchmark, Instance, Player, GCOptimizerManager, AllocatedMemoryMB, Instance, IsSuppressed, SuppressedEventsCount (+7 more)

### Community 3 - "TRLCoreSight.Core"
Cohesion: 0.17
Nodes (8): ECpuAffinityMode, Auto, Disabled, PerformanceCores, PhysicalCoresOnly, PrimaryCluster, TRLCoreSight.Core, TRLCoreSight.Configuration

### Community 4 - "DeclutterManager"
Cohesion: 0.10
Nodes (20): Bounds, Collider, GameObject, List, Renderer, DeclutterManager, Instance, EClutterType (+12 more)

### Community 5 - "NaturalConcealmentManager"
Cohesion: 0.10
Nodes (21): BodyConcealmentZone, CapsuleCollider, Collider, GameObject, GPUInstancerDetailManager, List, Player, Transform (+13 more)

### Community 6 - "AimVegetationInspector"
Cohesion: 0.11
Nodes (17): AimInspectionResult, ClassificationData, GPUInstancerDetailManager, List, Player, Texture2D, Vector3, AimInspectionResult (+9 more)

### Community 7 - ".IsProtectedTarkovComponent"
Cohesion: 0.07
Nodes (26): BakedLodContent, BallisticCollider, BorderZone, BotOwner, BotSpawner, LootItem, Player, CullingGroup (+18 more)

### Community 8 - "CpuTopologyManager"
Cohesion: 0.15
Nodes (14): CpuTopologyManager, NeedsMainThreadPin, NeedsMainThreadUnpin, LOGICAL_PROCESSOR_RELATIONSHIP, RelationAll, RelationCache, RelationGroup, RelationNumaNode (+6 more)

### Community 9 - "LootCullingManager"
Cohesion: 0.17
Nodes (11): Camera, List, LootItem, Plane, Player, Transform, LootCullingManager, CulledLootCount (+3 more)

### Community 10 - "AmbientAudioCullingManager"
Cohesion: 0.16
Nodes (10): AudioSource, List, Player, Transform, AmbientAudioCullingManager, Instance, PausedSourcesCount, TotalManagedSources (+2 more)

### Community 11 - "LocalLightShadowManager"
Cohesion: 0.15
Nodes (10): Dictionary, Player, LocalLightShadowManager, Instance, IsOptimized, OptimizedLightsCount, TotalManagedLights, Light (+2 more)

### Community 12 - "ConcealmentVisualizer"
Cohesion: 0.19
Nodes (8): BodyMeshGhost, GameObject, List, Player, BodyMeshGhost, ConcealmentVisualizer, Material, SkinnedMeshRenderer

### Community 13 - "VegetationSpatialIndex"
Cohesion: 0.17
Nodes (10): Collider, Dictionary, List, Renderer, Vector3, VegetationMeshEntry, VegetationSpatialIndex, TotalActiveCells (+2 more)

### Community 14 - ".DumpActiveSceneVegetation"
Cohesion: 0.25
Nodes (8): Collider, List, Renderer, Texture2D, VegetationAssetDumper, VegetationItemInfo, HashSet, VegetationItemInfo

### Community 15 - "BotPerformanceLimiter"
Cohesion: 0.30
Nodes (6): Animator, Camera, Plane, Player, BotPerformanceLimiter, OccludedBotsCount

### Community 16 - "ModConfig.cs"
Cohesion: 0.18
Nodes (10): ConfigurationManagerAttributes, IsAdvanced, Order, EDeclutterMode, FullGameObject, RendererOnly, LightShadowOptimizationMode, DisableAllSecondaryShadows (+2 more)

### Community 18 - "ModulePatch"
Cohesion: 0.19
Nodes (8): BotAimingClass, EnemyInfo, ModulePatch, MethodBase, PatchPostfix, BotAimOffsetPatch, BotVisionSpeedPatch, Type

### Community 19 - "GameWorldOnDestroyPatch"
Cohesion: 0.20
Nodes (6): GameWorld, MethodBase, PatchPostfix, PatchPrefix, GameWorldOnDestroyPatch, RaidStartPatch

### Community 20 - "TRLCoreSight.Patches"
Cohesion: 0.25
Nodes (7): BaseUnityPlugin, TRLCoreSight.Patches, TRLCoreSight, ManualLogSource, Plugin, Instance, LogSource

### Community 21 - "WeaponShotGCPatch"
Cohesion: 0.29
Nodes (4): MethodBase, PatchPostfix, WeaponManagerClass, WeaponShotGCPatch

### Community 22 - "LootRegisterPatch"
Cohesion: 0.33
Nodes (3): MethodBase, PatchPostfix, LootRegisterPatch

## Knowledge Gaps
- **119 isolated node(s):** `CpuAffinityMode`, `ProcessPriorityHigh`, `EnableFrametimeBenchmark`, `ModEnabled`, `DebugMode` (+114 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 215 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TRLCoreSight.Configuration` connect `TRLCoreSight.Core` to `ShellSpawnCullingPatch`, `PerformanceManager`, `DeclutterManager`, `AmbientAudioCullingManager`, `LocalLightShadowManager`, `ConcealmentVisualizer`, `ModConfig.cs`, `ModulePatch`, `TRLCoreSight.Patches`?**
  _High betweenness centrality (0.332) - this node is a cross-community bridge._
- **Why does `ModConfig` connect `ModConfig` to `ModConfig.cs`, `.Awake`?**
  _High betweenness centrality (0.260) - this node is a cross-community bridge._
- **Why does `DeclutterManager` connect `DeclutterManager` to `PerformanceManager`, `.IsProtectedTarkovComponent`?**
  _High betweenness centrality (0.203) - this node is a cross-community bridge._
- **What connects `CpuAffinityMode`, `ProcessPriorityHigh`, `EnableFrametimeBenchmark` to the rest of the system?**
  _119 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `ModConfig` be split into smaller, more focused modules?**
  _Cohesion score 0.029850746268656716 - nodes in this community are weakly interconnected._
- **Should `PerformanceManager` be split into smaller, more focused modules?**
  _Cohesion score 0.06859903381642513 - nodes in this community are weakly interconnected._
- **Should `DeclutterManager` be split into smaller, more focused modules?**
  _Cohesion score 0.09581646423751687 - nodes in this community are weakly interconnected._
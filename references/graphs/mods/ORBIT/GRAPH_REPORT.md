# Graph Report - modded  (2026-08-22)

## Corpus Check
- 79 files · ~90,565 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1204 nodes · 2982 edges · 65 communities (59 shown, 6 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 70 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `e9224ae0`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .Debug
- WeaponSwapper
- Squad
- WaypointSystem
- MovementSystem
- MethodImpl
- .TryHandleAsync
- Agent
- Waypoint
- SquadRegistry
- BotTypeUtils
- CoverPoint
- AgentComponents.cs
- Orbit.Entities
- WaypointConfig
- Entity
- ILootHandler
- MovementFixes.cs
- .Generate
- LootContainerAction
- .Postfix
- .TryHandleAsync
- .ShouldBypassForOrbitBot
- OrbitTelemetry.cs
- GotoObjectiveAction
- Plugin
- Orbit.Helpers
- .Info
- Orbit.Core
- OrbitBrainLayer
- EntityStorage.cs
- Task
- GuardAction
- DoorSystem
- .DelayedLoad
- StrategyManager
- SquadObjective
- DoorNavMesh
- PathHelper
- ComponentArray
- VersionLabelPatch
- HandbookPriceCache
- NavJob
- OrbitManager
- LookSystem
- IdleAction
- AirdropLandedPatch
- ConfigurationManagerAttributes
- RescueInterceptPatch
- ModulePatch
- PositionHistory
- .Postfix
- DoorUnlockTracePatch
- .HandleDoors
- Task.cs
- Comparer
- MiscHelpers.cs
- PerfMonitor
- InventoryChangePatch
- .OnDead
- PersonalityFallback
- Orbit.Config
- Orbit.csproj
- .OnLayerChanged
- InterpolatedStringHandlerAttribute.cs

## God Nodes (most connected - your core abstractions)
1. `Agent` - 112 edges
2. `WaypointSystem` - 108 edges
3. `Squad` - 86 edges
4. `Waypoint` - 63 edges
5. `OrbitLootHandler` - 54 edges
6. `MovementSystem` - 51 edges
7. `GotoObjectiveStrategy` - 40 edges
8. `Orbit.Helpers` - 31 edges
9. `OrbitManager` - 27 edges
10. `Orbit.Entities` - 25 edges

## Surprising Connections (you probably didn't know these)
- `OrbitBrainLayer` --references--> `OrbitManager`  [EXTRACTED]
  mods/ORBIT/modded/Orbit/Brain/OrbitBrainLayer.cs → mods/ORBIT/modded/Orbit/Core/OrbitManager.cs
- `OrbitBrainLayer` --references--> `Agent`  [EXTRACTED]
  mods/ORBIT/modded/Orbit/Brain/OrbitBrainLayer.cs → mods/ORBIT/modded/Orbit/Entities/Agent.cs
- `WaypointSystem` --references--> `ConfigBundle`  [EXTRACTED]
  mods/ORBIT/modded/Orbit/Systems/WaypointSystem.cs → mods/ORBIT/modded/Orbit/Config/ConfigBundle.cs
- `BuiltinZone` --references--> `Range`  [EXTRACTED]
  mods/ORBIT/modded/Orbit/Config/WaypointConfig.cs → mods/ORBIT/modded/Orbit/Config/Primitives.cs
- `Convergence` --references--> `Range`  [EXTRACTED]
  mods/ORBIT/modded/Orbit/Config/WaypointConfig.cs → mods/ORBIT/modded/Orbit/Config/Primitives.cs

## Import Cycles
- None detected.

## Communities (65 total, 6 thin omitted)

### Community 0 - ".Debug"
Cohesion: 0.06
Nodes (42): CancellationTokenSource, DrainEntry, GInterface424, InventoryEquipment, item, LootableContainer, LootItem, MonoBehaviour (+34 more)

### Community 1 - "WeaponSwapper"
Cohesion: 0.07
Nodes (43): AmmoItemClass, AmmoSnapshot, IEnumerable, IList, ItemAddress, Item, HeadsetScorer, bool (+35 more)

### Community 2 - "Squad"
Cohesion: 0.07
Nodes (20): bool, Dictionary, float, HashSet, int, List, string, Vector2Int (+12 more)

### Community 3 - "WaypointSystem"
Cohesion: 0.09
Nodes (16): BotsController, Convergence, Dictionary, Door, float, HashSet, int, List (+8 more)

### Community 4 - "MovementSystem"
Cohesion: 0.11
Nodes (19): EBodyPartColliderType, HardStuckRemediation, LayerMask, Dictionary, float, int, List, MethodImpl (+11 more)

### Community 5 - "MethodImpl"
Cohesion: 0.07
Nodes (24): IComparer, NavMeshObstacle, Door, float, List, NavMeshDoorLink, NavMeshPath, string (+16 more)

### Community 6 - ".TryHandleAsync"
Cohesion: 0.11
Nodes (25): float, Item, BackpackScorer, bool, BotOwner, CancellationToken, float, Item (+17 more)

### Community 7 - "Agent"
Cohesion: 0.08
Nodes (20): BifacialTransform, BotOwner, BotOwner, List, MethodImpl, BotRoster, ActionManager, bool (+12 more)

### Community 8 - "Waypoint"
Cohesion: 0.12
Nodes (10): Door, float, int, List, MonoBehaviour, string, Vector3, Waypoint (+2 more)

### Community 9 - "SquadRegistry"
Cohesion: 0.09
Nodes (18): ArchetypeTable, GameWorld, Dictionary, float, WildSpawnType, SquadRegistry, PersonalityArchetype, float (+10 more)

### Community 10 - "BotTypeUtils"
Cohesion: 0.10
Nodes (12): ConfigFile, WildSpawnType, BotType, BotTypeUtils, ExtractFaction, LootingFaction, bool, ConfigEntry (+4 more)

### Community 11 - "CoverPoint"
Cohesion: 0.11
Nodes (16): CoverData, CoverLevel, Exfil, Vector3, CoverCategory, CoverPoint, BotsController, Door (+8 more)

### Community 12 - "AgentComponents.cs"
Cohesion: 0.12
Nodes (20): JobHandle, NativeArray, bool, float, int, List, MethodImpl, Vector3 (+12 more)

### Community 13 - "Orbit.Entities"
Cohesion: 0.23
Nodes (8): Orbit.Tasks.Strategies, Orbit.Systems, Orbit.Sain, Orbit.Navigation, System.Runtime.CompilerServices, Orbit.Entities, Orbit.Tasks, Orbit.Tasks.Actions

### Community 14 - "WaypointConfig"
Cohesion: 0.12
Nodes (17): BuiltinZone, CustomZone, string, ConfigBundle, MethodImpl, Range, Convergence, Dictionary (+9 more)

### Community 15 - "Entity"
Cohesion: 0.17
Nodes (6): IEquatable, float, int, Entity, Vector2, Vector2Int

### Community 16 - "ILootHandler"
Cohesion: 0.12
Nodes (11): BotOwner, InteractableObject, Vector3, ILootHandler, bool, float, int, LootStats (+3 more)

### Community 17 - "MovementFixes.cs"
Cohesion: 0.15
Nodes (10): BotMover, MethodBase, PatchPrefix, Player, Vector3, BotVaultingPatch, HardTeleportTracePatch, ManualFixedUpdateSkipPatch (+2 more)

### Community 18 - ".Generate"
Cohesion: 0.17
Nodes (11): bool, float, string, Vector2Int, Vector3, MainObjective, MainObjectiveType, List (+3 more)

### Community 19 - "LootContainerAction"
Cohesion: 0.19
Nodes (3): Dictionary, int, LootContainerAction

### Community 20 - ".Postfix"
Cohesion: 0.13
Nodes (9): AICoreControllerClass, GameObject, DangerZoneCarver, BotsController, MethodBase, PatchPostfix, OrbitDisposePatch, OrbitInitPatch (+1 more)

### Community 21 - ".TryHandleAsync"
Cohesion: 0.18
Nodes (13): float, Item, ArmorScorer, bool, BotOwner, CancellationToken, EquipmentSlot, float (+5 more)

### Community 22 - ".ShouldBypassForOrbitBot"
Cohesion: 0.15
Nodes (10): BaseLogicLayerAbstractClass, GClass45, GClass75, GClass79, MethodBase, PatchPrefix, AssaultEnemyFarBypassPatch, BypassGate (+2 more)

### Community 23 - "OrbitTelemetry.cs"
Cohesion: 0.24
Nodes (13): Orbit.Api, bool, float, int, List, string, OrbitBotObjective, OrbitFieldCell (+5 more)

### Community 24 - "GotoObjectiveAction"
Cohesion: 0.21
Nodes (5): Dictionary, ExfiltrationPoint, float, Vector3, GotoObjectiveAction

### Community 25 - "Plugin"
Cohesion: 0.17
Nodes (8): BaseUnityPlugin, EventArgs, ManualLogSource, Action, ConfigEntry, string, Vector2, Plugin

### Community 26 - "Orbit.Helpers"
Cohesion: 0.17
Nodes (3): Orbit.Helpers, Orbit.Looting, Orbit.Looting.WeaponSwap

### Community 27 - ".Info"
Cohesion: 0.26
Nodes (5): Dictionary, ExfiltrationPoint, float, ExtractAction, VExState

### Community 28 - "Orbit.Core"
Cohesion: 0.19
Nodes (6): Orbit.Patches, Orbit.Core, Orbit.Brain, Orbit, Orbit.Interop, BsgBrain

### Community 29 - "OrbitBrainLayer"
Cohesion: 0.15
Nodes (9): CustomLayer, Action, bool, Collider, float, HashSet, string, StringBuilder (+1 more)

### Community 30 - "EntityStorage.cs"
Cohesion: 0.21
Nodes (7): Dictionary, List, Dataset, EntityArray, SquadArray, SquadData, Stack

### Community 31 - "Task"
Cohesion: 0.15
Nodes (6): Dictionary, DefinitionRegistry, HashSet, List, Task, ValueCollection

### Community 32 - "GuardAction"
Cohesion: 0.19
Nodes (7): MovementUrgency, SprintGate, float, int, List, GuardAction, Task

### Community 33 - "DoorSystem"
Cohesion: 0.27
Nodes (7): EDoorState, Collider, Door, HashSet, List, WorldInteractiveObject, DoorSystem

### Community 35 - "StrategyManager"
Cohesion: 0.36
Nodes (3): MethodImpl, BaseTaskManager, StrategyManager

### Community 36 - "SquadObjective"
Cohesion: 0.24
Nodes (8): bool, float, List, Vector3, Objective, ObjectiveStatus, SquadObjective, SquadObjectiveState

### Community 37 - "DoorNavMesh"
Cohesion: 0.29
Nodes (5): Dictionary, Door, HashSet, NavMeshDoorLink, DoorNavMesh

### Community 38 - "PathHelper"
Cohesion: 0.25
Nodes (4): Vector2, Vector3, PathHelper, NavMeshDoorLink

### Community 40 - "VersionLabelPatch"
Cohesion: 0.22
Nodes (6): Orbit.UI, LocalizedText, MethodBase, PatchPrefix, string, VersionLabelPatch

### Community 41 - "HandbookPriceCache"
Cohesion: 0.42
Nodes (3): JToken, Dictionary, HandbookPriceCache

### Community 42 - "NavJob"
Cohesion: 0.28
Nodes (5): NavMeshPathStatus, Queue, Vector3, NavJob, NavJobExecutor

### Community 43 - "OrbitManager"
Cohesion: 0.25
Nodes (7): float, List, string, OrbitManager, RegisterActionsDelegate, RegisterComponentsDelegate, RegisterStrategiesDelegate

### Community 44 - "LookSystem"
Cohesion: 0.36
Nodes (5): float, List, MethodImpl, Vector3, LookSystem

### Community 45 - "IdleAction"
Cohesion: 0.25
Nodes (4): ActionData, CustomLogic, BotOwner, IdleAction

### Community 46 - "AirdropLandedPatch"
Cohesion: 0.25
Nodes (5): AirdropLogicClass, Action, MethodBase, PatchPostfix, AirdropLandedPatch

### Community 47 - "ConfigurationManagerAttributes"
Cohesion: 0.25
Nodes (8): ConfigEntryBase&gt;, CustomHotkeyDrawerFunc, Func, bool, int, object, string, ConfigurationManagerAttributes

### Community 48 - "RescueInterceptPatch"
Cohesion: 0.25
Nodes (6): BotMover, float, MethodBase, PatchPrefix, Vector3, RescueInterceptPatch

### Community 49 - "ModulePatch"
Cohesion: 0.29
Nodes (4): ModulePatch, MethodBase, PatchPostfix, DoorCarverShrinkPatch

### Community 50 - "PositionHistory"
Cohesion: 0.38
Nodes (4): int, MethodImpl, Vector3, PositionHistory

### Community 51 - ".Postfix"
Cohesion: 0.29
Nodes (5): Corpse, MethodBase, PatchPostfix, Player, CorpseRegistrationPatch

### Community 52 - "DoorUnlockTracePatch"
Cohesion: 0.29
Nodes (4): MethodBase, PatchPostfix, WorldInteractiveObject, DoorUnlockTracePatch

### Community 53 - ".HandleDoors"
Cohesion: 0.33
Nodes (3): Door, string, DoorOpenWatch

### Community 54 - "Task.cs"
Cohesion: 0.33
Nodes (4): float, int, BaseTask, TaskAssignment

### Community 55 - "Comparer"
Cohesion: 0.47
Nodes (5): Comparer, MethodImpl, Vector3, Comparer, ValueTuple

### Community 56 - "MiscHelpers.cs"
Cohesion: 0.33
Nodes (3): InventoryController, ControllerExtensions, TraderControllerClass

### Community 57 - "PerfMonitor"
Cohesion: 0.40
Nodes (3): float, int, PerfMonitor

### Community 58 - "InventoryChangePatch"
Cohesion: 0.33
Nodes (4): Item, MethodBase, PatchPostfix, InventoryChangePatch

### Community 59 - ".OnDead"
Cohesion: 0.40
Nodes (4): DamageInfoStruct, IPlayer, EBodyPart, Player

### Community 60 - "PersonalityFallback"
Cohesion: 0.40
Nodes (4): float, int, Vector2, PersonalityFallback

## Knowledge Gaps
- **9 isolated node(s):** `Orbit.Api`, `BsgBrain`, `Outcome`, `Outcome`, `Outcome` (+4 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **6 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Agent` connect `Agent` to `Squad`, `WaypointSystem`, `MovementSystem`, `Waypoint`, `SquadRegistry`, `CoverPoint`, `AgentComponents.cs`, `Orbit.Entities`, `Entity`, `ILootHandler`, `LootContainerAction`, `GotoObjectiveAction`, `.Info`, `OrbitBrainLayer`, `Task`, `GuardAction`, `SquadObjective`, `PathHelper`, `NavJob`, `LookSystem`, `.HandleDoors`?**
  _High betweenness centrality (0.147) - this node is a cross-community bridge._
- **Why does `Orbit.Helpers` connect `Orbit.Helpers` to `DoorNavMesh`, `PathHelper`, `BotTypeUtils`, `AgentComponents.cs`, `Orbit.Entities`, `PositionHistory`, `Task.cs`, `MiscHelpers.cs`, `PerfMonitor`, `Orbit.Core`?**
  _High betweenness centrality (0.101) - this node is a cross-community bridge._
- **Why does `WaypointSystem` connect `WaypointSystem` to `.Debug`, `Squad`, `MovementSystem`, `MethodImpl`, `Waypoint`, `SquadRegistry`, `OrbitManager`, `CoverPoint`, `Orbit.Entities`, `WaypointConfig`, `Entity`, `.Generate`, `LootContainerAction`, `GotoObjectiveAction`?**
  _High betweenness centrality (0.085) - this node is a cross-community bridge._
- **What connects `Orbit.Api`, `BsgBrain`, `Outcome` to the rest of the system?**
  _9 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.Debug` be split into smaller, more focused modules?**
  _Cohesion score 0.057195149851292613 - nodes in this community are weakly interconnected._
- **Should `WeaponSwapper` be split into smaller, more focused modules?**
  _Cohesion score 0.07392607392607392 - nodes in this community are weakly interconnected._
- **Should `Squad` be split into smaller, more focused modules?**
  _Cohesion score 0.07477288609364081 - nodes in this community are weakly interconnected._
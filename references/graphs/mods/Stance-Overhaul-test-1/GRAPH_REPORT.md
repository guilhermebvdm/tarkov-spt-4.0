# Graph Report - mods\Stance-Overhaul-test-1\modded  (2026-07-16)

## Corpus Check
- 44 files · ~31,383 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 485 nodes · 700 edges · 46 communities (32 shown, 14 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `ef101906`
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
- [[_COMMUNITY_Community 7|Community 7]]
- [[_COMMUNITY_Community 8|Community 8]]
- [[_COMMUNITY_Community 9|Community 9]]
- [[_COMMUNITY_Community 10|Community 10]]
- [[_COMMUNITY_Community 11|Community 11]]
- [[_COMMUNITY_Community 12|Community 12]]
- [[_COMMUNITY_Community 13|Community 13]]
- [[_COMMUNITY_Community 14|Community 14]]
- [[_COMMUNITY_Community 15|Community 15]]
- [[_COMMUNITY_Community 16|Community 16]]
- [[_COMMUNITY_Community 17|Community 17]]
- [[_COMMUNITY_Community 18|Community 18]]
- [[_COMMUNITY_Community 19|Community 19]]
- [[_COMMUNITY_Community 20|Community 20]]
- [[_COMMUNITY_Community 21|Community 21]]
- [[_COMMUNITY_Community 22|Community 22]]
- [[_COMMUNITY_Community 23|Community 23]]
- [[_COMMUNITY_Community 24|Community 24]]
- [[_COMMUNITY_Community 25|Community 25]]
- [[_COMMUNITY_Community 28|Community 28]]
- [[_COMMUNITY_Community 29|Community 29]]
- [[_COMMUNITY_Community 30|Community 30]]
- [[_COMMUNITY_Community 31|Community 31]]
- [[_COMMUNITY_Community 32|Community 32]]
- [[_COMMUNITY_Community 33|Community 33]]
- [[_COMMUNITY_Community 34|Community 34]]
- [[_COMMUNITY_Community 35|Community 35]]
- [[_COMMUNITY_Community 36|Community 36]]
- [[_COMMUNITY_Community 37|Community 37]]
- [[_COMMUNITY_Community 38|Community 38]]
- [[_COMMUNITY_Community 39|Community 39]]
- [[_COMMUNITY_Community 40|Community 40]]

## God Nodes (most connected - your core abstractions)
1. `StanceController` - 73 edges
2. `StanceInputHandler` - 30 edges
3. `StanceBase` - 24 edges
4. `MethodBase` - 22 edges
5. `StanceInputListener` - 17 edges
6. `InputHookPipeline` - 15 edges
7. `UpdateHipInaccuracyPatch` - 15 edges
8. `FieldInfo` - 15 edges
9. `StanceState` - 14 edges
10. `StanceStaminaHandler` - 13 edges

## Surprising Connections (you probably didn't know these)
- `StanceMovementHandler` --implements--> `IControllerHelper`  [EXTRACTED]
  src/Handlers/StanceMovementHandler.cs → src/Handlers/IControllerHelper.cs
- `StanceStaminaHandler` --implements--> `IControllerHelper`  [EXTRACTED]
  src/Handlers/StanceStaminaHandler.cs → src/Handlers/IControllerHelper.cs
- `TacSprintHandler` --implements--> `IControllerHelper`  [EXTRACTED]
  src/Handlers/TacSprintHandler.cs → src/Handlers/IControllerHelper.cs
- `InputHookPipeline` --implements--> `IControllerHelper`  [EXTRACTED]
  src/Handlers/Input/InputHookPipeline.cs → src/Handlers/IControllerHelper.cs
- `StanceInputHandler` --implements--> `IControllerHelper`  [EXTRACTED]
  src/Handlers/Input/StanceInputHandler.cs → src/Handlers/IControllerHelper.cs

## Import Cycles
- None detected.

## Communities (46 total, 14 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.05
Nodes (16): IStance, EStanceType, Vector3Curve, EStanceType, Vector3Curve, EStanceType, Vector3Curve, EStanceType (+8 more)

### Community 1 - "Community 1"
Cohesion: 0.06
Nodes (16): StanceController, InputHookPipeline, MonoBehaviour, bool, EStanceType, FieldInfo, float, FloatMultiplierHandle (+8 more)

### Community 2 - "Community 2"
Cohesion: 0.08
Nodes (10): StanceAimHandler, StanceOverhaul.Handlers.Aiming, BoolGateHandle, IControllerHelper, StanceOverhaul.Handlers, StanceInputListener, StanceOverhaul.Handlers.StanceInput, bool (+2 more)

### Community 3 - "Community 3"
Cohesion: 0.09
Nodes (6): StanceInputHandler, StanceOverhaul.Handlers.StanceInput, bool, EStanceType, IStance, StanceState

### Community 4 - "Community 4"
Cohesion: 0.14
Nodes (8): MethodBase, ChangePosePatch, ChangeScopePatch, InitTransformsPatch, ShouldMoveWeapCloserPatch, UpdateWeaponVariablesPatch, WeaponOverlappingPatch, FieldInfo

### Community 5 - "Community 5"
Cohesion: 0.18
Nodes (4): InputHookPipeline, StanceOverhaul.Controllers.PatchHooks, InputContext, IDisposable

### Community 6 - "Community 6"
Cohesion: 0.17
Nodes (6): bool, IStance, Vector3, StanceSlot, StanceOverhaul.State, StanceState

### Community 7 - "Community 7"
Cohesion: 0.28
Nodes (5): Dictionary, FirearmController, Player, ProceduralWeaponAnimation, Vector3

### Community 8 - "Community 8"
Cohesion: 0.13
Nodes (10): AdsAnimator, StanceOverhaul.Handlers, ExtraDetailsAnimator, StanceOverhaul.Controllers.StanceControllers, IdleAnimator, StanceOverhaul.SpringAnimators, ISpringAnimator, StanceOverhaul.SpringAnimators (+2 more)

### Community 9 - "Community 9"
Cohesion: 0.20
Nodes (4): StanceStaminaHandler, bool, float, IStance

### Community 10 - "Community 10"
Cohesion: 0.22
Nodes (6): StanceOverhaul.Controllers.StateControllers, TacSprintHandler, bool, float, int, IStance

### Community 11 - "Community 11"
Cohesion: 0.18
Nodes (6): ModulePatch, SpringGetPatch, SpringGetRelativePatch, SpringResetPatch, SpringUpdatePatch, Spring

### Community 12 - "Community 12"
Cohesion: 0.18
Nodes (6): EStanceType, Vector3Curve, EStanceType, Vector3Curve, ActiveAim, ShortStock

### Community 14 - "Community 14"
Cohesion: 0.31
Nodes (4): UpdateHipInaccuracyPatch, FirearmController, List, Transform

### Community 16 - "Community 16"
Cohesion: 0.18
Nodes (6): EFireMode, FirearmsAnimator, MovementState, SetFireModePatch, WeaponOverlapViewPatch, PatchPrefix

### Community 17 - "Community 17"
Cohesion: 0.22
Nodes (4): StanceMovementHandler, StanceOverhaul.Controllers.StateControllers, FloatMultiplierHandle, IStance

### Community 18 - "Community 18"
Cohesion: 0.24
Nodes (5): BaseUnityPlugin, Player, Plugin, StanceOverhaul, StanceController

### Community 19 - "Community 19"
Cohesion: 0.29
Nodes (3): WeaponLengthPatch, PatchPostfix, ProceduralWeaponAnimation

### Community 20 - "Community 20"
Cohesion: 0.24
Nodes (3): EStanceType, IStance, StanceOverhaul.Stances

### Community 21 - "Community 21"
Cohesion: 0.22
Nodes (8): ConfigEntryBase>, Func<object, string>, Func<string, object>, object, ConfigurationManagerAttributes, bool, int, string

### Community 22 - "Community 22"
Cohesion: 0.25
Nodes (6): MaterialType, CollisionPatch, ZeroAdjustmentsPatch, PropertyInfo, int, Vector3

### Community 23 - "Community 23"
Cohesion: 0.28
Nodes (5): DisableAimOnReloadPatch, OperateStationaryWeaponPatch, StanceOverhaul.Patches, ReloadClass, Player

### Community 24 - "Community 24"
Cohesion: 0.25
Nodes (4): IStance, StanceState, Vector3, StanceSlot

### Community 28 - "Community 28"
Cohesion: 0.33
Nodes (4): MountingAndCollisionPatch, SetTiltPatch, bool, float

### Community 29 - "Community 29"
Cohesion: 0.40
Nodes (3): ConfigFile, PluginConfig, StanceOverhaul

## Knowledge Gaps
- **93 isolated node(s):** `netstandard2.1`, `Microsoft.NET.Sdk`, `bool`, `ConfigEntryBase>`, `string` (+88 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **14 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `IControllerHelper` connect `Community 2` to `Community 3`, `Community 5`, `Community 6`, `Community 9`, `Community 10`, `Community 17`?**
  _High betweenness centrality (0.132) - this node is a cross-community bridge._
- **Why does `InputHookPipeline` connect `Community 5` to `Community 2`?**
  _High betweenness centrality (0.088) - this node is a cross-community bridge._
- **Why does `StanceBase` connect `Community 0` to `Community 12`, `Community 5`?**
  _High betweenness centrality (0.086) - this node is a cross-community bridge._
- **What connects `netstandard2.1`, `Microsoft.NET.Sdk`, `bool` to the rest of the system?**
  _93 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.05053191489361702 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.05897435897435897 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.07954545454545454 - nodes in this community are weakly interconnected._
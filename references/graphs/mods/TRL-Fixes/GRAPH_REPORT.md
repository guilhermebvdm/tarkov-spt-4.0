# Graph Report - modded  (2026-08-16)

## Corpus Check
- 12 files · ~4,539 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 110 nodes · 128 edges · 12 communities (11 shown, 1 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `623d3a4b`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- ModulePatch
- BotWeaponManagerSafetyPatch
- TRLFixes.Patches
- FikaMainThreadUISafetyPatch
- PickupAimingSafetyPatch
- FikaRefreshSlotViewsSafetyPatch
- DynamicMapsSafetyPatch
- FikaProceedEmptyHandsSafetyPatch
- Plugin
- .PatchPrefix
- FlashbangRadiusPatch
- TRLFixes.csproj

## God Nodes (most connected - your core abstractions)
1. `TRLFixes.Patches` - 10 edges
2. `BotWeaponManagerSafetyPatch` - 9 edges
3. `FikaProceedEmptyHandsSafetyPatch` - 9 edges
4. `FikaMainThreadUISafetyPatch` - 6 edges
5. `FikaRefreshSlotViewsSafetyPatch` - 6 edges
6. `PickupAimingSafetyPatch` - 6 edges
7. `BotMountWeaponFixPatch` - 4 edges
8. `GClass81ShallUseNowPatch` - 4 edges
9. `BotStationaryWeaponDataMethod4Patch` - 4 edges
10. `FikaPlayerOperateStationaryWeaponPatch` - 4 edges

## Surprising Connections (you probably didn't know these)
- `DynamicMapsSafetyPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  mods/TRL-Fixes/modded/Patches/DynamicMapsSafetyPatch.cs →   _Bridges community 0 → community 6_
- `FikaMainThreadUISafetyPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  mods/TRL-Fixes/modded/Patches/FikaMainThreadUISafetyPatch.cs →   _Bridges community 0 → community 3_

## Import Cycles
- None detected.

## Communities (12 total, 1 thin omitted)

### Community 0 - "ModulePatch"
Cohesion: 0.19
Nodes (9): ExUsecBrainClass, ModulePatch, MethodBase, BotMountWeaponFixPatch, BotStationaryWeaponDataMethod4Patch, FikaPlayerOperateStationaryWeaponPatch, GClass81ShallUseNowPatch, PlayerOperateStationaryWeaponPatch (+1 more)

### Community 1 - "BotWeaponManagerSafetyPatch"
Cohesion: 0.20
Nodes (7): BotWeaponManager, BotWeaponSelector, IHandsController, Exception, float, int, BotWeaponManagerSafetyPatch

### Community 2 - "TRLFixes.Patches"
Cohesion: 0.18
Nodes (4): TRLFixes.Patches, FieldInfo, FixFikaReviveRagdollPatch, FlashbangBotPatch

### Community 3 - "FikaMainThreadUISafetyPatch"
Cohesion: 0.18
Nodes (7): Action, Exception, MethodBase, MethodInfo, PatchFinalizer, PatchPrefix, FikaMainThreadUISafetyPatch

### Community 4 - "PickupAimingSafetyPatch"
Cohesion: 0.25
Nodes (5): bool, Exception, float, int, PickupAimingSafetyPatch

### Community 5 - "FikaRefreshSlotViewsSafetyPatch"
Cohesion: 0.25
Nodes (5): ConstructorInfo, FieldInfo, Player, Type, FikaRefreshSlotViewsSafetyPatch

### Community 6 - "DynamicMapsSafetyPatch"
Cohesion: 0.29
Nodes (4): Exception, MethodBase, PatchFinalizer, DynamicMapsSafetyPatch

### Community 7 - "FikaProceedEmptyHandsSafetyPatch"
Cohesion: 0.22
Nodes (7): byte, FieldInfo, int, MethodInfo, Type, FikaProceedEmptyHandsSafetyPatch, PropertyInfo

### Community 8 - "Plugin"
Cohesion: 0.33
Nodes (4): BaseUnityPlugin, TRLFixes, ManualLogSource, Plugin

### Community 9 - ".PatchPrefix"
Cohesion: 0.24
Nodes (7): BotStationaryWeaponData, EStationaryCommand, FirearmController, GClass81, PatchPrefix, Player, StationaryWeapon

### Community 10 - "FlashbangRadiusPatch"
Cohesion: 0.33
Nodes (3): IExplosiveItem, FlashbangRadiusPatch, Vector3

## Knowledge Gaps
- **3 isolated node(s):** `TRLFixes`, `net472`, `Microsoft.NET.Sdk`
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TRLFixes.Patches` connect `TRLFixes.Patches` to `ModulePatch`, `BotWeaponManagerSafetyPatch`, `FikaMainThreadUISafetyPatch`, `PickupAimingSafetyPatch`, `FikaRefreshSlotViewsSafetyPatch`, `DynamicMapsSafetyPatch`, `FlashbangRadiusPatch`?**
  _High betweenness centrality (0.641) - this node is a cross-community bridge._
- **Why does `FikaMainThreadUISafetyPatch` connect `FikaMainThreadUISafetyPatch` to `ModulePatch`?**
  _High betweenness centrality (0.148) - this node is a cross-community bridge._
- **What connects `TRLFixes`, `net472`, `Microsoft.NET.Sdk` to the rest of the system?**
  _3 weakly-connected nodes found - possible documentation gaps or missing edges._
# Graph Report - modded  (2026-08-16)

## Corpus Check
- 22 files · ~5,925 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 307 nodes · 493 edges · 19 communities
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 17 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `623d3a4b`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- ModulePatch
- LoadAmmoController
- LoadAmmoUI
- LoadAmmoComponent
- MagazinePresetLoader
- ContinuousLoadAmmo.Utils
- CommonUtils
- ProfileMagazinePresetStore
- .Prefix
- .Postfix
- .Postfix
- ConfigurationManagerAttributes
- .Prefix
- MultiSelectInterop
- ContinuousLoadAmmo
- OnClickPatch
- SetSubInteractionsPatch
- InventoryScreenClosePatch

## God Nodes (most connected - your core abstractions)
1. `LoadAmmoController` - 36 edges
2. `LoadAmmoComponent` - 27 edges
3. `LoadAmmoUI` - 22 edges
4. `ContinuousLoadAmmo.Utils` - 15 edges
5. `MagazinePresetLoader` - 14 edges
6. `ContinuousLoadAmmo.Patches` - 13 edges
7. `ScreensPatches` - 11 edges
8. `CommonUtils` - 11 edges
9. `ConfigurationManagerAttributes` - 7 edges
10. `MultiSelectInterop` - 7 edges

## Surprising Connections (you probably didn't know these)
- `LoadAmmoComponent` --references--> `LoadAmmoController`  [EXTRACTED]
  mods/SPT-ContinuousLoadAmmo/modded/Components/LoadAmmoComponent.cs → mods/SPT-ContinuousLoadAmmo/modded/Controllers/LoadAmmoController.cs
- `LoadAmmoController` --references--> `MagazinePresetLoader`  [EXTRACTED]
  mods/SPT-ContinuousLoadAmmo/modded/Controllers/LoadAmmoController.cs → mods/SPT-ContinuousLoadAmmo/modded/Controllers/MagazinePresetLoader.cs
- `LoadAmmoUI` --references--> `LoadAmmoController`  [EXTRACTED]
  mods/SPT-ContinuousLoadAmmo/modded/Controllers/LoadAmmoUI.cs → mods/SPT-ContinuousLoadAmmo/modded/Controllers/LoadAmmoController.cs
- `RegisterPlayerPatch` --references--> `LoadAmmoUI`  [EXTRACTED]
  mods/SPT-ContinuousLoadAmmo/modded/Patches/RegisterPlayerPatch.cs → mods/SPT-ContinuousLoadAmmo/modded/Controllers/LoadAmmoUI.cs

## Import Cycles
- None detected.

## Communities (19 total, 0 thin omitted)

### Community 0 - "ModulePatch"
Cohesion: 0.09
Nodes (18): ContextInteractionSwitcherClass, ModulePatch, EnableContextPresetPatch, EItemInfoButton, MethodBase, PatchPrefix, ItemsPanelShowPatch, MapScreenPatch (+10 more)

### Community 1 - "LoadAmmoController"
Cohesion: 0.10
Nodes (16): AbstractHandsController, LoadAmmoController, AmmoItemClass, bool, CancellationToken, EquipmentSlot, GClass3248, IPlayer (+8 more)

### Community 2 - "LoadAmmoUI"
Cohesion: 0.08
Nodes (17): Action, LoadAmmoUI, FieldRef, Item, Transform, GameObject, GameWorld, GClass929 (+9 more)

### Community 3 - "LoadAmmoComponent"
Cohesion: 0.10
Nodes (15): LoadAmmoComponent, AmmoItemClass, FieldRef, int, InventoryController, List, Task, ECommand (+7 more)

### Community 4 - "MagazinePresetLoader"
Cohesion: 0.18
Nodes (13): CancellationTokenSource, MagazinePresetLoader, AmmoItemClass, CancellationToken, List, MagazineBuildPresetClass, MagazineItemClass, MongoID (+5 more)

### Community 5 - "ContinuousLoadAmmo.Utils"
Cohesion: 0.17
Nodes (7): ContinuousLoadAmmo.Patches, ContinuousLoadAmmo.Utils, ContinuousLoadAmmo.Components, ContinuousLoadAmmo.Models, ContinuousLoadAmmo.Controllers, ContinuousLoadAmmo, QuickLoadMode

### Community 6 - "CommonUtils"
Cohesion: 0.11
Nodes (13): InputTree, InventoryEquipment, TraderControllerClass, CommonUtils, AmmoItemClass, EquipmentSlot, GClass3248, List (+5 more)

### Community 7 - "ProfileMagazinePresetStore"
Cohesion: 0.15
Nodes (11): BaseUnityPlugin, ConfigEntry, ContinuousLoadAmmo, Dictionary, ManualLogSource, CaliberLastPreset, ProfileLastMagPresets, ProfileMagazinePresetStore (+3 more)

### Community 8 - ".Prefix"
Cohesion: 0.18
Nodes (9): GStruct155, IReadOnlyCollection, ItemUiContext, ApplyMagPresetPatch, MagazineBuildPresetClass, MagazineItemClass, MethodBase, PatchPrefix (+1 more)

### Community 9 - ".Postfix"
Cohesion: 0.22
Nodes (6): Class1204, LoadMagazineStartPatch, IResult, MethodBase, PatchPostfix, Task

### Community 10 - ".Postfix"
Cohesion: 0.22
Nodes (6): Class1207, UnloadMagazineStartPatch, IResult, MethodBase, PatchPostfix, Task

### Community 11 - "ConfigurationManagerAttributes"
Cohesion: 0.25
Nodes (7): ConfigEntryBase&gt;, ConfigurationManagerAttributes, bool, Func, int, string, object

### Community 12 - ".Prefix"
Cohesion: 0.25
Nodes (6): GClass3757, ISubInteractions, PresetSubInteractionsPatch, EItemInfoButton, MethodBase, PatchPrefix

### Community 13 - "MultiSelectInterop"
Cohesion: 0.25
Nodes (6): MethodInfo, MultiSelectInterop, bool, FieldRef, Func, Version

### Community 14 - "ContinuousLoadAmmo"
Cohesion: 0.29
Nodes (7): ContinuousLoadAmmo, netstandard2.1, Microsoft.CodeAnalysis.NetAnalyzers (10.0.*-*), Microsoft.Unity.Analyzers (1.25.*-*), Microsoft.VisualStudio.Threading.Analyzers (17.14.*-*), RoR2.BepInEx.Analyzers (1.0.*-*), Microsoft.NET.Sdk

### Community 15 - "OnClickPatch"
Cohesion: 0.29
Nodes (5): InputButton, ItemView, OnClickPatch, MethodBase, PatchPrefix

### Community 16 - "SetSubInteractionsPatch"
Cohesion: 0.33
Nodes (4): GClass3829, SetSubInteractionsPatch, MethodBase, PatchPrefix

### Community 17 - "InventoryScreenClosePatch"
Cohesion: 0.33
Nodes (4): InventoryScreenClosePatch, InventoryController, MethodBase, PatchPrefix

## Knowledge Gaps
- **8 isolated node(s):** `ContinuousLoadAmmo`, `netstandard2.1`, `Microsoft.Unity.Analyzers (1.25.*-*)`, `Microsoft.CodeAnalysis.NetAnalyzers (10.0.*-*)`, `Microsoft.VisualStudio.Threading.Analyzers (17.14.*-*)` (+3 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `LoadAmmoController` connect `LoadAmmoController` to `LoadAmmoUI`, `LoadAmmoComponent`, `MagazinePresetLoader`, `ContinuousLoadAmmo.Utils`?**
  _High betweenness centrality (0.281) - this node is a cross-community bridge._
- **Why does `ContinuousLoadAmmo.Utils` connect `ContinuousLoadAmmo.Utils` to `MultiSelectInterop`, `CommonUtils`, `ProfileMagazinePresetStore`?**
  _High betweenness centrality (0.243) - this node is a cross-community bridge._
- **Why does `LoadAmmoUI` connect `LoadAmmoUI` to `LoadAmmoController`, `ContinuousLoadAmmo.Utils`?**
  _High betweenness centrality (0.184) - this node is a cross-community bridge._
- **What connects `ContinuousLoadAmmo`, `netstandard2.1`, `Microsoft.Unity.Analyzers (1.25.*-*)` to the rest of the system?**
  _8 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `ModulePatch` be split into smaller, more focused modules?**
  _Cohesion score 0.08780487804878048 - nodes in this community are weakly interconnected._
- **Should `LoadAmmoController` be split into smaller, more focused modules?**
  _Cohesion score 0.1021021021021021 - nodes in this community are weakly interconnected._
- **Should `LoadAmmoUI` be split into smaller, more focused modules?**
  _Cohesion score 0.08143939393939394 - nodes in this community are weakly interconnected._
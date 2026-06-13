# Graph Report - mods\CustomClasses\modded  (2026-06-13)

## Corpus Check
- 78 files · ~62,972 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1178 nodes · 1453 edges · 76 communities (69 shown, 7 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS · INFERRED: 1 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `51cc7244`
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
- [[_COMMUNITY_Community 26|Community 26]]
- [[_COMMUNITY_Community 27|Community 27]]
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
- [[_COMMUNITY_Community 41|Community 41]]
- [[_COMMUNITY_Community 42|Community 42]]
- [[_COMMUNITY_Community 43|Community 43]]
- [[_COMMUNITY_Community 44|Community 44]]
- [[_COMMUNITY_Community 45|Community 45]]
- [[_COMMUNITY_Community 46|Community 46]]
- [[_COMMUNITY_Community 47|Community 47]]
- [[_COMMUNITY_Community 48|Community 48]]
- [[_COMMUNITY_Community 49|Community 49]]
- [[_COMMUNITY_Community 50|Community 50]]
- [[_COMMUNITY_Community 51|Community 51]]
- [[_COMMUNITY_Community 52|Community 52]]
- [[_COMMUNITY_Community 53|Community 53]]
- [[_COMMUNITY_Community 54|Community 54]]
- [[_COMMUNITY_Community 55|Community 55]]
- [[_COMMUNITY_Community 56|Community 56]]
- [[_COMMUNITY_Community 57|Community 57]]
- [[_COMMUNITY_Community 58|Community 58]]
- [[_COMMUNITY_Community 59|Community 59]]
- [[_COMMUNITY_Community 60|Community 60]]
- [[_COMMUNITY_Community 61|Community 61]]
- [[_COMMUNITY_Community 62|Community 62]]
- [[_COMMUNITY_Community 63|Community 63]]
- [[_COMMUNITY_Community 64|Community 64]]
- [[_COMMUNITY_Community 65|Community 65]]
- [[_COMMUNITY_Community 66|Community 66]]
- [[_COMMUNITY_Community 67|Community 67]]
- [[_COMMUNITY_Community 68|Community 68]]
- [[_COMMUNITY_Community 73|Community 73]]
- [[_COMMUNITY_Community 74|Community 74]]
- [[_COMMUNITY_Community 75|Community 75]]

## God Nodes (most connected - your core abstractions)
1. `CatalogService` - 46 edges
2. `ClassEditorService` - 24 edges
3. `InventoryBuilder` - 23 edges
4. `CostService` - 17 edges
5. `MongoId` - 16 edges
6. `ClassIdentityView` - 12 edges
7. `Item` - 12 edges
8. `MenuOverhaulBridge` - 11 edges
9. `SkillsNavButtonPatch` - 10 edges
10. `Plugin` - 10 edges

## Surprising Connections (you probably didn't know these)
- `MenuClassIdentityPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Client/Patches/MenuClassIdentityPatch.cs →   _Bridges community 40 → community 20_
- `OnTriggerPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Client/Patches/OnTriggerPatch.cs →   _Bridges community 40 → community 53_
- `PlayerModelWithStatsIdentityPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Client/Patches/PlayerModelWithStatsIdentityPatch.cs →   _Bridges community 40 → community 47_
- `PlayerNamePanelPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Client/Patches/PlayerNamePanelPatch.cs →   _Bridges community 40 → community 48_
- `RaidReadyPlayerPanelPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Client/Patches/RaidReadyPlayerPanelPatch.cs →   _Bridges community 40 → community 49_

## Import Cycles
- None detected.

## Communities (76 total, 7 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.10
Nodes (20): HideoutAreas, MapSeverity, OnParametersSet, Reload, ReloadAndRefresh, route:/customclasses/classes/{FileName}/edit, ClassEditorService, ClassWorkspace (+12 more)

### Community 1 - "Community 1"
Cohesion: 0.07
Nodes (25): ISptLogger, ItemHelper, Lazy, LocaleService, Price, SearchIndexRow, CatalogService, CatalogAmmo (+17 more)

### Community 2 - "Community 2"
Cohesion: 0.04
Nodes (51): ClassRegistrar, GearPanel, MudMenuItem, ClearCompare, ClothingLabel, CountSkills, DeltaChip, FormatRub (+43 more)

### Community 3 - "Community 3"
Cohesion: 0.05
Nodes (43): ClassLifecycleCreateDialog, MudTableSortLabel, EditClass, FormatRub, LoadRows, OnAfterRenderAsync, OnInitialized, OnRowClick (+35 more)

### Community 4 - "Community 4"
Cohesion: 0.05
Nodes (41): ModSpecModel, MudButtonGroup, MudCollapse, AmmoPicker, CatalogPreset, CatalogService, CatalogSlotInfo, CustomClasses.Web (+33 more)

### Community 5 - "Community 5"
Cohesion: 0.16
Nodes (17): IEnumerable, Packer, Root, Func, Grid, GridPacker, Item, ItemSpec (+9 more)

### Community 6 - "Community 6"
Cohesion: 0.12
Nodes (15): ConcurrentDictionary, CreateResult, FileStamp, IReadOnlySet, SaveResult, ClassEditorService, ClassDefinition, ClassDiagnostic (+7 more)

### Community 7 - "Community 7"
Cohesion: 0.09
Nodes (19): LocalizedText, Outfit, ClassDefinition, Dictionary, Func, ItemSpec, List, Loadout (+11 more)

### Community 8 - "Community 8"
Cohesion: 0.06
Nodes (32): ClassColumn, BuildOverflowSkills, CategoryHeader, Cell, ClassColumn, CostFooterCell, HeaderTitle, LoadColumns (+24 more)

### Community 9 - "Community 9"
Cohesion: 0.06
Nodes (30): MudNavLink, MudNavMenu, ClassEditorService, CostService, CustomClasses.Web, IJSRuntime, MudDivider, MudIcon (+22 more)

### Community 10 - "Community 10"
Cohesion: 0.14
Nodes (16): LoadoutCostBreakdown, CostService, ClassDefinition, Grid, GridPacker, Height, Item, ItemSpec (+8 more)

### Community 11 - "Community 11"
Cohesion: 0.05
Nodes (38): ActivatorContent, MudSlider, ChildContent, CustomClasses.Web, MudButton, MudChip, MudIconButton, MudMenu (+30 more)

### Community 12 - "Community 12"
Cohesion: 0.08
Nodes (25): CategoryOption, MudProgressLinear, CatalogCategory, CatalogItem, CatalogService, DialogActions, DialogContent, ItemPicker (+17 more)

### Community 13 - "Community 13"
Cohesion: 0.08
Nodes (23): CascadingValue, HeadContent, LayoutComponentBase, CycleDrawer, OnAfterRenderAsync, OnBeforeNavAsync, Reset, MudAppBar (+15 more)

### Community 14 - "Community 14"
Cohesion: 0.10
Nodes (20): CustomizationPicker, MudGrid, MudItem, OpenFilteredDialogAsync, OpenItemDialogAsync, route:/customclasses/picker-test, AmmoPicker, IDialogService (+12 more)

### Community 15 - "Community 15"
Cohesion: 0.15
Nodes (12): bottom, ChatSpecialIcon, Color, FieldInfo, float, GameObject, Image, TextMeshProUGUI (+4 more)

### Community 16 - "Community 16"
Cohesion: 0.11
Nodes (17): EditionRow, EditionRow, OnInitialized, route:/customclasses, DatabaseService, HeaderContent, MudButton, MudChip (+9 more)

### Community 17 - "Community 17"
Cohesion: 0.16
Nodes (11): FieldInfo, float, GameObject, IEnumerator, MenuScreen, MethodBase, PatchPostfix, string (+3 more)

### Community 18 - "Community 18"
Cohesion: 0.15
Nodes (8): BaseMeshEffect, Color, string, MultiplierFormat, Color, List, ClassIconGradient, VertexHelper

### Community 19 - "Community 19"
Cohesion: 0.12
Nodes (15): MudProgressCircular, ClassDiagnostic, ClassEditorService, DialogActions, DialogContent, MudAlert, MudButton, MudDialog (+7 more)

### Community 20 - "Community 20"
Cohesion: 0.16
Nodes (10): Color, IEnumerator, Image, MenuScreen, MethodBase, PatchPostfix, string, TextMeshProUGUI (+2 more)

### Community 21 - "Community 21"
Cohesion: 0.13
Nodes (14): ClassDiagnostic, ClassEditorService, DialogActions, DialogContent, MudAlert, MudButton, MudDialog, MudStack (+6 more)

### Community 22 - "Community 22"
Cohesion: 0.13
Nodes (14): ClassDiagnostic, ClassEditorService, DialogActions, DialogContent, MudAlert, MudButton, MudDialog, MudStack (+6 more)

### Community 23 - "Community 23"
Cohesion: 0.14
Nodes (13): CatalogAmmo, CatalogService, HeaderContent, MudChip, MudStack, MudTable, MudTd, MudText (+5 more)

### Community 24 - "Community 24"
Cohesion: 0.14
Nodes (13): CatalogPreset, CatalogService, HeaderContent, MudChip, MudStack, MudTable, MudTd, MudText (+5 more)

### Community 25 - "Community 25"
Cohesion: 0.15
Nodes (12): ClassViewItemSpec, CatalogService, ClassViewModSpec, MudChip, MudIcon, MudStack, MudText, MudTooltip (+4 more)

### Community 26 - "Community 26"
Cohesion: 0.23
Nodes (8): RegistrationPlan, ClassRegistrar, bool, ClassDefinition, ClassDiagnostic, List, PmcData, string

### Community 27 - "Community 27"
Cohesion: 0.20
Nodes (8): float, GameObject, MethodBase, PatchPostfix, SkillClass, string, TextMeshProUGUI, SkillPanelPatch

### Community 28 - "Community 28"
Cohesion: 0.17
Nodes (9): double, Origin, Dictionary, int, IReadOnlyDictionary, SkillTypes, SkillWeights, SkillWeightOrigin (+1 more)

### Community 29 - "Community 29"
Cohesion: 0.24
Nodes (7): Dictionary, ESkillId, MethodBase, PatchPostfix, PatchPrefix, WorkoutBehaviourPatch, WorkoutBehaviour

### Community 30 - "Community 30"
Cohesion: 0.18
Nodes (10): CustomClasses, CustomClasses.Web.Layouts, CustomClasses.Web.Shared, Microsoft.AspNetCore.Components.Forms, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web.Virtualization, Microsoft.JSInterop, MudBlazor (+2 more)

### Community 31 - "Community 31"
Cohesion: 0.25
Nodes (7): JsonConverter, JsonSerializerOptions, Type, LocalizedText, LocalizedTextConverter, Utf8JsonReader, Utf8JsonWriter

### Community 32 - "Community 32"
Cohesion: 0.25
Nodes (6): List, SkillTypes, string, SkillMaster, SkillCategory, SkillMasterEntry

### Community 33 - "Community 33"
Cohesion: 0.18
Nodes (10): CatalogService, ItemTooltip, LoadoutCostEntry, SPTarkov.Server.Core.Models.Common, System.Globalization, OnParametersSet, StashCell, StashGroup (+2 more)

### Community 34 - "Community 34"
Cohesion: 0.20
Nodes (6): bool, Dictionary, ESkillId, string, Payload, SkillMultipliers

### Community 35 - "Community 35"
Cohesion: 0.20
Nodes (7): bool, Color, string, MethodInfo, object, PropertyInfo, MenuOverhaulBridge

### Community 36 - "Community 36"
Cohesion: 0.24
Nodes (7): Customization, CustomizationItem, IReadOnlyDictionary, MongoId, OutfitSide, OutfitBuilder, TemplateSide

### Community 37 - "Community 37"
Cohesion: 0.20
Nodes (8): ClassVisualRegistry, JsonUtil, RouteAction, SaveServer, List, SkillMultipliersRouter, SkillMultiplierRegistry, StaticRouter

### Community 38 - "Community 38"
Cohesion: 0.20
Nodes (9): CatalogClothing, CatalogService, MudStack, MudText, MudTextField, MudVirtualize, ApplyFilter, OnParametersSet (+1 more)

### Community 39 - "Community 39"
Cohesion: 0.28
Nodes (5): BaseUnityPlugin, bool, Plugin, ConfigEntry, ManualLogSource

### Community 40 - "Community 40"
Cohesion: 0.22
Nodes (6): Image, MethodBase, PatchPostfix, TextMeshProUGUI, ModulePatch, ChatSpecialIconPatch

### Community 41 - "Community 41"
Cohesion: 0.22
Nodes (6): AbstractSkillClass, FieldInfo, MethodBase, PatchPostfix, Type, SkillLevelUpNotificationPatch

### Community 42 - "Community 42"
Cohesion: 0.22
Nodes (8): GearCell, CatalogService, ItemTooltip, MudText, SPTarkov.Server.Core.Models.Common, BuildCell, GearCell, OnParametersSet

### Community 43 - "Community 43"
Cohesion: 0.22
Nodes (5): IOnLoad, Task, CustomClassesMod, Task, HiddenEditionsLoader

### Community 44 - "Community 44"
Cohesion: 0.29
Nodes (5): Rotated, bool, GridPacker, X, Y

### Community 45 - "Community 45"
Cohesion: 0.22
Nodes (8): CatalogService, ClassViewModSpec, ModSpec, MudIcon, MudStack, MudText, SPTarkov.Server.Core.Models.Common, ResolveName

### Community 46 - "Community 46"
Cohesion: 0.44
Nodes (4): IJSRuntime, string, Task, UiPrefs

### Community 47 - "Community 47"
Cohesion: 0.25
Nodes (5): ChatSpecialIcon, MethodBase, PatchPostfix, TextMeshProUGUI, PlayerModelWithStatsIdentityPatch

### Community 48 - "Community 48"
Cohesion: 0.25
Nodes (5): FieldInfo, MethodBase, PatchPostfix, PlayerNamePanelPatch, PlayerNamePanel

### Community 49 - "Community 49"
Cohesion: 0.25
Nodes (5): FieldInfo, MethodBase, PatchPostfix, RaidReadyPlayerPanelPatch, RaidReadyPlayerPanel

### Community 50 - "Community 50"
Cohesion: 0.25
Nodes (5): Image, MethodBase, PatchPostfix, SkillClass, SkillIconBorderPatch

### Community 51 - "Community 51"
Cohesion: 0.25
Nodes (5): MethodBase, PatchPostfix, string, SkillsScreenIdentityPatch, SkillsAndMasteringScreen

### Community 52 - "Community 52"
Cohesion: 0.25
Nodes (3): ClassVisualRegistry, Dictionary, Visual

### Community 53 - "Community 53"
Cohesion: 0.29
Nodes (4): AbstractSkillClass, MethodBase, PatchPrefix, OnTriggerPatch

### Community 54 - "Community 54"
Cohesion: 0.27
Nodes (5): Color, Dictionary, string, Sprite, ClassIconCache

### Community 55 - "Community 55"
Cohesion: 0.33
Nodes (4): FieldInfo, IEnumerator, InventoryScreen, InventoryTabNavigator

### Community 56 - "Community 56"
Cohesion: 0.29
Nodes (6): net9.0, SPTarkov.Common (4.0.2), SPTarkov.DI (4.0.2), SPTarkov.Server.Core (4.0.2), SPTarkov.Server.Web (4.0.2), Microsoft.NET.Sdk.Web

### Community 57 - "Community 57"
Cohesion: 0.29
Nodes (5): HashSet, IReadOnlyList, string, SkillsExtendedCompat, SptMod

### Community 58 - "Community 58"
Cohesion: 0.11
Nodes (17): CatalogClothing, CatalogService, DialogActions, DialogContent, MudButton, MudDialog, MudStack, MudText (+9 more)

### Community 61 - "Community 61"
Cohesion: 0.40
Nodes (4): ChildContent, MudTooltip, System.Globalization, TooltipContent

### Community 63 - "Community 63"
Cohesion: 0.40
Nodes (3): Dictionary, PmcData, HideoutBuilder

### Community 66 - "Community 66"
Cohesion: 0.03
Nodes (57): CharacterDoll, CatalogService, ClassDiagnostic, ClassEditorService, ClassWorkspace, CostService, CustomClasses.Web, IDialogService (+49 more)

### Community 67 - "Community 67"
Cohesion: 0.09
Nodes (22): ItemSpecModel, PlacedCell, CatalogService, CostService, CustomClasses.Web, ItemSpec, ItemTooltip, Microsoft.AspNetCore.Components.Web (+14 more)

### Community 73 - "Community 73"
Cohesion: 0.25
Nodes (7): CatalogService, ItemTooltip, SPTarkov.Server.Core.Models.Common, BuildCell, GearCell, OnParametersSet, ShortLabel

### Community 74 - "Community 74"
Cohesion: 0.20
Nodes (9): CustomClasses.Web, DialogActions, DialogContent, ItemSpecEditor, MudButton, MudDialog, MudText, TitleContent (+1 more)

## Knowledge Gaps
- **768 isolated node(s):** `netstandard2.1`, `Microsoft.NET.Sdk`, `string`, `MethodBase`, `TextMeshProUGUI` (+763 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **7 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SkillPanelPatch` connect `Community 27` to `Community 40`?**
  _High betweenness centrality (0.004) - this node is a cross-community bridge._
- **Why does `RaidReadyPlayerPanelPatch` connect `Community 49` to `Community 40`?**
  _High betweenness centrality (0.003) - this node is a cross-community bridge._
- **Why does `SkillsNavButtonPatch` connect `Community 17` to `Community 40`?**
  _High betweenness centrality (0.002) - this node is a cross-community bridge._
- **What connects `netstandard2.1`, `Microsoft.NET.Sdk`, `string` to the rest of the system?**
  _768 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.09523809523809523 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.07291666666666667 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.038461538461538464 - nodes in this community are weakly interconnected._
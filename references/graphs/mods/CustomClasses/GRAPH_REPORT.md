# Graph Report - mods\CustomClasses\modded  (2026-07-19)

## Corpus Check
- 115 files · ~111,013 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1752 nodes · 2275 edges · 114 communities (104 shown, 10 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS · INFERRED: 2 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `f63e9657`
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
- [[_COMMUNITY_Community 79|Community 79]]
- [[_COMMUNITY_Community 80|Community 80]]
- [[_COMMUNITY_Community 81|Community 81]]
- [[_COMMUNITY_Community 82|Community 82]]
- [[_COMMUNITY_Community 83|Community 83]]
- [[_COMMUNITY_Community 84|Community 84]]
- [[_COMMUNITY_Community 85|Community 85]]
- [[_COMMUNITY_Community 86|Community 86]]
- [[_COMMUNITY_Community 87|Community 87]]
- [[_COMMUNITY_Community 88|Community 88]]
- [[_COMMUNITY_Community 89|Community 89]]
- [[_COMMUNITY_Community 90|Community 90]]
- [[_COMMUNITY_Community 91|Community 91]]
- [[_COMMUNITY_Community 92|Community 92]]
- [[_COMMUNITY_Community 93|Community 93]]
- [[_COMMUNITY_Community 94|Community 94]]
- [[_COMMUNITY_Community 96|Community 96]]
- [[_COMMUNITY_Community 97|Community 97]]
- [[_COMMUNITY_Community 98|Community 98]]
- [[_COMMUNITY_Community 99|Community 99]]
- [[_COMMUNITY_Community 100|Community 100]]
- [[_COMMUNITY_Community 101|Community 101]]
- [[_COMMUNITY_Community 102|Community 102]]
- [[_COMMUNITY_Community 103|Community 103]]
- [[_COMMUNITY_Community 104|Community 104]]
- [[_COMMUNITY_Community 105|Community 105]]
- [[_COMMUNITY_Community 107|Community 107]]
- [[_COMMUNITY_Community 108|Community 108]]
- [[_COMMUNITY_Community 109|Community 109]]
- [[_COMMUNITY_Community 110|Community 110]]
- [[_COMMUNITY_Community 111|Community 111]]
- [[_COMMUNITY_Community 112|Community 112]]
- [[_COMMUNITY_Community 113|Community 113]]

## God Nodes (most connected - your core abstractions)
1. `CatalogService` - 55 edges
2. `ClassEditorService` - 24 edges
3. `InventoryBuilder` - 24 edges
4. `LoadingClassHover` - 20 edges
5. `CostService` - 20 edges
6. `MongoId` - 18 edges
7. `SkillsClassTabPatch` - 16 edges
8. `ClassIdentityView` - 14 edges
9. `PerkLine` - 13 edges
10. `Item` - 13 edges

## Surprising Connections (you probably didn't know these)
- `SurgeryPenaltyPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Client/CombatMedicSurgery.cs →   _Bridges community 113 → community 90_
- `BulwarkPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Client/Patches/BulwarkPatch.cs →   _Bridges community 90 → community 80_
- `CalmSightsPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Client/Patches/CalmSightsPatch.cs →   _Bridges community 90 → community 108_
- `ChatSpecialIconPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Client/Patches/ChatSpecialIconPatch.cs →   _Bridges community 90 → community 40_
- `ChangeEnergyPatch` --inherits--> `ModulePatch`  [EXTRACTED]
  Client/Patches/ClassCombatHealthPatches.cs →   _Bridges community 90 → community 79_

## Import Cycles
- None detected.

## Communities (114 total, 10 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.09
Nodes (21): HideoutAreas, MapSeverity, OnParametersSetAsync, Reload, ReloadAndRefresh, route:/customclasses/classes/{FileName}/edit, ClassEditorService, ClassWorkspace (+13 more)

### Community 1 - "Community 1"
Cohesion: 0.06
Nodes (29): ItemHelper, Lazy, LocaleService, Price, SearchIndexRow, CatalogService, CatalogAmmo, CatalogCategory (+21 more)

### Community 2 - "Community 2"
Cohesion: 0.04
Nodes (52): ClassRegistrar, GearPanel, MudMenuItem, ClearCompare, ClothingLabel, CountSkills, DeltaChip, FormatRub (+44 more)

### Community 3 - "Community 3"
Cohesion: 0.04
Nodes (44): ClassLifecycleCreateDialog, MudTableSortLabel, EditClass, FormatRub, LoadRows, OnAfterRenderAsync, OnInitializedAsync, OnRowClick (+36 more)

### Community 4 - "Community 4"
Cohesion: 0.04
Nodes (46): ModSpecModel, MudButtonGroup, MudCollapse, AmmoPicker, CatalogPreset, CatalogService, CatalogSlotInfo, CustomClasses.Web (+38 more)

### Community 5 - "Community 5"
Cohesion: 0.16
Nodes (17): IEnumerable, Packer, Root, Func, Grid, GridPacker, Item, ItemSpec (+9 more)

### Community 6 - "Community 6"
Cohesion: 0.12
Nodes (15): CreateResult, FileStamp, IReadOnlySet, SaveResult, ClassEditorService, ClassDefinition, ClassDiagnostic, ClassFileEntry (+7 more)

### Community 7 - "Community 7"
Cohesion: 0.09
Nodes (19): LocalizedText, Outfit, ClassDefinition, Dictionary, Func, ItemSpec, List, Loadout (+11 more)

### Community 8 - "Community 8"
Cohesion: 0.06
Nodes (35): ClassColumn, MatrixSkeleton, BuildOverflowSkills, CategoryHeader, Cell, ClassColumn, CostFooterCell, HeaderTitle (+27 more)

### Community 9 - "Community 9"
Cohesion: 0.06
Nodes (31): MudNavLink, MudNavMenu, ClassEditorService, CostService, CustomClasses.Web, IJSRuntime, ListRowsSkeleton, MudDivider (+23 more)

### Community 10 - "Community 10"
Cohesion: 0.13
Nodes (18): Flea, Handbook, LoadoutCostBreakdown, CostService, ClassDefinition, Grid, GridPacker, Height (+10 more)

### Community 11 - "Community 11"
Cohesion: 0.05
Nodes (37): ActivatorContent, MudSlider, ChildContent, CustomClasses.Web, MudButton, MudChip, MudIconButton, MudMenu (+29 more)

### Community 12 - "Community 12"
Cohesion: 0.07
Nodes (29): CategoryOption, CatNodeData, MudProgressLinear, CatalogCategory, CatalogItem, CatalogService, DialogActions, DialogContent (+21 more)

### Community 13 - "Community 13"
Cohesion: 0.07
Nodes (27): CascadingValue, HeadContent, LayoutComponentBase, CycleDrawer, Dispose, OnAfterRenderAsync, OnBeforeNavAsync, OnInitialized (+19 more)

### Community 14 - "Community 14"
Cohesion: 0.10
Nodes (20): CustomizationPicker, MudGrid, MudItem, OpenFilteredDialogAsync, OpenItemDialogAsync, route:/customclasses/picker-test, AmmoPicker, IDialogService (+12 more)

### Community 15 - "Community 15"
Cohesion: 0.14
Nodes (13): bottom, ChatSpecialIcon, Color, FieldInfo, float, GameObject, Image, TextMeshProUGUI (+5 more)

### Community 16 - "Community 16"
Cohesion: 0.11
Nodes (17): EditionRow, EditionRow, OnInitialized, route:/customclasses, DatabaseService, HeaderContent, MudButton, MudChip (+9 more)

### Community 17 - "Community 17"
Cohesion: 0.16
Nodes (11): FieldInfo, float, GameObject, IEnumerator, MenuScreen, MethodBase, PatchPostfix, string (+3 more)

### Community 18 - "Community 18"
Cohesion: 0.13
Nodes (9): BaseMeshEffect, Color, PerkLine, string, MultiplierFormat, Color, List, ClassIconGradient (+1 more)

### Community 19 - "Community 19"
Cohesion: 0.12
Nodes (15): ClassDiagnostic, ClassEditorService, DialogActions, DialogContent, MudAlert, MudButton, MudDialog, MudPaper (+7 more)

### Community 20 - "Community 20"
Cohesion: 0.15
Nodes (11): Color, IEnumerator, Image, MenuScreen, MethodBase, PatchPostfix, string, TextMeshProUGUI (+3 more)

### Community 21 - "Community 21"
Cohesion: 0.12
Nodes (15): ClassDiagnostic, ClassEditorService, DialogActions, DialogContent, MudAlert, MudButton, MudDialog, MudProgressCircular (+7 more)

### Community 22 - "Community 22"
Cohesion: 0.12
Nodes (15): ClassDiagnostic, ClassEditorService, DialogActions, DialogContent, MudAlert, MudButton, MudDialog, MudProgressCircular (+7 more)

### Community 23 - "Community 23"
Cohesion: 0.14
Nodes (13): CatalogAmmo, CatalogService, HeaderContent, MudChip, MudStack, MudTable, MudTd, MudText (+5 more)

### Community 24 - "Community 24"
Cohesion: 0.18
Nodes (10): CatalogPreset, CatalogPresetPart, CatalogService, MudChip, MudIcon, MudStack, MudText, SPTarkov.Server.Core.Models.Common (+2 more)

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
Nodes (10): CustomClasses, CustomClasses.Web.Layouts, CustomClasses.Web.Shared, Microsoft.AspNetCore.Components.Forms, Microsoft.AspNetCore.Components.Routing, Microsoft.AspNetCore.Components.Web.Virtualization, MudBlazor, Microsoft.AspNetCore.Components.Web (+2 more)

### Community 31 - "Community 31"
Cohesion: 0.10
Nodes (14): float, Func, PerkDiag, PerkDiagnostics, Dictionary<string, float>, GUIStyle, JsonConverter, JsonSerializerOptions (+6 more)

### Community 32 - "Community 32"
Cohesion: 0.25
Nodes (6): List, SkillTypes, string, SkillMaster, SkillCategory, SkillMasterEntry

### Community 33 - "Community 33"
Cohesion: 0.18
Nodes (10): CatalogService, ItemTooltip, LoadoutCostEntry, SPTarkov.Server.Core.Models.Common, System.Globalization, OnParametersSet, StashCell, StashGroup (+2 more)

### Community 34 - "Community 34"
Cohesion: 0.20
Nodes (7): bool, Dictionary, ESkillId, string, Payload, SkillMultipliers, Payload

### Community 35 - "Community 35"
Cohesion: 0.20
Nodes (7): bool, Color, MethodInfo, object, string, PropertyInfo, MenuOverhaulBridge

### Community 36 - "Community 36"
Cohesion: 0.24
Nodes (7): Customization, CustomizationItem, IReadOnlyDictionary, MongoId, OutfitSide, OutfitBuilder, TemplateSide

### Community 37 - "Community 37"
Cohesion: 0.06
Nodes (27): ClassEditionKeyRegistry, ClassEffect, ClassIdentitiesRouter, ClassVisualRegistry, JsonUtil, List, RouteAction, SaveServer (+19 more)

### Community 38 - "Community 38"
Cohesion: 0.20
Nodes (9): CatalogClothing, CatalogService, MudStack, MudText, MudTextField, MudVirtualize, ApplyFilter, OnParametersSet (+1 more)

### Community 39 - "Community 39"
Cohesion: 0.24
Nodes (5): BaseUnityPlugin, bool, ConfigEntry, Plugin, ManualLogSource

### Community 40 - "Community 40"
Cohesion: 0.25
Nodes (5): Image, MethodBase, PatchPostfix, TextMeshProUGUI, ChatSpecialIconPatch

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
Cohesion: 0.29
Nodes (4): MethodBase, PatchPostfix, SkillManager, QuickHandsPatch

### Community 48 - "Community 48"
Cohesion: 0.23
Nodes (7): float, HarmonyPriority, MethodBase, PatchPrefix, ProceduralWeaponAnimation, RecoilFloorApplyPatch, RecoilFloorCapturePatch

### Community 49 - "Community 49"
Cohesion: 0.11
Nodes (16): bool, float, GameObject, MethodBase, object, PatchPostfix, SkillsAndMasteringScreen, string (+8 more)

### Community 50 - "Community 50"
Cohesion: 0.25
Nodes (5): Image, MethodBase, PatchPostfix, SkillClass, SkillIconBorderPatch

### Community 51 - "Community 51"
Cohesion: 0.25
Nodes (5): MethodBase, PatchPostfix, SkillsAndMasteringScreen, string, SkillsScreenIdentityPatch

### Community 52 - "Community 52"
Cohesion: 0.25
Nodes (3): ClassVisualRegistry, Dictionary, Visual

### Community 54 - "Community 54"
Cohesion: 0.27
Nodes (5): Color, Dictionary, Sprite, string, ClassIconCache

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

### Community 59 - "Community 59"
Cohesion: 0.20
Nodes (11): cellPxOf(), dispose(), fitsAt(), init(), onCancel(), onMove(), onUp(), registerSaveShortcut() (+3 more)

### Community 61 - "Community 61"
Cohesion: 0.40
Nodes (4): ChildContent, MudTooltip, System.Globalization, TooltipContent

### Community 63 - "Community 63"
Cohesion: 0.40
Nodes (3): Dictionary, PmcData, HideoutBuilder

### Community 66 - "Community 66"
Cohesion: 0.03
Nodes (65): CharacterDoll, MudOverlay, CatalogService, ClassDiagnostic, ClassEditorService, ClassWorkspace, CostService, CustomClasses.Web (+57 more)

### Community 67 - "Community 67"
Cohesion: 0.06
Nodes (30): ItemSpecModel, PlacedCell, CatalogService, ChildContent, CostService, CustomClasses.Web, IJSRuntime, ISnackbar (+22 more)

### Community 73 - "Community 73"
Cohesion: 0.22
Nodes (8): CatalogService, CostService, ItemTooltip, SPTarkov.Server.Core.Models.Common, BuildCell, GearCell, OnParametersSet, ShortLabel

### Community 74 - "Community 74"
Cohesion: 0.20
Nodes (9): CustomClasses.Web, DialogActions, DialogContent, ItemSpecEditor, MudButton, MudDialog, MudText, TitleContent (+1 more)

### Community 79 - "Community 79"
Cohesion: 0.11
Nodes (14): ActiveHealthController, DamageInfoStruct, FirearmController, float, MethodBase, PatchPostfix, PatchPrefix, ChangeEnergyPatch (+6 more)

### Community 80 - "Community 80"
Cohesion: 0.21
Nodes (7): DamageInfoStruct, Item, List, MethodBase, PatchPrefix, Player, BulwarkPatch

### Community 81 - "Community 81"
Cohesion: 0.13
Nodes (12): BasePhysicalClass, MethodBase, PatchPostfix, PatchPrefix, GClass2175, MovementContext, ClassMoveSpeed, ExecutionSpeedCapPatch (+4 more)

### Community 82 - "Community 82"
Cohesion: 0.09
Nodes (17): AISoundType, Func, int, MethodBase, PatchPostfix, PatchPrefix, Player, EAudioMovementState (+9 more)

### Community 83 - "Community 83"
Cohesion: 0.29
Nodes (4): AbstractSkillClass, MethodBase, PatchPrefix, OnTriggerPatch

### Community 84 - "Community 84"
Cohesion: 0.17
Nodes (13): bool, Dictionary, ESkillId, float, Func, Sprite, string, PerkGroup (+5 more)

### Community 85 - "Community 85"
Cohesion: 0.22
Nodes (6): IEnumerator, MethodBase, PatchPostfix, string, GameWorld, RaidPerksNotificationPatch

### Community 86 - "Community 86"
Cohesion: 0.08
Nodes (18): bool, FieldInfo, float, HarmonyPriority, int, Item, MethodBase, PatchPostfix (+10 more)

### Community 87 - "Community 87"
Cohesion: 0.25
Nodes (5): MethodBase, PatchPostfix, SkillManager, PackMule, PackMulePatch

### Community 88 - "Community 88"
Cohesion: 0.24
Nodes (7): ClassColorEntry, Color, ConfigEntry, Dictionary, string, PerksConfig, ConfigFile

### Community 90 - "Community 90"
Cohesion: 0.14
Nodes (11): MethodBase, float, MethodBase, ForceEffector, ModulePatch, AdrenalineTriggerPatch, AimPunchPatch, HeavyWeaponErgoPatch (+3 more)

### Community 91 - "Community 91"
Cohesion: 0.17
Nodes (6): bool, float, GameObject, Identity, PointerEventData, LoadingClassHover

### Community 93 - "Community 93"
Cohesion: 0.33
Nodes (3): ClassDefinition, string, LauncherLanguageConfig

### Community 94 - "Community 94"
Cohesion: 0.16
Nodes (9): FirearmController, HarmonyPriority, MethodBase, PatchPostfix, PatchPrefix, ProceduralWeaponAnimation, UnderbarrelMasteryXpPatch, WeaponMasteryErgoPatch (+1 more)

### Community 96 - "Community 96"
Cohesion: 0.29
Nodes (7): GameObject, Identity, PerkLine, TMP_FontAsset, Transform, PerksPanelView, PerkGroup

### Community 97 - "Community 97"
Cohesion: 0.17
Nodes (8): FieldInfo, MethodBase, PatchPostfix, PatchPrefix, GroupPlayerViewModelClass, PartyPlayerItem, PartyInfoPanelPrefetchPatch, PartyPlayerItemPatch

### Community 98 - "Community 98"
Cohesion: 0.17
Nodes (9): FieldInfo, float, GameObject, MethodBase, PatchPostfix, string, HealthParametersPanel, WeightMarkerPatch (+1 more)

### Community 99 - "Community 99"
Cohesion: 0.24
Nodes (8): ClassIdentities, bool, Dictionary, Player, string, Identity, Payload, PlayerEntry

### Community 100 - "Community 100"
Cohesion: 0.24
Nodes (7): BaseNotificationView, float, IEnumerator, MethodBase, PatchPrefix, NotificationAbstractClass, NotificationDurationPatch

### Community 101 - "Community 101"
Cohesion: 0.18
Nodes (8): Dictionary, FieldInfo, MethodBase, MethodInfo, object, PatchPostfix, Type, ClassDetailLoadingPatch

### Community 102 - "Community 102"
Cohesion: 0.24
Nodes (6): CanvasGroup, float, string, FadeIn, PanelState, MonoBehaviour

### Community 103 - "Community 103"
Cohesion: 0.29
Nodes (6): CardHover, Color, Image, PointerEventData, IPointerEnterHandler, IPointerExitHandler

### Community 105 - "Community 105"
Cohesion: 0.33
Nodes (4): SkillManager, Weapon, WeaponMastery, WeaponSkillClass

### Community 107 - "Community 107"
Cohesion: 0.25
Nodes (5): FieldInfo, MethodBase, PatchPostfix, PlayerNamePanelPatch, PlayerNamePanel

### Community 108 - "Community 108"
Cohesion: 0.29
Nodes (4): MethodBase, PatchPostfix, ProceduralWeaponAnimation, CalmSightsPatch

### Community 109 - "Community 109"
Cohesion: 0.25
Nodes (5): ChatSpecialIcon, MethodBase, PatchPostfix, TextMeshProUGUI, PlayerModelWithStatsIdentityPatch

### Community 110 - "Community 110"
Cohesion: 0.50
Nodes (3): DamageInfoStruct, PatchPostfix, Player

### Community 111 - "Community 111"
Cohesion: 0.16
Nodes (8): FirearmController, PatchPostfix, PatchPrefix, Player, ProceduralWeaponAnimation, Weapon, AdsSpeedPatch, HeavyWeapon

### Community 112 - "Community 112"
Cohesion: 0.29
Nodes (5): Fmt, Line, Pol, Dictionary, PerksCatalogData

### Community 113 - "Community 113"
Cohesion: 0.18
Nodes (7): CombatMedicSurgery, ActiveHealthController, bool, MethodBase, PatchPrefix, Player, SurgeryPenaltyPatch

## Knowledge Gaps
- **950 isolated node(s):** `float`, `string`, `bool`, `Player`, `Payload` (+945 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **10 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ClassDetailLoadingPatch` connect `Community 101` to `Community 90`?**
  _High betweenness centrality (0.021) - this node is a cross-community bridge._
- **Why does `LoadingClassHover` connect `Community 91` to `Community 101`, `Community 102`, `Community 103`?**
  _High betweenness centrality (0.019) - this node is a cross-community bridge._
- **Why does `MedsOperationScopePatch` connect `Community 86` to `Community 90`?**
  _High betweenness centrality (0.009) - this node is a cross-community bridge._
- **What connects `float`, `string`, `bool` to the rest of the system?**
  _950 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.09090909090909091 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.062456140350877196 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.03773584905660377 - nodes in this community are weakly interconnected._
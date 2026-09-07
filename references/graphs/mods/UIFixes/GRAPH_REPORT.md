# Graph Report - modded  (2026-09-06)

## Corpus Check
- 139 files · ~65,980 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3530 nodes · 6442 edges · 173 communities (168 shown, 3 thin omitted)
- Extraction: 99% EXTRACTED · 1% INFERRED · 0% AMBIGUOUS · INFERRED: 66 edges (avg confidence: 0.83)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `5ddac638`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Settings
- MultiSelectPatches
- InspectWindowResizePatches
- QuickMovePreview
- ScrollPatches
- MailPatches
- GridWindowButtonsPatch
- .IsEnabled
- TraderAvatarPatches
- .Init
- CompoundItem
- DropdownPatches
- SwapPatches
- FixFleaPatches
- MultiSelect
- HideoutSearchPatches
- QuickAccessPanelPatches
- NetworkTransactionWatcher
- IsInWishlistPatch
- TagPatches
- ReloadInPlacePatches
- R
- WeaponModdingPatches
- InputRepeater
- ModulePatch
- .Postfix
- ExtraRagfairOfferItemViewProperties
- PatchPostfix
- TradingAutoSwitchPatches
- .Deselect
- ContextMenuShortcutPatches
- InternalMagPatches
- Sync
- .Prefix
- WeaponPanPatches
- Type
- BarterOfferPatches
- .Enable
- SettingExtensions
- LoadMultipleMagazinesPatches
- SettingTypes.cs
- WeaponPresetConfirmPatches
- UIFixes
- .PatchPostfix
- TraderControllerClass
- PreviousFilterButton
- .Postfix
- DragItemContext
- AddOfferClickablePricesPatches
- EmptySlotMenu
- UIFixes.Server
- .CanModify
- UnloadAmmoPatches
- ItemSpecificationPanel
- DrawMultiSelect
- Item
- .InRaid
- .Awake
- .Prefix
- .Prefix
- InsuranceInteractions
- HideoutLevelPatches
- RepairInteractions
- QuestCache
- EItemInfoButton
- ArmorTooltipPatch
- .Prefix
- .Prefix
- PatchPrefix
- .Prefix
- .UpdateQuickbindType
- ItemUiContext
- .R
- UIFixes
- RebindConsumablesPatches
- .Prefix
- BarrelOnlyPatches
- InspectWindowStatsPatches
- EditBuildScreenZoomPatch
- WindowPatches
- .Postfix
- ProfileDataHelper
- .Postfix
- RepairStrategy
- ModMetadata
- MultiSelect
- .Prefix
- EmptySlotMenuTrigger
- OpenInteractions
- PatchPostfix
- RevolverPatches
- SyncScrollPositionPatches
- PatchPostfix
- BarrelLoadAmmoInteractions
- PlaceOfferClickPatch
- AutoOpenPatch
- TacticalBindsPatches
- DefaultMaxRepairPatch
- HideoutStashAdPatch
- DebounceStatusNotifyPatch
- FleaPrevSearchPatches
- InputHelper
- PaymentSlotsPatch
- Task
- ClosePatch
- GridView
- ReopenMessagesPatch
- WindowManager
- RememberAutoselectPatch
- TaskSerializer
- AddItemToStashPatch
- .Prefix
- HideoutCameraPatch
- CompassGogglesPatch
- PatchPostfix
- MultiGrid
- .Prefix
- ItemContextAbstractClass
- OpenWindow
- .Postfix
- .AutoExpandCategories
- PatchPrefix
- ItemMarketPricesPanel
- .Prefix
- UnlockCursorPatch
- .Prefix
- FiltersPanel
- TaskSerializer
- RagfairOfferItemView
- TradingItemView
- PatchPostfix
- WeaponPreviewCameraNearClipPatch
- .Prefix
- AssortUnlocksPatch
- .Prefix
- .Postfix
- OnDragEventPatch
- IResult
- RotateKeybindPatch
- MultiSelectDebug
- AutofillQuestItemsPatch
- .Postfix
- InventoryController
- LastResizePatch
- BeginDragPatch
- MoveTaskbarPatch
- RemoveDoorActionsPatch
- .GetDeepAttributes
- .GetClampedRepairAmount
- HideInviteUIPatch
- FixPlayerInspectPatch
- ProductionPanel
- .IsOnTop
- IEnumerable
- .Postfix
- FormatFullValuesPatch
- TransferMergePatch
- ButtonListener
- .RemoveTrailingZeros
- .Postfix
- DisableInsureOnUninsurable
- .PositionContextMenuFlyout
- ModifyUnsearchedContainerPatch
- InventoryShowPatch
- OfferViewList
- RagfairNewOfferItemView
- .Postfix
- .Postfix
- InsuranceInteractions.cs
- RepairInteractions.cs
- MultiSelectStrategy

## God Nodes (most connected - your core abstractions)
1. `Settings` - 156 edges
2. `UIFixes` - 115 edges
3. `Item` - 69 edges
4. `MultiSelect` - 55 edges
5. `MultiSelectPatches` - 51 edges
6. `R` - 48 edges
7. `Task` - 41 edges
8. `SwapPatches` - 32 edges
9. `ContextMenuPatches` - 26 edges
10. `WeaponModdingPatches` - 25 edges

## Surprising Connections (you probably didn't know these)
- `EmptySlotMenu` --references--> `Slot`  [EXTRACTED]
  mods/UIFixes/modded/src/ContextMenus/EmptySlotMenu.cs → mods/UIFixes/modded/src/R.cs
- `EmptySlotMenuTrigger` --references--> `Slot`  [EXTRACTED]
  mods/UIFixes/modded/src/ContextMenus/EmptySlotMenuTrigger.cs → mods/UIFixes/modded/src/R.cs
- `EmptySlotContext` --references--> `Slot`  [EXTRACTED]
  mods/UIFixes/modded/src/ContextMenus/EmptySlotMenuTrigger.cs → mods/UIFixes/modded/src/R.cs
- `InsuranceInteractions` --references--> `Item`  [EXTRACTED]
  mods/UIFixes/modded/src/ContextMenus/InsuranceInteractions.cs → mods/UIFixes/modded/src/R.cs
- `ContextMenuPatches` --references--> `InsuranceInteractions`  [EXTRACTED]
  mods/UIFixes/modded/src/Patches/ContextMenuPatches.cs → mods/UIFixes/modded/src/ContextMenus/InsuranceInteractions.cs

## Import Cycles
- None detected.

## Communities (173 total, 3 thin omitted)

### Community 0 - "Settings"
Cohesion: 0.01
Nodes (142): ConfigurationManager, ConfigEntry, AddContainerButtons, AddOfferKeyBind, AddToUnsearchedContainers, AlwaysSwapMags, AutoExpandCategories, AutofillQuestTurnIns (+134 more)

### Community 1 - "MultiSelectPatches"
Cohesion: 0.05
Nodes (38): AmmoPackReloadingClass, ContainedGridsView, IRollback, Color, GridItemAddress, GridView, Image, ItemContextAbstractClass (+30 more)

### Community 2 - "InspectWindowResizePatches"
Cohesion: 0.05
Nodes (37): ErrorScreen, InteractableElement, ItemInfoWindowLabels, ItemsListWindow, MessageWindow, SplitDialog, AddOfferWindow, MethodBase (+29 more)

### Community 3 - "QuickMovePreview"
Cohesion: 0.05
Nodes (31): IPointerEnterHandler, IPointerExitHandler, Slider, SliderMouseListener, ItemUiContext, ItemView, MethodBase, PatchPostfix (+23 more)

### Community 4 - "ScrollPatches"
Cohesion: 0.07
Nodes (29): ETradeMode, KeyScroller, NotesTask, AreaScreenSubstrate, CodeInstruction, IEnumerable, ItemUiContext, LightScroller (+21 more)

### Community 5 - "MailPatches"
Cohesion: 0.07
Nodes (29): ChatMessageClass, DateTime, DialogueClass, DialoguesContainer, DialogueView, SocialNetworkClass, CodeInstruction, FieldInfo (+21 more)

### Community 6 - "GridWindowButtonsPatch"
Cohesion: 0.05
Nodes (31): KeyPressState, LeftRightKeybind, EItemAttributeId, GridWindow, Image, MethodBase, PatchPostfix, Sprite (+23 more)

### Community 7 - ".IsEnabled"
Cohesion: 0.07
Nodes (29): EventArgs, GamePlayerOwner, IInputKey, KeyCombinationState, ECommand, EGameKey, KeyBindingClass, List (+21 more)

### Community 8 - "TraderAvatarPatches"
Cohesion: 0.06
Nodes (31): AbstractQuestControllerClass, Condition, DefaultUiButtonNewStyle, MagPresetEditor, QuestListItem, FieldInfo, List, MethodBase (+23 more)

### Community 9 - ".Init"
Cohesion: 0.06
Nodes (36): ProductionPanel, AddViewListClass, FieldInfo, LayoutElement, AddOperationResult, ResizeOperation, Type, AreaScreenSubstrate (+28 more)

### Community 10 - "CompoundItem"
Cohesion: 0.06
Nodes (28): IOrderedEnumerable, Dictionary, GridView, IEnumerable, KeyValuePair, MethodBase, PatchPostfix, PatchPrefix (+20 more)

### Community 11 - "DropdownPatches"
Cohesion: 0.07
Nodes (25): ClickHandler, EmptyItemView, IPointerClickHandler, ItemSelectionCell, ModdingScreenSlotView, ModdingSelectableItemView, Action, CameraViewporter (+17 more)

### Community 12 - "SwapPatches"
Cohesion: 0.08
Nodes (18): EOwnerType, GridItemView, MethodBase, PatchPostfix, CleanupSwapSourceContainerPatch, DetectFilterForSwapPatch, DetectGridHighlightPrecheckPatch, DetectSlotHighlightPrecheckPatch (+10 more)

### Community 13 - "FixFleaPatches"
Cohesion: 0.08
Nodes (22): RagfairCategoriesPanel, GameObject, HorizontalLayoutGroup, Image, MethodBase, OfferView, PatchPostfix, PatchPrefix (+14 more)

### Community 14 - "MultiSelect"
Cohesion: 0.11
Nodes (22): Context, Handler, Action, Dictionary, EItemInfoButton, Func, GameObject, ItemUiContext (+14 more)

### Community 15 - "HideoutSearchPatches"
Cohesion: 0.09
Nodes (23): ETranslateResult, KeyScrollListener, ProduceView, Scheme, AreaScreenSubstrate, Dictionary, ECommand, IEnumerable (+15 more)

### Community 16 - "QuickAccessPanelPatches"
Cohesion: 0.08
Nodes (23): BattleUIQuickbarManager, BindPanel, ControlSettingsClass, InventoryScreenQuickAccessPanel, QuickSlotItemView, EBoundItem, ECommand, GameObject (+15 more)

### Community 17 - "NetworkTransactionWatcher"
Cohesion: 0.06
Nodes (23): Class308, IDisposable, OffersList, RagfairSearch, Result, Callback, HarmonyPriority, IResult (+15 more)

### Community 18 - "IsInWishlistPatch"
Cohesion: 0.08
Nodes (19): AreaRequirement, EWishlistGroup, Requirement, AreaData, IEnumerable, IEnumerator, ItemContextAbstractClass, List (+11 more)

### Community 19 - "TagPatches"
Cohesion: 0.08
Nodes (22): EditTagWindow, IItemComponent, ItemProperties, Dictionary, EItemInfoButton, FieldInfo, GridItemView, IEnumerable (+14 more)

### Community 20 - "ReloadInPlacePatches"
Cohesion: 0.09
Nodes (21): FirearmsAnimator, RemoveOperation, Callback, FieldInfo, FirearmController, GridItemAddress, GStruct154, ItemAddress (+13 more)

### Community 21 - "R"
Cohesion: 0.07
Nodes (29): UIInputNode, PatchPrefix, GridSortPanel, OfferViewList, RagFairClass, Tab, AddOfferWindow, BulkOffer (+21 more)

### Community 22 - "WeaponModdingPatches"
Cohesion: 0.10
Nodes (16): MethodBase, ArmorSlotAcceptRaidPatch, DisassembleAllPatch, InspectLockedPatch, LockedStatePatch, LongerFullIdPatch, ModCanBeMovedPatch, ModEquippedPatch (+8 more)

### Community 23 - "InputRepeater"
Cohesion: 0.11
Nodes (17): FieldRef, FirearmHandsInputTranslator, MovementContext, EGameKey, Func, InputRepeater, FirearmController, MethodBase (+9 more)

### Community 24 - "ModulePatch"
Cohesion: 0.14
Nodes (18): ModulePatch, MethodBase, ChangeInteractionButtonCreationPatch, ContextMenuFontSizePatch, ContextMenuNamesPatch, ContextMenuPatches, CreateSubInteractionsInventoryPatch, DeclareSubInteractionsInventoryPatch (+10 more)

### Community 25 - ".Postfix"
Cohesion: 0.09
Nodes (19): AllButtonKeybind, BarterSchemePanel, HandoverExchangeableItemsWindow, HandoverRagfairMoneyWindow, Action, MethodBase, PatchPostfix, TMP_InputField (+11 more)

### Community 26 - "ExtraRagfairOfferItemViewProperties"
Cohesion: 0.12
Nodes (14): ConditionalWeakTable, Properties, IRaiseEvents, ItemViewStats, MoveOperation, RagfairOfferItemView, TemplatedGridsView, Vector2 (+6 more)

### Community 27 - "PatchPostfix"
Cohesion: 0.06
Nodes (16): EItemPinLockState, MenuUI, PinOperation, PlayerInventoryController, GStruct154, IContainer, IEnumerable, MoveOperation (+8 more)

### Community 28 - "TradingAutoSwitchPatches"
Cohesion: 0.09
Nodes (19): ETraderMode, ETradingItemViewType, InputButton, MethodBase, PatchPostfix, PatchPrefix, Tab, TraderDealScreen (+11 more)

### Community 29 - ".Deselect"
Cohesion: 0.10
Nodes (9): ETargetContainer, GEventArgs2, InfoWindow, GridItemView, GridView, MultiSelectItemContext, ItemView, PointerEventData (+1 more)

### Community 30 - "ContextMenuShortcutPatches"
Cohesion: 0.12
Nodes (15): HideoutItemView, EItemInfoButton, ItemContextAbstractClass, ItemInfoInteractionsAbstractClass, ItemUiContext, MethodBase, PatchPostfix, TMP_InputField (+7 more)

### Community 31 - "InternalMagPatches"
Cohesion: 0.09
Nodes (18): EItemInfoButton, GStruct156, IEnumerable, ItemContextAbstractClass, ItemOperation, ItemUiContext, MagazineItemClass, MethodBase (+10 more)

### Community 32 - "Sync"
Cohesion: 0.07
Nodes (17): UIFixes.Net, UIFixes.Fika, FikaGameEndedEvent, FikaNetworkManagerCreatedEvent, FikaRaidStartedEvent, INetSerializable, NetDataReader, NetDataWriter (+9 more)

### Community 33 - ".Prefix"
Cohesion: 0.09
Nodes (21): BuildsCategoriesPanel, EntityNodeClass, EntityNodeDictionary, EWindowType, HandbookClass, NodeBaseView, Action, EditBuildScreen (+13 more)

### Community 34 - "WeaponPanPatches"
Cohesion: 0.11
Nodes (17): ECursorResult, CameraViewporter, DragTrigger, EditBuildScreen, MethodBase, PatchPostfix, PatchPrefix, PointerEventData (+9 more)

### Community 35 - "Type"
Cohesion: 0.08
Nodes (24): EScrollOrder, EStringCase, SimpleContextMenu, SimpleContextMenuButton, CompactCharacteristicPanel, CompareItemAttribute, ItemAttribute, Type (+16 more)

### Community 36 - "BarterOfferPatches"
Cohesion: 0.12
Nodes (13): ItemViewStats, MethodBase, PatchPrefix, BarterOfferPatches, HideItemViewStatsPatch, IconsPatch, ItemViewScalePatch, NoPointerClickPatch (+5 more)

### Community 37 - ".Enable"
Cohesion: 0.10
Nodes (17): BipodViewController, LauncherViauslController, OpticSight, SightModVisualControllers, SkinnedMeshRenderer, CodeInstruction, IEnumerable, MethodBase (+9 more)

### Community 38 - "SettingExtensions"
Cohesion: 0.12
Nodes (10): ConfigEntryBase&gt;, DescriptionAttribute, Enum, ConfigurationManagerAttributes, Func, Action, ConfigEntry, ConfigEntryBase (+2 more)

### Community 39 - "LoadMultipleMagazinesPatches"
Cohesion: 0.13
Nodes (13): ItemFilter, MagazineBuildPresetClass, ItemUiContext, MagazineItemClass, MethodBase, PatchPostfix, PatchPrefix, CheckItemFilterPatch (+5 more)

### Community 40 - "SettingTypes.cs"
Cohesion: 0.08
Nodes (25): AutoFleaPrice, Average, Maximum, Minimum, None, AutoWishlistBehavior, All, Normal (+17 more)

### Community 41 - "WeaponPresetConfirmPatches"
Cohesion: 0.12
Nodes (12): NaiveAcceptable, EditBuildScreen, FieldInfo, MethodBase, PatchPostfix, PatchPrefix, ConfirmDiscardWeaponPresetChangesPatch, DetectWeaponPresetCloseTypePatch (+4 more)

### Community 42 - "UIFixes"
Cohesion: 0.08
Nodes (24): BepInEx.Analyzers (1.*), BepInEx.Core (5.*), BepInEx.PluginInfoProps (1.*), coverlet.collector (6.0.0), Microsoft.NET.Test.Sdk (17.8.0), Microsoft.NETFramework.ReferenceAssemblies (1.0.2), MSTest.TestAdapter (3.1.1), MSTest.TestFramework (3.1.1) (+16 more)

### Community 43 - ".PatchPostfix"
Cohesion: 0.11
Nodes (16): CodeInstruction, Color, GameObject, GridView, IEnumerable, Image, ItemContextClass, ItemView (+8 more)

### Community 44 - "TraderControllerClass"
Cohesion: 0.10
Nodes (13): Callback, IEnumerable, IRaiseEvents, ItemContextAbstractClass, ItemOperation, ItemUiContext, List, PatchPrefix (+5 more)

### Community 45 - "PreviousFilterButton"
Cohesion: 0.13
Nodes (14): ESetFilterSource, ESortType, FilterRule, ISession, PreviousFilterButton, RagfairScreen, DefaultUIButton, HorizontalLayoutGroup (+6 more)

### Community 46 - ".Postfix"
Cohesion: 0.11
Nodes (16): DefaultUIButton, GameObject, HorizontalLayoutGroup, IEnumerable, Image, LocalizedText, MethodBase, PatchPostfix (+8 more)

### Community 47 - "DragItemContext"
Cohesion: 0.20
Nodes (13): SlotView, Dictionary, GridView, IInventoryEventResult, ItemView, TraderControllerClass, GridViewCanAcceptSwapPatch, SlotCanAcceptSwapPatch (+5 more)

### Community 48 - "AddOfferClickablePricesPatches"
Cohesion: 0.14
Nodes (12): ItemMarketPricesPanel, AddOfferWindow, MethodBase, PatchPostfix, PatchPrefix, RagfairOfferSellHelperClass, RequirementView, AddOfferClickablePricesPatches (+4 more)

### Community 49 - "EmptySlotMenu"
Cohesion: 0.11
Nodes (14): ContextInteractionsAbstractClass, Action, EItemInfoButton, EItemUiContextType, IEnumerable, IResult, ItemContextAbstractClass, ItemUiContext (+6 more)

### Community 50 - "UIFixes.Server"
Cohesion: 0.10
Nodes (16): UIFixes.Server, RagfairCallbacks, SearchRequestData, StaticRouter, DatabaseService, Dictionary, ISptLogger, AssortUnlocksCallbacks (+8 more)

### Community 51 - ".CanModify"
Cohesion: 0.12
Nodes (11): InventoryError, Mod, DisplayableErrorWrapper, GStruct156, IContainer, ItemAddress, Weapon, ArmorPlatesInRaidError (+3 more)

### Community 52 - "UnloadAmmoPatches"
Cohesion: 0.14
Nodes (12): EItemInfoButton, IEnumerable, IResult, MagazineItemClass, MethodBase, PatchPostfix, PatchPrefix, NoScavStashPatch (+4 more)

### Community 53 - "ItemSpecificationPanel"
Cohesion: 0.13
Nodes (16): CompactCharacteristicDropdownPanel, ECurrencyType, Action, CompactCharacteristicPanel, Dictionary, IEnumerable, ItemAttributeClass, KeyValuePair (+8 more)

### Community 54 - "DrawMultiSelect"
Cohesion: 0.15
Nodes (11): GraphicRaycaster, MonoBehaviour, RaycastResult, GameObject, GridItemView, List, PointerEventData, RectTransform (+3 more)

### Community 55 - "Item"
Cohesion: 0.12
Nodes (12): GStruct155, Func, IEnumerable, ItemUiContext, ItemTaskSerializer, MultiSelectController, CommandStatus, ItemAddress (+4 more)

### Community 56 - ".InRaid"
Cohesion: 0.12
Nodes (12): ContextInteractionSwitcherClass, EItemInfoButton, ItemUiContext, MagazineItemClass, MethodBase, PatchPrefix, EnableContextMenuPatch, LoadAmmoInRaidPatches (+4 more)

### Community 57 - ".Awake"
Cohesion: 0.12
Nodes (10): BaseUnityPlugin, ConfigFile, ManualLogSource, MethodBase, PatchPrefix, FixTraderFiltersPatch, Version, Plugin (+2 more)

### Community 58 - ".Prefix"
Cohesion: 0.14
Nodes (13): Camera, InventoryPlayerModelWithStatsWindow, PlayerModelView, AddViewListClass, DragTrigger, MethodBase, PatchPrefix, PointerEventData (+5 more)

### Community 59 - ".Prefix"
Cohesion: 0.11
Nodes (15): DialogueController, GetMailDialogViewRequestData, GetMailDialogViewResponseData, Message, SaveServer, TimeUtil, Version, List (+7 more)

### Community 60 - "InsuranceInteractions"
Cohesion: 0.13
Nodes (13): EInsurers, InsuranceCompanyClass, InsuranceItem, Action, Dictionary, IEnumerable, IResult, ItemUiContext (+5 more)

### Community 61 - "HideoutLevelPatches"
Cohesion: 0.16
Nodes (10): ELevelType, AreaData, MethodBase, PatchPostfix, PatchPrefix, ChangeLevelPatch, ClearLevelPatch, HideoutLevelPatches (+2 more)

### Community 62 - "RepairInteractions"
Cohesion: 0.14
Nodes (11): ERepairers, ItemInfoInteractionsAbstractClass, RepairableComponent, IEnumerable, IRepairer, IResult, RepairControllerClass, ERepairers (+3 more)

### Community 63 - "QuestCache"
Cohesion: 0.11
Nodes (12): IReadOnlyCollection, RawQuestClass, IInventoryEventResult, MethodInfo, DialogWindow, Money, Type, QuestCache (+4 more)

### Community 64 - "EItemInfoButton"
Cohesion: 0.16
Nodes (11): ModdingItemContext, ModdingItemInteractions, Action, EItemInfoButton, IEnumerable, InventoryInteractions, ItemInfoInteractionsAbstractClass, CreateSubInteractionsTradingPatch (+3 more)

### Community 65 - "ArmorTooltipPatch"
Cohesion: 0.14
Nodes (12): QuestItemViewPanel, GridItemView, HoverTrigger, ItemUiContext, MethodBase, ModSlotView, PatchPostfix, PatchPrefix (+4 more)

### Community 66 - ".Prefix"
Cohesion: 0.12
Nodes (15): RagfairHelper, TemplateItem, App, DatabaseService, HashSet, ISptLogger, ItemHelper, List (+7 more)

### Community 67 - ".Prefix"
Cohesion: 0.15
Nodes (15): AddItemDirectRequest, HideoutSingleProductionStartRequestData, ICloner, InventoryHelper, ItemEventRouterResponse, App, IEnumerable, ISptLogger (+7 more)

### Community 68 - "PatchPrefix"
Cohesion: 0.11
Nodes (11): FirearmAddingModState, FirearmReadyState, InventoryError, ItemContextClass, PatchPrefix, SimpleTooltip, SlotView, ActiveWeaponMagDropPatch (+3 more)

### Community 69 - ".Prefix"
Cohesion: 0.13
Nodes (12): ITargetItemResult, MergeOperation, GStruct154, HarmonyPriority, ItemContextAbstractClass, ItemOperation, MethodBase, PatchPrefix (+4 more)

### Community 70 - ".UpdateQuickbindType"
Cohesion: 0.16
Nodes (12): ItemType, Dictionary, EBoundItem, EGameKey, MainMenuControllerClass, PatchPostfix, ItemType, Headlight (+4 more)

### Community 71 - "ItemUiContext"
Cohesion: 0.11
Nodes (16): PropertyInfo, EItemInfoButton, EItemUiContextType, GridWindow, ItemContextAbstractClass, ItemInfoInteractionsAbstractClass, ItemContext, Type (+8 more)

### Community 72 - ".R"
Cohesion: 0.11
Nodes (14): AddOperationResult, FoldOperationResult, InventoryScreen, ItemContext, MoveOperationResult, ScavengerInventoryScreen, GridWindow, TextMeshProUGUI (+6 more)

### Community 73 - "UIFixes"
Cohesion: 0.11
Nodes (6): UIFixes, MethodBase, PatchPrefix, OperationQueuePatch, MethodBase, TransferConfirmPatch

### Community 74 - "RebindConsumablesPatches"
Cohesion: 0.15
Nodes (11): DiscardOperation, CommandStatus, EquipmentSlot, FieldInfo, GStruct154, MethodBase, PatchPostfix, RebindConsumablesPatches (+3 more)

### Community 75 - ".Prefix"
Cohesion: 0.15
Nodes (12): DisplayMoneyPanel, DisplayMoneyPanelTMPText, IEnumerable, Image, LayoutElement, MethodBase, PatchPrefix, TextMeshProUGUI (+4 more)

### Community 76 - "BarrelOnlyPatches"
Cohesion: 0.18
Nodes (9): ItemContextAbstractClass, MethodBase, BarrelOnlyPatches, LoadAmmoIsActivePatch, LoadAmmoIsInteractivePatch, LoadAmmoSubInteractionsPatch, LoadBarrelPatch, UnloadAmmoPatch (+1 more)

### Community 77 - "InspectWindowStatsPatches"
Cohesion: 0.18
Nodes (9): EItemAttributeId, MethodBase, CompareModStatsPatch, FixDurabilityBarPatch, FixTraderCompatWithPatch, FormatCompactValuesPatch, HighlightFilledSlotsPatch, HighlightSlotsPatch (+1 more)

### Community 78 - "EditBuildScreenZoomPatch"
Cohesion: 0.17
Nodes (10): EditBuildScreen, MethodBase, PatchPrefix, ScrollTrigger, WeaponModdingScreen, WeaponPreview, EditBuildScreenZoomPatch, NoInventoryZoomPatch (+2 more)

### Community 79 - "WindowPatches"
Cohesion: 0.18
Nodes (9): MethodBase, PatchPrefix, BorderPrioritizedWindow, InventoryClosePatch, KeepTopOnScreenPatch, KeepWindowOnScreenPatch, WindowClosePatch, WindowOpenPatch (+1 more)

### Community 80 - ".Postfix"
Cohesion: 0.14
Nodes (12): CanvasRenderer, DogtagComponent, HorizontalOrVerticalLayoutGroup, IExchangeRequirement, ItemViewManager, OfferItemPriceBarter, List, ItemCacheHelper (+4 more)

### Community 81 - "ProfileDataHelper"
Cohesion: 0.14
Nodes (12): ConcurrentDictionary, FileUtil, Location, ParentId, SlotId, Dictionary, MongoId, ProfileData (+4 more)

### Community 82 - ".Postfix"
Cohesion: 0.15
Nodes (12): IOnLoad, App, Dictionary, ISptLogger, MethodBase, MongoId, PatchPostfix, PatchPrefix (+4 more)

### Community 83 - "RepairStrategy"
Cohesion: 0.12
Nodes (9): IRepairer, MongoID, RepairStrategy, CurrentRepairer, Repairers, Type, Scheme, EndProduct (+1 more)

### Community 84 - "ModMetadata"
Cohesion: 0.12
Nodes (16): AbstractModMetadata, Range, Dictionary, List, Version, ModMetadata, Author, Contributors (+8 more)

### Community 85 - "MultiSelect"
Cohesion: 0.14
Nodes (13): UIFixesInterop, Action, Func, IEnumerable, Item, ItemUiContext, MethodInfo, Task (+5 more)

### Community 86 - ".Prefix"
Cohesion: 0.15
Nodes (9): EMoveItemOrder, GridItemView, HarmonyPriority, InputButton, TraderControllerClass, TradingItemView, AdjustQuickFindFlagsPatch, DeselectOnTradingItemViewClickPatch (+1 more)

### Community 87 - "EmptySlotMenuTrigger"
Cohesion: 0.21
Nodes (7): IPointerDownHandler, IPointerUpHandler, ItemContextAbstractClass, ItemUiContext, PointerEventData, EmptySlotContext, EmptySlotMenuTrigger

### Community 88 - "OpenInteractions"
Cohesion: 0.18
Nodes (11): Options, GridItemView, IEnumerable, IResult, ItemContextAbstractClass, ItemUiContext, NestedContainerTaskSerializer, OpenInteractions (+3 more)

### Community 89 - "PatchPostfix"
Cohesion: 0.16
Nodes (9): Dictionary, ItemContextAbstractClass, ItemUiContext, ModSlotView, PatchPostfix, SlotView, TraderControllerClass, Vector2 (+1 more)

### Community 90 - "RevolverPatches"
Cohesion: 0.20
Nodes (8): MethodBase, CylinderMagApplyPatch, CylinderMagApplyWithoutRestrictionsPatch, DisablePresetPatch, LoadAmmoSubInteractionsPatch, LoadCylinderPatch, LoadTaskSerializer, RevolverPatches

### Community 91 - "SyncScrollPositionPatches"
Cohesion: 0.17
Nodes (8): AddOfferWindow, MethodBase, PatchPostfix, ScrollRect, Vector2, SyncOfferStashScrollPatch, SyncScrollPositionPatches, SyncStashScrollPatch

### Community 92 - "PatchPostfix"
Cohesion: 0.17
Nodes (9): CanvasGroup, FoldableComponent, FoldOperation, Func, GStruct154, ModSlotView, MoveOperation, PatchPostfix (+1 more)

### Community 93 - "BarrelLoadAmmoInteractions"
Cohesion: 0.18
Nodes (8): EMagInteraction, AmmoItemClass, Dictionary, ItemUiContext, BarrelLoadAmmoInteractions, HasIcons, EMagInteraction, NoCompatibleAmmo

### Community 94 - "PlaceOfferClickPatch"
Cohesion: 0.17
Nodes (8): AddOfferWindow, MethodBase, PatchPostfix, PatchPrefix, RequirementView, ClosePatch, KeepOfferWindowOpenPatches, PlaceOfferClickPatch

### Community 95 - "AutoOpenPatch"
Cohesion: 0.17
Nodes (9): EItemUiContextType, InputButton, ItemOperation, ItemUiContext, MethodBase, PatchPrefix, AutoOpenPatch, DefaultBindPatch (+1 more)

### Community 96 - "TacticalBindsPatches"
Cohesion: 0.22
Nodes (7): MethodBase, BindableTacticalPatch, BindTacticalPatch, InitQuickBindsPatch, ReachableTacticalPatch, TacticalBindsPatches, UnbindTacticalPatch

### Community 97 - "DefaultMaxRepairPatch"
Cohesion: 0.19
Nodes (8): ConditionCharacteristicsSlider, DropDownBox, RepairerParametersPanel, MethodBase, PatchPostfix, DefaultMaxRepairPatch, RememberRepairerPatch, RememberRepairerPatches

### Community 98 - "HideoutStashAdPatch"
Cohesion: 0.20
Nodes (8): EAreaType, Dictionary, GameObject, MethodBase, PatchPostfix, HideoutStashAdPatch, RemoveAdsPatches, StashPanelAdPatch

### Community 99 - "DebounceStatusNotifyPatch"
Cohesion: 0.18
Nodes (9): EQuestStatus, Dictionary, MethodBase, MongoID, PatchPrefix, QuestClass, BigButtonPatch, DebounceStatusNotifyPatch (+1 more)

### Community 100 - "FleaPrevSearchPatches"
Cohesion: 0.22
Nodes (7): HistoryEntry, MethodBase, ChangedViewListTypePatch, FleaPrevSearchPatches, OfferViewListCategoryPickedPatch, OfferViewListDoneLoadingPatch, RagfairScreenShowPatch

### Community 101 - "InputHelper"
Cohesion: 0.19
Nodes (8): IdleStateClass, Dictionary, EGameKey, InputBindingsDataClass, KeyBindingClass, InputHelper, InputBindingsDataClass, PatchPostfix

### Community 102 - "PaymentSlotsPatch"
Cohesion: 0.15
Nodes (9): IReadOnlyList, EquipmentSlot, MethodBase, PatchPrefix, BTRPaymentPatches, PaymentSlotsPatch, PatchPrefix, TraderControllerClass (+1 more)

### Community 103 - "Task"
Cohesion: 0.23
Nodes (6): Task, IResult, LoadMagazinePatch, LoadMultiBarrelWeaponPatch, LoadWeaponWithAmmoPatch, IResult

### Community 104 - "ClosePatch"
Cohesion: 0.20
Nodes (8): MethodBase, PatchPostfix, PatchPrefix, TraderAssortmentControllerClass, TradingGridView, ClosePatch, RequisiteChangePatch, TradingHighlightPatches

### Community 105 - "GridView"
Cohesion: 0.14
Nodes (13): Color, Image, ItemView, TraderControllerClass, GridView, HighlightPanel, InvalidOperationColor, ItemViews (+5 more)

### Community 106 - "ReopenMessagesPatch"
Cohesion: 0.21
Nodes (7): EEftScreenType, MainMenuControllerClass, MethodBase, PatchPostfix, KeepMessagesOpenPatches, ReopenMessagesPatch, SniffChatPanelClosePatch

### Community 107 - "WindowManager"
Cohesion: 0.19
Nodes (7): OpenWindow, Dictionary, HashSet, ItemContextAbstractClass, WindowContext, WindowManager, Instance

### Community 108 - "RememberAutoselectPatch"
Cohesion: 0.19
Nodes (7): MethodBase, PatchPostfix, PatchPrefix, AddOfferRememberAutoselectPatches, RememberAutoselectPatch, RestoreAutoselectPatch, UpdatableToggle

### Community 109 - "TaskSerializer"
Cohesion: 0.24
Nodes (7): Action, Func, IEnumerable, IEnumerator, TaskCompletionSource, TaskSerializer, TaskSerializerBase

### Community 110 - "AddItemToStashPatch"
Cohesion: 0.23
Nodes (7): AbstractPatch, MethodBase, Task, AddItemToStashPatch, PutToolsBack, RegisterProductionPatch, Upd

### Community 111 - ".Prefix"
Cohesion: 0.24
Nodes (8): CylinderMagazineItemClass, LoadTaskSerializer, AmmoItemClass, InventoryInteractions, ISubInteractions, ItemOperation, PatchPrefix, TraderControllerClass

### Community 112 - "HideoutCameraPatch"
Cohesion: 0.23
Nodes (6): HideoutCameraController, MethodBase, PatchPrefix, HideoutCameraPatch, HideoutCameraPatches, HideoutZoomPatch

### Community 113 - "CompassGogglesPatch"
Cohesion: 0.18
Nodes (8): IAnimatorEventParameter, ILeftHandController, LeftHandController, MethodBase, PatchPrefix, Player, CompassGogglesPatch, Continuation

### Community 114 - "PatchPostfix"
Cohesion: 0.18
Nodes (8): MarginsStruct, Color, GridWindow, InputNode, List, PatchPostfix, RectTransform, WindowData

### Community 115 - "MultiGrid"
Cohesion: 0.27
Nodes (7): Dictionary, GridItemAddress, GridView, LocationInGrid, StashGridClass, MultiGrid, Vector2Int

### Community 116 - ".Prefix"
Cohesion: 0.23
Nodes (8): ContextInteractionSwitcherClass, EItemInfoButton, InventoryInteractions, IResult, ISubInteractions, PatchPrefix, TraderControllerClass, Weapon

### Community 117 - "ItemContextAbstractClass"
Cohesion: 0.27
Nodes (7): DraggedItemView, IContainer, Image, ItemContextAbstractClass, ItemSpecificationPanel, ItemUiContext, LocationInGrid

### Community 118 - "OpenWindow"
Cohesion: 0.17
Nodes (10): WindowContext, MongoID, UIInputNode, Vector2, WindowData, OpenWindow, ItemId, Position (+2 more)

### Community 119 - ".Postfix"
Cohesion: 0.18
Nodes (7): BuildItemSelector, IItemResult, ModdingItemSelector, GStruct154, IEnumerable, FixNoGridErrorPatch, Error

### Community 120 - ".AutoExpandCategories"
Cohesion: 0.18
Nodes (7): CategoryView, CombinedView, BrowseCategoriesPanel, ItemAddress, ScrollRect, Extensions, SubcategoryView

### Community 121 - "PatchPrefix"
Cohesion: 0.18
Nodes (4): DynamicInteractionClass, GStruct156, PatchPrefix, SniffInteractionButtonCreationPatch

### Community 122 - "ItemMarketPricesPanel"
Cohesion: 0.18
Nodes (9): TextMeshProUGUI, ContextMenuButton, Text, Type, ItemMarketPricesPanel, AverageLabel, LowestLabel, MaximumLabel (+1 more)

### Community 123 - ".Prefix"
Cohesion: 0.20
Nodes (7): FilterPanel, GridView, ItemUiContext, MethodBase, PatchPrefix, TraderControllerClass, FixGridPrepareItemsPatch

### Community 124 - "UnlockCursorPatch"
Cohesion: 0.24
Nodes (6): FullScreenMode, Action, MethodBase, PatchPrefix, CursorPatches, UnlockCursorPatch

### Community 125 - ".Prefix"
Cohesion: 0.20
Nodes (6): ItemRotation, List, LocationInGrid, StashGridClass, FindPlaceToPutPatch, FindSpotKeepRotationPatch

### Community 126 - "FiltersPanel"
Cohesion: 0.20
Nodes (9): RagfairFilterButton, FiltersPanel, BarterButton, ExpirationButton, OfferItemButton, PriceButton, RatingButton, SortDescending (+1 more)

### Community 127 - "TaskSerializer"
Cohesion: 0.22
Nodes (7): TaskSerializer, LoadTaskSerializer, UnloadChambersTaskSerializer, MagazineItemClass, UnloadCamorasTaskSerializer, UnloadCylinderPatch, UnloadCamorasTaskSerializer

### Community 128 - "RagfairOfferItemView"
Cohesion: 0.22
Nodes (5): Image, RagfairOfferItemView, TextMeshProUGUI, ItemUpdateInfoPatch, ItemViewManager

### Community 129 - "TradingItemView"
Cohesion: 0.20
Nodes (8): TraderAssortmentControllerClass, TradingItemView, IsBeingSold, TraderAssortmentController, Type, TradingTableGridView, TraderAssortmentController, Type

### Community 130 - "PatchPostfix"
Cohesion: 0.33
Nodes (5): CharacteristicPanel, CompactCharacteristicPanel, Image, PatchPostfix, TextMeshProUGUI

### Community 131 - "WeaponPreviewCameraNearClipPatch"
Cohesion: 0.25
Nodes (5): Class3271, MethodBase, PatchPostfix, WeaponPreviewCameraNearClipPatch, WeaponPreviewPatches

### Community 132 - ".Prefix"
Cohesion: 0.22
Nodes (6): FlatItemsDataClass, ItemReceiver, LocationInGrid, MethodBase, PatchPrefix, PutToolsBackPatch

### Community 133 - "AssortUnlocksPatch"
Cohesion: 0.25
Nodes (6): HoverTooltipArea, Dictionary, MethodBase, OfferView, PatchPostfix, AssortUnlocksPatch

### Community 134 - ".Prefix"
Cohesion: 0.31
Nodes (6): IHandsController, LightComponent, Callback, PatchPrefix, Player, UseTacticalPatch

### Community 135 - ".Postfix"
Cohesion: 0.22
Nodes (6): ItemContextAbstractClass, ItemSpecificationPanel, ItemUiContext, SimpleTooltip, Transform, CalculateModStatsPatch

### Community 136 - "OnDragEventPatch"
Cohesion: 0.25
Nodes (5): MethodBase, PatchPrefix, PointerEventData, LimitDragPatches, OnDragEventPatch

### Community 137 - "IResult"
Cohesion: 0.42
Nodes (5): ContextInteractionSwitcherClass, EItemInfoButton, IResult, PatchPostfix, IsInteractivePatch

### Community 138 - "RotateKeybindPatch"
Cohesion: 0.22
Nodes (6): DraggedItemView, IContainer, ItemContextAbstractClass, MethodBase, PatchPrefix, RotateKeybindPatch

### Community 139 - "MultiSelectDebug"
Cohesion: 0.36
Nodes (5): GUIContent, GUIStyle, ItemContextAbstractClass, Rect, MultiSelectDebug

### Community 140 - "AutofillQuestItemsPatch"
Cohesion: 0.25
Nodes (5): HandoverQuestItemsWindow, ScrollRectNoDrag, MethodBase, PatchPostfix, AutofillQuestItemsPatch

### Community 141 - ".Postfix"
Cohesion: 0.29
Nodes (5): InteractionButtonsContainer, EItemInfoButton, ItemInfoInteractionsAbstractClass, Sprite, AddShowHideModStatsButtonPatch

### Community 142 - "InventoryController"
Cohesion: 0.39
Nodes (3): NightVisionComponent, PatchPostfix, InventoryController

### Community 143 - "LastResizePatch"
Cohesion: 0.29
Nodes (6): ResizeData, MongoID, StashGridClass, LastResizePatch, ResizeData, XYCellSizeStruct

### Community 144 - "BeginDragPatch"
Cohesion: 0.25
Nodes (4): DraggedItemView, RectTransform, TextMeshProUGUI, BeginDragPatch

### Community 145 - "MoveTaskbarPatch"
Cohesion: 0.25
Nodes (5): MenuTaskBar, MethodBase, PatchPostfix, RectTransform, MoveTaskbarPatch

### Community 146 - "RemoveDoorActionsPatch"
Cohesion: 0.29
Nodes (4): ActionsReturnClass, MethodBase, PatchPostfix, RemoveDoorActionsPatch

### Community 147 - ".GetDeepAttributes"
Cohesion: 0.33
Nodes (4): ArmorComponent, IList, ItemAttributeClass, List

### Community 148 - ".GetClampedRepairAmount"
Cohesion: 0.29
Nodes (3): ArmorHolderComponent, RepairStrategy, RepairControllerClass

### Community 149 - "HideInviteUIPatch"
Cohesion: 0.29
Nodes (4): GroupPanel, MethodBase, PatchPrefix, HideInviteUIPatch

### Community 150 - "FixPlayerInspectPatch"
Cohesion: 0.29
Nodes (4): GroupPlayerViewModelClass, MethodBase, PatchPrefix, FixPlayerInspectPatch

### Community 151 - "ProductionPanel"
Cohesion: 0.29
Nodes (6): ProductionBuildAbstractClass, ValidationInputField, ProductionPanel, ProductionBuilds, SeachInputField, Type

### Community 152 - ".IsOnTop"
Cohesion: 0.33
Nodes (4): Rect, Transform, Vector2, TransformExtensions

### Community 153 - "IEnumerable"
Cohesion: 0.33
Nodes (3): IEnumerable, ItemView, MultiSelectExtensions

### Community 154 - ".Postfix"
Cohesion: 0.38
Nodes (4): BrowseCategoriesPanel, EViewListType, PatchPostfix, OfferViewList

### Community 155 - "FormatFullValuesPatch"
Cohesion: 0.29
Nodes (5): CodeInstruction, IEnumerable, MethodInfo, PatchTranspiler, FormatFullValuesPatch

### Community 156 - "TransferMergePatch"
Cohesion: 0.29
Nodes (4): ItemContextAbstractClass, MethodBase, PatchPostfix, TransferMergePatch

### Community 157 - "ButtonListener"
Cohesion: 0.33
Nodes (4): ButtonListener, DefaultUIButton, PatchPostfix, ButtonListener

### Community 158 - ".RemoveTrailingZeros"
Cohesion: 0.33
Nodes (3): UIFixes.Test, TestMethod, UnitTests

### Community 160 - "DisableInsureOnUninsurable"
Cohesion: 0.33
Nodes (3): ContextInteractionSwitcherClass, IResult, DisableInsureOnUninsurable

### Community 161 - ".PositionContextMenuFlyout"
Cohesion: 0.53
Nodes (3): ISubInteractions, SimpleContextMenu, SimpleContextMenuButton

### Community 162 - "ModifyUnsearchedContainerPatch"
Cohesion: 0.33
Nodes (3): MethodBase, PatchPostfix, ModifyUnsearchedContainerPatch

### Community 163 - "InventoryShowPatch"
Cohesion: 0.53
Nodes (3): IEnumerator, ItemContextAbstractClass, InventoryShowPatch

### Community 164 - "OfferViewList"
Cohesion: 0.33
Nodes (5): FiltersPanel, LightScroller, OfferViewList, Scroller, Type

### Community 165 - "RagfairNewOfferItemView"
Cohesion: 0.33
Nodes (5): GameObject, RagfairNewOfferItemView, SelectedBackground, SelectedMark, Type

### Community 166 - ".Postfix"
Cohesion: 0.40
Nodes (3): CommonUI, RagfairNewOfferItemView, AddOfferWindow

### Community 167 - ".Postfix"
Cohesion: 0.50
Nodes (3): EModLockedState, GStruct448, KeyValuePair

### Community 170 - "MultiSelectStrategy"
Cohesion: 0.50
Nodes (4): MultiSelectStrategy, FirstOpenSpace, OriginalSpacing, SameRowOrLower

## Knowledge Gaps
- **331 isolated node(s):** `netstandard2.1`, `Microsoft.NET.Sdk`, `LinkedSearchId`, `SlotName`, `ModGuid` (+326 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1043 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `UIFixes` connect `UIFixes` to `InspectWindowResizePatches`, `QuickMovePreview`, `ScrollPatches`, `MailPatches`, `GridWindowButtonsPatch`, `.IsEnabled`, `TraderAvatarPatches`, `CompoundItem`, `DropdownPatches`, `FixFleaPatches`, `HideoutSearchPatches`, `QuickAccessPanelPatches`, `NetworkTransactionWatcher`, `IsInWishlistPatch`, `TagPatches`, `ReloadInPlacePatches`, `WeaponModdingPatches`, `InputRepeater`, `.Postfix`, `ExtraRagfairOfferItemViewProperties`, `TradingAutoSwitchPatches`, `ContextMenuShortcutPatches`, `InternalMagPatches`, `.Prefix`, `WeaponPanPatches`, `BarterOfferPatches`, `.Enable`, `SettingExtensions`, `LoadMultipleMagazinesPatches`, `SettingTypes.cs`, `WeaponPresetConfirmPatches`, `.PatchPostfix`, `.Postfix`, `AddOfferClickablePricesPatches`, `EmptySlotMenu`, `UnloadAmmoPatches`, `Item`, `.InRaid`, `.Awake`, `.Prefix`, `HideoutLevelPatches`, `ArmorTooltipPatch`, `.Prefix`, `.UpdateQuickbindType`, `.R`, `RebindConsumablesPatches`, `.Prefix`, `BarrelOnlyPatches`, `InspectWindowStatsPatches`, `EditBuildScreenZoomPatch`, `WindowPatches`, `.Postfix`, `EmptySlotMenuTrigger`, `OpenInteractions`, `RevolverPatches`, `SyncScrollPositionPatches`, `PlaceOfferClickPatch`, `AutoOpenPatch`, `DefaultMaxRepairPatch`, `HideoutStashAdPatch`, `DebounceStatusNotifyPatch`, `FleaPrevSearchPatches`, `InputHelper`, `PaymentSlotsPatch`, `ClosePatch`, `ReopenMessagesPatch`, `WindowManager`, `RememberAutoselectPatch`, `TaskSerializer`, `HideoutCameraPatch`, `CompassGogglesPatch`, `MultiGrid`, `.AutoExpandCategories`, `.Prefix`, `UnlockCursorPatch`, `WeaponPreviewCameraNearClipPatch`, `.Prefix`, `AssortUnlocksPatch`, `OnDragEventPatch`, `RotateKeybindPatch`, `MultiSelectDebug`, `AutofillQuestItemsPatch`, `MoveTaskbarPatch`, `RemoveDoorActionsPatch`, `HideInviteUIPatch`, `FixPlayerInspectPatch`, `.IsOnTop`, `IEnumerable`, `TransferMergePatch`, `ModifyUnsearchedContainerPatch`, `InsuranceInteractions.cs`, `RepairInteractions.cs`?**
  _High betweenness centrality (0.123) - this node is a cross-community bridge._
- **Why does `Settings` connect `Settings` to `Sync`, `SettingTypes.cs`, `UIFixes`, `MultiSelectStrategy`, `.Awake`?**
  _High betweenness centrality (0.092) - this node is a cross-community bridge._
- **Why does `Type` connect `Type` to `TradingItemView`, `OfferViewList`, `RagfairNewOfferItemView`, `ItemUiContext`, `OnDragEventPatch`, `.Init`, `GridView`, `DragItemContext`, `.CanModify`, `RepairStrategy`, `R`, `DrawMultiSelect`, `ItemSpecificationPanel`, `ProductionPanel`, `OpenWindow`, `ItemMarketPricesPanel`, `FiltersPanel`, `QuestCache`?**
  _High betweenness centrality (0.060) - this node is a cross-community bridge._
- **What connects `netstandard2.1`, `Microsoft.NET.Sdk`, `LinkedSearchId` to the rest of the system?**
  _331 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Settings` be split into smaller, more focused modules?**
  _Cohesion score 0.014084507042253521 - nodes in this community are weakly interconnected._
- **Should `MultiSelectPatches` be split into smaller, more focused modules?**
  _Cohesion score 0.05185185185185185 - nodes in this community are weakly interconnected._
- **Should `InspectWindowResizePatches` be split into smaller, more focused modules?**
  _Cohesion score 0.051929824561403506 - nodes in this community are weakly interconnected._
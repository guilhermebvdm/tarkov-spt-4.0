// SPT 4.0 compatibility layer — type names.
//
// ORBIT 2.1 is written against SPT 4.1, whose client assembly is deobfuscated: game types carry their real
// names and namespaces. SPT 4.0.13 (EFT 0.16.9.40087) still ships the remapped assembly, where the same types
// are flat aliases (AmmoItemClass) or raw obfuscated names (GClass45).
//
// Each alias below lets the upstream sources keep the 4.1 name while compiling against the 4.0 type, so the
// diff against upstream stays small and every obfuscated name is pinned in this one file. Left = 4.1 name
// used by the sources, right = 4.0 type. Source of each pair: the official 4.0 -> 4.1 table
// (wiki/spt/modding/SPT_41_Modding/client/Class_Name_Mappings.md), read right to left, then checked against
// references/eft-decompiled (class shape and, for brain layers, the literal returned by Name()).
//
// What an alias cannot do, and is therefore edited at the use site instead:
//   - open generic types (AICoreAgent<>, AICoreStrategy<>, AICoreActionResult<,>, AICoreLayer<>, OperationResult<>);
//   - fully qualified 4.1 names (EFT.HandBook.Handbook, EFT.GlobalEvents.GlobalEventsController, ...);
//   - member names (BotMover._owner -> BotOwner_0, BotMover.CastFromPos -> method_10, ...);
//   - runtime type names compared as strings — see Spt40TypeNames.cs.
// The Orbit.Fika project compiles this same file (linked in Orbit.Fika.csproj).

// --- AI core / brain layers ---
global using AICoreController = AICoreControllerClass;
global using AssaultEnemyFarLayer = GClass45;                    // Name(): "AssaultEnemyFar"
global using AvoidDangerLayer = GClass48;                        // Name(): "AvoidDanger"
global using BaseLogicLayerSimple = BaseLogicLayerAbstractClass;
global using BirdEyePatrolLayer = GClass79;                      // Name(): "PtrlBirdEye"
global using BoarAvoidDangerLayer = GClass49;                    // Name(): "BoarGrenadeDanger"
global using CoreActionResultParams = GClass26;
global using ExfiltrationLayer = GClass75;                       // Name(): "Exfiltration"
global using PartisanPlantingTargetManyLayer = GClass124;        // Name(): "PartMineAll"

// --- Bot subsystems ---
global using BossPartisan = GClass442;
global using BotFirstAid = BotFirstAidClass;
global using BotMoverBTR = GClass493;
global using BotMoverImpostor = GClass494;
global using BotStimulators = GClass491;
global using BotSurgicalKit = GClass489;
global using GlobalEventDispatcher = BotEventHandler;
global using PatrolDropItemsNode = GClass264;
global using PatrolMoveSimple = GClass510;
global using PatrolPointChooserBoss = GClass557;
global using PatrolPointChooserBossGluhar = GClass558;
global using PatrolPointChooserByData = GClass560;
global using PatrolTakeItemsNode = GClass265;
global using PatrollingAlternative = GClass247;

// --- Inventory (4.1: EFT.InventoryLogic.*) ---
global using Ammo = AmmoItemClass;
global using Armor = ArmorItemClass;
global using Backpack = BackpackItemClass;
global using Headphones = HeadphonesItemClass;
global using Headwear = HeadwearItemClass;
global using IItemInHandsEventArgs = GInterface418;
global using IItemOperationResult = GInterface424;
global using IOperationResult = IRaiseEvents;
global using ItemController = TraderControllerClass;
global using ItemEventArgs = GEventArgs1;
global using ItemManipulator = InteractionsHandlerClass;
global using Magazine = MagazineItemClass;
global using Money = MoneyItemClass;
global using MoveResult = GClass3411;
global using RemoveFromHandsEventArgs = GEventArgs10;
global using SearchableItem = SearchableItemItemClass;
global using SetInHandsEventArgs = GEventArgs9;
global using ThrowWeap = ThrowWeapItemClass;
global using Vest = VestItemClass;

// --- World / misc ---
global using CameraManager = CameraClass;                        // 4.1: EFT.CameraControl.CameraManager
global using ClientAirDrop = AirdropLogicClass;                  // 4.1: EFT.Airdrop.ClientAirDrop
global using DamageInfo = DamageInfoStruct;                      // 4.1: EFT.Ballistics.DamageInfo
global using LayersMaskController = LayerMaskClass;
global using PhysicsExtensions = EFTPhysicsClass;

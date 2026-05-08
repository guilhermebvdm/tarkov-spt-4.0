using ChatShared;
using EFT;
using EFT.Ballistics;
using EFT.Communications;
using EFT.Game.Spawning;
using EFT.Hideout;
using EFT.InputSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Quests;
using EFT.Settings.Graphics;
using EFT.UI.Ragfair;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

public abstract class JsonSerializerSettingsClass
{
	public static readonly JsonConverter[] Converters = new JsonConverter[54]
	{
		new GClass1861<LootItemPositionClass, GClass1403>(),
		new GClass1861<GClass1402, GClass1403>(),
		new GClass1861<GClass1877, GClass1878>(),
		new GClass1861<NavGraphFindDebug, GClass390>(),
		new GClass1861<StashGridClass, GClass3121>(),
		new GClass1861<Slot, GClass3126>(),
		new GClass1861<StackSlot, GClass3128>(),
		new GClass1865<EPlayerSideMask>(),
		new GClass1865<ESpawnCategoryMask>(),
		new GClass1860<ELocationTransition>(),
		new GClass1872<GClass1869, ISpawnColliderParams, string>(),
		new GClass1872<GClass1871, Condition, string>(),
		new GClass1872<ItemTemplate.GClass1868, ItemTemplate, string>(),
		new GClass1872<GClass1870, GClass3672, string>(),
		new GClass1861<ConditionsDict, GClass4022>(),
		new GClass1861<GClass1405, GClass1406>(),
		new GClass1861<WeatherClass, GClass2606>(),
		new GClass1861<Vector3, GStruct52>(),
		new GClass1861<Vector3?, GStruct53>(),
		new GClass1861<Bounds, GStruct54>(),
		new GClass1866<EBodyPart>(),
		new GClass1866<MaterialType>(),
		new GClass1866<EBodyPartColliderType>(),
		new GClass1866<EArmorMaterial>(),
		new GClass1866<EProbabilityFunctionType>(),
		new GClass1866<ERequirementState>(),
		new GClass1866<EquipmentSlot>(),
		new GClass1866<EExfiltrationType>(),
		new GClass1866<WildSpawnType>(caseSensitive: false),
		new GClass1866<GClass4050.ERewardGameMode>(),
		new GClass1866<ECustomizationType>(),
		new GClass1866<ECustomizationSource>(),
		new GClass1861<EPlayerSideMask, GClass1864<EPlayerSideMask>>(),
		new GClass1866<ENotificationType>(),
		new GClass1866<EReportType>(),
		new GClass1866<EAntialiasingMode>(),
		new GClass1866<EEnvironmentUIType>(),
		new GClass1866<EGameKey>(),
		new GClass1861<GClass3387.GClass3448, GClass3387.FullItemLocationSerializer>(),
		new GClass1861<NotificationManagerClass, GClass2567>(),
		new GClass1861<GClass1056, GClass1057>(),
		new GClass1861<UpdatableChatMember, GClass1058>(),
		new GClass1861<ChatMessageClass.GClass1067, GClass1061.GClass1062>(),
		new GClass1861<GClass1065, GClass1061.GClass1063>(),
		new GClass1861<ChatMessageClass, GClass1061.GClass1064>(),
		new GClass1861<Offer, GClass2357>(),
		new GClass1861<HandoverRequirement, GClass2356>(),
		new GClass1866<ESortType>(),
		new GClass1861<Stage, GClass2316>(),
		new GClass1861<AreaTemplate, GClass2317>(),
		new GClass2318(),
		new GClass1863(),
		new StringEnumConverter(),
		new GClass1862()
	};

	public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
	{
		Converters = Converters
	};
}

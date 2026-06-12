using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using EFT.SynchronizableObjects;
using EFT.Vehicle;
using JsonType;
using UnityEngine;

namespace EFT;

public class LocalPlayer : Player
{
	[Serializable]
	[CompilerGenerated]
	public class Class1782
	{
		public static readonly Class1782 class1782_0 = new Class1782();

		public static Action action_0;

		public void method_0()
		{
		}
	}

	[CompilerGenerated]
	private GClass3010 gclass3010_0;

	private readonly HashSet<ETraderServiceType> hashSet_0 = new HashSet<ETraderServiceType>();

	private LocalPlayerCullingHandlerClass localPlayerCullingHandlerClass;

	private bool bool_0;

	private Profile.TraderInfo traderInfo_0;

	private SinglePlayerInventoryController singlePlayerInventoryController_0;

	public GClass3010 PlayerHealthController
	{
		[CompilerGenerated]
		get
		{
			return gclass3010_0;
		}
		[CompilerGenerated]
		set
		{
			gclass3010_0 = value;
		}
	}

	public static async Task<LocalPlayer> Create(GameWorld gameWorld, int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, EUpdateMode armsUpdateMode, EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, IViewFilter filter, ISession session, ELocalMode localMode, bool isYourPlayer, bool isBot)
	{
		bool useSimpleAnimator;
		ResourceKey assetName = ((useSimpleAnimator = profile.Info.Settings.UseSimpleAnimator) ? ResourceKeyManagerAbstractClass.ZOMBIE_BUNDLE_NAME : ResourceKeyManagerAbstractClass.PLAYER_BUNDLE_NAME);
		LocalPlayer localPlayer = Player.Create<LocalPlayer>(gameWorld, assetName, playerId, position, updateQueue, armsUpdateMode, bodyUpdateMode, characterControllerMode, getSensitivity, getAimingSensitivity, prefix, aiControl, useSimpleAnimator);
		localPlayer.IsYourPlayer = isYourPlayer;
		SinglePlayerInventoryController singlePlayerInventoryController = new SinglePlayerInventoryController(localPlayer, profile, isBot);
		localPlayer.PlayerHealthController = new GClass3010(profile.Health, localPlayer, singlePlayerInventoryController, profile.Skills, aiControl);
		LocalQuestControllerClass localQuestControllerClass = null;
		LocalPlayerAchievementControllerClass localPlayerAchievementControllerClass = null;
		LocalPrestigeControllerClass prestigeController = null;
		GClass3619 dialogController = null;
		if (!aiControl)
		{
			localQuestControllerClass = new GClass4007(profile, singlePlayerInventoryController, singlePlayerInventoryController.PlayerSearchController, session);
			localQuestControllerClass.Init();
			localPlayerAchievementControllerClass = new LocalPlayerAchievementControllerClass(profile, singlePlayerInventoryController, localQuestControllerClass.Quests, session, localMode == ELocalMode.PVE_OFFLINE);
			localPlayerAchievementControllerClass.Init();
			localPlayerAchievementControllerClass.Run();
			localQuestControllerClass.Run();
			prestigeController = new LocalPrestigeControllerClass(profile, singlePlayerInventoryController, localQuestControllerClass.Quests, session);
			dialogController = new GClass3619(profile, localQuestControllerClass, singlePlayerInventoryController);
		}
		await localPlayer.Init(rotation, layerName, pointOfView, profile, singlePlayerInventoryController, localPlayer.PlayerHealthController, statisticsManager, localQuestControllerClass, localPlayerAchievementControllerClass, prestigeController, dialogController, filter, EVoipState.NotAvailable, aiControl, async: false);
		localPlayer.singlePlayerInventoryController_0 = singlePlayerInventoryController;
		foreach (MagazineItemClass item in localPlayer.Inventory.GetPlayerItems(EPlayerItems.NonQuestItems).OfType<MagazineItemClass>())
		{
			localPlayer.InventoryController.StrictCheckMagazine(item, status: true, localPlayer.Profile.MagDrillsMastering, notify: false, useOperation: false);
		}
		foreach (ETraderServiceType key in Singleton<BackendConfigSettingsClass>.Instance.ServicesData.Keys)
		{
			localPlayer.hashSet_0.Add(key);
		}
		localPlayer._handsController = EmptyHandsController.smethod_6<EmptyHandsController>(localPlayer);
		localPlayer._handsController.Spawn(1f, delegate
		{
		});
		localPlayer.AIData = new PlayerAIDataClass(null, localPlayer);
		localPlayer.AggressorFound = false;
		localPlayer._animators[0].enabled = true;
		if (!localPlayer.IsYourPlayer)
		{
			localPlayer.localPlayerCullingHandlerClass = new LocalPlayerCullingHandlerClass();
			localPlayer.localPlayerCullingHandlerClass.Initialize(localPlayer, localPlayer.PlayerBones);
		}
		if (localPlayer.IsYourPlayer)
		{
			if (localPlayer.Profile.TryGetTraderInfo("638f541a29ffd1183d187f57", out var traderInfo))
			{
				localPlayer.traderInfo_0 = traderInfo;
				localPlayer.traderInfo_0.OnStandingChanged += localPlayer.method_163;
			}
			RadioTransmitterRecodableComponent radioTransmitterRecodableComponent = localPlayer.FindRadioTransmitter();
			if (radioTransmitterRecodableComponent != null)
			{
				radioTransmitterRecodableComponent.OnRadioTransmitterStatusChanged += localPlayer.method_164;
				if (GClass855.IsZero(localPlayer.Profile.GetTraderStanding("638f541a29ffd1183d187f57")))
				{
					radioTransmitterRecodableComponent.SetEncoded(value: false);
				}
			}
			if (!GClass867.Is(profile.Info.MemberCategory, EMemberCategory.Developer))
			{
				foreach (Item playerItem in localPlayer.Profile.Inventory.GetPlayerItems(EPlayerItems.Equipment))
				{
					if (playerItem.Name == "58ac60eb86f77401897560ff Name")
					{
						Debug.LogError(" ");
						while (true)
						{
						}
					}
				}
			}
		}
		return localPlayer;
	}

	public override void ManualUpdate(float deltaTime, float? platformDeltaTime = null, int loop = 1)
	{
		base.ManualUpdate(deltaTime, platformDeltaTime, loop);
		if (base.IsAI)
		{
			localPlayerCullingHandlerClass.ManualUpdate(deltaTime);
		}
	}

	public void Hide()
	{
		if (base.IsAI)
		{
			localPlayerCullingHandlerClass.Hide();
		}
	}

	public override void ProceedLocalAbsorbedDamage(ref DamageInfoStruct damageInfo, float absorbedDamage)
	{
		damageInfo.DidArmorDamage = absorbedDamage;
	}

	public override void OnBeenKilledByAggressor(IPlayer aggressor, DamageInfoStruct damageInfo, EBodyPart bodyPart, EDamageType lethalDamageType)
	{
		base.OnBeenKilledByAggressor(aggressor, damageInfo, bodyPart, lethalDamageType);
		Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(aggressor.ProfileId);
		if (alivePlayerByProfileID == null || !BotSettingsRepoClass.IsSectant(base.Profile.Info.Settings.Role) || !(alivePlayerByProfileID is LocalPlayer localPlayer) || !alivePlayerByProfileID.InventoryController.HasCultistAmulet(out var _))
		{
			return;
		}
		RadioTransmitterRecodableComponent radioTransmitterRecodableComponent = localPlayer.FindRadioTransmitter();
		if (localPlayer.traderInfo_0 != null)
		{
			if (localPlayer.traderInfo_0.Standing > 0.009999999776482582)
			{
				localPlayer.traderInfo_0.SetStanding(0.009999999776482582);
				radioTransmitterRecodableComponent?.SetStatus(RadioTransmitterStatus.Yellow);
			}
			else
			{
				localPlayer.traderInfo_0.SetStanding(0.0);
				radioTransmitterRecodableComponent?.SetEncoded(value: false);
			}
			localPlayer.singlePlayerInventoryController_0.TryExpendCultistAmuletChargeOrDestroy(destroy: true, onGameEnd: false);
		}
	}

	public override void OnGameSessionEnd(ExitStatus exitStatus, float pastTime, string locationId, string exitName)
	{
		if (base.IsYourPlayer)
		{
			if (singlePlayerInventoryController_0 != null)
			{
				singlePlayerInventoryController_0.TryExpendCultistAmuletChargeOrDestroy(destroy: false, onGameEnd: true);
			}
			RadioTransmitterRecodableComponent radioTransmitterRecodableComponent = FindRadioTransmitter();
			if (radioTransmitterRecodableComponent != null)
			{
				radioTransmitterRecodableComponent.OnRadioTransmitterStatusChanged -= method_164;
			}
			if (traderInfo_0 != null)
			{
				traderInfo_0.OnStandingChanged -= method_163;
			}
		}
		base.OnGameSessionEnd(exitStatus, pastTime, locationId, exitName);
	}

	public void method_163()
	{
		if (traderInfo_0.Standing < 0.0 || GClass855.IsZero(traderInfo_0.Standing))
		{
			singlePlayerInventoryController_0.TryExpendCultistAmuletChargeOrDestroy(destroy: true, onGameEnd: false);
		}
	}

	public void method_164(RadioTransmitterStatus status)
	{
		if (status == RadioTransmitterStatus.Red)
		{
			singlePlayerInventoryController_0.TryExpendCultistAmuletChargeOrDestroy(destroy: true, onGameEnd: false);
		}
	}

	public override void OnDead(EDamageType damageType)
	{
		if (base.IsAI)
		{
			localPlayerCullingHandlerClass.DisableCullingOnDead();
			GStruct368 dogtagInfo = method_165();
			SetDogtagInfo(dogtagInfo);
		}
		base.OnDead(damageType);
	}

	public override void ManageAggressor(DamageInfoStruct damageInfo, EBodyPart bodyPart, EBodyPartColliderType colliderType)
	{
		base.ManageAggressor(damageInfo, bodyPart, colliderType);
		if (base.IsAI && !base.HealthController.IsAlive)
		{
			GStruct368 dogtagInfo = method_165();
			SetDogtagInfo(dogtagInfo);
		}
	}

	public GStruct368 method_165()
	{
		GClass788 aggressor = base.Profile.EftStats.Aggressor;
		int inventoryHashSum = InventoryController.Inventory.CreateInventoryHashSum(new EquipmentSlot[1] { EquipmentSlot.SecuredContainer });
		double time = EFTDateTimeClass.ToUnixTime(EFTDateTimeClass.UtcNow);
		string accountId = base.Profile.AccountId;
		string id = base.Profile.Id;
		string nickname = base.Profile.Nickname;
		EPlayerSide side = base.Profile.Info.Side;
		string status = ((aggressor == null) ? "Died" : "Killed by");
		object obj;
		if (aggressor == null)
		{
			obj = null;
		}
		else
		{
			obj = aggressor.AccountId;
			if (obj != null)
			{
				goto IL_008a;
			}
		}
		obj = string.Empty;
		goto IL_008a;
		IL_008a:
		object obj2;
		if (aggressor == null)
		{
			obj2 = null;
		}
		else
		{
			obj2 = aggressor.ProfileId;
			if (obj2 != null)
			{
				goto IL_009f;
			}
		}
		obj2 = string.Empty;
		goto IL_009f;
		IL_00b4:
		object obj3;
		return new GStruct368(inventoryHashSum, time, accountId, id, nickname, (int)side, status, (string)obj, (string)obj2, (string)obj3, base.Profile.Info.Level, (aggressor != null) ? aggressor.WeaponName : "-", (int)base.Profile.Info.Settings.Role);
		IL_009f:
		if (aggressor == null)
		{
			obj3 = null;
		}
		else
		{
			obj3 = aggressor.Name;
			if (obj3 != null)
			{
				goto IL_00b4;
			}
		}
		obj3 = string.Empty;
		goto IL_00b4;
	}

	public override void OnSkillLevelChanged(AbstractSkillClass skill)
	{
		base.OnSkillLevelChanged(skill);
		if (!base.IsAI)
		{
			NotificationManagerClass.DisplayNotification(new GClass2549(skill));
		}
	}

	public override void OnWeaponMastered(MasterSkillClass masterSkill)
	{
		base.OnWeaponMastered(masterSkill);
		if (!base.IsAI)
		{
			NotificationManagerClass.DisplayMessageNotification(string.Format(GClass2348.Localized("MasteringLevelUpMessage"), GClass2348.Localized(masterSkill.MasteringGroup.Id), masterSkill.Level.ToString()));
		}
	}

	public override void vmethod_4(TripwireSynchronizableObject tripwire)
	{
		base.vmethod_4(tripwire);
		tripwire.DisableTripwire();
		UpdateInteractionCast();
	}

	public override void vmethod_3(TransitControllerAbstractClass controller, int transitPointId, string keyId, EDateTime time)
	{
		base.vmethod_3(controller, transitPointId, keyId, time);
		controller.InteractWithTransit(this, controller.GetInteractPacket(transitPointId, keyId, time));
		UpdateInteractionCast();
	}

	public override void vmethod_5(GClass2282 controller, int objectId, EventObject.EInteraction interaction)
	{
		base.vmethod_5(controller, objectId, interaction);
		controller.InteractWithEventObject(this, controller.GetInteractPacket(objectId, interaction));
		UpdateInteractionCast();
	}

	public override void PauseAllEffectsOnPlayer()
	{
		PlayerHealthController.PauseAllEffects();
	}

	public override void UnpauseAllEffectsOnPlayer()
	{
		PlayerHealthController.UnpauseAllEffects();
	}

	public void IgnoreDamage(bool flag)
	{
		if (flag)
		{
			ActiveHealthController activeHealthController = base.ActiveHealthController;
			if (activeHealthController != null && GClass855.IsZero(activeHealthController.DamageCoeff))
			{
				bool_0 = true;
				return;
			}
		}
		if (flag || !bool_0)
		{
			base.ActiveHealthController?.SetDamageCoeff((!flag) ? 1 : 0);
		}
	}

	public void SetWeaponRoot(Transform targetTransform)
	{
		base.PlayerBones.Weapon_Root_Anim.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);
	}

	public IEnumerable<TraderServicesClass> GetAvailableTraderServices(string traderId)
	{
		IEnumerable<BackendConfigSettingsClass.ServiceData> eligibleServices = Singleton<GameWorld>.Instance.TraderServicesHandler.GetEligibleServices(traderId, _questController, base.Profile.TradersInfo);
		foreach (BackendConfigSettingsClass.ServiceData item in eligibleServices)
		{
			if (item.ServiceType == ETraderServiceType.PlayerTaxi)
			{
				BTRControllerClass btrController = Singleton<GameWorld>.Instance.BtrController;
				bool canAfford = btrController != null && btrController.BtrVehicle.BtrState != EBtrState.Running;
				yield return new TraderServicesClass(item.ServiceType, item.TraderId, canAfford, !hashSet_0.Contains(item.ServiceType), item.ServiceItemCost, item.UniqueItems, item.SubServices);
			}
			else if (item.ServiceType != ETraderServiceType.BtrItemsDelivery && item.ServiceType != ETraderServiceType.BtrBotCover)
			{
				yield return new TraderServicesClass(item.ServiceType, item.TraderId, InteractionsHandlerClass.PurchaseTraderService(item, null, _questController, _inventoryController, simulate: true).Succeeded, !hashSet_0.Contains(item.ServiceType), item.ServiceItemCost, item.UniqueItems, item.SubServices);
			}
			else
			{
				bool canAfford2 = base.Profile.Side != EPlayerSide.Savage;
				yield return new TraderServicesClass(item.ServiceType, item.TraderId, canAfford2, !hashSet_0.Contains(item.ServiceType), item.ServiceItemCost, item.UniqueItems, item.SubServices);
			}
		}
	}

	public async Task UpdateBtrTraderServiceData()
	{
		List<TraderServicesClass> traderServices = GetAvailableTraderServices("656f0f98d80a697f855d34b1").ToList();
		await Task.Delay(1000);
		method_166(traderServices);
	}

	public async Task UpdateTradersServiceData(string traderID)
	{
		List<TraderServicesClass> traderServices = GetAvailableTraderServices(traderID).ToList();
		await Task.Yield();
		method_166(traderServices);
	}

	public void method_166(IEnumerable<TraderServicesClass> traderServices)
	{
		Dictionary<ETraderServiceType, BackendConfigSettingsClass.ServiceData> servicesData = Singleton<BackendConfigSettingsClass>.Instance.ServicesData;
		foreach (TraderServicesClass traderService in traderServices)
		{
			BackendConfigSettingsClass.ServiceData serviceData = null;
			if (servicesData.TryGetValue(traderService.ServiceType, out var _))
			{
				serviceData = new BackendConfigSettingsClass.ServiceData(traderService, servicesData[traderService.ServiceType].TraderServiceRequirements);
				servicesData[serviceData.ServiceType] = serviceData;
			}
			else
			{
				serviceData = new BackendConfigSettingsClass.ServiceData(traderService);
				servicesData.Add(serviceData.ServiceType, serviceData);
			}
			if (!base.Profile.TradersInfo.TryGetValue(serviceData.TraderId, out var value2))
			{
				Debug.LogError($"Can't find trader with id: {serviceData.TraderId}");
			}
			else
			{
				value2.SetServiceAvailability(serviceData.ServiceType, traderService.CanAfford, traderService.WasPurchasedInThisRaid);
			}
		}
	}

	public void ProcessTraderServicePurchase(ETraderServiceType serviceType)
	{
		if (!hashSet_0.Remove(serviceType))
		{
			Debug.LogError($"Service {serviceType} has already been purchased!");
		}
	}

	public void SetTraderServiceAvailability(ETraderServiceType serviceType, bool available)
	{
		if (available)
		{
			hashSet_0.Add(serviceType);
		}
		else
		{
			hashSet_0.Remove(serviceType);
		}
	}

	public override void vmethod_2(BTRSide btr, byte placeId, EInteractionType interaction)
	{
		base.vmethod_2(btr, placeId, interaction);
		PlayerInteractPacket interactWithBtrPacket = btr.GetInteractWithBtrPacket(placeId, interaction);
		if (Singleton<AbstractGame>.Instance.GameType == EGameType.Offline)
		{
			BTRControllerClass.Instance.LocalBtrInteraction(this, interactWithBtrPacket);
		}
	}

	public override void Dispose()
	{
		if (localPlayerCullingHandlerClass != null)
		{
			localPlayerCullingHandlerClass.Dispose();
		}
		base.Dispose();
	}
}

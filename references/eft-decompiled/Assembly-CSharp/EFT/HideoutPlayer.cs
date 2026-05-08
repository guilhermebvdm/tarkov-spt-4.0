using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

namespace EFT;

[UsedImplicitly]
public class HideoutPlayer : LocalPlayer
{
	public class Class1309<T> : GClass2059<T> where T : class, ITogglableComponentContainer<TogglableComponent>
	{
		[NonSerialized]
		public Func<T, T> Func_1;

		public override T Component => Func_1(base.Component);

		public Class1309(Slot slot, Func<T, T> togglableComponentGetter, Func<T, Action, Action> subscriber)
			: base(slot, subscriber)
		{
			Func_1 = togglableComponentGetter;
			Update();
		}
	}

	public class GClass2352 : GInterface401
	{
		public float Intensity => 1.17f;

		public NightVisionComponent.EMask Mask => NightVisionComponent.EMask.Binocular;

		public float MaskSize => 1.25f;

		public float NoiseIntensity => 0.02f;

		public float NoiseScale => 5f;

		public Color32 Color => new Color32(121, 232, 121, byte.MaxValue);

		public float DiffuseIntensity => 0f;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1717
	{
		public static readonly Class1717 class1717_0 = new Class1717();

		public static Action action_0;

		public static Func<ThermalVisionComponent, Action, Action> func_0;

		public static Func<NightVisionComponent, Action, Action> func_1;

		public static Func<FaceShieldComponent, Action, Action> func_2;

		public static Func<FaceShieldComponent, Action, Action> func_3;

		public void method_0()
		{
		}

		public Action method_1(ThermalVisionComponent tv, Action handler)
		{
			return tv.Togglable.OnChanged.Subscribe(handler);
		}

		public Action method_2(NightVisionComponent nv, Action handler)
		{
			return nv.Togglable.OnChanged.Subscribe(handler);
		}

		public Action method_3(FaceShieldComponent fs, Action handler)
		{
			Action togglableSub = fs.Togglable?.OnChanged.Subscribe(handler);
			Action hitSub = fs.HitsChanged.Subscribe(handler);
			return delegate
			{
				togglableSub?.Invoke();
				hitSub();
			};
		}

		public Action method_4(FaceShieldComponent fs, Action handler)
		{
			Action togglableSub = fs.Togglable?.OnChanged.Subscribe(handler);
			Action hitSub = fs.HitsChanged.Subscribe(handler);
			return delegate
			{
				togglableSub?.Invoke();
				hitSub();
			};
		}
	}

	[CompilerGenerated]
	public class Class1718
	{
		public HideoutPlayer hideoutPlayer_0;

		public Action nvUnsub;

		public Action tvUnsub;

		public ThermalVisionComponent method_0(ThermalVisionComponent originalThermalComponent)
		{
			if (hideoutPlayer_0.PointOfView != EPointOfView.FirstPerson)
			{
				return null;
			}
			return originalThermalComponent;
		}

		public NightVisionComponent method_1(NightVisionComponent originalNVComponent)
		{
			if (hideoutPlayer_0.PointOfView != EPointOfView.FirstPerson)
			{
				return hideoutPlayer_0.nightVisionComponent_0;
			}
			if (originalNVComponent != null)
			{
				return originalNVComponent;
			}
			if (hideoutPlayer_0.ThermalVisionObserver?.Component != null)
			{
				return null;
			}
			return hideoutPlayer_0.nightVisionComponent_0;
		}

		public void method_2()
		{
			nvUnsub();
			tvUnsub();
		}
	}

	[CompilerGenerated]
	public class Class1719
	{
		public Action togglableSub;

		public Action hitSub;

		public void method_0()
		{
			togglableSub?.Invoke();
			hitSub();
		}
	}

	[CompilerGenerated]
	public class Class1720
	{
		public Action togglableSub;

		public Action hitSub;

		public void method_0()
		{
			togglableSub?.Invoke();
			hitSub();
		}
	}

	[CompilerGenerated]
	public class Class1721
	{
		public Slot playerSlot;

		public bool method_0(Slot slot)
		{
			return slot.ID == playerSlot.ID;
		}
	}

	[CompilerGenerated]
	public class Class1722
	{
		public bool handsAreEmpty;

		public void method_0(Result<GInterface198> setEmptyHandsResult)
		{
			handsAreEmpty = true;
		}
	}

	private readonly NightVisionComponent nightVisionComponent_0 = new NightVisionComponent(null, new GClass2352(), new TogglableComponent(null));

	private bool bool_1;

	private PlayerInventoryController playerInventoryController_0;

	private Action action_0;

	private NightVisionComponent nightVisionComponent_1;

	[CompilerGenerated]
	private GClass3388 gclass3388_0;

	[CompilerGenerated]
	private bool bool_2 = true;

	[CompilerGenerated]
	private bool bool_3;

	public GClass3388 OriginalInventory
	{
		[CompilerGenerated]
		get
		{
			return gclass3388_0;
		}
		[CompilerGenerated]
		set
		{
			gclass3388_0 = value;
		}
	}

	public InventoryController ShootingRangeInventory => playerInventoryController_0;

	public bool IsInPatrol
	{
		[CompilerGenerated]
		get
		{
			return bool_2;
		}
		[CompilerGenerated]
		set
		{
			bool_2 = value;
		}
	}

	public bool IsUpdateHideoutPlayerInventoryInProgress
	{
		[CompilerGenerated]
		get
		{
			return bool_3;
		}
		[CompilerGenerated]
		set
		{
			bool_3 = value;
		}
	}

	public bool NightVisionActive
	{
		get
		{
			NightVisionComponent component = base.NightVisionObserver.Component;
			if (component == null || !component.Togglable.On)
			{
				return base.ThermalVisionObserver.Component?.Togglable.On ?? false;
			}
			return true;
		}
	}

	public override EPointOfView PointOfView
	{
		get
		{
			return _playerBody?.PointOfView.Value ?? EPointOfView.ThirdPerson;
		}
		set
		{
			base.PointOfView = value;
			base.ThermalVisionObserver.Update();
			base.NightVisionObserver.Update();
		}
	}

	public bool VisorVisibility
	{
		set
		{
			if (CameraClass.Instance.VisorEffect != null)
			{
				CameraClass.Instance.VisorEffect.Visible = value;
			}
		}
	}

	public override InventoryController InventoryController
	{
		get
		{
			if (!bool_1)
			{
				return base.InventoryController;
			}
			return playerInventoryController_0;
		}
	}

	public override void UpdateBreathStatus()
	{
		if (PointOfView == EPointOfView.FirstPerson)
		{
			base.UpdateBreathStatus();
		}
		else if (Speaker.Busy)
		{
			Speaker.Shut();
		}
	}

	public static async Task<HideoutPlayer> Create(GameWorld gameWorld, int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, EUpdateMode armsUpdateMode, EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, AbstractQuestControllerClass questController, AbstractAchievementControllerClass achievementsController, AbstractPrestigeControllerClass prestigeController, GClass3619 dialogController, HealthControllerClass healthController, GClass3388 inventoryController)
	{
		await smethod_2(profile, JobPriorityClass.Low);
		HideoutPlayer hideoutPlayer = Player.Create<HideoutPlayer>(gameWorld, ResourceKeyManagerAbstractClass.PLAYER_BUNDLE_NAME, playerId, position, updateQueue, armsUpdateMode, bodyUpdateMode, characterControllerMode, getSensitivity, getAimingSensitivity, prefix, aiControl);
		hideoutPlayer.OriginalInventory = inventoryController;
		hideoutPlayer.playerInventoryController_0 = new SinglePlayerInventoryController(hideoutPlayer, profile.Clone(), isBot: false, examined: true);
		hideoutPlayer.IsYourPlayer = true;
		await hideoutPlayer.Init(rotation, layerName, pointOfView, profile, hideoutPlayer.playerInventoryController_0, healthController, statisticsManager, questController, achievementsController, prestigeController, dialogController, new GClass1855(), EVoipState.NotAvailable, aiControl, async: false);
		hideoutPlayer.playerInventoryController_0.UnregisterView(hideoutPlayer);
		hideoutPlayer.OriginalInventory.RegisterView(hideoutPlayer);
		foreach (MagazineItemClass item in hideoutPlayer.Inventory.GetPlayerItems(EPlayerItems.NonQuestItems).OfType<MagazineItemClass>())
		{
			hideoutPlayer.InventoryController.StrictCheckMagazine(item, status: true, hideoutPlayer.Profile.MagDrillsMastering, notify: false, useOperation: false);
		}
		hideoutPlayer._handsController = EmptyHandsController.smethod_6<EmptyHandsController>(hideoutPlayer);
		hideoutPlayer._handsController.Spawn(1f, delegate
		{
		});
		hideoutPlayer.AIData = new PlayerAIDataClass(null, hideoutPlayer);
		hideoutPlayer.AggressorFound = false;
		hideoutPlayer._animators[0].enabled = true;
		hideoutPlayer.PointOfViewChanged.Subscribe(hideoutPlayer.UpdateBreathStatus);
		return hideoutPlayer;
	}

	public override void CreateSlotObservers()
	{
		base.ThermalVisionObserver = new Class1309<ThermalVisionComponent>(base.Equipment.GetSlot(EquipmentSlot.Headwear), (ThermalVisionComponent originalThermalComponent) => (PointOfView != EPointOfView.FirstPerson) ? null : originalThermalComponent, (ThermalVisionComponent tv, Action handler) => tv.Togglable.OnChanged.Subscribe(handler));
		base.NightVisionObserver = new Class1309<NightVisionComponent>(base.Equipment.GetSlot(EquipmentSlot.Headwear), delegate(NightVisionComponent originalNVComponent)
		{
			if (PointOfView != EPointOfView.FirstPerson)
			{
				return nightVisionComponent_0;
			}
			if (originalNVComponent != null)
			{
				return originalNVComponent;
			}
			return (base.ThermalVisionObserver?.Component != null) ? null : nightVisionComponent_0;
		}, (NightVisionComponent nv, Action handler) => nv.Togglable.OnChanged.Subscribe(handler));
		base.FaceShieldObserver = new GClass2059<FaceShieldComponent>(base.Equipment.GetSlot(EquipmentSlot.Headwear), delegate(FaceShieldComponent fs, Action handler)
		{
			Action togglableSub = fs.Togglable?.OnChanged.Subscribe(handler);
			Action hitSub = fs.HitsChanged.Subscribe(handler);
			return delegate
			{
				togglableSub?.Invoke();
				hitSub();
			};
		});
		base.FaceCoverObserver = new GClass2059<FaceShieldComponent>(base.Equipment.GetSlot(EquipmentSlot.FaceCover), delegate(FaceShieldComponent fs, Action handler)
		{
			Action togglableSub = fs.Togglable?.OnChanged.Subscribe(handler);
			Action hitSub = fs.HitsChanged.Subscribe(handler);
			return delegate
			{
				togglableSub?.Invoke();
				hitSub();
			};
		});
		Action tvUnsub = base.ThermalVisionObserver.Changed.Subscribe(method_168);
		Action nvUnsub = base.NightVisionObserver.Changed.Subscribe(method_167);
		action_0 = delegate
		{
			nvUnsub();
			tvUnsub();
		};
	}

	public void method_167()
	{
		NightVisionComponent component = base.NightVisionObserver.Component;
		bool flag = component?.Togglable.On ?? false;
		object obj;
		if (PointOfView != EPointOfView.FirstPerson)
		{
			NightVisionComponent itemComponent = base.NightVisionObserver.GetItemComponent();
			if (itemComponent == null)
			{
				obj = null;
			}
			else
			{
				obj = itemComponent.Togglable;
				if (obj != null)
				{
					goto IL_0058;
				}
			}
			obj = base.ThermalVisionObserver.GetItemComponent()?.Togglable;
			goto IL_0058;
		}
		if (base.ThermalVisionObserver.Component == null && ((component != nightVisionComponent_0 && nightVisionComponent_0.Togglable.On != flag) || (nightVisionComponent_1 != nightVisionComponent_0 && nightVisionComponent_1 != null && nightVisionComponent_0.Togglable.On)))
		{
			nightVisionComponent_0.Togglable.ToggleSilent();
		}
		nightVisionComponent_1 = component;
		return;
		IL_0058:
		TogglableComponent togglableComponent = (TogglableComponent)obj;
		if (togglableComponent == null || togglableComponent.On != flag)
		{
			togglableComponent?.ToggleSilent();
		}
	}

	public void method_168()
	{
		if (PointOfView != EPointOfView.FirstPerson)
		{
			return;
		}
		ThermalVisionComponent component = base.ThermalVisionObserver.Component;
		bool flag = component?.Togglable.On ?? false;
		if (nightVisionComponent_0.Togglable.On != flag)
		{
			if (component == null)
			{
				nightVisionComponent_0.Togglable.Toggle();
			}
			else
			{
				nightVisionComponent_0.Togglable.ToggleSilent();
			}
		}
		base.NightVisionObserver.Update();
	}

	public void ToggleNightVision()
	{
		ITogglableComponentContainer togglableComponentContainer = base.ThermalVisionObserver?.Component;
		object obj = togglableComponentContainer;
		if (obj == null)
		{
			GClass2059<NightVisionComponent> nightVisionObserver = base.NightVisionObserver;
			if (nightVisionObserver == null)
			{
				obj = null;
				return;
			}
			obj = nightVisionObserver.Component;
			if (obj == null)
			{
				return;
			}
		}
		else if (obj == null)
		{
			return;
		}
		((ITogglableComponentContainer)obj).Togglable.Toggle();
	}

	public override void OnGameSessionEnd(ExitStatus exitStatus, float pastTime, string locationId, string exitName)
	{
		action_0?.Invoke();
		base.MovementContext.PhysicalConditionChanged -= base.ProceduralWeaponAnimation.PhysicalConditionUpdated;
		_healthController.DiedEvent -= OnDead;
		OriginalInventory.UnregisterView(this);
		ExfilUnsubscribe();
		base.NightVisionObserver.Dispose();
		base.ThermalVisionObserver.Dispose();
		base.FaceShieldObserver.Dispose();
		base.FaceCoverObserver.Dispose();
		base.OnGameSessionEnd(exitStatus, pastTime, locationId, exitName);
	}

	public async Task UpdateHideoutPlayerInventory()
	{
		if (IsUpdateHideoutPlayerInventoryInProgress)
		{
			return;
		}
		bool_1 = true;
		while (base.MovementContext.BlockFirearms && bool_1)
		{
			await Task.Yield();
		}
		IsUpdateHideoutPlayerInventoryInProgress = true;
		if (!bool_1)
		{
			IsUpdateHideoutPlayerInventoryInProgress = false;
			return;
		}
		Item item = null;
		using (CounterCreatorAbstractClass.StartWithToken("UpdateHideoutPlayerInventory"))
		{
			using (CounterCreatorAbstractClass.StartWithToken("UpdateHideoutBundles"))
			{
				await smethod_2(base.Profile, JobPriorityClass.Low);
			}
			playerInventoryController_0.Inventory.FastAccess.BoundItems.Clear();
			IEnumerable<Slot> allSlots = GClass3372.GetAllSlots(playerInventoryController_0.Inventory.Equipment);
			IEnumerable<Slot> allSlots2 = GClass3372.GetAllSlots(OriginalInventory.Inventory.Equipment);
			Item itemInHands = playerInventoryController_0.ItemInHands;
			foreach (Slot playerSlot in allSlots)
			{
				Slot slot = allSlots2.First((Slot slot2) => slot2.ID == playerSlot.ID);
				if (slot.ContainedItem != null)
				{
					ItemAddress to = playerSlot.CreateItemAddress();
					Item item2 = GClass3380.CloneItem(slot.ContainedItem);
					if (playerSlot.ContainedItem == itemInHands)
					{
						item = item2;
					}
					GStruct154<GClass3405> gStruct = InteractionsHandlerClass.AddWithoutRestrictions(item2, to, playerInventoryController_0);
					if (gStruct.Failed)
					{
						UnityEngine.Debug.LogError(gStruct.Error);
					}
				}
			}
		}
		await Task.Yield();
		if (item == null)
		{
			InventoryEquipment equipment = playerInventoryController_0.Inventory.Equipment;
			Item item3 = equipment.GetSlot(EquipmentSlot.FirstPrimaryWeapon).ContainedItem ?? equipment.GetSlot(EquipmentSlot.SecondPrimaryWeapon).ContainedItem ?? equipment.GetSlot(EquipmentSlot.Holster).ContainedItem;
			if (item3 == null)
			{
				IsUpdateHideoutPlayerInventoryInProgress = false;
				return;
			}
			await method_169();
			SetItemInHands(item3, delegate(Result<IHandsController> result)
			{
				if (result.Failed)
				{
					UnityEngine.Debug.LogError(result.Error);
				}
				IsUpdateHideoutPlayerInventoryInProgress = false;
			});
			return;
		}
		await method_169();
		SetItemInHands(item, delegate(Result<IHandsController> result)
		{
			if (result.Failed)
			{
				UnityEngine.Debug.LogError(result.Error);
			}
			IsUpdateHideoutPlayerInventoryInProgress = false;
		});
	}

	public async Task ReleaseShootingRangeInventory()
	{
		while (IsUpdateHideoutPlayerInventoryInProgress)
		{
			await Task.Yield();
		}
		IsUpdateHideoutPlayerInventoryInProgress = true;
		(HandsController as IFirearmHandsController)?.SetTriggerPressed(pressed: false);
		FastForwardCurrentOperations();
		if (!(HandsController is EmptyHandsController))
		{
			if (base.ProceduralWeaponAnimation.IsGrenadeLauncher && HandsController is FirearmController firearmController)
			{
				firearmController.ToggleLauncher();
				await Task.Yield();
				FastForwardCurrentOperations();
				await Task.Yield();
			}
			bool handsAreEmpty = false;
			await method_169();
			SetEmptyHands(delegate
			{
				handsAreEmpty = true;
			});
			if (IsInPatrol)
			{
				FastForwardCurrentOperations();
			}
			else
			{
				while (!handsAreEmpty)
				{
					if (PointOfView != EPointOfView.FirstPerson)
					{
						FastForwardCurrentOperations();
					}
					await Task.Yield();
				}
			}
		}
		foreach (Slot allSlot in GClass3372.GetAllSlots(playerInventoryController_0.Inventory.Equipment))
		{
			if (allSlot.ContainedItem != null)
			{
				GStruct154<GClass3410> gStruct = InteractionsHandlerClass.RemoveWithoutRestrictions(allSlot.ContainedItem, playerInventoryController_0);
				if (gStruct.Failed)
				{
					UnityEngine.Debug.LogError(gStruct.Error);
				}
			}
		}
		base.GameWorld.DestroyAllLoot();
		base.MovementContext.RestorePreviousPatrol();
		IsUpdateHideoutPlayerInventoryInProgress = false;
		bool_1 = false;
	}

	public void SetPatrol(bool patrol)
	{
		IsInPatrol = patrol || !bool_1;
		(HandsController as IFirearmHandsController)?.SetTriggerPressed(pressed: false);
		base.MovementContext.BlockFirearms = IsInPatrol;
		if (IsInPatrol && HandsController is IFirearmHandsController firearmHandsController)
		{
			firearmHandsController.SetAim(value: false);
		}
	}

	public async Task method_169()
	{
		if (IsInPatrol)
		{
			HandsController.FirearmsAnimator.SetPatrol(b: false);
			await Task.Yield();
			FastForwardCurrentOperations();
		}
	}

	public static async Task smethod_2(Profile profile, GDelegate62 yield)
	{
		ResourceKey[] resources = profile.GetAllPrefabPaths().ToArray();
		await Singleton<PoolManagerClass>.Instance.LoadBundlesAndCreatePools(PoolManagerClass.PoolsCategory.Raid, PoolManagerClass.AssemblyType.Local, resources, yield);
	}

	public override void ExecuteSkill(Action action)
	{
	}

	public override void ExecuteShotSkill(Item weapon)
	{
	}

	public override void OnSkillLevelChanged(AbstractSkillClass skill)
	{
	}

	public override void OnWeaponMastered(MasterSkillClass masterSkill)
	{
	}

	public override void InitVaultingComponent(bool aiControlled)
	{
	}

	[Conditional("DEBUG")]
	[Conditional("CONSOLE")]
	public async void method_170()
	{
		int num = 0;
		while (true)
		{
			if (num < 220)
			{
				if (!IsUpdateHideoutPlayerInventoryInProgress)
				{
					break;
				}
				await Task.Yield();
				num++;
				continue;
			}
			UnityEngine.Debug.LogError("UpdateHideoutPlayerInventory probably stuck in hands switch operations again!");
			break;
		}
	}

	[CompilerGenerated]
	public void method_171(Result<IHandsController> result)
	{
		if (result.Failed)
		{
			UnityEngine.Debug.LogError(result.Error);
		}
		IsUpdateHideoutPlayerInventoryInProgress = false;
	}

	[CompilerGenerated]
	public void method_172(Result<IHandsController> result)
	{
		if (result.Failed)
		{
			UnityEngine.Debug.LogError(result.Error);
		}
		IsUpdateHideoutPlayerInventoryInProgress = false;
	}
}

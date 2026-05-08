using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public abstract class BotReload : GClass429
{
	public enum EReloadType
	{
		MagReload,
		AmmoReload
	}

	[Serializable]
	[CompilerGenerated]
	public class Class190
	{
		public static readonly Class190 class190_0 = new Class190();

		public static Func<GClass3393, bool> func_0;

		public bool method_0(GClass3393 x)
		{
			return x != null;
		}
	}

	[CompilerGenerated]
	public class Class191
	{
		public MagazineItemClass magModInWeapon;

		public BotReload botReload_0;

		public GClass3393 method_0(StashGridClass grid)
		{
			return grid.FindLocationForItem(magModInWeapon);
		}

		public void method_1(IResult result)
		{
			botReload_0.NextReloadTime = Time.time + 0.5f;
			botReload_0.Reloading = false;
			botReload_0.method_1();
		}
	}

	[CompilerGenerated]
	public class Class192
	{
		public BotReload botReload_0;

		public int stockCount;

		public MongoID ammoToAdd;

		public void method_0(IResult s)
		{
			botReload_0.NextReloadTime = Time.time + 0.5f;
			botReload_0.Reloading = false;
			botReload_0.AddAmmoToPockets(ammoToAdd, stockCount);
		}

		public void method_1(IResult s)
		{
			botReload_0.NextReloadTime = Time.time + 0.5f;
			botReload_0.Reloading = false;
			botReload_0.AddAmmoToPockets(ammoToAdd, stockCount);
		}

		public void method_2(IResult s)
		{
			botReload_0.NextReloadTime = Time.time + 0.5f;
			botReload_0.Reloading = false;
			botReload_0.AddAmmoToPockets(ammoToAdd, stockCount);
		}
	}

	[CompilerGenerated]
	public class Class193
	{
		public string templateId;

		public Func<Item, bool> func_0;

		public bool method_0(Item x)
		{
			return x.TemplateId == (MongoID)templateId;
		}

		public bool method_1(Item x)
		{
			if (x.TemplateId == (MongoID)templateId)
			{
				return x.StackObjectsCount < x.StackMaxSize;
			}
			return false;
		}

		public bool method_2(Item x)
		{
			return x.TemplateId == (MongoID)templateId;
		}
	}

	[CompilerGenerated]
	public class Class194
	{
		public Slot magazineSlot;

		public bool method_0(MagazineItemClass mag)
		{
			return GClass3124.CanAccept(magazineSlot, mag);
		}
	}

	[NonSerialized]
	public const int AMMO_RELOAD_MAX_TIME = 40;

	[NonSerialized]
	public const int MAG_RELOAD_MAX_TIME = 16;

	[NonSerialized]
	public const float PAUSE_BETWEEN_TRY_TO_RELOAD = 0.5f;

	[NonSerialized]
	public static EquipmentSlot[] AvailableEquipmentSlots = new EquipmentSlot[4]
	{
		EquipmentSlot.Pockets,
		EquipmentSlot.TacticalVest,
		EquipmentSlot.Backpack,
		EquipmentSlot.SecuredContainer
	};

	[NonSerialized]
	public static List<MagazineItemClass> PreallocatedMagazineList = new List<MagazineItemClass>();

	[NonSerialized]
	public static List<AmmoItemClass> PreallocatedAmmoList = new List<AmmoItemClass>(10);

	[NonSerialized]
	public Player Player;

	[NonSerialized]
	public float ReloadStartTime;

	[NonSerialized]
	public bool Reloading_1;

	[NonSerialized]
	public float NextReloadTime;

	[NonSerialized]
	public bool ReloadFailDebug;

	[NonSerialized]
	public EReloadType ReloadType;

	[NonSerialized]
	public float NextCanExtraLoadAmmo;

	[NonSerialized]
	public float NextPossibleTryStopReload;

	[NonSerialized]
	public Weapon Weapon;

	public IFirearmHandsController ShootController => BotOwner_0.WeaponManager.ShootController;

	[field: NonSerialized]
	public bool NoAmmoForReloadCached { get; set; }

	public int MaxBulletCount
	{
		get
		{
			if (ShootController == null)
			{
				return 0;
			}
			Weapon item = ShootController.Item;
			if (item.ReloadMode == Weapon.EReloadMode.OnlyBarrel)
			{
				return item.Chambers.Length;
			}
			return item.GetCurrentMagazine().MaxCount;
		}
	}

	public int BulletCount
	{
		get
		{
			if (ShootController == null)
			{
				return 0;
			}
			Weapon item = ShootController.Item;
			if (item.ReloadMode == Weapon.EReloadMode.OnlyBarrel)
			{
				return item.ChamberAmmoCount;
			}
			MagazineItemClass currentMagazine = item.GetCurrentMagazine();
			if (Reloading && ReloadType == EReloadType.AmmoReload && item.MustBoltBeOpennedForInternalReload)
			{
				return 1;
			}
			if (currentMagazine != null)
			{
				return currentMagazine.Count + item.ChamberAmmoCount;
			}
			return 0;
		}
	}

	public bool Reloading
	{
		get
		{
			return Reloading_1;
		}
		set
		{
			Reloading_1 = value;
			if (Reloading_1)
			{
				BotOwner_0.ShootData.EndShoot();
				ReloadStartTime = Time.time;
			}
		}
	}

	public BotReload(BotOwner owner, Weapon weapon)
		: base(owner)
	{
		Weapon = weapon;
		Player = owner.GetPlayer;
	}

	public static BotReload Create(BotOwner owner, Weapon weapon, EquipmentSlot slot)
	{
		if (slot == EquipmentSlot.Scabbard)
		{
			return new GClass462(owner, weapon);
		}
		if (weapon is RevolverItemClass)
		{
			return new GClass464(owner, weapon);
		}
		if (weapon.ReloadMode == Weapon.EReloadMode.ExternalMagazine)
		{
			return new GClass461(owner, weapon);
		}
		if (weapon.ReloadMode == Weapon.EReloadMode.OnlyBarrel)
		{
			return new GClass463(owner, weapon);
		}
		if (weapon.ReloadMode != Weapon.EReloadMode.InternalMagazine)
		{
			_ = weapon.ReloadMode;
		}
		return new GClass461(owner, weapon);
	}

	public virtual bool FightShallReload()
	{
		if (BotOwner_0.Medecine.Using)
		{
			return false;
		}
		if (!BotOwner_0.WeaponManager.IsWeaponReady)
		{
			return false;
		}
		if (BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction() && BotOwner_0.WeaponManager.Malfunctions.MalfunctionType() != Weapon.EMalfunctionState.Misfire)
		{
			return false;
		}
		if (Reloading)
		{
			bool flag;
			switch (BotOwner_0.Brain.LastDecision)
			{
			case BotLogicDecision.heal:
			case BotLogicDecision.healStimulators:
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				TryStopReload();
			}
			return true;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		int bulletCount = BulletCount;
		if (bulletCount <= 0)
		{
			return !BotOwner_0.WeaponManager.Selector.ShallChangeIfNoAmmo(goalEnemy);
		}
		int maxBulletCount = MaxBulletCount;
		float num = (float)bulletCount / (float)maxBulletCount;
		if (BotOwner_0.Memory.IsPeace)
		{
			return num < BotOwner_0.Settings.FileSettings.Shoot.RELOAD_PECNET_NO_ENEMY;
		}
		if (goalEnemy != null && goalEnemy.IsVisible)
		{
			return false;
		}
		if (goalEnemy != null && !goalEnemy.IsVisible && Time.time - goalEnemy.TimeLastSeen > BotOwner_0.Settings.FileSettings.Shoot.NOT_TO_SEE_ENEMY_TO_WANT_RELOAD_SEC)
		{
			return num < BotOwner_0.Settings.FileSettings.Shoot.NOT_TO_SEE_ENEMY_TO_WANT_RELOAD_PERCENT;
		}
		if (BotOwner_0.Memory.GoalTarget.HavePlaceTarget() && BotOwner_0.Memory.GoalEnemy == null)
		{
			return num < BotOwner_0.Settings.FileSettings.Shoot.RELOAD_PECNET_NO_ENEMY;
		}
		if (BotOwner_0.BotRequestController.HaveActivatedRequests())
		{
			return num < BotOwner_0.Settings.FileSettings.Shoot.RELOAD_PECNET_NO_ENEMY;
		}
		return false;
	}

	public void CheckReloadLongTime()
	{
		if (!Reloading)
		{
			return;
		}
		float num = Time.time - ReloadStartTime;
		int num2 = ((ReloadType == EReloadType.MagReload) ? 16 : 40);
		if (num > (float)num2 && !ReloadFailDebug)
		{
			ReloadFailDebug = true;
			if (ShootController != null && ShootController.Item != null)
			{
				_ = ShootController.Item.Id + "  " + ShootController.Item.Name;
			}
			Reloading = false;
		}
	}

	public bool CanReload(bool withCheckByPeriod)
	{
		MagazineItemClass foundMag;
		List<AmmoItemClass> ammoForInternalReload;
		return CanReload(withCheckByPeriod, out foundMag, out ammoForInternalReload);
	}

	public abstract bool CanReload(bool withCheckByPeriod, out MagazineItemClass foundMag, out List<AmmoItemClass> ammoForInternalReload);

	public bool CanReloadPreCheck(bool withCheckByPeriod, out MagazineItemClass currentMagazine)
	{
		currentMagazine = null;
		if (ShootController == null)
		{
			return false;
		}
		if (!FightShallReload())
		{
			return false;
		}
		if (withCheckByPeriod && NextReloadTime > Time.time)
		{
			return false;
		}
		currentMagazine = BotOwner_0.WeaponManager.CurrentWeapon.GetCurrentMagazine();
		if (currentMagazine != null && currentMagazine.MaxCount == BotOwner_0.WeaponManager.CurrentWeapon.GetCurrentMagazineCount())
		{
			return false;
		}
		if (Reloading)
		{
			float num = Time.time - ReloadStartTime;
			int num2 = ((ReloadType == EReloadType.MagReload) ? 16 : 40);
			if (num > (float)num2 && !ReloadFailDebug)
			{
				ReloadFailDebug = true;
				if (ShootController != null && ShootController.Item != null)
				{
					_ = ShootController.Item.Id + "  " + ShootController.Item.Name;
				}
			}
			return false;
		}
		if (BotOwner_0.Settings.FileSettings.Shoot.CHANGE_TO_MAIN_WHEN_SUPPORT_NO_AMMO && BotOwner_0.WeaponManager.Selector.TryChangeToMain())
		{
			return false;
		}
		return true;
	}

	public virtual void Reload()
	{
		if (ShootController == null || (BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction() && BotOwner_0.WeaponManager.Malfunctions.MalfunctionType() != Weapon.EMalfunctionState.Misfire))
		{
			return;
		}
		BotOwner_0.ShootData.EndShoot();
		_ = ShootController.Item;
		if (CanReload(withCheckByPeriod: false, out var foundMag, out var ammoForInternalReload))
		{
			Reloading = true;
			if (foundMag != null)
			{
				ReloadMagazine(foundMag);
			}
			if (ammoForInternalReload != null && ammoForInternalReload.Count > 0)
			{
				ReloadAmmo(ammoForInternalReload);
			}
		}
	}

	public virtual void ReloadMagazine(MagazineItemClass foundMag)
	{
		GClass3393 itemAddress = null;
		MagazineItemClass magModInWeapon = Weapon.GetCurrentMagazine();
		if (magModInWeapon != null)
		{
			itemAddress = (from grid in GClass3372.GetPrioritizedGridsForUnloadedObject(Player.InventoryController.Inventory.Equipment)
				select grid.FindLocationForItem(magModInWeapon)).FirstOrDefault((GClass3393 x) => x != null);
		}
		ReloadType = EReloadType.MagReload;
		BotOwner_0.BotTalk.Say(EPhraseTrigger.NeedAmmo);
		ShootController.ReloadMag(foundMag, itemAddress, delegate
		{
			NextReloadTime = Time.time + 0.5f;
			Reloading = false;
			method_1();
		});
	}

	public virtual void ReloadAmmo(List<AmmoItemClass> foundAmmo)
	{
		int stockCount = foundAmmo.Count;
		int num = ((ShootController.Item.ChamberAmmoCount == 0) ? 1 : 0);
		MagazineItemClass currentMagazine = ShootController.Item.GetCurrentMagazine();
		if (currentMagazine != null)
		{
			num += currentMagazine.MaxCount - currentMagazine.Count;
		}
		List<AmmoItemClass> list = new List<AmmoItemClass>();
		for (int i = 0; i < foundAmmo.Count; i++)
		{
			AmmoItemClass ammoItemClass = foundAmmo[i];
			if (num > 0)
			{
				stockCount = ammoItemClass.StackObjectsCount;
				list.Add(ammoItemClass);
				num -= ammoItemClass.StackObjectsCount;
				if (num <= 0)
				{
					break;
				}
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		MongoID ammoToAdd = list[0].TemplateId;
		ReloadType = EReloadType.AmmoReload;
		BotOwner_0.BotTalk.Say(EPhraseTrigger.NeedAmmo);
		if (Weapon.ReloadMode == Weapon.EReloadMode.OnlyBarrel)
		{
			ShootController.ReloadBarrels(new AmmoPackReloadingClass(list), null, delegate
			{
				NextReloadTime = Time.time + 0.5f;
				Reloading = false;
				AddAmmoToPockets(ammoToAdd, stockCount);
			});
		}
		else if (Weapon is RevolverItemClass)
		{
			ShootController.ReloadCylinderMagazine(new AmmoPackReloadingClass(list), delegate
			{
				NextReloadTime = Time.time + 0.5f;
				Reloading = false;
				AddAmmoToPockets(ammoToAdd, stockCount);
			}, quickReload: false);
		}
		else
		{
			ShootController.ReloadWithAmmo(new AmmoPackReloadingClass(list), delegate
			{
				NextReloadTime = Time.time + 0.5f;
				Reloading = false;
				AddAmmoToPockets(ammoToAdd, stockCount);
			});
		}
	}

	public void TryStopReload()
	{
		if (NextPossibleTryStopReload < Time.time)
		{
			NextPossibleTryStopReload = Time.time + 5f;
			BotOwner_0.ShootData.Shoot();
		}
	}

	public bool TryReload()
	{
		if (Reloading)
		{
			CheckReloadLongTime();
			return false;
		}
		if (CanReload(withCheckByPeriod: true, out var foundMag, out var _))
		{
			Reload();
			return true;
		}
		if (!NoAmmoForReloadCached && foundMag != null)
		{
			return false;
		}
		BotOwner_0.WeaponManager.Selector.TrySwitchToLauncherOrChangeWeapon();
		return false;
	}

	public bool method_0(AmmoItemClass ammo)
	{
		if (ammo.StackObjectsCount > 0 && GClass3124.CanAccept(ShootController.Item.Chambers[0], ammo))
		{
			return ammo.CheckAction(null).Succeeded;
		}
		return false;
	}

	public void TryUploadMagazine()
	{
		if (NextCanExtraLoadAmmo < Time.time)
		{
			NextCanExtraLoadAmmo = Time.time + 2f;
			method_1();
		}
	}

	public MagazineItemClass GetMagazineForReload(Weapon weapon)
	{
		Slot magazineSlot = weapon.GetMagazineSlot();
		PreallocatedMagazineList.Clear();
		Player.InventoryController.GetReachableItemsOfTypeNonAlloc(PreallocatedMagazineList);
		MagazineItemClass magazineItemClass = null;
		foreach (MagazineItemClass preallocatedMagazine in PreallocatedMagazineList)
		{
			if (preallocatedMagazine.Count != 0 && InteractionsHandlerClass.CheckMoveIgnoringTargetItem(preallocatedMagazine, magazineSlot, Player.InventoryController).Succeeded && (magazineItemClass == null || magazineItemClass.Count < preallocatedMagazine.Count))
			{
				magazineItemClass = preallocatedMagazine;
			}
		}
		if (magazineItemClass == null)
		{
			method_1();
		}
		PreallocatedMagazineList.Clear();
		return magazineItemClass;
	}

	public virtual void AddAmmoToPockets(string templateId, int targetCount)
	{
		if (Player == null || Player.HealthController == null || !Player.HealthController.IsAlive)
		{
			return;
		}
		InventoryController inventoryController = Player.InventoryController;
		Item containedItem = inventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.SecuredContainer).ContainedItem;
		if (containedItem == null)
		{
			return;
		}
		Item containedItem2 = inventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Pockets).ContainedItem;
		Item containedItem3 = inventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.TacticalVest).ContainedItem;
		Item item = GClass3380.GetAllItems(containedItem).FirstOrDefault((Item x) => x.TemplateId == (MongoID)templateId);
		if (item == null)
		{
			return;
		}
		Item item2 = GClass3380.GetAllItems(containedItem2).Concat((containedItem3 != null) ? GClass3380.GetAllItems(containedItem3) : Enumerable.Empty<Item>()).FirstOrDefault((Item x) => x.TemplateId == (MongoID)templateId && x.StackObjectsCount < x.StackMaxSize);
		if (item2 == null)
		{
			CompoundItem compoundItem = containedItem2 as CompoundItem;
			int num = 1;
			if (item.StackMaxSize <= 1)
			{
				num = 2;
			}
			if (0 < num)
			{
				item = GClass3380.GetAllItems(containedItem).FirstOrDefault((Item x) => x.TemplateId == (MongoID)templateId);
				if (item != null)
				{
					GStruct153 operationResult = compoundItem.Apply(inventoryController, item, item.StackObjectsCount, simulate: true);
					if (!operationResult.Failed)
					{
						inventoryController.TryRunNetworkTransaction(operationResult);
					}
				}
				return;
			}
		}
		int num2 = Mathf.Clamp(targetCount, 10, item2.StackMaxSize) - item2.StackObjectsCount;
		GStruct153 operationResult2 = ((item.StackObjectsCount > num2) ? ((GStruct153)InteractionsHandlerClass.TransferMax(item, item2, num2, inventoryController, simulate: true)) : ((GStruct153)InteractionsHandlerClass.Merge(item, item2, inventoryController, simulate: true)));
		inventoryController.TryRunNetworkTransaction(operationResult2);
	}

	public void method_1()
	{
		if (Player == null || !Player.HealthController.IsAlive || ShootController == null)
		{
			return;
		}
		Weapon item = ShootController.Item;
		if (item == null || item.ReloadMode != Weapon.EReloadMode.ExternalMagazine)
		{
			return;
		}
		Slot magazineSlot = item.GetMagazineSlot();
		if (magazineSlot == null)
		{
			return;
		}
		PreallocatedMagazineList.Clear();
		Player.InventoryController.GetReachableItemsOfTypeNonAlloc(PreallocatedMagazineList, (MagazineItemClass mag) => GClass3124.CanAccept(magazineSlot, mag));
		if (PreallocatedMagazineList.Count == 0)
		{
			_ = BotOwner_0.Profile.Info.Settings.UseSimpleAnimator;
			return;
		}
		for (int num = 0; num < PreallocatedMagazineList.Count; num++)
		{
			MagazineItemClass magazineItemClass = PreallocatedMagazineList[num];
			if (magazineItemClass.Count < magazineItemClass.MaxCount)
			{
				method_2(item, magazineItemClass);
			}
		}
		PreallocatedMagazineList.Clear();
	}

	public void method_2(Weapon weapon, MagazineItemClass foundMag)
	{
		if (!Player.HealthController.IsAlive)
		{
			return;
		}
		AmmoItemClass ammoItemClass = method_3(weapon, foundMag);
		if (ammoItemClass == null)
		{
			NoAmmoForReloadCached = true;
			return;
		}
		NoAmmoForReloadCached = false;
		GStruct153 operationResult = foundMag.Apply(BotOwner_0.GetPlayer.InventoryController, ammoItemClass, ammoItemClass.StackObjectsCount, simulate: true);
		BotOwner_0.GetPlayer.InventoryController.TryRunNetworkTransaction(operationResult, delegate(IResult result)
		{
			_ = result.Failed;
		});
	}

	public AmmoItemClass method_3(Weapon weapon, MagazineItemClass foundMag)
	{
		Slot slot = (weapon.HasChambers ? weapon.Chambers[0] : null);
		PreallocatedAmmoList.Clear();
		Player.InventoryController.GetAcceptableItemsNonAlloc(AvailableEquipmentSlots, PreallocatedAmmoList);
		AmmoItemClass ammoItemClass = null;
		foreach (AmmoItemClass preallocatedAmmo in PreallocatedAmmoList)
		{
			if (((slot != null && GClass3124.CanAccept(slot, preallocatedAmmo)) || GClass3124.CheckItemFilter(foundMag.Cartridges.Filters, preallocatedAmmo)) && (ammoItemClass == null || ammoItemClass.StackObjectsCount < preallocatedAmmo.StackObjectsCount))
			{
				ammoItemClass = preallocatedAmmo;
			}
		}
		PreallocatedAmmoList.Clear();
		return ammoItemClass;
	}

	public virtual void ManualUpdate()
	{
	}

	public void TryFillMagazines()
	{
		method_1();
	}

	[CompilerGenerated]
	public static void smethod_0(IResult result)
	{
		_ = result.Failed;
	}
}

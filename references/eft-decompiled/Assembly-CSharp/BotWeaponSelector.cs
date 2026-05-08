using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class BotWeaponSelector : GClass429
{
	[Serializable]
	[CompilerGenerated]
	public class Class207
	{
		public static readonly Class207 class207_0 = new Class207();

		public static Func<string, Slot, string> func_0;

		public string method_0(string current, Slot slot)
		{
			return current + "  id:" + slot.ID + "   item:" + (slot.ContainedItem != null);
		}
	}

	[CompilerGenerated]
	public class Class208
	{
		public BotWeaponSelector botWeaponSelector_0;

		public EquipmentSlot slot;

		public Action<Result<IHandsController>> callback;

		public void method_0(Result<IHandsController> x)
		{
			botWeaponSelector_0.StartChangeTime = -1f;
			botWeaponSelector_0.LastEquipmentSlot_1 = slot;
			botWeaponSelector_0.OnActiveEquipmentSlotChanged?.Invoke(slot);
			botWeaponSelector_0.OnWeaponTaken(x);
			callback?.Invoke(x);
		}
	}

	[NonSerialized]
	public const int MAX_ERRORS = 20;

	[NonSerialized]
	public const float CHANGE_WEAPON_PERIOD = 25f;

	[NonSerialized]
	public Item FirstPrimaryWeaponItem_1;

	[NonSerialized]
	public Item SecondPrimaryWeaponItem_1;

	[NonSerialized]
	public EquipmentSlot LastEquipmentSlot_1;

	[NonSerialized]
	public bool CanChangeToSecondWeapons_1;

	[NonSerialized]
	public Item HolsterItem;

	[NonSerialized]
	public Item Melee;

	[NonSerialized]
	public EquipmentSlot MainWeapon;

	[NonSerialized]
	public EquipmentSlot SecondWeapon = EquipmentSlot.SecondPrimaryWeapon;

	[NonSerialized]
	public EquipmentSlot MeleeWeapon = EquipmentSlot.Scabbard;

	[NonSerialized]
	public EquipmentSlot SupportWeapon;

	[NonSerialized]
	public bool ErrorStuckLog;

	[NonSerialized]
	public bool IsFound;

	[NonSerialized]
	public bool CanChangeToSupportWeapons;

	[NonSerialized]
	public float NextChangeTime;

	[NonSerialized]
	public int ErrorCounter;

	[NonSerialized]
	public float PrevChangeWeaponTime;

	[NonSerialized]
	public float StartChangeTime;

	public Action<EquipmentSlot> OnActiveEquipmentSlotChanged;

	public Item FirstPrimaryWeaponItem => FirstPrimaryWeaponItem_1;

	public Item SecondPrimaryWeaponItem => SecondPrimaryWeaponItem_1;

	public EquipmentSlot LastEquipmentSlot => LastEquipmentSlot_1;

	public EquipmentSlot EquipmentSlot => LastEquipmentSlot_1;

	[field: NonSerialized]
	public bool CanChangeToMeleeWeapons { get; set; }

	[field: NonSerialized]
	public bool IsWeaponReady { get; set; }

	[field: NonSerialized]
	public bool IsChanging { get; set; }

	public float Single_0 => BotOwner_0.Settings.FileSettings.Shoot.CHANGE_WEAPON_PERIOD;

	public bool CanChangeToSecondWeapons => CanChangeToSecondWeapons_1;

	public BotWeaponSelector(BotOwner owner)
		: base(owner)
	{
	}

	public virtual void Activate()
	{
	}

	public bool TakePrevWeapon()
	{
		if (LastEquipmentSlot_1 == EquipmentSlot.SecondPrimaryWeapon)
		{
			return method_0();
		}
		return ChangeToMain();
	}

	public bool CanChangeWeaponCauseEnemyDistance()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return false;
		}
		if (!(goalEnemy.Distance > BotOwner_0.Settings.FileSettings.Shoot.LOW_DIST_TO_CHANGE_WEAPON))
		{
			return false;
		}
		if (!(goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Shoot.FAR_DIST_TO_CHANGE_WEAPON))
		{
			return false;
		}
		return true;
	}

	public void TakeMainWeapon()
	{
		SetSlotItem(OnWeaponTaken, order: true);
	}

	public bool TryChangeWeapon(bool ignoreDistance = false)
	{
		if (LastEquipmentSlot_1 == MainWeapon && (ignoreDistance || BotOwner_0.WeaponManager.Selector.CanChangeWeaponCauseEnemyDistance()))
		{
			return method_0();
		}
		return ChangeToMain();
	}

	public bool TryChangeToMain()
	{
		if (CanChangeToSupportWeapons && LastEquipmentSlot_1 == SupportWeapon)
		{
			if (BotOwner_0.Memory.GoalEnemy == null)
			{
				return ChangeToMain();
			}
			if (BotOwner_0.Memory.GoalEnemy.Distance > BotOwner_0.Settings.FileSettings.Shoot.DIST_TO_CHANGE_TO_MAIN)
			{
				return ChangeToMain();
			}
		}
		return false;
	}

	public void TrySwitchToLauncherOrChangeWeapon()
	{
		BotWeaponManager weaponManager = BotOwner_0.WeaponManager;
		BotUnderbarrelLauncherController underbarrelLauncherController = weaponManager.UnderbarrelLauncherController;
		if (underbarrelLauncherController.CanUseInsteadOfReload())
		{
			underbarrelLauncherController.TryEnable();
		}
		else if (BotOwner_0.Memory.HaveEnemy)
		{
			weaponManager.Selector.TryChangeWeapon(ignoreDistance: true);
		}
		else
		{
			underbarrelLauncherController.TryEnableReloadDisable();
		}
	}

	public bool method_0()
	{
		if (!CanChangeToSupportWeapons)
		{
			return false;
		}
		return TryChangeToSlot(SupportWeapon, changeToMain: false);
	}

	public void ChangeToMelee()
	{
		TryChangeToSlot(MeleeWeapon, changeToMain: false);
	}

	public bool ChangeToMain()
	{
		return TryChangeToSlot(MainWeapon, changeToMain: true);
	}

	public bool ChangeToSecond(Action<Result<IHandsController>> callback = null)
	{
		return TryChangeToSlot(SecondWeapon, changeToMain: false, callback);
	}

	public virtual bool TryChangeToSlot(EquipmentSlot slot, bool changeToMain, Action<Result<IHandsController>> callback = null)
	{
		if (!IsWeaponReady)
		{
			return false;
		}
		bool flag = changeToMain || ((slot == EquipmentSlot.Scabbard) ? CanChangeToMeleeWeapons : ((slot != EquipmentSlot.SecondPrimaryWeapon) ? CanChangeToSupportWeapons : CanChangeToSecondWeapons));
		if (flag && !BotOwner_0.WeaponManager.Reload.Reloading && NextChangeTime < Time.time)
		{
			NextChangeTime = Time.time + Single_0;
			IsWeaponReady = false;
			StartChangeTime = Time.time;
			BotOwner_0.WeaponManager.Reload.TryFillMagazines();
			BotOwner_0.GetPlayer.SetSlotItem(slot, delegate(Result<IHandsController> x)
			{
				StartChangeTime = -1f;
				LastEquipmentSlot_1 = slot;
				OnActiveEquipmentSlotChanged?.Invoke(slot);
				OnWeaponTaken(x);
				callback?.Invoke(x);
			});
			return true;
		}
		return false;
	}

	public virtual void OnWeaponTaken(Result<IHandsController> x)
	{
		IsChanging = false;
		bool allFine = false;
		if (x.Succeed)
		{
			BotOwner_0.WeaponManager.UpdateHandsController(x.Value, out allFine);
		}
		if (BotOwner_0.BotState != EBotState.Active)
		{
			if (BotOwner_0.BotState != EBotState.PreActive)
			{
				BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 0.5f, TakeMainWeapon);
			}
		}
		else if (!allFine)
		{
			ErrorCounter++;
			if (ErrorCounter < 20)
			{
				BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 0.5f, TakeMainWeapon);
			}
		}
	}

	public virtual void SetSlotItem(Callback<IHandsController> onSpawn, bool order)
	{
		FindItemsInSlots(anyway: false);
		IsChanging = true;
		Player getPlayer = BotOwner_0.GetPlayer;
		if (order)
		{
			if (FirstPrimaryWeaponItem_1 != null)
			{
				if (getPlayer.HandsController.Item == FirstPrimaryWeaponItem_1)
				{
					onSpawn(new Result<IHandsController>(getPlayer.HandsController));
					return;
				}
				MainWeapon = EquipmentSlot.FirstPrimaryWeapon;
				getPlayer.SetSlotItem(MainWeapon, onSpawn);
				return;
			}
			if (SecondPrimaryWeaponItem_1 != null)
			{
				if (getPlayer.HandsController.Item == SecondPrimaryWeaponItem_1)
				{
					onSpawn(new Result<IHandsController>(getPlayer.HandsController));
					return;
				}
				MainWeapon = EquipmentSlot.SecondPrimaryWeapon;
				getPlayer.SetSlotItem(EquipmentSlot.SecondPrimaryWeapon, onSpawn);
				return;
			}
		}
		else
		{
			if (SecondPrimaryWeaponItem_1 != null)
			{
				if (getPlayer.HandsController.Item == SecondPrimaryWeaponItem_1)
				{
					onSpawn(new Result<IHandsController>(getPlayer.HandsController));
				}
				else
				{
					getPlayer.SetSlotItem(EquipmentSlot.SecondPrimaryWeapon, onSpawn);
				}
				return;
			}
			if (FirstPrimaryWeaponItem_1 != null)
			{
				if (getPlayer.HandsController.Item == FirstPrimaryWeaponItem_1)
				{
					onSpawn(new Result<IHandsController>(getPlayer.HandsController));
				}
				else
				{
					getPlayer.SetSlotItem(EquipmentSlot.FirstPrimaryWeapon, onSpawn);
				}
				return;
			}
		}
		if (HolsterItem != null)
		{
			if (getPlayer.HandsController.Item == HolsterItem)
			{
				onSpawn(new Result<IHandsController>(getPlayer.HandsController));
				return;
			}
			MainWeapon = EquipmentSlot.Holster;
			getPlayer.SetSlotItem(EquipmentSlot.Holster, onSpawn);
			return;
		}
		string text = getPlayer.InventoryController.Inventory.Equipment.AllSlots.Aggregate("", (string current, Slot slot) => current + "  id:" + slot.ID + "   item:" + (slot.ContainedItem != null));
		Debug.LogError("Bot can't find weapon for self: " + text);
	}

	public void UpdateWeaponsList()
	{
		FindItemsInSlots(anyway: true);
	}

	public virtual void FindItemsInSlots(bool anyway)
	{
		if (!IsFound || anyway)
		{
			InventoryEquipment equipment = BotOwner_0.GetPlayer.InventoryController.Inventory.Equipment;
			FirstPrimaryWeaponItem_1 = equipment.GetSlot(EquipmentSlot.FirstPrimaryWeapon).ContainedItem;
			SecondPrimaryWeaponItem_1 = equipment.GetSlot(EquipmentSlot.SecondPrimaryWeapon).ContainedItem;
			HolsterItem = equipment.GetSlot(EquipmentSlot.Holster).ContainedItem;
			Melee = equipment.GetSlot(EquipmentSlot.Scabbard).ContainedItem;
			CanChangeToMeleeWeapons = Melee != null;
			IsFound = true;
			Weapon weapon = null;
			Weapon weapon2 = null;
			if (SecondPrimaryWeaponItem_1 is Weapon weapon3)
			{
				SupportWeapon = EquipmentSlot.SecondPrimaryWeapon;
				SecondWeapon = EquipmentSlot.SecondPrimaryWeapon;
				weapon2 = (weapon = weapon3);
			}
			else if (HolsterItem is Weapon weapon4)
			{
				SupportWeapon = EquipmentSlot.Holster;
				weapon = weapon4;
			}
			CanChangeToSupportWeapons = FirstPrimaryWeaponItem_1 is Weapon && weapon != null;
			CanChangeToSecondWeapons_1 = weapon2 != null;
		}
	}

	public void TryChangeWeaponCauseNoAmmo()
	{
		bool flag;
		if (!(flag = LastEquipmentSlot_1 == MainWeapon))
		{
			CanChangeToSecondWeapons_1 = false;
			CanChangeToSupportWeapons = false;
		}
		if (BotOwner_0.Settings.FileSettings.Shoot.TRY_CHANGE_WEAPON_WHEN_RELOAD || !flag)
		{
			TrySwitchToLauncherOrChangeWeapon();
		}
	}

	public bool ShallChangeIfNoAmmo(EnemyInfo goalEnemy)
	{
		if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.CanUseInsteadOfReload())
		{
			return true;
		}
		if (goalEnemy == null)
		{
			return false;
		}
		if (!goalEnemy.IsVisible)
		{
			return false;
		}
		if (!CanChangeToSupportWeapons)
		{
			return false;
		}
		if (LastEquipmentSlot_1 != MainWeapon)
		{
			return false;
		}
		if (PrevChangeWeaponTime + 25f > Time.time)
		{
			return false;
		}
		if (!(goalEnemy.Distance > BotOwner_0.Settings.FileSettings.Shoot.LOW_DIST_TO_CHANGE_WEAPON))
		{
			return false;
		}
		if (!(goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Shoot.FAR_DIST_TO_CHANGE_WEAPON))
		{
			return false;
		}
		PrevChangeWeaponTime = Time.time;
		if (SupportWeapon == EquipmentSlot.Holster && BotOwner_0.WeaponManager.PistolWeaponInfo != null)
		{
			BotWeaponInfo pistolWeaponInfo = BotOwner_0.WeaponManager.PistolWeaponInfo;
			if (pistolWeaponInfo != null && pistolWeaponInfo.BulletCount == 0)
			{
				return false;
			}
		}
		if (SupportWeapon == EquipmentSlot.SecondPrimaryWeapon && BotOwner_0.WeaponManager.SecondWeaponInfo != null && BotOwner_0.WeaponManager.SecondWeaponInfo.BulletCount == 0)
		{
			return false;
		}
		if (GClass856.IsTrue100(goalEnemy.Person.AIData.HaveHelmet ? BotOwner_0.Settings.FileSettings.Shoot.CHANCE_TO_CHANGE_WEAPON_WITH_HELMET : BotOwner_0.Settings.FileSettings.Shoot.CHANCE_TO_CHANGE_WEAPON))
		{
			method_0();
			return true;
		}
		return false;
	}

	public virtual void ManualUpdate()
	{
		if (!ErrorStuckLog && StartChangeTime > 0f && Time.time - StartChangeTime > 20f)
		{
			ErrorStuckLog = true;
		}
		if (CanChangeToSupportWeapons && IsWeaponReady && LastEquipmentSlot_1 == SupportWeapon)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy == null)
			{
				ChangeToMain();
			}
			else if (Time.time - goalEnemy.TimeLastSeen > 30f)
			{
				ChangeToMain();
			}
		}
	}

	public void Dispose()
	{
	}
}

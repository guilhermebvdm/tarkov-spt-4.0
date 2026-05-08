using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass461 : BotReload
{
	[CompilerGenerated]
	public class Class195
	{
		public Weapon weapon;

		public bool method_0(AmmoItemClass ammo)
		{
			if (ammo.StackObjectsCount > 0 && GClass3124.CanAccept(weapon.Chambers[0], ammo))
			{
				return ammo.CheckAction(null).Succeeded;
			}
			return false;
		}
	}

	public GClass461(BotOwner owner, Weapon weapon)
		: base(owner, weapon)
	{
	}

	public override bool CanReload(bool withCheckByPeriod, out MagazineItemClass foundMag, out List<AmmoItemClass> ammoForInternalReload)
	{
		foundMag = null;
		ammoForInternalReload = null;
		if (base.ShootController == null)
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
		if (BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction() && BotOwner_0.WeaponManager.Malfunctions.MalfunctionType() != Weapon.EMalfunctionState.Misfire)
		{
			return false;
		}
		MagazineItemClass currentMagazine = BotOwner_0.WeaponManager.CurrentWeapon.GetCurrentMagazine();
		if (currentMagazine != null && currentMagazine.MaxCount == BotOwner_0.WeaponManager.CurrentWeapon.GetCurrentMagazineCount())
		{
			return false;
		}
		if (base.Reloading)
		{
			float num = Time.time - ReloadStartTime;
			int num2 = ((ReloadType == EReloadType.MagReload) ? 16 : 40);
			if (num > (float)num2 && !ReloadFailDebug)
			{
				ReloadFailDebug = true;
				if (base.ShootController != null && base.ShootController.Item != null)
				{
					_ = base.ShootController.Item.Id + "  " + base.ShootController.Item.Name;
				}
			}
			return false;
		}
		if (BotOwner_0.Settings.FileSettings.Shoot.CHANGE_TO_MAIN_WHEN_SUPPORT_NO_AMMO && BotOwner_0.WeaponManager.Selector.TryChangeToMain())
		{
			return false;
		}
		Weapon weapon = base.ShootController.Item;
		if (weapon.ReloadMode == Weapon.EReloadMode.ExternalMagazine)
		{
			if (!(BotOwner_0.GetPlayer.HandsController is IFirearmHandsController))
			{
				NextReloadTime = Time.time + 0.5f;
				return false;
			}
			foundMag = GetMagazineForReload(weapon);
			if (foundMag == null)
			{
				BotOwner_0.WeaponManager.Selector.TrySwitchToLauncherOrChangeWeapon();
				NextReloadTime = Time.time + 0.5f;
				return false;
			}
			TryUploadMagazine();
			NextReloadTime = Time.time + 0.5f;
			return base.ShootController.CanStartReload();
		}
		if (weapon.ReloadMode == Weapon.EReloadMode.ExternalMagazineWithInternalReloadSupport || (weapon.ReloadMode == Weapon.EReloadMode.InternalMagazine && currentMagazine == null))
		{
			MagazineItemClass magazineForReload = GetMagazineForReload(weapon);
			if (magazineForReload != null)
			{
				foundMag = magazineForReload;
				NextReloadTime = Time.time + 0.5f;
				return base.ShootController.CanStartReload();
			}
			if (currentMagazine == null)
			{
				BotOwner_0.WeaponManager.Selector.TrySwitchToLauncherOrChangeWeapon();
				NextReloadTime = Time.time + 0.5f;
				return false;
			}
		}
		BotReload.PreallocatedAmmoList.Clear();
		Player.InventoryController.GetAcceptableItemsNonAlloc(BotReload.AvailableEquipmentSlots, BotReload.PreallocatedAmmoList, (AmmoItemClass ammo) => ammo.StackObjectsCount > 0 && GClass3124.CanAccept(weapon.Chambers[0], ammo) && ammo.CheckAction(null).Succeeded);
		if (BotReload.PreallocatedAmmoList.Count > 0)
		{
			ammoForInternalReload = BotReload.PreallocatedAmmoList.ToList();
			NextReloadTime = Time.time + 0.5f;
			return base.ShootController.CanStartReload();
		}
		BotOwner_0.WeaponManager.Selector.TryChangeWeaponCauseNoAmmo();
		NextReloadTime = Time.time + 0.5f;
		return false;
	}
}

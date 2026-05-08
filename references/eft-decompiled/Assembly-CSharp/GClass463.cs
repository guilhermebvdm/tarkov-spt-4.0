using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass463 : BotReload
{
	[CompilerGenerated]
	public class Class196
	{
		public GClass463 gclass463_0;

		public MagazineItemClass currentMagazine;

		public Weapon weapon;

		public bool method_0(AmmoItemClass ammo)
		{
			if (ammo.StackObjectsCount > 0 && gclass463_0.Player.InventoryController.Examined(ammo) && ammo.CheckAction(null).Succeeded)
			{
				return currentMagazine.CheckCompatibility(ammo);
			}
			return false;
		}

		public bool method_1(AmmoItemClass ammo)
		{
			if (ammo.StackObjectsCount > 0 && GClass3124.CanAccept(weapon.Chambers[0], ammo))
			{
				return ammo.CheckAction(null).Succeeded;
			}
			return false;
		}
	}

	[CompilerGenerated]
	public class Class197
	{
		public GClass463 gclass463_0;

		public int stockCount;

		public MongoID ammoToAdd;

		public void method_0(IResult s)
		{
			gclass463_0.NextReloadTime = Time.time + 0.5f;
			gclass463_0.Reloading = false;
			gclass463_0.AddAmmoToPockets(ammoToAdd, stockCount);
		}

		public void method_1(IResult s)
		{
			gclass463_0.NextReloadTime = Time.time + 0.5f;
			gclass463_0.Reloading = false;
			gclass463_0.AddAmmoToPockets(ammoToAdd, stockCount);
		}

		public void method_2(IResult s)
		{
			gclass463_0.NextReloadTime = Time.time + 0.5f;
			gclass463_0.Reloading = false;
			gclass463_0.AddAmmoToPockets(ammoToAdd, stockCount);
		}
	}

	public GClass463(BotOwner owner, Weapon weapon)
		: base(owner, weapon)
	{
	}

	public override bool CanReload(bool withCheckByPeriod, out MagazineItemClass foundMag, out List<AmmoItemClass> ammoForInternalReload)
	{
		foundMag = null;
		ammoForInternalReload = null;
		if (!CanReloadPreCheck(withCheckByPeriod, out var currentMagazine))
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
				BotOwner_0.WeaponManager.Selector.TryChangeWeapon();
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
				BotOwner_0.WeaponManager.Selector.TryChangeWeapon();
				NextReloadTime = Time.time + 0.5f;
				return false;
			}
		}
		BotReload.PreallocatedAmmoList.Clear();
		if (weapon is RevolverItemClass)
		{
			if (!BotOwner_0.WeaponManager.InIdleState())
			{
				NextReloadTime = Time.time + 0.5f;
				return false;
			}
			Player.InventoryController.GetReachableItemsOfTypeNonAlloc(BotReload.PreallocatedAmmoList, (AmmoItemClass ammo) => ammo.StackObjectsCount > 0 && Player.InventoryController.Examined(ammo) && ammo.CheckAction(null).Succeeded && currentMagazine.CheckCompatibility(ammo));
		}
		else
		{
			Player.InventoryController.GetAcceptableItemsNonAlloc(BotReload.AvailableEquipmentSlots, BotReload.PreallocatedAmmoList, (AmmoItemClass ammo) => ammo.StackObjectsCount > 0 && GClass3124.CanAccept(weapon.Chambers[0], ammo) && ammo.CheckAction(null).Succeeded);
		}
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

	public override void ReloadAmmo(List<AmmoItemClass> foundAmmo)
	{
		int stockCount = foundAmmo.Count;
		int num = ((base.ShootController.Item.ChamberAmmoCount == 0) ? 1 : 0);
		MagazineItemClass currentMagazine = base.ShootController.Item.GetCurrentMagazine();
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
			base.ShootController.ReloadBarrels(new AmmoPackReloadingClass(list), null, delegate
			{
				NextReloadTime = Time.time + 0.5f;
				base.Reloading = false;
				AddAmmoToPockets(ammoToAdd, stockCount);
			});
		}
		else if (Weapon is RevolverItemClass)
		{
			base.ShootController.ReloadCylinderMagazine(new AmmoPackReloadingClass(list), delegate
			{
				NextReloadTime = Time.time + 0.5f;
				base.Reloading = false;
				AddAmmoToPockets(ammoToAdd, stockCount);
			}, quickReload: false);
		}
		else
		{
			base.ShootController.ReloadWithAmmo(new AmmoPackReloadingClass(list), delegate
			{
				NextReloadTime = Time.time + 0.5f;
				base.Reloading = false;
				AddAmmoToPockets(ammoToAdd, stockCount);
			});
		}
	}
}

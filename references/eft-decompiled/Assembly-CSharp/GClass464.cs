using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass464(BotOwner owner, Weapon weapon) : BotReload(owner, weapon)
{
	[CompilerGenerated]
	public class Class198
	{
		public string templateId;

		public Func<Item, bool> func_0;

		public bool method_0(Item x)
		{
			return x.TemplateId == (MongoID)templateId;
		}
	}

	[CompilerGenerated]
	public class Class199
	{
		public GClass464 gclass464_0;

		public MagazineItemClass currentMagazine;

		public bool method_0(AmmoItemClass ammo)
		{
			if (ammo.StackObjectsCount > 0 && gclass464_0.Player.InventoryController.Examined(ammo) && ammo.CheckAction(null).Succeeded)
			{
				return currentMagazine.CheckCompatibility(ammo);
			}
			return false;
		}
	}

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Dictionary<ItemAddress, Item> Dictionary_0 = new Dictionary<ItemAddress, Item>();

	[NonSerialized]
	public Dictionary<ItemAddress, Item> Dictionary_1 = new Dictionary<ItemAddress, Item>();

	[NonSerialized]
	public const float Float_0 = 1.3f;

	[NonSerialized]
	public float Float_1;

	public void method_4(MagazineItemClass foundMag, List<AmmoItemClass> ammoList)
	{
		if (Bool_0 || ammoList == null || ammoList.Count <= 0)
		{
			return;
		}
		AmmoItemClass ammoItemClass = ammoList[0];
		Int_0 = ammoItemClass.StackMaxSize;
		if (Int_0 == 1)
		{
			CompoundItem container = (CompoundItem)BotOwner_0.GetPlayer.InventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.SecuredContainer).ContainedItem;
			foreach (AmmoItemClass ammo in ammoList)
			{
				if (!GClass3380.Contains(container, ammo))
				{
					Dictionary_0.Add(ammo.CurrentAddress, ammo);
				}
			}
		}
		Bool_0 = true;
	}

	public override bool CanReload(bool withCheckByPeriod, out MagazineItemClass foundMag, out List<AmmoItemClass> ammoForInternalReload)
	{
		foundMag = null;
		ammoForInternalReload = null;
		if (!CanReloadPreCheck(withCheckByPeriod, out var currentMagazine))
		{
			return false;
		}
		Weapon item = base.ShootController.Item;
		if (item.ReloadMode == Weapon.EReloadMode.ExternalMagazineWithInternalReloadSupport || (item.ReloadMode == Weapon.EReloadMode.InternalMagazine && currentMagazine == null))
		{
			MagazineItemClass magazineForReload = GetMagazineForReload(item);
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
		if (!BotOwner_0.WeaponManager.InIdleState())
		{
			NextReloadTime = Time.time + 0.5f;
			return false;
		}
		Player.InventoryController.GetReachableItemsOfTypeNonAlloc(BotReload.PreallocatedAmmoList, (AmmoItemClass ammo) => ammo.StackObjectsCount > 0 && Player.InventoryController.Examined(ammo) && ammo.CheckAction(null).Succeeded && currentMagazine.CheckCompatibility(ammo));
		if (BotReload.PreallocatedAmmoList.Count > 0)
		{
			ammoForInternalReload = BotReload.PreallocatedAmmoList.ToList();
			method_4(foundMag, ammoForInternalReload);
			NextReloadTime = Time.time + 0.5f;
			return base.ShootController.CanStartReload();
		}
		BotOwner_0.WeaponManager.Selector.TryChangeWeaponCauseNoAmmo();
		NextReloadTime = Time.time + 0.5f;
		return false;
	}

	public override void AddAmmoToPockets(string templateId, int targetCount)
	{
		if (Int_0 == 1)
		{
			method_5(templateId, targetCount);
		}
		else
		{
			base.AddAmmoToPockets(templateId, targetCount);
		}
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
		if (Float_1 + 1.3f < Time.time && (BotOwner_0.Memory.GoalEnemy == null || !BotOwner_0.Memory.GoalEnemy.HaveSeen))
		{
			Float_1 = Time.time;
			if (Weapon.ChamberAmmoCount < Weapon.GetMaxMagazineCount())
			{
				Reload();
			}
		}
	}

	public void method_5(string templateId, int targetCount)
	{
		if (!Player.HealthController.IsAlive)
		{
			return;
		}
		InventoryController inventoryController = Player.InventoryController;
		Item containedItem = inventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.SecuredContainer).ContainedItem;
		if (containedItem == null)
		{
			return;
		}
		Dictionary_1.Clear();
		ItemAddress key;
		Item value;
		foreach (KeyValuePair<ItemAddress, Item> item2 in Dictionary_0)
		{
			item2.Deconstruct(out key, out value);
			ItemAddress itemAddress = key;
			if (!object.Equals(value.CurrentAddress, itemAddress))
			{
				Item item = GClass3380.GetAllItems(containedItem).FirstOrDefault((Item x) => x.TemplateId == (MongoID)templateId);
				GStruct154<GClass3411> gStruct = InteractionsHandlerClass.Move(item, itemAddress, inventoryController, simulate: true);
				Dictionary_1[itemAddress] = item;
				inventoryController.TryRunNetworkTransaction(gStruct);
			}
		}
		foreach (KeyValuePair<ItemAddress, Item> item3 in Dictionary_1)
		{
			item3.Deconstruct(out key, out value);
			ItemAddress key2 = key;
			Item value2 = value;
			Dictionary_0[key2] = value2;
		}
	}
}

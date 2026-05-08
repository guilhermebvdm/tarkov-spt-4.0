using System;
using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class BotRandomPlanItemDropper(BotOwner owner) : GClass429(owner)
{
	[NonSerialized]
	public float ActivateTime;

	[NonSerialized]
	public float NextTimePossible;

	[NonSerialized]
	public const float NO_DROP_SEC_FROM_START = 60f;

	[NonSerialized]
	public const float CHANCE_TO_DROP = 160f;

	[NonSerialized]
	public List<Item> CacheItemsList = new List<Item>();

	public static BotRandomPlanItemDropper Create(BotOwner owner)
	{
		if (owner.Profile.Info.Settings.Role == WildSpawnType.bossSanitar)
		{
			return new GClass527(owner);
		}
		return new BotRandomPlanItemDropper(owner);
	}

	public void Activate()
	{
		if (BotOwner_0.Settings.FileSettings.Patrol.DO_RANDOM_DROP_ITEM)
		{
			BotOwner_0.PeacefulActions.OnStartPeacefulMove += method_0;
		}
	}

	public void Dispose()
	{
		if (BotOwner_0.PeacefulActions != null)
		{
			BotOwner_0.PeacefulActions.OnStartPeacefulMove -= method_0;
		}
	}

	public void method_0(GClass413 obj)
	{
		if (Time.time - ActivateTime < 60f || NextTimePossible > Time.time)
		{
			return;
		}
		NextTimePossible = Time.time + 10f;
		if (GClass856.IsTrue100(160f))
		{
			Item item = FindItemToDropToBot();
			if (item != null)
			{
				BotOwner_0.ItemDropper.SetItemToDropNext(item);
				BotOwner_0.ItemDropper.TryDoDrop();
			}
		}
	}

	public virtual Item FindItemToDropToBot()
	{
		Player getPlayer = BotOwner_0.GetPlayer;
		EquipmentSlot[] secureSlots = BotMedecine.secureSlots;
		CacheItemsList.Clear();
		getPlayer.InventoryController.GetAcceptableItemsNonAlloc(secureSlots, CacheItemsList);
		GClass856.Shuffle(CacheItemsList);
		foreach (Item cacheItems in CacheItemsList)
		{
			if (cacheItems is ArmoredEquipmentItemClass)
			{
				IEnumerable<Item> allItems = GClass3380.GetAllItems(cacheItems);
				bool flag = false;
				foreach (Item item in allItems)
				{
					if (item.TryGetItemComponent<NightVisionComponent>(out var _))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			if (!(cacheItems is NightVisionItemClass) && !(cacheItems is AmmoItemClass))
			{
				return cacheItems;
			}
		}
		return null;
	}
}

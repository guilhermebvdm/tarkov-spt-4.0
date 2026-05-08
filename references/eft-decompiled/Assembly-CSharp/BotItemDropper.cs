using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class BotItemDropper : GClass429
{
	[NonSerialized]
	public const float POSSIBLE_DROP_ITEM_PERIOD_SEC = 3f;

	[NonSerialized]
	public List<Item> ItemsListCache = new List<Item>();

	[NonSerialized]
	public Item ItemToDropNext;

	[NonSerialized]
	public float NextTimeDrop;

	[NonSerialized]
	public HashSet<string> PossibleItemsTIdsToDrop = new HashSet<string>();

	[NonSerialized]
	public List<Item> ItemsToDropCache = new List<Item>();

	public event Action<Item> OnItemDrop;

	public BotItemDropper(BotOwner owner)
		: base(owner)
	{
	}

	public void Activate()
	{
		string iTEMS_TO_DROP = BotOwner_0.Settings.FileSettings.Patrol.ITEMS_TO_DROP;
		if (iTEMS_TO_DROP != null)
		{
			string[] array = iTEMS_TO_DROP.Split(',');
			foreach (string item in array)
			{
				PossibleItemsTIdsToDrop.Add(item);
			}
		}
	}

	public void RefreshItemToDrop()
	{
		if (!BotOwner_0.Settings.FileSettings.Mind.CAN_DROP_ITEMS)
		{
			ItemToDropNext = null;
			return;
		}
		Player getPlayer = BotOwner_0.GetPlayer;
		EquipmentSlot[] secureSlots = BotMedecine.secureSlots;
		ItemsListCache.Clear();
		getPlayer.InventoryController.GetAcceptableItemsNonAlloc(secureSlots, ItemsListCache);
		if (ItemsListCache.Count > 0)
		{
			ItemsToDropCache.Clear();
			foreach (Item item in ItemsListCache)
			{
				if (PossibleItemsTIdsToDrop.Contains(item.TemplateId))
				{
					ItemsToDropCache.Add(item);
				}
			}
			ItemToDropNext = GClass856.RandomElement(ItemsToDropCache);
		}
		else
		{
			ItemToDropNext = null;
		}
	}

	public void SetItemToDropNext(Item item)
	{
		ItemToDropNext = item;
	}

	public void SetItemToDropNext(string itemId)
	{
		ItemsListCache.Clear();
		Item item = BotOwner_0.GetPlayer.InventoryController.FindItem<Item>(itemId);
		Debug.LogError($"SetItemToDropNext:{item}  itemId:{itemId}");
		ItemToDropNext = item;
	}

	public void TryDoDrop()
	{
		if (!(NextTimeDrop > Time.time) && ItemToDropNext != null)
		{
			Item itemToDropNext = ItemToDropNext;
			ItemToDropNext = null;
			this.OnItemDrop?.Invoke(itemToDropNext);
			NextTimeDrop = Time.time + 3f;
			BotOwner_0.GetPlayer.InventoryController.ThrowItem(itemToDropNext, downDirection: false, delegate
			{
				BotOwner_0.Medecine.RefreshCurMeds();
			});
		}
	}

	[CompilerGenerated]
	public void method_0(IResult result)
	{
		BotOwner_0.Medecine.RefreshCurMeds();
	}
}

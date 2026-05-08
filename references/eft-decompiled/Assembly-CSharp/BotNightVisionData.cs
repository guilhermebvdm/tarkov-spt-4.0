using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class BotNightVisionData : GClass429
{
	[CompilerGenerated]
	public class Class243
	{
		public TaskCompletionSource<object> completion;

		public void method_0(IResult arg)
		{
			completion.SetResult(true);
		}
	}

	[NonSerialized]
	public const float CHECK_PERIOD_SEC = 60f;

	public NightVisionComponent NightVisionItem;

	public Item TradableItem;

	[NonSerialized]
	public bool NightVisionAtPocket;

	[NonSerialized]
	public float NextTimeCheck;

	[NonSerialized]
	public Slot SlotHeadwear;

	[NonSerialized]
	public bool StopTryingMove;

	[field: NonSerialized]
	public bool HaveNightVision { get; set; }

	[field: NonSerialized]
	public bool UsingNow { get; set; }

	public BotNightVisionData(BotOwner owner)
		: base(owner)
	{
	}

	public async void Activate()
	{
		SlotHeadwear = BotOwner_0.GetPlayer.InventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Headwear);
		if (!(SlotHeadwear.ContainedItem is CompoundItem compoundItem))
		{
			return;
		}
		IEnumerable<NightVisionComponent> itemComponentsInChildren = GClass3380.GetItemComponentsInChildren<NightVisionComponent>(compoundItem);
		if (itemComponentsInChildren.Any())
		{
			await method_2(compoundItem);
			HaveNightVision = true;
			NextTimeCheck = Time.time + 10f;
			ArmorComponent itemComponent = compoundItem.GetItemComponent<ArmorComponent>();
			HelmetComponent itemComponent2 = compoundItem.GetItemComponent<HelmetComponent>();
			NightVisionComponent nightVisionComponent = (NightVisionItem = itemComponentsInChildren.First());
			if (itemComponent == null)
			{
				TradableItem = itemComponent2.Item;
			}
			else if (itemComponent.ArmorClass > 0)
			{
				TradableItem = nightVisionComponent.Item;
			}
			else
			{
				TradableItem = compoundItem;
			}
			method_0();
		}
	}

	public float UpdateVision(float distVision)
	{
		if (HaveNightVision && NightVisionItem.Togglable.On && !NightVisionAtPocket)
		{
			return BotOwner_0.Settings.FileSettings.Look.NIGHT_VISION_DIST;
		}
		return distVision;
	}

	public void ManualUpdate()
	{
		if (HaveNightVision && NextTimeCheck < Time.time)
		{
			float num = 60f;
			NextTimeCheck = Time.time + num;
			method_0();
		}
	}

	public void method_0()
	{
		if (BotOwner_0.FlashGrenade.IsFlashed)
		{
			return;
		}
		if (NightVisionAtPocket)
		{
			if (BotOwner_0.LookSensor.ClearVisibleDist < BotOwner_0.Settings.FileSettings.Look.NIGHT_VISION_ON)
			{
				method_4();
			}
			return;
		}
		if (BotOwner_0.LookSensor.ClearVisibleDist < BotOwner_0.Settings.FileSettings.Look.NIGHT_VISION_ON)
		{
			method_5();
		}
		if (BotOwner_0.LookSensor.ClearVisibleDist > BotOwner_0.Settings.FileSettings.Look.NIGHT_VISION_OFF)
		{
			method_1();
		}
	}

	public void method_1()
	{
		if (!StopTryingMove)
		{
			if (!method_7(TradableItem, EquipmentSlot.SecuredContainer))
			{
				StopTryingMove = true;
			}
			else
			{
				UsingNow = false;
			}
		}
	}

	public async Task method_2(Item item)
	{
		if (item == null || !(BotOwner_0.GetPlayer.InventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.SecuredContainer).ContainedItem is SearchableItemItemClass searchableItemItemClass))
		{
			return;
		}
		StashGridClass stashGridClass = searchableItemItemClass.Grids.FirstOrDefault();
		if (stashGridClass == null || stashGridClass.FindFreeSpace(item) != null)
		{
			return;
		}
		XYCellSizeStruct xYCellSizeStruct = item.CalculateCellSize();
		IEnumerable<KeyValuePair<Item, LocationInGrid>> itemsInRect = stashGridClass.GetItemsInRect(new GStruct423(0, 0, xYCellSizeStruct.X, xYCellSizeStruct.Y));
		if (itemsInRect != null)
		{
			KeyValuePair<Item, LocationInGrid>[] array = itemsInRect.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<Item, LocationInGrid> keyValuePair = array[i];
				var (item3, _) = (KeyValuePair<Item, LocationInGrid>)(ref keyValuePair);
				await method_3(item3);
			}
		}
	}

	public Task method_3(Item item)
	{
		TaskCompletionSource<object> completion = new TaskCompletionSource<object>();
		BotOwner_0.GetPlayer.InventoryController.ThrowItem(item, downDirection: false, delegate
		{
			completion.SetResult(true);
		});
		return completion.Task;
	}

	public void method_4()
	{
		method_6();
		method_5();
	}

	public void method_5()
	{
		if (!NightVisionItem.Togglable.On)
		{
			BotOwner_0.GetPlayer.InventoryController.TryRunNetworkTransaction(NightVisionItem.Togglable.Set(value: true, simulate: true));
		}
		UsingNow = true;
	}

	public void method_6()
	{
		TraderControllerClass inventoryController = BotOwner_0.GetPlayer.InventoryController;
		if (inventoryController == null)
		{
			return;
		}
		if (SlotHeadwear == null)
		{
			SlotHeadwear = BotOwner_0.GetPlayer.InventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Headwear);
		}
		Error error;
		ItemAddress itemAddress = SlotHeadwear.FindLocationForItem(TradableItem, out error);
		if (TradableItem == null)
		{
			return;
		}
		if (itemAddress == null)
		{
			if (SlotHeadwear.ContainedItem is CompoundItem compoundItem)
			{
				GStruct153 operationResult = compoundItem.Apply(inventoryController, TradableItem, 1, simulate: true);
				if (!operationResult.Failed)
				{
					inventoryController.TryRunNetworkTransaction(operationResult);
					NightVisionAtPocket = false;
				}
			}
		}
		else
		{
			GStruct154<GClass3411> gStruct = InteractionsHandlerClass.Move(TradableItem, itemAddress, inventoryController, simulate: true);
			inventoryController.TryRunNetworkTransaction(gStruct);
			NightVisionAtPocket = false;
		}
	}

	public bool method_7(Item item, EquipmentSlot slot)
	{
		TraderControllerClass inventoryController = BotOwner_0.GetPlayer.InventoryController;
		if (!(BotOwner_0.GetPlayer.InventoryController.Inventory.Equipment.GetSlot(slot).ContainedItem is SearchableItemItemClass searchableItemItemClass))
		{
			return false;
		}
		StashGridClass stashGridClass = searchableItemItemClass.Grids.FirstOrDefault();
		if (stashGridClass == null)
		{
			return false;
		}
		GClass3393 gClass = stashGridClass.FindLocationForItem(item);
		if (gClass != null)
		{
			GStruct154<GClass3411> gStruct = InteractionsHandlerClass.Move(item, gClass, inventoryController, simulate: true);
			inventoryController.TryRunNetworkTransaction(gStruct);
			NightVisionAtPocket = true;
			return true;
		}
		return false;
	}
}

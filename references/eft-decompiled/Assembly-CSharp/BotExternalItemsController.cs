using System;
using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;

public class BotExternalItemsController(BotOwner owner) : GClass429(owner)
{
	[NonSerialized]
	public HashSet<string> PickUpedItems = new HashSet<string>();

	public void Activate()
	{
		BotOwner_0.ItemTaker.OnItemTaken += method_1;
		BotOwner_0.ItemDropper.OnItemDrop += method_0;
	}

	public bool HaveItemsToDrop()
	{
		return PickUpedItems.Count > 0;
	}

	public string GetRandomItemToDrop()
	{
		return GClass856.RandomElement(PickUpedItems);
	}

	public void method_0(Item obj)
	{
		PickUpedItems.Remove(obj.Id);
	}

	public void method_1(Item obj, IPlayer itemLastOwner)
	{
		PickUpedItems.Add(obj.Id);
	}

	public void Dispose()
	{
		if (BotOwner_0.ItemTaker != null)
		{
			BotOwner_0.ItemTaker.OnItemTaken -= method_1;
		}
		if (BotOwner_0.ItemDropper != null)
		{
			BotOwner_0.ItemDropper.OnItemDrop -= method_0;
		}
	}
}

using System;
using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class PlayerSearchControllerClass : GClass2235
{
	[NonSerialized]
	public Dictionary<Item, ItemAddress> Dictionary_0 = new Dictionary<Item, ItemAddress>();

	[NonSerialized]
	public HashSet<Item> HashSet_1 = new HashSet<Item>();

	[NonSerialized]
	public HashSet<Item> HashSet_2 = new HashSet<Item>();

	public PlayerSearchControllerClass(Profile profile, Player.PlayerInventoryController inventoryController)
		: base(profile, inventoryController)
	{
		method_3(profile.Inventory.Equipment);
	}

	public void method_3(Item item)
	{
		if (!(item is GClass3248 gClass))
		{
			return;
		}
		if (item is SearchableItemItemClass item2)
		{
			UncoverItem(item2);
		}
		foreach (EFT.InventoryLogic.IContainer container in gClass.Containers)
		{
			bool flag = container is GInterface215;
			foreach (Item item3 in container.Items)
			{
				if (flag)
				{
					method_4(item3);
				}
				method_3(item3);
			}
		}
	}

	public override bool IsItemKnown(Item item, ItemAddress itemAddress = null)
	{
		itemAddress = itemAddress ?? item.Parent;
		if (!(itemAddress.Container is GInterface215))
		{
			return true;
		}
		if (HashSet_1.Contains(item))
		{
			return true;
		}
		foreach (var (item3, itemAddress3) in Dictionary_0)
		{
			if (item3 == item && itemAddress3.Equals(itemAddress))
			{
				return true;
			}
		}
		return false;
	}

	public void SetItemAsTemporaryKnown(Item item)
	{
		HashSet_1.Add(item);
	}

	public void RemoveItemFromTemporaryKnown(Item item)
	{
		HashSet_1.Remove(item);
	}

	public override void SetItemAsKnown(Item item, bool raiseEvents)
	{
		if (Dictionary_0.TryGetValue(item, out var value) && value.Equals(item.Parent))
		{
			Debug.LogError($"Item is already found: {item}");
		}
		RemoveItemFromTemporaryKnown(item);
		if (method_4(item) && raiseEvents)
		{
			method_1(item);
		}
	}

	public void ForgetItem(Item item)
	{
		Dictionary_0.Remove(item);
	}

	public bool method_4(Item item)
	{
		Dictionary_0[item] = item.Parent;
		return HashSet_2.Add(item);
	}
}

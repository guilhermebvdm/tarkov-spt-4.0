using System;
using System.Collections.Generic;
using EFT.InventoryLogic;

namespace EFT;

public class RecodableItemsHandler
{
	[NonSerialized]
	public Player Player;

	[NonSerialized]
	public List<GInterface412> RecodableItems;

	public RecodableItemsHandler(Player player)
	{
		RecodableItems = new List<GInterface412>();
		Player = player;
		Player.InventoryController.AddItemEvent += method_1;
		Player.InventoryController.RemoveItemEvent += method_2;
		method_0();
	}

	public void method_0()
	{
		foreach (Item playerItem in Player.Profile.Inventory.GetPlayerItems(EPlayerItems.Equipment))
		{
			if (playerItem is GInterface412)
			{
				((GInterface412)playerItem)?.InitializeHandler(Player);
				RecodableItems.Add((GInterface412)playerItem);
			}
		}
	}

	public void method_1(GEventArgs2 args)
	{
		if (args.Item is GInterface412)
		{
			GInterface412 gInterface = (GInterface412)args.Item;
			gInterface?.OnAddRecodableItem(Player);
			RecodableItems.Add(gInterface);
		}
	}

	public void method_2(GEventArgs3 args)
	{
		if (args.Item is GInterface412)
		{
			GInterface412 gInterface = (GInterface412)args.Item;
			gInterface?.OnRemoveRecodableItem();
			RecodableItems.Remove(gInterface);
		}
	}

	public T GetRecodableComponent<T>() where T : RecodableComponent
	{
		foreach (GInterface412 recodableItem in RecodableItems)
		{
			if (recodableItem is GInterface411<T>)
			{
				return ((GInterface411<T>)recodableItem).GetRecodableComponent();
			}
		}
		return null;
	}

	public bool TryToGetRecodableComponent<T>(out T component) where T : RecodableComponent
	{
		component = null;
		foreach (GInterface412 recodableItem in RecodableItems)
		{
			if (recodableItem is GInterface411<T>)
			{
				component = ((GInterface411<T>)recodableItem).GetRecodableComponent();
				return true;
			}
		}
		return false;
	}

	public List<T> GetRecodableComponents<T>() where T : RecodableComponent
	{
		List<T> list = new List<T>();
		foreach (GInterface412 recodableItem in RecodableItems)
		{
			if (recodableItem is GInterface411<T>)
			{
				list.Add(((GInterface411<T>)recodableItem).GetRecodableComponent());
			}
		}
		return list;
	}
}

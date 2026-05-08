using System;
using EFT;
using EFT.InventoryLogic;
using EFT.InventoryLogic.Operations;

public class GClass2237 : GClass2235
{
	[NonSerialized]
	public ClientPlayer.Class2443 Class2443_0;

	public GClass2237(Profile profile, ClientPlayer.Class2443 inventoryController)
		: base(profile, inventoryController)
	{
		Class2443_0 = inventoryController;
	}

	public override bool IsItemKnown(Item item, ItemAddress itemAddress = null)
	{
		return !(item is GClass3367);
	}

	public override bool IsSearched(SearchableItemItemClass item)
	{
		return item.IsSearched;
	}

	public override void SetItemAsSearched<TItem>(TItem item)
	{
		((GInterface171)item).SetItemInfo((GClass1802)null);
		base.SetItemAsSearched(item);
	}

	public override void SetItemAsKnown(Item item, bool raiseEvents)
	{
		throw new NotImplementedException();
	}

	public override void StopSearchOperation(SearchContentOperation operation)
	{
		Class2443_0.method_42(operation);
	}

	public override EObserverItemState GetObserverItemState(Item item, ItemAddress address)
	{
		if (!(item is GClass3367))
		{
			return EObserverItemState.Known;
		}
		return EObserverItemState.Unknown;
	}
}

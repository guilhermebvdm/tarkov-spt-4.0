using EFT;
using EFT.InventoryLogic;

public class GClass2240 : GClass2239
{
	public static readonly GClass2240 Instance = new GClass2240();

	public GClass2240()
		: base(searchResult: true)
	{
	}

	public override bool IsItemKnown(Item item, ItemAddress itemAddress = null)
	{
		return true;
	}

	public override bool TryFindChangedContainer(ItemAddress address, out GClass1802 changedContainer)
	{
		changedContainer = null;
		return false;
	}

	public override EObserverItemState GetObserverItemState(Item item, ItemAddress address)
	{
		return EObserverItemState.Known;
	}
}

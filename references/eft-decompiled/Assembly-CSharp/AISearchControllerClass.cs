using EFT;
using EFT.InventoryLogic;
using EFT.InventoryLogic.Operations;

public class AISearchControllerClass : GClass2235
{
	public override bool CanSearch => false;

	public AISearchControllerClass()
		: base(null, null)
	{
	}

	public override bool CanStartNewSearchOperation()
	{
		return false;
	}

	public override bool IsSearched(SearchableItemItemClass item)
	{
		return true;
	}

	public override bool IsItemKnown(Item item, ItemAddress itemAddress = null)
	{
		return true;
	}

	public override bool ContainsUnknownItems(SearchableItemItemClass item)
	{
		return false;
	}

	public override EObserverItemState GetObserverItemState(Item item, ItemAddress address)
	{
		return EObserverItemState.Known;
	}

	public override bool TryFindChangedContainer(ItemAddress address, out GClass1802 changedContainer)
	{
		changedContainer = null;
		return false;
	}

	public override void SearchContents(SearchableItemItemClass searchableItem)
	{
	}

	public override void StopSearching(MongoID itemId)
	{
	}

	public override void SetItemAsSearched<TItem>(TItem item)
	{
	}

	public override void SetItemAsKnown(Item item, bool raiseEvents)
	{
	}

	public override void StopSearchOperation(SearchContentOperation operation)
	{
	}
}

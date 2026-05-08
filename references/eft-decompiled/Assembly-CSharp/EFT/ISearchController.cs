using EFT.InventoryLogic;

namespace EFT;

public interface ISearchController
{
	bool IsSearched(SearchableItemItemClass item);

	bool ContainsUnknownItems(SearchableItemItemClass item);

	bool IsItemKnown(Item item, ItemAddress itemAddress = null);

	bool TryFindChangedContainer(ItemAddress address, out GClass1802 changedContainer);

	EObserverItemState GetObserverItemState(Item item, ItemAddress address);
}

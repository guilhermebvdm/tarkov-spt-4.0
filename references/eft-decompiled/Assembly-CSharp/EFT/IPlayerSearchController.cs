using System;
using EFT.InventoryLogic;
using EFT.InventoryLogic.Operations;

namespace EFT;

public interface IPlayerSearchController : ISearchController
{
	MongoID ProfileId { get; }

	bool CanSearch { get; }

	GInterface155<SearchContentOperation> SearchOperations { get; }

	event Action<Item> OnItemFound;

	event Action<SearchableItemItemClass> OnItemSearched;

	event Action OnItemFullySearchedEvent;

	void SetItemAsSearched<TItem>(TItem item) where TItem : SearchableItemItemClass, GInterface171;

	void SetItemAsKnown(Item item, bool raiseEvents);

	bool CanStartNewSearchOperation();

	void StopSearching(MongoID itemId);

	void SearchContents(SearchableItemItemClass searchableItem);

	void OnItemFullySearched();
}

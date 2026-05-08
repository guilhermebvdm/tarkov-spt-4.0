using System;
using EFT;

public abstract class GClass2239 : GClass2234
{
	[NonSerialized]
	public bool Bool_0;

	public override bool CanSearch => false;

	public GClass2239(bool searchResult)
	{
		Bool_0 = searchResult;
	}

	public override bool IsSearched(SearchableItemItemClass item)
	{
		return Bool_0;
	}

	public override bool ContainsUnknownItems(SearchableItemItemClass item)
	{
		return !Bool_0;
	}

	public override bool CanStartNewSearchOperation()
	{
		return false;
	}

	public override void StopSearching(MongoID itemId)
	{
	}

	public override void SearchContents(SearchableItemItemClass searchableItem)
	{
		throw new NotImplementedException();
	}
}

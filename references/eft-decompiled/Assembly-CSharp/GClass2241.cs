using EFT.InventoryLogic;

public class GClass2241 : GClass2239
{
	public static readonly GClass2241 Instance = new GClass2241();

	public GClass2241()
		: base(searchResult: true)
	{
	}

	public override bool IsItemKnown(Item item, ItemAddress itemAddress = null)
	{
		return true;
	}

	public override bool IsSearched(SearchableItemItemClass item)
	{
		return !item.HideEntrails;
	}
}

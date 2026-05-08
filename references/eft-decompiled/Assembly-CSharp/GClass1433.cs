using EFT.InventoryLogic;

public class GClass1433
{
	public string id;

	public int count;

	public GClass1433(string itemId, int count)
	{
		id = itemId;
		this.count = count;
	}

	public GClass1433(Item item, int count)
	{
		id = item.Id;
		this.count = count;
	}
}

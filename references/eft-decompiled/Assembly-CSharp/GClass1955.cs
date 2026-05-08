using EFT;

[GAttribute29]
public class GClass1955
{
	public MongoID ItemId;

	public int NumberToDestroy;

	public int NumberToPreserve;

	public void Deconstruct(out string itemId, out int numberToDestroy, out int numberToPreserve)
	{
		itemId = ItemId;
		numberToDestroy = NumberToDestroy;
		numberToPreserve = NumberToPreserve;
	}
}

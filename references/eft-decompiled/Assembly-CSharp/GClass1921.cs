[GAttribute29]
public class GClass1921
{
	public GClass1950 Address;

	public InventoryDescriptorClass Item;

	public void Deconstruct(out InventoryDescriptorClass item, out GClass1950 address)
	{
		item = Item;
		address = Address;
	}
}

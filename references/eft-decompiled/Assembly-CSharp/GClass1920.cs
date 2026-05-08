using System.Collections.Generic;

[GAttribute29]
public class GClass1920
{
	public byte SlotNumber;

	public List<InventoryDescriptorClass> ContainedItems;

	public GClass1920()
	{
	}

	public GClass1920(byte slotNumber, List<InventoryDescriptorClass> containedItems)
	{
		SlotNumber = slotNumber;
		ContainedItems = containedItems;
	}
}

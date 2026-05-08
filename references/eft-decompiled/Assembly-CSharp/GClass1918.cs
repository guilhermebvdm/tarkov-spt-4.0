using System;

[GAttribute29]
public class GClass1918
{
	public InventoryDescriptorClass Item;

	public byte X;

	public byte Y;

	public bool Horizontal;

	[NonSerialized]
	public LocationInGrid LocationInGrid_0;

	[GAttribute31]
	public LocationInGrid Location => LocationInGrid_0 ?? (LocationInGrid_0 = new LocationInGrid(X, Y, (!Horizontal) ? ItemRotation.Vertical : ItemRotation.Horizontal));

	public GClass1918()
	{
	}

	public GClass1918(InventoryDescriptorClass item, LocationInGrid location)
	{
		Item = item;
		LocationInGrid_0 = location;
		X = (byte)location.x;
		Y = (byte)location.y;
		Horizontal = location.r == ItemRotation.Horizontal;
	}
}

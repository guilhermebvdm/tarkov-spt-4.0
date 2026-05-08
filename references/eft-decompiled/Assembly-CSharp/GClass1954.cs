using System;

public class GClass1954 : GClass1950
{
	public byte X;

	public byte Y;

	public bool Horizontal;

	[NonSerialized]
	public LocationInGrid LocationInGrid_0;

	[GAttribute31]
	public LocationInGrid LocationInGrid => LocationInGrid_0 ?? (LocationInGrid_0 = new LocationInGrid(X, Y, (!Horizontal) ? ItemRotation.Vertical : ItemRotation.Horizontal));

	public GClass1954()
	{
	}

	public GClass1954(GClass1949 container, LocationInGrid location)
	{
		Container = container;
		LocationInGrid_0 = location;
		X = (byte)location.x;
		Y = (byte)location.y;
		Horizontal = location.r == ItemRotation.Horizontal;
	}

	public override string ToString()
	{
		return $"{Container}, Location: (x: {X}, y: {Y}, horizontal: {Horizontal})";
	}
}

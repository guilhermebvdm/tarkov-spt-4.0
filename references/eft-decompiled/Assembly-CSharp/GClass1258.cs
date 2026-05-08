using System;
using UnityEngine;

public abstract class GClass1258
{
	[NonSerialized]
	public static int[] Int_0 = new int[4] { 31, 37, 41, 43 };

	public Bounds cellBounds;

	public Bounds cellInnerBounds;

	public int coordX;

	public int coordY;

	public int coordZ;

	public bool isActive;

	public int CalculateHash()
	{
		return CalculateHash(coordX, coordY, coordZ);
	}

	public static int CalculateHash(int coordX, int coordY, int coordZ)
	{
		return (((Int_0[0] + coordX) * Int_0[1] + coordY) * Int_0[2] + coordZ) * Int_0[3];
	}

	public GClass1258()
	{
	}
}

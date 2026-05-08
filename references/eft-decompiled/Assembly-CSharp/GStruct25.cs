using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct GStruct25
{
	public Vector3 Position;

	public bool SitAtEnd;

	public PointWithLookSides LookSides;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	public readonly bool HasValue
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
	}

	public readonly bool HasLookSides
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
	}

	public GStruct25(Vector3 position, bool sitAtEnd, PointWithLookSides sides)
	{
		Position = position;
		SitAtEnd = sitAtEnd;
		Bool_0 = true;
		LookSides = sides;
		Bool_1 = false;
		Bool_1 = method_0();
	}

	public GStruct25(Vector3 position, bool sitAtEnd)
	{
		Position = position;
		SitAtEnd = sitAtEnd;
		Bool_0 = true;
		LookSides = null;
		Bool_1 = false;
		Bool_1 = method_0();
	}

	public GStruct25(Vector3 position)
	{
		Position = position;
		SitAtEnd = false;
		Bool_0 = true;
		LookSides = null;
		Bool_1 = false;
		Bool_1 = method_0();
	}

	public bool method_0()
	{
		if (LookSides != null)
		{
			return LookSides.Directions.Count > 0;
		}
		return false;
	}
}

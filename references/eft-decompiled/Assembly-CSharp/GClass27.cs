using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass27 : GClass26
{
	[NonSerialized]
	[CompilerGenerated]
	public Vector3? Nullable_0;

	public Vector3? PointToShoot
	{
		[CompilerGenerated]
		get
		{
			return Nullable_0;
		}
		[CompilerGenerated]
		set
		{
			Nullable_0 = value;
		}
	}

	public GClass27(Vector3? p)
	{
		PointToShoot = p;
	}
}

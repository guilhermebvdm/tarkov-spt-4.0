using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct GStruct168 : GInterface217<GStruct168>
{
	public int NetId;

	public Vector3 HitPosition;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct168> Ginterface217_0;

	public GInterface217<GStruct168> Nested
	{
		[CompilerGenerated]
		readonly get
		{
			return Ginterface217_0;
		}
		[CompilerGenerated]
		set
		{
			Ginterface217_0 = value;
		}
	}
}

using System;
using System.Runtime.CompilerServices;

public struct GStruct278 : GInterface217<GStruct278>
{
	public string WeaponId;

	public float LastShotTime;

	public float LastOverheat;

	public bool SlideOnOverheatReached;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct278> Ginterface217_0;

	public GInterface217<GStruct278> Nested
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

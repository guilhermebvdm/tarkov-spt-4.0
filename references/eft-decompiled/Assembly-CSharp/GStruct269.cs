using System;
using System.Runtime.CompilerServices;
using EFT;

public struct GStruct269 : GInterface217<GStruct269>
{
	public NetworkPlayer.ClientShot[] ClientShots;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct269> Ginterface217_0;

	public GInterface217<GStruct269> Nested
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

using System;
using System.Runtime.CompilerServices;

public struct GStruct264 : GInterface217<GStruct264>
{
	public string TraderId;

	public double Standing;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct264> Ginterface217_0;

	public GInterface217<GStruct264> Nested
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

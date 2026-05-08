using System;
using System.Runtime.CompilerServices;

public struct GStruct267 : GInterface217<GStruct267>
{
	public bool Active;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct267> Ginterface217_0;

	public GInterface217<GStruct267> Nested
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

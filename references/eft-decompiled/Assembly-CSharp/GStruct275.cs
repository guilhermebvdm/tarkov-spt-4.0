using System;
using System.Runtime.CompilerServices;

public struct GStruct275 : GInterface217<GStruct275>
{
	public uint LocalizationKey1;

	public uint LocalizationKey2;

	public int Value;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct275> Ginterface217_0;

	public GInterface217<GStruct275> Nested
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

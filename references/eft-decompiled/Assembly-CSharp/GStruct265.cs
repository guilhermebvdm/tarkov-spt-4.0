using System;
using System.Runtime.CompilerServices;

public struct GStruct265 : GInterface217<GStruct265>
{
	public string Message;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct265> Ginterface217_0;

	public GInterface217<GStruct265> Nested
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

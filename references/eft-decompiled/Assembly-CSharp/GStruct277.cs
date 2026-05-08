using System;
using System.Runtime.CompilerServices;

public struct GStruct277 : GInterface217<GStruct277>
{
	public uint CallbackId;

	public string Error;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct277> Ginterface217_0;

	public GInterface217<GStruct277> Nested
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

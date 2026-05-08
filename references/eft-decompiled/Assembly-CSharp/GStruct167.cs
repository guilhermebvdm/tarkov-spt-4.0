using System;
using System.Runtime.CompilerServices;
using EFT.Interactive;

public struct GStruct167 : GInterface217<GStruct167>
{
	public int NetId;

	public Turnable.EState State;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct167> Ginterface217_0;

	public GInterface217<GStruct167> Nested
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

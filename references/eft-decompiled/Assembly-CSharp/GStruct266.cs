using System;
using System.Runtime.CompilerServices;
using EFT;

public struct GStruct266 : GInterface217<GStruct266>
{
	public bool IsEncoded;

	public RadioTransmitterStatus Status;

	public bool IsAggressor;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct266> Ginterface217_0;

	public GInterface217<GStruct266> Nested
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

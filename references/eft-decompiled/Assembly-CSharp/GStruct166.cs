using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT.Interactive;

public struct GStruct166 : GInterface217<GStruct166>
{
	public string PointName;

	public EExfiltrationStatus Command;

	public List<string> QueuedPlayers;

	public bool ItemTransferred;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct166> Ginterface217_0;

	public GInterface217<GStruct166> Nested
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

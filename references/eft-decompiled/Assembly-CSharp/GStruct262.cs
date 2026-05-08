using System;
using System.Runtime.CompilerServices;

public struct GStruct262 : GInterface216<GStruct262>, GInterface217<GStruct262>
{
	public string GroupName;

	public float Value;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct262> Ginterface217_0;

	public GInterface217<GStruct262> Nested
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

	public bool TryUpdate(GStruct262 source)
	{
		if (GroupName != source.GroupName)
		{
			return false;
		}
		Value = source.Value;
		return true;
	}

	bool GInterface216<GStruct262>.TryUpdate(GStruct262 source)
	{
		//ILSpy generated this explicit interface implementation from .override directive in TryUpdate
		return this.TryUpdate(source);
	}
}

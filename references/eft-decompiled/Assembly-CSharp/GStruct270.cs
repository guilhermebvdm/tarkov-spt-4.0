using System;
using System.Runtime.CompilerServices;

public struct GStruct270 : GInterface217<GStruct270>
{
	public GStruct273 ProgressData;

	[NonSerialized]
	[CompilerGenerated]
	public GInterface217<GStruct270> Ginterface217_0;

	public GInterface217<GStruct270> Nested
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

	public override string ToString()
	{
		string text = ((Nested == null) ? "" : "\n");
		return string.Format("{0}, {1}-->{2}: {3}", ProgressData, text, "Nested", Nested);
	}
}

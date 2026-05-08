using System;
using System.Runtime.CompilerServices;

public struct GStruct119 : GInterface119
{
	[NonSerialized]
	[CompilerGenerated]
	public double Double_0;

	[NonSerialized]
	[CompilerGenerated]
	public double Double_1;

	public double remoteTime
	{
		[CompilerGenerated]
		readonly get
		{
			return Double_0;
		}
		[CompilerGenerated]
		set
		{
			Double_0 = value;
		}
	}

	public double localTime
	{
		[CompilerGenerated]
		readonly get
		{
			return Double_1;
		}
		[CompilerGenerated]
		set
		{
			Double_1 = value;
		}
	}

	public GStruct119(double remoteTime, double localTime)
	{
		this.remoteTime = remoteTime;
		this.localTime = localTime;
	}
}

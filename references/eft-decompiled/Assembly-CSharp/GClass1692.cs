using System;
using System.Runtime.CompilerServices;

public class GClass1692 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public long Long_0;

	public long Time
	{
		[CompilerGenerated]
		get
		{
			return Long_0;
		}
		[CompilerGenerated]
		set
		{
			Long_0 = value;
		}
	}

	public GClass1692()
	{
	}

	public GClass1692(long time)
	{
		Time = time;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteLong(writer, Time);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		Time = GClass1285.ReadLong(reader);
	}
}

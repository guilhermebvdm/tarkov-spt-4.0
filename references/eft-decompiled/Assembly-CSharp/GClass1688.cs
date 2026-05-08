using System;
using System.Runtime.CompilerServices;

public class GClass1688 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	public int Seconds
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public GClass1688()
	{
	}

	public GClass1688(int seconds)
	{
		Seconds = seconds;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteInt(writer, Seconds);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		Seconds = GClass1285.ReadInt(reader);
	}
}

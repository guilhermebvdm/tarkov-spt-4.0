using System;
using System.Runtime.CompilerServices;

public class GClass1703 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	public int ShellingFinished
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

	public int TimeToNextShelling
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public GClass1703()
	{
	}

	public GClass1703(int shellingFinished, int timeToNextShelling)
	{
		ShellingFinished = shellingFinished;
		TimeToNextShelling = timeToNextShelling;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteInt(writer, ShellingFinished);
		GClass1290.WriteInt(writer, TimeToNextShelling);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		ShellingFinished = GClass1285.ReadInt(reader);
		TimeToNextShelling = GClass1285.ReadInt(reader);
	}
}

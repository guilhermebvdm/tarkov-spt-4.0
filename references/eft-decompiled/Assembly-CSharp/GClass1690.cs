using System;
using System.Runtime.CompilerServices;

public class GClass1690 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	public float PastTime
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
		[CompilerGenerated]
		set
		{
			Float_0 = value;
		}
	}

	public int SessionSeconds
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

	public GClass1690()
	{
	}

	public GClass1690(float pastTime, int sessionSeconds)
	{
		PastTime = pastTime;
		SessionSeconds = sessionSeconds;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteFloat(writer, PastTime);
		GClass1290.WriteInt(writer, SessionSeconds);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		PastTime = GClass1285.ReadFloat(reader);
		SessionSeconds = GClass1285.ReadInt(reader);
	}
}

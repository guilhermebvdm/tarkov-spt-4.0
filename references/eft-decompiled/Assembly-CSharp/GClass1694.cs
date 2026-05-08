using System;
using System.Runtime.CompilerServices;

public class GClass1694 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

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

	public GClass1694()
	{
	}

	public GClass1694(int sessionSeconds)
	{
		SessionSeconds = sessionSeconds;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteInt(writer, SessionSeconds);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		SessionSeconds = GClass1285.ReadInt(reader);
	}
}

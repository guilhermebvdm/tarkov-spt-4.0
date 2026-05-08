using System;
using System.Runtime.CompilerServices;

public class GClass1681 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	public int PlayerId
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

	public GClass1681()
	{
	}

	public GClass1681(int playerId)
	{
		PlayerId = playerId;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteInt(writer, PlayerId);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		PlayerId = GClass1285.ReadInt(reader);
	}
}

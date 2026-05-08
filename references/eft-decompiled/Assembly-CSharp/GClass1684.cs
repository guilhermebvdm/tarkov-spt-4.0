using System;
using System.Runtime.CompilerServices;

public class GClass1684 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	public string PlayerProfileID
	{
		[CompilerGenerated]
		get
		{
			return String_0;
		}
		[CompilerGenerated]
		set
		{
			String_0 = value;
		}
	}

	public GClass1684()
	{
	}

	public GClass1684(string playerProfileID)
	{
		PlayerProfileID = playerProfileID;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteString(writer, PlayerProfileID);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		PlayerProfileID = GClass1285.ReadString(reader);
	}
}

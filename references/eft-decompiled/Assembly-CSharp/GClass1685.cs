using System;
using System.Runtime.CompilerServices;

public class GClass1685 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public string String_1;

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

	public string TraderId
	{
		[CompilerGenerated]
		get
		{
			return String_1;
		}
		[CompilerGenerated]
		set
		{
			String_1 = value;
		}
	}

	public GClass1685()
	{
	}

	public GClass1685(string playerProfileID, string traderId)
	{
		PlayerProfileID = playerProfileID;
		TraderId = traderId;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteString(writer, PlayerProfileID);
		GClass1290.WriteString(writer, TraderId);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		PlayerProfileID = GClass1285.ReadString(reader);
		TraderId = GClass1285.ReadString(reader);
	}
}

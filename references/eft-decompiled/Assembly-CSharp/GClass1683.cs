using System;
using System.Runtime.CompilerServices;

public class GClass1683 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public string String_1;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

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

	public string StatisticsType
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

	public int ValueToSet
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

	public GClass1683()
	{
	}

	public GClass1683(string playerProfileID, string statisticsType, int valueToSet)
	{
		PlayerProfileID = playerProfileID;
		StatisticsType = statisticsType;
		ValueToSet = valueToSet;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteString(writer, PlayerProfileID);
		GClass1290.WriteString(writer, StatisticsType);
		GClass1290.WriteInt(writer, ValueToSet);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		PlayerProfileID = GClass1285.ReadString(reader);
		StatisticsType = GClass1285.ReadString(reader);
		ValueToSet = GClass1285.ReadInt(reader);
	}
}

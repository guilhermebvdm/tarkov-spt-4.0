using System;
using System.Runtime.CompilerServices;

public class GClass1695 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public string String_1;

	public int DisconnectionCode
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

	public string AdditionalInfo
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

	public string TechnicalMessage
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

	public GClass1695()
	{
	}

	public GClass1695(int disconnectionCode, string additionalInfo, string technicalMessage)
	{
		DisconnectionCode = disconnectionCode;
		AdditionalInfo = additionalInfo;
		TechnicalMessage = technicalMessage;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteInt(writer, DisconnectionCode);
		GClass1290.WriteString(writer, AdditionalInfo);
		GClass1290.WriteString(writer, TechnicalMessage);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		DisconnectionCode = GClass1285.ReadInt(reader);
		AdditionalInfo = GClass1285.ReadString(reader);
		TechnicalMessage = GClass1285.ReadString(reader);
	}
}

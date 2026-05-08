using System;
using System.Runtime.CompilerServices;
using EFT;

public class GClass1699 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public GStruct180 Gstruct180_0;

	public GStruct180 Data
	{
		[CompilerGenerated]
		get
		{
			return Gstruct180_0;
		}
		[CompilerGenerated]
		set
		{
			Gstruct180_0 = value;
		}
	}

	public GClass1699()
	{
	}

	public GClass1699(GStruct180 data)
	{
		Data = data;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteString(writer, Data.PlayerProfileIDForSend);
		GClass1290.WriteInt(writer, Data.PlayerID);
		GClass1290.WriteBool(writer, Data.IsEncoded);
		writer.WriteByte((byte)Data.Status);
		GClass1290.WriteBool(writer, Data.IsAgressor);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		Data = new GStruct180
		{
			PlayerProfileIDForSend = GClass1285.ReadString(reader),
			PlayerID = GClass1285.ReadInt(reader),
			IsEncoded = GClass1285.ReadBool(reader),
			Status = (RadioTransmitterStatus)reader.ReadByte(),
			IsAgressor = GClass1285.ReadBool(reader)
		};
	}
}

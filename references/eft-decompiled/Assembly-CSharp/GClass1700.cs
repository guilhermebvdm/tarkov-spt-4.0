using System;
using System.Runtime.CompilerServices;
using EFT;

public class GClass1700 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public GStruct179 Gstruct179_0;

	public GStruct179 Data
	{
		[CompilerGenerated]
		get
		{
			return Gstruct179_0;
		}
		[CompilerGenerated]
		set
		{
			Gstruct179_0 = value;
		}
	}

	public GClass1700()
	{
	}

	public GClass1700(GStruct179 data)
	{
		Data = data;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteInt(writer, Data.AllowedPlayers.Length);
		GStruct434[] allowedPlayers = Data.AllowedPlayers;
		foreach (GStruct434 lighthouseTraderZonePlayerData in allowedPlayers)
		{
			SerializeLighthouse(writer, lighthouseTraderZonePlayerData);
		}
		GClass1290.WriteInt(writer, Data.UnallowedPlayers.Length);
		allowedPlayers = Data.UnallowedPlayers;
		foreach (GStruct434 lighthouseTraderZonePlayerData2 in allowedPlayers)
		{
			SerializeLighthouse(writer, lighthouseTraderZonePlayerData2);
		}
	}

	public void SerializeLighthouse(EFTWriterClass writer, GStruct434 lighthouseTraderZonePlayerData)
	{
		GClass1290.WriteString(writer, lighthouseTraderZonePlayerData.Nickname);
		writer.WriteByte((byte)lighthouseTraderZonePlayerData.Status);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		int num = GClass1285.ReadInt(reader);
		GStruct434[] array = new GStruct434[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = DeserializeLighthouse(reader);
		}
		int num2 = GClass1285.ReadInt(reader);
		GStruct434[] array2 = new GStruct434[num2];
		for (int j = 0; j < num2; j++)
		{
			array2[j] = DeserializeLighthouse(reader);
		}
		Data = new GStruct179
		{
			AllowedPlayers = array,
			UnallowedPlayers = array2
		};
	}

	public GStruct434 DeserializeLighthouse(EFTReaderClass reader)
	{
		return new GStruct434
		{
			Nickname = GClass1285.ReadString(reader),
			Status = (RadioTransmitterStatus)reader.ReadByte()
		};
	}
}

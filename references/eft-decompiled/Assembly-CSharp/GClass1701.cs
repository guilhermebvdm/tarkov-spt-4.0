using System;
using System.Runtime.CompilerServices;
using EFT.Quests;

public class GClass1701 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public GStruct271 Gstruct271_0;

	public GStruct271 Data
	{
		[CompilerGenerated]
		get
		{
			return Gstruct271_0;
		}
		[CompilerGenerated]
		set
		{
			Gstruct271_0 = value;
		}
	}

	public GClass1701()
	{
	}

	public GClass1701(GStruct271 data)
	{
		Data = data;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteInt(writer, Data.Data.Length);
		for (int i = 0; i < Data.Data.Length; i++)
		{
			GClass1290.WriteInt(writer, Data.Data[i].QuestId);
			writer.WriteByte((byte)Data.Data[i].Status);
			GClass2064.WriteMongoId(writer, Data.Data[i].ConditionId);
			GClass1290.WriteDouble(writer, Data.Data[i].Value);
			GClass1290.WriteBool(writer, Data.Data[i].Notify);
		}
	}

	public void Deserialize(EFTReaderClass reader)
	{
		int num = GClass1285.ReadInt(reader);
		GStruct272[] array = new GStruct272[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = default(GStruct272);
			array[i].QuestId = GClass1285.ReadInt(reader);
			array[i].Status = (EQuestStatus)reader.ReadByte();
			array[i].ConditionId = GClass2064.ReadMongoId(reader);
			array[i].Value = GClass1285.ReadDouble(reader);
			array[i].Notify = GClass1285.ReadBool(reader);
		}
		Data = new GStruct271
		{
			Data = array
		};
	}
}

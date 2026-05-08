using System;
using ComponentAce.Compression.Libs.zlib;
using UnityEngine;

namespace EFT;

public abstract class AbstractGameSession : AbstractSession
{
	public class Class1098 : GInterface382
	{
		[NonSerialized]
		public string String_0;

		[NonSerialized]
		public string String_1;

		[NonSerialized]
		public bool Bool_0;

		[NonSerialized]
		public byte[] Byte_0;

		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public string String_2;

		public void Deserialize(EFTReaderClass reader)
		{
			String_0 = GClass1285.ReadString(reader);
			String_1 = GClass1285.ReadString(reader);
			Bool_0 = GClass1285.ReadBool(reader);
			Byte_0 = GClass1285.ReadBytesAndSize(reader);
			Int_0 = GClass1285.ReadInt(reader);
			String_2 = GClass1285.ReadString(reader);
		}

		public void Serialize(EFTWriterClass writer)
		{
			GClass1290.WriteString(writer, String_0);
			GClass1290.WriteString(writer, String_1);
			GClass1290.WriteBool(writer, Bool_0);
			GClass1290.WriteBytesAndSize(writer, Byte_0);
			GClass1290.WriteInt(writer, Int_0);
			GClass1290.WriteString(writer, String_2);
		}
	}

	public class Class1099 : GInterface382
	{
		public byte TracerId;

		public string[] Context;

		public void Deserialize(EFTReaderClass reader)
		{
			TracerId = reader.ReadByte();
			ushort num = GClass1285.ReadUShort(reader);
			if (num != 0)
			{
				Context = new string[num];
				for (int i = 0; i < num; i++)
				{
					byte[] bytes = GClass1285.ReadBytesAndSize(reader);
					Context[i] = SimpleZlib.Decompress(bytes);
				}
			}
		}

		public void Serialize(EFTWriterClass writer)
		{
			writer.WriteByte(TracerId);
			string[] context = Context;
			ushort num = (ushort)((context != null) ? ((uint)context.Length) : 0u);
			GClass1290.WriteUShort(writer, num);
			if (num > 0)
			{
				string[] context2 = Context;
				for (int i = 0; i < context2.Length; i++)
				{
					byte[] buffer = SimpleZlib.CompressToBytes(context2[i] ?? "", 9);
					GClass1290.WriteBytesAndSize(writer, buffer);
				}
			}
		}
	}

	public class Class1100 : Class1099
	{
		public string Message;

		public new void Deserialize(EFTReaderClass reader)
		{
			Message = GClass1285.ReadString(reader);
			base.Deserialize(reader);
		}

		public new void Serialize(EFTWriterClass writer)
		{
			GClass1290.WriteString(writer, Message);
			base.Serialize(writer);
		}
	}

	public class Class1101 : Class1099
	{
		public ETraceCode Code;

		public new void Deserialize(EFTReaderClass reader)
		{
			Code = (ETraceCode)reader.ReadByte();
			base.Deserialize(reader);
		}

		public new void Serialize(EFTWriterClass writer)
		{
			writer.WriteByte((byte)Code);
			base.Serialize(writer);
		}
	}

	public class Class1102 : GInterface382
	{
		[NonSerialized]
		public bool Bool_0;

		[NonSerialized]
		public bool Bool_1;

		[NonSerialized]
		public GameDateTime GameDateTime_0;

		[NonSerialized]
		public byte[] Byte_0;

		[NonSerialized]
		public byte[] Byte_1;

		[NonSerialized]
		public byte[] Byte_2;

		[NonSerialized]
		public ESeason Eseason_0;

		[NonSerialized]
		public SeasonsSettingsClass SeasonsSettingsClass;

		[NonSerialized]
		public bool Bool_2;

		[NonSerialized]
		public EMemberCategory EmemberCategory_0;

		[NonSerialized]
		public float Float_0;

		[NonSerialized]
		public byte[] Byte_3;

		[NonSerialized]
		public byte[] Byte_4;

		[NonSerialized]
		public Bounds Bounds_0;

		[NonSerialized]
		public ushort Ushort_0;

		[NonSerialized]
		public ENetLogsLevel EnetLogsLevel_0;

		[NonSerialized]
		public GClass639 Gclass639_0;

		[NonSerialized]
		public bool Bool_3;

		[NonSerialized]
		public GClass2175.Config Config_0 = new GClass2175.Config();

		[NonSerialized]
		public VoipSettingsClass VoipSettingsClass;

		public void Deserialize(EFTReaderClass reader)
		{
			Bool_0 = GClass1285.ReadBool(reader);
			Bool_1 = GClass1285.ReadBool(reader);
			GameDateTime_0 = GameDateTime.Deserialize(reader);
			Byte_0 = GClass1285.ReadBytesAndSize(reader);
			Byte_1 = GClass1285.ReadBytesAndSize(reader);
			Byte_2 = GClass1285.ReadBytesAndSize(reader);
			Eseason_0 = (ESeason)reader.ReadByte();
			SeasonsSettingsClass = new SeasonsSettingsClass();
			SeasonsSettingsClass.Deserialize(reader);
			Bool_2 = GClass1285.ReadBool(reader);
			EmemberCategory_0 = (EMemberCategory)GClass1285.ReadInt(reader);
			Float_0 = GClass1285.ReadFloat(reader);
			Byte_3 = GClass1285.ReadBytesAndSize(reader);
			Byte_4 = GClass1285.ReadBytesAndSize(reader);
			Vector3 min = GClass1285.ReadVector3(reader);
			Vector3 max = GClass1285.ReadVector3(reader);
			Bounds_0 = new Bounds
			{
				min = min,
				max = max
			};
			Ushort_0 = GClass1285.ReadUShort(reader);
			EnetLogsLevel_0 = (ENetLogsLevel)reader.ReadByte();
			Gclass639_0 = GClass639.Deserialize(reader);
			Bool_3 = GClass1285.ReadBool(reader);
			if (Bool_3)
			{
				Config_0.Deserialize(reader);
			}
			if (GClass1285.ReadBool(reader))
			{
				VoipSettingsClass = new VoipSettingsClass();
				VoipSettingsClass.Deserialize(reader);
			}
		}

		public void Serialize(EFTWriterClass writer)
		{
			GClass1290.WriteBool(writer, Bool_0);
			GClass1290.WriteBool(writer, Bool_1);
			GameDateTime_0.Serialize(writer, gameOnly: true);
			GClass1290.WriteBytesAndSize(writer, Byte_0);
			GClass1290.WriteBytesAndSize(writer, Byte_1);
			GClass1290.WriteBytesAndSize(writer, Byte_2);
			writer.WriteByte((byte)Eseason_0);
			SeasonsSettingsClass.Serialize(writer);
			GClass1290.WriteBool(writer, Bool_2);
			GClass1290.WriteInt(writer, (int)EmemberCategory_0);
			GClass1290.WriteFloat(writer, Float_0);
			GClass1290.WriteBytesAndSize(writer, Byte_3);
			GClass1290.WriteBytesAndSize(writer, Byte_4);
			GClass1290.WriteVector3(writer, Bounds_0.min);
			GClass1290.WriteVector3(writer, Bounds_0.max);
			GClass1290.WriteUShort(writer, Ushort_0);
			writer.WriteByte((byte)EnetLogsLevel_0);
			Gclass639_0.Serialize(writer);
			GClass1290.WriteBool(writer, Bool_3);
			if (Bool_3)
			{
				Config_0.Serialize(writer);
			}
			GClass1290.WriteBool(writer, VoipSettingsClass != null);
			VoipSettingsClass?.Serialize(writer);
		}
	}

	public class Class1103 : GInterface382
	{
		[NonSerialized]
		public const int Int_0 = 100500;

		[NonSerialized]
		public const int Int_1 = 100501;

		[NonSerialized]
		public const int Int_2 = 100502;

		[NonSerialized]
		public const int Int_3 = 100503;

		[NonSerialized]
		public int Int_4;

		public void Deserialize(EFTReaderClass reader)
		{
			Int_4 = GClass1285.ReadInt(reader);
		}

		public void Serialize(EFTWriterClass writer)
		{
			GClass1290.WriteInt(writer, Int_4);
		}
	}

	public class Class1104 : GInterface382
	{
		[NonSerialized]
		public float Float_0;

		public void Deserialize(EFTReaderClass reader)
		{
			Float_0 = GClass1285.ReadFloat(reader);
		}

		public void Serialize(EFTWriterClass writer)
		{
			GClass1290.WriteFloat(writer, Float_0);
		}
	}

	public class Class1105 : GInterface382
	{
		[NonSerialized]
		public byte[] Byte_0;

		public void Serialize(EFTWriterClass writer)
		{
			GClass1290.WriteBytesAndSize(writer, Byte_0);
		}

		public void Deserialize(EFTReaderClass reader)
		{
			Byte_0 = GClass1285.ReadBytesAndSize(reader);
		}
	}

	public class Class1106 : GInterface382
	{
		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public byte[] Byte_0;

		[NonSerialized]
		public byte[] Byte_1;

		public void Deserialize(EFTReaderClass reader)
		{
			Int_0 = GClass1285.ReadInt(reader);
			Byte_0 = GClass1285.ReadBytesAndSize(reader);
			Byte_1 = GClass1285.ReadBytesAndSize(reader);
		}

		public void Serialize(EFTWriterClass writer)
		{
			GClass1290.WriteInt(writer, Int_0);
			GClass1290.WriteBytesAndSize(writer, Byte_0, 0, Byte_0.Length);
			GClass1290.WriteBytesAndSize(writer, Byte_1, 0, Byte_1.Length);
		}
	}

	public class Class1107 : GInterface382
	{
		[NonSerialized]
		public string String_0;

		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public float Float_0;

		public void Deserialize(EFTReaderClass reader)
		{
			String_0 = GClass1285.ReadString(reader);
			Int_0 = GClass1285.ReadInt(reader);
			Float_0 = GClass1285.ReadFloat(reader);
		}

		public void Serialize(EFTWriterClass writer)
		{
			GClass1290.WriteString(writer, String_0);
			GClass1290.WriteInt(writer, Int_0);
			GClass1290.WriteFloat(writer, Float_0);
		}
	}

	public static T Create<T>(Transform parent, string name, string profileId, string token) where T : AbstractGameSession
	{
		return AbstractSession.CreateSession<T>(parent, name, profileId, token);
	}

	public AbstractGameSession()
	{
	}
}

using System;
using System.Runtime.CompilerServices;

public class GClass1702 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public byte[] Byte_0;

	public string Id
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

	public byte[] Data
	{
		[CompilerGenerated]
		get
		{
			return Byte_0;
		}
		[CompilerGenerated]
		set
		{
			Byte_0 = value;
		}
	}

	public GClass1702()
	{
	}

	public GClass1702(string id, byte[] data)
	{
		Id = id;
		Data = data;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteString(writer, Id);
		GClass1290.WriteBytesAndSize(writer, Data, 0, Data.Length);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		Id = GClass1285.ReadString(reader);
		Data = GClass1285.ReadBytesAndSize(reader);
	}
}

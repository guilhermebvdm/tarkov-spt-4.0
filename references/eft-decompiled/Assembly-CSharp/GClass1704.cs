using System;
using System.Runtime.CompilerServices;

public class GClass1704 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public byte[] Byte_0;

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

	public GClass1704()
	{
	}

	public GClass1704(byte[] data)
	{
		Data = data;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteBytesAndSize(writer, Data, 0, Data.Length);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		Data = GClass1285.ReadBytesAndSize(reader);
	}
}

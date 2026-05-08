using System;
using System.Runtime.CompilerServices;

public class GClass1686 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

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

	public GClass1686()
	{
	}

	public GClass1686(string id)
	{
		Id = id;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteString(writer, Id);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		Id = GClass1285.ReadString(reader);
	}
}

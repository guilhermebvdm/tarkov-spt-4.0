using System;
using System.Runtime.CompilerServices;

public class GClass1680 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	public string Size
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

	public GClass1680()
	{
	}

	public GClass1680(string Size)
	{
		this.Size = Size;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteString(writer, Size);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		Size = GClass1285.ReadString(reader);
	}
}

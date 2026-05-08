using System;
using System.Runtime.CompilerServices;

public class GClass1696 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	public string Reporter
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

	public GClass1696()
	{
	}

	public GClass1696(string reporter)
	{
		Reporter = reporter;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteString(writer, Reporter);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		Reporter = GClass1285.ReadString(reader);
	}
}

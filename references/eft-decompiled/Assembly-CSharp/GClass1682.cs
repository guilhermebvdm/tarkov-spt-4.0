using System;
using System.Runtime.CompilerServices;

public class GClass1682 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public string PlayerProfileID
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

	public bool IsPaused
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public GClass1682()
	{
	}

	public GClass1682(string playerProfileID, bool isPaused)
	{
		PlayerProfileID = playerProfileID;
		IsPaused = isPaused;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteString(writer, PlayerProfileID);
		GClass1290.WriteBool(writer, IsPaused);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		PlayerProfileID = GClass1285.ReadString(reader);
		IsPaused = GClass1285.ReadBool(reader);
	}
}

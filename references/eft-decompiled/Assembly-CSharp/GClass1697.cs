using System;
using System.Runtime.CompilerServices;

public class GClass1697 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public bool CanSendAirdrop
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

	public GClass1697()
	{
	}

	public GClass1697(bool canSendAirdrop)
	{
		CanSendAirdrop = canSendAirdrop;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteBool(writer, CanSendAirdrop);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		CanSendAirdrop = GClass1285.ReadBool(reader);
	}
}

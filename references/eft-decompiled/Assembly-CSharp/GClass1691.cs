using System;
using System.Runtime.CompilerServices;
using EFT;

public class GClass1691 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public ExitStatus ExitStatus_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	public ExitStatus ExitStatus
	{
		[CompilerGenerated]
		get
		{
			return ExitStatus_0;
		}
		[CompilerGenerated]
		set
		{
			ExitStatus_0 = value;
		}
	}

	public int PlaySeconds
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public int ErrorCode
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public GClass1691()
	{
	}

	public GClass1691(ExitStatus exitStatus, int playSeconds, int errorCode)
	{
		ExitStatus = exitStatus;
		PlaySeconds = playSeconds;
		ErrorCode = errorCode;
	}

	public void Serialize(EFTWriterClass writer)
	{
		writer.WriteByte((byte)ExitStatus);
		GClass1290.WriteInt(writer, PlaySeconds);
		GClass1290.WriteInt(writer, ErrorCode);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		ExitStatus = (ExitStatus)reader.ReadByte();
		PlaySeconds = GClass1285.ReadInt(reader);
		ErrorCode = GClass1285.ReadInt(reader);
	}
}

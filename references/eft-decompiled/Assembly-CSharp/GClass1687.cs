using System;
using System.Runtime.CompilerServices;

public class GClass1687 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public ushort Ushort_0;

	[NonSerialized]
	[CompilerGenerated]
	public ushort Ushort_1;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	public ushort ActivitiesCounter
	{
		[CompilerGenerated]
		get
		{
			return Ushort_0;
		}
		[CompilerGenerated]
		set
		{
			Ushort_0 = value;
		}
	}

	public ushort MinCounter
	{
		[CompilerGenerated]
		get
		{
			return Ushort_1;
		}
		[CompilerGenerated]
		set
		{
			Ushort_1 = value;
		}
	}

	public int Seconds
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

	public GClass1687()
	{
	}

	public GClass1687(ushort activitiesCounter, ushort minCounter, int seconds)
	{
		ActivitiesCounter = activitiesCounter;
		MinCounter = minCounter;
		Seconds = seconds;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteUShort(writer, ActivitiesCounter);
		GClass1290.WriteUShort(writer, MinCounter);
		GClass1290.WriteInt(writer, Seconds);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		ActivitiesCounter = GClass1285.ReadUShort(reader);
		MinCounter = GClass1285.ReadUShort(reader);
		Seconds = GClass1285.ReadInt(reader);
	}
}

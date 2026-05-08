using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass1689 : GInterface382
{
	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	public Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
		[CompilerGenerated]
		set
		{
			Vector3_0 = value;
		}
	}

	public int ExfiltrationId
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

	public string EntryPoint
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

	public GClass1689()
	{
	}

	public GClass1689(Vector3 position, int exfiltrationId, string entryPoint)
	{
		Position = position;
		ExfiltrationId = exfiltrationId;
		EntryPoint = entryPoint;
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteVector3(writer, Position);
		GClass1290.WriteInt(writer, ExfiltrationId);
		GClass1290.WriteString(writer, EntryPoint);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		Position = GClass1285.ReadVector3(reader);
		ExfiltrationId = GClass1285.ReadInt(reader);
		EntryPoint = GClass1285.ReadString(reader);
	}
}

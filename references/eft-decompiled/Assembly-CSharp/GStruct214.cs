using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct GStruct214 : GInterface119
{
	public byte Iteration;

	public float LastServerTime;

	public float ClientTime;

	public Vector3 TransformPosition;

	public Vector2 Rotation;

	public Vector2 MovementDirection;

	public EPlayerState State;

	[NonSerialized]
	public const float Float_0 = -90f;

	[NonSerialized]
	public const float Float_1 = 90f;

	[NonSerialized]
	public const float Float_2 = -1f;

	[NonSerialized]
	public const float Float_3 = 1f;

	[NonSerialized]
	[CompilerGenerated]
	public double Double_0;

	[NonSerialized]
	[CompilerGenerated]
	public double Double_1;

	public double remoteTime
	{
		[CompilerGenerated]
		readonly get
		{
			return Double_0;
		}
		[CompilerGenerated]
		set
		{
			Double_0 = value;
		}
	}

	public double localTime
	{
		[CompilerGenerated]
		readonly get
		{
			return Double_1;
		}
		[CompilerGenerated]
		set
		{
			Double_1 = value;
		}
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(Iteration);
		writer.Write(LastServerTime);
		writer.Write(ClientTime);
		writer.Write(TransformPosition);
		method_0(writer);
		method_2(writer);
		writer.Write(Rotation);
		writer.Write(MovementDirection);
		writer.Write((byte)State);
	}

	public void Deserialize(IDataReader reader)
	{
		Iteration = reader.ReadByte();
		LastServerTime = reader.ReadFloat();
		ClientTime = reader.ReadFloat();
		TransformPosition = reader.ReadVector3();
		method_1(reader);
		method_3(reader);
		Rotation = reader.ReadVector2();
		MovementDirection = reader.ReadVector2();
		State = (EPlayerState)reader.ReadByte();
	}

	public void method_0(GInterface131 writer)
	{
		writer.Write(Rotation.x);
		writer.Write(GClass1295.ScaleFloatToShort(Rotation.y, -90f, 90f));
	}

	public void method_1(IDataReader reader)
	{
		Rotation = new Vector2(reader.ReadFloat(), GClass1295.ScaleShortToFloat(reader.ReadInt16(), -90f, 90f));
	}

	public void method_2(GInterface131 writer)
	{
		writer.Write(GClass1295.ScaleFloatToByte(MovementDirection.x, -1f, 1f));
		writer.Write(GClass1295.ScaleFloatToByte(MovementDirection.y, -1f, 1f));
	}

	public void method_3(IDataReader reader)
	{
		MovementDirection = new Vector2(GClass1295.ScaleByteToFloat(reader.ReadByte(), -1f, 1f), GClass1295.ScaleByteToFloat(reader.ReadByte(), -1f, 1f));
	}
}

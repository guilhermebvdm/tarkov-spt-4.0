using System;
using System.IO;
using UnityEngine;

public abstract class GClass881
{
	public static void Write(this BinaryWriter writer, ushort[] val)
	{
		int num = ((val != null) ? val.Length : 0);
		writer.Write(num);
		for (int i = 0; i < num; i++)
		{
			writer.Write(val[i]);
		}
	}

	public static ushort[] ReadUInt16Array(this BinaryReader reader)
	{
		int num = reader.ReadInt32();
		if (num == 0)
		{
			return Array.Empty<ushort>();
		}
		ushort[] array = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = reader.ReadUInt16();
		}
		return array;
	}

	public static void Write(this BinaryWriter writer, Vector3 val)
	{
		writer.Write(val.x);
		writer.Write(val.y);
		writer.Write(val.z);
	}

	public static Vector3 ReadVector3(this BinaryReader reader)
	{
		Vector3 result = default(Vector3);
		result.x = reader.ReadSingle();
		result.y = reader.ReadSingle();
		result.z = reader.ReadSingle();
		return result;
	}

	public static void Write(this BinaryWriter writer, Quaternion val)
	{
		writer.Write(val.x);
		writer.Write(val.y);
		writer.Write(val.z);
		writer.Write(val.w);
	}

	public static Quaternion ReadQuaternion(this BinaryReader reader)
	{
		Quaternion result = default(Quaternion);
		result.x = reader.ReadSingle();
		result.y = reader.ReadSingle();
		result.z = reader.ReadSingle();
		result.w = reader.ReadSingle();
		return result;
	}
}

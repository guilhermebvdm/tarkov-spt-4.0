using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class GClass1285
{
	public static byte ReadByte(this EFTReaderClass reader)
	{
		return reader.method_0<byte>();
	}

	public static byte? ReadByteNullable(this EFTReaderClass reader)
	{
		return reader.method_1<byte>();
	}

	public static sbyte ReadSByte(this EFTReaderClass reader)
	{
		return reader.method_0<sbyte>();
	}

	public static sbyte? ReadSByteNullable(this EFTReaderClass reader)
	{
		return reader.method_1<sbyte>();
	}

	public static char ReadChar(this EFTReaderClass reader)
	{
		return (char)reader.method_0<ushort>();
	}

	public static char? ReadCharNullable(this EFTReaderClass reader)
	{
		return (char?)reader.method_1<ushort>();
	}

	public static bool ReadBool(this EFTReaderClass reader)
	{
		return reader.method_0<byte>() != 0;
	}

	public static bool? ReadBoolNullable(this EFTReaderClass reader)
	{
		byte? b = reader.method_1<byte>();
		if (!b.HasValue)
		{
			return null;
		}
		return b.Value != 0;
	}

	public static short ReadShort(this EFTReaderClass reader)
	{
		return (short)ReadUShort(reader);
	}

	public static short? ReadShortNullable(this EFTReaderClass reader)
	{
		return reader.method_1<short>();
	}

	public static ushort ReadUShort(this EFTReaderClass reader)
	{
		return reader.method_0<ushort>();
	}

	public static ushort? ReadUShortNullable(this EFTReaderClass reader)
	{
		return reader.method_1<ushort>();
	}

	public static int ReadInt(this EFTReaderClass reader)
	{
		return reader.method_0<int>();
	}

	public static int? ReadIntNullable(this EFTReaderClass reader)
	{
		return reader.method_1<int>();
	}

	public static uint ReadUInt(this EFTReaderClass reader)
	{
		return reader.method_0<uint>();
	}

	public static uint? ReadUIntNullable(this EFTReaderClass reader)
	{
		return reader.method_1<uint>();
	}

	public static long ReadLong(this EFTReaderClass reader)
	{
		return reader.method_0<long>();
	}

	public static long? ReadLongNullable(this EFTReaderClass reader)
	{
		return reader.method_1<long>();
	}

	public static ulong ReadULong(this EFTReaderClass reader)
	{
		return reader.method_0<ulong>();
	}

	public static ulong? ReadULongNullable(this EFTReaderClass reader)
	{
		return reader.method_1<ulong>();
	}

	public static float ReadFloat(this EFTReaderClass reader)
	{
		return reader.method_0<float>();
	}

	public static float? ReadFloatNullable(this EFTReaderClass reader)
	{
		return reader.method_1<float>();
	}

	public static double ReadDouble(this EFTReaderClass reader)
	{
		return reader.method_0<double>();
	}

	public static double? ReadDoubleNullable(this EFTReaderClass reader)
	{
		return reader.method_1<double>();
	}

	public static decimal ReadDecimal(this EFTReaderClass reader)
	{
		return reader.method_0<decimal>();
	}

	public static decimal? ReadDecimalNullable(this EFTReaderClass reader)
	{
		return reader.method_1<decimal>();
	}

	public static TimeSpan ReadTimeSpan(this EFTReaderClass reader)
	{
		return new TimeSpan(ReadLong(reader));
	}

	public static string ReadString(this EFTReaderClass reader)
	{
		ushort num = ReadUShort(reader);
		if (num == 0)
		{
			return null;
		}
		int num2 = num - 1;
		if (num2 >= 32768)
		{
			throw new EndOfStreamException($"ReadString too long: {num2}. Limit is: {32768}");
		}
		ArraySegment<byte> arraySegment = reader.ReadBytesSegment(num2);
		return reader.Utf8Encoding_0.GetString(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
	}

	public static byte[] ReadBytesAndSize(this EFTReaderClass reader)
	{
		uint num = ReadUInt(reader);
		if (num != 0)
		{
			return ReadBytes(reader, checked((int)(num - 1)));
		}
		return null;
	}

	public static byte[] ReadBytes(this EFTReaderClass reader, int count)
	{
		byte[] array = new byte[count];
		reader.ReadBytes(array, count);
		return array;
	}

	public static ArraySegment<byte> ReadBytesAndSizeSegment(this EFTReaderClass reader)
	{
		uint num = ReadUInt(reader);
		if (num != 0)
		{
			return reader.ReadBytesSegment(checked((int)(num - 1)));
		}
		return default(ArraySegment<byte>);
	}

	public static Vector2 ReadVector2(this EFTReaderClass reader)
	{
		return reader.method_0<Vector2>();
	}

	public static Vector2? ReadVector2Nullable(this EFTReaderClass reader)
	{
		return reader.method_1<Vector2>();
	}

	public static Vector3 ReadVector3(this EFTReaderClass reader)
	{
		return reader.method_0<Vector3>();
	}

	public static Vector3? ReadVector3Nullable(this EFTReaderClass reader)
	{
		return reader.method_1<Vector3>();
	}

	public static Vector4 ReadVector4(this EFTReaderClass reader)
	{
		return reader.method_0<Vector4>();
	}

	public static Vector4? ReadVector4Nullable(this EFTReaderClass reader)
	{
		return reader.method_1<Vector4>();
	}

	public static Vector2Int ReadVector2Int(this EFTReaderClass reader)
	{
		return reader.method_0<Vector2Int>();
	}

	public static Vector2Int? ReadVector2IntNullable(this EFTReaderClass reader)
	{
		return reader.method_1<Vector2Int>();
	}

	public static Vector3Int ReadVector3Int(this EFTReaderClass reader)
	{
		return reader.method_0<Vector3Int>();
	}

	public static Vector3Int? ReadVector3IntNullable(this EFTReaderClass reader)
	{
		return reader.method_1<Vector3Int>();
	}

	public static Color ReadColor(this EFTReaderClass reader)
	{
		return reader.method_0<Color>();
	}

	public static Color? ReadColorNullable(this EFTReaderClass reader)
	{
		return reader.method_1<Color>();
	}

	public static Color32 ReadColor32(this EFTReaderClass reader)
	{
		return reader.method_0<Color32>();
	}

	public static Color32? ReadColor32Nullable(this EFTReaderClass reader)
	{
		return reader.method_1<Color32>();
	}

	public static Quaternion ReadQuaternion(this EFTReaderClass reader)
	{
		return reader.method_0<Quaternion>();
	}

	public static Quaternion? ReadQuaternionNullable(this EFTReaderClass reader)
	{
		return reader.method_1<Quaternion>();
	}

	public static Rect ReadRect(this EFTReaderClass reader)
	{
		return reader.method_0<Rect>();
	}

	public static Rect? ReadRectNullable(this EFTReaderClass reader)
	{
		return reader.method_1<Rect>();
	}

	public static Plane ReadPlane(this EFTReaderClass reader)
	{
		return reader.method_0<Plane>();
	}

	public static Plane? ReadPlaneNullable(this EFTReaderClass reader)
	{
		return reader.method_1<Plane>();
	}

	public static Ray ReadRay(this EFTReaderClass reader)
	{
		return reader.method_0<Ray>();
	}

	public static Ray? ReadRayNullable(this EFTReaderClass reader)
	{
		return reader.method_1<Ray>();
	}

	public static Matrix4x4 ReadMatrix4x4(this EFTReaderClass reader)
	{
		return reader.method_0<Matrix4x4>();
	}

	public static Matrix4x4? ReadMatrix4x4Nullable(this EFTReaderClass reader)
	{
		return reader.method_1<Matrix4x4>();
	}

	public static Guid ReadGuid(this EFTReaderClass reader)
	{
		if (reader.Remaining < 16)
		{
			throw new EndOfStreamException($"ReadGuid out of range: {reader}");
		}
		ReadOnlySpan<byte> b = new ReadOnlySpan<byte>(reader.ArraySegment_0.Array, reader.ArraySegment_0.Offset + reader.Position, 16);
		reader.Position += 16;
		return new Guid(b);
	}

	public static Guid? ReadGuidNullable(this EFTReaderClass reader)
	{
		if (!ReadBool(reader))
		{
			return null;
		}
		return ReadGuid(reader);
	}

	public static List<T> ReadList<T>(this EFTReaderClass reader)
	{
		int num = ReadInt(reader);
		if (num < 0)
		{
			return null;
		}
		List<T> list = new List<T>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add(reader.Read<T>());
		}
		return list;
	}

	public static T[] ReadArray<T>(this EFTReaderClass reader)
	{
		int num = ReadInt(reader);
		if (num < 0)
		{
			return null;
		}
		if (num > reader.Remaining)
		{
			throw new EndOfStreamException($"Received array that is too large: {num}");
		}
		T[] array = new T[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = reader.Read<T>();
		}
		return array;
	}

	public static Uri ReadUri(this EFTReaderClass reader)
	{
		string text = ReadString(reader);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return new Uri(text);
		}
		return null;
	}

	public static Texture2D ReadTexture2D(this EFTReaderClass reader)
	{
		short num = ReadShort(reader);
		if (num == -1)
		{
			return null;
		}
		short height = ReadShort(reader);
		Texture2D texture2D = new Texture2D(num, height);
		Color32[] pixels = ReadArray<Color32>(reader);
		texture2D.SetPixels32(pixels);
		texture2D.Apply();
		return texture2D;
	}

	public static Sprite ReadSprite(this EFTReaderClass reader)
	{
		Texture2D texture2D = ReadTexture2D(reader);
		if (texture2D == null)
		{
			return null;
		}
		return Sprite.Create(texture2D, ReadRect(reader), ReadVector2(reader));
	}
}

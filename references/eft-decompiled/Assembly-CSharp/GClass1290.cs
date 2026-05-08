using System;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class GClass1290
{
	[StructLayout(LayoutKind.Explicit)]
	public struct Struct251
	{
		[FieldOffset(0)]
		public double doubleValue;

		[FieldOffset(0)]
		public ulong longValue;
	}

	public static void WriteByte(this EFTWriterClass writer, byte value)
	{
		writer.method_1(value);
	}

	public static void WriteByteNullable(this EFTWriterClass writer, byte? value)
	{
		writer.method_2(value);
	}

	public static void WriteSByte(this EFTWriterClass writer, sbyte value)
	{
		writer.method_1(value);
	}

	public static void WriteSByteNullable(this EFTWriterClass writer, sbyte? value)
	{
		writer.method_2(value);
	}

	public static void WriteChar(this EFTWriterClass writer, char value)
	{
		writer.method_1((ushort)value);
	}

	public static void WriteCharNullable(this EFTWriterClass writer, char? value)
	{
		writer.method_2((ushort?)value);
	}

	public static void WriteBool(this EFTWriterClass writer, bool value)
	{
		writer.method_1((byte)(value ? 1u : 0u));
	}

	public static void WriteBoolNullable(this EFTWriterClass writer, bool? value)
	{
		writer.method_2(value.HasValue ? new byte?((byte)(value.Value ? 1u : 0u)) : ((byte?)null));
	}

	public static void WriteShort(this EFTWriterClass writer, short value)
	{
		writer.method_1(value);
	}

	public static void WriteShortNullable(this EFTWriterClass writer, short? value)
	{
		writer.method_2(value);
	}

	public static void WriteUShort(this EFTWriterClass writer, ushort value)
	{
		writer.method_1(value);
	}

	public static void WriteUShortNullable(this EFTWriterClass writer, ushort? value)
	{
		writer.method_2(value);
	}

	public static void WriteInt(this EFTWriterClass writer, int value)
	{
		writer.method_1(value);
	}

	public static void WriteIntNullable(this EFTWriterClass writer, int? value)
	{
		writer.method_2(value);
	}

	public static void WriteUInt(this EFTWriterClass writer, uint value)
	{
		writer.method_1(value);
	}

	public static void WriteUIntNullable(this EFTWriterClass writer, uint? value)
	{
		writer.method_2(value);
	}

	public static void WriteLong(this EFTWriterClass writer, long value)
	{
		writer.method_1(value);
	}

	public static void WriteLongNullable(this EFTWriterClass writer, long? value)
	{
		writer.method_2(value);
	}

	public static void WriteULong(this EFTWriterClass writer, ulong value)
	{
		writer.method_1(value);
	}

	public static void WriteULongNullable(this EFTWriterClass writer, ulong? value)
	{
		writer.method_2(value);
	}

	public static void WriteFloat(this EFTWriterClass writer, float value)
	{
		writer.method_1(value);
	}

	public static void WriteFloatNullable(this EFTWriterClass writer, float? value)
	{
		writer.method_2(value);
	}

	public static void WriteTimeSpan(this EFTWriterClass writer, TimeSpan value)
	{
		writer.method_1(value.Ticks);
	}

	public static void WriteDouble(this EFTWriterClass writer, double value)
	{
		writer.method_1(value);
	}

	public static void WriteDoubleNullable(this EFTWriterClass writer, double? value)
	{
		writer.method_2(value);
	}

	public static void WriteDecimal(this EFTWriterClass writer, decimal value)
	{
		writer.method_1(value);
	}

	public static void WriteDecimalNullable(this EFTWriterClass writer, decimal? value)
	{
		writer.method_2(value);
	}

	public static void WriteString(this EFTWriterClass writer, string value)
	{
		if (value == null)
		{
			WriteUShort(writer, 0);
			return;
		}
		int maxByteCount = writer.Utf8Encoding_0.GetMaxByteCount(value.Length);
		writer.method_0(writer.Position + 2 + maxByteCount);
		int bytes = writer.Utf8Encoding_0.GetBytes(value, 0, value.Length, writer.Byte_0, writer.Position + 2);
		if (bytes >= 32768)
		{
			throw new IndexOutOfRangeException($"NetworkWriter.Write(string) too long: {bytes}. Limit: {32768}");
		}
		WriteUShort(writer, checked((ushort)(bytes + 1)));
		writer.Position += bytes;
	}

	public static void WriteBytesAndSizeSegment(this EFTWriterClass writer, ArraySegment<byte> buffer)
	{
		WriteBytesAndSize(writer, buffer.Array, buffer.Offset, buffer.Count);
	}

	public static void WriteBytesAndSize(this EFTWriterClass writer, byte[] buffer)
	{
		WriteBytesAndSize(writer, buffer, 0, (buffer != null) ? buffer.Length : 0);
	}

	public static void WriteBytesAndSize(this EFTWriterClass writer, byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			WriteUInt(writer, 0u);
			return;
		}
		WriteUInt(writer, checked((uint)count) + 1);
		writer.WriteBytes(buffer, offset, count);
	}

	public static void WriteVector2(this EFTWriterClass writer, Vector2 value)
	{
		writer.method_1(value);
	}

	public static void WriteVector2Nullable(this EFTWriterClass writer, Vector2? value)
	{
		writer.method_2(value);
	}

	public static void WriteVector3(this EFTWriterClass writer, Vector3 value)
	{
		writer.method_1(value);
	}

	public static void WriteVector3Nullable(this EFTWriterClass writer, Vector3? value)
	{
		writer.method_2(value);
	}

	public static void WriteVector4(this EFTWriterClass writer, Vector4 value)
	{
		writer.method_1(value);
	}

	public static void WriteVector4Nullable(this EFTWriterClass writer, Vector4? value)
	{
		writer.method_2(value);
	}

	public static void WriteVector2Int(this EFTWriterClass writer, Vector2Int value)
	{
		writer.method_1(value);
	}

	public static void WriteVector2IntNullable(this EFTWriterClass writer, Vector2Int? value)
	{
		writer.method_2(value);
	}

	public static void WriteVector3Int(this EFTWriterClass writer, Vector3Int value)
	{
		writer.method_1(value);
	}

	public static void WriteVector3IntNullable(this EFTWriterClass writer, Vector3Int? value)
	{
		writer.method_2(value);
	}

	public static void WriteColor(this EFTWriterClass writer, Color value)
	{
		writer.method_1(value);
	}

	public static void WriteColorNullable(this EFTWriterClass writer, Color? value)
	{
		writer.method_2(value);
	}

	public static void WriteColor32(this EFTWriterClass writer, Color32 value)
	{
		writer.method_1(value);
	}

	public static void WriteColor32Nullable(this EFTWriterClass writer, Color32? value)
	{
		writer.method_2(value);
	}

	public static void WriteQuaternion(this EFTWriterClass writer, Quaternion value)
	{
		writer.method_1(value);
	}

	public static void WriteQuaternionNullable(this EFTWriterClass writer, Quaternion? value)
	{
		writer.method_2(value);
	}

	public static void WriteRect(this EFTWriterClass writer, Rect value)
	{
		writer.method_1(value);
	}

	public static void WriteRectNullable(this EFTWriterClass writer, Rect? value)
	{
		writer.method_2(value);
	}

	public static void WritePlane(this EFTWriterClass writer, Plane value)
	{
		writer.method_1(value);
	}

	public static void WritePlaneNullable(this EFTWriterClass writer, Plane? value)
	{
		writer.method_2(value);
	}

	public static void WriteRay(this EFTWriterClass writer, Ray value)
	{
		writer.method_1(value);
	}

	public static void WriteRayNullable(this EFTWriterClass writer, Ray? value)
	{
		writer.method_2(value);
	}

	public static void WriteMatrix4x4(this EFTWriterClass writer, Matrix4x4 value)
	{
		writer.method_1(value);
	}

	public static void WriteMatrix4x4Nullable(this EFTWriterClass writer, Matrix4x4? value)
	{
		writer.method_2(value);
	}

	public static void WriteGuid(this EFTWriterClass writer, Guid value)
	{
		writer.method_0(writer.Position + 16);
		value.TryWriteBytes(new Span<byte>(writer.Byte_0, writer.Position, 16));
		writer.Position += 16;
	}

	public static void WriteGuidNullable(this EFTWriterClass writer, Guid? value)
	{
		WriteBool(writer, value.HasValue);
		if (value.HasValue)
		{
			WriteGuid(writer, value.Value);
		}
	}

	public static void WriteUri(this EFTWriterClass writer, Uri uri)
	{
		WriteString(writer, uri?.ToString());
	}
}

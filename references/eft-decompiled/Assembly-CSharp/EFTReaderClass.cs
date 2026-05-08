using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class EFTReaderClass
{
	[NonSerialized]
	public ArraySegment<byte> ArraySegment_0;

	public int Position;

	[NonSerialized]
	public UTF8Encoding Utf8Encoding_0 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	public int Remaining => ArraySegment_0.Count - Position;

	public int Capacity => ArraySegment_0.Count;

	public EFTReaderClass(ArraySegment<byte> segment)
	{
		ArraySegment_0 = segment;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBuffer(ArraySegment<byte> segment)
	{
		ArraySegment_0 = segment;
		Position = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe T method_0<[Attribute1] T>() where T : struct
	{
		int num = Unsafe.SizeOf<T>();
		if (Remaining < num)
		{
			throw new EndOfStreamException($"ReadBlittable<{typeof(T)}> not enough data in buffer to read {num} bytes: {ToString()}");
		}
		T result;
		fixed (byte* ptr = &ArraySegment_0.Array[ArraySegment_0.Offset + Position])
		{
			result = Unsafe.Read<T>(ptr);
		}
		Position += num;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? method_1<[Attribute1] T>() where T : struct
	{
		if (ReadByte() == 0)
		{
			return null;
		}
		return method_0<T>();
	}

	public byte ReadByte()
	{
		return method_0<byte>();
	}

	public byte[] ReadBytes(byte[] bytes, int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("ReadBytes requires count >= 0");
		}
		if (count > bytes.Length)
		{
			throw new EndOfStreamException($"ReadBytes can't read {count} + bytes because the passed byte[] only has length {bytes.Length}");
		}
		if (Remaining < count)
		{
			throw new EndOfStreamException($"ReadBytesSegment can't read {count} bytes because it would read past the end of the stream. {ToString()}");
		}
		Array.Copy(ArraySegment_0.Array, ArraySegment_0.Offset + Position, bytes, 0, count);
		Position += count;
		return bytes;
	}

	public ArraySegment<byte> ReadBytesSegment(int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("ReadBytesSegment requires count >= 0");
		}
		if (Remaining < count)
		{
			throw new EndOfStreamException($"ReadBytesSegment can't read {count} bytes because it would read past the end of the stream. {ToString()}");
		}
		ArraySegment<byte> result = new ArraySegment<byte>(ArraySegment_0.Array, ArraySegment_0.Offset + Position, count);
		Position += count;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Read<T>()
	{
		Func<EFTReaderClass, T> read = GClass1284<T>.read;
		if (read == null)
		{
			Debug.LogError($"No reader found for {typeof(T)}. Use a type supported by Mirror or define a custom reader extension for {typeof(T)}.");
			return default(T);
		}
		return read(this);
	}

	public override string ToString()
	{
		return $"[{GClass1298.ToHexString(ArraySegment_0)} @ {Position}/{Capacity}]";
	}
}

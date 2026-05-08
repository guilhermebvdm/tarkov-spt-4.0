using System;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class EFTWriterClass
{
	public const int MaxStringLength = 32768;

	public const int DefaultCapacity = 1500;

	[NonSerialized]
	public byte[] Byte_0 = new byte[1500];

	public int Position;

	[NonSerialized]
	public UTF8Encoding Utf8Encoding_0 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	public int Capacity => Byte_0.Length;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Reset()
	{
		Position = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void method_0(int value)
	{
		if (Byte_0.Length < value)
		{
			int newSize = Math.Max(value, Byte_0.Length * 2);
			Array.Resize(ref Byte_0, newSize);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte[] ToArray()
	{
		byte[] array = new byte[Position];
		Array.ConstrainedCopy(Byte_0, 0, array, 0, Position);
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ArraySegment<byte> ToArraySegment()
	{
		return new ArraySegment<byte>(Byte_0, 0, Position);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator ArraySegment<byte>(EFTWriterClass w)
	{
		return w.ToArraySegment();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void method_1<[Attribute1] T>(T value) where T : struct
	{
		int num = Unsafe.SizeOf<T>();
		method_0(Position + num);
		fixed (byte* ptr = &Byte_0[Position])
		{
			Unsafe.Write(ptr, value);
		}
		Position += num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void method_2<[Attribute1] T>(T? value) where T : struct
	{
		WriteByte((byte)(value.HasValue ? 1u : 0u));
		if (value.HasValue)
		{
			method_1(value.Value);
		}
	}

	public void WriteByte(byte value)
	{
		method_1(value);
	}

	public void WriteBytes(byte[] array, int offset, int count)
	{
		method_0(Position + count);
		Array.ConstrainedCopy(array, offset, Byte_0, Position, count);
		Position += count;
	}

	public unsafe bool WriteBytes(byte* ptr, int offset, int size)
	{
		method_0(Position + size);
		fixed (byte* destination = &Byte_0[Position])
		{
			UnsafeUtility.MemCpy(destination, ptr + offset, size);
		}
		Position += size;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Write<T>(T value)
	{
		Action<EFTWriterClass, T> write = GClass1289<T>.write;
		if (write == null)
		{
			Debug.LogError($"No writer found for {typeof(T)}. This happens either if you are missing a NetworkWriter extension for your custom type, or if weaving failed. Try to reimport a script to weave again.");
		}
		else
		{
			write(this, value);
		}
	}

	public override string ToString()
	{
		return $"[{GClass1298.ToHexString(ToArraySegment())} @ {Position}/{Capacity}]";
	}
}

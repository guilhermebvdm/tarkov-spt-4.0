using System;
using System.Runtime.CompilerServices;

public struct GStruct126(long x, long y, long z)
{
	public long x = x;

	public long y = y;

	public long z = z;

	public static readonly GStruct126 zero = new GStruct126(0L, 0L, 0L);

	public static readonly GStruct126 one = new GStruct126(1L, 1L, 1L);

	public static readonly GStruct126 forward = new GStruct126(0L, 0L, 1L);

	public static readonly GStruct126 back = new GStruct126(0L, 0L, -1L);

	public static readonly GStruct126 left = new GStruct126(-1L, 0L, 0L);

	public static readonly GStruct126 right = new GStruct126(1L, 0L, 0L);

	public static readonly GStruct126 up = new GStruct126(0L, 1L, 0L);

	public static readonly GStruct126 down = new GStruct126(0L, -1L, 0L);

	public long this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return index switch
			{
				0 => x, 
				1 => y, 
				2 => z, 
				_ => throw new IndexOutOfRangeException($"Vector3Long[{index}] out of range."), 
			};
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			switch (index)
			{
			default:
				throw new IndexOutOfRangeException($"Vector3Long[{index}] out of range.");
			case 0:
				x = value;
				break;
			case 1:
				y = value;
				break;
			case 2:
				z = value;
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GStruct126 operator +(GStruct126 a, GStruct126 b)
	{
		return new GStruct126(a.x + b.x, a.y + b.y, a.z + b.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GStruct126 operator -(GStruct126 a, GStruct126 b)
	{
		return new GStruct126(a.x - b.x, a.y - b.y, a.z - b.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GStruct126 operator -(GStruct126 v)
	{
		return new GStruct126(-v.x, -v.y, -v.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GStruct126 operator *(GStruct126 a, long n)
	{
		return new GStruct126(a.x * n, a.y * n, a.z * n);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GStruct126 operator *(long n, GStruct126 a)
	{
		return new GStruct126(a.x * n, a.y * n, a.z * n);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(GStruct126 a, GStruct126 b)
	{
		if (a.x == b.x && a.y == b.y)
		{
			return a.z == b.z;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(GStruct126 a, GStruct126 b)
	{
		return !(a == b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return $"({x} {y} {z})";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(GStruct126 other)
	{
		if (x == other.x && y == other.y)
		{
			return z == other.z;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object other)
	{
		if (other is GStruct126 other2)
		{
			return Equals(other2);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return HashCode.Combine(x, y, z);
	}
}

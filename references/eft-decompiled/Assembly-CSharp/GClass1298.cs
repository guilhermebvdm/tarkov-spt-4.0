using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public abstract class GClass1298
{
	public static string ToHexString(this ArraySegment<byte> segment)
	{
		return BitConverter.ToString(segment.Array, segment.Offset, segment.Count);
	}

	public static int GetStableHashCode(this string text)
	{
		int num = 23;
		foreach (char c in text)
		{
			num = num * 31 + c;
		}
		return num;
	}

	public static string smethod_0(this Delegate func)
	{
		return func.Method.Name;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CopyTo<T>(this IEnumerable<T> source, List<T> destination)
	{
		destination.AddRange(source);
	}

	public static bool TryDequeue<T>(this Queue<T> source, out T element)
	{
		if (source.Count > 0)
		{
			element = source.Dequeue();
			return true;
		}
		element = default(T);
		return false;
	}
}

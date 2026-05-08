using System;

public abstract class GClass867
{
	public static string ToStringNoBox<T>(this T value) where T : struct, Enum
	{
		return GClass866<T>.GetName(value);
	}

	public static bool HasFlagNoBox<T>(this T value, T flag) where T : struct, Enum
	{
		return GClass866<T>.HasFlag(value, flag);
	}

	public static void AddFlag<T>(this ref T value, T flag) where T : struct, Enum
	{
		GClass866<T>.AddFlag(ref value, flag);
	}

	public static void RemoveFlag<T>(this ref T value, T flag) where T : struct, Enum
	{
		GClass866<T>.RemoveFlag(ref value, flag);
	}

	public static bool EqualsNoBox<T>(this T x, T y) where T : struct, Enum
	{
		return GClass866<T>.EqualityComparer.Equals(x, y);
	}

	public static int GetIndex<T>(this T value) where T : struct, Enum
	{
		return GClass866<T>.IndexOf(value);
	}

	public static bool IsCorrectValue<T>(this T value) where T : struct, Enum
	{
		return GClass866<T>.IndexOf(value) >= 0;
	}

	public static string LocalizationKey<T>(this T value) where T : struct, Enum
	{
		return GClass866<T>.GetLocalizationKey(value);
	}

	public static byte GetByte<T>(this T value) where T : struct, Enum
	{
		return GClass866<T>.GetByteFromValue(value);
	}

	public static int GetInt<T>(this T value) where T : struct, Enum
	{
		return GClass866<T>.GetIntFromValue(value);
	}

	public static short GetShort<T>(this T value) where T : struct, Enum
	{
		return GClass866<T>.GetShortFromValue(value);
	}

	public static long GetLong<T>(this T value) where T : struct, Enum
	{
		return GClass866<T>.GetLongFromValue(value);
	}

	public static bool Is<T>(this T enumWithFlags, T value) where T : Enum
	{
		return enumWithFlags.HasFlag(value);
	}
}

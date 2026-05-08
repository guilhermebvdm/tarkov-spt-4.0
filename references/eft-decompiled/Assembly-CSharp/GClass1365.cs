using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class GClass1365
{
	public abstract class GClass1366<[Attribute1] T> where T : struct, Enum
	{
		[Serializable]
		[CompilerGenerated]
		public class Class912
		{
			public static readonly Class912 class912_0 = new Class912();

			public long method_0(KeyValuePair<int, T> x)
			{
				return x.Key;
			}

			public T method_1(KeyValuePair<int, T> x)
			{
				return x.Value;
			}

			public long method_2(KeyValuePair<short, T> x)
			{
				return x.Key;
			}

			public T method_3(KeyValuePair<short, T> x)
			{
				return x.Value;
			}

			public long method_4(KeyValuePair<byte, T> x)
			{
				return x.Key;
			}

			public T method_5(KeyValuePair<byte, T> x)
			{
				return x.Value;
			}

			public long method_6(T @enum)
			{
				return (int)Class915<T>.ToIntCaster(@enum);
			}

			public long method_7(KeyValuePair<long, T> x)
			{
				return x.Key;
			}

			public T method_8(KeyValuePair<long, T> x)
			{
				return x.Value;
			}
		}

		public static readonly long Min;

		public static readonly long Max;

		[NonSerialized]
		public static Func<T, long> Func_0;

		[NonSerialized]
		public static Dictionary<long, T> Dictionary_0;

		static GClass1366()
		{
			Type underlyingType = Enum.GetUnderlyingType(typeof(T));
			if (underlyingType == typeof(int))
			{
				Func_0 = Class913<T>.ToIntCaster;
				Min = Class913<T>.Min;
				Max = Class913<T>.Max;
				Dictionary_0 = ((IEnumerable<KeyValuePair<int, T>>)Class913<T>.IntToEnumTable).ToDictionary((Func<KeyValuePair<int, T>, long>)((KeyValuePair<int, T> x) => x.Key), (Func<KeyValuePair<int, T>, T>)((KeyValuePair<int, T> x) => x.Value));
				return;
			}
			if (underlyingType == typeof(short))
			{
				Func_0 = Class917<T>.ToIntCaster;
				Min = Class917<T>.Min;
				Max = Class917<T>.Max;
				Dictionary_0 = ((IEnumerable<KeyValuePair<short, T>>)Class917<T>.IntToEnumTable).ToDictionary((Func<KeyValuePair<short, T>, long>)((KeyValuePair<short, T> x) => x.Key), (Func<KeyValuePair<short, T>, T>)((KeyValuePair<short, T> x) => x.Value));
				return;
			}
			if (underlyingType == typeof(byte))
			{
				Func_0 = Class919<T>.ToIntCaster;
				Min = Class919<T>.Min;
				Max = Class919<T>.Max;
				Dictionary_0 = ((IEnumerable<KeyValuePair<byte, T>>)Class919<T>.IntToEnumTable).ToDictionary((Func<KeyValuePair<byte, T>, long>)((KeyValuePair<byte, T> x) => x.Key), (Func<KeyValuePair<byte, T>, T>)((KeyValuePair<byte, T> x) => x.Value));
				return;
			}
			if (!(underlyingType == typeof(long)))
			{
				throw new Exception($"type mismatch {underlyingType}");
			}
			Func_0 = (T @enum) => (int)Class915<T>.ToIntCaster(@enum);
			Min = Class915<T>.Min;
			Max = Class915<T>.Max;
			Dictionary_0 = Class915<T>.IntToEnumTable.ToDictionary((KeyValuePair<long, T> x) => x.Key, (KeyValuePair<long, T> x) => x.Value);
		}

		public static T ToEnum(long i)
		{
			return Dictionary_0[i];
		}

		public static long ToInt(T e)
		{
			return Func_0(e);
		}
	}

	public abstract class Class913<[Attribute1] T> where T : struct, Enum
	{
		[Serializable]
		[CompilerGenerated]
		public class Class914
		{
			public static readonly Class914 class914_0 = new Class914();

			public static Func<T, int> func_0;

			public static Func<T, T> func_1;

			public int method_0(T x)
			{
				return (int)(object)x;
			}

			public T method_1(T x)
			{
				return x;
			}
		}

		public static int Min => Enum.GetValues(typeof(T)).Cast<int>().Min();

		public static int Max => Enum.GetValues(typeof(T)).Cast<int>().Max();

		public static Dictionary<int, T> IntToEnumTable => ((T[])Enum.GetValues(typeof(T))).ToDictionary((T x) => (int)(object)x, (T x) => x);

		public unsafe static long ToIntCaster(T arg)
		{
			int* ptr = (int*)(&arg);
			return *ptr;
		}
	}

	public abstract class Class915<[Attribute1] T> where T : struct, Enum
	{
		[Serializable]
		[CompilerGenerated]
		public class Class916
		{
			public static readonly Class916 class916_0 = new Class916();

			public static Func<T, long> func_0;

			public static Func<T, T> func_1;

			public long method_0(T x)
			{
				return (long)(object)x;
			}

			public T method_1(T x)
			{
				return x;
			}
		}

		public static long Min => Enum.GetValues(typeof(T)).Cast<long>().Min();

		public static long Max => Enum.GetValues(typeof(T)).Cast<long>().Max();

		public static Dictionary<long, T> IntToEnumTable => ((T[])Enum.GetValues(typeof(T))).ToDictionary((T x) => (long)(object)x, (T x) => x);

		public unsafe static long ToIntCaster(T arg)
		{
			long* ptr = (long*)(&arg);
			return *ptr;
		}
	}

	public abstract class Class917<[Attribute1] T> where T : struct, Enum
	{
		[Serializable]
		[CompilerGenerated]
		public class Class918
		{
			public static readonly Class918 class918_0 = new Class918();

			public static Func<T, short> func_0;

			public static Func<T, T> func_1;

			public short method_0(T x)
			{
				return (short)(object)x;
			}

			public T method_1(T x)
			{
				return x;
			}
		}

		public static short Min => Enum.GetValues(typeof(T)).Cast<short>().Min();

		public static short Max => Enum.GetValues(typeof(T)).Cast<short>().Max();

		public static Dictionary<short, T> IntToEnumTable => ((T[])Enum.GetValues(typeof(T))).ToDictionary((T x) => (short)(object)x, (T x) => x);

		public unsafe static long ToIntCaster(T arg)
		{
			short* ptr = (short*)(&arg);
			return *ptr;
		}
	}

	public abstract class Class919<[Attribute1] T> where T : struct, Enum
	{
		[Serializable]
		[CompilerGenerated]
		public class Class920
		{
			public static readonly Class920 class920_0 = new Class920();

			public static Func<T, byte> func_0;

			public static Func<T, T> func_1;

			public byte method_0(T x)
			{
				return (byte)(object)x;
			}

			public T method_1(T x)
			{
				return x;
			}
		}

		public static byte Min => Enum.GetValues(typeof(T)).Cast<byte>().Min();

		public static byte Max => Enum.GetValues(typeof(T)).Cast<byte>().Max();

		public static Dictionary<byte, T> IntToEnumTable => ((T[])Enum.GetValues(typeof(T))).ToDictionary((T x) => (byte)(object)x, (T x) => x);

		public unsafe static long ToIntCaster(T arg)
		{
			byte* ptr = (byte*)(&arg);
			return *ptr;
		}
	}

	public static int BitRequired(long min, long max)
	{
		if (min != max)
		{
			return (int)(Log2((uint)(max - min)) + 1);
		}
		return 0;
	}

	public static uint Log2(uint value)
	{
		uint num = value | (value >> 1);
		uint num2 = num | (num >> 2);
		uint num3 = num2 | (num2 >> 4);
		uint num4 = num3 | (num3 >> 8);
		return smethod_0((num4 | (num4 >> 16)) >> 1);
	}

	public static void CalculateDataForQuantizing(float min, float max, float res, out float delta, out int maxIntegerValue, out int bits)
	{
		delta = max - min;
		float f = delta / res;
		maxIntegerValue = Mathf.CeilToInt(f);
		bits = BitRequired(0L, maxIntegerValue);
	}

	public static float DequantizeUIntToFloat(uint integerValue, float min, int maxIntegerValue, float delta)
	{
		return (float)integerValue / (float)maxIntegerValue * delta + min;
	}

	public static uint QuantizeFloatValue(float value, float min, float delta, int maxIntegerValue)
	{
		return (uint)Mathf.FloorToInt(Mathf.Clamp((value - min) / delta, 0f, 1f) * (float)maxIntegerValue + 0.5f);
	}

	public static uint smethod_0(uint x)
	{
		uint num = x - ((x >> 1) & 0x55555555);
		uint num2 = ((num >> 2) & 0x33333333) + (num & 0x33333333);
		uint num3 = ((num2 >> 4) + num2) & 0xF0F0F0F;
		uint num4 = num3 + (num3 >> 8);
		return (num4 + (num4 >> 16)) & 0x3F;
	}
}

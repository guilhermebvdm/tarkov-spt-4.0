using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

public abstract class GClass866<T> where T : struct, Enum
{
	public class Class488 : IComparer<T>
	{
		public int Compare(T x, T y)
		{
			return GClass866<T>.GetLongFromValue(x).CompareTo(GClass866<T>.GetLongFromValue(y));
		}
	}

	public class Class489 : IEqualityComparer<T>
	{
		public bool Equals(T x, T y)
		{
			return GClass866<T>.GetLongFromValue(x) == GClass866<T>.GetLongFromValue(y);
		}

		public int GetHashCode(T obj)
		{
			return GClass866<T>.GetLongFromValue(obj).GetHashCode();
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class490
	{
		public static readonly Class490 class490_0 = new Class490();

		public static Func<string, string> func_0;

		public static Func<string, string> func_1;

		public string method_0(string originalName)
		{
			GAttribute24 gAttribute = GClass866<T>.smethod_0<GAttribute24>(originalName);
			object obj;
			if (gAttribute == null)
			{
				obj = null;
			}
			else
			{
				obj = gAttribute.Name;
				if (obj != null)
				{
					goto IL_0017;
				}
			}
			obj = originalName;
			goto IL_0017;
			IL_0017:
			return (string)obj;
		}

		public string method_1(string name)
		{
			return GClass866<T>.Type.Name + "/" + name;
		}
	}

	public static readonly Type Type;

	public static readonly ReadOnlyCollection<T> Values;

	public static readonly ReadOnlyCollection<string> Names;

	public static readonly int Count;

	public static readonly TypeCode TypeCode;

	public static readonly IComparer<T> Comparer;

	public static readonly IEqualityComparer<T> EqualityComparer;

	[NonSerialized]
	public static string[] String_0_1;

	[NonSerialized]
	public static string[] String_1;

	[NonSerialized]
	public static Dictionary<string, T> Dictionary_0;

	[NonSerialized]
	public static Dictionary<T, string> Dictionary_1;

	[NonSerialized]
	public static Func<T, byte> Func_0;

	[NonSerialized]
	public static Func<T, short> Func_1;

	[NonSerialized]
	public static Func<T, int> Func_2;

	[NonSerialized]
	public static Func<T, long> Func_3;

	public static readonly int ByteLength;

	[NonSerialized]
	public static long Long_0;

	public static string[] String_0 => String_1 ?? (String_1 = Names.Select(delegate(string originalName)
	{
		GAttribute24 gAttribute = smethod_0<GAttribute24>(originalName);
		object obj;
		if (gAttribute == null)
		{
			obj = null;
		}
		else
		{
			obj = gAttribute.Name;
			if (obj != null)
			{
				goto IL_0017;
			}
		}
		obj = originalName;
		goto IL_0017;
		IL_0017:
		return (string)obj;
	}).ToArray());

	public static IReadOnlyDictionary<string, T> IReadOnlyDictionary_0
	{
		get
		{
			if (Dictionary_0 == null)
			{
				smethod_1();
			}
			return Dictionary_0;
		}
	}

	public static IReadOnlyDictionary<T, string> IReadOnlyDictionary_1
	{
		get
		{
			if (Dictionary_1 == null)
			{
				smethod_2();
			}
			return Dictionary_1;
		}
	}

	static GClass866()
	{
		Type = typeof(T);
		Values = Array.AsReadOnly((T[])Enum.GetValues(typeof(T)));
		Names = Array.AsReadOnly(Enum.GetNames(typeof(T)));
		Count = Values.Count;
		TypeCode = Convert.GetTypeCode(default(T));
		Comparer = new Class488();
		EqualityComparer = new Class489();
		long num = 0L;
		foreach (T value in Values)
		{
			long num2 = Convert.ToInt64(value);
			if (num2 < Long_0)
			{
				Long_0 = num2;
			}
			if (num2 > num)
			{
				num = num2;
			}
		}
		num -= Long_0;
		for (int i = 0; i < 8; i++)
		{
			ByteLength++;
			num >>= 8;
			if (num == 0L)
			{
				break;
			}
		}
		switch (TypeCode)
		{
		case TypeCode.Byte:
			Func_0 = smethod_5<byte>();
			break;
		case TypeCode.Int16:
			Func_1 = smethod_5<short>();
			break;
		case TypeCode.Int32:
			Func_2 = smethod_5<int>();
			break;
		default:
			throw smethod_4();
		case TypeCode.Int64:
			Func_3 = smethod_5<long>();
			break;
		}
	}

	public static long ToSerializationValue(T enumValue)
	{
		return Unsafe.As<T, long>(ref enumValue) - Long_0;
	}

	public static T FromSerializationValue(long encryptedValue)
	{
		encryptedValue += Long_0;
		return Unsafe.As<long, T>(ref encryptedValue);
	}

	public static int IndexOf(T value)
	{
		int num = 0;
		while (true)
		{
			if (num < Count)
			{
				if (EqualityComparer.Equals(value, Values[num]))
				{
					break;
				}
				num++;
				continue;
			}
			return -1;
		}
		return num;
	}

	public static byte GetByteFromValue(T value)
	{
		return TypeCode switch
		{
			TypeCode.Byte => Func_0(value), 
			TypeCode.Int16 => (byte)Func_1(value), 
			TypeCode.Int32 => (byte)Func_2(value), 
			TypeCode.Int64 => (byte)Func_3(value), 
			_ => throw smethod_4(), 
		};
	}

	public static short GetShortFromValue(T value)
	{
		return TypeCode switch
		{
			TypeCode.Byte => Func_0(value), 
			TypeCode.Int16 => Func_1(value), 
			TypeCode.Int32 => (short)Func_2(value), 
			TypeCode.Int64 => (short)Func_3(value), 
			_ => throw smethod_4(), 
		};
	}

	public static int GetIntFromValue(T value)
	{
		return TypeCode switch
		{
			TypeCode.Byte => Func_0(value), 
			TypeCode.Int16 => Func_1(value), 
			TypeCode.Int32 => Func_2(value), 
			TypeCode.Int64 => (int)Func_3(value), 
			_ => throw smethod_4(), 
		};
	}

	public static long GetLongFromValue(T value)
	{
		return TypeCode switch
		{
			TypeCode.Byte => Func_0(value), 
			TypeCode.Int16 => Func_1(value), 
			TypeCode.Int32 => Func_2(value), 
			TypeCode.Int64 => Func_3(value), 
			_ => throw smethod_4(), 
		};
	}

	public static T GetValue(int integer)
	{
		return (T)(object)integer;
	}

	public static Dictionary<T, TValue> GetDictWith<TValue>()
	{
		return new Dictionary<T, TValue>(Count, EqualityComparer);
	}

	public static bool TryDeserializeValue(string name, out T value)
	{
		if (IReadOnlyDictionary_0.TryGetValue(name, out value))
		{
			return true;
		}
		int num = 0;
		while (true)
		{
			if (num < Count)
			{
				if (Names[num] == name)
				{
					break;
				}
				num++;
				continue;
			}
			Debug.LogError("Enum \"" + typeof(T).Name + "\" does not contain any value named \"" + name + "\".");
			value = default(T);
			return false;
		}
		value = Values[num];
		return true;
	}

	public static string GetName(T value)
	{
		int num = IndexOf(value);
		if (num >= 0)
		{
			return Names[num];
		}
		return value.ToString();
	}

	public static string GetLocalizationKey(T value)
	{
		smethod_3();
		int num = IndexOf(value);
		if (num >= 0)
		{
			return String_0_1[num];
		}
		string text = value.ToString();
		Debug.LogError("Attempt to localize undefined " + Type.Name + " " + text);
		return text;
	}

	public static IReadOnlyList<string> GetLocalizedNames()
	{
		smethod_3();
		string[] array = new string[Count];
		for (int i = 0; i < String_0_1.Length; i++)
		{
			array[i] = GClass2348.Localized(String_0_1[i]);
		}
		return array;
	}

	public static string SerializeValue(T value)
	{
		return IReadOnlyDictionary_1[value];
	}

	public static bool HasFlag(T value, T flag)
	{
		long longFromValue = GetLongFromValue(value);
		long longFromValue2 = GetLongFromValue(flag);
		return (longFromValue & longFromValue2) == longFromValue2;
	}

	public static void AddFlag(ref T currentFlags, T flagToAdd)
	{
		int intFromValue = GetIntFromValue(currentFlags);
		int intFromValue2 = GetIntFromValue(flagToAdd);
		currentFlags = GetValue(intFromValue | intFromValue2);
	}

	public static void RemoveFlag(ref T currentFlags, T flagToRemove)
	{
		int intFromValue = GetIntFromValue(currentFlags);
		int intFromValue2 = GetIntFromValue(flagToRemove);
		currentFlags = GetValue(intFromValue & ~intFromValue2);
	}

	public static bool IsDefined(T value)
	{
		foreach (T value2 in Values)
		{
			if (EqualityComparer.Equals(value, value2))
			{
				return true;
			}
		}
		return false;
	}

	[CanBeNull]
	public static U smethod_0<U>([NotNull] string name)
	{
		return (U)Type.GetMember(name).First().GetCustomAttributes(typeof(U), inherit: false)
			.FirstOrDefault();
	}

	public static void smethod_1()
	{
		int i = 0;
		Dictionary_0 = new Dictionary<string, T>(Count);
		try
		{
			for (i = 0; i < Count; i++)
			{
				Dictionary_0.Add(String_0[i], Values[i]);
			}
		}
		catch
		{
			throw new Exception("Enum \"" + typeof(T).Name + "\"(" + String_0[i] + ") has equal serialization names.");
		}
	}

	public static void smethod_2()
	{
		int i = 0;
		Dictionary_1 = GetDictWith<string>();
		try
		{
			for (i = 0; i < Count; i++)
			{
				Dictionary_1.Add(Values[i], String_0[i]);
			}
		}
		catch
		{
			throw new Exception("Enum \"" + typeof(T).Name + "\"(" + Names[i] + ") has values with equal hashes.");
		}
	}

	public static void smethod_3()
	{
		if (String_0_1 == null)
		{
			String_0_1 = Names.Select((string name) => Type.Name + "/" + name).ToArray();
		}
	}

	public static ArgumentException smethod_4()
	{
		return new ArgumentException("EnumHelper can work correct only with Byte/Int16/Int32-based enums, " + $"{Type.Name} is based on {TypeCode}");
	}

	public static bool TryGetValueByName(string name, out T value)
	{
		return IReadOnlyDictionary_0.TryGetValue(name, out value);
	}

	[CompilerGenerated]
	public static Func<T, U> smethod_5<U>()
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(T), null);
		return Expression.Lambda<Func<T, U>>(Expression.ConvertChecked(parameterExpression, typeof(U)), new ParameterExpression[1] { parameterExpression }).Compile();
	}
}

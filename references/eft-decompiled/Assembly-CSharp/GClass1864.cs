using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;

public class GClass1864<T> : List<string>, ISerializer<T>
{
	[Serializable]
	[CompilerGenerated]
	public class Class1148
	{
		public static readonly Class1148 class1148_0 = new Class1148();

		public static Func<int, string, int> func_0;

		public static Func<int, T> func_1;

		public static Func<T, string> func_2;

		public int method_0(int acc, string side)
		{
			return Convert.ToInt32(Enum.Parse(typeof(T), side)) | acc;
		}

		public T method_1(int acc)
		{
			return (T)(object)acc;
		}

		public string method_2(T x)
		{
			return x.ToString();
		}
	}

	[CompilerGenerated]
	public class Class1149
	{
		public int intValue;

		public bool method_0(T x)
		{
			return (Convert.ToInt32(x) & intValue) != 0;
		}
	}

	public T Deserialize()
	{
		return this.Aggregate(0, (int acc, string side) => Convert.ToInt32(Enum.Parse(typeof(T), side)) | acc, (int acc) => (T)(object)acc);
	}

	public object Serialize(T t)
	{
		int intValue = Convert.ToInt32(t);
		return (from x in (T[])Enum.GetValues(typeof(T))
			where (Convert.ToInt32(x) & intValue) != 0
			select x.ToString()).ToList();
	}
}

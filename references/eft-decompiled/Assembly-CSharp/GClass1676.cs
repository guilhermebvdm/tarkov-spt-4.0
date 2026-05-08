using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass1676
{
	public delegate T Delegate6<out T>(string stringValue);

	[Serializable]
	[CompilerGenerated]
	public class Class1090
	{
		public static readonly Class1090 class1090_0 = new Class1090();

		public static Delegate6<string> delegate6_0;

		public static Delegate6<string> delegate6_1;

		public string method_0(string stringValue)
		{
			return stringValue;
		}

		public string method_1(string stringValue)
		{
			return stringValue;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1091<TEnum> where TEnum : struct
	{
		public static readonly Class1091<TEnum> class1091_0 = new Class1091<TEnum>();

		public static Delegate6<TEnum> delegate6_0;

		public TEnum method_0(string value)
		{
			return (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase: true);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1092<TEnum> where TEnum : struct
	{
		public static readonly Class1092<TEnum> class1092_0 = new Class1092<TEnum>();

		public static Delegate6<TEnum> delegate6_0;

		public TEnum method_0(string value)
		{
			return (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase: true);
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public string String_0_1;

	[NonSerialized]
	[CompilerGenerated]
	public string[] String_1_1;

	[NonSerialized]
	public Dictionary<string, string> Dictionary_0;

	public string String_0
	{
		[CompilerGenerated]
		get
		{
			return String_0_1;
		}
		[CompilerGenerated]
		set
		{
			String_0_1 = value;
		}
	}

	public string[] String_1
	{
		[CompilerGenerated]
		get
		{
			return String_1_1;
		}
		[CompilerGenerated]
		set
		{
			String_1_1 = value;
		}
	}

	public IReadOnlyDictionary<string, string> ParsedArgs => Dictionary_0;

	public static GClass1676 Create()
	{
		GClass1676 gClass = new GClass1676
		{
			String_0 = Environment.CommandLine,
			String_1 = Environment.GetCommandLineArgs(),
			Dictionary_0 = new Dictionary<string, string>()
		};
		string[] string_ = gClass.String_1;
		foreach (string text in string_)
		{
			if (text.StartsWith("-") && text.Contains("="))
			{
				int num = text.IndexOf('=');
				string text2 = text.Substring(1, num - 1);
				string text3 = text.Substring(num + 1);
				gClass.Dictionary_0.Add(text2, text3);
				Debug.Log("key:" + text2 + " value:" + text3);
			}
			else
			{
				Debug.Log("Skipped command line argument:" + text);
			}
		}
		return gClass;
	}

	public string GetString(string argumentName)
	{
		return method_11(argumentName, (string stringValue) => stringValue);
	}

	public string GetString(string argumentName, string defaultValue)
	{
		return method_10(argumentName, (string stringValue) => stringValue, defaultValue);
	}

	public int method_0(string argumentName)
	{
		return method_9(argumentName, int.Parse);
	}

	public int method_1(string argumentName, int defaultValue)
	{
		return method_9(argumentName, int.Parse, defaultValue);
	}

	public ushort method_2(string argumentName)
	{
		return method_9(argumentName, ushort.Parse);
	}

	public ushort method_3(string argumentName, ushort defaultValue)
	{
		return method_9(argumentName, ushort.Parse, defaultValue);
	}

	public bool method_4(string argumentName)
	{
		return method_9(argumentName, bool.Parse);
	}

	public bool method_5(string argumentName, bool defaultValue)
	{
		return method_9(argumentName, bool.Parse, defaultValue);
	}

	public T method_6<T>(string argumentName) where T : struct
	{
		if (!typeof(T).IsEnum)
		{
			throw new Exception("Not enum type passed to GetEnumCustomArgument function");
		}
		return method_9(argumentName, (string value) => (T)Enum.Parse(typeof(T), value, ignoreCase: true));
	}

	public T method_7<T>(string argumentName, T defaultValue) where T : struct
	{
		if (!typeof(T).IsEnum)
		{
			throw new Exception("Not enum type passed to GetEnumCustomArgument function");
		}
		return method_9(argumentName, (string value) => (T)Enum.Parse(typeof(T), value, ignoreCase: true), defaultValue);
	}

	public bool method_8(string argumentName, out string value)
	{
		if (Dictionary_0.ContainsKey(argumentName))
		{
			value = Dictionary_0[argumentName];
			return true;
		}
		value = null;
		return false;
	}

	public T method_9<T>(string argumentName, Delegate6<T> convertDelegate, T? defaultValue = null) where T : struct
	{
		if (method_8(argumentName, out var value))
		{
			return convertDelegate(value);
		}
		if (!defaultValue.HasValue)
		{
			throw new Exception("CommandLineReader - Can't retrieve any custom argument named [" + argumentName + "] in the command line [" + String_0 + "].");
		}
		return defaultValue.Value;
	}

	public T method_10<T>(string argumentName, Delegate6<T> convertDelegate, T defaultValue) where T : class
	{
		if (method_8(argumentName, out var value))
		{
			return convertDelegate(value);
		}
		return defaultValue;
	}

	public T method_11<T>(string argumentName, Delegate6<T> convertDelegate) where T : class
	{
		if (!method_8(argumentName, out var value))
		{
			throw new Exception("CommandLineReader - Can't retrieve any custom argument named [" + argumentName + "] in the command line [" + String_0 + "].");
		}
		return convertDelegate(value);
	}
}

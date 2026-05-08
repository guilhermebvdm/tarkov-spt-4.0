using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass9
{
	public delegate T Delegate0<T>(string stringValue);

	[Serializable]
	[CompilerGenerated]
	public class Class93
	{
		public static readonly Class93 class93_0 = new Class93();

		public static Delegate0<string> delegate0_0;

		public static Delegate0<string> delegate0_1;

		public static Func<string, bool> func_0;

		public static Func<string, bool> func_1;

		public string method_0(string stringValue)
		{
			return stringValue;
		}

		public string method_1(string stringValue)
		{
			return stringValue;
		}

		public bool method_2(string str)
		{
			return str.Contains("-CustomArgs:");
		}

		public bool method_3(string row)
		{
			return row.Contains("-CustomArgs:");
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class94<TEnum> where TEnum : struct
	{
		public static readonly Class94<TEnum> class94_0 = new Class94<TEnum>();

		public static Delegate0<TEnum> delegate0_0;

		public TEnum method_0(string value)
		{
			return (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase: true);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class95<TEnum> where TEnum : struct
	{
		public static readonly Class95<TEnum> class95_0 = new Class95<TEnum>();

		public static Delegate0<TEnum> delegate0_0;

		public TEnum method_0(string value)
		{
			return (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase: true);
		}
	}

	[NonSerialized]
	public const string String_0 = "-CustomArgs:";

	[NonSerialized]
	public static char[] Char_0 = new char[1] { ';' };

	[NonSerialized]
	[CompilerGenerated]
	public static Dictionary<string, string> Dictionary_0 = smethod_4();

	public static Dictionary<string, string> CustomArguments
	{
		[CompilerGenerated]
		get
		{
			return Dictionary_0;
		}
	}

	public static string[] GetCommandLineArgs()
	{
		return Environment.GetCommandLineArgs();
	}

	public static string GetCommandLine()
	{
		string[] commandLineArgs = GetCommandLineArgs();
		if (commandLineArgs.Length != 0)
		{
			return string.Join(" ", commandLineArgs);
		}
		Debug.LogError("CommandLineReader.cs - GetCommandLine() - Can't find any command line arguments!");
		return "";
	}

	public static string GetString(string argumentName)
	{
		return smethod_3(argumentName, (string stringValue) => stringValue);
	}

	public static string GetString(string argumentName, string defaultValue)
	{
		return smethod_2(argumentName, (string stringValue) => stringValue, defaultValue);
	}

	public static int GetInt(string argumentName)
	{
		return smethod_1(argumentName, int.Parse);
	}

	public static int GetInt(string argumentName, int defaultValue)
	{
		return smethod_1(argumentName, int.Parse, defaultValue);
	}

	public static bool GetBool(string argumentName)
	{
		return smethod_1(argumentName, bool.Parse);
	}

	public static bool GetBool(string argumentName, bool defaultValue)
	{
		return smethod_1(argumentName, bool.Parse, defaultValue);
	}

	public static TEnum GetEnum<TEnum>(string argumentName) where TEnum : struct
	{
		if (!typeof(TEnum).IsEnum)
		{
			throw new Exception("Not enum type passed to GetEnumCustomArgument function");
		}
		return smethod_1(argumentName, (string value) => (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase: true));
	}

	public static TEnum GetEnum<TEnum>(string argumentName, TEnum defaultValue) where TEnum : struct
	{
		if (!typeof(TEnum).IsEnum)
		{
			throw new Exception("Not enum type passed to GetEnumCustomArgument function");
		}
		return smethod_1(argumentName, (string value) => (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase: true), defaultValue);
	}

	public static bool smethod_0(string argumentName, out string value)
	{
		return CustomArguments.TryGetValue(argumentName, out value);
	}

	public static T smethod_1<T>(string argumentName, Delegate0<T> convertDelegate, T? defaultValue = null) where T : struct
	{
		if (smethod_0(argumentName, out var value))
		{
			return convertDelegate(value);
		}
		if (!defaultValue.HasValue)
		{
			throw new Exception("CommandLineReader - Can't retrieve any custom argument named [" + argumentName + "] in the command line [" + GetCommandLine() + "].");
		}
		return defaultValue.Value;
	}

	public static T smethod_2<T>(string argumentName, Delegate0<T> convertDelegate, T defaultValue) where T : class
	{
		if (smethod_0(argumentName, out var value))
		{
			return convertDelegate(value);
		}
		return defaultValue;
	}

	public static T smethod_3<T>(string argumentName, Delegate0<T> convertDelegate) where T : class
	{
		if (!smethod_0(argumentName, out var value))
		{
			throw new Exception("CommandLineReader - Can't retrieve any custom argument named [" + argumentName + "] in the command line [" + GetCommandLine() + "].");
		}
		return convertDelegate(value);
	}

	public static Dictionary<string, string> smethod_4()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] commandLineArgs = GetCommandLineArgs();
		if (!commandLineArgs.Any((string str) => str.Contains("-CustomArgs:")))
		{
			return dictionary;
		}
		string text;
		try
		{
			text = commandLineArgs.Single((string row) => row.Contains("-CustomArgs:"));
		}
		catch (Exception arg)
		{
			Debug.LogError($"CommandLineReader.cs - GetCustomArguments() - Can't retrieve any custom arguments in the command line [{commandLineArgs}]. Exception: {arg}");
			return dictionary;
		}
		string[] array = text.Replace("-CustomArgs:", string.Empty).Split(Char_0, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			Debug.LogFormat("custom arg string '{0}'", text2);
		}
		array2 = array;
		foreach (string text3 in array2)
		{
			string[] array3 = text3.Split('=');
			if (array3.Length == 2)
			{
				dictionary.Add(array3[0], array3[1]);
			}
			else
			{
				Debug.LogWarning("CommandLineReader.cs - GetCustomArguments() - The custom argument [" + text3 + "] seem to be malformed.");
			}
		}
		return dictionary;
	}
}

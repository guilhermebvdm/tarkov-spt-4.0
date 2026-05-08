using System;
using System.Collections.Generic;
using System.Linq;

public abstract class GClass815
{
	[NonSerialized]
	public const string String_0 = "<color=#{0}><b>!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!</b></color> <b>{1}:</b> ";

	[NonSerialized]
	public static Dictionary<string, string> Dictionary_0 = new Dictionary<string, string>();

	[NonSerialized]
	public const string String_1 = "default";

	[NonSerialized]
	public static List<int> List_0 = new List<int>();

	[NonSerialized]
	public static string[] String_2 = new string[56]
	{
		"FF0000", "00FF00", "0000FF", "FFFF00", "FF00FF", "00FFFF", "000000", "800000", "008000", "000080",
		"808000", "800080", "008080", "808080", "C00000", "00C000", "0000C0", "C0C000", "C000C0", "00C0C0",
		"C0C0C0", "400000", "004000", "000040", "404000", "400040", "004040", "404040", "200000", "002000",
		"000020", "202000", "200020", "002020", "202020", "600000", "006000", "000060", "606000", "600060",
		"006060", "606060", "A00000", "00A000", "0000A0", "A0A000", "A000A0", "00A0A0", "A0A0A0", "E00000",
		"00E000", "0000E0", "E0E000", "E000E0", "00E0E0", "E0E0E0"
	};

	public static void smethod_0(string name)
	{
		if (List_0.Count == 0)
		{
			for (int i = 0; i < String_2.Length; i++)
			{
				List_0.Add(i);
			}
			List_0 = GClass856.Randomize(List_0).ToList();
		}
		int num = List_0[List_0.Count - 1];
		List_0.RemoveAt(List_0.Count - 1);
		string arg = String_2[num];
		Dictionary_0[name] = $"<color=#{arg}><b>!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!</b></color> <b>{name}:</b> ";
	}

	public static string smethod_1(string name)
	{
		if (!Dictionary_0.ContainsKey(name))
		{
			smethod_0(name);
		}
		return Dictionary_0[name];
	}

	public static void NLogFormat(string name, string format, params object[] args)
	{
	}

	public static void NLog(string name, object message)
	{
	}

	public static void LogFormat(string format, params object[] args)
	{
	}

	public static void Log(object message)
	{
	}
}

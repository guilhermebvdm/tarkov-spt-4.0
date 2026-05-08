using System;
using System.Text;

public abstract class GClass839
{
	[NonSerialized]
	public const int Int_0 = 1000;

	[NonSerialized]
	public static string String_0;

	[NonSerialized]
	public static string[] String_1;

	static GClass839()
	{
		String_0 = "-";
		String_1 = new string[1001];
		for (int i = 0; i < String_1.Length; i++)
		{
			String_1[i] = i.ToString();
		}
	}

	public static void AppendDigitalValue(this StringBuilder stringBuilder, float f)
	{
		if (f < 0f)
		{
			stringBuilder.Append(String_0);
			f = Math.Abs(f);
		}
		int num = (int)Math.Truncate(f);
		int num2 = (int)((f - (float)num) * 1000f);
		if (num >= String_1.Length)
		{
			num = String_1.Length - 2;
		}
		if (num2 >= String_1.Length)
		{
			num2 = String_1.Length - 2;
		}
		stringBuilder.Append(String_1[num + 1]);
		stringBuilder.Append('.');
		stringBuilder.Append(String_1[num2 + 1]);
	}

	public static void AppendDigitalValue(this StringBuilder stringBuilder, double f)
	{
		if (f < 0.0)
		{
			stringBuilder.Append(String_0);
			f = Math.Abs(f);
		}
		int num = (int)Math.Truncate(f);
		int num2 = (int)((f - (double)num) * 1000.0);
		if (num >= String_1.Length)
		{
			num = String_1.Length - 2;
		}
		if (num2 >= String_1.Length)
		{
			num2 = String_1.Length - 2;
		}
		stringBuilder.Append(String_1[num]);
		stringBuilder.Append('.');
		stringBuilder.Append(String_1[num2]);
	}

	public static void AppendDigitalValue(this StringBuilder stringBuilder, int @int, int charsCount = -1)
	{
		if (@int < 0)
		{
			stringBuilder.Append(String_0);
			@int = Math.Abs(@int);
		}
		if (@int >= String_1.Length)
		{
			@int = String_1.Length - 2;
		}
		string text = String_1[@int];
		if (charsCount < 0)
		{
			stringBuilder.Append(text);
			return;
		}
		int num = charsCount - text.Length;
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append(String_1[0]);
		}
		stringBuilder.Append(text);
	}
}

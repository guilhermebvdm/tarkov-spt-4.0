using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class GAttribute11 : PropertyAttribute
{
	[Serializable]
	[CompilerGenerated]
	public class Class592
	{
		public static readonly Class592 class592_0 = new Class592();

		public static Func<char, bool> func_0;

		public bool method_0(char c)
		{
			return c == '\n';
		}
	}

	public readonly string Text;

	public readonly float Height;

	public GAttribute11(string text)
	{
		Text = text;
		int num = text.Count((char c) => c == '\n') + 1;
		Height = 7f + (float)num * 11f;
		Height = Math.Max(Height, 38f);
	}
}

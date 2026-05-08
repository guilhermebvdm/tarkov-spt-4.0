using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public abstract class GClass1673
{
	[Serializable]
	[CompilerGenerated]
	public class Class1089
	{
		public static readonly Class1089 class1089_0 = new Class1089();

		public static Func<string, string> func_0;

		public string method_0(string part)
		{
			if (part.Length != 0)
			{
				return char.ToUpper(part[0]) + ((part.Length > 1) ? part.Substring(1).ToLowerInvariant() : "");
			}
			return "";
		}
	}

	[NonSerialized]
	public const string String_0 = "0123456789";

	[NonSerialized]
	public static Dictionary<TMP_FontAsset, float> Dictionary_0 = new Dictionary<TMP_FontAsset, float>();

	[NonSerialized]
	public static Regex Regex_0 = new Regex("(\\D*)(\\d+)(\\D*)");

	public static bool IsNullOrEmpty(this string @string)
	{
		return string.IsNullOrEmpty(@string);
	}

	public static string ReplaceFirst(this string text, string search, string replace)
	{
		int num = text.IndexOf(search, StringComparison.Ordinal);
		if (num < 0)
		{
			return text;
		}
		return text.Substring(0, num) + replace + text.Substring(num + search.Length);
	}

	public static string Yellow(this string input)
	{
		return "<color=yellow>" + input + "</color>";
	}

	public static string Red(this string input)
	{
		return "<color=red>" + input + "</color>";
	}

	public static string Green(this string input)
	{
		return "<color=green>" + input + "</color>";
	}

	public static string Blue(this string input)
	{
		return "<color=blue>" + input + "</color>";
	}

	public static void CopyToClipboard(this string s)
	{
		GUIUtility.systemCopyBuffer = s;
	}

	public static void SetMonospaceText(this TMP_Text textField, string text, bool onlyDigits = true)
	{
		if (string.IsNullOrEmpty(text))
		{
			textField.text = text;
			return;
		}
		TMP_TextInfo textInfo = textField.GetTextInfo(text);
		TMP_CharacterInfo[] characterInfo = textInfo.characterInfo;
		StringBuilder stringBuilder = new StringBuilder(text);
		bool flag = false;
		float num = 0f;
		int num2 = 0;
		for (int num3 = textInfo.characterCount - 1; num3 >= 0; num3--)
		{
			TMP_CharacterInfo tMP_CharacterInfo = characterInfo[num3];
			if (tMP_CharacterInfo.isVisible)
			{
				if (onlyDigits && !char.IsDigit(tMP_CharacterInfo.character))
				{
					if (flag)
					{
						stringBuilder.Insert(num2, $"<mspace={num:F2}em>");
						flag = false;
					}
				}
				else
				{
					TMP_FontAsset fontAsset = tMP_CharacterInfo.fontAsset;
					if (!Dictionary_0.TryGetValue(fontAsset, out var value))
					{
						value = smethod_0(fontAsset);
					}
					if ((tMP_CharacterInfo.style & FontStyles.Bold) != FontStyles.Normal)
					{
						value += fontAsset.boldSpacing / 100f;
					}
					num2 = tMP_CharacterInfo.index;
					if (!flag)
					{
						stringBuilder.Insert(num2 + 1, "</mspace>");
						num = value;
						flag = true;
					}
					else if (!GClass855.ApproxEquals(num, value))
					{
						stringBuilder.Insert(num2 + 1, $"</mspace><mspace={num:F2}em>");
						num = value;
					}
				}
			}
		}
		if (flag)
		{
			stringBuilder.Insert(0, $"<mspace={num:F2}em>");
		}
		text = stringBuilder.ToString();
		textField.GetTextInfo(text);
		textField.text = text;
	}

	public static float smethod_0(TMP_FontAsset fontAsset)
	{
		float num = 0f;
		string text = "0123456789";
		foreach (char key in text)
		{
			if (fontAsset.characterLookupTable.TryGetValue(key, out var value))
			{
				float val = value.glyph.metrics.width / (float)fontAsset.faceInfo.pointSize;
				num = Math.Max(num, val);
			}
		}
		Dictionary_0.Add(fontAsset, num);
		return num;
	}

	public static string ConvertToMonospace(this string text, bool onlyDigits = true)
	{
		if (!onlyDigits)
		{
			return "<mspace=0.6em>" + text + "</mspace>";
		}
		return Regex_0.Replace(text, "$1<mspace=0.6em>$2</mspace>$3");
	}

	public static string ToEscapedLiteral(this string input)
	{
		using StringWriter stringWriter = new StringWriter();
		using CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
		codeDomProvider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), stringWriter, null);
		return stringWriter.ToString();
	}

	public static bool EqualsAndNotNull(this string s1, string s2)
	{
		if (!string.IsNullOrEmpty(s1))
		{
			return s1.Equals(s2);
		}
		return false;
	}

	public static string ConvertToResourceLocation(this string s)
	{
		return new StringBuilder(s).Replace("/files", "UI").Replace("/achievement/", "/Icons/achievements/").Replace("/handbook/", "/Icons/handbook/")
			.Replace("/Hideout/", "/Icons/Hideout/")
			.Replace("/CONTENT/banners/", "/Banners/")
			.Replace(".png", "")
			.Replace(".jpg", "")
			.ToString();
	}

	public static string SetColor(this string s, Color color)
	{
		return "<color=" + GClass819.GetRichTextColor(color) + ">" + s + "</color>";
	}

	public static void Append(this StringBuilder builder, string text, Color color)
	{
		builder.AppendFormat("<color={0}>{1}</color>", GClass819.GetRichTextColor(color), text);
	}

	public static void AppendLine(this StringBuilder builder, string text, Color color)
	{
		builder.AppendFormat("<color={0}>{1}</color>\n", GClass819.GetRichTextColor(color), text);
	}

	public static void AppendLabeledValue(this StringBuilder builder, string label, string data, Color labelColor, Color dataColor, bool labelEnabled = true)
	{
		AppendLabeledValue(builder, label, data, GClass819.GetRichTextColor(labelColor), GClass819.GetRichTextColor(dataColor), labelEnabled);
	}

	public static void AppendLabeledValue(this StringBuilder builder, string label, string data, string labelColor, Color dataColor, bool labelEnabled = true)
	{
		AppendLabeledValue(builder, label, data, labelColor, GClass819.GetRichTextColor(dataColor), labelEnabled);
	}

	public static void AppendLabeledValue(this StringBuilder builder, string label, string data, Color labelColor, string dataColor, bool labelEnabled = true)
	{
		AppendLabeledValue(builder, label, data, GClass819.GetRichTextColor(labelColor), dataColor, labelEnabled);
	}

	public static void AppendLabeledValue(this StringBuilder builder, string label, string data, string labelColor, string dataColor, bool labelEnabled = true)
	{
		if (labelEnabled)
		{
			builder.AppendFormat("<color={0}>{1}:</color>", labelColor, label);
		}
		builder.AppendFormat("<color={0}>{1}</color>\n", dataColor, data);
	}

	public static string ToStringReduceThousands(this int num)
	{
		if (num >= 100000)
		{
			return num / 1000 + "K";
		}
		return num.ToString();
	}

	public static string Capitalize(this string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		input = input.ToLower();
		return $"{char.ToUpper(input[0])}{input.Substring(1)}";
	}

	public static string ToPascalCase(this string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return string.Join("", from part in input.Split(new char[1] { '_' }, StringSplitOptions.RemoveEmptyEntries)
			select (part.Length != 0) ? (char.ToUpper(part[0]) + ((part.Length > 1) ? part.Substring(1).ToLowerInvariant() : "")) : "");
	}

	public static bool IsPunctuation(char character)
	{
		if (character != '.' && character != ',' && character != '!' && character != '?' && character != ':')
		{
			return character == ';';
		}
		return true;
	}

	public static string SplitByCapitalLetters(this string input)
	{
		return Regex.Replace(input, "(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ");
	}
}

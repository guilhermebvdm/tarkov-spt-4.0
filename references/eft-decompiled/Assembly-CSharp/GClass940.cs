using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class GClass940
{
	public enum HintAlignment
	{
		Left,
		Rigth
	}

	public enum Languages
	{
		Any,
		En,
		Ru
	}

	public static string SeparateNumericString(string str)
	{
		string text = "";
		int num = 0;
		if (str.Length % 3 == 0)
		{
			num = 3;
		}
		else if (str.Length % 3 == 2)
		{
			num = 2;
		}
		else if (str.Length % 3 == 1)
		{
			num = 1;
		}
		int num2 = 0;
		while (num2 < str.Length)
		{
			if (num2 == 0 && str.Length % 3 != 0)
			{
				text = text + str.Substring(num2, num) + " ";
				num2 += num;
			}
			else
			{
				text = text + str.Substring(num2, 3) + " ";
				num2 += 3;
			}
		}
		return text;
	}

	public static string KFormat(int value)
	{
		if (value < 1000)
		{
			return value.ToString();
		}
		return value / 1000 + "k";
	}

	public static string GetTag(string tag)
	{
		if (string.IsNullOrEmpty(tag))
		{
			return string.Empty;
		}
		if (tag.Length == 0)
		{
			return string.Empty;
		}
		if (tag[0] == '[')
		{
			return tag.Substring(9);
		}
		return tag;
	}

	public static Color GetTagColor(string tag)
	{
		try
		{
			string text = tag.Substring(2, 6);
			float num = Convert.ToInt32(text.Substring(0, 2), 16);
			float num2 = Convert.ToInt32(text.Substring(2, 2), 16);
			float num3 = Convert.ToInt32(text.Substring(4, 2), 16);
			return new Color(num / 256f, num2 / 256f, num3 / 256f, 1f);
		}
		catch (Exception exception)
		{
			Debug.Log("Something wrong in Helpers.GetTagColor().");
			Debug.LogException(exception);
		}
		return default(Color);
	}

	public static string GetTagHexColor(string tag)
	{
		try
		{
			string text = tag.Substring(2, 6);
			return "#" + text;
		}
		catch (Exception exception)
		{
			Debug.Log("Something wrong in Helpers.GetTagColor().");
			Debug.LogException(exception);
		}
		return "#FFFFFF";
	}

	public static Color HexToColor(string hexColor, float alpha = 1f)
	{
		if ((double)alpha > 1.0)
		{
			alpha = 1f;
		}
		if ((double)alpha < 0.0)
		{
			alpha = 0f;
		}
		float num = Convert.ToInt32(hexColor.Substring(0, 2), 16);
		float num2 = Convert.ToInt32(hexColor.Substring(2, 2), 16);
		return new Color(b: (float)Convert.ToInt32(hexColor.Substring(4, 2), 16) / 256f, r: num / 256f, g: num2 / 256f, a: alpha);
	}

	public static string ColoredTag(string tag)
	{
		if (string.IsNullOrEmpty(tag))
		{
			return string.Empty;
		}
		if (!tag.Contains("[#"))
		{
			return tag;
		}
		string text = tag.Substring(1, 7);
		return "<color=" + text + ">" + tag.Substring(9) + "</color>";
	}

	public static string ColoredLog(object obj, string color = "#00FF00")
	{
		return "<b><color=" + color + ">Log: </color> " + obj?.ToString() + "</b>";
	}

	public static void Hint(Rect onHoverRect, string hint, GUIStyle style, HintAlignment alignment = HintAlignment.Left, float xOffset = 0f, float yOffset = 0f)
	{
		if (onHoverRect.Contains(Event.current.mousePosition))
		{
			style.wordWrap = true;
			GUI.Label(default(Rect), hint, style);
		}
	}

	public static void HintList(Rect onHoverRect, List<string> list, GUIStyle style, bool left = false, float xOffset = 0f, float yOffset = 0f)
	{
		if (onHoverRect.Contains(Event.current.mousePosition))
		{
			_ = list?.Count;
		}
	}

	public static string ColoredText(object text, string color = "#FFFFFF")
	{
		return "<color=" + color + ">" + text.ToString() + "</color>";
	}

	public static string HexOnly(string str)
	{
		string replacement = "";
		return new Regex("[^a-fA-F0-9]").Replace(str, replacement);
	}

	public static string FixedNickname(string editableName, string currentName)
	{
		if (editableName == currentName)
		{
			return currentName;
		}
		Regex regex = new Regex("[^a-zA-Zа-яА-Я0-9]");
		if (smethod_0(editableName) == Languages.En)
		{
			regex = new Regex("[^a-zA-Z0-9]");
		}
		if (smethod_0(editableName) == Languages.Ru)
		{
			regex = new Regex("[^а-яА-Я0-9]");
		}
		return regex.Replace(editableName, "");
	}

	public static Languages smethod_0(string s)
	{
		int num = 0;
		while (true)
		{
			if (num < s.Length)
			{
				char c = s[num];
				if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z'))
				{
					if ((c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я'))
					{
						break;
					}
					num++;
					continue;
				}
				return Languages.En;
			}
			return Languages.Any;
		}
		return Languages.Ru;
	}

	public static bool IsPositionInsideCollider(Collider collider, Vector3 position)
	{
		return (collider.ClosestPoint(position) - position).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
	}

	public static bool IsPositionIdentical(Vector3 posA, Vector3 posB, float epsilon = 1.1754944E-38f)
	{
		return (posA - posB).sqrMagnitude < epsilon * epsilon;
	}
}

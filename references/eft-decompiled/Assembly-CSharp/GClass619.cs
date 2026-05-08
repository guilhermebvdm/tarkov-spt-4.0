using System;
using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;
using UnityEngine;

public class GClass619
{
	public static string DoClear(string target, string origins, string key)
	{
		string text = target;
		string text2 = origins;
		if (target == null)
		{
			return text;
		}
		int num = target.IndexOf(key, StringComparison.Ordinal);
		if (num < 0)
		{
			return text;
		}
		target = target.Substring(num + key.Length, target.Length - (num + key.Length));
		int num2 = text2.IndexOf(key, StringComparison.Ordinal);
		if (num2 < 0)
		{
			return text;
		}
		text2 = text2.Substring(num2 + key.Length, text2.Length - (num2 + key.Length));
		string text3 = smethod_2("{", "}", text2, 2);
		if (text3 != null && text3.Length >= 5)
		{
			int num3 = 0;
			List<string> list = new List<string>();
			int num4 = 500;
			int startIndex = 0;
			while (num4 > 0)
			{
				num4--;
				int open;
				int close;
				string text4 = smethod_4(startIndex, "\"", "\"", text3, out open, out close);
				if (text4 == null)
				{
					break;
				}
				if (text4.Length >= 2)
				{
					int indexofRemain;
					string text5 = smethod_0(text3, text4, open, out indexofRemain);
					if (text5 == null || indexofRemain <= 0)
					{
						break;
					}
					indexofRemain++;
					text3 = text3.Substring(indexofRemain, text3.Length - indexofRemain);
					startIndex = 0;
					if (!FindNameInJson(target, text4, 0, out var foundPosition))
					{
						continue;
					}
					string text6 = smethod_0(target, text4, foundPosition, out indexofRemain);
					string value = text5.Trim();
					string text7 = text6.Trim();
					if (!text7.Equals(value) || text7.Length < 1)
					{
						continue;
					}
					int num5 = target.IndexOf(text4, StringComparison.Ordinal);
					int num6 = target.IndexOf(text7, num5, StringComparison.Ordinal);
					if (num6 <= 0)
					{
						Debug.LogError("can't find at target:" + text7 + " key:" + key);
						continue;
					}
					if (num5 <= 0)
					{
						Debug.LogError("can't find at target:" + text4 + " key:" + key);
						continue;
					}
					try
					{
						num6 += text7.Length;
						int num7 = target.IndexOf(",", num6, StringComparison.Ordinal);
						int num8 = target.IndexOf("}", num6, StringComparison.Ordinal);
						if (num8 <= 0 || num8 >= num7)
						{
							num6 = num7 + 1;
						}
						num5--;
						list.Add(target.Substring(num5, num6 - num5));
						num3++;
					}
					catch (Exception)
					{
						Debug.LogError($"can't del:{target.Length}  startOfDel:{num5}  endToDel:{num6}  trimTargetValue:{text7.Length}  val{text7}");
					}
					continue;
				}
				Debug.LogError("name is too short:" + text4 + "  block:" + key);
				break;
			}
			{
				foreach (string item in list)
				{
					text = text.Replace(item, "");
				}
				return text;
			}
		}
		return text;
	}

	public static string smethod_0(string data, string fieldName, int startSearchIndex, out int indexofRemain)
	{
		int startIndex = startSearchIndex + fieldName.Length - 1;
		int num = data.IndexOf(":", startIndex, StringComparison.Ordinal);
		if (num < 0)
		{
			indexofRemain = -1;
			return null;
		}
		int i;
		for (i = num + 1; i < data.Length && char.IsWhiteSpace(data[i]); i++)
		{
		}
		if (i >= data.Length)
		{
			indexofRemain = -1;
			return null;
		}
		char c = data[i];
		int num2 = -1;
		switch (c)
		{
		case '"':
		{
			bool flag = false;
			for (int m = i + 1; m < data.Length; m++)
			{
				if (flag)
				{
					flag = false;
				}
				else if (data[m] == '\\')
				{
					flag = true;
				}
				else if (data[m] == '"')
				{
					num2 = m + 1;
					break;
				}
			}
			if (num2 < 0)
			{
				indexofRemain = -1;
				return null;
			}
			int num8 = -1;
			for (int n = num2; n < data.Length; n++)
			{
				if (data[n] != ',' && data[n] != '}' && data[n] != ']')
				{
					if (!char.IsWhiteSpace(data[n]))
					{
						break;
					}
					continue;
				}
				num8 = n;
				break;
			}
			if (num8 < 0)
			{
				indexofRemain = -1;
				return null;
			}
			indexofRemain = num8;
			return data.Substring(i, num2 - i);
		}
		case '[':
		{
			int num4 = smethod_1(data, i, '[', ']');
			if (num4 < 0)
			{
				indexofRemain = -1;
				return null;
			}
			int num5 = -1;
			for (int k = num4; k < data.Length; k++)
			{
				if (data[k] != ',' && data[k] != '}' && data[k] != ']')
				{
					if (!char.IsWhiteSpace(data[k]))
					{
						break;
					}
					continue;
				}
				num5 = k;
				break;
			}
			if (num5 < 0)
			{
				num5 = num4;
			}
			indexofRemain = num5;
			return data.Substring(i, num4 - i);
		}
		case '{':
		{
			int num6 = smethod_1(data, i, '{', '}');
			if (num6 < 0)
			{
				indexofRemain = -1;
				return null;
			}
			int num7 = -1;
			for (int l = num6; l < data.Length; l++)
			{
				if (data[l] != ',' && data[l] != '}' && data[l] != ']')
				{
					if (!char.IsWhiteSpace(data[l]))
					{
						break;
					}
					continue;
				}
				num7 = l;
				break;
			}
			if (num7 < 0)
			{
				num7 = num6;
			}
			indexofRemain = num7;
			return data.Substring(i, num6 - i);
		}
		default:
		{
			int num3 = -1;
			for (int j = i; j < data.Length; j++)
			{
				if (data[j] == ',' || data[j] == '}' || data[j] == ']')
				{
					num3 = j;
					break;
				}
			}
			if (num3 < 0)
			{
				indexofRemain = -1;
				return null;
			}
			indexofRemain = num3;
			return data.Substring(i, num3 - i).Trim();
		}
		}
	}

	public static bool FindNameInJson(string jsonString, string name, int startPosition, out int foundPosition)
	{
		foundPosition = -1;
		if (!string.IsNullOrEmpty(jsonString) && !string.IsNullOrEmpty(name))
		{
			string text = "\"" + name + "\"";
			int startIndex = startPosition;
			bool flag = false;
			while (true)
			{
				if (!flag)
				{
					foundPosition = jsonString.IndexOf(text, startIndex, StringComparison.Ordinal);
					if (foundPosition < 0)
					{
						break;
					}
					bool flag2 = false;
					if (foundPosition == 0)
					{
						flag2 = true;
					}
					else
					{
						char c = jsonString[foundPosition - 1];
						flag2 = c == '{' || c == ',' || char.IsWhiteSpace(c);
					}
					bool flag3 = false;
					int num = foundPosition + text.Length;
					if (num < jsonString.Length)
					{
						int num2 = jsonString.IndexOf(':', num);
						if (num2 >= 0)
						{
							bool flag4 = true;
							for (int i = num; i < num2; i++)
							{
								if (!char.IsWhiteSpace(jsonString[i]))
								{
									flag4 = false;
									break;
								}
							}
							flag3 = flag4;
						}
					}
					if (flag2 && flag3)
					{
						flag = true;
					}
					else
					{
						startIndex = foundPosition + 1;
					}
					continue;
				}
				try
				{
					if (jsonString.IndexOf(":", foundPosition + text.Length, StringComparison.Ordinal) < 0)
					{
						return false;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Ошибка при поиске значения для " + name + ": " + ex.Message);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public static int smethod_1(string text, int openBracketPosition, char openBracket, char closeBracket)
	{
		int num = 0;
		int num2 = openBracketPosition;
		while (true)
		{
			if (num2 < text.Length)
			{
				if (text[num2] == openBracket)
				{
					num++;
				}
				else if (text[num2] == closeBracket)
				{
					num--;
					if (num == 0)
					{
						break;
					}
				}
				num2++;
				continue;
			}
			return -1;
		}
		return num2 + 1;
	}

	public static string DoMerge(string from, string to, string key)
	{
		int num = from.IndexOf(key, StringComparison.Ordinal);
		if (num < 0)
		{
			return to;
		}
		from = from.Substring(num, from.Length - num);
		string text = smethod_2("{", "}", from, 2);
		if (text == null)
		{
			return to;
		}
		int num2 = 500;
		while (num2 > 0)
		{
			num2--;
			string text2 = smethod_2("\"", "\"", text);
			if (text2 == null)
			{
				break;
			}
			int indexofRemain;
			string text3 = smethod_0(text, text2, 0, out indexofRemain);
			if (text3 == null || indexofRemain <= 0)
			{
				break;
			}
			indexofRemain++;
			text = text.Substring(indexofRemain, text.Length - indexofRemain);
			if (!FindNameInJson(to, text2, 0, out var foundPosition))
			{
				string text4 = text2 + "\":" + text3 + ",";
				int startIndex = to.IndexOf(key, StringComparison.Ordinal);
				int num3 = to.IndexOf("{", startIndex, StringComparison.Ordinal);
				to = to.Insert(num3 + 1, text4);
				Debug.Log("  can't find name:" + text2 + "  at pvp inserted:" + text4);
				continue;
			}
			int indexofRemain2;
			string text5 = smethod_0(to.Substring(foundPosition, to.Length - foundPosition), text2, 0, out indexofRemain2);
			if (text5 != null)
			{
				int num4 = to.IndexOf(":", foundPosition + text2.Length - 1, StringComparison.Ordinal);
				int num5 = to.IndexOf(text5, foundPosition + text2.Length - 1, StringComparison.Ordinal) + text5.Length - num4;
				to = to.Remove(num4 + 1, num5 - 1);
				int num6 = to.IndexOf(text2, StringComparison.Ordinal);
				int num7 = to.IndexOf(":", num6, StringComparison.Ordinal);
				if (num6 > 0)
				{
					to = to.Insert(num7 + 1, text3);
					Debug.LogError("chnage:" + text2 + "   valueToChange:" + text5 + " to:" + text3);
				}
			}
		}
		return to;
	}

	public static string smethod_2(string from, string to, string data, int offset = 0)
	{
		int num = data.IndexOf(from, StringComparison.Ordinal);
		if (num < 0)
		{
			return null;
		}
		int num2 = data.IndexOf(to, num + 1, StringComparison.Ordinal);
		if (num2 < 0)
		{
			return null;
		}
		return data.Substring(num + offset, num2 - (num + offset));
	}

	public static string smethod_3(string from, string to, string data, out int open, out int close)
	{
		open = data.IndexOf(from, StringComparison.Ordinal);
		close = -1;
		if (open < 0)
		{
			return null;
		}
		close = data.IndexOf(to, open + 1, StringComparison.Ordinal);
		if (close < 0)
		{
			return null;
		}
		open++;
		return data.Substring(open, close - open);
	}

	public static string smethod_4(int startIndex, string from, string to, string data, out int open, out int close)
	{
		open = data.IndexOf(from, startIndex, StringComparison.Ordinal);
		close = -1;
		if (open < 0)
		{
			return null;
		}
		close = data.IndexOf(to, open + 1, StringComparison.Ordinal);
		if (close < 0)
		{
			return null;
		}
		open++;
		return data.Substring(open, close - open);
	}

	public static string RemoveEmptyLines(string input, bool removeComments = true, bool removeEmptyLines = true)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		string[] array = input.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			string text2 = text.Trim();
			if ((!removeEmptyLines || !string.IsNullOrWhiteSpace(text2)) && (!removeComments || !text2.StartsWith("//")))
			{
				list.Add(text);
			}
		}
		return string.Join(Environment.NewLine, list);
	}

	public static string AddTrailingCommas(string jsonString)
	{
		if (string.IsNullOrEmpty(jsonString))
		{
			return jsonString;
		}
		string[] array = jsonString.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		List<string> list = new List<string>();
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (!string.IsNullOrWhiteSpace(text) && !text.StartsWith("//"))
			{
				if (!text.EndsWith(",") && !text.EndsWith("{") && !text.EndsWith("[") && !text.EndsWith(":"))
				{
					string text2 = null;
					for (int j = i + 1; j < array.Length; j++)
					{
						string text3 = array[j].Trim();
						if (!string.IsNullOrWhiteSpace(text3) && !text3.StartsWith("//"))
						{
							text2 = text3;
							break;
						}
					}
					if (text2 != null && (text2.StartsWith("}") || text2.StartsWith("]")) && !array[i].EndsWith(","))
					{
						array[i] += ",";
					}
				}
				list.Add(array[i]);
			}
			else
			{
				list.Add(array[i]);
			}
		}
		return string.Join(Environment.NewLine, list);
	}

	public static string Clear(WildSpawnType role, BotDifficulty dif)
	{
		string text = LocalBotSettingsProviderClass.LoadDifficultyStringInternal(dif, role, isPve: false);
		if (text == null)
		{
			return null;
		}
		string text2 = LocalBotSettingsProviderClass.LoadDifficultyStringInternal(dif, role, isPve: true);
		if (text2 == null)
		{
			return null;
		}
		text2 = AddTrailingCommas(text2);
		text = AddTrailingCommas(text);
		text2 = DoClear(text2, text, "Mind");
		text2 = DoClear(text2, text, "Lay");
		text2 = DoClear(text2, text, "Aiming");
		text2 = DoClear(text2, text, "Look");
		text2 = DoClear(text2, text, "Shoot");
		text2 = DoClear(text2, text, "Move");
		text2 = DoClear(text2, text, "Grenade");
		text2 = DoClear(text2, text, "Cover");
		text2 = DoClear(text2, text, "Hearing");
		text2 = DoClear(text2, text, "Boss");
		text2 = DoClear(text2, text, "Change");
		text2 = DoClear(text2, text, "Scattering");
		try
		{
			text2 = RemoveEmptyLines(text2);
			JsonParserClass.ParseJsonTo<BotSettingsComponents>(text2, Array.Empty<JsonConverter>());
			LocalBotSettingsProviderClass.SaveDifficultyStringInternal(dif, role, isPve: true, text2);
		}
		catch (Exception arg)
		{
			Debug.LogError($"ERROR Final clear :{arg}");
			Debug.LogError("ERROR Final clear :" + text2);
		}
		return text2;
	}
}

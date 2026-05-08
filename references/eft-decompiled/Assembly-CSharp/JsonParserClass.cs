using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class JsonParserClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class462
	{
		public static readonly Class462 class462_0 = new Class462();

		public static Func<char, bool> func_0;

		public static Func<char, bool> func_1;

		public static Func<char, bool> func_2;

		public static Func<char, bool> func_3;

		public static Func<char, bool> func_4;

		public bool method_0(char c)
		{
			return c == '{';
		}

		public bool method_1(char c)
		{
			return c == '}';
		}

		public bool method_2(char c)
		{
			return c == '[';
		}

		public bool method_3(char c)
		{
			return c == ']';
		}

		public bool method_4(char c)
		{
			return c == '"';
		}
	}

	public static T ParseJsonTo<T>(this string json, params JsonConverter[] converters)
	{
		JsonConverter[] converters2 = ((converters == null || converters.Length == 0) ? JsonSerializerSettingsClass.Converters : converters);
		return JsonConvert.DeserializeObject<T>(json, converters2);
	}

	public static T ParseJsonTo<T>(this GClass846 unparsedData, params JsonConverter[] converters)
	{
		JsonConverter[] converters2 = ((converters == null || converters.Length == 0) ? JsonSerializerSettingsClass.Converters : converters);
		return JsonConvert.DeserializeObject<T>(ToJson(unparsedData), converters2);
	}

	public static object ParseJsonTo(this GClass846 unparsedData, Type type, object existingObject, params JsonConverter[] converters)
	{
		JsonConvert.PopulateObject(ToJson(unparsedData), existingObject, new JsonSerializerSettings
		{
			Converters = ((converters == null || converters.Length == 0) ? JsonSerializerSettingsClass.Converters : converters)
		});
		return existingObject;
	}

	public static string ToPrettyJson<T>(this T obj, params JsonConverter[] converters)
	{
		return JsonConvert.SerializeObject(obj, Formatting.Indented, (converters == null || converters.Length == 0) ? JsonSerializerSettingsClass.Converters : converters);
	}

	public static string ToJson<T>(this T obj, params JsonConverter[] converters)
	{
		return JsonConvert.SerializeObject(obj, (converters == null || converters.Length == 0) ? JsonSerializerSettingsClass.Converters : converters);
	}

	public static GClass846 ToUnparsedData<T>(this T obj, params JsonConverter[] converters)
	{
		JsonSerializer jsonSerializer = new JsonSerializer();
		converters = ((converters == null || converters.Length == 0) ? JsonSerializerSettingsClass.Converters : converters);
		JsonConverter[] array = converters;
		foreach (JsonConverter item in array)
		{
			jsonSerializer.Converters.Add(item);
		}
		return new GClass846
		{
			JToken = JToken.FromObject(obj, jsonSerializer)
		};
	}

	public static void PopulateJson<T>(this string json, T target)
	{
		JsonConvert.PopulateObject(json, target);
	}

	public static string BeautifyJson(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return json;
		}
		return smethod_0(json, Formatting.Indented);
	}

	public static string MinifyJson(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return json;
		}
		return smethod_0(json, Formatting.None);
	}

	public static string smethod_0(string json, Formatting formatting)
	{
		using StringReader reader = new StringReader(json);
		using StringWriter stringWriter = new StringWriter();
		using (JsonTextReader reader2 = new JsonTextReader(reader))
		{
			using JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter);
			jsonTextWriter.Formatting = formatting;
			jsonTextWriter.WriteToken(reader2);
		}
		return stringWriter.ToString();
	}

	public static void LogJsonDeserializationError<T>(string jsonData)
	{
		try
		{
			JsonConvert.DeserializeObject<T>(jsonData, JsonSerializerSettingsClass.Converters);
		}
		catch (JsonSerializationException jsonEx)
		{
			string jsonData2;
			try
			{
				jsonData2 = BeautifyJson(jsonData);
			}
			catch (Exception)
			{
				jsonData2 = jsonData;
			}
			LogJsonDeserializationError(jsonEx, jsonData2);
		}
		catch (JsonReaderException jsonEx2)
		{
			smethod_1(jsonEx2, jsonData);
		}
		catch (Exception ex2)
		{
			Debug.LogError("Unexpected JSON error: " + ex2.Message + "\nStack trace: " + ex2.StackTrace);
		}
	}

	public static void smethod_1(JsonReaderException jsonEx, string jsonData)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string message = jsonEx.Message;
		stringBuilder.AppendLine("JSON syntax error: " + message);
		if (!string.IsNullOrEmpty(jsonData))
		{
			int lineNumber = jsonEx.LineNumber;
			int linePosition = jsonEx.LinePosition;
			if (lineNumber > 0)
			{
				stringBuilder.AppendLine($"Error on line {lineNumber}, position {linePosition}");
				string[] array = jsonData.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length != 0)
				{
					int num = Math.Max(0, lineNumber - 4);
					int num2 = Math.Min(array.Length - 1, lineNumber + 2);
					stringBuilder.AppendLine("Error context:");
					for (int i = num; i <= num2; i++)
					{
						string text = (i + 1).ToString().PadLeft(4);
						string text2 = ((i < array.Length) ? array[i] : string.Empty);
						stringBuilder.AppendLine(text + ": " + text2);
						if (i == lineNumber - 1)
						{
							string text3 = new string(' ', text.Length + 2);
							for (int j = 0; j < Math.Min(linePosition - 1, text2.Length); j++)
							{
								text3 += ((text2[j] == '\t') ? '\t' : ' ');
							}
							text3 += "^ ERROR";
							stringBuilder.AppendLine(text3);
						}
					}
				}
				else
				{
					stringBuilder.AppendLine("Could not split JSON into lines.");
					stringBuilder.AppendLine("JSON content (truncated if too large):");
					stringBuilder.AppendLine((jsonData.Length > 1000) ? (jsonData.Substring(0, 1000) + "...") : jsonData);
				}
			}
			else
			{
				stringBuilder.AppendLine("JSON content (truncated if too large):");
				stringBuilder.AppendLine((jsonData.Length > 1000) ? (jsonData.Substring(0, 1000) + "...") : jsonData);
			}
		}
		else
		{
			stringBuilder.AppendLine("JSON data is empty.");
		}
		stringBuilder.AppendLine("Stack trace: " + jsonEx.StackTrace);
		Debug.LogError(stringBuilder.ToString());
		Debug.LogException(jsonEx);
	}

	public static void LogJsonDeserializationError(JsonSerializationException jsonEx, string jsonData = null, string prefix = "JSON")
	{
		StringBuilder stringBuilder = new StringBuilder();
		string message = jsonEx.Message;
		stringBuilder.AppendLine(prefix + " deserialization error: " + message + ".");
		if (!string.IsNullOrEmpty(jsonData))
		{
			string[] obj = new string[4] { "line (\\d+), position (\\d+)", "Line (\\d+), position (\\d+)", "at line (\\d+), position (\\d+)", "Error occurred at line (\\d+), position (\\d+)" };
			int num = -1;
			int num2 = -1;
			string[] array = obj;
			foreach (string pattern in array)
			{
				Match match = Regex.Match(message, pattern);
				if (match.Success)
				{
					num = int.Parse(match.Groups[1].Value);
					num2 = int.Parse(match.Groups[2].Value);
					break;
				}
			}
			if (num == -1 && jsonEx.StackTrace != null)
			{
				Match match2 = Regex.Match(jsonEx.StackTrace, "at position (\\d+) in JSON");
				if (match2.Success)
				{
					int val = int.Parse(match2.Groups[1].Value);
					string[] array2 = jsonData.Substring(0, Math.Min(val, jsonData.Length)).Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
					if (array2.Length != 0)
					{
						num = array2.Length;
						num2 = array2[^1].Length + 1;
					}
				}
			}
			if (num != -1)
			{
				stringBuilder.AppendLine($"Error on line {num}, position {num2}");
				string[] array3 = jsonData.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
				int num3 = ((array3.Length > 100) ? 2 : 5);
				int num4 = Math.Max(0, num - num3);
				int num5 = Math.Min(array3.Length - 1, num + num3);
				stringBuilder.AppendLine("Error context:");
				for (int j = num4; j <= num5 && j < array3.Length; j++)
				{
					string text = (j + 1).ToString().PadLeft(4);
					string text2 = array3[j];
					stringBuilder.AppendLine(text + ": " + text2);
					if (j == num - 1)
					{
						string text3 = new string(' ', text.Length + 2);
						int num6 = Math.Min(num2 - 1, text2.Length);
						for (int k = 0; k < num6; k++)
						{
							text3 += ((text2[k] == '\t') ? '\t' : ' ');
						}
						text3 += "^ ERROR";
						stringBuilder.AppendLine(text3);
					}
				}
			}
			else
			{
				stringBuilder.AppendLine("Unable to determine exact error location. JSON preview:");
				if (jsonData.Length > 2000)
				{
					stringBuilder.AppendLine("Start of JSON:");
					stringBuilder.AppendLine(jsonData.Substring(0, 1000));
					stringBuilder.AppendLine("...");
					stringBuilder.AppendLine("End of JSON:");
					stringBuilder.AppendLine(jsonData.Substring(jsonData.Length - 1000));
				}
				else
				{
					string[] array4 = jsonData.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
					int num7 = Math.Min(array4.Length, 50);
					for (int l = 0; l < num7; l++)
					{
						stringBuilder.AppendLine($"{l + 1}: {array4[l]}");
					}
					if (array4.Length > num7)
					{
						stringBuilder.AppendLine($"... {array4.Length - num7} more lines ...");
					}
				}
				smethod_2(jsonData, stringBuilder);
			}
		}
		else
		{
			stringBuilder.AppendLine("JSON data is empty or not provided.");
		}
		stringBuilder.AppendLine("Stack trace: " + jsonEx.StackTrace);
		Debug.LogError(stringBuilder.ToString());
		Debug.LogException(jsonEx);
	}

	public static void smethod_2(string jsonData, StringBuilder errorLog)
	{
		if (!string.IsNullOrEmpty(jsonData))
		{
			errorLog.AppendLine("\nPossible error causes:");
			int num = jsonData.Count((char c) => c == '{');
			int num2 = jsonData.Count((char c) => c == '}');
			if (num != num2)
			{
				errorLog.AppendLine($"- Unbalanced curly braces: {num} opening vs {num2} closing");
			}
			int num3 = jsonData.Count((char c) => c == '[');
			int num4 = jsonData.Count((char c) => c == ']');
			if (num3 != num4)
			{
				errorLog.AppendLine($"- Unbalanced square brackets: {num3} opening vs {num4} closing");
			}
			int num5 = jsonData.Count((char c) => c == '"');
			if (num5 % 2 != 0)
			{
				errorLog.AppendLine($"- Odd number of quotes: {num5} (should be even)");
			}
			if (jsonData.Contains(",]") || jsonData.Contains(",}"))
			{
				errorLog.AppendLine("- Trailing comma before closing bracket or brace");
			}
			if (Regex.IsMatch(jsonData, "\"[^\"]*\":\\s*(?![\\{\\}\\[\\]\"0-9null])"))
			{
				errorLog.AppendLine("- Possible invalid value type (not object, array, string, number or null)");
			}
		}
	}
}

using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public abstract class GClass1200
{
	public static void SaveToJson<T>(T data, string folderName, string fileName, bool overrideFileIfExist = false)
	{
		string text = Path.Combine(Application.persistentDataPath, folderName);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string text2 = Path.Combine(text, fileName + ".json");
		if (File.Exists(text2))
		{
			if (!overrideFileIfExist)
			{
				Debug.LogWarning("File already exists: " + text2);
				return;
			}
			File.Delete(text2);
		}
		string contents = JsonConvert.SerializeObject(data, Formatting.Indented);
		File.WriteAllText(text2, contents);
	}

	public static T LoadFromJson<T>(string folderName, string fileName)
	{
		string path = Path.Combine(Application.persistentDataPath, folderName, fileName);
		if (File.Exists(path))
		{
			return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
		}
		Debug.LogError("File not found!");
		return default(T);
	}

	public static T[] LoadAllFromJson<T>(string folderName)
	{
		string path = Path.Combine(Application.persistentDataPath, folderName);
		if (!Directory.Exists(path))
		{
			Debug.LogError("Folder not found!");
			return Array.Empty<T>();
		}
		string[] files = Directory.GetFiles(path, "*.json");
		T[] array = new T[files.Length];
		for (int i = 0; i < files.Length; i++)
		{
			string value = File.ReadAllText(files[i]);
			array[i] = JsonConvert.DeserializeObject<T>(value);
		}
		return array;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass882
{
	[CompilerGenerated]
	public class Class506
	{
		public MonoBehaviour sender;

		public IEnumerator enumerator;

		public void method_0()
		{
			sender.StartCoroutine(enumerator);
		}
	}

	[NonSerialized]
	public static string String_0 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EFTOutput");

	[NonSerialized]
	public static string String_1 = "Error code {0}: {1}. {2}";

	[NonSerialized]
	public static string String_2 = "Performing pause. Lasts {0} seconds.";

	[NonSerialized]
	public static string String_3 = "Critical error. Application will be closed";

	[NonSerialized]
	public static GClass882 Gclass882_0;

	[NonSerialized]
	public List<string> List_0 = new List<string>();

	public static GClass882 GClass882_0 => Gclass882_0 ?? (Gclass882_0 = new GClass882());

	public void method_0(MonoBehaviour sender, ErrorCode code = ErrorCode.InitOperationError, string message = null, IEnumerator enumerator = null)
	{
		string text = string.Format(String_1, (int)code, code, " In class: " + sender.GetType().Name + message);
		Debug.LogWarning(text);
		List_0.Add(text);
		switch (code)
		{
		case ErrorCode.SceneLoadFailed:
		case ErrorCode.LoginShowOperationFailed:
			sender.StopAllCoroutines();
			sender.StartCoroutine(method_2(5f, delegate
			{
				sender.StartCoroutine(enumerator);
			}));
			break;
		case ErrorCode.InitOperationError:
			method_3();
			break;
		}
		method_1();
	}

	public void method_1()
	{
		string path = String_0 + "\\log_" + EFTDateTimeClass.Now.ToString("dd.MM.yy") + ".txt";
		if (!Directory.Exists(String_0))
		{
			Directory.CreateDirectory(String_0);
		}
		if (File.Exists(path))
		{
			string[] array = File.ReadAllLines(path);
			foreach (string item in array)
			{
				List_0.Add(item);
			}
		}
		File.WriteAllLines(path, List_0.ToArray());
	}

	public IEnumerator method_2(float time, Action callback)
	{
		string text = string.Format(String_2, time);
		Debug.LogWarning(text);
		List_0.Add(text);
		yield return new WaitForSeconds(time);
		callback();
	}

	public void method_3()
	{
		Debug.LogError(String_3);
		List_0.Add(String_3);
		Application.Quit();
	}
}

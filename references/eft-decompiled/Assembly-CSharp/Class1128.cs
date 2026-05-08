using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Class1128
{
	[Serializable]
	[CompilerGenerated]
	public class Class1129
	{
		public static readonly Class1129 class1129_0 = new Class1129();

		public static Func<string, bool> func_0;

		public static Func<string, string> func_1;

		public bool method_0(string url)
		{
			return !string.IsNullOrWhiteSpace(url);
		}

		public string method_1(string url)
		{
			return smethod_1(url);
		}
	}

	[CompilerGenerated]
	public class Class1130
	{
		public string url;

		public string method_0(string u)
		{
			return u + url;
		}
	}

	public static string[] smethod_0(this IEnumerable<string> urls, string url)
	{
		return urls.Select((string u) => u + url).ToArray();
	}

	public static string smethod_1(this string url)
	{
		while (!string.IsNullOrEmpty(url))
		{
			char c = url[url.Length - 1];
			if (c != '/' && c != '\\')
			{
				break;
			}
			url = url.Substring(0, url.Length - 1);
		}
		return url;
	}

	public static string[] smethod_2(string backendUrl, string[] alternativeUrls)
	{
		string[] array = new string[1] { smethod_1(backendUrl) };
		if (alternativeUrls == null)
		{
			return array;
		}
		alternativeUrls = (from url in alternativeUrls
			where !string.IsNullOrWhiteSpace(url)
			select smethod_1(url)).ToArray();
		array = array.Concat(alternativeUrls).ToArray();
		return smethod_3(array);
	}

	public static string[] smethod_3(string[] urls, int count = 3)
	{
		if (urls.Length <= 1)
		{
			return urls;
		}
		for (int i = 0; i < count; i++)
		{
			int num = UnityEngine.Random.Range(0, urls.Length);
			int num2 = UnityEngine.Random.Range(0, urls.Length);
			string text = urls[num];
			urls[num] = urls[num2];
			urls[num2] = text;
		}
		return urls;
	}
}

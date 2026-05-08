using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass1807
{
	public Dictionary<string, string> menu;

	public GClass2347 ConvertToLocale()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, string> item in menu)
		{
			try
			{
				dictionary.Add(item.Key, item.Value);
			}
			catch (ArgumentException)
			{
				Debug.LogError("<b>Locale</b>. Trying to add duplicate: " + item.Key);
			}
		}
		return new GClass2347(dictionary);
	}
}

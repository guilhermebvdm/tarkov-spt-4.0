using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public class GClass706
{
	[Serializable]
	[CompilerGenerated]
	public class Class371
	{
		public static readonly Class371 class371_0 = new Class371();

		public static Func<KeyValuePair<string, object>, string> func_0;

		public static Func<KeyValuePair<string, object>, object> func_1;

		public static Func<KeyValuePair<string, object>, string> func_2;

		public static Func<KeyValuePair<string, object>, object> func_3;

		public static Func<KeyValuePair<string, int>, bool> func_4;

		public string method_0(KeyValuePair<string, object> x)
		{
			return x.Key;
		}

		public object method_1(KeyValuePair<string, object> x)
		{
			return x.Value;
		}

		public string method_2(KeyValuePair<string, object> x)
		{
			return x.Key;
		}

		public object method_3(KeyValuePair<string, object> x)
		{
			return x.Value;
		}

		public bool method_4(KeyValuePair<string, int> kvp)
		{
			return kvp.Value == 0;
		}
	}

	[CompilerGenerated]
	public class Class372
	{
		public Dictionary<string, object> oldProperties;

		public bool method_0(KeyValuePair<string, object> a)
		{
			if (oldProperties.ContainsKey(a.Key))
			{
				return !smethod_0(oldProperties[a.Key], a.Value);
			}
			return false;
		}
	}

	[NonSerialized]
	public Dictionary<string, Dictionary<string, object>> Dictionary_0 = new Dictionary<string, Dictionary<string, object>>();

	public void SaveItem(string itemId, Dictionary<string, object> properties)
	{
		Dictionary_0[itemId] = properties.ToDictionary((KeyValuePair<string, object> x) => x.Key, (KeyValuePair<string, object> x) => x.Value);
	}

	public Dictionary<string, object> GetDiff(string itemId, Dictionary<string, object> properties)
	{
		Dictionary<string, object> oldProperties = Dictionary_0[itemId];
		return properties.Where((KeyValuePair<string, object> a) => oldProperties.ContainsKey(a.Key) && !smethod_0(oldProperties[a.Key], a.Value)).ToDictionary((KeyValuePair<string, object> x) => x.Key, (KeyValuePair<string, object> x) => x.Value);
	}

	public static bool smethod_0(object a, object b)
	{
		string[] array = a as string[];
		string[] array2 = b as string[];
		if (array != null && array2 != null)
		{
			return smethod_1(array, array2);
		}
		Dictionary<string, object> obj = a as Dictionary<string, object>;
		Dictionary<string, object> dictionary = b as Dictionary<string, object>;
		if (obj != null && dictionary != null)
		{
			return true;
		}
		return a.Equals(b);
	}

	public static bool smethod_1(ICollection<string> left, ICollection<string> right)
	{
		if (!object.Equals(left.Count, right.Count))
		{
			return false;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (string item in left)
		{
			dictionary[item] = 1;
		}
		foreach (string item2 in right)
		{
			if (dictionary.ContainsKey(item2))
			{
				dictionary[item2]--;
				continue;
			}
			return false;
		}
		return dictionary.All((KeyValuePair<string, int> kvp) => kvp.Value == 0);
	}
}

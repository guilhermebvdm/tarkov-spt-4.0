using System.Collections.Generic;

public class GClass624<T, U, V>
{
	public readonly Dictionary<T, Dictionary<U, V>> baseDictionary = new Dictionary<T, Dictionary<U, V>>();

	public int Count => baseDictionary.Count;

	public string CountFull()
	{
		int num = 0;
		foreach (KeyValuePair<T, Dictionary<U, V>> item in baseDictionary)
		{
			num += item.Value.Count;
		}
		return $"Count:{Count} Sub:{num}";
	}

	public void Add(T primary, U secondary, V val)
	{
		if (!baseDictionary.TryGetValue(primary, out var value))
		{
			value = new Dictionary<U, V>();
			baseDictionary.Add(primary, value);
		}
		if (value.ContainsKey(secondary))
		{
			value[secondary] = val;
		}
		else
		{
			value.Add(secondary, val);
		}
	}

	public V Get(T primary, U secondary)
	{
		if (baseDictionary.TryGetValue(primary, out var value))
		{
			return value[secondary];
		}
		return default(V);
	}

	public void Set(T primary, U secondary, V val)
	{
		if (baseDictionary.TryGetValue(primary, out var value))
		{
			if (value.ContainsKey(secondary))
			{
				value[secondary] = val;
			}
			else
			{
				value.Add(secondary, val);
			}
		}
		else
		{
			Dictionary<U, V> dictionary = new Dictionary<U, V>();
			baseDictionary.Add(primary, dictionary);
			dictionary.Add(secondary, val);
		}
	}

	public Dictionary<U, V> TryGetAllValsByKey(T primary)
	{
		if (baseDictionary.TryGetValue(primary, out var value))
		{
			return value;
		}
		return null;
	}

	public bool TryGet(T primary, U secondary, out V val)
	{
		if (baseDictionary.TryGetValue(primary, out var value) && value.TryGetValue(secondary, out var value2))
		{
			val = value2;
			return true;
		}
		val = default(V);
		return false;
	}

	public bool ContainsKey(T primary, U secondary)
	{
		if (baseDictionary.TryGetValue(primary, out var value))
		{
			return value.ContainsKey(secondary);
		}
		return false;
	}

	public IEnumerable<GClass623<T, U, V>> GetAllVals()
	{
		List<GClass623<T, U, V>> list = new List<GClass623<T, U, V>>();
		foreach (KeyValuePair<T, Dictionary<U, V>> item in baseDictionary)
		{
			foreach (KeyValuePair<U, V> item2 in item.Value)
			{
				list.Add(new GClass623<T, U, V>(item.Key, item2.Key, item2.Value));
			}
		}
		return list;
	}

	public void Clear()
	{
		baseDictionary.Clear();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

public class GClass818<T, U>
{
	public struct GStruct48(List<U> list)
	{
		[NonSerialized]
		public List<U> List_0 = list;

		public List<U>.Enumerator GetEnumerator()
		{
			return List_0.GetEnumerator();
		}
	}

	[NonSerialized]
	public Dictionary<T, U> Dictionary_0;

	[NonSerialized]
	public List<U> List_0;

	public int Count => List_0.Count;

	public GClass818(int capacity = 0)
	{
		Dictionary_0 = new Dictionary<T, U>(capacity);
		List_0 = new List<U>(capacity);
	}

	public U GetByKey(T key)
	{
		return Dictionary_0[key];
	}

	public bool ContainsKey(T key)
	{
		return Dictionary_0.ContainsKey(key);
	}

	public bool TryGetByKey(T key, out U value)
	{
		return Dictionary_0.TryGetValue(key, out value);
	}

	public U GetByIndex(int index)
	{
		return List_0[index];
	}

	public void Add(T key, U value)
	{
		if (Dictionary_0.ContainsKey(key))
		{
			throw new Exception("Key already been added");
		}
		method_0(key, value);
	}

	public void method_0(T key, U value)
	{
		Dictionary_0[key] = value;
		List_0.Add(value);
	}

	public U AddOrReplace(T key, U value)
	{
		if (TryGetByKey(key, out var value2))
		{
			int index = List_0.IndexOf(value2);
			Dictionary_0[key] = value;
			List_0[index] = value;
			return value2;
		}
		method_0(key, value);
		return default(U);
	}

	public bool Remove(T key)
	{
		U value;
		bool num = Dictionary_0.TryGetValue(key, out value);
		if (num)
		{
			Dictionary_0.Remove(key);
			List_0.Remove(value);
		}
		return num;
	}

	public void Clear()
	{
		Dictionary_0.Clear();
		List_0.Clear();
	}

	public IEnumerable<U> Where(Func<U, bool> predicate)
	{
		return List_0.Where(predicate);
	}

	public GStruct48 GetValuesEnumerator()
	{
		return new GStruct48(List_0);
	}
}

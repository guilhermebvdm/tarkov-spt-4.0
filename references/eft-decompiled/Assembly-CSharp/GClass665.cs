using System;
using System.Collections.Generic;

public class GClass665<T>
{
	[NonSerialized]
	public Dictionary<Type, T> Dictionary_0 = new Dictionary<Type, T>();

	public Dictionary<Type, T>.ValueCollection Values => Dictionary_0.Values;

	public bool ContainsType(Type type)
	{
		return Dictionary_0.ContainsKey(type);
	}

	public void Add<GT>(Type type, GT t) where GT : T
	{
		Dictionary_0.Add(type, (T)(object)t);
	}

	public void Add<GT>(GT t) where GT : T
	{
		Dictionary_0.Add(typeof(GT), (T)(object)t);
	}

	public GT Get<GT>() where GT : T
	{
		if (Dictionary_0.TryGetValue(typeof(GT), out var value))
		{
			return (GT)(object)value;
		}
		return default(GT);
	}

	public GT Get<GT>(Type type) where GT : T
	{
		if (Dictionary_0.TryGetValue(type, out var value))
		{
			return (GT)(object)value;
		}
		return default(GT);
	}

	public bool Remove(Type t)
	{
		return Dictionary_0.Remove(t);
	}

	public bool Remove(T element)
	{
		return Dictionary_0.Remove(element.GetType());
	}

	public void Clear()
	{
		Dictionary_0.Clear();
	}
}

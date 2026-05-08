using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass1503<T>
{
	[NonSerialized]
	public List<T> List_0 = new List<T>();

	public bool TryGetRandomObject(T[] objects, out T obj)
	{
		obj = default(T);
		if (objects.Length == 0)
		{
			return false;
		}
		obj = method_0(objects);
		return obj != null;
	}

	public bool TryGetRandomObject(List<T> objects, out T obj)
	{
		obj = default(T);
		if (objects.Count == 0)
		{
			return false;
		}
		obj = method_0(objects);
		return obj != null;
	}

	public T method_0(IEnumerable<T> objects)
	{
		if (List_0.Count == 0)
		{
			method_1(objects);
		}
		int index = UnityEngine.Random.Range(0, List_0.Count - 1);
		T val = List_0[index];
		List_0.Remove(val);
		return val;
	}

	public void method_1(IEnumerable<T> objects)
	{
		List_0.AddRange(objects);
	}
}

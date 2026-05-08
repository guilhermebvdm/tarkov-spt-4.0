using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GClass422<T>
{
	public struct Struct8<U>(float pr, U d)
	{
		public readonly U data = d;

		public readonly float priority = pr;
	}

	[NonSerialized]
	public List<Struct8<T>> List_0 = new List<Struct8<T>>(256);

	public int Count => List_0.Count;

	public void Clear()
	{
		List_0.Clear();
	}

	public void Enqueue(float priority, T item)
	{
		int num = List_0.Count - 1;
		while (true)
		{
			if (num >= 0)
			{
				if (!(priority <= List_0[num].priority))
				{
					break;
				}
				num--;
				continue;
			}
			List_0.Insert(0, new Struct8<T>(priority, item));
			return;
		}
		List_0.Insert(num + 1, new Struct8<T>(priority, item));
	}

	public T Dequeue()
	{
		Struct8<T> @struct = List_0[List_0.Count - 1];
		List_0.RemoveAt(List_0.Count - 1);
		return @struct.data;
	}

	public void Log()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(Count:");
		stringBuilder.Append(List_0.Count);
		stringBuilder.Append(")");
		for (int i = 0; i < List_0.Count; i++)
		{
			stringBuilder.Append("[");
			stringBuilder.Append(List_0[i].priority);
			stringBuilder.Append("]");
		}
		Debug.LogError(stringBuilder);
	}
}

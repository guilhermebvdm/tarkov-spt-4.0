using System.Collections.Generic;
using UnityEngine;

public abstract class GClass1243
{
	public static void Shuffle<T>(this List<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = Random.Range(0, num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}
}

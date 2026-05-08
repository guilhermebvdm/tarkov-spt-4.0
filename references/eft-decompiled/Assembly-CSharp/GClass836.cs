using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass836
{
	[NonSerialized]
	public Queue<float> Queue_0;

	[NonSerialized]
	public float Float_0;

	public float Sum => Float_0;

	public GClass836(int capacity = 0)
	{
		Queue_0 = new Queue<float>(capacity);
	}

	public void Enqueue(float value)
	{
		Queue_0.Enqueue(value);
		Float_0 += value;
	}

	public float Dequeue()
	{
		float num = Queue_0.Dequeue();
		Float_0 -= num;
		return num;
	}

	public void DequeueCount(int count)
	{
		if (Queue_0.Count < count)
		{
			Debug.LogErrorFormat("Dequeuing '{0}' more than exists {1}", count, Queue_0.Count);
		}
		if (Queue_0.Count <= count)
		{
			Queue_0.Clear();
			return;
		}
		while (count-- > 0)
		{
			Dequeue();
		}
	}
}

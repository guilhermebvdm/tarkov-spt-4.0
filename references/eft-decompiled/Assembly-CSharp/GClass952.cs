using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass952
{
	public interface GInterface48
	{
		void Release();
	}

	public struct Struct145
	{
		public double time;

		public GInterface48 item;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class600
	{
		public static readonly Class600 class600_0 = new Class600();

		public int method_0(Struct145 a, Struct145 b)
		{
			return a.time.CompareTo(b.time);
		}
	}

	[CompilerGenerated]
	public class Class601
	{
		public GInterface48 queueItem;

		public bool method_0(Struct145 x)
		{
			return x.item == queueItem;
		}
	}

	[NonSerialized]
	public List<Struct145> List_0 = new List<Struct145>();

	[NonSerialized]
	public static IComparer<Struct145> Icomparer_0 = Comparer<Struct145>.Create((Struct145 a, Struct145 b) => a.time.CompareTo(b.time));

	public void Update()
	{
		Update(AudioSettings.dspTime);
	}

	public void Update(double time)
	{
		while (List_0.Count > 0 && !(List_0[0].time > time))
		{
			Struct145 @struct = List_0[0];
			List_0.RemoveAt(0);
			@struct.item.Release();
		}
	}

	public void Add(double time, double keyTime, GInterface48 queueItem)
	{
		Update(time);
		if (keyTime <= time)
		{
			queueItem.Release();
			return;
		}
		Struct145 item = new Struct145
		{
			time = keyTime,
			item = queueItem
		};
		int num = List_0.BinarySearch(item, Icomparer_0);
		if (num < 0)
		{
			num = ~num;
		}
		List_0.Insert(num, item);
	}

	public bool Remove(GInterface48 queueItem)
	{
		int num = List_0.FindIndex((Struct145 x) => x.item == queueItem);
		if (num >= 0)
		{
			List_0.RemoveAt(num);
			return true;
		}
		return false;
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass1159<T>
{
	public class Class797
	{
		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public int Int_1;

		[NonSerialized]
		public int Int_2;

		[NonSerialized]
		[CompilerGenerated]
		public Class797[] Class797_0 = Array.Empty<Class797>();

		[NonSerialized]
		[CompilerGenerated]
		public Bounds Bounds_0;

		[NonSerialized]
		[CompilerGenerated]
		public List<(T Item, Bounds ItemBounds)> List_0 = new List<(T, Bounds)>();

		public Class797[] Children
		{
			[CompilerGenerated]
			get
			{
				return Class797_0;
			}
			[CompilerGenerated]
			set
			{
				Class797_0 = value;
			}
		}

		public Bounds Bounds
		{
			[CompilerGenerated]
			get
			{
				return Bounds_0;
			}
		}

		public List<(T Item, Bounds ItemBounds)> Items
		{
			[CompilerGenerated]
			get
			{
				return List_0;
			}
		}

		public Class797(Bounds bounds, int maxItems, int maxDepth, int depth = 0)
		{
			Bounds_0 = bounds;
			Int_0 = maxItems;
			Int_1 = maxDepth;
			Int_2 = depth;
		}

		public void Insert(T item, Bounds itemBounds)
		{
			if (!Bounds.Intersects(itemBounds))
			{
				return;
			}
			if (Children.Length != 0)
			{
				Class797[] children = Children;
				for (int i = 0; i < children.Length; i++)
				{
					children[i].Insert(item, itemBounds);
				}
				return;
			}
			Items.Add((item, itemBounds));
			if (Items.Count <= Int_0 || Int_2 >= Int_1)
			{
				return;
			}
			method_0();
			foreach (var item4 in Items)
			{
				T item2 = item4.Item;
				Bounds item3 = item4.ItemBounds;
				Class797[] children = Children;
				for (int i = 0; i < children.Length; i++)
				{
					children[i].Insert(item2, item3);
				}
			}
			Items.Clear();
		}

		public void Query(Vector3 point, List<T> result)
		{
			if (!Bounds.Contains(point))
			{
				return;
			}
			foreach (var (item, bounds) in Items)
			{
				if (bounds.Contains(point))
				{
					result.Add(item);
				}
			}
			if (Children.Length != 0)
			{
				Class797[] children = Children;
				for (int i = 0; i < children.Length; i++)
				{
					children[i].Query(point, result);
				}
			}
		}

		public void Query(Bounds bounds, List<T> result)
		{
			if (!Bounds.Intersects(bounds))
			{
				return;
			}
			foreach (var (item, bounds2) in Items)
			{
				if (bounds2.Intersects(bounds))
				{
					result.Add(item);
				}
			}
			if (Children.Length != 0)
			{
				Class797[] children = Children;
				for (int i = 0; i < children.Length; i++)
				{
					children[i].Query(bounds, result);
				}
			}
		}

		public void method_0()
		{
			Children = new Class797[8];
			Vector3 size = Bounds.size * 0.5f;
			Vector3 center = Bounds.center;
			int num = 0;
			for (int i = -1; i <= 1; i += 2)
			{
				for (int j = -1; j <= 1; j += 2)
				{
					for (int k = -1; k <= 1; k += 2)
					{
						Vector3 vector = new Vector3((float)i * size.x * 0.5f, (float)j * size.y * 0.5f, (float)k * size.z * 0.5f);
						Bounds bounds = new Bounds(center + vector, size);
						Children[num++] = new Class797(bounds, Int_0, Int_1, Int_2 + 1);
					}
				}
			}
		}
	}

	[NonSerialized]
	public Class797 Class797_0;

	public GClass1159(Bounds worldBounds, int maxItemsPerNode = 8, int maxDepth = 6)
	{
		Class797_0 = new Class797(worldBounds, maxItemsPerNode, maxDepth);
	}

	public void Insert(T item, Bounds itemBounds)
	{
		Class797_0.Insert(item, itemBounds);
	}

	public void Query(Vector3 point, List<T> result)
	{
		result.Clear();
		Class797_0.Query(point, result);
	}

	public List<T> Query(Vector3 point)
	{
		List<T> result = new List<T>();
		Query(point, result);
		return result;
	}

	public void Query(Bounds bounds, List<T> result)
	{
		result.Clear();
		Class797_0.Query(bounds, result);
	}

	public List<T> Query(Bounds bounds)
	{
		List<T> result = new List<T>();
		Query(bounds, result);
		return result;
	}
}

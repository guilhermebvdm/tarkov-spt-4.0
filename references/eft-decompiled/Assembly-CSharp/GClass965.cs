using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass965<T, U> where T : class where U : GClass962<T>
{
	[NonSerialized]
	public Rect Rect_0;

	[NonSerialized]
	public float Float_0 = 2f;

	[NonSerialized]
	public GClass962<T> Gclass962_0;

	[NonSerialized]
	public IDictionary<T, GClass962<T>> Idictionary_0;

	public Rect Bounds
	{
		get
		{
			return Rect_0;
		}
		set
		{
			Rect_0 = value;
			method_1();
		}
	}

	public float SizeLimit
	{
		get
		{
			return Float_0;
		}
		set
		{
			Float_0 = value;
			method_1();
		}
	}

	public virtual GClass962<T> CreateQuadrant(GClass962<T> parent, Rect bounds)
	{
		return new GClass962<T>(parent, bounds);
	}

	public void Insert(T nodeData, Rect bounds)
	{
		if (Rect_0.width != 0f && Rect_0.height != 0f)
		{
			if (bounds.width != 0f && bounds.height != 0f)
			{
				if (Gclass962_0 == null)
				{
					Gclass962_0 = CreateQuadrant(null, Rect_0);
				}
				GClass962<T> value = Gclass962_0.method_0(nodeData, bounds, Float_0);
				if (Idictionary_0 == null)
				{
					Idictionary_0 = new Dictionary<T, GClass962<T>>();
				}
				Idictionary_0[nodeData] = value;
				return;
			}
			throw new ArgumentException("Bounds width or height is zero");
		}
		throw new ArgumentException("Own bounds width or height is zero");
	}

	public IEnumerable<T> GetNodesInside(Rect bounds)
	{
		foreach (GClass961<T> item in method_0(bounds))
		{
			yield return item.Node;
		}
	}

	public bool HasNodesInside(Rect bounds)
	{
		if (Gclass962_0 == null)
		{
			return false;
		}
		return Gclass962_0.method_2(bounds);
	}

	public IEnumerable<GClass961<T>> method_0(Rect bounds)
	{
		List<GClass961<T>> list = new List<GClass961<T>>();
		if (Gclass962_0 != null)
		{
			Gclass962_0.method_1(list, bounds);
		}
		return list;
	}

	public bool Remove(T node)
	{
		if (Idictionary_0 != null && Idictionary_0.TryGetValue(node, out var value))
		{
			value.method_3(node);
			Idictionary_0.Remove(node);
			return true;
		}
		return false;
	}

	public void method_1()
	{
		Gclass962_0 = null;
		foreach (GClass961<T> item in method_0(Rect_0))
		{
			Insert(item.Node, item.Bounds);
		}
	}
}

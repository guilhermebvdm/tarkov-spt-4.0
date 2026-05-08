using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass962<T> where T : class
{
	[NonSerialized]
	public GClass962<T> Gclass962_0;

	[NonSerialized]
	public Rect Rect_0_1;

	[NonSerialized]
	public List<GClass961<T>> List_0;

	[NonSerialized]
	public GClass962<T> Gclass962_1;

	[NonSerialized]
	public GClass962<T> Gclass962_2;

	[NonSerialized]
	public GClass962<T> Gclass962_3;

	[NonSerialized]
	public GClass962<T> Gclass962_4;

	public GClass962<T> GClass962_0 => Gclass962_0;

	public Rect Rect_0 => Rect_0_1;

	public virtual GClass962<T> CreateQuadrant(GClass962<T> parent, Rect bounds)
	{
		return new GClass962<T>(parent, bounds);
	}

	public GClass962(GClass962<T> parent, Rect bounds)
	{
		Gclass962_0 = parent;
		if (bounds.width != 0f && bounds.height != 0f)
		{
			Rect_0_1 = bounds;
		}
		else
		{
			Debug.LogError("Width or height is zero");
		}
	}

	public List<T> GetDataList()
	{
		if (List_0 == null)
		{
			return null;
		}
		List<T> list = new List<T>();
		foreach (GClass961<T> item in List_0)
		{
			list.Add(item.Node);
		}
		return list;
	}

	public GClass962<T> method_0(T node, Rect bounds, float sizeLimit)
	{
		if (bounds.width != 0f && bounds.height != 0f)
		{
			GClass962<T> gClass = this;
			while (true)
			{
				float num = gClass.Rect_0_1.width / 2f;
				if (num < 1f)
				{
					num = 1f;
				}
				float num2 = gClass.Rect_0_1.height / 2f;
				if (num2 < 1f)
				{
					num2 = 1f;
				}
				Rect rect = new Rect(gClass.Rect_0_1.xMin, gClass.Rect_0_1.yMin, num, num2);
				Rect rect2 = new Rect(gClass.Rect_0_1.xMin + num, gClass.Rect_0_1.yMin, num, num2);
				Rect rect3 = new Rect(gClass.Rect_0_1.xMin, gClass.Rect_0_1.yMin + num2, num, num2);
				Rect rect4 = new Rect(gClass.Rect_0_1.xMin + num, gClass.Rect_0_1.yMin + num2, num, num2);
				GClass962<T> gClass2 = null;
				if (gClass.Rect_0_1.width > sizeLimit && gClass.Rect_0_1.height > sizeLimit)
				{
					if (GClass960.Contains(rect, bounds))
					{
						if (gClass.Gclass962_1 == null)
						{
							gClass.Gclass962_1 = CreateQuadrant(gClass, rect);
						}
						gClass2 = gClass.Gclass962_1;
					}
					else if (GClass960.Contains(rect2, bounds))
					{
						if (gClass.Gclass962_2 == null)
						{
							gClass.Gclass962_2 = CreateQuadrant(gClass, rect2);
						}
						gClass2 = gClass.Gclass962_2;
					}
					else if (GClass960.Contains(rect3, bounds))
					{
						if (gClass.Gclass962_3 == null)
						{
							gClass.Gclass962_3 = CreateQuadrant(gClass, rect3);
						}
						gClass2 = gClass.Gclass962_3;
					}
					else if (GClass960.Contains(rect4, bounds))
					{
						if (gClass.Gclass962_4 == null)
						{
							gClass.Gclass962_4 = CreateQuadrant(gClass, rect4);
						}
						gClass2 = gClass.Gclass962_4;
					}
				}
				if (gClass2 == null)
				{
					break;
				}
				gClass = gClass2;
			}
			GClass961<T> item = new GClass961<T>(node, bounds);
			if (gClass.List_0 == null)
			{
				gClass.List_0 = new List<GClass961<T>>();
			}
			gClass.List_0.Add(item);
			return gClass;
		}
		throw new ArgumentException("Can not have bounds with zero width or height");
	}

	public void method_1(List<GClass961<T>> nodes, Rect bounds)
	{
		if (!GClass960.IsEmpty(bounds))
		{
			float num = Rect_0_1.width / 2f;
			float num2 = Rect_0_1.height / 2f;
			Rect rect = new Rect(Rect_0_1.xMin, Rect_0_1.yMin, num, num2);
			Rect rect2 = new Rect(Rect_0_1.xMin + num, Rect_0_1.yMin, num, num2);
			Rect rect3 = new Rect(Rect_0_1.xMin, Rect_0_1.yMin + num2, num, num2);
			Rect rect4 = new Rect(Rect_0_1.xMin + num, Rect_0_1.yMin + num2, num, num2);
			if (rect.Overlaps(bounds) && Gclass962_1 != null)
			{
				Gclass962_1.method_1(nodes, bounds);
			}
			if (rect2.Overlaps(bounds) && Gclass962_2 != null)
			{
				Gclass962_2.method_1(nodes, bounds);
			}
			if (rect3.Overlaps(bounds) && Gclass962_3 != null)
			{
				Gclass962_3.method_1(nodes, bounds);
			}
			if (rect4.Overlaps(bounds) && Gclass962_4 != null)
			{
				Gclass962_4.method_1(nodes, bounds);
			}
			smethod_0(List_0, nodes, bounds);
		}
	}

	public static void smethod_0(List<GClass961<T>> last, List<GClass961<T>> nodes, Rect bounds)
	{
		if (last == null)
		{
			return;
		}
		foreach (GClass961<T> item in last)
		{
			if (item.Bounds.Overlaps(bounds))
			{
				nodes.Add(item);
			}
		}
	}

	public bool method_2(Rect bounds)
	{
		if (GClass960.IsEmpty(bounds))
		{
			return false;
		}
		float num = Rect_0_1.width / 2f;
		float num2 = Rect_0_1.height / 2f;
		Rect rect = new Rect(Rect_0_1.xMin, Rect_0_1.yMin, num, num2);
		Rect rect2 = new Rect(Rect_0_1.xMin + num, Rect_0_1.yMin, num, num2);
		Rect rect3 = new Rect(Rect_0_1.xMin, Rect_0_1.yMin + num2, num, num2);
		Rect rect4 = new Rect(Rect_0_1.xMin + num, Rect_0_1.yMin + num2, num, num2);
		bool flag = false;
		if (rect.Overlaps(bounds) && Gclass962_1 != null)
		{
			flag = Gclass962_1.method_2(bounds);
		}
		if (!flag && rect2.Overlaps(bounds) && Gclass962_2 != null)
		{
			flag = Gclass962_2.method_2(bounds);
		}
		if (!flag && rect3.Overlaps(bounds) && Gclass962_3 != null)
		{
			flag = Gclass962_3.method_2(bounds);
		}
		if (!flag && rect4.Overlaps(bounds) && Gclass962_4 != null)
		{
			flag = Gclass962_4.method_2(bounds);
		}
		if (!flag)
		{
			flag = smethod_1(List_0, bounds);
		}
		return flag;
	}

	public static bool smethod_1(List<GClass961<T>> last, Rect bounds)
	{
		if (last != null)
		{
			foreach (GClass961<T> item in last)
			{
				if (item.Bounds.Overlaps(bounds))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool method_3(T node)
	{
		bool result = false;
		if (List_0 != null)
		{
			int num = 0;
			foreach (GClass961<T> item in List_0)
			{
				if (item.Node != node)
				{
					num++;
					continue;
				}
				result = true;
				List_0.RemoveAt(num);
				break;
			}
		}
		return result;
	}
}

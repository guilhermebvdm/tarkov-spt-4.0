using System;
using UnityEngine;

public class GClass961<T> where T : class
{
	[NonSerialized]
	public Rect Rect_0;

	[NonSerialized]
	public T Gparam_0;

	public T Node
	{
		get
		{
			return Gparam_0;
		}
		set
		{
			Gparam_0 = value;
		}
	}

	public Rect Bounds => Rect_0;

	public GClass961(T node, Rect bounds)
	{
		Gparam_0 = node;
		Rect_0 = bounds;
	}
}

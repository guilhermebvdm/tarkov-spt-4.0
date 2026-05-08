using System;
using UnityEngine;

public class Class686
{
	public enum XAlignment
	{
		Left,
		Center,
		Right
	}

	public enum YAlignment
	{
		Top,
		Center,
		Bottom
	}

	[NonSerialized]
	public GUIStyle Guistyle_0;

	[NonSerialized]
	public XAlignment Xalignment_0;

	[NonSerialized]
	public YAlignment Yalignment_0;

	[NonSerialized]
	public Vector2 Vector2_0;

	[NonSerialized]
	public Vector2 Vector2_1;

	[NonSerialized]
	public Rect Rect_0;

	[NonSerialized]
	public bool Bool_0;

	public string Text;

	public TextAnchor TextAnchor
	{
		get
		{
			return Guistyle_0.alignment;
		}
		set
		{
			Guistyle_0.alignment = value;
		}
	}

	public Class686(XAlignment xAlig, YAlignment yAlig, Vector2 pos, Vector2 size, bool relatively = false)
	{
		Xalignment_0 = xAlig;
		Yalignment_0 = yAlig;
		Vector2_0 = pos;
		Vector2_1 = size;
		Bool_0 = relatively;
		GClass989.OnScreenChange += method_0;
		method_0();
		if (Guistyle_0 == null)
		{
			Guistyle_0 = new GUIStyle();
			Guistyle_0.alignment = TextAnchor.MiddleCenter;
			Guistyle_0.normal.textColor = Color.white;
		}
	}

	public void method_0()
	{
		Vector2 size = GClass989.Size;
		float num = Vector2_0.x;
		float num2 = Vector2_0.y;
		if (Bool_0)
		{
			num *= size.x;
			num2 *= size.y;
		}
		switch (Xalignment_0)
		{
		case XAlignment.Right:
			num = size.x - num;
			break;
		case XAlignment.Center:
			num = (size.x - Vector2_1.x) * 0.5f + num;
			break;
		}
		switch (Yalignment_0)
		{
		case YAlignment.Bottom:
			num2 = size.y - num2;
			break;
		case YAlignment.Center:
			num2 = (size.y - Vector2_1.y) * 0.5f + num2;
			break;
		}
		Rect_0 = new Rect(num, num2, Vector2_1.x, Vector2_1.y);
	}

	public void Draw()
	{
		GUI.Label(Rect_0, Text, Guistyle_0);
	}
}

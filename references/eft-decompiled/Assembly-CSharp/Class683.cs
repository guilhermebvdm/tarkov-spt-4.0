using System;
using UnityEngine;

public class Class683
{
	public Texture Texture;

	[NonSerialized]
	public Rect Rect_0;

	public Class683(Texture texture)
	{
		Texture = texture;
		Rect_0 = GClass989.Rect;
		GClass989.OnScreenChange += method_0;
	}

	public void method_0()
	{
		Rect_0 = GClass989.Rect;
	}

	public void Draw()
	{
		GUI.DrawTexture(Rect_0, Texture);
	}

	public void Draw(float alpha)
	{
		Color color = GUI.color;
		GUI.color = new Color(0f, 0f, 0f, alpha);
		GUI.DrawTexture(Rect_0, Texture);
		GUI.color = color;
	}

	public void DrawGL(Color color)
	{
	}
}

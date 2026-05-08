using System;
using UnityEngine;

public class Class682
{
	[NonSerialized]
	public Material Material_0;

	[NonSerialized]
	public Texture Texture_0;

	[NonSerialized]
	public Rect Rect_0;

	[NonSerialized]
	public Rect Rect_1;

	public bool HorAlign;

	public Texture Texture
	{
		get
		{
			return Texture_0;
		}
		set
		{
			if (!(value == Texture_0))
			{
				Texture_0 = value;
				method_1();
			}
		}
	}

	public Class682(Texture texture, bool horAlign = false)
	{
		HorAlign = horAlign;
		Texture_0 = texture;
		Rect_0 = GClass989.Rect;
		GClass989.OnScreenChange += method_0;
		method_1();
	}

	public void method_0()
	{
		Rect_0 = GClass989.Rect;
		method_1();
	}

	public void method_1()
	{
		float num = Rect_0.width / (float)Texture_0.width;
		float num2 = Rect_0.height / (float)Texture_0.height;
		if (HorAlign)
		{
			float num3 = num / num2;
			Rect_1 = new Rect((1f - num3) * 0.5f, 0f, num3, 1f);
		}
		else
		{
			float num4 = num2 / num;
			Rect_1 = new Rect(0f, (1f - num4) * 0.5f, 1f, num4);
		}
	}

	public void Draw()
	{
		GUI.DrawTextureWithTexCoords(Rect_0, Texture_0, Rect_1);
	}
}

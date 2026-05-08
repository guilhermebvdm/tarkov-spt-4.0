using System;
using UnityEngine;

public class Class685
{
	[NonSerialized]
	public Texture Texture_0;

	[NonSerialized]
	public Texture Texture_1;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public Rect Rect_0;

	[NonSerialized]
	public Rect Rect_1;

	[NonSerialized]
	public Rect Rect_2;

	[NonSerialized]
	public Rect Rect_3;

	[NonSerialized]
	public Vector2 Vector2_0;

	public Texture BackGround
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

	public Texture ForeGround
	{
		get
		{
			return Texture_1;
		}
		set
		{
			if (!(value == Texture_1))
			{
				Texture_1 = value;
				method_1();
			}
		}
	}

	public float Value
	{
		get
		{
			return Float_0;
		}
		set
		{
			if (value != Float_0)
			{
				Float_0 = value;
				Rect_3.width = Float_0;
				Rect_2.width = Rect_1.width * Float_0;
			}
		}
	}

	public Class685(Texture backGround, Texture foreGround, Vector2 scale)
	{
		Texture_0 = backGround;
		Texture_1 = foreGround;
		Rect_0 = GClass989.Rect;
		GClass989.OnScreenChange += method_0;
		Vector2_0 = scale;
		method_1();
		Rect_3 = new Rect(0f, 0f, 1f, 1f);
	}

	public void method_0()
	{
		Rect_0 = GClass989.Rect;
		method_1();
	}

	public void method_1()
	{
		Rect_1 = new Rect((Rect_0.width - (float)Texture_0.width * Vector2_0.x) * 0.5f, Rect_0.height - 200f, (float)Texture_0.width * Vector2_0.x, (float)Texture_0.height * Vector2_0.y);
		Rect_2 = Rect_1;
	}

	public void Draw()
	{
		GUI.DrawTexture(Rect_1, Texture_0);
		GUI.DrawTextureWithTexCoords(Rect_2, Texture_1, Rect_3);
	}
}

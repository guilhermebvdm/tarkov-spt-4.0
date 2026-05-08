using System;
using UnityEngine;

public class Class684
{
	[NonSerialized]
	public Texture Texture_0;

	[NonSerialized]
	public Rect Rect_0;

	[NonSerialized]
	public Rect Rect_1;

	[NonSerialized]
	public float Float_0;

	public bool HorTile;

	public bool VerTile;

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

	public Class684(Texture texture, float scale, bool horTile = false, bool verTile = false)
	{
		HorTile = horTile;
		VerTile = verTile;
		Float_0 = scale;
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
		float height = (VerTile ? (Rect_0.height / ((float)Texture_0.height * Float_0)) : 1f);
		float width = (HorTile ? (Rect_0.width / ((float)Texture_0.width * Float_0)) : 1f);
		Rect_1 = new Rect(0f, 0f, width, height);
	}

	public void Draw()
	{
		GUI.DrawTextureWithTexCoords(Rect_0, Texture_0, Rect_1);
	}
}

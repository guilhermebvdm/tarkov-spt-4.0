using System;
using UnityEngine;

public class GClass938
{
	public class Class589
	{
		public Color color;

		public float val;

		[NonSerialized]
		public int Int_0;

		public Class589(Color color)
		{
			this.color = color;
		}

		public void Drawe(Color[] colors, GClass938 G)
		{
			int num = G.method_0(val);
			int num2;
			int num3;
			if (num > Int_0)
			{
				num2 = Int_0;
				num3 = num;
			}
			else
			{
				num2 = num;
				num3 = Int_0;
			}
			for (int i = num2; i <= num3; i++)
			{
				colors[i] = color;
			}
			Int_0 = num;
		}
	}

	public Color BackGround = Color.white;

	public Color CenterLine = Color.black;

	public float Scale;

	public Rect Rect;

	public bool DrawCenterLine = true;

	[NonSerialized]
	public Texture2D Texture2D_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public Class589[] Class589_0;

	[NonSerialized]
	public bool Bool_0;

	public float this[int index]
	{
		get
		{
			return Class589_0[index].val;
		}
		set
		{
			Bool_0 = true;
			Class589_0[index].val = value;
		}
	}

	public GClass938(Rect rect, float scale, params Color[] colors)
	{
		Rect = rect;
		Int_1 = (int)rect.height;
		Int_3 = (int)rect.width;
		Int_2 = (int)(rect.height / 2f);
		Scale = scale;
		Texture2D_0 = new Texture2D((int)rect.width, (int)rect.height);
		Texture2D_0.name = "DebugGraph";
		if (colors.Length != 0)
		{
			Class589_0 = new Class589[colors.Length];
			for (int i = 0; i < Class589_0.Length; i++)
			{
				Class589_0[i] = new Class589(colors[i]);
			}
		}
	}

	public void OnGUI()
	{
		if (Bool_0)
		{
			Update();
			Bool_0 = false;
		}
		GUI.DrawTexture(Rect, Texture2D_0);
	}

	public int method_0(float value)
	{
		int num = (int)(value * Scale) + Int_2;
		if (num >= Int_1)
		{
			num = Int_1 - 1;
		}
		if (num < 0)
		{
			num = 0;
		}
		return num;
	}

	public void Update()
	{
		Color[] array = new Color[Int_1];
		for (int i = 0; i < Int_1; i++)
		{
			array[i] = BackGround;
		}
		if (DrawCenterLine)
		{
			array[Int_2] = CenterLine;
		}
		for (int j = 0; j < Class589_0.Length; j++)
		{
			Class589_0[j].Drawe(array, this);
		}
		Texture2D_0.SetPixels(Int_0, 0, 1, Int_1, array);
		Texture2D_0.Apply();
		Int_0++;
		if (Int_0 >= Int_3)
		{
			Int_0 = 0;
		}
	}
}

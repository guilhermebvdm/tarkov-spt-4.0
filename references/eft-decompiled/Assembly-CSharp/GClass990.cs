using System;
using UnityEngine;

public abstract class GClass990
{
	public static void FuncGraph(ref Texture2D texture, ref Color32[] colors, Func<float, float> func, Vector2 gSize, int width, int height, float start, float softness)
	{
		if (texture == null)
		{
			texture = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false);
			texture.name = "OlegUtils";
			colors = texture.GetPixels32();
		}
		Color32 color = new Color32(0, 0, 0, byte.MaxValue);
		Color32 color2 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = color;
		}
		float num = gSize.x / (float)width;
		float num2 = 1f / gSize.y;
		for (int j = 0; j < width; j++)
		{
			float arg = (float)j * num;
			float num3 = Mathf.Clamp01(func(arg) * num2);
			colors[j + width * (int)(num3 * (float)(height - 1))] = color2;
		}
		texture.SetPixels32(colors);
		texture.Apply();
	}

	public static Texture2D GetTexture(int width, int heigth, params Color[] colors)
	{
		Texture2D texture2D = new Texture2D(width, heigth, TextureFormat.RGBA32, mipChain: false);
		texture2D.name = "OlegUtils";
		texture2D.SetPixels(colors);
		texture2D.filterMode = FilterMode.Point;
		texture2D.Apply();
		return texture2D;
	}

	public static Color[] GetColors(Texture texture)
	{
		return ((Texture2D)texture).GetPixels();
	}

	public static Texture GetTexture(Color[] colors)
	{
		int num = (int)Mathf.Sqrt(colors.Length);
		Texture2D texture2D = new Texture2D(num, num);
		texture2D.name = "OlegUtils";
		texture2D.SetPixels(colors);
		texture2D.Apply();
		return texture2D;
	}

	public static Color[] ColorSelect(Color[] colors, Color color, float contrast)
	{
		float num = color.r / color.g;
		float num2 = color.r / color.b;
		float num3 = color.r + color.g + color.b;
		for (int i = 0; i < colors.Length; i++)
		{
			Color color2 = colors[i];
			float num4 = num - color2.r / color2.g;
			float num5 = num2 - color2.r / color2.b;
			float num6 = num3 - (color2.r + color2.g + color2.b);
			if (num4 < 0f)
			{
				num4 = 0f - num4;
			}
			if (num5 < 0f)
			{
				num5 = 0f - num5;
			}
			if (num6 < 0f)
			{
				num6 = 0f - num5;
			}
			float num7 = 1f - (num4 + num5 + num6) * contrast;
			colors[i] = new Color(num7, num7, num7, 1f);
		}
		return colors;
	}

	public static Color[] GetMipMap(Color[] inColors, int level)
	{
		int num = 1 << level;
		int num2 = num * num;
		int num3 = (int)Mathf.Sqrt(inColors.Length) >> level;
		Color[] array = new Color[num3 * num3];
		float num4 = 1f / (float)num2;
		int i = 0;
		int num5 = -1;
		int num6 = -1;
		for (; i < inColors.Length; i++)
		{
			if (i % num == 0)
			{
				num5++;
				if (num5 % num3 == 0)
				{
					num6++;
					if (num6 % num != 0)
					{
						num5 -= num3;
					}
				}
			}
			array[num5].r += inColors[i].r * num4;
			array[num5].g += inColors[i].g * num4;
			array[num5].b += inColors[i].b * num4;
		}
		return array;
	}
}

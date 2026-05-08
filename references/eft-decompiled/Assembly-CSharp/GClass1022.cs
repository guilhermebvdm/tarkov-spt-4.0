using UnityEngine;

public abstract class GClass1022
{
	public static Texture2D GetTexture(int width, int heigth, params Color[] colors)
	{
		Texture2D texture2D = new Texture2D(width, heigth, TextureFormat.RGBA32, mipChain: false);
		texture2D.name = "TextureHelper";
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
		texture2D.name = "TextureHelper";
		texture2D.SetPixels(colors);
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D GetRadialTexture(int width, int height, Color color1, Color color2, AnimationCurve curve)
	{
		Texture2D texture2D = new Texture2D(width, height);
		texture2D.name = "TextureHelper";
		float num = width / 2;
		float num2 = height / 2;
		float num3 = 1f / num;
		float num4 = -1f;
		float num5 = 1f / num2;
		float num6 = -1f;
		Color[] array = new Color[width * height];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				num4 += num3;
				float num7 = curve.Evaluate(new Vector2(num4, num6).magnitude);
				array[width * i + j] = Color.Lerp(color1, color2, 1f - num7);
			}
			num4 = -1f;
			num6 += num5;
		}
		texture2D.SetPixels(array);
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

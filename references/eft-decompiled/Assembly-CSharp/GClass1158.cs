using UnityEngine;

public abstract class GClass1158
{
	public static Color GetRandomColor()
	{
		return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.5f);
	}

	public static Texture2D MakeTex(int width, int height, Color col)
	{
		Color[] array = new Color[width * height];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = col;
		}
		Texture2D texture2D = new Texture2D(width, height);
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	public static GUIStyle CreateStyle(Color backgroundColor, Color textColor, GUIStyle other, TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle fontStyle = FontStyle.Normal, float width = 0f)
	{
		GUIStyle gUIStyle = new GUIStyle(other)
		{
			fontStyle = fontStyle,
			alignment = alignment,
			normal = 
			{
				textColor = textColor,
				background = MakeTex(2, 3, backgroundColor)
			},
			fontSize = 12,
			fixedHeight = 20f
		};
		if (width > 0f)
		{
			gUIStyle.fixedWidth = width;
		}
		return gUIStyle;
	}
}

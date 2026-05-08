using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace GPUInstancer;

public class ColorPicker : MonoBehaviour
{
	[CompilerGenerated]
	public class Class872
	{
		public Texture2D satvalTex;

		public Color[] satvalColors;

		public float Hue;

		public Color[] hueColors;

		public Action resetSatValTexture;

		public float Saturation;

		public float Value;

		public GameObject result;

		public ColorPicker colorPicker_0;

		public GameObject hueGO;

		public Action dragH;

		public GameObject satvalGO;

		public Action dragSV;

		public Vector2 hueSz;

		public Action applyHue;

		public Action applySaturationValue;

		public GameObject hueKnob;

		public Action idle;

		public Vector2 satvalSz;

		public GameObject satvalKnob;

		public void method_0()
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					satvalTex.SetPixel(j, i, satvalColors[j + i * 2]);
				}
			}
			satvalTex.Apply();
		}

		public void method_1()
		{
			int num = Mathf.Clamp((int)Hue, 0, 5);
			int num2 = (num + 1) % 6;
			Color color = Color.Lerp(hueColors[num], hueColors[num2], Hue - (float)num);
			satvalColors[3] = color;
			resetSatValTexture();
		}

		public void method_2()
		{
			Vector2 vector = new Vector2(Saturation, Value);
			Vector2 vector2 = new Vector2(1f - vector.x, 1f - vector.y);
			Color color = vector2.x * vector2.y * satvalColors[0];
			Color color2 = vector.x * vector2.y * satvalColors[1];
			Color color3 = vector2.x * vector.y * satvalColors[2];
			Color color4 = vector.x * vector.y * satvalColors[3];
			Color color5 = color + color2 + color3 + color4;
			result.GetComponent<Image>().color = color5;
			if (colorPicker_0.color_0 != color5)
			{
				if (colorPicker_0.action_0 != null)
				{
					colorPicker_0.action_0(color5);
				}
				if (colorPicker_0.action_1 != null)
				{
					colorPicker_0.action_1();
				}
				colorPicker_0.color_0 = color5;
			}
		}

		public void method_3()
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (smethod_1(hueGO, out var vector))
				{
					colorPicker_0.action_2 = dragH;
				}
				else if (smethod_1(satvalGO, out vector))
				{
					colorPicker_0.action_2 = dragSV;
				}
			}
		}

		public void method_4()
		{
			smethod_1(hueGO, out var vector);
			Hue = vector.y / hueSz.y * 6f;
			applyHue();
			applySaturationValue();
			hueKnob.transform.localPosition = new Vector2(hueKnob.transform.localPosition.x, vector.y);
			if (Input.GetMouseButtonUp(0))
			{
				colorPicker_0.action_2 = idle;
			}
		}

		public void method_5()
		{
			smethod_1(satvalGO, out var vector);
			Saturation = vector.x / satvalSz.x;
			Value = vector.y / satvalSz.y;
			applySaturationValue();
			satvalKnob.transform.localPosition = vector;
			if (Input.GetMouseButtonUp(0))
			{
				colorPicker_0.action_2 = idle;
			}
		}
	}

	private Color color_0 = Color.red;

	private Action<Color> action_0;

	private Action action_1;

	private Action action_2;

	public Color Color
	{
		get
		{
			return color_0;
		}
		set
		{
			method_1(value);
		}
	}

	public void SetOnValueChangeCallback(Action<Color> onValueChange)
	{
		action_0 = onValueChange;
	}

	public void SetOnValueChangeCallback(Action onValueChange)
	{
		action_1 = onValueChange;
	}

	public static void smethod_0(Color color, out float h, out float s, out float v)
	{
		float num = Mathf.Min(color.r, color.g, color.b);
		float num2 = Mathf.Max(color.r, color.g, color.b);
		float num3 = num2 - num;
		if (num3 == 0f)
		{
			h = 0f;
		}
		else if (num2 == color.r)
		{
			h = Mathf.Repeat((color.g - color.b) / num3, 6f);
		}
		else if (num2 == color.g)
		{
			h = (color.b - color.r) / num3 + 2f;
		}
		else
		{
			h = (color.r - color.g) / num3 + 4f;
		}
		s = ((num2 == 0f) ? 0f : (num3 / num2));
		v = num2;
	}

	public static bool smethod_1(GameObject go, out Vector2 result)
	{
		RectTransform rectTransform = (RectTransform)go.transform;
		Vector3 point = rectTransform.InverseTransformPoint(Input.mousePosition);
		result.x = Mathf.Clamp(point.x, rectTransform.rect.min.x, rectTransform.rect.max.x);
		result.y = Mathf.Clamp(point.y, rectTransform.rect.min.y, rectTransform.rect.max.y);
		return rectTransform.rect.Contains(point);
	}

	public static Vector2 smethod_2(GameObject go)
	{
		return ((RectTransform)go.transform).rect.size;
	}

	public GameObject method_0(string name)
	{
		return base.transform.Find(name).gameObject;
	}

	public void method_1(Color inputColor)
	{
		GameObject satvalGO = method_0("SaturationValue");
		GameObject satvalKnob = method_0("SaturationValue/Knob");
		GameObject hueGO = method_0("Hue");
		GameObject hueKnob = method_0("Hue/Knob");
		GameObject result = method_0("Result");
		Color[] hueColors = new Color[6]
		{
			Color.red,
			Color.yellow,
			Color.green,
			Color.cyan,
			Color.blue,
			Color.magenta
		};
		Color[] satvalColors = new Color[4]
		{
			new Color(0f, 0f, 0f),
			new Color(0f, 0f, 0f),
			new Color(1f, 1f, 1f),
			hueColors[0]
		};
		Texture2D texture2D = new Texture2D(1, 7);
		for (int i = 0; i < 7; i++)
		{
			texture2D.SetPixel(0, i, hueColors[i % 6]);
		}
		texture2D.Apply();
		hueGO.GetComponent<Image>().sprite = Sprite.Create(texture2D, new Rect(0f, 0.5f, 1f, 6f), new Vector2(0.5f, 0.5f));
		Vector2 hueSz = smethod_2(hueGO);
		Texture2D satvalTex = new Texture2D(2, 2);
		satvalGO.GetComponent<Image>().sprite = Sprite.Create(satvalTex, new Rect(0.5f, 0.5f, 1f, 1f), new Vector2(0.5f, 0.5f));
		Action resetSatValTexture = delegate
		{
			for (int j = 0; j < 2; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					satvalTex.SetPixel(k, j, satvalColors[k + j * 2]);
				}
			}
			satvalTex.Apply();
		};
		Vector2 satvalSz = smethod_2(satvalGO);
		smethod_0(inputColor, out var Hue, out var Saturation, out var Value);
		Action applyHue = delegate
		{
			int num = Mathf.Clamp((int)Hue, 0, 5);
			int num2 = (num + 1) % 6;
			Color color = Color.Lerp(hueColors[num], hueColors[num2], Hue - (float)num);
			satvalColors[3] = color;
			resetSatValTexture();
		};
		Action applySaturationValue = delegate
		{
			Vector2 vector = new Vector2(Saturation, Value);
			Vector2 vector2 = new Vector2(1f - vector.x, 1f - vector.y);
			Color color = vector2.x * vector2.y * satvalColors[0];
			Color color2 = vector.x * vector2.y * satvalColors[1];
			Color color3 = vector2.x * vector.y * satvalColors[2];
			Color color4 = vector.x * vector.y * satvalColors[3];
			Color color5 = color + color2 + color3 + color4;
			result.GetComponent<Image>().color = color5;
			if (color_0 != color5)
			{
				if (action_0 != null)
				{
					action_0(color5);
				}
				if (action_1 != null)
				{
					action_1();
				}
				color_0 = color5;
			}
		};
		applyHue();
		applySaturationValue();
		satvalKnob.transform.localPosition = new Vector2(Saturation * satvalSz.x, Value * satvalSz.y);
		hueKnob.transform.localPosition = new Vector2(hueKnob.transform.localPosition.x, Hue / 6f * satvalSz.y);
		Action dragH = null;
		Action dragSV = null;
		Action idle = delegate
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (smethod_1(hueGO, out var result2))
				{
					action_2 = dragH;
				}
				else if (smethod_1(satvalGO, out result2))
				{
					action_2 = dragSV;
				}
			}
		};
		dragH = delegate
		{
			smethod_1(hueGO, out var result2);
			Hue = result2.y / hueSz.y * 6f;
			applyHue();
			applySaturationValue();
			hueKnob.transform.localPosition = new Vector2(hueKnob.transform.localPosition.x, result2.y);
			if (Input.GetMouseButtonUp(0))
			{
				action_2 = idle;
			}
		};
		dragSV = delegate
		{
			smethod_1(satvalGO, out var result2);
			Saturation = result2.x / satvalSz.x;
			Value = result2.y / satvalSz.y;
			applySaturationValue();
			satvalKnob.transform.localPosition = result2;
			if (Input.GetMouseButtonUp(0))
			{
				action_2 = idle;
			}
		};
		action_2 = idle;
	}

	public void SetRandomColor()
	{
		System.Random random = new System.Random();
		float r = (float)(random.Next() % 1000) / 1000f;
		float g = (float)(random.Next() % 1000) / 1000f;
		float b = (float)(random.Next() % 1000) / 1000f;
		Color = new Color(r, g, b);
	}

	public void Awake()
	{
		Color = Color.red;
	}

	public void Update()
	{
		action_2();
	}
}

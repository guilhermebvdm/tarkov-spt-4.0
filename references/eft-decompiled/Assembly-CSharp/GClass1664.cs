using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ChartAndGraph;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class GClass1664
{
	public class Class1071 : IEqualityComparer<int>
	{
		public bool Equals(int x, int y)
		{
			return x == y;
		}

		public int GetHashCode(int obj)
		{
			return obj.GetHashCode();
		}
	}

	[NonSerialized]
	public static Material Material_0_1;

	[NonSerialized]
	[CompilerGenerated]
	public static IEqualityComparer<int> IequalityComparer_0;

	public static Material Material_0
	{
		get
		{
			if (Material_0_1 == null)
			{
				Material_0_1 = new Material(GClass872.Find("Standard"));
				Material_0_1.color = Color.blue;
			}
			return Material_0_1;
		}
	}

	public static bool Boolean_0
	{
		get
		{
			if (!Application.isPlaying)
			{
				return Application.isEditor;
			}
			return false;
		}
	}

	public static IEqualityComparer<int> DefaultIntComparer
	{
		[CompilerGenerated]
		get
		{
			return IequalityComparer_0;
		}
		[CompilerGenerated]
		set
		{
			IequalityComparer_0 = value;
		}
	}

	static GClass1664()
	{
		DefaultIntComparer = new Class1071();
	}

	public static float smethod_0(float from, float to, float factor)
	{
		return from * (1f - factor) + to * factor;
	}

	public static GameObject smethod_1()
	{
		GameObject gameObject = new GameObject("item", typeof(RectTransform));
		gameObject.AddComponent<ChartItem>();
		return gameObject;
	}

	public static GameObject smethod_2()
	{
		GameObject gameObject = new GameObject();
		gameObject.AddComponent<ChartItem>();
		return gameObject;
	}

	public static void smethod_3(GameObject obj, bool hideMode)
	{
	}

	public static float smethod_4(AnyChart parent, ChartOrientation orientation)
	{
		if (orientation != ChartOrientation.Vertical)
		{
			return ((IInternalUse)parent).InternalTotalHeight;
		}
		return ((IInternalUse)parent).InternalTotalWidth;
	}

	public static float smethod_5(AnyChart parent, ChartOrientation orientation, ChartDivisionInfo info)
	{
		float result = ((IInternalUse)parent).InternalTotalDepth;
		if (!info.MarkDepth.Automatic)
		{
			result = info.MarkDepth.Value;
		}
		return result;
	}

	public static float smethod_6(AnyChart parent, ChartOrientation orientation, ChartDivisionInfo info)
	{
		float result = ((orientation == ChartOrientation.Vertical) ? ((IInternalUse)parent).InternalTotalWidth : ((IInternalUse)parent).InternalTotalHeight);
		if (!info.MarkLength.Automatic)
		{
			result = info.MarkLength.Value;
		}
		return result;
	}

	public static Vector2 smethod_7(Vector2 v)
	{
		return new Vector2(v.y, 0f - v.x);
	}

	public static bool smethod_8(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
	{
		intersection = default(Vector2);
		Vector2 vector = a2 - a1;
		Vector2 v = b2 - b1;
		float num = Vector2.Dot(vector, smethod_7(v));
		if (num == 0f)
		{
			return false;
		}
		Vector2 lhs = b1 - a1;
		float num2 = Vector2.Dot(lhs, smethod_7(v)) / num;
		if (!(num2 < 0f) && num2 <= 1f)
		{
			float num3 = Vector2.Dot(lhs, smethod_7(vector)) / num;
			if (!(num3 < 0f) && num3 <= 1f)
			{
				intersection = a1 + num2 * vector;
				return true;
			}
			return false;
		}
		return false;
	}

	public static Vector2 smethod_9(float angleDeg, float radius)
	{
		float x = radius * Mathf.Cos(angleDeg);
		float y = radius * Mathf.Sin(angleDeg);
		return new Vector2(x, y);
	}

	public static Vector2 smethod_10(float angleDeg, float radius)
	{
		angleDeg *= MathF.PI / 180f;
		float x = radius * Mathf.Cos(angleDeg);
		float y = radius * Mathf.Sin(angleDeg);
		return new Vector2(x, y);
	}

	public static Rect smethod_11(Rect r)
	{
		float x = r.x;
		float y = r.y;
		float num = r.width;
		float num2 = r.height;
		if (num < 0f)
		{
			x = r.x + num;
			num = 0f - num;
		}
		if (num2 < 0f)
		{
			y = r.y + num2;
			num2 = 0f - num2;
		}
		return new Rect(x, y, num, num2);
	}

	public static bool smethod_12(Renderer renderer, Material material, Material defualt)
	{
		Material material2 = material;
		if (material2 == null)
		{
			material2 = defualt;
			if (material2 == null)
			{
				material2 = Material_0;
			}
		}
		renderer.sharedMaterial = material2;
		return material != null;
	}

	public static void smethod_13(Mesh newMesh, ref Mesh cleanMesh)
	{
		if (!(cleanMesh == newMesh))
		{
			if (cleanMesh != null)
			{
				SafeDestroy(cleanMesh);
			}
			cleanMesh = newMesh;
		}
	}

	public static void SafeDestroy(UnityEngine.Object obj)
	{
		if (!(obj == null))
		{
			if (Application.isEditor && !Application.isPlaying)
			{
				UnityEngine.Object.DestroyImmediate(obj);
			}
			else
			{
				UnityEngine.Object.Destroy(obj);
			}
		}
	}

	public static UIVertex smethod_14(Vector3 pos, Vector2 uv)
	{
		return smethod_15(pos, uv, pos.z);
	}

	public static UIVertex smethod_15(Vector3 pos, Vector2 uv, float z)
	{
		UIVertex result = new UIVertex
		{
			color = Color.white,
			uv0 = uv
		};
		pos.z = z;
		result.position = pos;
		return result;
	}

	public static float smethod_16(MaterialTiling tiling)
	{
		if (!tiling.EnableTiling)
		{
			return -1f;
		}
		return tiling.TileFactor;
	}

	public static void smethod_17(ItemLabelsBase labels, BillboardText text)
	{
		float num = Mathf.Clamp(labels.FontSharpness, 1f, 3f);
		text.Scale = 1f / num;
		text.UIText.fontSize = (int)((float)labels.FontSize * num);
		text.UIText.transform.localScale = new Vector3(text.Scale, text.Scale);
	}

	public static T smethod_18<T>(GameObject obj) where T : Component
	{
		T val = obj.GetComponent<T>();
		if (val == null)
		{
			val = obj.AddComponent<T>();
		}
		return val;
	}

	public static float smethod_19(Vector2 a, Vector2 b, Vector2 c)
	{
		Vector2 lhs = b - a;
		Vector2 rhs = c - b;
		return Vector2.Dot(lhs, rhs);
	}

	public static float smethod_20(Vector2 a, Vector2 b, Vector2 c)
	{
		Vector2 vector = b - a;
		Vector2 vector2 = c - a;
		return vector.x * vector2.y - vector.y * vector2.x;
	}

	public static float smethod_21(Vector2 a, Vector2 b, Vector2 point)
	{
		if (smethod_19(a, b, point) > 0f)
		{
			return (b - point).sqrMagnitude;
		}
		if (smethod_19(b, a, point) > 0f)
		{
			return (a - point).sqrMagnitude;
		}
		float num = smethod_20(a, b, point);
		return num * num / (a - b).sqrMagnitude;
	}

	public static BillboardText smethod_22(BillboardText billboardText, Transform parentTransform, string text, float x, float y, float z, float angle, Transform relativeFrom, bool hideHirarechy)
	{
		GameObject gameObject = billboardText.UIText.gameObject;
		GameObject gameObject2 = billboardText.gameObject;
		smethod_3(gameObject2, hideHirarechy);
		TextDirection component = gameObject.GetComponent<TextDirection>();
		Text text2 = billboardText.UIText;
		if (component != null)
		{
			text2 = component.Text;
			if (relativeFrom != null)
			{
				component.SetRelativeTo(relativeFrom, gameObject2.transform);
			}
			else
			{
				component.SetDirection(angle);
			}
		}
		text2.text = text;
		gameObject2.transform.localPosition = new Vector3(x, y, z);
		return billboardText;
	}

	public static BillboardText smethod_23([CanBeNull] BillboardText item, Text prefab, Transform parentTransform, string text, float x, float y, float z, float angle, Transform relativeFrom, bool hideHirarechy, int fontSize, float sharpness)
	{
		if (item != null)
		{
			return smethod_22(item, parentTransform, text, x, y, z, angle, relativeFrom, hideHirarechy);
		}
		if (prefab == null || prefab.gameObject == null)
		{
			prefab = (GClass861.Load("Chart And Graph/DefaultText") as GameObject).GetComponent<Text>();
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab.gameObject);
		GameObject gameObject2 = new GameObject();
		smethod_3(gameObject2, hideHirarechy);
		if (parentTransform != null)
		{
			gameObject2.transform.SetParent(parentTransform, worldPositionStays: false);
			gameObject.transform.SetParent(parentTransform, worldPositionStays: false);
		}
		BillboardText billboardText = gameObject2.AddComponent<BillboardText>();
		gameObject2.AddComponent<ChartItem>();
		TextDirection component = gameObject.GetComponent<TextDirection>();
		Text text2 = gameObject.GetComponent<Text>();
		if (component != null)
		{
			text2 = component.Text;
			if (relativeFrom != null)
			{
				component.SetRelativeTo(relativeFrom, gameObject2.transform);
			}
			else
			{
				component.SetDirection(angle);
			}
		}
		if (!(billboardText == null) && !(text2 == null))
		{
			sharpness = Mathf.Clamp(sharpness, 1f, 3f);
			text2.fontSize = (int)((float)fontSize * sharpness);
			text2.horizontalOverflow = HorizontalWrapMode.Overflow;
			text2.verticalOverflow = VerticalWrapMode.Overflow;
			text2.resizeTextForBestFit = false;
			billboardText.Scale = 1f / sharpness;
			text2.text = text;
			billboardText.UIText = text2;
			billboardText.textDirection_0 = component;
			if (component != null)
			{
				billboardText.RectTransformOverride = component.GetComponent<RectTransform>();
			}
			else
			{
				billboardText.RectTransformOverride = null;
			}
			gameObject2.transform.localPosition = new Vector3(x, y, z);
			return billboardText;
		}
		SafeDestroy(gameObject);
		SafeDestroy(gameObject2);
		return null;
	}
}

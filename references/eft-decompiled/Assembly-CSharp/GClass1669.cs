using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ChartAndGraph;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public abstract class GClass1669 : Interface7
{
	[NonSerialized]
	public List<BillboardText> List_0 = new List<BillboardText>();

	[NonSerialized]
	public List<BillboardText> List_1 = new List<BillboardText>();

	[NonSerialized]
	public Dictionary<string, BillboardText> Dictionary_0 = new Dictionary<string, BillboardText>();

	[NonSerialized]
	public List<BillboardText> List_2 = new List<BillboardText>();

	[NonSerialized]
	public float Float_0 = 1f;

	[NonSerialized]
	[CompilerGenerated]
	public ChartOrientation ChartOrientation_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	public Vector2[][] Vector2_0 = new Vector2[2][]
	{
		new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		},
		new Vector2[4]
		{
			new Vector2(0f, 1f),
			new Vector2(0f, 0f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		}
	};

	[NonSerialized]
	public Vector2[] Vector2_1 = new Vector2[4];

	public ChartOrientation Orientation
	{
		[CompilerGenerated]
		get
		{
			return ChartOrientation_0;
		}
		[CompilerGenerated]
		set
		{
			ChartOrientation_0 = value;
		}
	}

	public float Tile
	{
		get
		{
			return Float_0;
		}
		set
		{
			Float_0 = value;
		}
	}

	public float Offset
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
		[CompilerGenerated]
		set
		{
			Float_1 = value;
		}
	}

	public float Length
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
		[CompilerGenerated]
		set
		{
			Float_2 = value;
		}
	}

	public bool RecycleText
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public virtual List<BillboardText> CurrentTextObjects => List_1;

	public virtual List<BillboardText> TextObjects => List_0;

	public GClass1669()
	{
		Orientation = ChartOrientation.Vertical;
	}

	public Vector2[] method_0(Rect rect)
	{
		return method_1(rect, Orientation);
	}

	public virtual void Clear()
	{
		if (RecycleText)
		{
			for (int i = 0; i < List_1.Count; i++)
			{
				string text = List_1[i].UIText.text;
				List_1[i].SetVisible(visible: false);
				List_1[i].Recycled = true;
				if (Dictionary_0.ContainsKey(text))
				{
					List_2.Add(List_1[i]);
				}
				else
				{
					Dictionary_0[text] = List_1[i];
				}
			}
		}
		List_0.Clear();
		List_1.Clear();
	}

	public Vector2[] method_1(Rect rect, ChartOrientation orientaion)
	{
		Vector2[] array = ((Orientation != ChartOrientation.Vertical) ? Vector2_0[1] : Vector2_0[0]);
		if (Tile <= 0f)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 vector = array[i];
				Vector2_1[i] = new Vector2(Offset + vector.x * Length, vector.y);
			}
			return Vector2_1;
		}
		float num = rect.width;
		if (orientaion == ChartOrientation.Horizontal)
		{
			num = rect.height;
		}
		num /= Math.Abs(Length);
		for (int j = 0; j < 4; j++)
		{
			Vector2 vector2 = array[j];
			Vector2_1[j] = new Vector2((Offset + vector2.x * Length) * num / Tile, vector2.y);
		}
		return Vector2_1;
	}

	public void method_2(BillboardText t)
	{
		t.Recycled = false;
		Text uIText = t.UIText;
		TextDirection textDirection_ = t.textDirection_0;
		if (uIText != null && uIText.gameObject != null)
		{
			GClass1664.SafeDestroy(uIText.gameObject);
		}
		if (textDirection_ != null && textDirection_.gameObject != null)
		{
			GClass1664.SafeDestroy(textDirection_.gameObject);
		}
		if (t != null)
		{
			GClass1664.SafeDestroy(t.gameObject);
		}
	}

	public void DestoryRecycled()
	{
		foreach (BillboardText value in Dictionary_0.Values)
		{
			List_2.Add(value);
		}
		Dictionary_0.Clear();
	}

	[CanBeNull]
	public virtual BillboardText AddText(AnyChart chart, Text prefab, Transform parentTransform, int fontSize, float fontSharpness, string text, float x, float y, float z, float angle, object userData)
	{
		BillboardText value = null;
		if (RecycleText)
		{
			if (Dictionary_0.TryGetValue(text, out value))
			{
				value.SetVisible(visible: true);
				value.Recycled = false;
				Dictionary_0.Remove(text);
			}
			else if (List_2.Count > 0)
			{
				value = List_2[List_2.Count - 1];
				value.SetVisible(visible: true);
				value.Recycled = false;
				List_2.RemoveAt(List_2.Count - 1);
			}
			else
			{
				value = null;
			}
		}
		BillboardText billboardText = GClass1664.smethod_23(value, prefab, parentTransform, text, x, y, z, angle, null, ((IInternalUse)chart).HideHierarchy, fontSize, fontSharpness);
		billboardText.Recycled = false;
		billboardText.UserData = userData;
		if (value == null)
		{
			List_0.Add(billboardText);
		}
		List_1.Add(billboardText);
		return billboardText;
	}

	public abstract void AddQuad(UIVertex vLeftTop, UIVertex vRightTop, UIVertex vLeftBottom, UIVertex vRightBottom);

	public abstract void AddXYRect(Rect rect, int subMeshGroup, float depth);

	public abstract void AddXZRect(Rect rect, int subMeshGroup, float yPosition);

	public abstract void AddYZRect(Rect rect, int subMeshGroup, float xPosition);
}

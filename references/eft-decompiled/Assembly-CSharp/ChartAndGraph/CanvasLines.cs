using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph;

public class CanvasLines : MaskableGraphic
{
	public struct Struct271
	{
		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_0;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_0;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_1;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_2;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_3;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_4;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_5;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_6;

		[NonSerialized]
		[CompilerGenerated]
		public float Float_0;

		[NonSerialized]
		[CompilerGenerated]
		public Vector3 Vector3_7;

		public bool Degenerated
		{
			[CompilerGenerated]
			readonly get
			{
				return Bool_0;
			}
			[CompilerGenerated]
			set
			{
				Bool_0 = value;
			}
		}

		public Vector3 P1
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_0;
			}
			[CompilerGenerated]
			set
			{
				Vector3_0 = value;
			}
		}

		public Vector3 P2
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_1;
			}
			[CompilerGenerated]
			set
			{
				Vector3_1 = value;
			}
		}

		public Vector3 P3
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_2;
			}
			[CompilerGenerated]
			set
			{
				Vector3_2 = value;
			}
		}

		public Vector3 P4
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_3;
			}
			[CompilerGenerated]
			set
			{
				Vector3_3 = value;
			}
		}

		public Vector3 From
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_4;
			}
			[CompilerGenerated]
			set
			{
				Vector3_4 = value;
			}
		}

		public Vector3 To
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_5;
			}
			[CompilerGenerated]
			set
			{
				Vector3_5 = value;
			}
		}

		public Vector3 Dir
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_6;
			}
			[CompilerGenerated]
			set
			{
				Vector3_6 = value;
			}
		}

		public float Mag
		{
			[CompilerGenerated]
			readonly get
			{
				return Float_0;
			}
			[CompilerGenerated]
			set
			{
				Float_0 = value;
			}
		}

		public Vector3 Normal
		{
			[CompilerGenerated]
			readonly get
			{
				return Vector3_7;
			}
			[CompilerGenerated]
			set
			{
				Vector3_7 = value;
			}
		}

		public Struct271(Vector3 from, Vector3 to, float halfThickness, bool hasNext, bool hasPrev)
		{
			this = default(Struct271);
			Vector3 vector = to - from;
			float num = 0f;
			if (hasNext)
			{
				num += halfThickness;
			}
			if (hasPrev)
			{
				num += halfThickness;
			}
			Mag = vector.magnitude - num * 2f;
			Degenerated = false;
			if (Mag <= 0f)
			{
				Degenerated = true;
			}
			Dir = vector.normalized;
			Vector3 vector2 = halfThickness * 2f * Dir;
			if (hasPrev)
			{
				from += vector2;
			}
			if (hasNext)
			{
				to -= vector2;
			}
			From = from;
			To = to;
			Normal = new Vector3(Dir.y, 0f - Dir.x, Dir.z);
			P1 = From + Normal * halfThickness;
			P2 = from - Normal * halfThickness;
			P3 = to + Normal * halfThickness;
			P4 = to - Normal * halfThickness;
		}
	}

	public class Class1065
	{
		[Serializable]
		[CompilerGenerated]
		public class Class1066
		{
			public static readonly Class1066 class1066_0 = new Class1066();

			public static Func<Vector3, Vector4> func_0;

			public Vector4 method_0(Vector3 x)
			{
				return new Vector4(x.x, x.y, x.z, -1f);
			}
		}

		[NonSerialized]
		public List<Vector4> List_0 = new List<Vector4>();

		public int PointCount
		{
			get
			{
				if (List_0 == null)
				{
					return 0;
				}
				return List_0.Count;
			}
		}

		public int LineCount
		{
			get
			{
				if (List_0 == null)
				{
					return 0;
				}
				if (List_0.Count < 2)
				{
					return 0;
				}
				return List_0.Count - 1;
			}
		}

		public Class1065(IList<Vector3> lines)
		{
			List_0.AddRange(lines.Select((Vector3 x) => new Vector4(x.x, x.y, x.z, -1f)));
		}

		public Class1065(IList<Vector4> lines)
		{
			List_0.AddRange(lines);
		}

		public void ModifiyLines(List<Vector4> v)
		{
			List_0.Clear();
			List_0.AddRange(v);
		}

		public Vector4 getPoint(int index)
		{
			return List_0[index];
		}

		public void GetLine(int index, out Vector3 from, out Vector3 to)
		{
			from = List_0[index];
			to = List_0[index + 1];
		}

		public Struct271 GetLine(int index, float halfThickness, bool hasPrev, bool hasNext)
		{
			Vector3 vector = List_0[index];
			Vector3 to = List_0[index + 1];
			return new Struct271(vector, to, halfThickness, hasNext: false, hasPrev: false);
		}
	}

	public float Thickness = 2f;

	private float float_0 = 1f;

	private bool bool_0;

	private bool bool_1;

	private float float_1 = 5f;

	private Rect rect_0;

	private bool bool_2;

	private Material material_0;

	private SensitivityControl sensitivityControl_0;

	private ChartItemEffect chartItemEffect_0;

	private ChartItemEffect chartItemEffect_1;

	private List<ChartItemEffect> list_0 = new List<ChartItemEffect>();

	private List<ChartItemEffect> list_1 = new List<ChartItemEffect>();

	private List<Class1065> list_2;

	private bool bool_3;

	private float float_2;

	private float float_3;

	private float float_4;

	private float float_5;

	private int int_0 = -1;

	private int int_1 = -1;

	private Vector2 vector2_0;

	private GraphicRaycaster graphicRaycaster_0;

	[CompilerGenerated]
	private Action<int, Vector2> action_0;

	[CompilerGenerated]
	private Action<int, Vector2> action_1;

	[CompilerGenerated]
	private Action action_2;

	private Rect? nullable_0;

	private GClass1670 gclass1670_0;

	private Rect? nullable_1;

	private bool bool_4;

	public int refrenceIndex;

	private UIVertex[] uivertex_0 = new UIVertex[4];

	private static readonly int int_2 = Shader.PropertyToID("_ChartTiling");

	public float Tiling
	{
		get
		{
			return float_0;
		}
		set
		{
			float_0 = value;
		}
	}

	public override Material material
	{
		get
		{
			return base.material;
		}
		set
		{
			GClass1664.SafeDestroy(material_0);
			if (value == null)
			{
				material_0 = null;
				base.material = null;
				return;
			}
			material_0 = new Material(value);
			material_0.hideFlags = HideFlags.DontSave;
			if (material_0.HasProperty("_ChartTiling"))
			{
				material_0.SetFloat(int_2, Tiling);
			}
			base.material = material_0;
		}
	}

	public event Action<int, Vector2> Hover
	{
		[CompilerGenerated]
		add
		{
			Action<int, Vector2> action = action_0;
			Action<int, Vector2> action2;
			do
			{
				action2 = action;
				Action<int, Vector2> value2 = (Action<int, Vector2>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int, Vector2> action = action_0;
			Action<int, Vector2> action2;
			do
			{
				action2 = action;
				Action<int, Vector2> value2 = (Action<int, Vector2>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<int, Vector2> Click
	{
		[CompilerGenerated]
		add
		{
			Action<int, Vector2> action = action_1;
			Action<int, Vector2> action2;
			do
			{
				action2 = action;
				Action<int, Vector2> value2 = (Action<int, Vector2>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int, Vector2> action = action_1;
			Action<int, Vector2> action2;
			do
			{
				action2 = action;
				Action<int, Vector2> value2 = (Action<int, Vector2>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action Leave
	{
		[CompilerGenerated]
		add
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void SetViewRect(Rect r, Rect uvRect)
	{
		nullable_0 = r;
		nullable_1 = uvRect;
	}

	public ChartItemEffect LockHoverObject(int index)
	{
		int count = list_1.Count;
		ChartItemEffect chartItemEffect = null;
		if (count > 0)
		{
			chartItemEffect = list_1[count - 1];
			list_1.RemoveAt(count - 1);
		}
		else
		{
			if (chartItemEffect_0 == null)
			{
				return null;
			}
			GameObject obj = UnityEngine.Object.Instantiate(chartItemEffect_0.gameObject);
			MaskableGraphic component = obj.GetComponent<MaskableGraphic>();
			if (component != null)
			{
				component.maskable = false;
			}
			GClass1664.smethod_18<ChartItem>(obj);
			obj.transform.SetParent(base.transform);
			chartItemEffect = obj.GetComponent<ChartItemEffect>();
			chartItemEffect.Deactivate += method_0;
		}
		chartItemEffect.int_0 = index;
		list_0.Add(chartItemEffect);
		return chartItemEffect;
	}

	public void method_0(ChartItemEffect obj)
	{
		list_0.Remove(obj);
		list_1.Add(obj);
	}

	public void SetHoverPrefab(ChartItemEffect prefab)
	{
		chartItemEffect_0 = prefab;
	}

	public void MakePointRender(float pointSize)
	{
		float_1 = pointSize;
		bool_1 = true;
	}

	public void MakeFillRender(Rect fillRect, bool stretchY)
	{
		rect_0 = fillRect;
		bool_0 = true;
		bool_2 = stretchY;
	}

	public void method_1(List<Vector4> lines)
	{
		if (list_2.Count == 0)
		{
			list_2.Add(new Class1065(lines.ToArray()));
			return;
		}
		list_2[0].ModifiyLines(lines);
		float_2 = float.PositiveInfinity;
		float_3 = float.PositiveInfinity;
		float_4 = float.NegativeInfinity;
		float_5 = float.NegativeInfinity;
		sensitivityControl_0 = GetComponentInParent<SensitivityControl>();
		if (list_2 != null)
		{
			for (int i = 0; i < list_2.Count; i++)
			{
				Class1065 @class = list_2[i];
				int pointCount = @class.PointCount;
				for (int j = 0; j < pointCount; j++)
				{
					Vector3 vector = @class.getPoint(j);
					float_2 = Mathf.Min(float_2, vector.x);
					float_3 = Mathf.Min(float_3, vector.y);
					float_4 = Mathf.Max(float_4, vector.x);
					float_5 = Mathf.Max(float_5, vector.y);
				}
			}
		}
		SetVerticesDirty();
		Rebuild(CanvasUpdate.PostLayout);
		bool_4 = true;
	}

	public void method_2(List<Class1065> lines)
	{
		list_2 = lines;
		float_2 = float.PositiveInfinity;
		float_3 = float.PositiveInfinity;
		float_4 = float.NegativeInfinity;
		float_5 = float.NegativeInfinity;
		sensitivityControl_0 = GetComponentInParent<SensitivityControl>();
		if (list_2 != null)
		{
			for (int i = 0; i < list_2.Count; i++)
			{
				Class1065 @class = list_2[i];
				int pointCount = @class.PointCount;
				for (int j = 0; j < pointCount; j++)
				{
					Vector3 vector = @class.getPoint(j);
					float_2 = Mathf.Min(float_2, vector.x);
					float_3 = Mathf.Min(float_3, vector.y);
					float_4 = Mathf.Max(float_4, vector.x);
					float_5 = Mathf.Max(float_5, vector.y);
				}
			}
		}
		SetAllDirty();
		bool_3 = false;
		for (int k = 0; k < list_0.Count; k++)
		{
			list_0[k].gameObject.SetActive(value: false);
			list_1.Add(list_0[k]);
		}
		list_0.Clear();
		if (chartItemEffect_1 != null)
		{
			chartItemEffect_1.gameObject.SetActive(value: false);
			list_1.Add(chartItemEffect_1);
			chartItemEffect_1 = null;
		}
		int_1 = -1;
		int_0 = -1;
		Rebuild(CanvasUpdate.PreRender);
	}

	public override void UpdateMaterial()
	{
		base.UpdateMaterial();
		base.canvasRenderer.SetTexture(material.mainTexture);
	}

	public void method_3(Vector3 point, Vector3 dir, Vector3 normal, float dist, float size, float z, out Vector3 p1, out Vector3 p2)
	{
		point.z = z;
		point += dir * dist;
		normal *= size;
		p1 = point + normal;
		p2 = point - normal;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		GClass1664.SafeDestroy(material_0);
	}

	public override void OnDisable()
	{
		base.OnDisable();
	}

	public void Update()
	{
		Material material = this.material;
		if (material_0 != null && material != null && material_0.HasProperty("_ChartTiling"))
		{
			if (material_0 != material)
			{
				material_0.CopyPropertiesFromMaterial(material);
			}
			material_0.SetFloat(int_2, Tiling);
		}
		HandleMouseMove(bool_4);
		bool_4 = false;
	}

	public IEnumerable<UIVertex> method_4()
	{
		if (list_2 == null)
		{
			yield break;
		}
		float z = 0f;
		_ = float_1 * 0.5f;
		int num = 0;
		while (num < list_2.Count)
		{
			Class1065 @class = list_2[num];
			int pointCount = @class.PointCount;
			int num4;
			for (int num2 = 0; num2 < pointCount; num2 = num4)
			{
				Vector4 point = @class.getPoint(num2);
				if (point.w != 0f)
				{
					Vector3 vector = point;
					float num3 = float_1 * 0.5f;
					if (point.w >= 0f)
					{
						num3 = point.w * 0.5f;
					}
					Vector3 pos = vector + new Vector3(0f - num3, 0f - num3, 0f);
					Vector3 pos2 = vector + new Vector3(num3, 0f - num3, 0f);
					Vector3 pos3 = vector + new Vector3(0f - num3, num3, 0f);
					Vector3 pos4 = vector + new Vector3(num3, num3, 0f);
					Vector2 uv = new Vector2(0f, 0f);
					Vector2 uv2 = new Vector2(1f, 0f);
					Vector2 uv3 = new Vector2(0f, 1f);
					Vector2 uv4 = new Vector2(1f, 1f);
					UIVertex uIVertex = GClass1664.smethod_15(pos, uv, z);
					UIVertex uIVertex2 = GClass1664.smethod_15(pos2, uv2, z);
					UIVertex uIVertex3 = GClass1664.smethod_15(pos3, uv3, z);
					UIVertex uIVertex4 = GClass1664.smethod_15(pos4, uv4, z);
					yield return uIVertex;
					yield return uIVertex2;
					yield return uIVertex3;
					yield return uIVertex4;
				}
				num4 = num2 + 1;
			}
			num4 = num + 1;
			num = num4;
		}
	}

	public Vector2 method_5(Vector2 uv)
	{
		if (!nullable_1.HasValue)
		{
			return uv;
		}
		Rect value = nullable_1.Value;
		float x = value.x + uv.x * value.width;
		float y = value.y + uv.y * value.height;
		return new Vector2(x, y);
	}

	public IEnumerable<UIVertex> method_6()
	{
		if (list_2 == null)
		{
			yield break;
		}
		float z = 0f;
		int num = 0;
		while (num < list_2.Count)
		{
			Class1065 @class = list_2[num];
			int lineCount = @class.LineCount;
			int num3;
			for (int num2 = 0; num2 < lineCount; num2 = num3)
			{
				@class.GetLine(num2, out var from, out var to);
				Vector2 to2 = to;
				Vector2 from2 = from;
				method_14(rect_0.xMin, rect_0.yMin, rect_0.xMax, rect_0.yMin, xAxis: true, oposite: false, ref from2, ref to2);
				to = new Vector3(to2.x, to2.y, to.z);
				from = new Vector3(from2.x, from2.y, from.z);
				Vector3 pos = from;
				Vector3 pos2 = to;
				pos.y = rect_0.yMin;
				pos2.y = rect_0.yMin;
				float y = 1f;
				float y2 = 1f;
				if (!bool_2)
				{
					y = Mathf.Abs((from.y - rect_0.yMin) / rect_0.height);
					y2 = Mathf.Abs((to.y - rect_0.yMin) / rect_0.height);
				}
				float x = (from.x - rect_0.xMin) / rect_0.width;
				float x2 = (to.x - rect_0.xMin) / rect_0.width;
				Vector2 uv = method_5(new Vector2(x, y));
				Vector2 uv2 = method_5(new Vector2(x2, y2));
				Vector2 uv3 = method_5(new Vector2(x, 0f));
				Vector2 uv4 = method_5(new Vector2(x2, 0f));
				UIVertex uIVertex = GClass1664.smethod_15(from, uv, z);
				UIVertex uIVertex2 = GClass1664.smethod_15(to, uv2, z);
				UIVertex uIVertex3 = GClass1664.smethod_15(pos, uv3, z);
				UIVertex uIVertex4 = GClass1664.smethod_15(pos2, uv4, z);
				yield return uIVertex;
				yield return uIVertex2;
				yield return uIVertex3;
				yield return uIVertex4;
				num3 = num2 + 1;
			}
			num3 = num + 1;
			num = num3;
		}
	}

	public IEnumerable<UIVertex> method_7()
	{
		if (list_2 == null)
		{
			yield break;
		}
		float num = Thickness * 0.5f;
		float num2 = 0f;
		int num3 = 0;
		while (num3 < list_2.Count)
		{
			Class1065 @class = list_2[num3];
			int lineCount = @class.LineCount;
			Struct271? @struct = null;
			Struct271? struct2 = null;
			float num4 = 0f;
			float num5 = 0f;
			for (int i = 0; i < lineCount; i++)
			{
				num5 += @class.GetLine(i, num, hasPrev: false, hasNext: false).Mag;
			}
			int num7;
			for (int num6 = 0; num6 < lineCount; num6 = num7)
			{
				bool hasNext = num6 + 1 < lineCount;
				Struct271 value = ((!@struct.HasValue) ? @class.GetLine(num6, num, struct2.HasValue, hasNext) : @struct.Value);
				@struct = null;
				if (num6 + 1 < lineCount)
				{
					@struct = @class.GetLine(num6 + 1, num, hasPrev: true, num6 + 2 < lineCount);
				}
				Vector3 p = value.P1;
				Vector3 p2 = value.P2;
				Vector3 p3 = value.P3;
				Vector3 p4 = value.P4;
				Vector2 uv = new Vector2(num4 * Tiling, 0f);
				Vector2 uv2 = new Vector2(num4 * Tiling, 1f);
				num4 += value.Mag / num5;
				Vector2 uv3 = new Vector2(num4 * Tiling, 0f);
				Vector2 uv4 = new Vector2(num4 * Tiling, 1f);
				UIVertex uIVertex = GClass1664.smethod_15(p, uv, num2);
				UIVertex uIVertex2 = GClass1664.smethod_15(p2, uv2, num2);
				UIVertex uIVertex3 = GClass1664.smethod_15(p3, uv3, num2);
				UIVertex uIVertex4 = GClass1664.smethod_15(p4, uv4, num2);
				yield return uIVertex;
				yield return uIVertex2;
				yield return uIVertex3;
				yield return uIVertex4;
				Vector3 p6;
				if (@struct.HasValue)
				{
					float z = num2 + 0.2f;
					method_3(value.To, value.Dir, value.Normal, num * 0.5f, num * 0.6f, uIVertex3.position.z, out var p5, out p6);
					yield return uIVertex3;
					yield return uIVertex4;
					yield return GClass1664.smethod_15(p5, uIVertex3.uv0, z);
					yield return GClass1664.smethod_15(p6, uIVertex4.uv0, z);
					p5 = default(Vector3);
					p6 = default(Vector3);
				}
				if (struct2.HasValue)
				{
					float z = num2 + 0.2f;
					method_3(value.From, -value.Dir, value.Normal, num * 0.5f, num * 0.6f, uIVertex.position.z, out var p7, out p6);
					yield return GClass1664.smethod_15(p7, uIVertex.uv0, z);
					yield return GClass1664.smethod_15(p6, uIVertex2.uv0, z);
					yield return uIVertex;
					yield return uIVertex2;
					p6 = default(Vector3);
				}
				num2 -= 0.05f;
				struct2 = value;
				num7 = num6 + 1;
			}
			num7 = num3 + 1;
			num3 = num7;
		}
	}

	public IEnumerable<UIVertex> method_8()
	{
		if (bool_1)
		{
			return method_4();
		}
		if (bool_0)
		{
			return method_6();
		}
		return method_7();
	}

	public override void OnPopulateMesh(VertexHelper vh)
	{
		base.OnPopulateMesh(vh);
		vh.Clear();
		int num = 0;
		foreach (UIVertex item in method_8())
		{
			uivertex_0[num++] = item;
			if (num == 4)
			{
				UIVertex uIVertex = uivertex_0[2];
				uivertex_0[2] = uivertex_0[3];
				uivertex_0[3] = uIVertex;
				num = 0;
				vh.AddUIVertexQuad(uivertex_0);
			}
		}
	}

	public override void OnPopulateMesh(Mesh m)
	{
		if (gclass1670_0 == null)
		{
			gclass1670_0 = new GClass1670(1);
		}
		else
		{
			gclass1670_0.Clear();
		}
		int num = 0;
		foreach (UIVertex item in method_8())
		{
			uivertex_0[num++] = item;
			if (num == 4)
			{
				num = 0;
				gclass1670_0.AddQuad(uivertex_0[0], uivertex_0[1], uivertex_0[2], uivertex_0[3]);
			}
		}
		gclass1670_0.ApplyToMesh(m);
	}

	public void method_9(Vector3 mouse, out int segment, out int line)
	{
		float num = float.PositiveInfinity;
		segment = -1;
		line = -1;
		if (list_2 == null)
		{
			return;
		}
		for (int i = 0; i < list_2.Count; i++)
		{
			Class1065 @class = list_2[i];
			int lineCount = @class.LineCount;
			for (int j = 0; j < lineCount; j++)
			{
				@class.GetLine(j, out var from, out var to);
				float num2 = GClass1664.smethod_21(from, to, mouse);
				if (num2 < num)
				{
					num = num2;
					segment = i;
					line = j;
				}
			}
		}
		float num3 = 10f;
		if (sensitivityControl_0 != null)
		{
			num3 = sensitivityControl_0.Sensitivity;
		}
		float num4 = Thickness + num3;
		if ((nullable_0.HasValue && !nullable_0.Value.Contains(mouse)) || num > num4 * num4)
		{
			segment = -1;
			line = -1;
		}
	}

	public void method_10(Vector3 mouse, out int i, out int j)
	{
		if (bool_1)
		{
			method_11(mouse, out i, out j);
		}
		else
		{
			method_9(mouse, out i, out j);
		}
		if (j >= 0)
		{
			j += refrenceIndex;
		}
	}

	public void method_11(Vector3 mouse, out int segment, out int point)
	{
		float num = float.PositiveInfinity;
		segment = -1;
		point = -1;
		if (list_2 == null)
		{
			return;
		}
		float w = float_1;
		for (int i = 0; i < list_2.Count; i++)
		{
			Class1065 @class = list_2[i];
			int pointCount = @class.PointCount;
			for (int j = 0; j < pointCount; j++)
			{
				Vector4 point2 = @class.getPoint(j);
				if (point2.w == 0f)
				{
					continue;
				}
				float sqrMagnitude = (mouse - (Vector3)point2).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					w = point2.w;
					if (w < 0f)
					{
						w = float_1;
					}
					num = sqrMagnitude;
					segment = i;
					point = j;
				}
			}
		}
		float num2 = 10f;
		if (sensitivityControl_0 != null)
		{
			num2 = sensitivityControl_0.Sensitivity;
		}
		float num3 = w + num2;
		if ((nullable_0.HasValue && !nullable_0.Value.Contains(mouse)) || num > num3 * num3)
		{
			segment = -1;
			point = -1;
		}
	}

	public void method_12()
	{
		if (list_0 == null)
		{
			return;
		}
		foreach (ChartItemEffect item in list_0)
		{
			method_13(item);
		}
	}

	public void method_13([CanBeNull] ChartItemEffect hover)
	{
		if (hover == null || list_2 == null || list_2.Count == 0)
		{
			return;
		}
		int num = hover.int_0 - refrenceIndex;
		if (num < 0)
		{
			return;
		}
		if (bool_1)
		{
			if (num < list_2[0].PointCount)
			{
				Vector4 point = list_2[0].getPoint(num);
				RectTransform component = hover.GetComponent<RectTransform>();
				component.localScale = new Vector3(1f, 1f, 1f);
				component.sizeDelta = new Vector2(5f, 5f);
				component.anchoredPosition3D = new Vector3(point.x, point.y, 0f);
			}
		}
		else if (num < list_2[0].LineCount)
		{
			list_2[0].GetLine(num, out var from, out var to);
			if (nullable_0.HasValue)
			{
				Vector2 from2 = from;
				Vector2 to2 = to;
				method_15(nullable_0.Value, ref from2, ref to2);
				from = new Vector3(from2.x, from2.y, from.z);
				to = new Vector3(to2.x, to2.y, to.z);
			}
			Vector3 vector = to - from;
			float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			RectTransform component2 = hover.GetComponent<RectTransform>();
			component2.sizeDelta = new Vector2(vector.magnitude, Thickness);
			component2.localScale = new Vector3(1f, 1f, 1f);
			component2.localRotation = Quaternion.Euler(0f, 0f, z);
			Vector3 vector2 = (from + to) * 0.5f;
			component2.anchoredPosition3D = new Vector3(vector2.x, vector2.y, 0f);
		}
	}

	public void method_14(float x1, float y1, float x2, float y2, bool xAxis, bool oposite, ref Vector2 from, ref Vector2 to)
	{
		Vector2 a = new Vector2(x1, y1);
		Vector2 a2 = new Vector2(x2, y2);
		if (!GClass1664.smethod_8(a, a2, from, to, out var intersection))
		{
			return;
		}
		if (xAxis)
		{
			if ((to.y > from.y) ^ oposite)
			{
				from = intersection;
			}
			else
			{
				to = intersection;
			}
		}
		else if ((to.x > from.x) ^ oposite)
		{
			from = intersection;
		}
		else
		{
			to = intersection;
		}
	}

	public void method_15(Rect r, ref Vector2 from, ref Vector2 to)
	{
		method_14(r.xMin, r.yMin, r.xMax, r.yMin, xAxis: true, oposite: false, ref from, ref to);
		method_14(r.xMin, r.yMax, r.xMax, r.yMax, xAxis: true, oposite: true, ref from, ref to);
		method_14(r.xMin, r.yMin, r.xMin, r.yMax, xAxis: false, oposite: false, ref from, ref to);
		method_14(r.xMax, r.yMin, r.xMax, r.yMax, xAxis: false, oposite: true, ref from, ref to);
	}

	public void method_16(ChartItemEffect hover)
	{
		hover.TriggerOut(deactivateOnEnd: true);
		ChartMaterialController component = hover.GetComponent<ChartMaterialController>();
		if ((bool)component)
		{
			component.TriggerOff();
		}
	}

	public void method_17(ChartItemEffect hover)
	{
		hover.TriggerIn(deactivateOnEnd: false);
		ChartMaterialController component = hover.GetComponent<ChartMaterialController>();
		if ((bool)component)
		{
			component.TriggerOn();
		}
	}

	public void method_18(Vector3 mouse, bool leave)
	{
		if (list_2 == null)
		{
			return;
		}
		int num = int_0;
		int num2 = int_1;
		if (leave)
		{
			int_0 = -1;
			int_1 = -1;
		}
		else
		{
			method_10(mouse, out int_0, out int_1);
		}
		if (num == int_0 && num2 == int_1)
		{
			return;
		}
		if (chartItemEffect_1 != null)
		{
			method_16(chartItemEffect_1);
			if (int_0 == -1 && int_1 == -1 && action_2 != null)
			{
				action_2();
			}
			chartItemEffect_1 = null;
		}
		if (int_0 == -1 || int_1 == -1)
		{
			return;
		}
		chartItemEffect_1 = LockHoverObject(int_1);
		if (!(chartItemEffect_1 == null))
		{
			if (action_0 != null)
			{
				action_0(int_1, mouse);
			}
			chartItemEffect_1.gameObject.SetActive(value: true);
			method_13(chartItemEffect_1);
			method_17(chartItemEffect_1);
		}
	}

	public void HandleMouseMove()
	{
		HandleMouseMove(force: false);
	}

	public void HandleMouseMove(bool force)
	{
		graphicRaycaster_0 = GetComponentInParent<GraphicRaycaster>();
		if (graphicRaycaster_0 == null)
		{
			return;
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle(base.transform as RectTransform, Input.mousePosition, graphicRaycaster_0.eventCamera, out var localPoint);
		float num = 10f;
		if (sensitivityControl_0 != null)
		{
			num = sensitivityControl_0.Sensitivity;
		}
		float num2 = Mathf.Max(Thickness, float_1) + num;
		if (Input.GetMouseButtonDown(0))
		{
			method_19();
		}
		if (force)
		{
			method_12();
		}
		if (!(localPoint.x < float_2 - num2) && !(localPoint.y < float_3 - num2) && !(localPoint.x > float_4 + num2) && localPoint.y <= float_5 + num2)
		{
			if (!bool_3)
			{
				bool_3 = true;
				vector2_0 = localPoint;
				method_18(localPoint, leave: false);
			}
			else if ((vector2_0 - localPoint).sqrMagnitude > 1f || force)
			{
				vector2_0 = localPoint;
				method_18(localPoint, leave: false);
			}
		}
		else if (bool_3)
		{
			bool_3 = false;
			method_18(localPoint, leave: true);
		}
	}

	public void method_19()
	{
		if (int_0 != -1 && int_1 != -1 && action_1 != null)
		{
			graphicRaycaster_0 = GetComponentInParent<GraphicRaycaster>();
			if (!(graphicRaycaster_0 == null))
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(base.transform as RectTransform, Input.mousePosition, graphicRaycaster_0.eventCamera, out var localPoint);
				Vector3 vector = base.transform.InverseTransformPoint(localPoint);
				action_1(int_1, vector);
			}
		}
	}
}

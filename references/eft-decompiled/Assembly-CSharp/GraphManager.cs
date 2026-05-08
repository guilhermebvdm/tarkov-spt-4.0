using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
	public struct GStruct0(Vector3 translation, Quaternion rotation, Vector3 scale)
	{
		public Vector3 Translation = translation;

		public Quaternion Rotation = rotation;

		public Vector3 Scale = scale;

		public Matrix4x4 TRS = Matrix4x4.TRS(translation, rotation, scale);
	}

	public struct GStruct1
	{
		[NonSerialized]
		[CompilerGenerated]
		public float Float_0;

		[NonSerialized]
		[CompilerGenerated]
		public Color Color_0;

		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_0;

		[NonSerialized]
		[CompilerGenerated]
		public Color Color_1;

		public float Value
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

		public Color Color
		{
			[CompilerGenerated]
			readonly get
			{
				return Color_0;
			}
			[CompilerGenerated]
			set
			{
				Color_0 = value;
			}
		}

		public bool Mark
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

		public Color MarkColor
		{
			[CompilerGenerated]
			readonly get
			{
				return Color_1;
			}
			[CompilerGenerated]
			set
			{
				Color_1 = value;
			}
		}

		public GStruct1(float value)
		{
			this = default(GStruct1);
			Value = value;
			Color = Color.red;
		}

		public GStruct1(float value, Color color)
		{
			this = default(GStruct1);
			Value = value;
			Color = color;
		}

		public GStruct1(float value, Color color, bool mark, Color markColor)
		{
			this = default(GStruct1);
			Value = value;
			Color = color;
			Mark = mark;
			MarkColor = markColor;
		}
	}

	public class GClass11
	{
		public GameObject ParentUI;

		public List<GStruct1> DataPairs;

		public string Name;

		public GameObject CanvasUI;

		public Canvas Canvas;

		public RectTransform CanvasRect;

		public GameObject MaxUI;

		public GameObject MinUI;

		public GameObject AvgUI;

		public Text MaxText;

		public Text MinText;

		public Text AvgText;

		public RectTransform MaxTransform;

		public RectTransform AvgTransform;

		public RectTransform MinTransform;

		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_0;

		[NonSerialized]
		[CompilerGenerated]
		public GStruct0 Gstruct0_0;

		[NonSerialized]
		[CompilerGenerated]
		public int Int_0;

		[NonSerialized]
		public int Int_1;

		[NonSerialized]
		[CompilerGenerated]
		public float Float_0;

		[NonSerialized]
		[CompilerGenerated]
		public float Float_1;

		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_1;

		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_2;

		[NonSerialized]
		[CompilerGenerated]
		public float Float_2;

		[NonSerialized]
		[CompilerGenerated]
		public float Float_3;

		[NonSerialized]
		[CompilerGenerated]
		public float Float_4;

		[NonSerialized]
		[CompilerGenerated]
		public Vector4 Vector4_0;

		public bool IsWorldSpace
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

		public GStruct0 TRS
		{
			[CompilerGenerated]
			get
			{
				return Gstruct0_0;
			}
			[CompilerGenerated]
			set
			{
				Gstruct0_0 = value;
			}
		}

		public int MaxNumPoints
		{
			[CompilerGenerated]
			get
			{
				return Int_0;
			}
			[CompilerGenerated]
			set
			{
				Int_0 = value;
			}
		}

		public float MinValue
		{
			[CompilerGenerated]
			get
			{
				return Float_0;
			}
			[CompilerGenerated]
			set
			{
				Float_0 = value;
			}
		}

		public float MaxValue
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

		public bool AutoMaxValue
		{
			[CompilerGenerated]
			get
			{
				return Bool_1;
			}
			[CompilerGenerated]
			set
			{
				Bool_1 = value;
			}
		}

		public bool AutoMinValue
		{
			[CompilerGenerated]
			get
			{
				return Bool_2;
			}
			[CompilerGenerated]
			set
			{
				Bool_2 = value;
			}
		}

		public float CurrentValue
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

		public float AverageValue
		{
			[CompilerGenerated]
			get
			{
				return Float_3;
			}
			[CompilerGenerated]
			set
			{
				Float_3 = value;
			}
		}

		public float CurrentRange
		{
			[CompilerGenerated]
			get
			{
				return Float_4;
			}
			[CompilerGenerated]
			set
			{
				Float_4 = value;
			}
		}

		public Vector4 Rectangle
		{
			[CompilerGenerated]
			get
			{
				return Vector4_0;
			}
			[CompilerGenerated]
			set
			{
				Vector4_0 = value;
			}
		}

		public GClass11()
		{
			DataPairs = new List<GStruct1>();
			MaxNumPoints = 400;
			Int_1 = 0;
			Rectangle = new Vector4(0f, 0f, 1f, 0.2f);
			Name = "";
			ParentUI = null;
			MinValue = -1f;
			MaxValue = 9f;
			ResetDefault();
		}

		public void ResetDefault()
		{
			CurrentValue = 0f;
			AverageValue = 0f;
			CurrentRange = MaxValue - MinValue;
			IsWorldSpace = false;
		}

		public void ResetUI()
		{
			if ((bool)CanvasUI)
			{
				UnityEngine.Object.Destroy(CanvasUI);
				CanvasUI = null;
				Canvas = null;
				CanvasRect = null;
			}
			if ((bool)MaxUI)
			{
				UnityEngine.Object.Destroy(MaxUI);
				MaxUI = null;
				MaxText = null;
			}
			if ((bool)MinUI)
			{
				UnityEngine.Object.Destroy(MinUI);
				MinUI = null;
				MinText = null;
			}
			if ((bool)AvgUI)
			{
				UnityEngine.Object.Destroy(AvgUI);
				AvgUI = null;
				AvgText = null;
			}
		}

		public void UpdateStats(float dataPoint)
		{
			ResetDefault();
			if (AutoMaxValue)
			{
				MaxValue = float.MinValue;
			}
			if (AutoMinValue)
			{
				MinValue = float.MaxValue;
			}
			for (int i = 0; i < DataPairs.Count; i++)
			{
				float value = DataPairs[i].Value;
				if (AutoMaxValue && MaxValue < value)
				{
					MaxValue = value;
				}
				if (AutoMinValue && MinValue > value)
				{
					MinValue = value;
				}
			}
			AverageValue = DataPairs[DataPairs.Count - 1].Value;
			CurrentValue = dataPoint;
			CurrentRange = MaxValue - MinValue;
		}

		public void InitializeParentUI()
		{
			if (!(ParentUI == null))
			{
				return;
			}
			if (GraphManagerUI == null || GraphsUI == null)
			{
				smethod_3();
			}
			if (!GraphManagerUI)
			{
				return;
			}
			foreach (Transform item in GraphManagerUI.transform)
			{
				if (string.Equals(item.gameObject.name, "Graphs_UI"))
				{
					ParentUI = item.gameObject;
					break;
				}
			}
		}

		public void UpdateUI()
		{
			InitializeParentUI();
			if (!ParentUI)
			{
				return;
			}
			if (CanvasUI == null)
			{
				string name = string.Format("{0}_Canvas_{1}", IsWorldSpace ? "WS" : "SS", Name);
				CanvasUI = GameObject.Find(name);
				if (CanvasUI == null)
				{
					CanvasUI = new GameObject();
					CanvasUI.transform.parent = ParentUI.transform;
					CanvasUI.gameObject.name = name;
					Canvas = CanvasUI.AddComponent<Canvas>();
					if (IsWorldSpace)
					{
						Canvas.renderMode = RenderMode.WorldSpace;
					}
					else if (smethod_0())
					{
						Canvas.renderMode = RenderMode.ScreenSpaceCamera;
						Canvas.worldCamera = Camera.current;
						Canvas.planeDistance = 126f;
					}
					else
					{
						Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
					}
					CanvasRect = Canvas.GetComponent<RectTransform>();
					CanvasRect.anchorMin = new Vector2(0f, 0f);
					CanvasRect.anchorMax = new Vector2(0f, 0f);
					CanvasUI.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 5000f;
					CanvasUI.AddComponent<GraphicRaycaster>();
				}
				else
				{
					Canvas = CanvasUI.GetComponent<Canvas>();
					CanvasRect = Canvas.GetComponent<RectTransform>();
				}
			}
			if ((bool)CanvasRect)
			{
				CanvasRect.position = TRS.Translation;
				CanvasRect.rotation = TRS.Rotation;
				CanvasRect.sizeDelta = new Vector2(TRS.Scale.x, TRS.Scale.y);
			}
			Vector2 zero = Vector2.zero;
			zero = ((!IsWorldSpace) ? new Vector2(100f, 20f) : new Vector2(0.8f, 0.2f));
			if (!(CanvasUI != null))
			{
				return;
			}
			if (MaxUI == null)
			{
				string name2 = string.Format("{0}_{1}", CanvasUI.gameObject.name, "Max");
				MaxUI = GameObject.Find(name2);
				if (MaxUI == null)
				{
					MaxUI = new GameObject();
					MaxUI.transform.parent = CanvasUI.transform;
					MaxUI.gameObject.name = name2;
					MaxText = MaxUI.AddComponent<Text>();
					MaxText.text = "20.0";
					MaxText.color = Color.white;
					MaxText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
					MaxText.alignment = TextAnchor.UpperLeft;
				}
				else
				{
					MaxText = MaxUI.GetComponent<Text>();
				}
				MaxTransform = MaxText.GetComponent<RectTransform>();
				MaxTransform.sizeDelta = zero;
				MaxTransform.localScale = new Vector3(1f, 1f, 1f);
			}
			if ((bool)MaxTransform)
			{
				if (IsWorldSpace)
				{
					MaxTransform.localPosition = new Vector3(0f - TRS.Scale.x / 2f + MaxTransform.sizeDelta.x / 2f, 0f + TRS.Scale.y / 2f - MaxTransform.sizeDelta.y / 2f, 0f);
				}
				else
				{
					MaxTransform.localPosition = new Vector3(0f - (float)Screen.width / 2f + MaxTransform.sizeDelta.x / 2f + Rectangle.x * (float)Screen.width, (float)Screen.height / 2f - MaxTransform.sizeDelta.y / 2f - Rectangle.y * (float)Screen.height, 0f);
				}
			}
			if (AvgUI == null)
			{
				string name3 = string.Format("{0}_{1}", CanvasUI.gameObject.name, "Avg");
				AvgUI = GameObject.Find(name3);
				if (AvgUI == null)
				{
					AvgUI = new GameObject();
					AvgUI.transform.parent = CanvasUI.transform;
					AvgUI.gameObject.name = string.Format("{0}_{1}", CanvasUI.gameObject.name, "Avg");
					AvgText = AvgUI.AddComponent<Text>();
					AvgText.text = "0.0f";
					AvgText.color = Color.white;
					AvgText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
					AvgText.alignment = TextAnchor.MiddleLeft;
				}
				else
				{
					AvgText = AvgUI.GetComponent<Text>();
				}
				AvgTransform = AvgText.GetComponent<RectTransform>();
				AvgTransform.sizeDelta = zero;
				AvgTransform.localScale = new Vector3(1f, 1f, 1f);
			}
			if ((bool)AvgTransform)
			{
				if (IsWorldSpace)
				{
					AvgTransform.localPosition = new Vector3(0f - TRS.Scale.x / 2f + AvgTransform.sizeDelta.x / 2f, 0f, 0f);
				}
				else
				{
					AvgTransform.localPosition = new Vector3(0f - (float)Screen.width / 2f + AvgTransform.sizeDelta.x / 2f + Rectangle.x * (float)Screen.width, (float)Screen.height / 2f - Rectangle.y * (float)Screen.height - 0.5f * Rectangle.w * (float)Screen.height, 0f);
				}
			}
			if (MinUI == null)
			{
				string name4 = string.Format("{0}_{1}", CanvasUI.gameObject.name, "Min");
				MinUI = GameObject.Find(name4);
				if (MinUI == null)
				{
					MinUI = new GameObject();
					MinUI.transform.parent = CanvasUI.transform;
					MinUI.gameObject.name = string.Format("{0}_{1}", CanvasUI.gameObject.name, "Min");
					MinText = MinUI.AddComponent<Text>();
					MinText.text = "20.0";
					MinText.color = Color.white;
					MinText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
					MinText.alignment = TextAnchor.LowerLeft;
				}
				else
				{
					MinText = MinUI.GetComponent<Text>();
				}
				MinTransform = MinText.GetComponent<RectTransform>();
				MinTransform.sizeDelta = zero;
				MinTransform.localScale = new Vector3(1f, 1f, 1f);
			}
			if ((bool)MinTransform)
			{
				if (IsWorldSpace)
				{
					MinTransform.localPosition = new Vector3(0f - TRS.Scale.x / 2f + MinTransform.sizeDelta.x / 2f, 0f - TRS.Scale.y / 2f + MinTransform.sizeDelta.y / 2f, 0f);
				}
				else
				{
					MinTransform.localPosition = new Vector3(0f - (float)Screen.width / 2f + MinTransform.sizeDelta.x / 2f + Rectangle.x * (float)Screen.width, (float)Screen.height / 2f + MinTransform.sizeDelta.y / 2f - Rectangle.y * (float)Screen.height - 1f * Rectangle.w * (float)Screen.height, 0f);
				}
			}
		}

		public void AddPair(float dataPoint)
		{
			DataPairs.Add(new GStruct1(dataPoint));
			TRS = default(GStruct0);
			Shrink();
			UpdateStats(dataPoint);
			UpdateUI();
		}

		public void AddPair(float dataPoint, Color color)
		{
			DataPairs.Add(new GStruct1(dataPoint, color));
			TRS = default(GStruct0);
			Shrink();
			UpdateStats(dataPoint);
		}

		public void AddPair(float dataPoint, Color color, Rect rectangle)
		{
			method_0(new GStruct1(dataPoint, color), rectangle);
		}

		public void AddPair(float dataPoint, Color color, bool mark, Color markColor, Rect rectangle)
		{
			method_0(new GStruct1(dataPoint, color, mark, markColor), rectangle);
		}

		public void method_0(GStruct1 dataPair, Rect rectangle)
		{
			Rectangle = new Vector4(rectangle.x / (float)Screen.width, rectangle.y / (float)Screen.height, rectangle.width / (float)Screen.width, rectangle.height / (float)Screen.height);
			DataPairs.Add(dataPair);
			TRS = default(GStruct0);
			Shrink();
			UpdateStats(dataPair.Value);
			UpdateUI();
		}

		public void AddPair(float dataPoint, GStruct0 trs)
		{
			AddPair(dataPoint);
			IsWorldSpace = true;
			TRS = trs;
			Rectangle = new Vector4(0f, 0f, 1f, 1f);
			UpdateUI();
		}

		public void AddPair(float dataPoint, Color color, GStruct0 trs)
		{
			AddPair(dataPoint, color);
			IsWorldSpace = true;
			TRS = trs;
			Rectangle = new Vector4(0f, 0f, 1f, 1f);
			UpdateUI();
		}

		public void Shrink()
		{
			if (DataPairs.Count > MaxNumPoints)
			{
				DataPairs.RemoveAt(0);
			}
		}

		public void Reset()
		{
			DataPairs.RemoveRange(0, DataPairs.Count - 1);
			ResetDefault();
			ResetUI();
		}

		public void Update()
		{
			if (Int_1 != MaxNumPoints)
			{
				Int_1 = MaxNumPoints;
				if (DataPairs.Count > 0)
				{
					Reset();
				}
			}
			if (DataPairs.Count > 0)
			{
				CurrentValue = DataPairs[DataPairs.Count - 1].Value;
			}
		}

		public float GetX(int index, float offset)
		{
			float num = index;
			float num2 = (float)MaxNumPoints - 1f;
			return num / num2 + offset;
		}

		public float GetY(int index)
		{
			return (DataPairs[index].Value - MinValue) / CurrentRange;
		}

		public void Render(Material graphMaterial)
		{
			if (CurrentRange == 0f || !((float)DataPairs.Count > 1f))
			{
				return;
			}
			smethod_1(graphMaterial, IsWorldSpace);
			float offset = ((DataPairs.Count != MaxNumPoints) ? (1f - (float)DataPairs.Count / (float)MaxNumPoints) : 0f);
			for (int i = 1; i < DataPairs.Count; i++)
			{
				Vector3 pos = new Vector3(GetX(i - 1, offset), GetY(i - 1), 0f);
				Vector3 pos2 = new Vector3(GetX(i, offset), GetY(i), 0f);
				pos = method_1(pos);
				pos2 = method_1(pos2);
				if (DataPairs[i].Mark)
				{
					GL.Color(DataPairs[i].MarkColor);
					Vector3 a = method_1(new Vector3(GetX(i, offset), -0.1f, 0f));
					Vector3 b = pos2;
					DrawLine(a, b, invertCoordinate: false);
				}
				GL.Color(DataPairs[i].Color);
				DrawLine(pos, pos2, invertCoordinate: false);
			}
			if (IsWorldSpace)
			{
				Vector4 rectangle = Rectangle;
				rectangle.x -= 0.5f;
				rectangle.y -= 0.5f;
				DrawRectangle(rectangle, IsWorldSpace, TRS.TRS, invertCoordinate: true);
			}
			else
			{
				DrawRectangle(Rectangle, IsWorldSpace, TRS.TRS, invertCoordinate: true);
			}
			smethod_2();
			UpdateText();
		}

		public Vector3 method_1(Vector3 pos)
		{
			if (IsWorldSpace)
			{
				pos.x -= 0.5f;
				pos.y -= 0.5f;
				pos = TRS.TRS.MultiplyPoint(pos);
			}
			else
			{
				pos = Vector3.Min(Vector3.one, pos);
				pos.x *= Rectangle.z;
				pos.y *= Rectangle.w;
				pos.x += Rectangle.x;
				pos.y = 1f - Rectangle.y - Rectangle.w + pos.y;
			}
			return pos;
		}

		public void UpdateText()
		{
			if ((bool)MaxText)
			{
				MaxText.text = MaxValue.ToString("0.00");
			}
			if ((bool)MinText)
			{
				MinText.text = MinValue.ToString("0.00");
			}
			if ((bool)AvgText)
			{
				AvgText.text = AverageValue.ToString("0.00");
			}
		}
	}

	public class GClass12
	{
		public Dictionary<string, GClass11> Graphs;

		public GClass12()
		{
			Graphs = new Dictionary<string, GClass11>();
		}

		public void Update()
		{
			foreach (KeyValuePair<string, GClass11> graph in Graphs)
			{
				graph.Value.Update();
			}
		}

		public GClass11 Retrieve(string key, float value)
		{
			if (!Graphs.TryGetValue(key, out var value2))
			{
				value2 = new GClass11();
				value2.Name = key;
				Graphs.Add(key, value2);
			}
			return value2;
		}

		public void ResetAll()
		{
			foreach (KeyValuePair<string, GClass11> graph in Graphs)
			{
				graph.Value.Reset();
			}
			Graphs.Clear();
		}

		public bool Reset(string key)
		{
			GClass11 value;
			bool num = Graphs.TryGetValue(key, out value);
			if (num)
			{
				value.Reset();
				Graphs.Remove(key);
			}
			return num;
		}

		public void Plot(string key, float value)
		{
			Retrieve(key, value).AddPair(value);
		}

		public void Plot(string key, float value, GStruct0 trs)
		{
			Retrieve(key, value).AddPair(value, trs);
		}

		public void Plot(string key, float value, Color color)
		{
			Retrieve(key, value).AddPair(value, color);
		}

		public void Plot(string key, float value, Color color, Rect rectangle)
		{
			Retrieve(key, value).AddPair(value, color, rectangle);
		}

		public void Plot(string key, float value, Color color, bool mark, Color markColor, Rect rectangle)
		{
			Retrieve(key, value).AddPair(value, color, mark, markColor, rectangle);
		}

		public void Plot(string key, float value, Color color, GStruct0 trs)
		{
			Retrieve(key, value).AddPair(value, color, trs);
		}
	}

	public static GClass12 Graph;

	public static GameObject GraphManagerUI;

	public static GameObject GraphsUI;

	private Material material_0;

	public static bool smethod_0()
	{
		return false;
	}

	public static void DrawRectangle(Vector4 rect, bool isWorldSpace, Matrix4x4 trs, bool invertCoordinate)
	{
		GL.Color(Color.white);
		Vector3 vector = new Vector3(rect.x, rect.y, 0f);
		Vector3 vector2 = new Vector3(rect.x + rect.z, rect.y, 0f);
		Vector3 vector3 = new Vector3(rect.x, rect.y + rect.w, 0f);
		Vector3 vector4 = new Vector3(rect.x + rect.z, rect.y + rect.w, 0f);
		if (isWorldSpace)
		{
			vector = trs.MultiplyPoint(vector);
			vector2 = trs.MultiplyPoint(vector2);
			vector3 = trs.MultiplyPoint(vector3);
			vector4 = trs.MultiplyPoint(vector4);
		}
		if (isWorldSpace)
		{
			invertCoordinate = false;
		}
		DrawLine(vector, vector2, invertCoordinate);
		DrawLine(vector3, vector4, invertCoordinate);
		DrawLine(vector, vector3, invertCoordinate);
		DrawLine(vector2, vector4, invertCoordinate);
	}

	public static void DrawLine(Vector3 A, Vector3 B, bool invertCoordinate)
	{
		if (invertCoordinate)
		{
			A.y = 1f - A.y;
			B.y = 1f - B.y;
		}
		GL.Vertex(A);
		GL.Vertex(B);
	}

	public static void smethod_1(Material material, bool isWorldSpace)
	{
		material.SetPass(0);
		if (!isWorldSpace)
		{
			GL.LoadOrtho();
		}
		GL.Begin(1);
	}

	public static void smethod_2()
	{
		GL.End();
	}

	public static GameObject smethod_3()
	{
		if (GraphManagerUI == null)
		{
			GraphManagerUI = GameObject.Find("GraphManager_UI");
			if (GraphManagerUI == null)
			{
				GraphManagerUI = new GameObject();
				GraphManagerUI.transform.position = new Vector3(0f, 0f, 0f);
				GraphManagerUI.name = "GraphManager_UI";
			}
		}
		if ((bool)GraphManagerUI && GraphsUI == null)
		{
			GraphsUI = GameObject.Find("Graphs_UI");
			if (GraphsUI == null)
			{
				GraphsUI = new GameObject();
				GraphsUI.transform.parent = GraphManagerUI.transform;
				GraphsUI.name = "Graphs_UI";
			}
		}
		return GraphManagerUI;
	}

	public void Start()
	{
		Graph = new GClass12();
		if (!material_0)
		{
			material_0 = new Material(GClass872.Find("Hidden/GPUGraph"));
			material_0.hideFlags = HideFlags.HideAndDontSave;
		}
		smethod_3();
	}

	public void Update()
	{
		if (Graph != null)
		{
			Graph.Update();
			return;
		}
		Graph = new GClass12();
		smethod_3();
	}

	public void method_0()
	{
		if (Graph == null)
		{
			return;
		}
		foreach (KeyValuePair<string, GClass11> graph in Graph.Graphs)
		{
			graph.Value.Render(material_0);
		}
	}

	public static void SaveCurrentResults(string filePath)
	{
		if (Graph == null)
		{
			return;
		}
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
		StreamWriter streamWriter = new StreamWriter(filePath, append: true);
		streamWriter.WriteLine("Last movement history (horizontal lines): ");
		int num = 10;
		int num2 = 0;
		string text = "";
		foreach (KeyValuePair<string, GClass11> graph in Graph.Graphs)
		{
			foreach (GStruct1 dataPair in graph.Value.DataPairs)
			{
				num2++;
				text = text + dataPair.Value.ToString("0.00") + " | ";
				if (num2 == num)
				{
					streamWriter.WriteLine(text);
					num2 = 0;
					text = "";
				}
			}
			streamWriter.WriteLine(text);
			num2 = 0;
			text = "";
			streamWriter.WriteLine();
			streamWriter.WriteLine();
			streamWriter.WriteLine();
			streamWriter.WriteLine();
			streamWriter.WriteLine();
		}
		streamWriter.Close();
	}

	public void OnRenderObject()
	{
		method_0();
	}
}

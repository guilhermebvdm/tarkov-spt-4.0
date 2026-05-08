using System;
using System.Collections.Generic;
using ChartAndGraph;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class GClass1670 : GClass1669
{
	[NonSerialized]
	public List<Vector3> List_3;

	[NonSerialized]
	public List<Vector2> List_4;

	[NonSerialized]
	public List<int>[] List_5;

	[NonSerialized]
	public bool Bool_1;

	public GClass1670(bool isCanvas)
		: this(1)
	{
		method_3();
		Bool_1 = isCanvas;
	}

	public GClass1670(int groups)
	{
		method_3();
		List_5 = new List<int>[groups];
	}

	public void method_3()
	{
		if (List_3 == null)
		{
			List_3 = new List<Vector3>();
		}
		if (List_4 == null)
		{
			List_4 = new List<Vector2>();
		}
	}

	public int AddVertex(UIVertex v)
	{
		return AddVertex(v.position, v.uv0);
	}

	[CanBeNull]
	public override BillboardText AddText(AnyChart chart, Text prefab, Transform parentTransform, int fontSize, float fontScale, string text, float x, float y, float z, float angle, object userData)
	{
		if (Bool_1)
		{
			return null;
		}
		return base.AddText(chart, prefab, parentTransform, fontSize, fontScale, text, x, y, z, angle, userData);
	}

	public int AddVertex(Vector3 pos, Vector2 uv)
	{
		if (List_3.Count != List_4.Count)
		{
			throw new InvalidOperationException();
		}
		int count = List_3.Count;
		List_3.Add(pos);
		List_4.Add(uv);
		return count;
	}

	public void AddTriangle(int x, int y, int z)
	{
		List<int> tringleList = method_5(0);
		method_4(tringleList, x, y, z);
	}

	public void method_4(List<int> tringleList, int x, int y, int z)
	{
		tringleList.Add(x);
		tringleList.Add(y);
		tringleList.Add(z);
	}

	public override void Clear()
	{
		base.Clear();
		List<int>[] list_ = List_5;
		for (int i = 0; i < list_.Length; i++)
		{
			list_[i]?.Clear();
		}
		List_3.Clear();
		List_4.Clear();
	}

	public List<int> method_5(int subMeshGroup)
	{
		List<int> list;
		if (List_5[subMeshGroup] == null)
		{
			list = new List<int>();
			List_5[subMeshGroup] = list;
		}
		else
		{
			list = List_5[subMeshGroup];
		}
		return list;
	}

	public override void AddYZRect(Rect rect, int subMeshGroup, float xPosition)
	{
		if (!Bool_1)
		{
			method_3();
			Vector2[] array = method_1(rect, (base.Orientation != ChartOrientation.Vertical) ? ChartOrientation.Vertical : ChartOrientation.Horizontal);
			int num = AddVertex(new Vector3(xPosition, rect.xMin, rect.yMin), array[1]);
			int y = AddVertex(new Vector3(xPosition, rect.xMax, rect.yMin), array[3]);
			int y2 = AddVertex(new Vector3(xPosition, rect.xMin, rect.yMax), array[0]);
			int num2 = AddVertex(new Vector3(xPosition, rect.xMax, rect.yMax), array[2]);
			List<int> tringleList = method_5(subMeshGroup);
			method_4(tringleList, num, y, num2);
			method_4(tringleList, num2, y2, num);
			num = AddVertex(new Vector3(xPosition, rect.xMin, rect.yMin), array[1]);
			y = AddVertex(new Vector3(xPosition, rect.xMax, rect.yMin), array[3]);
			y2 = AddVertex(new Vector3(xPosition, rect.xMin, rect.yMax), array[0]);
			num2 = AddVertex(new Vector3(xPosition, rect.xMax, rect.yMax), array[2]);
			method_4(tringleList, num2, y, num);
			method_4(tringleList, num, y2, num2);
		}
	}

	public override void AddXZRect(Rect rect, int subMeshGroup, float yPosition)
	{
		if (!Bool_1)
		{
			method_3();
			Vector2[] array = method_0(rect);
			int num = AddVertex(new Vector3(rect.xMin, yPosition, rect.yMin), array[0]);
			int y = AddVertex(new Vector3(rect.xMax, yPosition, rect.yMin), array[1]);
			int y2 = AddVertex(new Vector3(rect.xMin, yPosition, rect.yMax), array[2]);
			int num2 = AddVertex(new Vector3(rect.xMax, yPosition, rect.yMax), array[3]);
			List<int> tringleList = method_5(subMeshGroup);
			method_4(tringleList, num, y, num2);
			method_4(tringleList, num2, y2, num);
			num = AddVertex(new Vector3(rect.xMin, yPosition, rect.yMin), array[0]);
			y = AddVertex(new Vector3(rect.xMax, yPosition, rect.yMin), array[1]);
			y2 = AddVertex(new Vector3(rect.xMin, yPosition, rect.yMax), array[2]);
			num2 = AddVertex(new Vector3(rect.xMax, yPosition, rect.yMax), array[3]);
			method_4(tringleList, num2, y, num);
			method_4(tringleList, num, y2, num2);
		}
	}

	public override void AddXYRect(Rect rect, int subMeshGroup, float depth)
	{
		method_3();
		Vector2[] array = method_0(rect);
		int num = AddVertex(new Vector3(rect.xMin, rect.yMin, depth), array[0]);
		int y = AddVertex(new Vector3(rect.xMax, rect.yMin, depth), array[1]);
		int y2 = AddVertex(new Vector3(rect.xMin, rect.yMax, depth), array[2]);
		int num2 = AddVertex(new Vector3(rect.xMax, rect.yMax, depth), array[3]);
		List<int> tringleList = method_5(subMeshGroup);
		method_4(tringleList, num, y, num2);
		method_4(tringleList, num2, y2, num);
		num = AddVertex(new Vector3(rect.xMin, rect.yMin, depth), array[0]);
		y = AddVertex(new Vector3(rect.xMax, rect.yMin, depth), array[1]);
		y2 = AddVertex(new Vector3(rect.xMin, rect.yMax, depth), array[2]);
		num2 = AddVertex(new Vector3(rect.xMax, rect.yMax, depth), array[3]);
		method_4(tringleList, num2, y, num);
		method_4(tringleList, num, y2, num2);
	}

	public override void AddQuad(UIVertex vLeftTop, UIVertex vRightTop, UIVertex vLeftBottom, UIVertex vRightBottom)
	{
		method_3();
		int num = AddVertex(vLeftTop);
		int y = AddVertex(vRightTop);
		int y2 = AddVertex(vLeftBottom);
		int num2 = AddVertex(vRightBottom);
		List<int> tringleList = method_5(0);
		method_4(tringleList, num, y, num2);
		method_4(tringleList, num2, y2, num);
	}

	public Color[] method_6()
	{
		Color[] array = new Color[List_3.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Color.white;
		}
		return array;
	}

	public void ApplyToMesh(Mesh mesh)
	{
		method_3();
		mesh.Clear();
		mesh.subMeshCount = List_5.Length;
		mesh.vertices = List_3.ToArray();
		mesh.colors = method_6();
		mesh.uv = List_4.ToArray();
		for (int i = 0; i < List_5.Length; i++)
		{
			mesh.SetTriangles((List_5[i] == null) ? new int[0] : List_5[i].ToArray(), i);
		}
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}

	public Mesh Generate([CanBeNull] Mesh mesh)
	{
		method_3();
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		mesh.subMeshCount = List_5.Length;
		mesh.SetVertices(List_3);
		mesh.SetUVs(0, List_4);
		mesh.colors = method_6();
		for (int i = 0; i < List_5.Length; i++)
		{
			if (List_5[i] == null)
			{
				mesh.SetTriangles(new int[0], i);
			}
			else
			{
				mesh.SetTriangles(List_5[i], i);
			}
		}
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}

	public Mesh Generate()
	{
		method_3();
		Mesh mesh = new Mesh
		{
			subMeshCount = List_5.Length,
			vertices = List_3.ToArray(),
			uv = List_4.ToArray(),
			colors = method_6()
		};
		for (int i = 0; i < List_5.Length; i++)
		{
			mesh.SetTriangles((List_5[i] == null) ? new int[0] : List_5[i].ToArray(), i);
		}
		mesh.RecalculateNormals();
		return mesh;
	}
}

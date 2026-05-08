using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChartAndGraph;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class CylinderPathGenerator : SmoothPathGenerator
{
	public int CircleVertices = 10;

	[Range(0.01f, 10f)]
	public float HeightRatio = 1f;

	private Vector3[] vector3_0;

	private Vector3[] vector3_1;

	private List<int> list_1 = new List<int>();

	private List<Vector2> list_2 = new List<Vector2>();

	private List<Vector3> list_3 = new List<Vector3>();

	public void method_1()
	{
		if (vector3_0 == null || vector3_0.Length != CircleVertices)
		{
			vector3_0 = new Vector3[CircleVertices];
			for (int i = 0; i < CircleVertices; i++)
			{
				float f = (float)i / (float)CircleVertices * MathF.PI * 2f;
				vector3_0[i] = new Vector3(Mathf.Cos(f), Mathf.Sin(f), 0f);
			}
			vector3_1 = new Vector3[CircleVertices];
		}
	}

	public int method_2(float thickness, Quaternion angle, Vector3 center, float u)
	{
		method_1();
		vector3_0.CopyTo(vector3_1, 0);
		for (int i = 0; i < vector3_1.Length; i++)
		{
			vector3_1[i] *= thickness;
			vector3_1[i].y *= HeightRatio;
			vector3_1[i] = angle * vector3_1[i];
			vector3_1[i] += center;
			float y = (float)i / (float)(vector3_1.Length - 1);
			list_2.Add(new Vector2(u, y));
		}
		int count = list_3.Count;
		list_3.AddRange(vector3_1);
		return count;
	}

	public void method_3(List<int> tringles, int from, int to)
	{
		if (CircleVertices <= 1)
		{
			return;
		}
		for (int i = 0; i < CircleVertices; i++)
		{
			int num = i - 1;
			if (num < 0)
			{
				num = CircleVertices - 1;
			}
			int item = from + num;
			int item2 = to + num;
			int item3 = from + i;
			int item4 = to + i;
			tringles.Add(item);
			tringles.Add(item3);
			tringles.Add(item4);
			tringles.Add(item4);
			tringles.Add(item2);
			tringles.Add(item);
		}
	}

	public override void Generator(Vector3[] path, float thickness, bool closed)
	{
		if (!EnsureMeshFilter())
		{
			return;
		}
		Clear();
		if (path.Length <= 1)
		{
			return;
		}
		list_1.Clear();
		list_2.Clear();
		list_3.Clear();
		ModifyPath(path, closed);
		if (TmpCenters.Count <= 1)
		{
			return;
		}
		float num = 0f;
		int num2 = method_2(thickness, LookRotation(TmpCenters[1] - TmpCenters[0]), TmpCenters[0], num);
		if (!closed)
		{
			int num3 = method_2(thickness, LookRotation(TmpCenters[1] - TmpCenters[0]), TmpCenters[0], num);
			list_3.Add(TmpCenters[0]);
			list_2.Add(new Vector2(0f, 0.5f));
			for (int i = 0; i < CircleVertices; i++)
			{
				int num4 = (i + 1) % CircleVertices;
				list_1.Add(num3 + CircleVertices);
				list_1.Add(num3 + num4);
				list_1.Add(num3 + i);
			}
		}
		Vector3 vector = Vector3.zero;
		Quaternion angle = Quaternion.identity;
		for (int j = 1; j < TmpCenters.Count; j++)
		{
			Vector3 vector2 = TmpCenters[j - 1];
			vector = TmpCenters[j];
			Vector3 diff = vector - vector2;
			float magnitude = diff.magnitude;
			num += magnitude;
			angle = LookRotation(diff);
			int num5 = method_2(thickness, angle, vector, 0f);
			method_3(list_1, num2, num5);
			num2 = num5;
		}
		if (!closed)
		{
			int num6 = method_2(thickness, angle, vector, 1f);
			list_3.Add(vector);
			list_2.Add(new Vector2(1f, 0.5f));
			for (int k = 0; k < CircleVertices; k++)
			{
				int num7 = (k + 1) % CircleVertices;
				list_1.Add(num6 + num7);
				list_1.Add(num6 + CircleVertices);
				list_1.Add(num6 + k);
			}
		}
		for (int l = 0; l < list_2.Count; l++)
		{
			Vector2 value = list_2[l];
			value.x /= num;
			list_2[l] = value;
		}
		Mesh mesh = new Mesh();
		mesh.vertices = list_3.ToArray();
		mesh.uv = list_2.ToArray();
		mesh.triangles = list_1.ToArray();
		mesh.RecalculateNormals();
		SetMesh(mesh);
		list_1.Clear();
		list_2.Clear();
		list_3.Clear();
	}
}

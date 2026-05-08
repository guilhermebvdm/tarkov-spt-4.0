using System.Collections.Generic;
using UnityEngine;

namespace ChartAndGraph;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class BoxPathGenerator : SmoothPathGenerator
{
	[Range(0f, 10f)]
	public float HeightRatio = 1f;

	private List<int> list_1 = new List<int>();

	private List<Vector2> list_2 = new List<Vector2>();

	private List<Vector3> list_3 = new List<Vector3>();

	public int method_1(float thickness, Quaternion rotation, Vector3 center, float u)
	{
		Vector3 vector = new Vector3(thickness, 0f, 0f);
		Vector3 vector2 = new Vector3(0f, thickness * HeightRatio, 0f);
		int count = list_3.Count;
		list_3.Add(center + rotation * (vector + vector2));
		list_3.Add(center + rotation * (-vector + vector2));
		list_3.Add(center + rotation * (-vector - vector2));
		list_3.Add(center + rotation * (vector - vector2));
		list_3.Add(list_3[count]);
		list_3.Add(list_3[count + 1]);
		list_3.Add(list_3[count + 2]);
		list_3.Add(list_3[count + 3]);
		list_2.Add(new Vector2(u, 0f));
		list_2.Add(new Vector2(u, 1f));
		list_2.Add(new Vector2(u, 0f));
		list_2.Add(new Vector2(u, 1f));
		list_2.Add(new Vector2(u, 0f));
		list_2.Add(new Vector2(u, 1f));
		list_2.Add(new Vector2(u, 0f));
		list_2.Add(new Vector2(u, 1f));
		return count;
	}

	public void method_2(List<int> tringles, int from, int to)
	{
		tringles.Add(from + 1);
		tringles.Add(to);
		tringles.Add(from);
		tringles.Add(to + 1);
		tringles.Add(to);
		tringles.Add(from + 1);
		tringles.Add(from + 2);
		tringles.Add(to + 5);
		tringles.Add(from + 5);
		tringles.Add(to + 2);
		tringles.Add(to + 5);
		tringles.Add(from + 2);
		tringles.Add(from + 3);
		tringles.Add(to + 6);
		tringles.Add(from + 6);
		tringles.Add(to + 3);
		tringles.Add(to + 6);
		tringles.Add(from + 3);
		tringles.Add(from + 4);
		tringles.Add(to + 7);
		tringles.Add(from + 7);
		tringles.Add(to + 4);
		tringles.Add(to + 7);
		tringles.Add(from + 4);
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
		if (!closed)
		{
			int num2 = method_1(thickness, LookRotation(TmpCenters[1] - TmpCenters[0]), TmpCenters[0], num);
			list_1.Add(num2 + 3);
			list_1.Add(num2 + 1);
			list_1.Add(num2);
			list_1.Add(num2 + 3);
			list_1.Add(num2 + 2);
			list_1.Add(num2 + 1);
		}
		int num3 = method_1(thickness, LookRotation(TmpCenters[1] - TmpCenters[0]), TmpCenters[0], num);
		Quaternion quaternion = Quaternion.identity;
		Vector3 vector = Vector3.zero;
		for (int i = 1; i < TmpCenters.Count; i++)
		{
			Vector3 vector2 = TmpCenters[i - 1];
			vector = TmpCenters[i];
			Vector3 diff = vector - vector2;
			float magnitude = diff.magnitude;
			if (!(magnitude <= 0.0001f))
			{
				num += magnitude;
				quaternion = LookRotation(diff);
				if (i + 1 < TmpCenters.Count)
				{
					Vector3 vector3 = TmpCenters[i + 1];
					Quaternion b = LookRotation(vector3 - vector);
					quaternion = Quaternion.Lerp(quaternion, b, 0.5f);
				}
				int num4 = method_1(thickness, quaternion, vector, num);
				method_2(list_1, num3, num4);
				num3 = num4;
			}
		}
		if (!closed)
		{
			int num5 = method_1(thickness, quaternion, vector, num);
			list_1.Add(num5);
			list_1.Add(num5 + 1);
			list_1.Add(num5 + 3);
			list_1.Add(num5 + 1);
			list_1.Add(num5 + 2);
			list_1.Add(num5 + 3);
		}
		for (int j = 0; j < list_2.Count; j++)
		{
			Vector2 value = list_2[j];
			value.x /= num;
			list_2[j] = value;
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

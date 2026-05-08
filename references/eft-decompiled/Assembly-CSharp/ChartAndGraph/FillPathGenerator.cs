using System.Collections.Generic;
using UnityEngine;

namespace ChartAndGraph;

public class FillPathGenerator : SmoothPathGenerator
{
	public bool WithTop = true;

	public bool MatchLine = true;

	private float float_0;

	private float float_1 = 10f;

	private bool bool_0;

	private List<int> list_1 = new List<int>();

	private List<Vector2> list_2 = new List<Vector2>();

	private List<Vector3> list_3 = new List<Vector3>();

	private bool bool_1;

	private int int_0;

	private float float_2;

	public override float JointSizeLink
	{
		get
		{
			if (MatchLine && bool_1)
			{
				return float_2;
			}
			return base.JointSizeLink;
		}
	}

	public override int JointSmoothingLink
	{
		get
		{
			if (MatchLine && bool_1)
			{
				return int_0;
			}
			return base.JointSmoothingLink;
		}
	}

	public void SetLineSmoothing(bool hasParent, int jointSmoothing, float jointSize)
	{
		bool_1 = hasParent;
		int_0 = jointSmoothing;
		float_2 = jointSize;
	}

	public void SetStrechFill(bool strech)
	{
		bool_0 = strech;
	}

	public void SetGraphBounds(float bottom, float top)
	{
		float_0 = bottom;
		float_1 = top;
	}

	public int method_1(Vector3 position, float thickness, float u)
	{
		Vector3 vector = new Vector3(0f, 0f, thickness);
		Vector3 vector2 = new Vector3(position.x, float_0, position.z);
		int count = list_3.Count;
		float y = 1f;
		if (!bool_0)
		{
			y = Mathf.Abs((position.y - float_0) / (float_0 - float_1));
		}
		list_3.Add(position - vector);
		list_3.Add(position + vector);
		list_3.Add(vector2 - vector);
		list_3.Add(vector2 + vector);
		list_3.Add(list_3[count + 2]);
		list_3.Add(list_3[count + 3]);
		list_2.Add(new Vector2(u, y));
		list_2.Add(new Vector2(u, y));
		list_2.Add(new Vector2(u, 0f));
		list_2.Add(new Vector2(u, 0f));
		list_2.Add(new Vector2(u, 0f));
		list_2.Add(new Vector2(u, 0f));
		if (WithTop)
		{
			list_3.Add(list_3[count]);
			list_3.Add(list_3[count + 1]);
			list_2.Add(new Vector2(u, y));
			list_2.Add(new Vector2(u, y));
		}
		return count;
	}

	public void method_2(List<int> tringles, int from, int to)
	{
		tringles.Add(from);
		tringles.Add(to);
		tringles.Add(from + 2);
		tringles.Add(to);
		tringles.Add(to + 2);
		tringles.Add(from + 2);
		tringles.Add(from + 3);
		tringles.Add(to + 1);
		tringles.Add(from + 1);
		tringles.Add(from + 3);
		tringles.Add(to + 3);
		tringles.Add(to + 1);
		tringles.Add(from + 4);
		tringles.Add(to + 4);
		tringles.Add(from + 5);
		tringles.Add(to + 4);
		tringles.Add(to + 5);
		tringles.Add(from + 5);
		if (WithTop)
		{
			tringles.Add(from + 7);
			tringles.Add(to + 6);
			tringles.Add(from + 6);
			tringles.Add(from + 7);
			tringles.Add(to + 7);
			tringles.Add(to + 6);
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
		if (TmpCenters.Count > 1)
		{
			float num = 0f;
			int num2 = method_1(TmpCenters[0], thickness, num);
			list_1.Add(num2 + 2);
			list_1.Add(num2 + 1);
			list_1.Add(num2);
			list_1.Add(num2 + 1);
			list_1.Add(num2 + 2);
			list_1.Add(num2 + 3);
			int num3 = method_1(TmpCenters[0], thickness, num);
			for (int i = 1; i < TmpCenters.Count; i++)
			{
				Vector3 vector = TmpCenters[i - 1];
				Vector3 vector2 = TmpCenters[i];
				float magnitude = (vector2 - vector).magnitude;
				num += magnitude;
				int num4 = method_1(vector2, thickness, num);
				method_2(list_1, num3, num4);
				num3 = num4;
			}
			int num5 = method_1(TmpCenters[TmpCenters.Count - 1], thickness, num);
			list_1.Add(num5);
			list_1.Add(num5 + 1);
			list_1.Add(num5 + 2);
			list_1.Add(num5 + 3);
			list_1.Add(num5 + 2);
			list_1.Add(num5 + 1);
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
}

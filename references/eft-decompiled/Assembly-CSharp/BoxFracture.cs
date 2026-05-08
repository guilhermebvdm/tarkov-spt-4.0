using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Trigger))]
[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("Destruction/Box Fracture")]
[RequireComponent(typeof(BoxCollider))]
public class BoxFracture : BaseFracture
{
	[CompilerGenerated]
	public class Class656
	{
		public Vector3[] points;

		public int i;

		public Comparison<Vector3> comparison_0;

		public int method_0(Vector3 p1, Vector3 p2)
		{
			float sqrMagnitude = (p1 - points[i]).sqrMagnitude;
			float sqrMagnitude2 = (p2 - points[i]).sqrMagnitude;
			return sqrMagnitude.CompareTo(sqrMagnitude2);
		}
	}

	[CompilerGenerated]
	public class Class657
	{
		public Vector3 point;

		public int method_0(Vector3 p1, Vector3 p2)
		{
			float sqrMagnitude = (p1 - point).sqrMagnitude;
			double num = (p2 - point).sqrMagnitude;
			return sqrMagnitude.CompareTo((float)num);
		}
	}

	private List<Vector3> list_0;

	public override void ImmediateFracture(Vector3[] points)
	{
		InitializeDestruction();
		list_0 = new List<Vector3>(points);
		int i = 0;
		while (i < points.Length)
		{
			method_0(points[i]);
			list_0.Sort(delegate(Vector3 p1, Vector3 p2)
			{
				float sqrMagnitude = (p1 - points[i]).sqrMagnitude;
				float sqrMagnitude2 = (p2 - points[i]).sqrMagnitude;
				return sqrMagnitude.CompareTo(sqrMagnitude2);
			});
			method_1(points[i]);
			if (Vertices.Count > 3)
			{
				NewVertices = Vertices.ToArray();
				NewTriangles = ConvexHull.ComputeIncremental(NewVertices);
				if (NewTriangles != null)
				{
					CreateShardMesh(points[i]);
				}
			}
			int num = i + 1;
			i = num;
		}
		FinalizeDestruction();
	}

	public override IEnumerator SpreadFracture(Vector3[] points)
	{
		InitializeDestruction();
		list_0 = new List<Vector3>(points);
		int num = 0;
		int num2 = points.Length;
		while (num2 > 0)
		{
			for (int i = 0; i < Mathf.Min(ShardsPerFrame, num2); i++)
			{
				int num3 = num2 - 1;
				num2 = num3;
				Vector3 point = points[num++];
				method_0(point);
				list_0.Sort(delegate(Vector3 p1, Vector3 p2)
				{
					float sqrMagnitude = (p1 - point).sqrMagnitude;
					double num4 = (p2 - point).sqrMagnitude;
					return sqrMagnitude.CompareTo((float)num4);
				});
				method_1(point);
				if (Vertices.Count > 3)
				{
					NewVertices = Vertices.ToArray();
					NewTriangles = ConvexHull.ComputeIncremental(NewVertices);
					if (NewTriangles != null)
					{
						CreateShardMesh(point);
					}
				}
			}
			yield return null;
		}
		FinalizeDestruction();
	}

	public void method_0(Vector3 point)
	{
		Vector3 vector = point - MaxBounds;
		Vector3 vector2 = MinBounds - point;
		BaseFracture.InitPlanes[0].w = vector.x;
		BaseFracture.InitPlanes[1].w = vector.y;
		BaseFracture.InitPlanes[2].w = vector.z;
		BaseFracture.InitPlanes[3].w = vector2.x;
		BaseFracture.InitPlanes[4].w = vector2.y;
		BaseFracture.InitPlanes[5].w = vector2.z;
		Planes.Clear();
		Planes.AddRange(BaseFracture.InitPlanes);
	}

	public void method_1(Vector3 point)
	{
		float num = float.PositiveInfinity;
		for (int i = 1; i < list_0.Count; i++)
		{
			Vector3 vector = list_0[i] - point;
			float sqrMagnitude = vector.sqrMagnitude;
			if (!((double)sqrMagnitude > (double)num * (double)num))
			{
				Vector4 item = vector.normalized;
				item.w = (0f - Mathf.Sqrt(sqrMagnitude)) / 2f;
				Planes.Add(item);
				GetVerticesInPlane();
				if (Vertices.Count > 3)
				{
					method_3();
					num = method_2();
					continue;
				}
				break;
			}
			break;
		}
	}

	public float method_2()
	{
		float num = Vertices[0].sqrMagnitude;
		int count = Vertices.Count;
		for (int i = 1; i < count; i++)
		{
			float sqrMagnitude = Vertices[i].sqrMagnitude;
			if ((double)num < (double)sqrMagnitude)
			{
				num = sqrMagnitude;
			}
		}
		return Mathf.Sqrt(num) * 2f;
	}

	public void method_3()
	{
		if (PlaneIndices.Count == Planes.Count)
		{
			return;
		}
		int num = 0;
		foreach (int planeIndex in PlaneIndices)
		{
			if (num != planeIndex)
			{
				Planes[num] = Planes[planeIndex];
			}
			num++;
		}
		int num2 = PlaneIndices.Count - num;
		if (num2 < 0)
		{
			Planes.RemoveRange(num - 1, num2);
		}
	}
}

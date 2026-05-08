using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChartAndGraph;

public abstract class SmoothPathGenerator : PathGenerator
{
	public int JointSmoothing = 2;

	public float JointSize = 0.1f;

	protected List<int> SkipJoints = new List<int>();

	private readonly List<Vector3> list_0 = new List<Vector3>();

	protected List<Vector3> TmpCenters = new List<Vector3>();

	private MeshFilter meshFilter_0;

	private Mesh mesh_0;

	public virtual int JointSmoothingLink => JointSmoothing;

	public virtual float JointSizeLink => JointSize;

	public override void Clear()
	{
		GClass1664.smethod_13(null, ref mesh_0);
	}

	public void OnDestroy()
	{
		Clear();
	}

	public bool EnsureMeshFilter()
	{
		if (meshFilter_0 == null)
		{
			meshFilter_0 = GetComponent<MeshFilter>();
		}
		return meshFilter_0 != null;
	}

	public void SetMesh(Mesh mesh)
	{
		mesh.hideFlags = HideFlags.DontSave;
		meshFilter_0.sharedMesh = mesh;
		GClass1664.smethod_13(mesh, ref mesh_0);
		MeshCollider component = GetComponent<MeshCollider>();
		if (component != null)
		{
			component.sharedMesh = mesh;
		}
	}

	public Quaternion LookRotation(Vector3 diff)
	{
		if (diff.sqrMagnitude < float.Epsilon)
		{
			return Quaternion.identity;
		}
		Vector3 normalized = new Vector3(diff.y, 0f - diff.x, 0f).normalized;
		return Quaternion.LookRotation(diff, normalized);
	}

	public void method_0(Vector3 from, Vector3 curr, Vector3 to)
	{
		if (JointSmoothingLink <= 0)
		{
			list_0.Add(curr);
			return;
		}
		for (int i = 0; i < JointSmoothingLink; i++)
		{
			float num = (float)(i + 1) / (float)(JointSmoothingLink + 1);
			float num2 = 1f - num;
			Vector3 item = num2 * num2 * from + 2f * num2 * num * curr + num * num * to;
			list_0.Add(item);
		}
	}

	public void ModifyPath(Vector3[] path, bool closed, List<Vector3> res)
	{
		if (path.Length <= 1)
		{
			return;
		}
		list_0.Clear();
		for (int i = 0; i <= path.Length; i++)
		{
			if ((i == 0 && !closed) || SkipJoints.Contains(i))
			{
				if (i < path.Length)
				{
					list_0.Add(path[i]);
				}
			}
			else if (i < path.Length - 1 || closed)
			{
				int num = i - 1;
				if (num < 0)
				{
					num = path.Length - 1;
				}
				Vector3 vector = path[num];
				Vector3 vector2 = path[i % path.Length];
				int num2 = i + 1;
				Vector3 vector3 = path[num2 % path.Length];
				Vector3 vector4 = vector2 - vector;
				Vector3 vector5 = vector3 - vector2;
				float magnitude = vector4.magnitude;
				float magnitude2 = vector5.magnitude;
				vector4.Normalize();
				vector5.Normalize();
				float num3 = Math.Min(JointSizeLink, magnitude * 0.5f);
				float num4 = Math.Min(JointSizeLink, magnitude2 * 0.5f);
				Vector3 item = vector2 - vector4 * num3;
				Vector3 item2 = vector2 + vector5 * num4;
				Vector3 vector6 = vector2 - vector4 * num3 * 0.7f;
				Vector3 vector7 = vector2 + vector5 * num4 * 0.7f;
				if (i > 0)
				{
					list_0.Add(item);
					list_0.Add(vector6);
					method_0(vector6, vector2, vector7);
					list_0.Add(vector7);
				}
				list_0.Add(item2);
			}
		}
		if (!closed)
		{
			list_0.Add(path[^1]);
		}
		res.Clear();
		res.Add(list_0[0]);
		for (int j = 1; j < list_0.Count; j++)
		{
			Vector3 vector8 = list_0[j - 1];
			Vector3 vector9 = list_0[j];
			if (!((double)(vector9 - vector8).sqrMagnitude < 1E-06))
			{
				res.Add(vector9);
			}
		}
	}

	public void ModifyPath(Vector3[] path, bool closed)
	{
		ModifyPath(path, closed, TmpCenters);
	}

	public SmoothPathGenerator()
	{
	}
}

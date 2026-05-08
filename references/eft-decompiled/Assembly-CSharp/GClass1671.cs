using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass1671
{
	public int JointSmoothing = 2;

	public float JointSize = 0.1f;

	[NonSerialized]
	public List<Vector3> List_0 = new List<Vector3>();

	[NonSerialized]
	public List<Vector3> List_1 = new List<Vector3>();

	[NonSerialized]
	public List<Vector3> List_2 = new List<Vector3>();

	public Quaternion method_0(Vector3 diff)
	{
		Vector3 normalized = new Vector3(diff.y, 0f - diff.x, 0f).normalized;
		return Quaternion.LookRotation(diff, normalized);
	}

	public void method_1(Vector3 from, Vector3 curr, Vector3 to)
	{
		if (JointSmoothing <= 0)
		{
			List_0.Add(curr);
			return;
		}
		for (int i = 0; i < JointSmoothing; i++)
		{
			float num = (float)(i + 1) / (float)(JointSmoothing + 1);
			float num2 = 1f - num;
			Vector3 item = num2 * num2 * from + 2f * num2 * num * curr + num * num * to;
			List_0.Add(item);
		}
	}

	public void method_2(Vector3 translation, float scale)
	{
		foreach (Vector3 item in List_1)
		{
			List_2.Add(item * scale + translation);
		}
	}

	public void ApplyToMesh(GClass1670 mesh)
	{
	}

	public void MultiplyPath(Vector3[] paths)
	{
		List_2.Clear();
		foreach (Vector3 translation in paths)
		{
			method_2(translation, 1f);
		}
	}

	public void ModifyPath(Vector3[] path, bool closed)
	{
		if (path.Length <= 1)
		{
			return;
		}
		List_0.Clear();
		for (int i = 0; i <= path.Length; i++)
		{
			if (i == 0 && !closed)
			{
				List_0.Add(path[0]);
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
				float num3 = Math.Min(JointSize, magnitude * 0.5f);
				float num4 = Math.Min(JointSize, magnitude2 * 0.5f);
				Vector3 item = vector2 - vector4 * num3;
				Vector3 item2 = vector2 + vector5 * num4;
				Vector3 vector6 = vector2 - vector4 * num3 * 0.7f;
				Vector3 vector7 = vector2 + vector5 * num4 * 0.7f;
				if (i > 0)
				{
					List_0.Add(item);
					List_0.Add(vector6);
					method_1(vector6, vector2, vector7);
					List_0.Add(vector7);
				}
				List_0.Add(item2);
			}
		}
		if (!closed)
		{
			List_0.Add(path[^1]);
		}
		List_1.Clear();
		List_1.Add(List_0[0]);
		for (int j = 1; j < List_0.Count; j++)
		{
			Vector3 vector8 = List_0[j - 1];
			Vector3 vector9 = List_0[j];
			if (!((double)(vector9 - vector8).sqrMagnitude < 1E-06))
			{
				List_1.Add(vector9);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class GClass635
{
	[NonSerialized]
	public const float Float_0 = 0.001f;

	[NonSerialized]
	public static NavMeshPath NavMeshPath_0 = new NavMeshPath();

	public static bool ModifyZigZagPercentOffset(Vector3[] corners, float offsetLength, float chance, float maxStepDistToOffset, float percentToSideOffset, out List<Vector3> modifedWay)
	{
		if (corners.Length <= 2)
		{
			modifedWay = null;
			return false;
		}
		List<Vector3> list = smethod_5(corners, offsetLength);
		GClass855.SideTurn turnSide = (GClass856.IsTrue100(50f) ? GClass855.SideTurn.right : GClass855.SideTurn.left);
		List<Vector3> list2 = new List<Vector3>();
		int debugIndex = 0;
		int num = 0;
		for (int i = 0; i + 2 < list.Count; i += 2)
		{
			Vector3 vector = list[i];
			Vector3 vector2 = list[i + 1];
			Vector3 p = list[i + 2];
			num = i + 2;
			if (!smethod_0(maxStepDistToOffset, vector, vector2, p, chance, percentToSideOffset, turnSide, list2, ref debugIndex))
			{
				list2.Add(vector);
				list2.Add(vector2);
			}
		}
		if (num < list.Count)
		{
			for (int j = num; j < list.Count; j++)
			{
				Vector3 item = list[j];
				list2.Add(item);
			}
		}
		modifedWay = list2;
		return true;
	}

	public static bool ModifyZigZagAllSteps(Vector3[] corners, float offsetLength, float sideOffset, out List<Vector3> modifedWay)
	{
		List<Vector3> list = smethod_5(corners, offsetLength);
		GClass855.SideTurn turnSide = (GClass856.IsTrue100(50f) ? GClass855.SideTurn.right : GClass855.SideTurn.left);
		List<Vector3> list2 = new List<Vector3>();
		int debugIndex = 0;
		int num = 0;
		for (int i = 0; i + 2 < list.Count; i += 2)
		{
			Vector3 vector = list[i];
			Vector3 vector2 = list[i + 1];
			Vector3 p = list[i + 2];
			num = i + 2;
			if (!smethod_1(sideOffset, vector, vector2, p, turnSide, list2, ref debugIndex))
			{
				list2.Add(vector);
				list2.Add(vector2);
			}
		}
		if (num < list.Count)
		{
			for (int j = num; j < list.Count; j++)
			{
				Vector3 item = list[j];
				list2.Add(item);
			}
		}
		modifedWay = list2;
		return modifedWay.Count > 2;
	}

	public static bool smethod_0(float maxDist, Vector3 p1, Vector3 p2, Vector3 p3, float chance100, float percentToSideOffset, GClass855.SideTurn turnSide, List<Vector3> collectedPath, ref int debugIndex)
	{
		if (!GClass856.IsTrue100(chance100))
		{
			return false;
		}
		float magnitude = (p1 - p3).magnitude;
		if (magnitude < maxDist)
		{
			return false;
		}
		float num = percentToSideOffset * magnitude;
		Vector3 vector = GClass855.Rotate90(GClass855.NormalizeFastSelf(p2 - p1), turnSide);
		if (!NavMesh.SamplePosition(p2 + vector * num, out var hit, num * 2f, -1))
		{
			return false;
		}
		return smethod_2(p1, p3, collectedPath, ref debugIndex, hit.position);
	}

	public static bool smethod_1(float sideOffset, Vector3 p1, Vector3 p2, Vector3 p3, GClass855.SideTurn turnSide, List<Vector3> collectedPath, ref int debugIndex)
	{
		if ((p1 - p3).magnitude < sideOffset * 2f)
		{
			return false;
		}
		Vector3 vector = GClass855.Rotate90(GClass855.NormalizeFastSelf(p2 - p1), turnSide);
		if (!NavMesh.SamplePosition(p2 + vector * sideOffset, out var hit, sideOffset, -1))
		{
			return false;
		}
		return smethod_2(p1, p3, collectedPath, ref debugIndex, hit.position);
	}

	public static bool smethod_2(Vector3 p1, Vector3 p3, List<Vector3> collectedPath, ref int debugIndex, Vector3 possibleTestPoint)
	{
		if (smethod_4(p1, possibleTestPoint, out var path) && smethod_4(possibleTestPoint, p3, out var path2) && !smethod_3(path, path2))
		{
			for (int i = 0; i < path.Length; i++)
			{
				collectedPath.Add(path[i]);
			}
			for (int j = 1; j < path2.Length - 1; j++)
			{
				collectedPath.Add(path2[j]);
			}
			debugIndex++;
			return true;
		}
		return false;
	}

	public static bool smethod_3(Vector3[] path1, Vector3[] path2)
	{
		for (int i = 1; i < path1.Length - 1; i++)
		{
			Vector3 vector = path1[i];
			for (int j = 1; j < path2.Length - 1; j++)
			{
				Vector3 vector2 = path2[j];
				if (!((vector - vector2).sqrMagnitude >= 0.01f))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool smethod_4(Vector3 from, Vector3 to, out Vector3[] path)
	{
		if (NavMesh.CalculatePath(from, to, -1, NavMeshPath_0) && NavMeshPath_0.status == NavMeshPathStatus.PathComplete)
		{
			float magnitude = (from - to).magnitude;
			if (GClass371.CalculatePathLength(NavMeshPath_0) < magnitude * 1.35f)
			{
				path = NavMeshPath_0.corners;
				return true;
			}
		}
		path = null;
		return false;
	}

	public static List<Vector3> smethod_5(Vector3[] corners, float stepLength)
	{
		List<Vector3> list = new List<Vector3>();
		int num = 0;
		for (int i = 0; i < corners.Length - 1; i++)
		{
			Vector3 vector = corners[i];
			Vector3 vector2 = corners[i + 1];
			num = i + 1;
			Vector3 v = vector2 - vector;
			Vector3 vector3 = GClass855.NormalizeFastSelf(v);
			float num2 = v.magnitude;
			list.Add(vector);
			Vector3 item = vector;
			if (num2 < stepLength)
			{
				continue;
			}
			int num3 = 10;
			while (num2 > stepLength && num3 > 0)
			{
				num3--;
				if (num2 > stepLength * 2f)
				{
					num2 -= stepLength;
					item += vector3 * stepLength;
					list.Add(item);
					continue;
				}
				float num4 = num2 * 0.5f;
				item += vector3 * num4;
				list.Add(item);
				num2 = 0f;
				break;
			}
		}
		if (num < corners.Length)
		{
			for (int j = num; j < corners.Length; j++)
			{
				Vector3 item2 = corners[j];
				list.Add(item2);
			}
		}
		return list;
	}

	public static bool smethod_6(List<Vector3> allModifWay, Vector3 c1)
	{
		if (allModifWay.Count == 0)
		{
			allModifWay.Add(c1);
			return true;
		}
		if (NavMesh.CalculatePath(allModifWay[allModifWay.Count - 1], c1, -1, NavMeshPath_0) && NavMeshPath_0.status == NavMeshPathStatus.PathComplete && NavMeshPath_0.corners.Length != 0)
		{
			for (int i = 0; i < NavMeshPath_0.corners.Length; i++)
			{
				Vector3 vector = allModifWay[allModifWay.Count - 1];
				if ((NavMeshPath_0.corners[i] - vector).sqrMagnitude > 0.001f)
				{
					allModifWay.Add(NavMeshPath_0.corners[i]);
				}
			}
			return true;
		}
		return false;
	}
}

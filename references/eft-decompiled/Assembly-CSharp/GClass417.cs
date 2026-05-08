using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class GClass417
{
	[Serializable]
	[CompilerGenerated]
	public class Class157
	{
		public static readonly Class157 class157_0 = new Class157();

		public static Func<NavGraphEditorPathValue, float> func_0;

		public float method_0(NavGraphEditorPathValue x)
		{
			return x.Coef;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct7
	{
		public bool withDebug;
	}

	public const int MAX_ITERATION = 10;

	public static void CachePathesToVoxel(NavGraphVoxel navGraphVoxel, bool withDebug = false)
	{
		Struct7 struct7_ = default(Struct7);
		struct7_.withDebug = withDebug;
		HashSet<int> usedPathes = new HashSet<int>();
		HashSet<NavGraphEditorPathValue> hashSet = new HashSet<NavGraphEditorPathValue>();
		smethod_3(navGraphVoxel, usedPathes, hashSet, ref struct7_);
		foreach (NavGraphVoxel neightbourhood in navGraphVoxel.Neightbourhoods)
		{
			smethod_3(neightbourhood, usedPathes, hashSet, ref struct7_);
		}
		List<NavGraphEditorPathValue> list = hashSet.ToList();
		smethod_0(navGraphVoxel, list, struct7_.withDebug);
		navGraphVoxel.Pathes = list;
	}

	public static void smethod_0(NavGraphVoxel navGraphVoxel, List<NavGraphEditorPathValue> list, bool withDebug = false)
	{
		if (list.Count == 0)
		{
			return;
		}
		HashSet<NavGraphEditorPathValue> hashSet = new HashSet<NavGraphEditorPathValue>();
		list.Max((NavGraphEditorPathValue x) => x.Coef);
		Vector3 vector = navGraphVoxel.GetSize() * 0.5f;
		Vector3 boxPos = navGraphVoxel.Position + vector;
		Vector3 p = new Vector3(boxPos.x + vector.x, 0f, boxPos.z + vector.z);
		Vector3 p2 = new Vector3(boxPos.x + vector.x, 0f, boxPos.z - vector.z);
		Vector3 p3 = new Vector3(boxPos.x - vector.x, 0f, boxPos.z - vector.z);
		Vector3 p4 = new Vector3(boxPos.x - vector.x, 0f, boxPos.z + vector.z);
		foreach (NavGraphEditorPathValue item in list)
		{
			NavGraphEditorPath path = item.Path;
			Vector3 position = path.From.Position;
			Vector3 position2 = path.To.Position;
			bool flag;
			if (flag = GClass369.PointInOABB(position, boxPos, vector * 2f) || GClass369.PointInOABB(position2, boxPos, vector * 2f))
			{
				Debug.DrawRay(position, Vector3.up * 4f, Color.cyan, 5f);
			}
			if (path.Dist > vector.x && !flag && smethod_1(p, p2, p3, p4, position, position2))
			{
				flag = true;
			}
			if (flag)
			{
				hashSet.Add(item);
			}
		}
		foreach (NavGraphEditorPathValue item2 in list)
		{
			if (hashSet.Contains(item2))
			{
				item2.Coef = 100f;
			}
			else
			{
				item2.Coef = 50f;
			}
		}
	}

	public static bool smethod_1(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 tmp1, Vector3 tmp2)
	{
		if (GClass369.GetCrossPoint(p1, p2, tmp1, tmp2).HasValue)
		{
			return true;
		}
		if (GClass369.GetCrossPoint(p2, p3, tmp1, tmp2).HasValue)
		{
			return true;
		}
		if (GClass369.GetCrossPoint(p3, p4, tmp1, tmp2).HasValue)
		{
			return true;
		}
		if (GClass369.GetCrossPoint(p4, p1, tmp1, tmp2).HasValue)
		{
			return true;
		}
		return false;
	}

	public static void smethod_2(NavGraphEditorPoint point, HashSet<int> usedPath, HashSet<NavGraphEditorPathValue> navGraphEditorPaths, float curDist, float maxDist, int iteration, bool withDebug = false)
	{
		iteration++;
		if (iteration > 10)
		{
			return;
		}
		foreach (NavGraphEditorPointWay item2 in point.ClosestPointsWaysArray)
		{
			if (!usedPath.Contains(item2.Path.Id))
			{
				usedPath.Add(item2.Path.Id);
				NavGraphEditorPathValue item = new NavGraphEditorPathValue(item2.Path, iteration);
				navGraphEditorPaths.Add(item);
				float num = curDist + item2.Path.Dist;
				if (num < maxDist)
				{
					smethod_2(item2.TargetPoint, usedPath, navGraphEditorPaths, num, maxDist, iteration, withDebug);
				}
			}
		}
	}

	[CompilerGenerated]
	public static void smethod_3(NavGraphVoxel voxel, HashSet<int> usedPathes, HashSet<NavGraphEditorPathValue> navGraphEditorPaths, ref Struct7 struct7_0)
	{
		foreach (NavGraphEditorPoint point in voxel.Points)
		{
			smethod_2(point, usedPathes, navGraphEditorPaths, 0f, 8f, 0, struct7_0.withDebug);
		}
	}
}

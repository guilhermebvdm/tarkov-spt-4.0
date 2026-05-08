using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class StartCoverFinderTester : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class126
	{
		public static readonly Class126 class126_0 = new Class126();

		public static Func<NavGraphVoxelSimple, bool> func_0;

		public static Func<NavGraphVoxelSimple, bool> func_1;

		public bool method_0(NavGraphVoxelSimple x)
		{
			if (x.Points != null)
			{
				return x.Points.Count > 0;
			}
			return false;
		}

		public bool method_1(NavGraphVoxelSimple x)
		{
			if (x.Points != null)
			{
				return x.Points.Count <= 0;
			}
			return true;
		}
	}

	[CompilerGenerated]
	public class Class127
	{
		public StartCoverFinderTester startCoverFinderTester_0;

		public Vector3 pos;

		public Vector3 method_0(int i)
		{
			return startCoverFinderTester_0.LastVoxel.Points[i].Position - pos;
		}
	}

	public GroupPoint LastPoint;

	public bool WorkOnUpdate = true;

	public AICoversData CoversData;

	public GroupPoint LastSearched;

	public NavGraphVoxelSimple LastVoxel;

	public void DoFind(AICoversData data)
	{
	}

	public void DropLastPoint()
	{
		LastPoint = null;
	}

	public void ManualUpdate()
	{
		if (WorkOnUpdate)
		{
			DoSearch();
		}
	}

	public void DoSearch()
	{
		if (CoversData == null)
		{
			CoversData = AICoversData.CreateOrFind();
			CoversData.RestoreData();
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		LastSearched = GetStartPoint2(CoversData, base.transform.position);
		stopwatch.Stop();
		UnityEngine.Debug.LogError($"period big inner:{stopwatch.ElapsedTicks}");
		Stopwatch stopwatch2 = new Stopwatch();
		stopwatch2.Start();
		LastSearched = GetStartPoint(CoversData, base.transform.position);
		stopwatch2.Stop();
		UnityEngine.Debug.LogError($"period big outher:{stopwatch2.ElapsedTicks}");
		Stopwatch stopwatch3 = new Stopwatch();
		stopwatch3.Start();
		LastSearched = GetStartPointSingle(CoversData, base.transform.position);
		stopwatch3.Stop();
		UnityEngine.Debug.LogError($"period single:{stopwatch3.ElapsedTicks}");
	}

	public void OnDrawGizmos()
	{
		if (LastVoxel != null)
		{
			List<NavGraphVoxelSimple> voxelesExtended = CoversData.GetVoxelesExtended(LastVoxel.IndexX, LastVoxel.IndexY, LastVoxel.IndexZ, 1, onlyWithPoints: false);
			List<NavGraphVoxelSimple> list = voxelesExtended.Where((NavGraphVoxelSimple x) => x.Points != null && x.Points.Count > 0).ToList();
			List<NavGraphVoxelSimple> list2 = voxelesExtended.Where((NavGraphVoxelSimple x) => x.Points == null || x.Points.Count <= 0).ToList();
			Color value = (Gizmos.color = Color.cyan);
			foreach (NavGraphVoxelSimple item in list2)
			{
				item.DrawGizmos(drawClosestPoint: false, drawDoorsLinks: false, -1f, drawPointsInside: true, value);
			}
			value = Color.blue;
			value.a = 0.5f;
			foreach (NavGraphVoxelSimple item2 in list)
			{
				Gizmos.color = value;
				item2.DrawGizmos(drawClosestPoint: false, drawDoorsLinks: false, -1f, drawPointsInside: true, value);
			}
		}
		if (LastSearched != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(LastSearched.Position, 0.3f);
			Gizmos.DrawWireSphere(LastSearched.Position, 0.4f);
			Gizmos.color = Color.white;
			Gizmos.DrawLine(LastSearched.Position, base.transform.position);
		}
		else
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(base.transform.position, 0.3f);
			Gizmos.DrawWireSphere(base.transform.position, 0.4f);
		}
	}

	public GroupPoint GetStartPointSingle(AICoversData data, Vector3 pos)
	{
		LastVoxel = data.GetVoxelSafe(pos);
		if (LastVoxel.Points != null && LastVoxel.Points.Count > 0)
		{
			int nearEntity = GClass369.GetNearEntity(LastVoxel.Points.Count, (int i) => LastVoxel.Points[i].Position - pos);
			return LastVoxel.Points[nearEntity];
		}
		return LastVoxel.PointStartSearch;
	}

	public GroupPoint GetStartPoint2(AICoversData data, Vector3 pos)
	{
		LastVoxel = data.GetVoxelSafe(pos);
		if (LastPoint != null)
		{
			Vector3 vector = LastPoint.Position - pos;
			if (Mathf.Abs(vector.y) < 1f && vector.sqrMagnitude < 16f)
			{
				return LastPoint;
			}
		}
		if (LastVoxel == null)
		{
			return null;
		}
		GroupPoint pointStartSearch = LastVoxel.PointStartSearch;
		GroupPoint closestsPointInVoxelesExtended = data.GetClosestsPointInVoxelesExtended(LastVoxel.IndexX, LastVoxel.IndexY, LastVoxel.IndexZ, 1, pos, null);
		if (closestsPointInVoxelesExtended != null)
		{
			return closestsPointInVoxelesExtended;
		}
		if (pointStartSearch == null)
		{
			using (List<GroupPoint>.Enumerator enumerator = LastVoxel.Points.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					return enumerator.Current;
				}
			}
			return null;
		}
		return pointStartSearch;
	}

	public GroupPoint GetStartPoint(AICoversData data, Vector3 pos)
	{
		LastVoxel = data.GetVoxelSafe(pos);
		if (LastPoint != null)
		{
			Vector3 vector = LastPoint.Position - pos;
			if (Mathf.Abs(vector.y) < 1f && vector.sqrMagnitude < 16f)
			{
				return LastPoint;
			}
		}
		if (LastVoxel == null)
		{
			return null;
		}
		GroupPoint pointStartSearch = LastVoxel.PointStartSearch;
		List<NavGraphVoxelSimple> voxelesExtended = data.GetVoxelesExtended(LastVoxel.IndexX, LastVoxel.IndexY, LastVoxel.IndexZ, 1, onlyWithPoints: true);
		if (voxelesExtended.Count > 0)
		{
			GroupPoint result = null;
			float num = float.MaxValue;
			int num2 = 0;
			{
				foreach (NavGraphVoxelSimple item in voxelesExtended)
				{
					for (int i = 0; i < item.Points.Count; i++)
					{
						num2++;
						GroupPoint groupPoint = item.Points[i];
						float sqrMagnitude = (groupPoint.Position - pos).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							result = groupPoint;
						}
					}
				}
				return result;
			}
		}
		if (pointStartSearch == null)
		{
			using (List<GroupPoint>.Enumerator enumerator2 = LastVoxel.Points.GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					return enumerator2.Current;
				}
			}
			return null;
		}
		return pointStartSearch;
	}
}

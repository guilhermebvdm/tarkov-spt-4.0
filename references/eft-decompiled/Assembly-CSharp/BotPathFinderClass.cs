using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class BotPathFinderClass : GClass429
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct15
	{
		public HashSet<AICorePoint> hash;

		public AICorePoint to;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct16
	{
		public Action errorCallback;

		public Vector3 from;

		public GroupPoint coverFrom;

		public IAICorePointLink coverTo;
	}

	[NonSerialized]
	public const int Int_0 = 5;

	[NonSerialized]
	public const int Int_1 = 10;

	[NonSerialized]
	public static float Float_0;

	public int StartsFromDebug;

	public float StartsDiffDebug;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public BotMover BotMover_0;

	[NonSerialized]
	public PathControllerClass PathControllerClass;

	[NonSerialized]
	public AICoversData AicoversData_0;

	[NonSerialized]
	public GClass546 Gclass546_0 = new GClass546();

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public BotMover BotMover_1;

	[NonSerialized]
	public int Int_4 = 5;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public bool SlowAtTheEnd
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public static EPathCalcResult CalculatePathFull(int connectionGroup, Vector3 from, IAICorePointLink coverTo, AICoversData covers, out Vector3[] path, Action errorCallback = null)
	{
		Struct16 struct16_ = default(Struct16);
		struct16_.errorCallback = errorCallback;
		struct16_.from = from;
		struct16_.coverTo = coverTo;
		if (FindPath(struct16_.from, struct16_.coverTo.Position, out path))
		{
			return EPathCalcResult.PathComplete;
		}
		if (!GClass855.IsOnNavMesh(struct16_.from, 0.1f) && NavMesh.SamplePosition(struct16_.from + Vector3.up * 0.1f, out var hit, 0.5f, -1))
		{
			Mathf.Abs(hit.position.y - struct16_.from.y);
			struct16_.from = hit.position;
		}
		struct16_.coverFrom = GetClosestPointWithWay(covers, struct16_.from, connectionGroup, out var corners);
		if (corners == null && !FindPath(struct16_.from, struct16_.coverFrom.Position, out corners))
		{
			Vector3[] corners2;
			foreach (AICorePoint corePoint in covers.AICorePointsHolder.CorePoints)
			{
				if (corePoint.ConnectionGroupId == struct16_.coverTo.CorePointInGame.ConnectionGroupId && FindPath(struct16_.from, corePoint.Position, out corners2))
				{
					if (corePoint == struct16_.coverFrom.CorePointInGame && FindPath(struct16_.coverFrom.CorePointInGame.Position, struct16_.coverFrom.Position, out var corners3))
					{
						Vector3[] array = LinkWays(new List<Vector3[]> { corners2, corners3 });
						path = array;
						return EPathCalcResult.PathComplete;
					}
					if (smethod_1(corePoint, struct16_.coverTo.CorePointInGame, out var way) && FindPath(struct16_.coverTo.CorePointInGame.Position, struct16_.coverTo.Position, out var corners4))
					{
						Vector3[] array2 = LinkWays(new List<Vector3[]> { corners2, way, corners4 });
						path = array2;
						return EPathCalcResult.PathComplete;
					}
				}
			}
			if (FindPath(struct16_.from, struct16_.coverFrom.CorePointInGame.Position, out corners2) && FindPath(struct16_.coverFrom.CorePointInGame.Position, struct16_.coverTo.Position, out var corners5))
			{
				Vector3[] array3 = LinkWays(new List<Vector3[]> { corners2, corners5 });
				path = array3;
				return EPathCalcResult.PathComplete;
			}
			if (FindPath(struct16_.from, struct16_.coverFrom.CorePointInGame.Position, out corners2))
			{
				if (FindPath(struct16_.coverFrom.CorePointInGame.Position, struct16_.coverTo.Position, out var corners6))
				{
					Vector3[] array4 = LinkWays(new List<Vector3[]> { corners2, corners6 });
					path = array4;
					return EPathCalcResult.PathComplete;
				}
				Debug.LogError($"can't find way from cover to core cover. from simple pos:{struct16_.from} coverFrom.CorePointInGame.Position:{struct16_.coverFrom.CorePointInGame.Position} ");
			}
			else
			{
				Debug.LogError($"can't find way from position to core cover. from simple pos:{struct16_.from} coverFrom.CorePointInGame.Position:{struct16_.coverFrom.CorePointInGame.Position} ");
			}
			Debug.LogError($"can't find way from position to closest cover. from:{struct16_.from} cg:{connectionGroup}   coverFrom.Position:{struct16_.coverFrom.Position}  coverCG:{struct16_.coverFrom.ConnectionGroup}");
			smethod_3("0", ref struct16_);
			return EPathCalcResult.NeedTeleport;
		}
		if (FindPath(struct16_.coverFrom.Position, struct16_.coverTo.Position, out var corners7))
		{
			Vector3[] array5 = LinkWays(new List<Vector3[]> { corners, corners7 });
			path = array5;
			return EPathCalcResult.PathComplete;
		}
		if (!FindPath(struct16_.coverFrom.Position, struct16_.coverFrom.CorePointInGame.Position, out var corners8))
		{
			smethod_3("1", ref struct16_);
			path = corners8;
			return EPathCalcResult.NeedTeleport;
		}
		if (!smethod_1(struct16_.coverFrom.CorePointInGame, struct16_.coverTo.CorePointInGame, out var way2))
		{
			smethod_3($"2 coreError:[{struct16_.coverFrom.CorePointInGame.Id}({struct16_.coverFrom.CorePointInGame.ConnectionGroupId}) => {struct16_.coverTo.CorePointInGame.Id}({struct16_.coverTo.CorePointInGame.ConnectionGroupId})]   {Environment.StackTrace.ToString()}", ref struct16_);
			path = way2;
			return EPathCalcResult.NeedTeleport;
		}
		if (!FindPath(struct16_.coverTo.CorePointInGame.Position, struct16_.coverTo.Position, out var corners9))
		{
			smethod_3("3", ref struct16_);
			path = corners9;
			return EPathCalcResult.NeedTeleport;
		}
		Vector3[] array6 = LinkWays(new List<Vector3[]> { corners, corners8, way2, corners9 });
		path = array6;
		return EPathCalcResult.PathComplete;
	}

	public static EPathCalcResult CalculatePathFull(int connectionGroup, Vector3 from, Vector3 to, AICoversData covers, out Vector3[] path, bool onlyShortTrie = false, Action errorCallback = null)
	{
		if (FindPath(from, to, out path))
		{
			return EPathCalcResult.PathComplete;
		}
		if (onlyShortTrie)
		{
			return EPathCalcResult.NeedTeleport;
		}
		Vector3[] corners;
		GroupPoint closestPointWithWay = GetClosestPointWithWay(covers, to, connectionGroup, out corners);
		corners = corners?.Reverse().ToArray();
		IAICorePointLink iAICorePointLink = closestPointWithWay;
		if (corners == null && !FindPath(closestPointWithWay.Position, to, out corners))
		{
			if (!FindPath(closestPointWithWay.CorePointInGame.Position, to, out corners))
			{
				List<AICorePoint> corePoints = covers.AICorePointsHolder.CorePoints;
				bool flag = false;
				foreach (AICorePoint item in corePoints)
				{
					if (FindPath(item.Position, to, out corners))
					{
						iAICorePointLink = item;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return EPathCalcResult.NeedTeleport;
				}
			}
			else
			{
				iAICorePointLink = closestPointWithWay.CorePointInGame;
			}
		}
		if (FindPath(from, iAICorePointLink.Position, out var corners2))
		{
			Vector3[] array = LinkWays(new List<Vector3[]> { corners2, corners });
			path = array;
			return EPathCalcResult.PathComplete;
		}
		if (CalculatePathFull(connectionGroup, from, iAICorePointLink, covers, out var path2, errorCallback) == EPathCalcResult.PathComplete)
		{
			Vector3[] array2 = LinkWays(new List<Vector3[]> { path2, corners });
			path = array2;
			return EPathCalcResult.PathComplete;
		}
		return EPathCalcResult.NeedTeleport;
	}

	public static void FindPathOfCoresTest(Vector3 f, Vector3 t)
	{
		AICorePoint closest = AICorePointHolder.GetClosest(f);
		AICorePoint closest2 = AICorePointHolder.GetClosest(t);
		smethod_0(closest, closest2);
		smethod_0(closest2, closest);
	}

	public static void smethod_0(AICorePoint coreFrom, AICorePoint coreTo)
	{
		float duration = 20f;
		bool flag;
		Color color;
		if (flag = smethod_1(coreFrom, coreTo, out var way))
		{
			color = Color.cyan;
			GClass369.DebugDrawWay(way, Vector3.up, Color.cyan, duration);
		}
		else
		{
			color = Color.red;
			Debug.LogError($"Can't draw way between:{coreFrom.Id} and {coreTo.Id}");
		}
		Debug.DrawRay(coreFrom.Position, Vector3.up * 60f, color, duration);
		Debug.DrawRay(coreTo.Position, Vector3.up * 100f, color, duration);
		Debug.LogError($"FindPathOfCoresTest  {flag}  coreFrom:{coreFrom.Id}   coreTo:{coreTo.Id}");
	}

	public static GroupPoint GetClosestPoint(AICoversData covers, Vector3 from, int connectionGroup)
	{
		NavGraphVoxelSimple voxelSafe = covers.GetVoxelSafe(from);
		if (voxelSafe != null && voxelSafe.HaveStartPointId)
		{
			if (voxelSafe.PointStartSearch.ConnectionGroup == connectionGroup)
			{
				return voxelSafe.PointStartSearch;
			}
			foreach (GroupPoint point in voxelSafe.Points)
			{
				if (point.ConnectionGroup == connectionGroup && FindPath(from, point.Position, out var _))
				{
					return point;
				}
			}
		}
		Vector3Int indexes = covers.GetIndexes(from);
		for (int i = 1; i < 10; i++)
		{
			foreach (NavGraphVoxelSimple item in covers.GetVoxelesExtended(indexes.x, indexes.y, indexes.z, i, onlyWithPoints: false))
			{
				if (item == null || !item.HaveStartPointId)
				{
					continue;
				}
				foreach (GroupPoint point2 in item.Points)
				{
					if (point2.ConnectionGroup == connectionGroup && FindPath(from, point2.Position, out var _))
					{
						return point2;
					}
				}
			}
		}
		return covers.GetClosest(from, connectionGroup);
	}

	public static GroupPoint GetClosestPointWithWay(AICoversData covers, Vector3 from, int connectionGroup, out Vector3[] corners)
	{
		NavGraphVoxelSimple voxelSafe = covers.GetVoxelSafe(from);
		int num = 0;
		GroupPoint groupPoint = null;
		int num2 = 10;
		if (voxelSafe != null && voxelSafe.HaveStartPointId)
		{
			if (voxelSafe.PointStartSearch.ConnectionGroup == connectionGroup)
			{
				bool num3 = FindPath(from, voxelSafe.PointStartSearch.Position, out corners);
				if (groupPoint == null)
				{
					groupPoint = voxelSafe.PointStartSearch;
				}
				num++;
				if (num3)
				{
					return voxelSafe.PointStartSearch;
				}
			}
			foreach (GroupPoint point in voxelSafe.Points)
			{
				if (point.ConnectionGroup == connectionGroup)
				{
					bool flag = FindPath(from, point.Position, out corners);
					if (groupPoint == null)
					{
						groupPoint = point;
					}
					if (num > num2)
					{
						corners = null;
						return groupPoint;
					}
					num++;
					if (flag)
					{
						return point;
					}
				}
			}
		}
		Vector3Int indexes = covers.GetIndexes(from);
		for (int i = 1; i < 5; i++)
		{
			foreach (NavGraphVoxelSimple item in covers.GetVoxelesExtended(indexes.x, indexes.y, indexes.z, i, onlyWithPoints: false))
			{
				if (item == null || !item.HaveStartPointId)
				{
					continue;
				}
				foreach (GroupPoint point2 in item.Points)
				{
					if (point2.ConnectionGroup == connectionGroup)
					{
						bool flag2 = FindPath(from, point2.Position, out corners);
						if (groupPoint == null)
						{
							groupPoint = point2;
						}
						if (num > num2)
						{
							corners = null;
							return groupPoint;
						}
						num++;
						if (flag2)
						{
							return point2;
						}
					}
				}
			}
		}
		GroupPoint closest = covers.GetClosest(from, connectionGroup);
		corners = null;
		return closest;
	}

	public static bool smethod_1(AICorePoint from, AICorePoint to, out Vector3[] way)
	{
		List<Vector3[]> list = new List<Vector3[]>();
		if (from.Id == to.Id)
		{
			way = new Vector3[0];
			return true;
		}
		if (smethod_2(from, to, out var way2))
		{
			for (int i = 0; i < way2.Length - 1; i++)
			{
				AICorePoint obj = way2[i];
				if (FindPath(t: way2[i + 1].Position, f: obj.Position, corners: out var corners))
				{
					list.Add(corners);
					continue;
				}
				way = null;
				return false;
			}
		}
		if (list.Count > 0)
		{
			way = LinkWays(list);
			return true;
		}
		way = null;
		return false;
	}

	public static bool smethod_2(AICorePoint from, AICorePoint to, out AICorePoint[] way)
	{
		Struct15 struct15_ = default(Struct15);
		struct15_.to = to;
		if (from.ConnectionGroupId != struct15_.to.ConnectionGroupId)
		{
			way = null;
			return false;
		}
		if (from.Id == struct15_.to.Id)
		{
			way = new AICorePoint[1] { from };
			return true;
		}
		struct15_.hash = new HashSet<AICorePoint>();
		struct15_.hash.Add(from);
		List<AICorePoint> list = new List<AICorePoint>();
		if (smethod_4(from, list, 0, ref struct15_))
		{
			list.Add(from);
			list.Reverse();
			way = list.ToArray();
			return true;
		}
		Debug.LogError($"Can't draw cause can't find way at graph.  from:{from.Id}  to:{struct15_.to.Id}");
		way = null;
		return false;
	}

	public static Vector3[] LinkWays(List<Vector3[]> ways)
	{
		if (ways.Count == 0)
		{
			return null;
		}
		List<Vector3> list = ways[0].ToList();
		for (int i = 1; i < ways.Count; i++)
		{
			Vector3[] array = ways[i];
			if (array != null && array.Length != 0)
			{
				list.AddRange(array);
			}
		}
		return list.ToArray();
	}

	public static bool FindPath(Vector3 f, Vector3 t, out Vector3[] corners)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		if (NavMesh.CalculatePath(f, t, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
		{
			corners = navMeshPath.corners;
			return true;
		}
		corners = null;
		return false;
	}

	public BotPathFinderClass(BotOwner owner, BotMover botMover, PathControllerClass pathController, AICoversData covers)
		: base(owner)
	{
		BotMover_0 = botMover;
		PathControllerClass = pathController;
		AicoversData_0 = covers;
	}

	public bool method_0(Vector3 pos, bool slowAtTheEnd, bool getUpWithCheck)
	{
		if (BotOwner_0.BotLay.IsLay)
		{
			Vector3 vector = pos - BotOwner_0.Mover.PositionOnWay;
			if (vector.y < 0.5f)
			{
				vector.y = 0f;
			}
			if (vector.sqrMagnitude > 0.2f)
			{
				BotOwner_0.BotLay.GetUp(getUpWithCheck);
			}
			if (BotOwner_0.BotLay.IsLay)
			{
				return false;
			}
		}
		BotOwner_0.WeaponManager.Stationary.StartMove();
		SlowAtTheEnd = slowAtTheEnd;
		return true;
	}

	public NavMeshPathStatus GoToPosition(IAICorePointLink target, bool slowAtTheEnd, float reachDist, bool getUpWithCheck)
	{
		SlowAtTheEnd = slowAtTheEnd;
		if (PathControllerClass.IsSameWay(target, BotOwner_0.Mover.PositionOnWay))
		{
			return NavMeshPathStatus.PathComplete;
		}
		if (!method_0(target.Position, slowAtTheEnd, getUpWithCheck))
		{
			return NavMeshPathStatus.PathInvalid;
		}
		Vector3 vector;
		if (GClass855.IsOnNavMesh(BotOwner_0.Position, 0.3f))
		{
			StartsFromDebug = 1;
			vector = BotOwner_0.Position;
		}
		else if ((BotOwner_0.Mover.PositionOnWay - BotOwner_0.Position).sqrMagnitude > 1f)
		{
			StartsFromDebug = 2;
			vector = BotOwner_0.Mover.PositionOnWay;
		}
		else
		{
			StartsFromDebug = 3;
			vector = BotOwner_0.Position;
		}
		StartsDiffDebug = (BotOwner_0.Mover.PositionOnWay - BotOwner_0.Position).magnitude;
		if (CalculatePathFull(BotOwner_0.StartCorePoint.ConnectionGroupId, vector, target, AicoversData_0, out var path) == EPathCalcResult.PathComplete)
		{
			PathControllerClass.GoToByWay(path, reachDist);
			return NavMeshPathStatus.PathComplete;
		}
		return NavMeshPathStatus.PathInvalid;
	}

	public NavMeshPathStatus GoToPosition(Vector3 target, bool slowAtTheEnd, float reachDist, bool getUpWithCheck, bool mustHaveWay, bool onlyShortTrie = false, bool force = false, bool slowCalcUsingNativeNavMesh = false)
	{
		SlowAtTheEnd = slowAtTheEnd;
		if (!force && PathControllerClass.IsSameWay(target, BotOwner_0.Mover.PositionOnWay))
		{
			return NavMeshPathStatus.PathComplete;
		}
		if (slowCalcUsingNativeNavMesh)
		{
			NavMeshPath navMeshPath = new NavMeshPath();
			if (NavMesh.CalculatePath(BotOwner_0.Position, target, -1, navMeshPath) && navMeshPath.status != NavMeshPathStatus.PathInvalid)
			{
				PathControllerClass.GoToByWay(navMeshPath.corners, reachDist);
				return navMeshPath.status;
			}
		}
		if (!method_0(target, slowAtTheEnd, getUpWithCheck))
		{
			return NavMeshPathStatus.PathInvalid;
		}
		if (CalculatePathFull(BotOwner_0.StartCorePoint.ConnectionGroupId, BotOwner_0.Position, target, AicoversData_0, out var path, onlyShortTrie, method_1) == EPathCalcResult.PathComplete)
		{
			Int_2 = 0;
			PathControllerClass.GoToByWay(path, reachDist);
			return NavMeshPathStatus.PathComplete;
		}
		if (mustHaveWay)
		{
			GroupPoint closestPoint = GetClosestPoint(AicoversData_0, BotOwner_0.Position, BotOwner_0.StartCorePoint.ConnectionGroupId);
			BotOwner_0.GetPlayer.Teleport(closestPoint.Position);
		}
		return NavMeshPathStatus.PathInvalid;
	}

	public void method_1()
	{
		_ = $"Bot path find error. {BotOwner_0.Id} PrevSuccessLinkedFrom:{BotOwner_0.Mover.PrevSuccessLinkedFrom}  PrevPosLinkedTime:{Time.time - BotOwner_0.Mover.PrevPosLinkedTime} PrevCorner:{BotOwner_0.Mover.PrevCorner()}";
		Int_2++;
		if (Int_2 > 5)
		{
			Int_2 = 0;
			CoverSearchData data = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.hide, 100f, 0f, CoverSearchType.distToBot, null, null, null, ECheckSHootHide.hide, new CoverSearchDefenceDataClass(0f), PointsArrayType.allWithBush);
			CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
			BotOwner_0.Mover.Teleport(coverPointMain.Position);
		}
	}

	public void method_2()
	{
	}

	public NavMeshPathStatus method_3(Vector3 pos, bool mustHaveWay)
	{
		if (mustHaveWay)
		{
			Int_3++;
			if (Int_3 <= 10)
			{
				float dist = 0.2f;
				if (!GClass855.IsOnNavMesh(BotOwner_0.Position, dist) || GClass855.IsOnNavMesh(pos, dist))
				{
					Int_3++;
					if (Int_4 > 0)
					{
						Int_4--;
						Gclass546_0.AddLog(string.Format("Bot NotOnNavMesh id:{0} role:{2}   can't find way from position:{1} to:{3}", BotOwner_0.Id, BotOwner_0.Position, BotOwner_0.Profile.Info.Settings.Role.ToString(), pos));
						if (GClass398.Instance.IsTraceEnable())
						{
							method_2();
						}
					}
				}
				PathControllerClass.ReturnToNavExtra();
				return NavMeshPathStatus.PathComplete;
			}
			Int_3 = 0;
			BotMover_1.Stop();
			CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position);
			if (closestPoint != null)
			{
				Gclass546_0.AddLog($"Bot id:{BotOwner_0.Id} _failNavmeshTimes:{Int_3}   TELEPORTED TO {closestPoint.Position}.   ");
				Gclass546_0.ClearAndPrint();
				BotOwner_0.Mover.Teleport(closestPoint.Position);
			}
			else
			{
				Gclass546_0.AddLog($"Bot id:{BotOwner_0.Id} _failNavmeshTimes:{Int_3}   and can't find closest cover. All BAD!! ");
			}
		}
		return NavMeshPathStatus.PathInvalid;
	}

	[CompilerGenerated]
	public static void smethod_3(string sub, ref Struct16 struct16_0)
	{
		if (Time.time - Float_0 > 5f)
		{
			Float_0 = Time.time;
			struct16_0.errorCallback?.Invoke();
			_ = $"Stage {sub} Can't find path from:{struct16_0.from} (id:{struct16_0.coverFrom.Id},{struct16_0.coverFrom.Position},{struct16_0.coverFrom.CorePointId},{struct16_0.coverFrom.CorePointInGame.ConnectionGroupId} ) ----> TO:{struct16_0.coverTo.Position}  CorePointInGame:({struct16_0.coverTo.CorePointInGame.Id},{struct16_0.coverTo.CorePointInGame.ConnectionGroupId} )";
		}
	}

	[CompilerGenerated]
	public static bool smethod_4(AICorePoint p, List<AICorePoint> wayResult, int deep, ref Struct15 struct15_0)
	{
		if (deep > 20)
		{
			return false;
		}
		foreach (AICorePoint item in p.ConnectionsAtNet)
		{
			if (!struct15_0.hash.Contains(item))
			{
				struct15_0.hash.Add(item);
				if (item.Id == struct15_0.to.Id)
				{
					wayResult.Add(item);
					return true;
				}
				if (smethod_4(item, wayResult, deep + 1, ref struct15_0))
				{
					wayResult.Add(item);
					return true;
				}
			}
		}
		return false;
	}
}

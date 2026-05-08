using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class PathControllerClass : GClass429
{
	[CompilerGenerated]
	public class Class249
	{
		public Vector3 centerAvoid;

		public List<Vector3> subAvoids;

		public float method_0(List<Vector3> path)
		{
			float num = float.MaxValue;
			foreach (Vector3 item in path)
			{
				float sqrMagnitude = (centerAvoid - item).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
				}
			}
			return num;
		}

		public float method_1(List<Vector3> path)
		{
			float num = float.MaxValue;
			foreach (Vector3 item in subAvoids)
			{
				foreach (Vector3 item2 in path)
				{
					float sqrMagnitude = (item - item2).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
					}
				}
			}
			return num;
		}
	}

	public Action<int, Vector3[]> onPathCornerChanged;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public string String_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	public GClass551 Gclass551_0 = new GClass551();

	[NonSerialized]
	public BotCurrentPathAbstractClass BotCurrentPathAbstractClass;

	[NonSerialized]
	public BotCurrentPathAbstractClass BotCurrentPathAbstractClass_1;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public Vector3 Vector3_2;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	[CompilerGenerated]
	private Action<bool> action_0;

	public Vector3 StopPos;

	[NonSerialized]
	public const float Float_3 = 25f;

	[NonSerialized]
	public float Float_4 = 5f;

	[NonSerialized]
	public static NavMeshPath NavMeshPath_0 = new NavMeshPath();

	[NonSerialized]
	public static NavMeshPath NavMeshPath_1 = new NavMeshPath();

	[NonSerialized]
	public static NavMeshPath NavMeshPath_2 = new NavMeshPath();

	[NonSerialized]
	public static NavMeshPath NavMeshPath_3 = new NavMeshPath();

	[NonSerialized]
	public static NavMeshPath NavMeshPath_4 = new NavMeshPath();

	[NonSerialized]
	public float Float_5 = 1.44f;

	public string CallstackPathComeFrom
	{
		[CompilerGenerated]
		get
		{
			return String_1;
		}
		[CompilerGenerated]
		set
		{
			String_1 = value;
		}
	}

	public float PlayerRemainingDist
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
		[CompilerGenerated]
		set
		{
			Float_0 = value;
		}
	}

	public float LastPathSetTime
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
		[CompilerGenerated]
		set
		{
			Float_1 = value;
		}
	}

	public float LastEndPathTime
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
		[CompilerGenerated]
		set
		{
			Float_2 = value;
		}
	}

	public Vector3 PrevCorner => Vector3_1;

	public bool HavePath => CurPath != null;

	public Vector3? TargetPoint
	{
		get
		{
			if (CurPath == null)
			{
				return null;
			}
			return CurPath.TargetPoint.Position;
		}
	}

	public BotCurrentPathAbstractClass CurPath
	{
		get
		{
			return BotCurrentPathAbstractClass_1;
		}
		set
		{
			if (BotCurrentPathAbstractClass_1 != null)
			{
				BotCurrentPathAbstractClass = BotCurrentPathAbstractClass_1;
			}
			BotCurrentPathAbstractClass_1 = value;
			if (BotCurrentPathAbstractClass_1 != null)
			{
				BotCurrentPathAbstractClass_1.onPathCornerChanged = onPathCornerChanged;
				BotCurrentPathAbstractClass_1.CurPathChanged();
			}
			if (value != null)
			{
				value.onPathCornerChanged = null;
			}
			if (BotCurrentPathAbstractClass_1 != null)
			{
				PlayerRemainingDist = 5f;
				Vector3_0 = BotCurrentPathAbstractClass_1.TargetPoint.Position;
			}
			else
			{
				StopPos = BotOwner_0.Position;
			}
		}
	}

	public Vector3 CurrentCornerPoint => Vector3_2;

	public string PathStack => String_0;

	public event Action<bool> OnWayChanged
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static Vector3 smethod_0(BotOwner bot, Vector3 p, Vector3 safePoint)
	{
		int num = 0;
		while (true)
		{
			if (num < bot.BotsGroup.MembersCount)
			{
				BotOwner botOwner = bot.BotsGroup.Member(num);
				if (botOwner != bot && !((botOwner.PatrollingData.CurTargetPoint - p).sqrMagnitude >= 1f))
				{
					break;
				}
				num++;
				continue;
			}
			return p;
		}
		return safePoint;
	}

	public PathControllerClass(BotOwner owner)
		: base(owner)
	{
	}

	public void SetPath(Vector3[] pathCorners)
	{
		GClass550 combineBotPath = new GClass550(pathCorners, method_1);
		method_0(combineBotPath);
	}

	public void IncCornerIndex()
	{
		Vector3_1 = Vector3_2;
		Vector3_2 = CurPath.CurrentCorner();
		CurPath.IncCornerIndex();
	}

	public void OnEndInteract()
	{
		if (HavePath)
		{
			BotOwner_0.GoToPoint(BotOwner_0.Mover.LastDestination(), slowAtTheEnd: true, -1f, getUpWithCheck: false, mustHaveWay: true, mustGetUp: true, onlyShortTrie: false, force: true);
		}
	}

	public void GoToPointNoWay(Vector3? trg)
	{
		if (trg.HasValue)
		{
			Vector3[] path = new Vector3[1] { trg.Value };
			SetPath(path);
		}
	}

	public void GoToByWay(Vector3[] way, float reachDist)
	{
		GClass550 gClass = new GClass550(way, method_1);
		gClass.SetReachDist(reachDist);
		method_0(gClass);
	}

	public void Stop()
	{
		if (CurPath != null)
		{
			BotCurrentPathAbstractClass_1.CheckIsFinished(BotOwner_0);
			if (BotCurrentPathAbstractClass_1 != null)
			{
				BotCurrentPathAbstractClass = BotCurrentPathAbstractClass_1;
			}
			method_0(null);
		}
	}

	public void DrawSelectedGizmosWay()
	{
		CurPath?.DrawSelectedGizmosWay(0.5f, Color.blue);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(BotOwner_0.Position + Vector3.up, Vector3_2);
	}

	public void DrawSelectedGizmosLastWay()
	{
		BotCurrentPathAbstractClass?.DrawSelectedGizmosWay(0.5f, Color.cyan);
	}

	public Vector3 CurrentCorner()
	{
		return CurPath.CurrentCorner();
	}

	public bool IsLast()
	{
		if (BotCurrentPathAbstractClass_1 != null)
		{
			return BotCurrentPathAbstractClass_1.IsLast();
		}
		return false;
	}

	public Vector3 LastTargetPoint(float distCoef)
	{
		BotCurrentPathAbstractClass curPath = CurPath;
		int value = curPath.Length - 1;
		float num = 0f;
		float[] array = new float[curPath.Length];
		for (int i = 0; i < curPath.Length - 1; i++)
		{
			float sqrMagnitude = (curPath.GetPoint(i) - curPath.GetPoint(i + 1)).sqrMagnitude;
			num = (array[i] = num + sqrMagnitude);
		}
		float num2 = num * distCoef;
		for (int j = 0; j < array.Length; j++)
		{
			if (!(array[j] <= num2))
			{
				value = j;
				break;
			}
		}
		value = Mathf.Clamp(value, 1, curPath.Length + 1);
		Vector3 point = curPath.GetPoint(value);
		return smethod_0(BotOwner_0, point, curPath.GetPoint(curPath.Length - 1));
	}

	public bool CheckShouldMove()
	{
		Vector3 position = BotOwner_0.Position;
		Vector3 vector = CurPath.TargetPoint.Position - position;
		PlayerRemainingDist = vector.magnitude;
		vector.y = ((Mathf.Abs(vector.y) > BotOwner_0.Settings.FileSettings.Move.Y_APPROXIMATION) ? vector.y : 0f);
		bool num = PlayerRemainingDist > CurPath.ReachDist;
		if (!num)
		{
			Stop();
		}
		return num;
	}

	public bool GoToSamePoint(Vector3 trg)
	{
		if (CurPath == null)
		{
			return false;
		}
		return (CurPath.TargetPoint.Position - trg).sqrMagnitude < 0.1f;
	}

	public Vector3 LastDestination()
	{
		return Vector3_0;
	}

	public void DebugDrawLastWay()
	{
		Gclass551_0.DrawLastWay();
	}

	public void ReturnToNavExtra()
	{
	}

	public bool IsSameWay(Vector3 target, Vector3 ownerPosition)
	{
		if (BotCurrentPathAbstractClass_1 != null && BotCurrentPathAbstractClass_1.Length > 0)
		{
			return BotCurrentPathAbstractClass_1.IsSame(target, ownerPosition);
		}
		return false;
	}

	public bool IsSameWay(IAICorePointLink target, Vector3 ownerPosition)
	{
		if (BotCurrentPathAbstractClass_1 != null && BotCurrentPathAbstractClass_1.Length > 0)
		{
			return BotCurrentPathAbstractClass_1.IsSame(target, ownerPosition);
		}
		return false;
	}

	public void method_0(GClass550 combineBotPath)
	{
		bool obj = CurPath != null;
		if (CurPath != null && combineBotPath == null)
		{
			LastEndPathTime = Time.time;
		}
		CurPath = combineBotPath;
		if (CurPath != null)
		{
			PlayerRemainingDist = (CurPath.TargetPoint.Position - BotOwner_0.Position).magnitude;
			LastPathSetTime = Time.time;
		}
		else
		{
			PlayerRemainingDist = 0f;
		}
		Gclass551_0.AddWay(combineBotPath, BotOwner_0.Position);
		action_0?.Invoke(obj);
	}

	public void method_1(GInterface9 obj)
	{
		Stop();
		obj.BotCome(BotOwner_0);
	}

	public bool method_2(Vector3[] pathToCheck)
	{
		int num = 0;
		while (true)
		{
			if (num < pathToCheck.Length - 1)
			{
				Vector3 vector = pathToCheck[num];
				Vector3 vector2 = pathToCheck[num + 1] - vector;
				if (vector2.x == 0f && vector2.z == 0f && !(Mathf.Abs(vector2.y) <= 0f))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		string text = "";
		for (int i = 0; i < pathToCheck.Length; i++)
		{
			Vector3 vector3 = pathToCheck[i];
			text = text + vector3.ToString() + "  ";
		}
		Debug.LogError($"new way fail !!! {BotOwner_0.Id}   path:{text}    curPos:{BotOwner_0.Position}");
		return true;
	}

	public Vector3[] GetWayPoints(int maxWayPoints)
	{
		if (CurPath == null)
		{
			return new Vector3[0];
		}
		int curIndex = CurPath.CurIndex;
		int num = Mathf.Min(CurPath.Length - curIndex, maxWayPoints);
		Vector3[] array = new Vector3[num];
		int num2 = 0;
		for (int i = curIndex; i < num + curIndex; i++)
		{
			Vector3 point = CurPath.GetPoint(i);
			array[num2] = point;
			num2++;
		}
		return array;
	}

	public bool TryReplacePathAround(Vector3 centerAvoid, [CanBeNull] List<Vector3> subAvoids, out BotCurrentPathAbstractClass curPath)
	{
		Vector3 vector = BotOwner_0.Position - centerAvoid;
		if (BotCurrentPathAbstractClass_1 == null)
		{
			curPath = BotCurrentPathAbstractClass_1;
			return false;
		}
		float duration = 5f;
		int curIndex = BotCurrentPathAbstractClass_1.CurIndex;
		int num = 0;
		Func<List<Vector3>, float> avoidFund = ((subAvoids == null || subAvoids.Count <= 1) ? ((Func<List<Vector3>, float>)delegate(List<Vector3> path)
		{
			float num3 = float.MaxValue;
			foreach (Vector3 item in path)
			{
				float sqrMagnitude2 = (centerAvoid - item).sqrMagnitude;
				if (sqrMagnitude2 < num3)
				{
					num3 = sqrMagnitude2;
				}
			}
			return num3;
		}) : ((Func<List<Vector3>, float>)delegate(List<Vector3> path)
		{
			float num3 = float.MaxValue;
			foreach (Vector3 subAvoid in subAvoids)
			{
				foreach (Vector3 item2 in path)
				{
					float sqrMagnitude2 = (subAvoid - item2).sqrMagnitude;
					if (sqrMagnitude2 < num3)
					{
						num3 = sqrMagnitude2;
					}
				}
			}
			return num3;
		}));
		int num2 = curIndex;
		Vector3 point;
		while (true)
		{
			if (num2 < BotCurrentPathAbstractClass_1.Length)
			{
				point = BotCurrentPathAbstractClass_1.GetPoint(num2);
				Vector3 rhs = point - centerAvoid;
				num++;
				float sqrMagnitude = rhs.sqrMagnitude;
				GClass810.DebugPoint(point + Vector3.up * 0.5f, 0.3f, duration, depthTest: true, Color.white);
				if (sqrMagnitude > 25f && !(Vector3.Dot(vector, rhs) >= 0f))
				{
					break;
				}
				num2++;
				continue;
			}
			curPath = BotCurrentPathAbstractClass_1;
			return false;
		}
		Vector3 n = GClass855.NormalizeFastSelf(vector) * Float_4;
		Vector3 vector2 = point;
		Vector3 vector3 = GClass855.Rotate90(n, GClass855.SideTurn.left) + centerAvoid;
		Vector3 vector4 = GClass855.Rotate90(n, GClass855.SideTurn.right) + centerAvoid;
		NavMesh.CalculatePath(BotOwner_0.Position, vector3, -1, NavMeshPath_0);
		NavMesh.CalculatePath(BotOwner_0.Position, vector4, -1, NavMeshPath_1);
		GClass369.DebugDrawArc(BotOwner_0.Position, vector3, 2f, duration, Color.green);
		GClass369.DebugDrawArc(vector2, vector3, 2f, duration, Color.green);
		GClass369.DebugDrawArc(BotOwner_0.Position, vector4, 2f, duration, Color.cyan);
		GClass369.DebugDrawArc(vector2, vector4, 2f, duration, Color.cyan);
		List<Vector3> list = method_3(NavMeshPath_0, NavMeshPath_1, vector2, avoidFund);
		if (list == null)
		{
			curPath = BotCurrentPathAbstractClass_1;
			return false;
		}
		List<Vector3> list2 = TryRuildbuildWay(BotCurrentPathAbstractClass_1, num2, list);
		if (list2 != null)
		{
			BotOwner_0.Mover.GoToByWay(list2.ToArray(), 0.4f);
			curPath = BotCurrentPathAbstractClass_1;
			return true;
		}
		curPath = BotCurrentPathAbstractClass_1;
		return false;
	}

	public List<Vector3> method_3(NavMeshPath p1, NavMeshPath p2, Vector3 fin, Func<List<Vector3>, float> avoidFund)
	{
		if (p1.status == NavMeshPathStatus.PathComplete && p1.status != NavMeshPathStatus.PathComplete)
		{
			NavMesh.CalculatePath(p1.corners[p1.corners.Length - 1], fin, -1, NavMeshPath_3);
			if (NavMeshPath_3.status == NavMeshPathStatus.PathComplete)
			{
				return method_4(p1.corners, NavMeshPath_3.corners);
			}
			return null;
		}
		if (p2.status == NavMeshPathStatus.PathComplete && p2.status != NavMeshPathStatus.PathComplete)
		{
			NavMesh.CalculatePath(p2.corners[p2.corners.Length - 1], fin, -1, NavMeshPath_4);
			if (NavMeshPath_4.status == NavMeshPathStatus.PathComplete)
			{
				return method_4(p2.corners, NavMeshPath_4.corners);
			}
			return null;
		}
		if (p1.status == NavMeshPathStatus.PathComplete && p2.status == NavMeshPathStatus.PathComplete)
		{
			NavMesh.CalculatePath(p1.corners[p1.corners.Length - 1], fin, -1, NavMeshPath_3);
			NavMesh.CalculatePath(p2.corners[p2.corners.Length - 1], fin, -1, NavMeshPath_4);
			if (NavMeshPath_3.status == NavMeshPathStatus.PathComplete && NavMeshPath_4.status != NavMeshPathStatus.PathComplete)
			{
				return method_4(p1.corners, NavMeshPath_3.corners);
			}
			if (NavMeshPath_4.status == NavMeshPathStatus.PathComplete && NavMeshPath_3.status != NavMeshPathStatus.PathComplete)
			{
				return method_4(p2.corners, NavMeshPath_4.corners);
			}
			float num = GClass371.CalculatePathLength(p1);
			float num2 = GClass371.CalculatePathLength(p2);
			float num3 = GClass371.CalculatePathLength(NavMeshPath_3);
			float num4 = GClass371.CalculatePathLength(NavMeshPath_4);
			GClass369.DebugDrawWay(p1.corners, Vector3.up, Color.green);
			GClass369.DebugDrawWay(NavMeshPath_3.corners, Vector3.up, Color.green);
			GClass369.DebugDrawWay(p2.corners, Vector3.up, Color.cyan);
			GClass369.DebugDrawWay(NavMeshPath_4.corners, Vector3.up, Color.cyan);
			float num5 = num + num3;
			float num6 = num2 + num4;
			if (num5 < num6)
			{
				List<Vector3> list = method_4(p1.corners, NavMeshPath_3.corners);
				float num7 = avoidFund(list);
				if (num7 < Float_5)
				{
					List<Vector3> list2 = method_4(p2.corners, NavMeshPath_4.corners);
					float num8 = avoidFund(list2);
					if (num8 > num7)
					{
						if (num8 > Float_5)
						{
							return null;
						}
						return list2;
					}
				}
				return list;
			}
			List<Vector3> list3 = method_4(p2.corners, NavMeshPath_4.corners);
			float num9 = avoidFund(list3);
			if (num9 < Float_5)
			{
				List<Vector3> list4 = method_4(p1.corners, NavMeshPath_3.corners);
				float num10 = avoidFund(list4);
				if (num9 < num10)
				{
					if (num10 > Float_5)
					{
						return null;
					}
					return list4;
				}
			}
			return list3;
		}
		return null;
	}

	public List<Vector3> method_4(Vector3[] p1, Vector3[] p2)
	{
		List<Vector3> list = new List<Vector3>();
		Vector3[] array = p1;
		foreach (Vector3 item in array)
		{
			list.Add(item);
		}
		array = p2;
		foreach (Vector3 item2 in array)
		{
			list.Add(item2);
		}
		return list;
	}

	public static List<Vector3> TryRuildbuildWay(BotCurrentPathAbstractClass curPath, int curPathCurIndex, List<Vector3> firstPath)
	{
		for (int i = curPathCurIndex; i < curPath.Length; i++)
		{
			Vector3 point = curPath.GetPoint(i);
			firstPath.Add(point);
		}
		return firstPath;
	}

	public void SetReachDist(float reachDist)
	{
		if (BotCurrentPathAbstractClass_1 != null)
		{
			BotCurrentPathAbstractClass_1.SetReachDist(reachDist);
		}
	}

	public bool IsPointOnCurrentWay(Vector3 point, float maxDist)
	{
		if (BotCurrentPathAbstractClass_1 == null)
		{
			return false;
		}
		return BotCurrentPathAbstractClass_1.IsPointOnWay(point, maxDist);
	}

	public Vector3? FindPointFarThan(float dist, out int index)
	{
		index = 0;
		if (BotCurrentPathAbstractClass_1 == null)
		{
			return null;
		}
		Vector3 position = BotOwner_0.Position;
		float num = 0f;
		int num2 = BotCurrentPathAbstractClass_1.CurIndex;
		Vector3 point;
		while (true)
		{
			if (num2 < BotCurrentPathAbstractClass_1.Length)
			{
				point = BotCurrentPathAbstractClass_1.GetPoint(num2);
				float magnitude = (position - point).magnitude;
				num += magnitude;
				if (!(num <= dist))
				{
					break;
				}
				num2++;
				continue;
			}
			return null;
		}
		index = num2;
		return point;
	}

	public List<Vector3> GetCornersFromIndex(int index)
	{
		if (BotCurrentPathAbstractClass_1 == null)
		{
			return null;
		}
		return BotCurrentPathAbstractClass_1.GetCornersFromIndex(index);
	}
}

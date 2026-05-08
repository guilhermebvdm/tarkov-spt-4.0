using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class AIManualPointsHolder : MonoBehaviour
{
	[CompilerGenerated]
	public class Class145
	{
		public GroupPoint point;

		public bool method_0(GroupPoint x)
		{
			return x.CorePointId == point.CorePointId;
		}
	}

	[SerializeField]
	public List<GroupPoint> ManualPoints = new List<GroupPoint>();

	public bool DrawWithEdges;

	public bool DrawWithCore;

	public bool AlwaysDrawGizmos;

	public bool AddPoint(GroupPoint point)
	{
		AICoversData aICoversData = AICoversData.CreateOrFind();
		if (!aICoversData.RestoreData())
		{
			Debug.LogError("Error restore data");
			return false;
		}
		if (!TryConnectPoint(point, aICoversData))
		{
			Debug.LogError("Can't connect point to net");
			return false;
		}
		ManualPoints.Add(point);
		AICalculationsInfo.UnSuccess(EAICalcSystem.SaveManualCovers);
		AICalculationsInfo.UnSuccess(EAICalcSystem.FindClosestPointsToVoxeles);
		return true;
	}

	public void OnDrawGizmos()
	{
		if (!AlwaysDrawGizmos)
		{
			return;
		}
		foreach (GroupPoint manualPoint in ManualPoints)
		{
			manualPoint.DrawGizmos(DrawWithEdges, DrawWithCore, 3f);
		}
	}

	public void RemovePoint(GroupPoint point)
	{
		AICoversData aICoversData = AICoversData.CreateOrFind();
		aICoversData.RestoreData();
		if (ManualPoints.Remove(point))
		{
			aICoversData.RemovePoint(point);
			AICalculationsInfo.UnSuccess(EAICalcSystem.SaveManualCovers);
			AICalculationsInfo.UnSuccess(EAICalcSystem.FindClosestPointsToVoxeles);
		}
	}

	public static bool TryConnectPoint(GroupPoint point, AICoversData data)
	{
		List<GroupPoint> toChoose = data.Points.Where((GroupPoint x) => x.CorePointId == point.CorePointId).ToList();
		List<GroupPoint> closests = AICoversData.GetClosests(point, 4, toChoose);
		bool result = false;
		foreach (GroupPoint item in closests)
		{
			if (TryConnect(point, item, data))
			{
				result = true;
			}
		}
		return result;
	}

	public static bool TryConnect(GroupPoint p1, GroupPoint p2, AICoversData covers)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		if (NavMesh.CalculatePath(p1.Position, p2.Position, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
		{
			float dist = GClass371.CalculatePathLength(navMeshPath);
			GroupPointWay groupPointWay = new GroupPointWay(covers.GetNextPathId(), p1, dist);
			GroupPointWay groupPointWay2 = new GroupPointWay(covers.GetNextPathId(), p2, dist);
			GroupPointPath path = new GroupPointPath(dist, covers.GetNextPathId(), groupPointWay, groupPointWay2);
			covers.AddPath(path);
			p1.AddConnectedGroupPoint(groupPointWay2);
			p2.AddConnectedGroupPoint(groupPointWay);
			return true;
		}
		return false;
	}

	public int GetMaxIdAndRecalcIds()
	{
		if (ManualPoints.Count == 0)
		{
			return 1;
		}
		int num = 1;
		foreach (GroupPoint manualPoint in ManualPoints)
		{
			manualPoint.Id = num;
			num++;
		}
		return num + 1;
	}
}

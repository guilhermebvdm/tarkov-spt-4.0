using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DebugCoverFindGraphSearchData : Interface0
{
	public float TimeSetted;

	[NonSerialized]
	public CoverSearchData Data;

	[NonSerialized]
	public int LastStep;

	[NonSerialized]
	public NavGraphVoxelSimple StartVoxel;

	[NonSerialized]
	public List<GroupPointSearchData> Checked_1 = new List<GroupPointSearchData>();

	[NonSerialized]
	public List<List<GroupPointSearchData>> PointsAtSteps = new List<List<GroupPointSearchData>>();

	[NonSerialized]
	public bool DoLog;

	public List<GroupPointSearchData> Checked => Checked_1;

	public Vector3 PositionStart => Data.CenterPos;

	public DebugCoverFindGraphSearchData(CoverSearchData data, NavGraphVoxelSimple voxel)
	{
		TimeSetted = Time.time;
		bool doLog = GClass396.Instance.IsTraceEnable();
		StartVoxel = voxel;
		Data = data;
		DoLog = doLog;
	}

	public void AddStep(GroupPointSearchData point, GClass368<GroupPointSearchData> list)
	{
		if (!DoLog)
		{
			return;
		}
		List<GroupPointSearchData> list2 = new List<GroupPointSearchData>();
		foreach (GroupPointSearchData item in list.AllData())
		{
			list2.Add(item);
		}
		Checked_1.Add(point);
		PointsAtSteps.Add(list2);
	}

	public void TryPrintLogs(CustomNavigationPoint result)
	{
		if (!DoLog)
		{
			return;
		}
		string text = $"data search id:{Data.Bot.Id}  role:{Data.Bot.Type} SearchType:{Data.SearchType} ShootTYpe:{Data.shootType.ToString()}  botPos:{Data.Bot.Position}   CheckShootHide:{Data.CheckShootHide}";
		StringBuilder stringBuilder = new StringBuilder("Cover search complete:" + text);
		stringBuilder.AppendLine();
		stringBuilder.Append("CarePositions:");
		foreach (Vector3 carePosition in Data.CarePositions)
		{
			stringBuilder.Append(" " + carePosition.ToString("0.0") + " ");
		}
		stringBuilder.AppendLine();
		for (int i = 0; i < Checked_1.Count; i++)
		{
			GroupPointSearchData groupPointSearchData = Checked_1[i];
			List<GroupPointSearchData> list = PointsAtSteps[i];
			StringBuilder stringBuilder2 = new StringBuilder($" id:{groupPointSearchData.Point.Id} power:{groupPointSearchData.Power} ({groupPointSearchData.Cause.ToString()}) remain:");
			foreach (GroupPointSearchData item in list)
			{
				stringBuilder2.Append($" {item.Point.Id}[{item.Power}],");
			}
			stringBuilder.AppendLine(stringBuilder2.ToString());
		}
		if (result != null)
		{
			stringBuilder.AppendLine($"Result:{result.ParentGroupPointId}");
		}
		else
		{
			stringBuilder.AppendLine("No result");
		}
	}

	public static Color GetColorByCause(ECoverCheckCause cause)
	{
		return cause switch
		{
			ECoverCheckCause.CantHide => Color.red, 
			ECoverCheckCause.CantShoot => Color.blue, 
			ECoverCheckCause.IsSpotted => Color.yellow, 
			ECoverCheckCause.Behind => Color.black, 
			ECoverCheckCause.TooClose => Color.magenta, 
			ECoverCheckCause.NotFree => Color.cyan, 
			_ => Color.white, 
		};
	}

	public void GizmosDrawBySteps(int debugLastCoverDrawSteps)
	{
		Vector3 vector = Data.CenterPos + Vector3.up;
		if (StartVoxel != null)
		{
			StartVoxel.DrawGizmos(drawClosestPoint: false, drawDoorsLinks: false);
		}
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(vector, 0.3f);
		Gizmos.DrawRay(vector, Vector3.up * 100f);
		Gizmos.color = Color.red;
		Vector3 size = new Vector3(0.1f, 5f, 0.1f);
		foreach (Vector3 carePosition in Data.CarePositions)
		{
			Gizmos.DrawCube(carePosition, size);
		}
		for (int i = 0; i < Checked_1.Count && i < debugLastCoverDrawSteps; i++)
		{
			GroupPointSearchData groupPointSearchData = Checked_1[i];
			Vector3 center = groupPointSearchData.Point.Position + Vector3.up;
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(center, 0.2f);
			Gizmos.color = GetColorByCause(groupPointSearchData.Cause);
			Gizmos.DrawWireSphere(center, 0.3f);
		}
		int index = Mathf.Clamp(debugLastCoverDrawSteps, 0, Checked_1.Count - 1);
		List<GroupPointSearchData> list = PointsAtSteps[index];
		Gizmos.color = Color.green;
		foreach (GroupPointSearchData item in list)
		{
			Vector3 center2 = item.Point.Position + Vector3.up;
			Gizmos.DrawWireSphere(center2, 0.2f);
			Gizmos.DrawSphere(center2, 0.1f);
		}
	}
}

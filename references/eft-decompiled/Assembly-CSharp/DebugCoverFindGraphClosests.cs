using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugCoverFindGraphClosests : Interface0
{
	[NonSerialized]
	public List<GroupPointSearchData> SearchedPoints = new List<GroupPointSearchData>();

	[NonSerialized]
	public Vector3 StartPos;

	public float TimeSetted;

	public List<GroupPointSearchData> Checked => SearchedPoints;

	public Vector3 PositionStart => StartPos;

	public void UpdateList(GClass368<GroupPointSearchData> list, Vector3 startPos, CustomNavigationPoint taken)
	{
		TimeSetted = Time.time;
		StartPos = startPos;
		if (!GClass396.Instance.IsTraceEnable())
		{
			return;
		}
		SearchedPoints.Clear();
		foreach (GroupPointSearchData item in list.AllData())
		{
			SearchedPoints.Add(item);
		}
		if (SearchedPoints.Count == 0)
		{
			SearchedPoints.Add(new GroupPointSearchData(taken.GroupPoint, 1f));
		}
	}

	public void GizmosDrawBySteps(int debugLastCoverDrawSteps)
	{
		Vector3 vector = StartPos + Vector3.up;
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(vector, 0.3f);
		Gizmos.DrawRay(vector, Vector3.up * 100f);
		Gizmos.color = Color.red;
		new Vector3(0.1f, 5f, 0.1f);
		for (int i = 0; i < SearchedPoints.Count && i < debugLastCoverDrawSteps; i++)
		{
			GroupPointSearchData groupPointSearchData = SearchedPoints[i];
			Vector3 center = groupPointSearchData.Point.Position + Vector3.up;
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(center, 0.2f);
			Gizmos.color = DebugCoverFindGraphSearchData.GetColorByCause(groupPointSearchData.Cause);
			Gizmos.DrawWireSphere(center, 0.3f);
		}
	}
}

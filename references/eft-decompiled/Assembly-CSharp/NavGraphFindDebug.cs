using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class NavGraphFindDebug
{
	public Vector3 CenterPosition;

	public int StartPoint = -1;

	public CoverShootType ShootType;

	public int BotId;

	public List<NavGraphFindDebugStepInfo> StepsInfo = new List<NavGraphFindDebugStepInfo>();

	public string EndInfo;

	public string ZoneName;

	public NavGraphEditorPoint[] ResultWay;

	public int[] ResultWayIds;

	[NonSerialized]
	public int Step;

	[NonSerialized]
	public HashSet<int> CollectedSteps = new HashSet<int>();

	[NonSerialized]
	public NavGraphFindDebugStepInfo Cur;

	public NavGraphFindDebug(int botId, string zoneName, CoverShootType shootType)
	{
		ZoneName = zoneName;
		ShootType = shootType;
		BotId = botId;
		Cur = new NavGraphFindDebugStepInfo(this.Step, 0f);
		StepsInfo.Add(Cur);
	}

	public void Step(float rad, int step)
	{
		if (!CollectedSteps.Contains(step))
		{
			CollectedSteps.Add(step);
			this.Step = step;
			Cur = new NavGraphFindDebugStepInfo(this.Step, rad);
			StepsInfo.Add(Cur);
		}
	}

	public void AddInfo(int id, ENavGraphFindRemoveCause cause)
	{
		NavGraphFindDebugPointInfo info = new NavGraphFindDebugPointInfo
		{
			Cause = cause,
			Id = id
		};
		Cur.Add(info);
	}

	public void PrintLogs()
	{
		StringBuilder stringBuilder = new StringBuilder($"debug find zone:{ZoneName} cover BotId:{BotId}  shootType:{ShootType.ToString()}\n");
		stringBuilder.AppendLine($"StartPoint:{StartPoint} CenterPosition:{CenterPosition}");
		foreach (NavGraphFindDebugStepInfo item in StepsInfo)
		{
			stringBuilder.Append(item.GetInfo());
		}
		if (!GClass856.IsNullOrEmpty(EndInfo))
		{
			stringBuilder.AppendLine(EndInfo);
		}
		if (ResultWay != null)
		{
			stringBuilder.AppendLine("way:[");
			NavGraphEditorPoint[] resultWay = ResultWay;
			foreach (NavGraphEditorPoint navGraphEditorPoint in resultWay)
			{
				stringBuilder.Append($" {navGraphEditorPoint.Id}");
			}
			stringBuilder.Append("]");
		}
	}

	public void DrawGizmos(int stepConstrain, float radiusConstrain, NavGraphContainer container, int coverSelectId)
	{
		foreach (NavGraphFindDebugStepInfo item in StepsInfo)
		{
			if (item._step <= stepConstrain && item.Rad <= radiusConstrain)
			{
				item.DrawGizmos(container, coverSelectId);
				continue;
			}
			return;
		}
		if (ResultWay != null)
		{
			IPositionPoint[] resultWay = ResultWay;
			GClass420.DrawWayGizmos(resultWay);
		}
	}

	public void BadSearch(string whileEnd)
	{
		EndInfo = whileEnd;
	}

	public void AddResult(NavGraphEditorPoint[] way)
	{
		ResultWay = way;
	}

	public bool TryRefreshIdsToPoints(out NavGraphContainer container)
	{
		container = NavGraphContainer.CreateOfFind();
		List<NavGraphEditorPoint> list = new List<NavGraphEditorPoint>();
		container.RefreshData();
		int[] resultWayIds = ResultWayIds;
		int num = 0;
		int num2;
		while (true)
		{
			if (num < resultWayIds.Length)
			{
				num2 = resultWayIds[num];
				if (!container.DcPoints.TryGetValue(num2, out var value))
				{
					break;
				}
				list.Add(value);
				num++;
				continue;
			}
			ResultWay = list.ToArray();
			return true;
		}
		Debug.LogError($"can't find point with id:{num2}");
		return false;
	}

	public void AddPath(int nextWayPathId)
	{
		Cur.AddUsedPath(nextWayPathId);
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class NavGraphFindDebugStepInfo
{
	public int _step;

	public float Rad;

	public List<NavGraphFindDebugPointInfo> DebugPointInfos = new List<NavGraphFindDebugPointInfo>();

	public List<int> usedPathId = new List<int>();

	public NavGraphFindDebugStepInfo(int step, float rad)
	{
		_step = step;
		Rad = rad;
	}

	public void AddUsedPath(int info)
	{
		usedPathId.Add(info);
	}

	public void Add(NavGraphFindDebugPointInfo info)
	{
		DebugPointInfos.Add(info);
	}

	public StringBuilder GetInfo()
	{
		StringBuilder stringBuilder = new StringBuilder(_step);
		if (DebugPointInfos.Count == 0)
		{
			stringBuilder.AppendLine($"{_step.ToString()} nothing  Rad:{Rad}");
		}
		else
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (NavGraphFindDebugPointInfo debugPointInfo in DebugPointInfos)
			{
				stringBuilder2.Append(debugPointInfo.GetInfo() + " ,");
			}
			stringBuilder.AppendLine($"{_step.ToString()}  Rad:{Rad}  {stringBuilder2} ");
		}
		return stringBuilder;
	}

	public void DrawGizmos(NavGraphContainer container, int coverSelectId)
	{
		Vector3 up = Vector3.up * 0.1f;
		foreach (int item in usedPathId)
		{
			if (container.DcPath.TryGetValue(item, out var value))
			{
				value.DrawGizmosSelected(up);
			}
		}
		foreach (NavGraphFindDebugPointInfo debugPointInfo in DebugPointInfos)
		{
			int id = debugPointInfo.Id;
			NavGraphEditorPoint navGraphEditorPoint = container.DcPoints[id];
			if (id == coverSelectId)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(navGraphEditorPoint.Position + new Vector3(0f, 5f, 0f), 0.3f);
				Gizmos.DrawWireSphere(navGraphEditorPoint.Position + new Vector3(0f, 6f, 0f), 0.3f);
				Gizmos.DrawWireSphere(navGraphEditorPoint.Position + new Vector3(0f, 7f, 0f), 0.3f);
			}
			float num = 10f;
			switch (debugPointInfo.Cause)
			{
			case ENavGraphFindRemoveCause.noCover:
				Gizmos.color = Color.black;
				break;
			case ENavGraphFindRemoveCause.noHide:
				Gizmos.color = new Color(0.1f, 0.9f, 0f);
				Gizmos.DrawWireCube(navGraphEditorPoint.Position + new Vector3(0f, 2.5f), new Vector3(0.1f, 5f, 0.1f));
				Gizmos.color = Color.red;
				break;
			case ENavGraphFindRemoveCause.good:
				Gizmos.color = new Color(0.1f, 0.9f, 0f);
				Gizmos.DrawWireCube(navGraphEditorPoint.Position + new Vector3(0f, 5f), new Vector3(0.1f, 10f, 0.1f));
				break;
			case ENavGraphFindRemoveCause.place:
				Gizmos.color = Color.blue;
				break;
			case ENavGraphFindRemoveCause.coverType:
				Gizmos.color = Color.grey;
				break;
			case ENavGraphFindRemoveCause.wrongRaidus:
				Gizmos.color = Color.black;
				num = 1f;
				break;
			}
			Gizmos.DrawRay(navGraphEditorPoint.Position, Vector3.up * num);
		}
	}
}

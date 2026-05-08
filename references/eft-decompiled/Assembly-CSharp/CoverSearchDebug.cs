using System.Collections.Generic;
using UnityEngine;

public class CoverSearchDebug : MonoBehaviour
{
	public static CoverSearchDebug Instance;

	[SerializeField]
	private List<CoverSearchDebugIteration> _coverSearchIterations = new List<CoverSearchDebugIteration>();

	private GClass388 gclass388_0;

	public List<CoverSearchDebugIteration> CoverSearchIterations => _coverSearchIterations;

	public void Awake()
	{
		Instance = this;
	}

	public void SetCoverPointsDataCollector(GClass388 collector, Vector3 centerPoint, float radius, float deltaRadius)
	{
		if (gclass388_0 != collector)
		{
			gclass388_0 = collector;
		}
		if (_coverSearchIterations.Count == 0 || GClass856.SqrDistance(_coverSearchIterations[_coverSearchIterations.Count - 1].CenterPoint, centerPoint) > 0.25f)
		{
			CoverSearchDebugIteration coverSearchDebugIteration = new CoverSearchDebugIteration();
			coverSearchDebugIteration.CoverPointGroups.Radius = 0.5f;
			_coverSearchIterations.Add(coverSearchDebugIteration);
			coverSearchDebugIteration.CenterPoint = centerPoint;
		}
	}

	public void ProcessAddNearGroups(HashSet<CustomNavigationPoint> nearGroups, int level, Vector3 centerPos, float sqrRadius, string searchLabel)
	{
		CoverSearchDebugIteration coverSearchDebugIteration;
		if (level == 0)
		{
			coverSearchDebugIteration = new CoverSearchDebugIteration();
			_coverSearchIterations.Add(coverSearchDebugIteration);
			coverSearchDebugIteration.CenterPoint = centerPos;
			coverSearchDebugIteration.CoverPointGroups.Radius = 0.5f;
			coverSearchDebugIteration.searchLabel = searchLabel;
		}
		else
		{
			coverSearchDebugIteration = _coverSearchIterations[_coverSearchIterations.Count - 1];
		}
		foreach (CustomNavigationPoint nearGroup in nearGroups)
		{
			coverSearchDebugIteration.AddPointToIteration(nearGroup.Position, level, sqrRadius);
		}
	}

	public void SetResultCover(CustomNavigationPoint point)
	{
		if (point != null)
		{
			_coverSearchIterations[_coverSearchIterations.Count - 1].ResultCover = point.Position;
		}
	}

	public void OnDrawGizmos()
	{
		foreach (CoverSearchDebugIteration coverSearchIteration in _coverSearchIterations)
		{
			coverSearchIteration.Draw();
		}
	}
}

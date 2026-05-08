using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TrajectoryDebug : MonoBehaviour
{
	[CompilerGenerated]
	public class Class503
	{
		public string groupName;

		public bool method_0(TrajectoryDebugPointsGroup p)
		{
			return p.Label == groupName;
		}
	}

	[SerializeField]
	private List<TrajectoryDebugPointsGroup> _points = new List<TrajectoryDebugPointsGroup>();

	public List<TrajectoryDebugPointsGroup> Points => _points;

	public void Clear()
	{
		_points.Clear();
	}

	public void Add(string groupName, Vector3 point, float time)
	{
		Add(groupName, "Default", point, time);
	}

	public void Add(string groupName, string label, Vector3 point, float time)
	{
		TrajectoryDebugPointsGroup trajectoryDebugPointsGroup = _points.FirstOrDefault((TrajectoryDebugPointsGroup p) => p.Label == groupName);
		if (trajectoryDebugPointsGroup == null)
		{
			trajectoryDebugPointsGroup = new TrajectoryDebugPointsGroup(groupName);
			_points.Add(trajectoryDebugPointsGroup);
		}
		trajectoryDebugPointsGroup.Add(label, point, time);
	}

	public void OnDrawGizmos()
	{
		foreach (TrajectoryDebugPointsGroup point in _points)
		{
			point.Draw();
		}
	}
}

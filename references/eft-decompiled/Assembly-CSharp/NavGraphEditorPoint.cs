using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class NavGraphEditorPoint : IPositionPoint
{
	[NonSerialized]
	public HashSet<NavGraphEditorPoint> ClosestPointsAsPoints = new HashSet<NavGraphEditorPoint>();

	public List<int> ClosestPointsWaysIds = new List<int>();

	[NonSerialized]
	public List<NavGraphEditorPointWay> ClosestPointsWaysArray = new List<NavGraphEditorPointWay>();

	[NonSerialized]
	public ReachableClass Reachable;

	public int Id;

	public Vector3 _position;

	[NonSerialized]
	public bool IsInited;

	[NonSerialized]
	public float pathLength;

	public int _coverId;

	public NavGraphEditorPoint _parentPointSearchWay;

	[NonSerialized]
	public int SearchWayId;

	[NonSerialized]
	public uint SearchCoverId;

	[NonSerialized]
	public PatrolPoint PatrolPoint_0;

	[NonSerialized]
	[CompilerGenerated]
	public GroupPoint GroupPoint_0;

	public GroupPoint CoverData
	{
		[CompilerGenerated]
		get
		{
			return GroupPoint_0;
		}
		[CompilerGenerated]
		set
		{
			GroupPoint_0 = value;
		}
	}

	public int CoverDataId => _coverId;

	[field: NonSerialized]
	public NavGraphEditorPoint nodeFrom { get; set; }

	public Vector3 Position => _position;

	public bool HaveLinkedCover => CoverData != null;

	public bool HaveCoverId => _coverId > 0;

	public bool HavePatrol => PatrolPoint_0 != null;

	public bool HaveData
	{
		get
		{
			if (!HavePatrol)
			{
				return HaveLinkedCover;
			}
			return true;
		}
	}

	[field: NonSerialized]
	public FloatContainer FloatContainer { get; set; }

	public NavGraphEditorPoint ParentPointSearchWay
	{
		get
		{
			return _parentPointSearchWay;
		}
		set
		{
			_parentPointSearchWay = value;
			ParentPointSearchWayId = ((value == null) ? (-1) : _parentPointSearchWay.Id);
		}
	}

	[field: NonSerialized]
	public int ParentPointSearchWayId { get; set; } = -1;

	public static NavGraphEditorPoint Create(Vector3 p, GroupPoint coverPoint, int id, NavGraphVoxel voxel, NavGraphContainer container, ReachableClass reachable)
	{
		NavGraphEditorPoint navGraphEditorPoint = new NavGraphEditorPoint(p, id, reachable);
		voxel.AddPoint(navGraphEditorPoint);
		container.Points.Add(navGraphEditorPoint);
		navGraphEditorPoint.SetCoverData(coverPoint);
		return navGraphEditorPoint;
	}

	public static NavGraphEditorPoint Create(Vector3 p, PatrolPoint pointToAdd, int id, NavGraphVoxel voxel, NavGraphContainer container, ReachableClass reachable)
	{
		NavGraphEditorPoint navGraphEditorPoint = new NavGraphEditorPoint(p, id, reachable);
		voxel.AddPoint(navGraphEditorPoint);
		container.Points.Add(navGraphEditorPoint);
		navGraphEditorPoint.method_0(pointToAdd);
		return navGraphEditorPoint;
	}

	public static NavGraphEditorPoint Create(Vector3 p, int id, NavGraphVoxel voxel, NavGraphContainer container, ReachableClass reachable)
	{
		NavGraphEditorPoint navGraphEditorPoint = new NavGraphEditorPoint(p, id, reachable);
		voxel.AddPoint(navGraphEditorPoint);
		container.Points.Add(navGraphEditorPoint);
		return navGraphEditorPoint;
	}

	public bool CanReach(NavGraphEditorPoint navGraphEditor)
	{
		return Reachable.Id == navGraphEditor.Reachable.Id;
	}

	public static void ConnectPoints(NavGraphEditorPoint p1, NavGraphEditorPoint p2)
	{
		ReachableClass reachable;
		if (p1.Reachable.Count > p2.Reachable.Count)
		{
			reachable = p2.Reachable;
			foreach (NavGraphEditorPoint point in p2.Reachable.Points)
			{
				p1.Reachable.Add(point);
			}
		}
		else
		{
			reachable = p1.Reachable;
			foreach (NavGraphEditorPoint point2 in p1.Reachable.Points)
			{
				p2.Reachable.Add(point2);
			}
		}
		reachable.SetDeprecated();
	}

	public NavGraphEditorPoint(Vector3 p, int id, ReachableClass reachable)
	{
		Reachable = reachable;
		Reachable.Add(this);
		Id = id;
		_position = p;
	}

	public void SetCoverData(GroupPoint point)
	{
		CoverData = point;
		_coverId = point.Id;
		SearchWayId = 0;
		SearchCoverId = 0u;
	}

	public void AddWay(NavGraphEditorPointWay p1)
	{
		ClosestPointsWaysIds.Add(p1.Id);
		ClosestPointsWaysArray.Add(p1);
	}

	public void Copy(NavGraphVoxel voxel, NavGraphContainer nContainer)
	{
		bool num = CoverData != null;
		bool num2 = PatrolPoint_0 != null;
		NavGraphEditorPoint navGraphEditorPoint = new NavGraphEditorPoint(Position, Id, Reachable);
		voxel.AddPoint(navGraphEditorPoint);
		nContainer.Points.Add(navGraphEditorPoint);
		if (num2)
		{
			navGraphEditorPoint.method_0(PatrolPoint_0);
		}
		if (num)
		{
			navGraphEditorPoint.SetCoverData(CoverData);
		}
	}

	public void method_0(PatrolPoint pointToAdd)
	{
		PatrolPoint_0 = pointToAdd;
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public class NavGraphVoxelSimple : NavGraphVoxelBase
{
	[SerializeField]
	public int _closetsPointId;

	[NonSerialized]
	public List<AILootPoint> LootPoints = new List<AILootPoint>();

	[NonSerialized]
	public List<AIExfiltrationPoint> ExfiltrationPoints = new List<AIExfiltrationPoint>();

	[NonSerialized]
	public List<GroupPoint> Points = new List<GroupPoint>();

	[NonSerialized]
	public List<NavMeshDoorLink> DoorLinks;

	[NonSerialized]
	public List<BotOwner> BotsInside = new List<BotOwner>();

	public List<PlantedMineAIInfo> Mines = new List<PlantedMineAIInfo>();

	[NonSerialized]
	public List<IPositionPoint> PlacesToAvoid = new List<IPositionPoint>();

	public bool HaveNavMesh;

	public List<int> PointsIds = new List<int>();

	public List<int> EntrancesIds = new List<int>();

	public List<int> DoorLinksIds = new List<int>();

	public List<int> LootPointsIds = new List<int>();

	public List<int> ExfiltrationPointsIds = new List<int>();

	[NonSerialized]
	[CompilerGenerated]
	public GroupPoint GroupPoint_0;

	public bool HaveStartPointId => _closetsPointId > 0;

	public GroupPoint PointStartSearch
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

	public NavGraphVoxelSimple(Vector3 pos, int indexX, int indexY, int indexZ, int id)
		: base(pos, indexX, indexY, indexZ, id)
	{
	}

	public void AddCover(GroupPoint cover)
	{
		Points.Add(cover);
		PointsIds.Add(cover.Id);
	}

	public void AddMine(PlantedMineAIInfo mine)
	{
		Mines.Add(mine);
	}

	public void RemoveMine(PlantedMineAIInfo mine)
	{
		Mines.Remove(mine);
	}

	public void AddPlaceToAvoid(IPositionPoint mine)
	{
		if (PlacesToAvoid == null)
		{
			PlacesToAvoid = new List<IPositionPoint>();
		}
		PlacesToAvoid.Add(mine);
	}

	public void RemovePlaceToAvoid(IPositionPoint mine)
	{
		PlacesToAvoid.Remove(mine);
	}

	public void AddNeightbourhood(NavGraphVoxelSimple voxel)
	{
	}

	public void SetClosestsPoint(GroupPoint closetsPoint)
	{
		_closetsPointId = closetsPoint.Id;
		PointStartSearch = closetsPoint;
	}

	public Vector3 GetSize()
	{
		return new Vector3(10f, 5f, 10f);
	}

	public GroupPoint FindClosest(int botId, Vector3 pos, bool noRestrictions)
	{
		float num = float.MaxValue;
		GroupPoint result = null;
		if (Points == null)
		{
			return null;
		}
		for (int i = 0; i < Points.Count; i++)
		{
			GroupPoint groupPoint = Points[i];
			if (noRestrictions || (groupPoint.IsFreeById(botId) && !groupPoint.IsSpotted))
			{
				float sqrMagnitude = (groupPoint.Position - pos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = groupPoint;
				}
			}
		}
		return result;
	}

	[CanBeNull]
	public GroupPoint FindClosestPoint(int botId, Vector3 pos, bool noRestrictions = false, Func<GroupPoint, bool> isGood = null)
	{
		if (Points == null)
		{
			return null;
		}
		float num = float.MaxValue;
		GroupPoint result = null;
		for (int i = 0; i < Points.Count; i++)
		{
			GroupPoint groupPoint = Points[i];
			if ((noRestrictions || (groupPoint.IsFreeById(botId) && !groupPoint.IsSpotted)) && (isGood == null || isGood(groupPoint)))
			{
				float sqrMagnitude = (groupPoint.Position - pos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = groupPoint;
				}
			}
		}
		return result;
	}

	public GroupPoint GetClosestPoint(Vector3 crn, out float sDist)
	{
		return GetClosestPoint(crn, out sDist, -1);
	}

	public GroupPoint GetClosestPoint(Vector3 crn, out float sDist, int excludedPoint)
	{
		sDist = float.MaxValue;
		if (Points.Count == 0)
		{
			return null;
		}
		GroupPoint result = null;
		foreach (GroupPoint point in Points)
		{
			if (excludedPoint <= 0 || point.Id != excludedPoint)
			{
				float sqrMagnitude = (point.Position - crn).sqrMagnitude;
				if (sqrMagnitude < sDist)
				{
					sDist = sqrMagnitude;
					result = point;
				}
			}
		}
		return result;
	}

	public void AddDoor(NavMeshDoorLink link)
	{
		DoorLinks.Add(link);
		DoorLinksIds.Add(link.Id);
	}

	public void DrawGizmos(bool drawClosestPoint, bool drawDoorsLinks, float sizeCoef = 1f, bool drawPointsInside = false, Color? color = null)
	{
		Vector3 size = GetSize();
		if (color.HasValue)
		{
			Gizmos.color = color.Value;
		}
		else
		{
			Color cyan = Color.cyan;
			cyan.a = 0.4f;
			Gizmos.color = cyan;
		}
		Gizmos.DrawWireCube(Position + size * 0.5f, size * sizeCoef);
		Gizmos.DrawWireCube(Position, Vector3.one * 0.01f);
		if (drawPointsInside && Points != null)
		{
			foreach (GroupPoint point in Points)
			{
				point.DrawGizmos(withEdges: true, withCore: false);
			}
		}
		if (drawClosestPoint)
		{
			if (!HaveNavMesh)
			{
				return;
			}
			if (PointStartSearch != null)
			{
				if ((Position - PointStartSearch.Position).magnitude > 20f)
				{
					Gizmos.color = Color.yellow;
				}
				Gizmos.DrawLine(Position, PointStartSearch.Position);
			}
			else
			{
				Gizmos.color = Color.red;
			}
			Gizmos.DrawWireCube(Position + size * 0.5f, size * sizeCoef);
			Gizmos.DrawWireCube(Position, Vector3.one * 0.01f);
		}
		else
		{
			if (!drawDoorsLinks || DoorLinksIds == null || DoorLinksIds.Count <= 0)
			{
				return;
			}
			if (DoorLinks != null)
			{
				foreach (NavMeshDoorLink doorLink in DoorLinks)
				{
					Vector3 position = doorLink.transform.position;
					Gizmos.DrawWireCube(position, new Vector3(0.1f, 2f, 0.1f));
					Gizmos.DrawLine(position, Position);
				}
			}
			Gizmos.DrawWireCube(Position + size * 0.5f, size * sizeCoef);
			Gizmos.DrawWireCube(Position, Vector3.one * 0.01f);
		}
	}

	public void DrawDebug(Color color, float sizeCoef = 1f)
	{
		Vector3 size = GetSize();
		GClass810.DebugCube(Position + size * 0.5f, Quaternion.identity, size * sizeCoef, color);
	}

	public void Restore(Dictionary<int, GroupPoint> data)
	{
		if (_closetsPointId > 0)
		{
			PointStartSearch = data[_closetsPointId];
		}
		BotsInside = new List<BotOwner>();
		if (Mines == null)
		{
			Mines = new List<PlantedMineAIInfo>();
		}
		if (LootPoints == null)
		{
			LootPoints = new List<AILootPoint>();
		}
	}

	public void ClearEntrances()
	{
		EntrancesIds = new List<int>();
	}

	public void ClearDoors()
	{
		DoorLinks = new List<NavMeshDoorLink>();
		DoorLinksIds = new List<int>();
	}

	public void ClearLootPoints()
	{
		LootPoints = new List<AILootPoint>();
		LootPointsIds = new List<int>();
	}

	public void AddLootPoint(AILootPoint logic)
	{
		LootPointsIds.Add(logic.Id);
		LootPoints.Add(logic);
	}

	public void AddExfiltrationPoint(AIExfiltrationPoint logic)
	{
		ExfiltrationPointsIds.Add(logic.Id);
		ExfiltrationPoints.Add(logic);
	}

	public void RemovePoint(GroupPoint point)
	{
		Points?.Remove(point);
		PointsIds?.Remove(point.Id);
		if (point.Id == _closetsPointId && _closetsPointId > 0)
		{
			AICalculationsInfo.UnSuccess(EAICalcSystem.FindClosestPointsToVoxeles);
			Debug.LogError("BLOCKER! removed closest point! NEED TO RECALCULATE CLOSESTS POINTS TO VOXELES");
		}
	}

	public List<PlantedMineAIInfo> GetNearestMines()
	{
		return Mines;
	}

	public List<IPositionPoint> GetNearestPlacesToAvoid()
	{
		return PlacesToAvoid;
	}
}

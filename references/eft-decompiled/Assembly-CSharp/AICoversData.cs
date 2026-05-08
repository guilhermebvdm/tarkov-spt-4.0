using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AICoversData : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class140
	{
		public static readonly Class140 class140_0 = new Class140();

		public static Func<GroupPoint, bool> func_0;

		public static Func<GroupPoint, bool> func_1;

		public static Func<GroupPoint, bool> func_2;

		public static Func<GroupPoint, bool> func_3;

		public static Func<GroupPoint, bool> func_4;

		public static Func<NavGraphVoxelSimple, bool> func_5;

		public static Func<NavGraphVoxelSimple, bool> func_6;

		public static Func<NavGraphVoxelSimple, bool> func_7;

		public static Func<GroupPoint, bool> func_8;

		public static Func<GroupPoint, bool> func_9;

		public static Func<GroupPointWay, int> func_10;

		public static Func<GroupPointPath, int> func_11;

		public static Func<NavGraphVoxelSimple, bool> func_12;

		public static Func<GroupPoint, bool> func_13;

		public static Func<GroupPoint, bool> func_14;

		public static Func<GroupPoint, bool> func_15;

		public bool method_0(GroupPoint x)
		{
			return x.CoverType == CoverType.Foliage;
		}

		public bool method_1(GroupPoint x)
		{
			return x.CoverType == CoverType.Wall;
		}

		public bool method_2(GroupPoint x)
		{
			if (x.PointWithNeighborType != PointWithNeighborType.cover)
			{
				return x.PointWithNeighborType == PointWithNeighborType.both;
			}
			return true;
		}

		public bool method_3(GroupPoint x)
		{
			return x.PointWithNeighborType == PointWithNeighborType.ambush;
		}

		public bool method_4(GroupPoint x)
		{
			return x.AlwaysGood;
		}

		public bool method_5(NavGraphVoxelSimple x)
		{
			return x.DoorLinksIds.Count > 0;
		}

		public bool method_6(NavGraphVoxelSimple x)
		{
			return x.DoorLinks.Count > 0;
		}

		public bool method_7(NavGraphVoxelSimple x)
		{
			return x.EntrancesIds.Count > 0;
		}

		public bool method_8(GroupPoint x)
		{
			return x.NeighbourhoodsWaysIds.Count == 0;
		}

		public bool method_9(GroupPoint x)
		{
			return x.CoverType == CoverType.Foliage;
		}

		public int method_10(GroupPointWay x)
		{
			return x.Id;
		}

		public int method_11(GroupPointPath x)
		{
			return x.IdPath;
		}

		public bool method_12(NavGraphVoxelSimple x)
		{
			return x.HaveNavMesh;
		}

		public bool method_13(GroupPoint x)
		{
			return x.IsGoodInsideBuilding;
		}

		public bool method_14(GroupPoint x)
		{
			return x.IdEnvironment > 0;
		}

		public bool method_15(GroupPoint x)
		{
			return x.CoverType == CoverType.Foliage;
		}
	}

	[CompilerGenerated]
	public class Class141
	{
		public GroupPoint toClose;

		public int method_0(GroupPoint x, GroupPoint y)
		{
			float sqrMagnitude = (x.Position - toClose.Position).sqrMagnitude;
			float sqrMagnitude2 = (y.Position - toClose.Position).sqrMagnitude;
			if (sqrMagnitude > sqrMagnitude2)
			{
				return 1;
			}
			if (sqrMagnitude2 > sqrMagnitude)
			{
				return -1;
			}
			return 0;
		}
	}

	[CompilerGenerated]
	public class Class142
	{
		public AICoversData aicoversData_0;

		public Vector3 pos;

		public Vector3 method_0(int i)
		{
			return aicoversData_0.Points[i].Position - pos;
		}
	}

	[CompilerGenerated]
	public class Class143
	{
		public int connectionGroupId;

		public List<GroupPoint> connectionGroupOnly;

		public Vector3 pos;

		public bool method_0(GroupPoint x)
		{
			return x.ConnectionGroup == connectionGroupId;
		}

		public Vector3 method_1(int i)
		{
			return connectionGroupOnly[i].Position - pos;
		}
	}

	[SerializeField]
	public List<GroupPoint> Points = new List<GroupPoint>();

	public AIVoxelesData Voxels;

	public AIPatrolsData Patrols;

	public AIManualPointsHolder AIManualPointsHolder;

	public AICorePointHolder AICorePointsHolder;

	public AIMinesPositionsHolder AIMinesPositions;

	public AIDangerPlacesHolder AIDangerPlacesHolder;

	public AIPlaceInfoHolder AIPlaceInfoHolder;

	public BotZoneEntranceInfo EntranceInfo;

	public List<GroupPointPath> Pathes = new List<GroupPointPath>();

	public List<GroupPointWay> Ways = new List<GroupPointWay>();

	[SerializeField]
	private int _lastId;

	private int _lastPathId = -1;

	private GClass411 _cache;

	public bool DrawPointsWithCore;

	public bool DrawPointsWays;

	public bool DrawPoints;

	public bool DrawPointsOnlyBush;

	public bool DrawPointsOnlyIsGoodInsideBuilding;

	public bool DrawPointsOnlyIdEnvironment;

	public ECoverPointSpecial DrawPointsSpecial;

	public string SelectedPointsCount;

	public int DrawPlaceId = -1;

	public int DrawCorePoint = -1;

	public int DrawConnectionGroup = -1;

	public float GroupPonitUpRay = -1f;

	public int DistDraw = 20;

	public string NetSteps;

	public string UnconnctedMiniGroups;

	public string CoverByPortals;

	public NavGraphVoxelSimple[,,] VoxelesArray => Voxels.VoxelesArray;

	public List<NavGraphVoxelSimple> VoxelsList => Voxels.VoxelsList;

	public Vector3 MaxVoxelesValues => Voxels.MaxVoxelesValues;

	public Vector3 MinVoxelesValues => Voxels.MinVoxelesValues;

	public int MaxX => Voxels.MaxX;

	public int MaxY => Voxels.MaxY;

	public int MaxZ => Voxels.MaxZ;

	public int BushCovers { get; set; } = -1;

	public int AlwaysGood { get; set; } = -1;

	public int WallCovers { get; set; } = -1;

	public int ShootCovers { get; set; } = -1;

	public int AmbushCovers { get; set; } = -1;

	public int VoxelesWithDoorsByObjects { get; set; } = -1;

	public int VoxelesWithDoors { get; set; } = -1;

	public int VoxelesWithEntrances { get; set; } = -1;

	public int PointsSingleFoliage { get; set; } = -1;

	public int PointsSingle { get; set; } = -1;

	public HashSet<int> ConnectionsGroupCount { get; set; }

	public HashSet<int> CorePointsGroupByObjects { get; set; }

	public HashSet<int> CorePointsGroup { get; set; }

	public static AICoversData CreateOrFind(bool undestandAtGame = false)
	{
		BotZone botZone = UnityEngine.Object.FindObjectOfType<BotZone>();
		AIManualPointsHolder aIManualPointsHolder = UnityEngine.Object.FindObjectOfType<AIManualPointsHolder>();
		if (aIManualPointsHolder == null)
		{
			GameObject obj = new GameObject("AIManualPointsHolder");
			aIManualPointsHolder = obj.AddComponent<AIManualPointsHolder>();
			obj.transform.SetAsFirstSibling();
			obj.transform.SetParent(botZone.gameObject.transform);
			obj.transform.SetParent(null);
		}
		AICorePointHolder aICorePointHolder = UnityEngine.Object.FindObjectOfType<AICorePointHolder>();
		if (aICorePointHolder == null)
		{
			GameObject obj2 = new GameObject("AICorePointHolder");
			aICorePointHolder = obj2.AddComponent<AICorePointHolder>();
			obj2.transform.SetAsFirstSibling();
			obj2.transform.SetParent(botZone.gameObject.transform);
			obj2.transform.SetParent(null);
		}
		AIMinesPositionsHolder aIMinesPositionsHolder = UnityEngine.Object.FindObjectOfType<AIMinesPositionsHolder>();
		if (aIMinesPositionsHolder == null)
		{
			GameObject obj3 = new GameObject("AIMinesPositionsHolder");
			aIMinesPositionsHolder = obj3.AddComponent<AIMinesPositionsHolder>();
			obj3.transform.SetAsFirstSibling();
			obj3.transform.SetParent(botZone.gameObject.transform);
			obj3.transform.SetParent(null);
		}
		AIDangerPlacesHolder aIDangerPlacesHolder = UnityEngine.Object.FindObjectOfType<AIDangerPlacesHolder>();
		if (aIDangerPlacesHolder == null)
		{
			GameObject obj4 = new GameObject("AIDangerPlacesHolder");
			aIDangerPlacesHolder = obj4.AddComponent<AIDangerPlacesHolder>();
			obj4.transform.SetAsFirstSibling();
			obj4.transform.SetParent(botZone.gameObject.transform);
			obj4.transform.SetParent(null);
		}
		AIPlaceInfoHolder aIPlaceInfoHolder = UnityEngine.Object.FindObjectOfType<AIPlaceInfoHolder>();
		if (aIPlaceInfoHolder == null)
		{
			GameObject obj5 = new GameObject("AIPlaceInfoHolder");
			aIPlaceInfoHolder = obj5.AddComponent<AIPlaceInfoHolder>();
			obj5.transform.SetAsFirstSibling();
			obj5.transform.SetParent(botZone.gameObject.transform);
			obj5.transform.SetParent(null);
		}
		AIPatrolsData aIPatrolsData = UnityEngine.Object.FindObjectOfType<AIPatrolsData>();
		if (aIPatrolsData == null)
		{
			GameObject obj6 = new GameObject("AIPatrolsData");
			aIPatrolsData = obj6.AddComponent<AIPatrolsData>();
			obj6.transform.SetAsFirstSibling();
			obj6.transform.SetParent(botZone.gameObject.transform);
			obj6.transform.SetParent(null);
		}
		AIVoxelesData aIVoxelesData = UnityEngine.Object.FindObjectOfType<AIVoxelesData>();
		if (aIVoxelesData == null)
		{
			GameObject obj7 = new GameObject("AIVoxelesData");
			aIVoxelesData = obj7.AddComponent<AIVoxelesData>();
			obj7.transform.SetParent(botZone.gameObject.transform);
			obj7.transform.SetParent(null);
		}
		aIVoxelesData.transform.SetAsFirstSibling();
		AICoversData aICoversData = UnityEngine.Object.FindObjectOfType<AICoversData>();
		if (aICoversData == null)
		{
			aICoversData = new GameObject("AICoversData").AddComponent<AICoversData>();
		}
		if (botZone != null)
		{
			aICoversData.transform.SetParent(botZone.gameObject.transform);
			aICoversData.transform.SetParent(null);
		}
		aICoversData.transform.SetAsFirstSibling();
		aICoversData.Patrols = aIPatrolsData;
		aICoversData.Voxels = aIVoxelesData;
		aICoversData.AIDangerPlacesHolder = aIDangerPlacesHolder;
		aICoversData.AIMinesPositions = aIMinesPositionsHolder;
		aICoversData.AICorePointsHolder = aICorePointHolder;
		aICoversData.AIManualPointsHolder = aIManualPointsHolder;
		aICoversData.AIPlaceInfoHolder = aIPlaceInfoHolder;
		return aICoversData;
	}

	public bool RestoreData()
	{
		Dictionary<int, NavGraphVoxelSimple> dictionary = new Dictionary<int, NavGraphVoxelSimple>();
		Dictionary<int, GroupPoint> dictionary2 = new Dictionary<int, GroupPoint>();
		Dictionary<int, GroupPointWay> dictionary3 = new Dictionary<int, GroupPointWay>();
		dictionary2 = new Dictionary<int, GroupPoint>();
		int num = 0;
		GroupPoint groupPoint = null;
		try
		{
			foreach (GroupPoint point in Points)
			{
				groupPoint = point;
				num = point.Id;
				dictionary2.Add(num, point);
			}
			for (int i = 0; i < AIManualPointsHolder.ManualPoints.Count; i++)
			{
				GroupPoint groupPoint2 = AIManualPointsHolder.ManualPoints[i];
				num = groupPoint2.Id;
				if (dictionary2.ContainsKey(num))
				{
					AIManualPointsHolder.RemovePoint(groupPoint2);
					i--;
				}
				else
				{
					groupPoint = groupPoint2;
					dictionary2.Add(num, groupPoint2);
				}
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"can't add id:{num}   dcPoints.Count:{dictionary2.Count}  error:{arg}");
			if (groupPoint != null)
			{
				Debug.LogError($"lastPoint:{groupPoint.Position} Id:{groupPoint.Id}  ParentNavPoint:{groupPoint.ParentNavPoint} {groupPoint.CoverType.ToString()}");
				if (dictionary2.TryGetValue(num, out var value))
				{
					Debug.LogError($" oldGroupPoint:{value.Position} Id:{value.Id}  ParentNavPoint:{value.ParentNavPoint} {value.CoverType.ToString()}");
				}
			}
			return false;
		}
		Voxels.RestoreData(dictionary2, this);
		Patrols.Restore(AICorePointsHolder);
		foreach (GroupPointWay way in Ways)
		{
			dictionary3.Add(way.Id, way);
			way.Restore(dictionary2[way.IdTarget]);
		}
		HashSet<int> hashSet = new HashSet<int>();
		HashSet<int> hashSet2 = new HashSet<int>();
		HashSet<int> hashSet3 = new HashSet<int>();
		foreach (GroupPoint point2 in Points)
		{
			point2.NeighbourhoodsWays = new List<GroupPointWay>();
			foreach (int neighbourhoodsWaysId in point2.NeighbourhoodsWaysIds)
			{
				if (dictionary3.TryGetValue(neighbourhoodsWaysId, out var value2))
				{
					point2.NeighbourhoodsWays.Add(value2);
					continue;
				}
				Debug.LogError($"Restore error: point:{point2.Id} can't find wayData:{neighbourhoodsWaysId}");
				return false;
			}
			hashSet3.Add(point2.CorePointId);
			hashSet.Add(point2.ConnectionGroup);
		}
		ConnectionsGroupCount = hashSet;
		CorePointsGroup = hashSet3;
		AICorePoint[] array = UnityEngine.Object.FindObjectsOfType<AICorePoint>();
		foreach (AICorePoint aICorePoint in array)
		{
			hashSet2.Add(aICorePoint.ConnectionGroupId);
		}
		CorePointsGroupByObjects = hashSet2;
		try
		{
			float num2 = MaxVoxelesValues.x * MaxVoxelesValues.y * MaxVoxelesValues.z;
			Debug.Log($"Compare voxeles count:{num2}  and  {VoxelsList.Count}");
			dictionary.Clear();
			foreach (NavGraphVoxelSimple voxels in VoxelsList)
			{
				voxels.Points = new List<GroupPoint>();
				VoxelesArray[voxels.IndexX, voxels.IndexY, voxels.IndexZ] = voxels;
				if (voxels.PointsIds == null)
				{
					Debug.LogError("voxel have no points is null");
					continue;
				}
				foreach (int pointsId in voxels.PointsIds)
				{
					try
					{
						GroupPoint item = dictionary2[pointsId];
						voxels.Points.Add(item);
					}
					catch (Exception)
					{
						Debug.LogError($"ERROR! try get point from voxel by id:{pointsId} DcPoints!=null:{dictionary2 != null} Points:{Points.Count}");
						return false;
					}
				}
				if (!dictionary.ContainsKey(voxels.Id))
				{
					dictionary.Add(voxels.Id, voxels);
				}
			}
		}
		catch (Exception arg2)
		{
			Debug.LogError($"Refresh error  \n:{arg2}  ");
			return false;
		}
		BushCovers = Points.Count((GroupPoint x) => x.CoverType == CoverType.Foliage);
		IEnumerable<GroupPoint> source = Points.Where((GroupPoint x) => x.CoverType == CoverType.Wall);
		WallCovers = source.Count();
		ShootCovers = source.Count((GroupPoint x) => x.PointWithNeighborType == PointWithNeighborType.cover || x.PointWithNeighborType == PointWithNeighborType.both);
		AmbushCovers = source.Count((GroupPoint x) => x.PointWithNeighborType == PointWithNeighborType.ambush);
		AlwaysGood = Points.Count((GroupPoint x) => x.AlwaysGood);
		VoxelesWithDoors = VoxelsList.Count((NavGraphVoxelSimple x) => x.DoorLinksIds.Count > 0);
		VoxelesWithDoorsByObjects = VoxelsList.Count((NavGraphVoxelSimple x) => x.DoorLinks.Count > 0);
		VoxelesWithEntrances = VoxelsList.Count((NavGraphVoxelSimple x) => x.EntrancesIds.Count > 0);
		List<GroupPoint> list = Points.Where((GroupPoint x) => x.NeighbourhoodsWaysIds.Count == 0).ToList();
		PointsSingle = list.Count;
		PointsSingleFoliage = list.Count((GroupPoint x) => x.CoverType == CoverType.Foliage);
		InitForGamePoints();
		AIMinesPositions.InGameRestore(this);
		AIDangerPlacesHolder.InGameRestore(this);
		Debug.Log($"Restored  BushCovers:{BushCovers}  WallCovers:{WallCovers}(Shoot:{ShootCovers},Ambush:{AmbushCovers})  VoxelesWithDoors:{VoxelesWithDoors}/{VoxelesWithDoorsByObjects}  VoxelesWithEntrances:{VoxelesWithEntrances}");
		return true;
	}

	public void AddRange(List<GroupPoint> blanketPoints)
	{
		if (Points == null)
		{
			Points = new List<GroupPoint>();
		}
		foreach (GroupPoint blanketPoint in blanketPoints)
		{
			Points.Add(blanketPoint);
		}
	}

	public Vector3Int GetIndexes(Vector3 pos)
	{
		int x = (int)((float)(int)(pos.x - MinVoxelesValues.x) / 10f);
		int y = (int)((float)(int)(pos.y - MinVoxelesValues.y) / 5f);
		int z = (int)((float)(int)(pos.z - MinVoxelesValues.z) / 10f);
		return new Vector3Int(x, y, z);
	}

	public static NavGraphVoxelSimple smethod_0(AICoversData covers, Vector3 from)
	{
		NavGraphVoxelSimple voxelSafe = covers.GetVoxelSafe(from);
		if (voxelSafe != null)
		{
			return voxelSafe;
		}
		Vector3Int indexes = covers.GetIndexes(from);
		for (int i = 1; i < 10; i++)
		{
			foreach (NavGraphVoxelSimple item in covers.GetVoxelesExtended(indexes.x, indexes.y, indexes.z, i, onlyWithPoints: false))
			{
				if (item != null)
				{
					return item;
				}
			}
		}
		return null;
	}

	public NavGraphVoxelSimple GetVoxelSafe(Vector3 pos, bool withLogs = false)
	{
		int value = (int)((float)(int)(pos.x - MinVoxelesValues.x) / 10f);
		int value2 = (int)((float)(int)(pos.y - MinVoxelesValues.y) / 5f);
		int value3 = (int)((float)(int)(pos.z - MinVoxelesValues.z) / 10f);
		value = Mathf.Clamp(value, 0, MaxX - 1);
		value2 = Mathf.Clamp(value2, 0, MaxY - 1);
		value3 = Mathf.Clamp(value3, 0, MaxZ - 1);
		if (withLogs)
		{
			Debug.LogError($"GetVoxelSafe graph voxel:  x:{value}  y:{value2} z:{value3}  MaxX:{MaxX} MaxY:{MaxY}  MaxZ:{MaxZ}  min:{MinVoxelesValues}   pos:{pos}");
		}
		try
		{
			return VoxelesArray[value, value2, value3];
		}
		catch (Exception ex)
		{
			if (VoxelesArray == null)
			{
				Debug.LogError("Voxeles not INITED");
			}
			Debug.DrawRay(pos, Vector3.up * 50f, Color.yellow, 10f);
			Debug.LogError($"nav graph voxel error:  x:{value}  y:{value2} z:{value3}  MaxX:{MaxX} MaxY:{MaxY}  MaxZ:{MaxZ}");
			throw ex;
		}
	}

	public NavGraphVoxelSimple GetVoxelSafeByIndexes(int x, int y, int z)
	{
		x = Mathf.Clamp(x, 0, MaxX - 1);
		y = Mathf.Clamp(y, 0, MaxY - 1);
		z = Mathf.Clamp(z, 0, MaxZ - 1);
		return VoxelesArray[x, y, z];
	}

	public GroupPoint GetClosestsPointInVoxelesExtended(int x, int y, int z, int rad, Vector3 pos, int? cg, bool withFoliage = true)
	{
		GroupPoint result = null;
		float num = float.MaxValue;
		int num2 = Mathf.Clamp(rad + x + 1, 0, MaxX - 1);
		int num3 = Mathf.Clamp(rad + y + 1, 0, MaxY - 1);
		int num4 = Mathf.Clamp(rad + z + 1, 0, MaxZ - 1);
		int num5 = Mathf.Clamp(x - rad, 0, MaxX - 1);
		int num6 = Mathf.Clamp(y - rad, 0, MaxY - 1);
		int num7 = Mathf.Clamp(z - rad, 0, MaxZ - 1);
		for (int i = num5; i < num2; i++)
		{
			for (int j = num6; j < num3; j++)
			{
				for (int k = num7; k < num4; k++)
				{
					NavGraphVoxelSimple navGraphVoxelSimple = VoxelesArray[i, j, k];
					if (navGraphVoxelSimple.Points.Count <= 0)
					{
						continue;
					}
					for (int l = 0; l < navGraphVoxelSimple.Points.Count; l++)
					{
						GroupPoint groupPoint = navGraphVoxelSimple.Points[l];
						if ((withFoliage || groupPoint.CoverType != CoverType.Foliage) && (!cg.HasValue || groupPoint.ConnectionGroup == cg.Value))
						{
							float sqrMagnitude = (groupPoint.Position - pos).sqrMagnitude;
							if (sqrMagnitude < num)
							{
								num = sqrMagnitude;
								result = groupPoint;
							}
						}
					}
				}
			}
		}
		return result;
	}

	public List<NavGraphVoxelSimple> GetVoxelesExtended(int x, int y, int z, int rad, bool onlyWithPoints)
	{
		List<NavGraphVoxelSimple> list = new List<NavGraphVoxelSimple>();
		int num = Mathf.Clamp(rad + x + 1, 0, MaxX - 1);
		int num2 = Mathf.Clamp(rad + y + 1, 0, MaxY - 1);
		int num3 = Mathf.Clamp(rad + z + 1, 0, MaxZ - 1);
		int num4 = Mathf.Clamp(x - rad, 0, MaxX - 1);
		int num5 = Mathf.Clamp(y - rad, 0, MaxY - 1);
		int num6 = Mathf.Clamp(z - rad, 0, MaxZ - 1);
		for (int i = num4; i < num; i++)
		{
			for (int j = num5; j < num2; j++)
			{
				for (int k = num6; k < num3; k++)
				{
					NavGraphVoxelSimple navGraphVoxelSimple = VoxelesArray[i, j, k];
					bool flag = true;
					if (onlyWithPoints)
					{
						flag = navGraphVoxelSimple.Points.Count > 0;
					}
					if (flag)
					{
						list.Add(navGraphVoxelSimple);
					}
				}
			}
		}
		return list;
	}

	public void Clear()
	{
		Voxels?.Clear();
		Points?.Clear();
		_lastId = AIManualPointsHolder.GetMaxIdAndRecalcIds() + 1;
	}

	public int GetNextPathId()
	{
		if (_lastPathId <= 0)
		{
			int a = ((Ways.Count > 0) ? Ways.Max((GroupPointWay x) => x.Id) : 0);
			int b = ((Pathes.Count > 0) ? Pathes.Max((GroupPointPath x) => x.IdPath) : 0);
			_lastPathId = Mathf.Max(a, b) + 1;
		}
		_lastPathId++;
		return _lastPathId;
	}

	public int GetGroupPointNextId()
	{
		if (_lastId <= 1)
		{
			_lastId = Points.Count + 10;
		}
		_lastId++;
		return _lastId;
	}

	public void InitForGamePoints()
	{
		foreach (GroupPoint point in Points)
		{
			point.InitForGame(AICorePointsHolder);
		}
		if (AIManualPointsHolder != null)
		{
			foreach (GroupPoint manualPoint in AIManualPointsHolder.ManualPoints)
			{
				manualPoint.InitForGame(AICorePointsHolder);
			}
		}
		_cache = new GClass411(this);
	}

	public void CalcVoxelesCountTest()
	{
		List<NavGraphVoxelSimple> voxelsList = VoxelsList;
		int num = voxelsList.Count((NavGraphVoxelSimple x) => x.HaveNavMesh);
		Debug.LogError($"Voxeles with nav mesh:{num} / total:{voxelsList.Count}");
	}

	public void AddPath(GroupPointPath path)
	{
		Pathes.Add(path);
		Ways.Add(path.WayA);
		Ways.Add(path.WayB);
	}

	public bool DeletePath(GroupPointPath path)
	{
		if (!Pathes.Remove(path))
		{
			return false;
		}
		Ways.Remove(path.WayA);
		Ways.Remove(path.WayB);
		path.WayA.Target.RemoveWay(path.WayB);
		path.WayB.Target.RemoveWay(path.WayA);
		return true;
	}

	public void AddFoliagePoint(List<GClass414> grass, AITriangleData triangles)
	{
		foreach (GClass414 item in grass)
		{
			item.Point.CoverType = CoverType.Foliage;
			NavPoint navPoint = smethod_1(triangles, item.Point);
			method_0(navPoint, item.Triangle, triangles);
			Points.Add(item.Point);
		}
	}

	public void AddPortalPoint(GroupPoint point, AITriangleData triangles, NavTriangle triangle)
	{
		point.CoverType = CoverType.Wall;
		NavPoint navPoint = smethod_1(triangles, point);
		method_0(navPoint, triangle, triangles);
		Points.Add(point);
	}

	public static NavPoint smethod_1(AITriangleData triangles, GroupPoint point)
	{
		NavPoint navPoint;
		if (point.ParentNavPoint <= 0)
		{
			navPoint = triangles.GetPointNew(point.Position);
			triangles.TryAddPoint(navPoint);
			point.ParentNavPoint = navPoint.Id;
		}
		else
		{
			navPoint = triangles.GetPoint(point.ParentNavPoint);
		}
		return navPoint;
	}

	public void OnDrawGizmosSelected()
	{
		if (Points == null)
		{
			return;
		}
		int num = DistDraw * DistDraw;
		Vector3 position = Camera.current.transform.position;
		if (!DrawPoints)
		{
			return;
		}
		int num2 = 0;
		IEnumerable<GroupPoint> source = Points;
		if (DrawConnectionGroup >= 0)
		{
			source = source.Where((GroupPoint x) => x.ConnectionGroup == DrawConnectionGroup);
		}
		if (DrawCorePoint >= 0)
		{
			source = source.Where((GroupPoint x) => x.CorePointId == DrawCorePoint);
		}
		if (DrawPlaceId >= 0)
		{
			source = source.Where((GroupPoint x) => x.PlaceId == DrawPlaceId);
		}
		if (DrawPointsOnlyIsGoodInsideBuilding)
		{
			source = source.Where((GroupPoint x) => x.IsGoodInsideBuilding);
		}
		if (DrawPointsOnlyIdEnvironment)
		{
			source = source.Where((GroupPoint x) => x.IdEnvironment > 0);
		}
		if (DrawPointsSpecial > (ECoverPointSpecial)0)
		{
			source = source.Where((GroupPoint x) => x.Special.HasFlag(DrawPointsSpecial));
		}
		if (DrawPointsOnlyBush)
		{
			source = source.Where((GroupPoint x) => x.CoverType == CoverType.Foliage);
		}
		List<GroupPoint> list = source.ToList();
		int num3 = 0;
		foreach (GroupPoint item in list)
		{
			float sqrMagnitude = (item.Position - position).sqrMagnitude;
			num2++;
			if (sqrMagnitude < (float)num)
			{
				num3++;
				item.DrawGizmos(DrawPointsWays, DrawPointsWithCore, GroupPonitUpRay);
			}
		}
		SelectedPointsCount = $"{list.Count} Draw:{num3}";
	}

	public void method_0(NavPoint navPoint, NavTriangle triangle, AITriangleData data)
	{
		foreach (NavPoint point in triangle.Points)
		{
			NavEdge item = new NavEdge(data.GetId(), point, navPoint, null);
			navPoint.Edges.Add(item);
			navPoint.Edges.Add(item);
			data.Edge.Add(item);
		}
	}

	public void ClearDoors()
	{
		foreach (NavGraphVoxelSimple voxels in VoxelsList)
		{
			voxels.ClearDoors();
		}
	}

	public void ClearLootPoints()
	{
		foreach (NavGraphVoxelSimple voxels in VoxelsList)
		{
			voxels.ClearLootPoints();
		}
	}

	public void RemovePoint(GroupPoint point)
	{
		Points.Remove(point);
		Voxels.RemovePoint(point);
		foreach (GroupPointWay neighbourhoodsWay in point.NeighbourhoodsWays)
		{
			Ways.Remove(neighbourhoodsWay);
			GroupPoint target = neighbourhoodsWay.Target;
			List<GroupPointWay> list = new List<GroupPointWay>();
			foreach (GroupPointWay neighbourhoodsWay2 in target.NeighbourhoodsWays)
			{
				if (neighbourhoodsWay2.Target.Id == point.Id)
				{
					list.Add(neighbourhoodsWay2);
				}
			}
			foreach (GroupPointWay item in list)
			{
				target.RemoveWay(item);
				Ways.Remove(item);
			}
		}
	}

	public static List<GroupPoint> GetClosests(GroupPoint toClose, int count, List<GroupPoint> toChoose)
	{
		toChoose.Sort(delegate(GroupPoint x, GroupPoint y)
		{
			float sqrMagnitude = (x.Position - toClose.Position).sqrMagnitude;
			float sqrMagnitude2 = (y.Position - toClose.Position).sqrMagnitude;
			if (sqrMagnitude > sqrMagnitude2)
			{
				return 1;
			}
			return (sqrMagnitude2 > sqrMagnitude) ? (-1) : 0;
		});
		List<GroupPoint> list = new List<GroupPoint>();
		for (int num = 0; num < toChoose.Count && num < count; num++)
		{
			GroupPoint groupPoint = toChoose[num];
			if (groupPoint.Id != toClose.Id)
			{
				list.Add(groupPoint);
			}
		}
		return list;
	}

	public GroupPoint GetClosest(Vector3 pos)
	{
		if (Points.Count == 0)
		{
			return null;
		}
		int nearEntity = GClass369.GetNearEntity(Points.Count, (int i) => Points[i].Position - pos);
		return Points[nearEntity];
	}

	public GroupPoint GetClosest(Vector3 pos, int connectionGroupId)
	{
		if (Points.Count == 0)
		{
			return null;
		}
		List<GroupPoint> connectionGroupOnly = Points.Where((GroupPoint x) => x.ConnectionGroup == connectionGroupId).ToList();
		int nearEntity = GClass369.GetNearEntity(connectionGroupOnly.Count, (int i) => connectionGroupOnly[i].Position - pos);
		return connectionGroupOnly[nearEntity];
	}

	public List<CustomNavigationPoint> GetCovers(int botId)
	{
		return _cache.GetCovers(botId);
	}

	public void CachePoints()
	{
		_cache.PreCache(30);
	}

	[CompilerGenerated]
	public bool method_1(GroupPoint x)
	{
		return x.ConnectionGroup == DrawConnectionGroup;
	}

	[CompilerGenerated]
	public bool method_2(GroupPoint x)
	{
		return x.CorePointId == DrawCorePoint;
	}

	[CompilerGenerated]
	public bool method_3(GroupPoint x)
	{
		return x.PlaceId == DrawPlaceId;
	}

	[CompilerGenerated]
	public bool method_4(GroupPoint x)
	{
		return x.Special.HasFlag(DrawPointsSpecial);
	}
}

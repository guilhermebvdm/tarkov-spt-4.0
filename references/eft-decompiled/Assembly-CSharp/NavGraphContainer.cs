using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NavGraphContainer : MonoBehaviour
{
	[CompilerGenerated]
	public class Class158
	{
		public NavGraphEditorPoint from;

		public NavGraphEditorPoint to;

		public bool method_0(NavGraphEditorPath x)
		{
			if (x.FromId == from.Id && x.ToId == to.Id)
			{
				return true;
			}
			if (x.FromId == to.Id)
			{
				return x.ToId == from.Id;
			}
			return false;
		}
	}

	public List<NavMeshDoorLink> DoorLinks;

	public List<NavGraphEditorPoint> Points = new List<NavGraphEditorPoint>();

	public List<NavGraphEditorPath> Path = new List<NavGraphEditorPath>();

	public List<NavGraphEditorPointWay> PointWays = new List<NavGraphEditorPointWay>();

	public List<NavGraphVoxel> VoxelsList = new List<NavGraphVoxel>();

	[NonSerialized]
	public Dictionary<int, NavGraphEditorPoint> DcPoints = new Dictionary<int, NavGraphEditorPoint>();

	[NonSerialized]
	public Dictionary<int, NavGraphEditorPath> DcPath = new Dictionary<int, NavGraphEditorPath>();

	[NonSerialized]
	public Dictionary<int, NavGraphEditorPointWay> DcPointWays = new Dictionary<int, NavGraphEditorPointWay>();

	[NonSerialized]
	public Dictionary<int, NavGraphVoxel> DcVoxeles = new Dictionary<int, NavGraphVoxel>();

	[NonSerialized]
	public NavGraphVoxel[,,] VoxelesArray;

	public int MaxX;

	public int MaxY;

	public int MaxZ;

	public bool _drawSelected;

	public float DistDrawCover = 10f;

	public float DistDrawVoxeles = 10f;

	public bool DrawVoxeles;

	[NonSerialized]
	public bool IsInited;

	[NonSerialized]
	private int int_0;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private int int_1 = 10;

	private GClass420 gclass420_0;

	public Vector3 Min => vector3_0;

	public Vector3 Max => vector3_1;

	public static List<GroupPoint> CollectCovers()
	{
		AICoversData aICoversData = AICoversData.CreateOrFind();
		return aICoversData.Points.ToList().Concat(aICoversData.AIManualPointsHolder.ManualPoints).ToList();
	}

	public static NavGraphEditorPoint smethod_0(Vector3 pos, NavGraphVoxel voxel, HashSet<NavGraphVoxel> used)
	{
		int num = 0;
		NavGraphVoxel navGraphVoxel;
		while (true)
		{
			if (num < voxel.Neightbourhoods.Count)
			{
				navGraphVoxel = voxel.Neightbourhoods[num];
				if (!used.Contains(navGraphVoxel))
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		used.Add(navGraphVoxel);
		float sDist;
		NavGraphEditorPoint closestPoint = navGraphVoxel.GetClosestPoint(pos, out sDist);
		if (closestPoint != null)
		{
			return closestPoint;
		}
		return smethod_0(pos, navGraphVoxel, used);
	}

	public static List<IPositionPoint> smethod_1()
	{
		List<GroupPoint> list = CollectCovers();
		List<IPositionPoint> list2 = new List<IPositionPoint>();
		foreach (GroupPoint item in list)
		{
			list2.Add(item);
		}
		return list2;
	}

	public void InitEditor()
	{
		Path.Clear();
		Points.Clear();
		PointWays.Clear();
		DcPoints.Clear();
		DcPoints.Clear();
		DcPointWays.Clear();
		List<IPositionPoint> points = smethod_1();
		method_0(points);
		method_3();
	}

	public NavGraphVoxel GetVoxelEditor(Vector3 pos)
	{
		int num = (int)((float)(int)(pos.x - vector3_0.x) / 10f);
		int num2 = (int)((float)(int)(pos.y - vector3_0.y) / 5f);
		int num3 = (int)((float)(int)(pos.z - vector3_0.z) / 10f);
		try
		{
			return VoxelesArray[num, num2, num3];
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.DrawRay(pos, Vector3.up * 50f, Color.yellow, 10f);
			UnityEngine.Debug.LogError($"nav graph voxel error: x:{num}  y:{num2}  z:{num3}  MaxX:{MaxX}  MaxY:{MaxY}  MaxZ:{MaxZ}  min:{vector3_0} max:{vector3_1}  pos:{pos}");
			if (VoxelesArray != null)
			{
				UnityEngine.Debug.LogError($"VoxelesArray:{VoxelesArray.Length}");
			}
			throw ex;
		}
	}

	public NavGraphVoxel GetVoxelSafe(Vector3 pos, bool withLogs = false)
	{
		int value = (int)((float)(int)(pos.x - vector3_0.x) / 10f);
		int value2 = (int)((float)(int)(pos.y - vector3_0.y) / 5f);
		int value3 = (int)((float)(int)(pos.z - vector3_0.z) / 10f);
		value = Mathf.Clamp(value, 0, MaxX - 1);
		value3 = Mathf.Clamp(value3, 0, MaxZ - 1);
		value2 = Mathf.Clamp(value2, 0, MaxY - 1);
		if (withLogs)
		{
			UnityEngine.Debug.LogError($"GetVoxelSafe graph voxel:  x:{value}  y:{value2} z:{value3}  MaxX:{MaxX}  MaxZ:{MaxZ}  min:{vector3_0}   pos:{pos}");
		}
		try
		{
			return VoxelesArray[value, value2, value3];
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.DrawRay(pos, Vector3.up * 50f, Color.yellow, 10f);
			UnityEngine.Debug.LogError($"nav graph voxel error:  x:{value}  y:{value2} z:{value3}  MaxX:{MaxX}  MaxZ:{MaxZ}");
			throw ex;
		}
	}

	public void ConvertDicToList()
	{
	}

	public void ConvertListToDic()
	{
		DcPointWays.Clear();
		foreach (NavGraphEditorPointWay pointWay in PointWays)
		{
			DcPointWays.Add(pointWay.Id, pointWay);
		}
		DcPath.Clear();
		foreach (NavGraphEditorPath item in Path)
		{
			DcPath.Add(item.Id, item);
		}
	}

	public NavGraphEditorPointWay GetWay(int navGraphEditorPointWay)
	{
		return DcPointWays[navGraphEditorPointWay];
	}

	public int GetNewId()
	{
		return int_1++;
	}

	public void ResetId()
	{
		int_1 = 10;
	}

	public void AddPath(NavGraphEditorPath navGraphEditorPath)
	{
		Path.Add(navGraphEditorPath);
	}

	public void AddWay(NavGraphEditorPointWay p1)
	{
		PointWays.Add(p1);
	}

	public void CheckData()
	{
		if (!IsInited)
		{
			UnityEngine.Debug.LogError("do init");
			return;
		}
		int num = 0;
		foreach (NavGraphEditorPoint point in Points)
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			foreach (NavGraphEditorPointWay item in point.ClosestPointsWaysArray)
			{
				num++;
				if (list.Contains(item.Id))
				{
					UnityEngine.Debug.LogError($"point have wrong:{point.Id} have bad closetsWays:{item.Id}");
				}
				else
				{
					list.Add(item.Id);
				}
				if (list2.Contains(item.TargetPointId))
				{
					UnityEngine.Debug.LogError($"point have wrong:{point.Id} have bad listIdsTrgs:{item.TargetPointId}");
				}
				else
				{
					list2.Add(item.TargetPointId);
				}
			}
		}
		UnityEngine.Debug.Log($"Check complete:{num}");
	}

	public void RefreshData()
	{
		ConvertListToDic();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		RefreshPointsData();
		foreach (KeyValuePair<int, NavGraphEditorPointWay> dcPointWay in DcPointWays)
		{
			NavGraphEditorPointWay value = dcPointWay.Value;
			if (DcPoints.TryGetValue(value.TargetPointId, out var value2))
			{
				value.TargetPoint = value2;
			}
			else
			{
				UnityEngine.Debug.LogError($" RefreshData:  DcPoints:{value.Id}  can't find trg TargetPointId:{value.TargetPointId}  way:{value.Id} StartPointId:{value.StartPointId}");
			}
			if (DcPoints.TryGetValue(value.StartPointId, out var value3))
			{
				value.StartPoint = value3;
			}
			else
			{
				UnityEngine.Debug.LogError($"RefreshData:  DcPoints:{value.Id}  can't find startPoint :{value.StartPointId}  ");
			}
			if (DcPath.TryGetValue(value.PathId, out var value4))
			{
				value.Path = value4;
			}
			else
			{
				UnityEngine.Debug.LogError($" RefreshData:  DcPoints:{value.Id}  can't find path:{value.PathId}");
			}
			if (DcPointWays.TryGetValue(value.ReverseId, out var value5))
			{
				value.Reverse = value5;
			}
			else
			{
				UnityEngine.Debug.LogError($"RefreshData:  DcPoints:{value.Id}  can't double path:{value.ReverseId}");
			}
		}
		foreach (KeyValuePair<int, NavGraphEditorPath> item2 in DcPath)
		{
			NavGraphEditorPath value6 = item2.Value;
			value6.SetBaseCoef();
			if (DcPoints.TryGetValue(value6.FromId, out var value7))
			{
				value6.From = value7;
			}
			else
			{
				UnityEngine.Debug.LogError($" RefreshData:  DcPath:{value6.Id}  can't from path:{value6.FromId}");
			}
			if (DcPoints.TryGetValue(value6.ToId, out var value8))
			{
				value6.To = value8;
			}
			else
			{
				UnityEngine.Debug.LogError($" RefreshData:  DcPath:{value6.Id}  can't to path:{value6.ToId}");
			}
		}
		VoxelesArray = new NavGraphVoxel[MaxX, MaxY, MaxZ];
		try
		{
			int num = MaxX * MaxZ * MaxY;
			UnityEngine.Debug.Log($"Compare voxeles count:{num}  and  {VoxelsList.Count}");
			DcVoxeles.Clear();
			foreach (NavGraphVoxel voxels in VoxelsList)
			{
				voxels.Points = new List<NavGraphEditorPoint>();
				VoxelesArray[voxels.IndexX, voxels.IndexY, voxels.IndexZ] = voxels;
				if (voxels.PointsIds == null)
				{
					UnityEngine.Debug.LogError("voxel have no points is null");
					continue;
				}
				foreach (int pointsId in voxels.PointsIds)
				{
					try
					{
						NavGraphEditorPoint item = DcPoints[pointsId];
						voxels.Points.Add(item);
					}
					catch (Exception)
					{
						UnityEngine.Debug.LogError($"ERROR! try get point from voxel by id:{pointsId}   ");
					}
				}
				if (!DcVoxeles.ContainsKey(voxels.Id))
				{
					DcVoxeles.Add(voxels.Id, voxels);
				}
			}
			method_1(DcVoxeles);
			foreach (NavGraphVoxel voxels2 in VoxelsList)
			{
				GClass417.CachePathesToVoxel(voxels2);
			}
			if (DoorLinks != null)
			{
				foreach (NavMeshDoorLink doorLink in DoorLinks)
				{
					if (doorLink.ConnectedNavGraphPathOpenStateId > 0 && DcPath.TryGetValue(doorLink.ConnectedNavGraphPathOpenStateId, out var value9))
					{
						doorLink.ConnectedNavGraphOpenStatePath = value9;
					}
					if (doorLink.ConnectedNavGraphPathCloseStateId > 0 && DcPath.TryGetValue(doorLink.ConnectedNavGraphPathCloseStateId, out var value10))
					{
						doorLink.ConnectedNavGraphCloseStatePath = value10;
					}
				}
			}
			method_2(MaxX, MaxY, MaxZ);
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"Refresh error   \n:{arg}  ");
		}
		List<IPositionPoint> points = smethod_1();
		method_0(points);
		stopwatch.Stop();
		UnityEngine.Debug.Log($"Refresh nav graph ends ElapsedMilliseconds:{stopwatch.ElapsedMilliseconds}");
		IsInited = true;
	}

	public void RemovePoint(NavGraphEditorPoint point)
	{
		if (point == null)
		{
			UnityEngine.Debug.LogError("Try remove null point");
			return;
		}
		Points.Remove(point);
		DcPoints.Remove(point.Id);
		foreach (NavGraphEditorPoint closestPointsAsPoint in point.ClosestPointsAsPoints)
		{
			closestPointsAsPoint.ClosestPointsAsPoints.Remove(point);
		}
		foreach (NavGraphEditorPointWay item in point.ClosestPointsWaysArray.ToList())
		{
			item.TargetPoint.ClosestPointsWaysIds.Remove(item.ReverseId);
			item.TargetPoint.ClosestPointsWaysArray.Remove(item.Reverse);
			PointWays.Remove(item);
			PointWays.Remove(item.Reverse);
			Path.Remove(item.Path);
			DcPath.Remove(item.PathId);
			DcPointWays.Remove(item.Id);
			DcPointWays.Remove(item.Reverse.Id);
			DcPath.Remove(item.PathId);
		}
		GetVoxelSafe(point.Position).RemovePoint(point);
	}

	public void RefreshPointsData()
	{
		DcPoints.Clear();
		DcPointWays.Clear();
		foreach (NavGraphEditorPoint point in Points)
		{
			DcPoints.Add(point.Id, point);
		}
		foreach (NavGraphEditorPointWay pointWay in PointWays)
		{
			DcPointWays.Add(pointWay.Id, pointWay);
		}
		foreach (KeyValuePair<int, NavGraphEditorPoint> dcPoint in DcPoints)
		{
			NavGraphEditorPoint value = dcPoint.Value;
			List<NavGraphEditorPointWay> list = new List<NavGraphEditorPointWay>();
			value.ClosestPointsAsPoints = new HashSet<NavGraphEditorPoint>();
			foreach (int closestPointsWaysId in value.ClosestPointsWaysIds)
			{
				if (DcPointWays.TryGetValue(closestPointsWaysId, out var value2))
				{
					list.Add(value2);
					value.ClosestPointsAsPoints.Add(value2.TargetPoint);
				}
				else
				{
					UnityEngine.Debug.LogError($"RefreshData:  point:{value.Id}  can't find way:{closestPointsWaysId} DcPointWays:{DcPointWays.Count}");
				}
			}
			value.ClosestPointsWaysArray = list.ToList();
			value.IsInited = true;
		}
	}

	public void SetLastWayAstar(GClass420 wayStarCalc)
	{
		gclass420_0 = wayStarCalc;
	}

	public bool HaveSamePath(NavGraphEditorPoint from, NavGraphEditorPoint to)
	{
		return Path.FirstOrDefault(delegate(NavGraphEditorPath x)
		{
			if (x.FromId == from.Id && x.ToId == to.Id)
			{
				return true;
			}
			return x.FromId == to.Id && x.ToId == from.Id;
		}) != null;
	}

	public NavGraphEditorPoint GetClosets(Vector3 pos)
	{
		NavGraphVoxel voxelSafe = GetVoxelSafe(pos);
		float sDist;
		NavGraphEditorPoint closestPoint = voxelSafe.GetClosestPoint(pos, out sDist);
		if (closestPoint != null)
		{
			return closestPoint;
		}
		HashSet<NavGraphVoxel> hashSet = new HashSet<NavGraphVoxel>();
		hashSet.Add(voxelSafe);
		NavGraphEditorPoint navGraphEditorPoint = smethod_0(pos, voxelSafe, hashSet);
		if (navGraphEditorPoint != null)
		{
			return navGraphEditorPoint;
		}
		return Points.FirstOrDefault();
	}

	public int GetSearchIdInGame()
	{
		int_0++;
		return int_0;
	}

	public NavGraphVoxel GetVoxelByIndex(int i, int j, int k)
	{
		i = Mathf.Clamp(i, 0, MaxX - 1);
		j = Mathf.Clamp(j, 0, MaxY - 1);
		k = Mathf.Clamp(k, 0, MaxZ - 1);
		return VoxelesArray[i, j, k];
	}

	public void method_0<T>(List<T> points) where T : IPositionPoint
	{
		vector3_1 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		vector3_0 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		foreach (T point in points)
		{
			float x = point.Position.x;
			float y = point.Position.y;
			float z = point.Position.z;
			if (x > vector3_1.x)
			{
				vector3_1.x = x;
			}
			if (z > vector3_1.z)
			{
				vector3_1.z = z;
			}
			if (y > vector3_1.y)
			{
				vector3_1.y = y;
			}
			if (x < vector3_0.x)
			{
				vector3_0.x = x;
			}
			if (z < vector3_0.z)
			{
				vector3_0.z = z;
			}
			if (y < vector3_0.y)
			{
				vector3_0.y = y;
			}
		}
		vector3_0.x = (int)(vector3_0.x - 1f);
		vector3_0.y = (int)(vector3_0.y - 1f);
		vector3_0.z = (int)(vector3_0.z - 1f);
		vector3_1.x = (int)(vector3_1.x + 1f);
		vector3_1.y = (int)(vector3_1.y + 1f);
		vector3_1.z = (int)(vector3_1.z + 1f);
		UnityEngine.Debug.Log($"InitMinMax: min:{vector3_0} max:{vector3_1}   points:{points.Count}");
	}

	public void method_1(Dictionary<int, NavGraphVoxel> dcVoxeles)
	{
		foreach (KeyValuePair<int, NavGraphVoxel> dcVoxele in dcVoxeles)
		{
			NavGraphVoxel value = dcVoxele.Value;
			value.Neightbourhoods = new List<NavGraphVoxel>();
			foreach (int neightbourhoodsId in value.NeightbourhoodsIds)
			{
				value.Neightbourhoods.Add(dcVoxeles[neightbourhoodsId]);
			}
		}
	}

	public void method_2(int deltaX, int deltaY, int deltaZ)
	{
		for (int i = 0; i < deltaX; i++)
		{
			for (int j = 0; j < deltaZ; j++)
			{
				for (int k = 0; k < deltaY; k++)
				{
					NavGraphVoxel navGraphVoxel = VoxelesArray[i, k, j];
					navGraphVoxel.Neightbourhoods = new List<NavGraphVoxel>();
					if (i > 0)
					{
						navGraphVoxel.AddNeightbourhood(VoxelesArray[i - 1, k, j]);
					}
					if (i < deltaX - 1)
					{
						navGraphVoxel.AddNeightbourhood(VoxelesArray[i + 1, k, j]);
					}
					if (j > 0)
					{
						navGraphVoxel.AddNeightbourhood(VoxelesArray[i, k, j - 1]);
					}
					if (j < deltaZ - 1)
					{
						navGraphVoxel.AddNeightbourhood(VoxelesArray[i, k, j + 1]);
					}
				}
			}
		}
	}

	public void method_3()
	{
		int num = 1 + (int)((vector3_1.x - vector3_0.x) / 10f);
		int num2 = 2 + (int)((vector3_1.y - vector3_0.y) / 5f);
		int num3 = 1 + (int)((vector3_1.z - vector3_0.z) / 10f);
		UnityEngine.Debug.DrawLine(vector3_0, vector3_1, Color.red, 10f);
		VoxelesArray = new NavGraphVoxel[num, num2, num3];
		VoxelsList = new List<NavGraphVoxel>(num * num3);
		int num4 = 0;
		for (int i = 0; i < num; i++)
		{
			float x = vector3_0.x + (float)i * 10f;
			for (int j = 0; j < num3; j++)
			{
				float z = vector3_0.z + (float)j * 10f;
				for (int k = 0; k < num2; k++)
				{
					float y = vector3_0.y + (float)k * 5f;
					NavGraphVoxel navGraphVoxel = new NavGraphVoxel(new Vector3(x, y, z), i, k, j, num4++);
					VoxelsList.Add(navGraphVoxel);
					VoxelesArray[i, k, j] = navGraphVoxel;
				}
			}
		}
		method_2(num, num2, num3);
		MaxX = num;
		MaxY = num2;
		MaxZ = num3;
		UnityEngine.Debug.Log($"Voxeles created  min:{vector3_0}   max:{vector3_1}   deltaX:{num}   deltaY:{num2} deltaZ:{num3}");
	}

	public void OnDrawGizmos()
	{
		if (IsInited)
		{
			gclass420_0?.OnDrawGizmosSelected();
			if (_drawSelected)
			{
				method_4();
			}
		}
	}

	public void method_4()
	{
		Vector3 position = Camera.current.transform.position;
		float num = DistDrawCover * DistDrawCover;
		Vector3 vector = Vector3.up * 0.1f;
		foreach (NavGraphEditorPoint point in Points)
		{
			if ((point.Position - position).sqrMagnitude < num)
			{
				Color color = (Gizmos.color = (point.HaveLinkedCover ? Color.yellow : (point.HavePatrol ? Color.blue : Color.green)));
				Gizmos.DrawRay(point.Position, vector);
				GClass810.DrawCircle(point.Position, color, 0.05f);
				GClass810.DrawCircle(point.Position + vector, color, 0.1f);
			}
		}
		Gizmos.color = Color.magenta;
		foreach (NavGraphEditorPath item in Path)
		{
			if ((item.MidPos() - position).sqrMagnitude < num)
			{
				item.DrawGizmosSelected(vector);
			}
		}
		method_5();
	}

	public void OnDrawGizmosSelected()
	{
		if (IsInited && !_drawSelected)
		{
			method_4();
		}
	}

	public void method_5()
	{
		if (!DrawVoxeles)
		{
			return;
		}
		Gizmos.DrawRay(vector3_0, Vector3.up * 100f);
		Vector3 position = Camera.current.transform.position;
		float num = DistDrawVoxeles * DistDrawVoxeles;
		for (int i = 0; i < MaxX; i++)
		{
			for (int j = 0; j < MaxZ; j++)
			{
				for (int k = 0; k < MaxY; k++)
				{
					NavGraphVoxel navGraphVoxel = VoxelesArray[i, k, j];
					if (!((navGraphVoxel.Position - position).sqrMagnitude > num))
					{
						navGraphVoxel.OnDrawGizmos(Color.cyan);
					}
				}
			}
		}
	}

	public static NavGraphContainer CreateOfFind()
	{
		NavGraphContainer navGraphContainer = UnityEngine.Object.FindObjectOfType<NavGraphContainer>();
		if (navGraphContainer != null)
		{
			return navGraphContainer;
		}
		navGraphContainer = new GameObject("NavGraphContainer").AddComponent<NavGraphContainer>();
		BotZone botZone = UnityEngine.Object.FindObjectOfType<BotZone>();
		if (botZone != null)
		{
			navGraphContainer.transform.SetParent(botZone.transform.parent);
		}
		return navGraphContainer;
	}
}

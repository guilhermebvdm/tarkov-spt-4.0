using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using UnityEngine;

public class BotVoxelesPersonalData : GClass429
{
	[NonSerialized]
	public List<CustomNavigationPoint> AllPoints = new List<CustomNavigationPoint>();

	[NonSerialized]
	public AICoversData Data;

	public int MaxX => Data.MaxX;

	public int MaxY => Data.MaxY;

	public int MaxZ => Data.MaxZ;

	public Vector3 _min => Data.MinVoxelesValues;

	[field: NonSerialized]
	public NavGraphVoxelSimple CurVoxel { get; set; }

	public event Action<NavGraphVoxelSimple> OnVoxelChange;

	public BotVoxelesPersonalData(BotOwner owner)
		: base(owner)
	{
	}

	public List<CustomNavigationPoint> Points(Func<CustomNavigationPoint, bool> func)
	{
		return AllPoints.Where(func).ToList();
	}

	public void Activate(AICoversData data)
	{
		Data = data;
		int id = BotOwner_0.Id;
		AllPoints = data.GetCovers(id);
	}

	public int GetLastConnectionId()
	{
		if (BotOwner_0.Memory.BotCurrentCoverInfo.LastCover != null)
		{
			return BotOwner_0.Memory.BotCurrentCoverInfo.LastCover.CorePointInGame.ConnectionGroupId;
		}
		return BotOwner_0.VoxelesPersonalData.CurVoxel.PointStartSearch.CorePointInGame.ConnectionGroupId;
	}

	public NavGraphVoxelSimple GetVoxelSafe(Vector3 pos, bool withLogs = false)
	{
		int value = (int)((float)(int)(pos.x - _min.x) / 10f);
		int value2 = (int)((float)(int)(pos.y - _min.y) / 5f);
		int value3 = (int)((float)(int)(pos.z - _min.z) / 10f);
		value = Mathf.Clamp(value, 0, MaxX - 1);
		value2 = Mathf.Clamp(value2, 0, MaxY - 1);
		value3 = Mathf.Clamp(value3, 0, MaxZ - 1);
		if (withLogs)
		{
			Debug.LogError($"GetVoxelSafe graph voxel:  x:{value}  y:{value2} z:{value3}  MaxX:{MaxX}  MaxZ:{MaxZ}  min:{_min}   pos:{pos}");
		}
		try
		{
			NavGraphVoxelSimple navGraphVoxelSimple = Data.VoxelesArray[value, value2, value3];
			if (navGraphVoxelSimple != null)
			{
				return navGraphVoxelSimple;
			}
			_ = $"bot id:{BotOwner_0.Id}  at pos:{BotOwner_0.Position}  can't find SIMPLE voxel at :{pos}  _min:{_min}   MaxX:{MaxX}  MaxY:{MaxY} MaxZ:{MaxZ}  ";
			for (int i = 1; i < 10; i++)
			{
				foreach (NavGraphVoxelSimple item in Data.GetVoxelesExtended(value, value2, value3, i, onlyWithPoints: false))
				{
					if (item != null && item.HaveStartPointId)
					{
						return item;
					}
				}
			}
			return null;
		}
		catch (Exception ex)
		{
			Debug.DrawRay(pos, Vector3.up * 50f, Color.yellow, 10f);
			Debug.LogError($"nav graph voxel error:  x:{value}  y:{value2} z:{value3}  MaxX:{MaxX}  MaxZ:{MaxZ}");
			throw ex;
		}
	}

	public List<NavGraphVoxelSimple> GetVoxelesExtended(Vector3 pos, int rad, bool onlyWithPoints)
	{
		int value = (int)((float)(int)(pos.x - _min.x) / 10f);
		int value2 = (int)((float)(int)(pos.y - _min.y) / 5f);
		int value3 = (int)((float)(int)(pos.z - _min.z) / 10f);
		value = Mathf.Clamp(value, 0, MaxX - 1);
		value2 = Mathf.Clamp(value2, 0, MaxY - 1);
		value3 = Mathf.Clamp(value3, 0, MaxZ - 1);
		return Data.GetVoxelesExtended(value, value2, value3, rad, onlyWithPoints);
	}

	public GroupPoint GetClosestsPointVoxelesExtended(Vector3 pos, int rad, int connectionGroup, bool withFoliage)
	{
		int value = (int)((float)(int)(pos.x - _min.x) / 10f);
		int value2 = (int)((float)(int)(pos.y - _min.y) / 5f);
		int value3 = (int)((float)(int)(pos.z - _min.z) / 10f);
		value = Mathf.Clamp(value, 0, MaxX - 1);
		value2 = Mathf.Clamp(value2, 0, MaxY - 1);
		value3 = Mathf.Clamp(value3, 0, MaxZ - 1);
		return Data.GetClosestsPointInVoxelesExtended(value, value2, value3, rad, pos, connectionGroup, withFoliage);
	}

	public void SetCurrectVoxel(NavGraphVoxelSimple prevVoxel)
	{
		if (prevVoxel != CurVoxel)
		{
			CurVoxel = prevVoxel;
			this.OnVoxelChange?.Invoke(CurVoxel);
		}
	}

	public NavGraphVoxelSimple GetVoxelSafeByIndex(int x, int y, int z)
	{
		x = Mathf.Clamp(x, 0, MaxX - 1);
		z = Mathf.Clamp(z, 0, MaxY - 1);
		y = Mathf.Clamp(y, 0, MaxZ - 1);
		return Data.VoxelesArray[x, y, z];
	}
}

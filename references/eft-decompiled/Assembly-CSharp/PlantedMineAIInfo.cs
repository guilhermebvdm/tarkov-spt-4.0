using System;
using System.Collections.Generic;
using EFT.SynchronizableObjects;
using UnityEngine;

public class PlantedMineAIInfo
{
	[NonSerialized]
	public TripwireSynchronizableObject Throwweap;

	[NonSerialized]
	public List<NavGraphVoxelSimple> ConnectedVoxeles = new List<NavGraphVoxelSimple>();

	[NonSerialized]
	public int Id_1;

	[NonSerialized]
	public static int NextId;

	[NonSerialized]
	public bool IsDisposed;

	[NonSerialized]
	public int DeacitvatingBotId = -1;

	[NonSerialized]
	public float DeacitvatingTimeEnd = -1f;

	[NonSerialized]
	public bool IsDeactivating;

	public bool IsActive
	{
		get
		{
			if (Throwweap != null)
			{
				return Throwweap.IsActive;
			}
			return false;
		}
	}

	[field: NonSerialized]
	public Vector3 Pos { get; set; }

	public TripwireSynchronizableObject MineObject => Throwweap;

	public int Id => Id_1;

	public bool IsDeactivatingProcess
	{
		get
		{
			if (IsDeactivating && DeacitvatingTimeEnd < Time.time)
			{
				IsDeactivating = false;
			}
			return IsDeactivating;
		}
	}

	public event Action<PlantedMineAIInfo> OnDisposed;

	public PlantedMineAIInfo(TripwireSynchronizableObject throwweap, Vector3 pos)
	{
		Id_1 = NextId++;
		Throwweap = throwweap;
		Pos = pos;
		if (Throwweap != null)
		{
			Throwweap.OnDestroyed += method_0;
		}
	}

	public void Init(List<NavGraphVoxelSimple> allNearVoxeles)
	{
		ConnectedVoxeles = allNearVoxeles;
		foreach (NavGraphVoxelSimple connectedVoxele in ConnectedVoxeles)
		{
			connectedVoxele.AddMine(this);
		}
	}

	public void SetDeactivate(int botId)
	{
		DeacitvatingBotId = botId;
		DeacitvatingTimeEnd = Time.time + 0.3f;
		IsDeactivating = true;
	}

	public bool CanTakeToDeactivate(int botId)
	{
		if (IsDisposed)
		{
			return false;
		}
		if (Throwweap.TripwireState != ETripwireState.Wait)
		{
			return false;
		}
		if (IsDeactivatingProcess)
		{
			return botId == DeacitvatingBotId;
		}
		return true;
	}

	public void method_0(TripwireSynchronizableObject obj)
	{
		Dispose();
	}

	public void Dispose()
	{
		IsDisposed = true;
		foreach (NavGraphVoxelSimple connectedVoxele in ConnectedVoxeles)
		{
			connectedVoxele.RemoveMine(this);
		}
		if (Throwweap != null)
		{
			Throwweap.OnDestroyed -= method_0;
		}
		this.OnDisposed?.Invoke(this);
		this.OnDisposed = null;
	}

	public void Deativate()
	{
		Dispose();
	}
}

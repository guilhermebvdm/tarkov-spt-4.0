using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.SynchronizableObjects;
using UnityEngine;

public class BotBewarePlantedMine : GClass429
{
	[NonSerialized]
	public float NextCheck;

	[NonSerialized]
	public PlantedMineAIInfo Deactivating;

	[NonSerialized]
	public bool NextChance;

	[NonSerialized]
	public bool IsAtProcessDeactivating;

	[NonSerialized]
	public float DeativateFinish;

	[NonSerialized]
	public float DistCheck;

	[NonSerialized]
	public bool LastDistCheck;

	[NonSerialized]
	public float POSSIBLE_TIME_ENEMY_TO_DEAC = 20f;

	[NonSerialized]
	public const float SDITS_TO_WORK_WITH = 625f;

	public PlantedMineAIInfo DeactivatingPlace => Deactivating;

	public BotBewarePlantedMine(BotOwner owner)
		: base(owner)
	{
	}

	public void Activate()
	{
		BotOwner_0.VoxelesPersonalData.OnVoxelChange += method_1;
	}

	public void SetMineToDeactivate(PlantedMineAIInfo toDeactivate)
	{
		if (Deactivating != toDeactivate)
		{
			if (Deactivating != null)
			{
				Deactivating.OnDisposed -= method_0;
			}
			Deactivating = toDeactivate;
			if (Deactivating != null)
			{
				Deactivating.SetDeactivate(BotOwner_0.Id);
				Deactivating.OnDisposed += method_0;
			}
			NextChance = GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Mind.CHACE_TO_DEACTIVATE);
		}
	}

	public void method_0(PlantedMineAIInfo obj)
	{
		SetMineToDeactivate(null);
	}

	public void method_1(NavGraphVoxelSimple obj)
	{
		NextCheck = Time.time + 0.3f;
	}

	public void Dispose()
	{
		if (Deactivating != null)
		{
			Deactivating.OnDisposed -= method_0;
		}
		BotOwner_0.VoxelesPersonalData.OnVoxelChange -= method_1;
		SetMineToDeactivate(null);
	}

	public void Update()
	{
		if (NextCheck > Time.time)
		{
			return;
		}
		List<PlantedMineAIInfo> nearestMines = BotOwner_0.VoxelesPersonalData.CurVoxel.GetNearestMines();
		if (nearestMines.Count == 0)
		{
			return;
		}
		if (!BotOwner_0.HasPathAndNotComplete)
		{
			NextCheck = Time.time + 0.5f;
		}
		else
		{
			if (GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Mind.CHANCE_TO_IGNORE_TRIPWIRE))
			{
				return;
			}
			float num = float.MaxValue;
			float num2 = float.MinValue;
			PlantedMineAIInfo plantedMineAIInfo = null;
			PlantedMineAIInfo plantedMineAIInfo2 = null;
			List<Vector3> list = new List<Vector3>();
			foreach (PlantedMineAIInfo item in nearestMines)
			{
				Vector3 lhs = item.Pos - BotOwner_0.Position;
				if (!(Vector3.Dot(lhs, BotOwner_0.LookDirection) < 0f) && !(Mathf.Abs(lhs.y) > 1f))
				{
					float sqrMagnitude = lhs.sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						plantedMineAIInfo = item;
					}
					if (sqrMagnitude > num2)
					{
						num2 = sqrMagnitude;
						plantedMineAIInfo2 = item;
					}
					list.Add(item.Pos);
				}
			}
			if (plantedMineAIInfo == null)
			{
				return;
			}
			Vector3 vector = (plantedMineAIInfo.Pos + plantedMineAIInfo2.Pos) * 0.5f;
			GClass369.DebugDrawArc(BotOwner_0.Position, vector, 2f, 4f, Color.cyan);
			if (BotOwner_0.Mover.TryRelacePathAround(vector, list, out var _))
			{
				NextCheck = Time.time + 4f;
				return;
			}
			if (plantedMineAIInfo.CanTakeToDeactivate(BotOwner_0.Id) && BotOwner_0.Mover.IsPointOnCurrentWay(plantedMineAIInfo.Pos, 2.5f))
			{
				BotOwner_0.BewarePlantedMine.SetMineToDeactivate(plantedMineAIInfo);
			}
			NextCheck = Time.time + 2f;
		}
	}

	public bool CanDeactivate()
	{
		if (Deactivating == null)
		{
			return false;
		}
		if (BotOwner_0.Memory.HaveEnemy && Time.time - BotOwner_0.Memory.GoalEnemy.PersonalLastSeenTime < POSSIBLE_TIME_ENEMY_TO_DEAC)
		{
			return false;
		}
		if (!Deactivating.IsActive)
		{
			Deactivating = null;
			return false;
		}
		if (!Deactivating.CanTakeToDeactivate(BotOwner_0.Id))
		{
			Deactivating = null;
			return false;
		}
		if (DistCheck < Time.time)
		{
			DistCheck = Time.time + 3f;
			float sqrMagnitude = (Deactivating.Pos - BotOwner_0.Position).sqrMagnitude;
			LastDistCheck = sqrMagnitude < 625f;
		}
		return LastDistCheck;
	}

	public void DoDeactivateCurrent()
	{
		if (IsAtProcessDeactivating)
		{
			if (DeativateFinish < Time.time)
			{
				IsAtProcessDeactivating = false;
				GameWorld instance = Singleton<GameWorld>.Instance;
				if (instance != null && NextChance && Deactivating.MineObject.TripwireState == ETripwireState.Wait)
				{
					instance.DeActivateTripwire(Deactivating.MineObject);
				}
				Singleton<BotEventHandler>.Instance.DeactivateTripwire(Deactivating.MineObject);
			}
		}
		else
		{
			IsAtProcessDeactivating = true;
			DeativateFinish = Time.time + 5f;
		}
	}
}

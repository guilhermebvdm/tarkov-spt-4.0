using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class BotDogFight : GClass429
{
	[NonSerialized]
	public const float DIST_TO_BACK = 2f;

	[NonSerialized]
	public float NextRecalc;

	[NonSerialized]
	public float PosibleCheckCanGo;

	[NonSerialized]
	public bool PursuitInProgress;

	[NonSerialized]
	public NavMeshPath PathCached = new NavMeshPath();

	[field: NonSerialized]
	public BotDogFightStatus DogFightState { get; set; }

	public BotDogFight([NotNull] BotOwner owner)
		: base(owner)
	{
		DogFightState = BotDogFightStatus.none;
	}

	public void method_0()
	{
		DogFightState = BotDogFightStatus.shootFromPlace;
	}

	public bool method_1(out Vector3 trgPos)
	{
		trgPos = Vector3.zero;
		if (BotOwner_0.DoorOpener.DogFightHaveNearestDoor())
		{
			return false;
		}
		Vector3 vector = -GClass855.NormalizeFastSelf(BotOwner_0.Memory.GoalEnemy.Direction);
		float num = 0f;
		if (NavMesh.SamplePosition(BotOwner_0.Position + vector * 2f / 2f, out var hit, 1f, -1))
		{
			trgPos = hit.position;
			Vector3 vector2 = trgPos - BotOwner_0.Position;
			float magnitude = vector2.magnitude;
			if (magnitude != 0f)
			{
				Vector3 vector3 = vector2 / magnitude;
				num = magnitude;
				if (NavMesh.SamplePosition(BotOwner_0.Position + vector3 * 2f, out hit, 1f, -1))
				{
					trgPos = hit.position;
					num = (trgPos - BotOwner_0.Position).magnitude;
				}
			}
		}
		if (num != 0f && num > BotOwner_0.Settings.FileSettings.Move.REACH_DIST)
		{
			PathCached.ClearCorners();
			if (NavMesh.CalculatePath(BotOwner_0.Position, trgPos, -1, PathCached) && PathCached.status == NavMeshPathStatus.PathComplete)
			{
				trgPos = PathCached.corners[PathCached.corners.Length - 1];
				return method_2(PathCached, num);
			}
		}
		return false;
	}

	public bool ShallStartCauseHavePlace()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.IsVisible && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Mind.DOG_FIGHT_IN && BotOwner_0.DogFight.method_1(out var _))
		{
			DogFightState = BotDogFightStatus.dogFight;
			return true;
		}
		return false;
	}

	public void ManualUpdate()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			DogFightState = BotDogFightStatus.none;
			return;
		}
		if (BotOwner_0.Memory.BotCurrentCoverInfo.UseDogFight(BotOwner_0.Settings.FileSettings.Cover.DOG_FIGHT_AFTER_LEAVE))
		{
			DogFightState = BotDogFightStatus.dogFight;
			return;
		}
		Vector3 trgPos;
		switch (DogFightState)
		{
		case BotDogFightStatus.none:
			if (goalEnemy.IsVisible && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Mind.DOG_FIGHT_IN)
			{
				if (method_1(out trgPos))
				{
					DogFightState = BotDogFightStatus.dogFight;
				}
				else
				{
					method_0();
				}
			}
			break;
		case BotDogFightStatus.dogFight:
			if (goalEnemy.Distance > BotOwner_0.Settings.FileSettings.Mind.DOG_FIGHT_OUT)
			{
				DogFightState = BotDogFightStatus.none;
			}
			break;
		case BotDogFightStatus.shootFromPlace:
			if (PosibleCheckCanGo < Time.time)
			{
				PosibleCheckCanGo = Time.time + 1f;
				if (method_1(out trgPos))
				{
					DogFightState = BotDogFightStatus.none;
				}
			}
			if (goalEnemy.Distance > BotOwner_0.Settings.FileSettings.Mind.DOG_FIGHT_OUT)
			{
				DogFightState = BotDogFightStatus.none;
			}
			break;
		}
	}

	public void Fight()
	{
		if (!(NextRecalc < Time.time))
		{
			return;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			PursuitInProgress = false;
			return;
		}
		NextRecalc = Time.time + 1f;
		if (!PursuitInProgress && method_1(out var trgPos))
		{
			BotOwner_0.GoToPoint(trgPos, slowAtTheEnd: true, -1f, getUpWithCheck: false, mustHaveWay: false);
		}
		else if (goalEnemy.CanShoot)
		{
			PursuitInProgress = false;
			method_0();
		}
		else
		{
			PursuitInProgress = true;
			BotOwner_0.MoveToEnemyData.TryMoveToEnemy(goalEnemy.CurrPosition);
		}
	}

	public bool method_2(NavMeshPath path, float straighDist)
	{
		if (GClass371.CalculatePathLength(path) < straighDist * 1.2f)
		{
			return true;
		}
		return false;
	}
}

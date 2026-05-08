using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class BotRun : GClass429
{
	[CompilerGenerated]
	public class Class189
	{
		public BotRun botRun_0;

		public bool zigZag;

		public void method_0(CustomNavigationPoint navigationPoint)
		{
			botRun_0.BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(navigationPoint);
			botRun_0.method_1(navigationPoint, zigZag);
		}
	}

	[NonSerialized]
	public static NavMeshPath CachePath = new NavMeshPath();

	[NonSerialized]
	public float GoodCoverRechceckTime;

	[NonSerialized]
	public float NextPosibleRunTime;

	[NonSerialized]
	public float TryRunUntil;

	[NonSerialized]
	public bool TryRunUntilVal;

	[NonSerialized]
	public bool Going;

	[NonSerialized]
	public float NextTimeHeckRun;

	[NonSerialized]
	public float NextRecalcMove;

	[NonSerialized]
	public bool CanRunByManyShoots;

	public static bool CalcPathToMoveZigZagAndGo(Vector3 targetPoint1, BotOwner bot, EZigZAgType type)
	{
		if (NavMesh.CalculatePath(bot.Transform.position, targetPoint1, -1, CachePath) && CachePath.status == NavMeshPathStatus.PathComplete)
		{
			smethod_0(CachePath.corners, bot, type);
			return true;
		}
		float maxDistance = 2.6f;
		if (NavMesh.SamplePosition(targetPoint1, out var hit, maxDistance, -1) && NavMesh.CalculatePath(bot.Transform.position, hit.position, -1, CachePath) && CachePath.status == NavMeshPathStatus.PathComplete)
		{
			smethod_0(CachePath.corners, bot, type);
			return true;
		}
		return false;
	}

	public static void smethod_0(Vector3[] corners, BotOwner bot, EZigZAgType type)
	{
		if (bot.Settings.FileSettings.Move.NO_ZIG_ZAG)
		{
			bot.GoToByWay(corners);
			return;
		}
		List<Vector3> modifedWay = null;
		bool flag = ((type == EZigZAgType.allSteps || type != EZigZAgType.percentOffset) ? GClass635.ModifyZigZagAllSteps(corners, 6f, 2.2f, out modifedWay) : GClass635.ModifyZigZagPercentOffset(corners, 6f, 100f, 3f, 0.05f, out modifedWay));
		if (flag && modifedWay != null && modifedWay.Count > 0)
		{
			bot.GoToByWay(modifedWay.ToArray());
		}
		else
		{
			bot.GoToByWay(corners);
		}
	}

	public BotRun(BotOwner owner)
		: base(owner)
	{
		CanRunByManyShoots = owner.Profile.Info.Settings.Role == WildSpawnType.assault;
	}

	public void Run(CoverShootType shootType, ShootPointClass shootPos, float deltaCalcTime, bool notOpeningDoor, bool checkRecalc, bool zigZag)
	{
		method_0(shootType, deltaCalcTime, checkRecalc, zigZag);
		method_3(notOpeningDoor, -1f);
	}

	public bool RunAwayGrenade(float reachDist = -1f)
	{
		return method_3(notOpeningDoor: false, reachDist);
	}

	public bool Run(Vector3 point, bool notOpeningDoor, float reachDist = -1f)
	{
		if (BotOwner_0.BotLay.IsLay)
		{
			BotOwner_0.BotLay.GetUp(withCheck: false);
		}
		if (!method_2(point))
		{
			return false;
		}
		method_3(notOpeningDoor, reachDist);
		return true;
	}

	public void EndMove()
	{
		Going = false;
	}

	public void CheckRunByShoots()
	{
		if (!CanRunByManyShoots || !(NextTimeHeckRun < Time.time))
		{
			return;
		}
		float num = 2f;
		if (BotOwner_0.ShootData.IsManySHootsOnAttack() && BotOwner_0.Memory.GoalEnemy.Distance > LocalBotSettingsProviderClass.Core.MIN_DIST_TO_RUN_WHILE_ATTACK_MOVING)
		{
			float num2 = float.MaxValue;
			bool flag = false;
			foreach (EnemyInfo value in BotOwner_0.EnemiesController.EnemyInfos.Values)
			{
				if (value != BotOwner_0.Memory.GoalEnemy && value.Distance < num2)
				{
					num2 = value.Distance;
					flag = true;
				}
			}
			if (!flag || num2 < LocalBotSettingsProviderClass.Core.MIN_DIST_TO_RUN_WHILE_ATTACK_MOVING_OTHER_ENEMIS)
			{
				Vector3 direction = BotOwner_0.Memory.GoalEnemy.Direction;
				if (Vector3.Dot(BotOwner_0.Mover.NormDirCurPoint, direction) < 0f)
				{
					num = 15f;
					RunFor(5f);
				}
			}
		}
		NextTimeHeckRun = Time.time + num;
	}

	public void RunFor(float sec)
	{
		TryRunUntilVal = true;
		TryRunUntil = Time.time + sec;
	}

	public void StopRunAnyway()
	{
		TryRunUntilVal = false;
	}

	public bool ShallRunAnyway()
	{
		if (BotOwner_0.Settings.FileSettings.Mind.NO_RUN_AWAY_FOR_SAFE)
		{
			return false;
		}
		if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.Distance < LocalBotSettingsProviderClass.Core.MIN_DIST_TO_STOP_RUN)
		{
			return false;
		}
		if (!TryRunUntilVal)
		{
			return false;
		}
		if (TryRunUntil > Time.time)
		{
			return true;
		}
		TryRunUntilVal = false;
		return false;
	}

	public bool IsCoverGoodForRun(CustomNavigationPoint point)
	{
		if (point != null && point.IsFreeById(BotOwner_0.Id) && !point.IsSpotted)
		{
			return true;
		}
		return false;
	}

	public void method_0(CoverShootType shootType, float deltaCalcTime, bool cantChangeCover, bool zigZag)
	{
		if ((!BotOwner_0.Mover.HasPathAndNoComplete || !Going) && BotOwner_0.Memory.BotCurrentCoverInfo.HaveCover)
		{
			Going = true;
			if (BotOwner_0.Memory.CurCustomCoverPoint.IsFreeById(BotOwner_0.Id) && !BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
			{
				method_1(BotOwner_0.Memory.CurCustomCoverPoint, zigZag);
				return;
			}
		}
		else if (NextRecalcMove < Time.time && BotOwner_0.Memory.CurCustomCoverPoint != null)
		{
			NextRecalcMove = Time.time + 4f;
			Going = true;
			if (BotOwner_0.Memory.CurCustomCoverPoint.IsFreeById(BotOwner_0.Id) && !BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
			{
				method_1(BotOwner_0.Memory.CurCustomCoverPoint, zigZag);
				return;
			}
		}
		if (cantChangeCover && BotOwner_0.Memory.BotCurrentCoverInfo.HaveCover && IsCoverGoodForRun(BotOwner_0.Memory.CurCustomCoverPoint))
		{
			if (GoodCoverRechceckTime < Time.time)
			{
				GoodCoverRechceckTime = Time.time + 3f;
				method_1(BotOwner_0.Memory.CurCustomCoverPoint, zigZag);
			}
		}
		else if (NextPosibleRunTime < Time.time || BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint == null)
		{
			NextPosibleRunTime = Time.time + deltaCalcTime;
			CoverSearchType searchType = BotOwner_0.Tactic.SubTactic.SearchRunToCover(shootType);
			if (shootType == CoverShootType.hide)
			{
				searchType = CoverSearchType.distToBotAndToCenter;
			}
			bool canRunNoAmmo;
			Vector3 centerPos = ((!BotOwner_0.Memory.IsInCover || BotOwner_0.Memory.GoalEnemy == null || BotOwner_0.LookSensor.EnoughDistToShoot(out canRunNoAmmo)) ? BotOwner_0.Transform.position : ((BotOwner_0.Position + BotOwner_0.Memory.GoalEnemy.EnemyLastPosition) / 2f));
			ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
			float sTART_DIST_TO_COV = LocalBotSettingsProviderClass.Core.START_DIST_TO_COV;
			BotOwner_0.BotAttackManager.TryPointGetting(centerPos, shootType, sTART_DIST_TO_COV, searchType, shoot2point, delegate(CustomNavigationPoint navigationPoint)
			{
				BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(navigationPoint);
				method_1(navigationPoint, zigZag);
			}, null);
		}
	}

	public void method_1(CustomNavigationPoint target, bool withZIgZag)
	{
		if (!withZIgZag || !CalcPathToMoveZigZagAndGo(target.Position, BotOwner_0, EZigZAgType.allSteps))
		{
			BotOwner_0.GoToPoint(target);
		}
	}

	public bool method_2(Vector3 point)
	{
		if (NextPosibleRunTime < Time.time && BotOwner_0.GoToPoint(point) != NavMeshPathStatus.PathComplete)
		{
			return false;
		}
		return true;
	}

	public bool method_3(bool notOpeningDoor, float reachDist)
	{
		BotOwner_0.SetPose(1f);
		BotOwner_0.SetTargetMoveSpeed(1f);
		if (reachDist < 0f)
		{
			reachDist = BotOwner_0.Settings.FileSettings.Move.REACH_DIST_RUN;
		}
		if (BotOwner_0.Mover.DistDestination < BotOwner_0.Settings.FileSettings.Move.RUN_TO_COVER_MIN)
		{
			BotOwner_0.Sprint(val: false, withDebugCallback: false);
		}
		if (BotOwner_0.DoorOpener.Interacting)
		{
			BotOwner_0.AimingManager.CurrentAiming.LoseTarget();
			if (notOpeningDoor)
			{
				BotOwner_0.Sprint(val: false);
			}
			else
			{
				BotOwner_0.Sprint(val: true);
			}
		}
		else
		{
			BotOwner_0.Sprint(val: true);
		}
		BotOwner_0.Steering.LookToMovingDirection();
		if (BotOwner_0.Mover.IsComeTo(reachDist, onCover: true))
		{
			BotOwner_0.Sprint(val: false, withDebugCallback: false);
			BotOwner_0.Memory.ComeToPoint();
			return true;
		}
		return false;
	}
}

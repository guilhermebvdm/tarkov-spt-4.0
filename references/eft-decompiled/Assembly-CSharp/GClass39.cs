using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass39(BotOwner bot, int priority) : GClass38(bot, priority)
{
	[NonSerialized]
	public BotLogicDecision? Nullable_1;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public bool Bool_5 = true;

	[NonSerialized]
	public Vector3? Nullable_2;

	[NonSerialized]
	public const float Float_5 = 5f;

	[NonSerialized]
	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> Gstruct8_1;

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.GoalEnemy != null;
	}

	public override string Name()
	{
		return "AssaultHaveEnemy";
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (!BotOwner_0.BewareGrenade.ShallRunAway() && BotOwner_0.AssaultDangerArea.HaveTarget && BotOwner_0.AssaultDangerArea.ShallAssault == EBotAssaultAreaStatus.shallGoClose)
		{
			return BotOwner_0.AssaultDangerArea.GetCloseCover();
		}
		if (data.Shoot2Point != null && BaseLogicLayerSimpleAbstractClass.IsPointInsideDangerZone(BotOwner_0, data.Shoot2Point.Point))
		{
			data.shootType = CoverShootType.hide;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		Gclass25_0.Update();
		if (Bool_4)
		{
			if (method_14(out Gstruct8_1))
			{
				return Gstruct8_1;
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "toofar");
		}
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			return aICoreActionResultStruct.Value;
		}
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "kgba54");
		}
		if (Bool_5 && Nullable_2.HasValue && Time.time - BotOwner_0.Memory.ComeToCoverTime > 5f)
		{
			GClass29 paramsData = new GClass29(Nullable_2.Value);
			Nullable_2 = null;
			if (BotOwner_0.Memory.IsInCover)
			{
				BotOwner_0.Memory.Spotted(byHit: false);
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMovingFlank, "kj8y3", paramsData);
		}
		if (Nullable_1.HasValue)
		{
			BotLogicDecision value = Nullable_1.Value;
			Nullable_1 = null;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(value, "_nextLogic");
		}
		if (method_14(out Gstruct8_1))
		{
			return Gstruct8_1;
		}
		if (BotOwner_0.CanSprintPlayer && BotOwner_0.WeaponManager.Stationary.CheckWantTakeStationary(BotOwner_0.Settings.FileSettings.Cover.STATIONARY_WEAPON_MAX_DIST_TO_USE) != null)
		{
			BotLogicDecision? currentDecision = BotOwner_0.WeaponManager.Stationary.GetCurrentDecision();
			if (currentDecision.HasValue)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(currentDecision.Value, "stationaryw");
			}
		}
		if (CanSearchEnemy())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.search, "CanSearchEn");
		}
		if (BotOwner_0.Memory.IsInCover && !goalEnemy.IsVisible)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "InCover&Attack");
		}
		if (BotOwner_0.Memory.ShallRunIfNoAmmo && BotOwner_0.WeaponManager.Reload.BulletCount == 0 && (BotOwner_0.Memory.GoalEnemy == null || BotOwner_0.Memory.GoalEnemy.Distance > LocalBotSettingsProviderClass.Core.MIN_DIST_TO_STOP_RUN) && BotOwner_0.CanSprintPlayer && !BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "ShallRunIfNoAmmo");
		}
		bool flag = false;
		bool flag2 = false;
		if (goalEnemy.IsVisible)
		{
			if (BotOwner_0.BotLay.IsLay)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "enemy.Visib");
			}
			if (BotOwner_0.DogFight.DogFightState != BotDogFightStatus.none)
			{
				switch (BotOwner_0.DogFight.DogFightState)
				{
				case BotDogFightStatus.shootFromPlace:
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "DogFromPlace");
				case BotDogFightStatus.dogFight:
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "DogFight");
				}
			}
			BotOwner_0.BotRun.CheckRunByShoots();
			bool canRunNoAmmo;
			Vector3 centerPos = ((!BotOwner_0.Memory.IsInCover || BotOwner_0.LookSensor.EnoughDistToShoot(out canRunNoAmmo)) ? BotOwner_0.Transform.position : ((BotOwner_0.Position + goalEnemy.EnemyLastPosition) / 2f));
			CoverShootType coverShootType = ((!BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Attack) && !BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Protect)) ? CoverShootType.hide : CoverShootType.shoot);
			CoverSearchType searchType = BotOwner_0.Tactic.SubTactic.SearchTypeAttack(coverShootType);
			ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
			if (Float_4 < Time.time)
			{
				BotOwner_0.BotAttackManager.TryPointGetting(centerPos, coverShootType, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, searchType, shoot2point, delegate(CustomNavigationPoint point)
				{
					BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(point);
				}, null, checkCurrent: false);
				Float_4 = Time.time + 1f;
			}
			if (BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint != null)
			{
				float sqrMagnitude = (BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint.Position - BotOwner_0.Position).sqrMagnitude;
				flag = sqrMagnitude < BotOwner_0.Settings.FileSettings.Cover.RUN_IF_FAR_SQRT && sqrMagnitude > 0.7f;
				flag2 = sqrMagnitude > BotOwner_0.Settings.FileSettings.Cover.STAY_IF_FAR_SQRT;
			}
			if (BotOwner_0.CanSprintPlayer && (flag || (BotOwner_0.Settings.FileSettings.Move.RUN_IF_CANT_SHOOT && BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Ambush) && (BotOwner_0.Memory.GoalEnemy == null || !BotOwner_0.Memory.GoalEnemy.CanShoot))) && BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.Distance > BotOwner_0.Settings.FileSettings.Move.RUN_IF_GAOL_FAR_THEN && BotOwner_0.Memory.LastDamageDataActive)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "runIfCoverF");
			}
			if (flag2 && goalEnemy.CanShoot)
			{
				if (BotOwner_0.WeaponManager.Reload.Reloading)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "ReloaddogFight");
				}
				if (BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Ambush))
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "Ambush");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "shootFromPlace8");
			}
			if (!goalEnemy.CanShoot && !goalEnemy.IsVisible && BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Ambush))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "Ambush 2");
			}
			if (goalEnemy.CanShoot && goalEnemy.Distance < 7f && BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint != null)
			{
				NavMeshPath navMeshPath = new NavMeshPath();
				if (NavMesh.CalculatePath(BotOwner_0.Position, BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint.Position, -1, navMeshPath) && GClass369.IsPointOnWay(navMeshPath.corners, goalEnemy.CurrPosition, 4f, 0))
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "DOG_FIGHT_IN");
				}
			}
			if (BotOwner_0.Memory.IsInCover)
			{
				HoldFor(2f);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "attackHld");
			}
			if (goalEnemy.Distance > 15f && GClass856.IsTrue100(50f))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "runToNtg");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "attackNtg");
		}
		Vector3 position = BotOwner_0.Transform.position;
		CoverShootType coverShootType2 = ((!BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Attack) && !BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Protect)) ? CoverShootType.hide : CoverShootType.shoot);
		CoverSearchType searchType2 = BotOwner_0.Tactic.SubTactic.SearchTypeAttack(coverShootType2);
		ShootPointClass shoot2point2 = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		BotOwner_0.BotAttackManager.TryPointGetting(position, coverShootType2, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, searchType2, shoot2point2, delegate(CustomNavigationPoint point)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(point);
		}, null, checkCurrent: false);
		float num = ((Time.time - goalEnemy.TimeLastSeen < BotOwner_0.Settings.FileSettings.Mind.SEC_TO_MORE_DIST_TO_RUN) ? 1.5f : 1f);
		if (BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint != null)
		{
			float sqrMagnitude2 = (BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint.Position - BotOwner_0.Position).sqrMagnitude;
			flag = sqrMagnitude2 < BotOwner_0.Settings.FileSettings.Cover.STAY_IF_FAR_SQRT * num && sqrMagnitude2 > 0.7f;
		}
		if (BotOwner_0.CanSprintPlayer && flag && BotOwner_0.Memory.LastDamageDataActive)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "LastDamageDataActive");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_15(), "Last");
	}

	public bool method_14(out global::AICoreActionResultStruct<BotLogicDecision, GClass26> aiCoreActionResult)
	{
		if (BotOwner_0.Medecine.FirstAid.Have2Do || BotOwner_0.Medecine.SurgicalKit.HaveWork)
		{
			if (!BotOwner_0.Memory.HaveEnemy)
			{
				aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal2");
				return true;
			}
			if (!BaseLogicLayerSimpleAbstractClass.CheckMedsToStop(BotOwner_0))
			{
				aiCoreActionResult = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal1");
				return true;
			}
		}
		aiCoreActionResult = default(global::AICoreActionResultStruct<BotLogicDecision, GClass26>);
		return false;
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		if (Bool_5 && Time.time - BotOwner_0.Memory.ComeToCoverTime > 5f && Time.time - BotOwner_0.ShootData.LastTriggerPressd < 4f && BotOwner_0.Memory.IsInCover)
		{
			CustomNavigationPoint curCustomCoverPoint = BotOwner_0.Memory.CurCustomCoverPoint;
			Vector3 v = curCustomCoverPoint.FirePosition - curCustomCoverPoint.Position;
			v.y = 0f;
			Vector3 value = BotOwner_0.Position - GClass855.NormalizeFastSelf(v) * 8f;
			if (BotOwner_0.Memory.GoalEnemy != null)
			{
				Vector3 vector = GClass855.NormalizeFastSelf(BotOwner_0.Position - BotOwner_0.Memory.GoalEnemy.CurrPosition);
				vector.y = 0f;
				value += vector * 3f;
			}
			Nullable_2 = value;
			GClass369.DebugDrawArc(curCustomCoverPoint.FirePosition, curCustomCoverPoint.Position, 2.5f, 5f, Color.magenta);
			GClass369.DebugDrawArc(BotOwner_0.Position, Nullable_2.Value, 3f, 5f, Color.magenta);
			BotOwner_0.Memory.Spotted(byHit: false);
			return new AICoreActionEndStruct("ghyFlank");
		}
		return base.EndShootFromCover();
	}

	public override AICoreActionEndStruct EndAttackMovingFlank()
	{
		if (method_3())
		{
			return new AICoreActionEndStruct("96jk");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("jh7");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndSearch()
	{
		if (CanSearchEnemy())
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("CanSearch");
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		return base.EndAttackMoving();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.Medecine.FirstAid.Have2Do || BotOwner_0.Medecine.SurgicalKit.HaveWork)
		{
			if (!BotOwner_0.Memory.HaveEnemy)
			{
				return new AICoreActionEndStruct("FirstAid");
			}
			if (!BaseLogicLayerSimpleAbstractClass.CheckMedsToStop(BotOwner_0))
			{
				return new AICoreActionEndStruct("FirstAid");
			}
		}
		Gclass25_0.Update();
		if (Bool_4)
		{
			return AICoreActionEndStruct_1;
		}
		return base.EndHoldPosition();
	}

	public BotLogicDecision method_15()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return BotLogicDecision.holdPosition;
		}
		return BotLogicDecision.attackMoving;
	}

	public override AICoreActionEndStruct EndHeal()
	{
		if (!BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new AICoreActionEndStruct("EndHeal");
		}
		if (BaseLogicLayerSimpleAbstractClass.CheckMedsToStop(BotOwner_0))
		{
			BotOwner_0.Medecine.FirstAid.CancelCurrent();
			return new AICoreActionEndStruct("CancelHeal");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToStationary()
	{
		if (!BotOwner_0.CanSprintPlayer)
		{
			return new AICoreActionEndStruct("can't run");
		}
		return base.EndRunToStationary();
	}

	[CompilerGenerated]
	public void method_16(CustomNavigationPoint point)
	{
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(point);
	}

	[CompilerGenerated]
	public void method_17(CustomNavigationPoint point)
	{
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(point);
	}
}

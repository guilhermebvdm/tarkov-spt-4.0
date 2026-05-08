using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass87 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public float Float_3 = 20f;

	[NonSerialized]
	public float Float_4 = 50f;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public List<EnemyInfo> List_0 = new List<EnemyInfo>();

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public GClass528 Gclass528_0;

	[NonSerialized]
	public GClass412 Gclass412_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public float Float_6 = 15f;

	public GClass87(BotOwner bot, int priority, float enemyDistanceToStartFight = -1f, float chanceToRun = -1f)
		: base(bot, priority)
	{
		Gclass528_0 = new GClass528(BotOwner_0);
		Gclass412_0 = BotOwner_0.BotsController.Bots.GetConnector();
		if (enemyDistanceToStartFight > 0f)
		{
			Float_3 = enemyDistanceToStartFight;
		}
		if (chanceToRun > 0f)
		{
			Float_4 = chanceToRun;
		}
		Vector3_1 = GClass856.RandomHorizontal(5f, 8f);
	}

	public void method_13()
	{
		float kILLA_AFTER_GRENADE_SUPPRESS_DELAY = BotOwner_0.Settings.FileSettings.Boss.KILLA_AFTER_GRENADE_SUPPRESS_DELAY;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (kILLA_AFTER_GRENADE_SUPPRESS_DELAY > 0f && goalEnemy != null && !goalEnemy.CanShoot)
		{
			Nullable_0 = BotLogicDecision.holdPosition;
			HoldFor(kILLA_AFTER_GRENADE_SUPPRESS_DELAY);
		}
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.CalledData.MayUse;
	}

	public override string Name()
	{
		return "Help";
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			data = method_15(data);
			return base.FindPoint(data, p, checkCurrent);
		}
		if (!Bool_4)
		{
			if (CustomNavigationPoint_0 != null)
			{
				return CustomNavigationPoint_0;
			}
		}
		else
		{
			Bool_4 = false;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		Bool_5 = false;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			if (BotOwner_0.CalledData.Target != null)
			{
				if ((BotOwner_0.CalledData.Target.Value - BotOwner_0.Position).magnitude < 2f)
				{
					HoldFor(4f);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "close wait");
				}
				if (method_17(BotOwner_0.CalledData.Target.Value, BotOwner_0.CalledData.ShouldComeNeartarget(), out var vector))
				{
					Debug.DrawRay(vector, Vector3.up * 100f, Color.green, 3f);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "NoEnemy", new GClass30(vector));
				}
			}
			if (BotOwner_0.CalledData.CallerGroupHaveEnemy && BotOwner_0.CalledData.TryGetPriorityEnemyPosition(out var position, out var idEnv))
			{
				if ((position - BotOwner_0.Position).magnitude < 2f)
				{
					HoldFor(4f);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "close wait");
				}
				if (method_17(position, idEnv > 0, out var vector2))
				{
					Debug.DrawRay(vector2, Vector3.up * 100f, Color.green, 3f);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "NoEnemy", new GClass30(vector2));
				}
			}
			if (BotOwner_0.Memory.IsInCover && !BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(4f), "hf4");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "rf4");
		}
		if (Nullable_0.HasValue)
		{
			BotLogicDecision value = Nullable_0.Value;
			Nullable_0 = null;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(value, "NextLogic");
		}
		int num = 0;
		List_0.Clear();
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			if (Mathf.Abs(enemyInfo.Value.CurrPosition.y - BotOwner_0.Position.y) < BotOwner_0.Settings.FileSettings.Boss.KILLA_Y_DELTA_TO_BE_ENEMY_BOSS && enemyInfo.Value.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_DITANCE_TO_BE_ENEMY_BOSS)
			{
				num++;
				List_0.Add(enemyInfo.Value);
			}
		}
		if (!goalEnemy.IsVisible)
		{
			if (BotOwner_0.SmokeGrenade.ShallShoot())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootToSmoke, "ShootSmoke");
			}
			if (BotOwner_0.SmokeGrenade.IsInSmoke)
			{
				if (BotOwner_0.Memory.IsInCover)
				{
					HoldFor(1f);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "RunToCover");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "RunToCover");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_16(), "GoToEnemy");
		}
		if (goalEnemy.Distance > Float_3)
		{
			BotLogicDecision action = (goalEnemy.IsVisible ? BotLogicDecision.shootFromPlace : method_16());
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(action, "EnemySoFar");
		}
		return method_19();
	}

	public override void OnActivate()
	{
		BotOwner_0.WeaponManager.Grenades.OnGrenadeThrowStart += method_13;
		base.OnActivate();
	}

	public override BotLogicDecision HoldFor(float sec)
	{
		Bool_5 = BotOwner_0.Memory.IsInCover;
		return base.HoldFor(sec);
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("CanShoot");
		}
		return base.EndGoToEnemy();
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (goalEnemy.IsVisible || goalEnemy.CanShoot))
		{
			return new AICoreActionEndStruct("Enemy");
		}
		if (BotOwner_0.GoToSomePointData.IsCome())
		{
			return new AICoreActionEndStruct("Come");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			BotOwner_0.BotRun.EndMove();
			return new AICoreActionEndStruct("IsInCover");
		}
		if (method_3())
		{
			BotOwner_0.BotRun.EndMove();
			return new AICoreActionEndStruct("StartD");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		if (BotOwner_0.Brain.LastDecision.HasValue && BotOwner_0.Brain.LastDecision.Value == BotLogicDecision.runToEnemyZigZag)
		{
			float sqrMagnitude = (Vector3_0 - BotOwner_0.Position).sqrMagnitude;
			Vector3_0 = BotOwner_0.Position;
			if (Gclass528_0.CheckIsBadVal(sqrMagnitude, 0.0001f))
			{
				return new AICoreActionEndStruct("BadVal");
			}
		}
		if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("CanShoot");
		}
		if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false))
		{
			return new AICoreActionEndStruct("isCome");
		}
		return base.EndRunToEnemy();
	}

	public override AICoreActionEndStruct EndDogFight()
	{
		return base.EndDogFight();
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool flag = false;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemynull");
		}
		if (!goalEnemy.IsVisible)
		{
			flag = Time.time - goalEnemy.TimeLastSeen > 4f;
		}
		if (method_3() || BotOwner_0.Memory.IsInCover || flag)
		{
			return new AICoreActionEndStruct("DogFig");
		}
		return AICoreActionEndStruct_1;
	}

	public override bool CanSearchEnemy()
	{
		return false;
	}

	public void method_14()
	{
		if (Float_5 < Time.time)
		{
			Float_5 = 5f + Time.time;
			CoverSearchData data = method_15();
			CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
		}
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			return new AICoreActionEndStruct("haveEnemy");
		}
		if (BotOwner_0.CalledData.Target == null)
		{
			if (CustomNavigationPoint_0 != null && !CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id))
			{
				CustomNavigationPoint_0 = null;
			}
			if (CustomNavigationPoint_0 != null && BotOwner_0.Memory.IsInCover && BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint.Id != CustomNavigationPoint_0.Id)
			{
				BotOwner_0.Memory.Spotted(byHit: false);
				BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
				return new AICoreActionEndStruct("betterCover");
			}
			float num = ((BotOwner_0.Memory.CurCustomCoverPoint == null) ? (BotOwner_0.Position - BotOwner_0.CalledData.CallerPosition).magnitude : (BotOwner_0.Memory.CurCustomCoverPoint.Position - BotOwner_0.CalledData.CallerPosition).magnitude);
			if (num > Float_6)
			{
				method_14();
			}
		}
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
		}
		if (Bool_5 && !BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("!IsInCover");
		}
		return AICoreActionEndStruct_1;
	}

	public CoverSearchData method_15(CoverSearchData data = null)
	{
		ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		Vector3 centerPos = ((BotOwner_0.CalledData.Target == null) ? BotOwner_0.CalledData.CallerPosition : ((!BotOwner_0.CalledData.ShouldComeNeartarget()) ? BotOwner_0.CalledData.Target.Value : (BotOwner_0.CalledData.Target.Value + Vector3_1)));
		int num = 75 * 75;
		return new CoverSearchData(centerPos, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, num, 0f, CoverSearchType.distToToCenter, shoot2point, null, null, data?.CheckShootHide ?? ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(-1f));
	}

	public BotLogicDecision method_16()
	{
		if (!GClass856.IsTrue100(Float_4))
		{
			return BotLogicDecision.goToEnemy;
		}
		return BotLogicDecision.runToEnemy;
	}

	public bool method_17(Vector3 testTarget, bool findPlaceNear, out Vector3 vector3)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		if (findPlaceNear)
		{
			testTarget += GClass856.RandomHorizontal(6f, 8f);
		}
		NavMesh.CalculatePath(BotOwner_0.Position, testTarget, -1, navMeshPath);
		switch (navMeshPath.status)
		{
		default:
			vector3 = Vector3.zero;
			return false;
		case NavMeshPathStatus.PathPartial:
		{
			if (NavMesh.SamplePosition(testTarget, out var hit, 8f, -1))
			{
				NavMesh.CalculatePath(BotOwner_0.Position, hit.position, -1, navMeshPath);
				_ = navMeshPath.status;
				vector3 = navMeshPath.corners[navMeshPath.corners.Length - 1];
				return true;
			}
			Vector3 vector4 = navMeshPath.corners[navMeshPath.corners.Length - 1];
			vector3 = vector4;
			return true;
		}
		case NavMeshPathStatus.PathComplete:
			vector3 = testTarget;
			return true;
		}
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_18()
	{
		GClass413 closest = Gclass412_0.GetClosest(BotOwner_0);
		if (closest != null && closest.Dist < 1f)
		{
			IPlayer another = closest.GetAnother(BotOwner_0);
			if (another.IsAI && another.AIData.BotOwner.Id > BotOwner_0.Id)
			{
				Vector3 vector = GClass855.Rotate90(-GClass855.NormalizeFastSelf(BotOwner_0.Memory.GoalEnemy.Direction), GClass855.SideTurn.left);
				if (NavMesh.SamplePosition(BotOwner_0.Position + vector * 2f, out var hit, 5f, -1))
				{
					Vector3 position = hit.position;
					NavMeshPath path = new NavMeshPath();
					if (NavMesh.CalculatePath(position, BotOwner_0.Position, -1, path))
					{
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "ShootFromPl", new GClass30(position));
					}
				}
			}
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "ShootFromPl");
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_19()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		if (shootPointClass != null)
		{
			shootPointClass.DistCoef = 0.7f;
			CoverSearchData data = new CoverSearchData((BotOwner_0.Position + shootPointClass.Point) / 2f, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, 625f, 0f, CoverSearchType.distToToCenter, shootPointClass, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
			Bool_4 = true;
			float num = Time.time - goalEnemy.TimeLastSeenReal;
			if (!goalEnemy.IsVisible && num > BotOwner_0.Settings.FileSettings.Boss.KILLA_START_SEARCH_SEC)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.search, "AssaultMode");
			}
			CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
		}
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "AssaultMode");
		}
		if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_CLOSE_ATTACK_DIST)
		{
			return method_20();
		}
		if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_MIDDLE_ATTACK_DIST)
		{
			return method_21();
		}
		return method_23();
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_20()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			Int_1++;
			return method_18();
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0, BotLogicDecision.goToEnemy), "CloseDist");
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_21()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return method_18();
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			float num = Time.time - BotOwner_0.Memory.ComeToCoverTime;
			float num2 = BotOwner_0.Settings.FileSettings.Boss.KILLA_HOLD_DELAY - num;
			if (num2 > 0f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(num2), "MidDist");
			}
		}
		if (Bool_2)
		{
			return method_8();
		}
		if (!goalEnemy.IsVisible && Int_1 > BotOwner_0.Settings.FileSettings.Boss.KILLA_CLOSEATTACK_TIMES)
		{
			Int_1 = 0;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(BotOwner_0.Settings.FileSettings.Boss.KILLA_CLOSEATTACK_DELAY), "MidDist");
		}
		if (!goalEnemy.CanShoot && method_9())
		{
			BotOwner_0.WeaponManager.Reload.Reload();
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "MidDist");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_22(), "runMid");
	}

	public BotLogicDecision method_22()
	{
		return BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0, BotLogicDecision.runToEnemyZigZag);
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_23()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return method_18();
		}
		if (CustomNavigationPoint_0 == null)
		{
			if (!goalEnemy.IsVisible)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0), "FarestDist");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "FarestDist");
		}
		if ((CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude < 1f)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0), "FarestDist");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "FarestDist");
	}

	public override void Dispose()
	{
		BotOwner_0.WeaponManager.Grenades.OnGrenadeThrowStart -= method_13;
		base.Dispose();
	}
}

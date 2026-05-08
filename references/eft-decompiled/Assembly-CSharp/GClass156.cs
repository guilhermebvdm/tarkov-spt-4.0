using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass156 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public const float Float_3 = 20f;

	[NonSerialized]
	public const float Float_4 = 7f;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public List<EnemyInfo> List_0 = new List<EnemyInfo>();

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public GClass412 Gclass412_0;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public float Float_9;

	public float Single_0 => 15f;

	public float Single_1 => 30f;

	public BotGlobalsBossSettings BotGlobalsBossSettings_0 => BotOwner_0.Settings.FileSettings.Boss;

	public GClass156(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "KillaAgro";
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.DogFight.ShallStartCauseHavePlace())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "df1");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if ((goalEnemy == null || !goalEnemy.IsVisible) && BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "HealInCover");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "GoCoverHeal");
		}
		if (goalEnemy == null)
		{
			HoldFor(5f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "noEnemy");
		}
		method_15();
		if (method_12(1f))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "getHit1");
		}
		if (goalEnemy.Distance > Single_1)
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "vbhj67");
			}
			if (method_13())
			{
				HoldFor(5f);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "waitEne");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "GoTGC");
		}
		if (goalEnemy.Distance < Single_1)
		{
			if (goalEnemy.Distance < Single_0)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "matk");
			}
			if (CustomNavigationPoint_0 == null)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "m7atk");
			}
			if (BotOwner_0.Memory.IsInCover)
			{
				if (!(Time.time - BotOwner_0.Memory.ComeToCoverTime > 5f))
				{
					HoldFor(5f);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hf65");
				}
				BotOwner_0.Memory.Spotted(byHit: false);
			}
			BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCoverZigZag, "getCls");
		}
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "sfp56");
		}
		if (CustomNavigationPoint_0 != null)
		{
			BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "getCls");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "gt78");
	}

	public bool method_13()
	{
		if (!BotOwner_0.Memory.IsInCover)
		{
			return false;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return true;
		}
		return Vector3.Dot(BotOwner_0.Memory.CurCustomCoverPoint.ToWallVector, goalEnemy.Direction) > 0f;
	}

	public CoverSearchData method_14()
	{
		float num = BotOwner_0.Memory.GoalEnemy.Distance / 3f - 1f;
		if (num < 2f)
		{
			num = 2f;
		}
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		Vector3 centerPos = (BotOwner_0.Position + shootPointClass.Point) / 2f;
		float maxDistSqr = num * num;
		return new CoverSearchData(centerPos, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, maxDistSqr, 0f, CoverSearchType.distToToCenter, shootPointClass, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
	}

	public void method_15()
	{
		if (!(Float_9 < Time.time))
		{
			return;
		}
		CoverSearchData coverSearchData = method_14();
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		Float_9 = 1f + Time.time;
		CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: false);
		if (CustomNavigationPoint_0 == null)
		{
			return;
		}
		float magnitude = (CustomNavigationPoint_0.Position - shootPointClass.Point).magnitude;
		if (!((BotOwner_0.Position - shootPointClass.Point).magnitude - 1f < magnitude))
		{
			return;
		}
		coverSearchData.shootType = CoverShootType.hide;
		CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: false);
		if (CustomNavigationPoint_0 != null)
		{
			magnitude = (CustomNavigationPoint_0.Position - shootPointClass.Point).magnitude;
			if ((BotOwner_0.Position - shootPointClass.Point).magnitude < magnitude)
			{
				CustomNavigationPoint_0 = null;
			}
		}
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			data = new CoverSearchData(data.CenterPos, data.Bot, CoverShootType.hide, data.MaxDistSqr, 0f, CoverSearchType.distToBot, data.Shoot2Point, null, null, data.CheckShootHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
			return base.FindPoint(data, p, checkCurrent);
		}
		if (!Bool_4)
		{
			if (CustomNavigationPoint_0 != null && !CustomNavigationPoint_0.IsSpotted && CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id))
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

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("eexts");
		}
		if (method_12(1f))
		{
			return new AICoreActionEndStruct("get hit");
		}
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		if (BotOwner_0.DogFight.ShallStartCauseHavePlace())
		{
			return new AICoreActionEndStruct("5h54");
		}
		if (method_19())
		{
			return new AICoreActionEndStruct("WantFinishP");
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

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		if (BotOwner_0.DogFight.ShallStartCauseHavePlace())
		{
			return new AICoreActionEndStruct("87oldfs");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		return base.EndGoToCoverPoint();
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		if (BotOwner_0.DogFight.ShallStartCauseHavePlace())
		{
			return new AICoreActionEndStruct("df54");
		}
		if (method_19())
		{
			return new AICoreActionEndStruct("WantFinishP");
		}
		return base.EndGoToEnemy();
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool flag = false;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemyisnull");
		}
		if (!goalEnemy.IsVisible)
		{
			flag = Time.time - goalEnemy.TimeLastSeen > 4f;
		}
		if (method_3() && goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("StartD");
		}
		if (BotOwner_0.Memory.IsInCover || flag)
		{
			return new AICoreActionEndStruct("InCoverdelt");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			Bool_2 = false;
			return new AICoreActionEndStruct("enemyNull");
		}
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			Bool_2 = false;
			return new AICoreActionEndStruct("CanShoot");
		}
		if (Bool_2)
		{
			if (Float_2 < Time.time)
			{
				Bool_2 = false;
				return new AICoreActionEndStruct("EndHoldTime");
			}
		}
		else if (BotOwner_0.Memory.IsInCover && Time.time - BotOwner_0.Memory.ComeToCoverTime > 5f)
		{
			return new AICoreActionEndStruct("holdLong");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("!IsInCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndSuppressGrenade()
	{
		return base.EndSuppressGrenade();
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		return base.EndShootFromPlace();
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		return base.EndShootFromCover();
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		return AICoreActionEndStruct;
	}

	public bool method_16()
	{
		if (List_0.Count > 1)
		{
			foreach (EnemyInfo item in List_0)
			{
				if (!item.IsSuppressed())
				{
					return false;
				}
			}
		}
		return BotOwner_0.Memory.GoalEnemy.IsSuppressed();
	}

	public bool method_17(EnemyInfo enemy)
	{
		if (enemy != null)
		{
			if (!enemy.IsSuppressed())
			{
				return Float_5 + 7f > Time.time;
			}
			return true;
		}
		return false;
	}

	public override AICoreActionEndStruct EndSearch()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return AICoreActionEndStruct_1;
		}
		if (!goalEnemy.IsVisible && !goalEnemy.CanShoot && goalEnemy.CanISearch)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("find enemy");
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_18()
	{
		GClass413 gClass = Gclass412_0?.GetClosest(BotOwner_0);
		if (gClass != null && gClass.Dist < 1f)
		{
			IPlayer another = gClass.GetAnother(BotOwner_0);
			if (another.IsAI && another.AIData.BotOwner.Id > BotOwner_0.Id)
			{
				Vector3 vector = GClass855.Rotate90(-GClass855.NormalizeFastSelf(BotOwner_0.Memory.GoalEnemy.Direction), GClass855.SideTurn.left);
				if (NavMesh.SamplePosition(BotOwner_0.Position + vector * 2f, out var hit, 5f, -1))
				{
					Vector3 position = hit.position;
					NavMeshPath path = new NavMeshPath();
					if (NavMesh.CalculatePath(position, BotOwner_0.Position, -1, path))
					{
						BotOwner_0.GoToSomePointData.SetPoint(position);
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "ShootFromPl");
					}
				}
			}
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "ShootFromPl");
	}

	public bool method_19()
	{
		if (Time.time - BotOwner_0.Memory.LastEnemyTimeSeen > 20f)
		{
			BotOwner_0.Memory.GoalEnemy = null;
			return true;
		}
		return false;
	}

	public bool method_20()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && !goalEnemy.IsVisible && !goalEnemy.CanShoot)
		{
			if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_CLOSE_ATTACK_DIST)
			{
				return false;
			}
			if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_MIDDLE_ATTACK_DIST)
			{
				if (!method_16() && goalEnemy.ShallISuppress())
				{
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}
}

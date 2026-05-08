using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass159 : GClass157
{
	[NonSerialized]
	public const int Int_2 = 2;

	[NonSerialized]
	public const float Float_5 = 20f;

	[NonSerialized]
	public const float Float_6 = 7f;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public List<EnemyInfo> List_1 = new List<EnemyInfo>();

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public GClass412 Gclass412_0;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8 = 20f;

	[NonSerialized]
	public float Float_9 = 28f;

	[NonSerialized]
	public float Float_10 = 10f;

	[NonSerialized]
	public float Float_11;

	[NonSerialized]
	public float Float_12;

	public BotGlobalsBossSettings BotGlobalsBossSettings_0 => BotOwner_0.Settings.FileSettings.Boss;

	public GClass159(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override AICoreActionEndStruct EndHeal()
	{
		AICoreActionEndStruct result = base.EndHeal();
		if (result.Value)
		{
			Float_12 = Time.time + 20f;
		}
		return result;
	}

	public override AICoreActionEndStruct EndRunToCoverZigZag()
	{
		return base.EndRunToCoverZigZag();
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "TagillaAgro";
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.CallForHelp.WantCallForSavages())
		{
			BotOwner_0.CallForHelp.CallForSavages();
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if ((goalEnemy == null || !goalEnemy.IsVisible) && BotOwner_0.Medecine.FirstAid.Have2Do && Float_12 < Time.time)
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
		if (method_13(out var _, resetCache: true) >= 2)
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "ghj57");
			}
			if (BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "sdgj56");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "mtc89");
		}
		method_17();
		if (!(goalEnemy.Distance > Float_9) && method_18())
		{
			if (goalEnemy.Distance < Float_9)
			{
				if (goalEnemy.Distance < Float_8 && method_18())
				{
					BotOwner_0.BotTalk.Say(EPhraseTrigger.OnSwitchToMeleeWeapon, sayImmediately: true);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.oneMeleeAttack, "matk");
				}
				if (CustomNavigationPoint_0 != null)
				{
					if (CustomNavigationPoint_0 != null && BotOwner_0.Memory.CurCustomCoverPoint != CustomNavigationPoint_0)
					{
						BotOwner_0.Memory.SetCoverPoints(CustomNavigationPoint_0);
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCoverZigZag, "getCls");
					}
					if (goalEnemy.Distance < Float_8 && method_18())
					{
						BotOwner_0.BotTalk.Say(EPhraseTrigger.OnSwitchToMeleeWeapon, sayImmediately: true);
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.oneMeleeAttack, "m7atk");
					}
					HoldFor(5f);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "wait5");
				}
				if (method_15() && !BotOwner_0.WeaponManager.Reload.Reloading)
				{
					if (goalEnemy.IsVisible && goalEnemy.CanShoot)
					{
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "dgh8");
					}
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "gte5");
				}
				if (goalEnemy.Distance < Float_8 && method_18())
				{
					BotOwner_0.BotTalk.Say(EPhraseTrigger.OnSwitchToMeleeWeapon, sayImmediately: true);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.oneMeleeAttack, "m7atk");
				}
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
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "vbhj67");
		}
		if (BotOwner_0.BotsController.EventsController.BotsMinotaurLabirint.IsHelpersSeen(goalEnemy.Person.Id))
		{
			BotOwner_0.BotTalk.Say(EPhraseTrigger.OnSwitchToMeleeWeapon, sayImmediately: true);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "helpSn");
		}
		if (method_14(BotOwner_0.Memory.CurCustomCoverPoint))
		{
			HoldFor(5f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "waitEne");
		}
		if (method_14(CustomNavigationPoint_0))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "GoTGC", new GClass31(CustomNavigationPoint_0));
		}
		HoldFor(4f);
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "anyTen");
	}

	public bool method_14(CustomNavigationPoint point)
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
		if (point == null)
		{
			return false;
		}
		return Vector3.Dot(point.ToWallVector, goalEnemy.Direction) > 0f;
	}

	public bool method_15()
	{
		return false;
	}

	public CoverSearchData method_16()
	{
		float num = BotOwner_0.Memory.GoalEnemy.Distance / 3f - 1f;
		if (num < 2f)
		{
			num = 2f;
		}
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		Vector3 centerPos = (BotOwner_0.Position + shootPointClass.Point) / 2f;
		float maxDistSqr = num * num;
		return new CoverSearchData(centerPos, BotOwner_0.CoverSearchInfo, CoverShootType.hide, maxDistSqr, 0f, CoverSearchType.distToToCenter, shootPointClass, null, null, ECheckSHootHide.hide, new CoverSearchDefenceDataClass(-1f));
	}

	public void method_17()
	{
		if (Float_11 < Time.time)
		{
			if (CustomNavigationPoint_0 != null)
			{
				CustomNavigationPoint_0.Spotted(10f);
			}
			CoverSearchData data = method_16();
			Float_11 = 1f + Time.time;
			CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
		}
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			data = new CoverSearchData(data.CenterPos, data.Bot, CoverShootType.hide, data.MaxDistSqr, 0f, CoverSearchType.distToBot, data.Shoot2Point, null, null, data.CheckShootHide, new CoverSearchDefenceDataClass(-1f));
			return base.FindPoint(data, p, checkCurrent);
		}
		if (!Bool_5)
		{
			if (CustomNavigationPoint_0 != null && !CustomNavigationPoint_0.IsSpotted && CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id))
			{
				return CustomNavigationPoint_0;
			}
		}
		else
		{
			Bool_5 = false;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.GoalEnemy.Distance < Float_8 && method_18())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		if (method_18())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
		}
		if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false))
		{
			return new AICoreActionEndStruct("isCome");
		}
		return base.EndRunToEnemy();
	}

	public override AICoreActionEndStruct EndDogFight()
	{
		if (method_18())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("nEnm");
		}
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			return new AICoreActionEndStruct("rld23");
		}
		if (!goalEnemy.IsVisible && Time.time - goalEnemy.PersonalLastSeenTime > 4f)
		{
			return new AICoreActionEndStruct("dfpste5");
		}
		return AICoreActionEndStruct_1;
	}

	public bool method_18()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.Distance > Float_9)
		{
			return false;
		}
		if (!(Time.time - goalEnemy.PersonalLastSeenTime < Float_10) && !BotOwner_0.BotsGroup.BotGame.BotsController.EventsController.BotsMinotaurLabirint.IsHelpersSeen(goalEnemy.Person.Id, Float_10))
		{
			return false;
		}
		return Gclass459_0.WantMeleeAssault();
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		return base.EndGoToCoverPoint();
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		if (method_18())
		{
			return new AICoreActionEndStruct("WMA5");
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
		if (method_18())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
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
		if (goalEnemy.Distance < Float_8 && method_18())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
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
			return AICoreActionEndStruct_1;
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
		if (method_18())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
		}
		return base.EndShootFromPlace();
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		if (method_18())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
		}
		return base.EndShootFromCover();
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		if (method_13(out var _, resetCache: true) >= 2)
		{
			return new AICoreActionEndStruct("Ene>");
		}
		return Gclass459_0.EndOneMeleeAttack();
	}

	public bool method_19()
	{
		if (List_1.Count > 1)
		{
			foreach (EnemyInfo item in List_1)
			{
				if (!item.IsSuppressed())
				{
					return false;
				}
			}
		}
		return BotOwner_0.Memory.GoalEnemy.IsSuppressed();
	}

	public bool method_20()
	{
		if (BotOwner_0.Brain.LastDecision != BotLogicDecision.oneMeleeAttack && (BotOwner_0.Memory.GoalEnemy.Distance > BotOwner_0.Settings.FileSettings.Boss.TAGILLA_SECOND_ASSAULT_RADIUS || method_21(BotOwner_0.Memory.GoalEnemy) || !BotOwner_0.Memory.GoalEnemy.CanShoot || !BotOwner_0.Memory.IsInCover) && BotOwner_0.Memory.LastEnemyTimeSeen + 10f > Time.time)
		{
			return !BotOwner_0.Memory.GoalEnemy.CanShoot;
		}
		return false;
	}

	public bool method_21(EnemyInfo enemy)
	{
		if (enemy != null)
		{
			if (!enemy.IsSuppressed())
			{
				return Float_7 + 7f > Time.time;
			}
			return true;
		}
		return false;
	}

	public bool method_22()
	{
		if (BotOwner_0.Memory.GoalEnemy != null && !(BotOwner_0.Memory.GoalEnemy.Owner == null) && BotOwner_0.Memory.CurCustomCoverPoint != null)
		{
			return !BotOwner_0.Memory.CurCustomCoverPoint.CanIHide(BotOwner_0.Covers.CarePositions(), 1.5f * GClass856.SqrDistance(BotOwner_0.Position, BotOwner_0.Memory.GoalEnemy.CurrPosition), useRaycast: false);
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

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_23()
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

	public bool method_24()
	{
		if (Time.time - BotOwner_0.Memory.LastEnemyTimeSeen > 20f)
		{
			BotOwner_0.Memory.GoalEnemy = null;
			return true;
		}
		return false;
	}

	public bool method_25()
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
				if (!method_19() && goalEnemy.ShallISuppress())
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

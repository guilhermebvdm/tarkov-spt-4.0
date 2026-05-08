using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;
using UnityEngine.AI;

public class GClass80 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public const float Float_3 = -1000f;

	[NonSerialized]
	public const float Float_4 = -1000f;

	[NonSerialized]
	public const float Float_5 = 32f;

	[NonSerialized]
	public const float Float_6 = 10f;

	[NonSerialized]
	public const float Float_7 = 2500f;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public const float Float_9 = 8f;

	[NonSerialized]
	public const int Int_1 = 20;

	[NonSerialized]
	public const int Int_2 = 6;

	[NonSerialized]
	public const int Int_3 = 15;

	[NonSerialized]
	public const int Int_4 = 8;

	[NonSerialized]
	public const float Float_10 = 1600f;

	[NonSerialized]
	public const float Float_11 = 256f;

	[NonSerialized]
	public const float Float_12 = 1600f;

	[NonSerialized]
	public const float Float_13 = 144f;

	[NonSerialized]
	public const float Float_14 = 100f;

	[NonSerialized]
	public const float Float_15 = 8f;

	[NonSerialized]
	public const float Float_16 = 3f;

	[NonSerialized]
	public const int Int_5 = 8;

	[NonSerialized]
	public const int Int_6 = 3;

	[NonSerialized]
	public const int Int_7 = 30;

	[NonSerialized]
	public const int Int_8 = 7;

	[NonSerialized]
	public const int Int_9 = 8;

	[NonSerialized]
	public const int Int_10 = 4;

	[NonSerialized]
	public const float Float_17 = 0.52f;

	[NonSerialized]
	public const float Float_18 = 15f;

	[NonSerialized]
	public const float Float_19 = 2f;

	[NonSerialized]
	public const float Float_20 = 3f;

	[NonSerialized]
	public const float Float_21 = 5f;

	[NonSerialized]
	public const float Float_22 = 2f;

	[NonSerialized]
	public const int Int_11 = 20;

	[NonSerialized]
	public const float Float_23 = 0.5f;

	[NonSerialized]
	public const float Float_24 = 5f;

	[NonSerialized]
	public const float Float_25 = 3f;

	[NonSerialized]
	public const float Float_26 = 22f;

	[NonSerialized]
	public const float Float_27 = 3f;

	[NonSerialized]
	public const float Float_28 = 10f;

	[NonSerialized]
	public const float Float_29 = 4f;

	[NonSerialized]
	public const int Int_12 = 2;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_30;

	[NonSerialized]
	public float Float_31;

	[NonSerialized]
	public float Float_32 = float.MaxValue;

	[NonSerialized]
	public float Float_33 = -120f;

	[NonSerialized]
	public float Float_34;

	public bool DEBUG_ALWAYS_BAD_HEALTH;

	public bool DEBUG_ALWAYS_NOASK_HEAL = true;

	[NonSerialized]
	public float Float_35 = 100f;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public GClass440 Gclass440_0;

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public bool Bool_7;

	[NonSerialized]
	public float Float_36;

	[NonSerialized]
	public float Float_37;

	[NonSerialized]
	public int Int_13 = 10;

	[NonSerialized]
	public float Float_38;

	[NonSerialized]
	public float Float_39;

	[NonSerialized]
	public bool Bool_8;

	[NonSerialized]
	public bool Bool_9;

	[NonSerialized]
	public bool Bool_10;

	[NonSerialized]
	public bool Bool_11;

	[NonSerialized]
	public float Float_40;

	[NonSerialized]
	public float Float_41;

	[NonSerialized]
	public float Float_42 = 1.5f;

	[NonSerialized]
	public float Float_43;

	[NonSerialized]
	public float Float_44 = -1000f;

	[NonSerialized]
	public float Float_45;

	[NonSerialized]
	public float Float_46;

	[NonSerialized]
	public float Float_47;

	[NonSerialized]
	public float Float_48;

	[NonSerialized]
	public bool Bool_12;

	[NonSerialized]
	public float Float_49;

	[NonSerialized]
	public int Int_14;

	[NonSerialized]
	public float Float_50;

	[NonSerialized]
	public int Int_15;

	[NonSerialized]
	public float Float_51 = -100f;

	[NonSerialized]
	public int Int_16;

	[NonSerialized]
	public HashSet<string> HashSet_0 = new HashSet<string> { "ZoneSanatorium1", "ZoneSanatorium2" };

	[NonSerialized]
	public bool Bool_13;

	[NonSerialized]
	public float Float_52;

	[NonSerialized]
	public bool Bool_14;

	[NonSerialized]
	public float Float_53;

	[NonSerialized]
	public const float Float_54 = -49f;

	public bool Boolean_0 => BotOwner_0.Settings.FileSettings.Boss.SANITAR_TWO_COVER_TACTIC;

	public GClass80(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override string Name()
	{
		return "KnightFight";
	}

	public override void OnActivate()
	{
		if (BotOwner_0.Boss.BossLogic is GClass440 bossData)
		{
			SetBossData(bossData);
		}
		BotOwner_0.HealAnotherTarget.OnHealAsked += method_16;
		BotOwner_0.GetPlayer.ActiveHealthController.DoPainKiller();
		Bool_7 = HashSet_0.Contains(BotOwner_0.BotsGroup.BotZone.NameZone);
		Float_48 = (Bool_7 ? 3f : 8f);
		BotOwner_0.Medecine.FirstAid.OnStartApply += method_15;
		BotOwner_0.Medecine.FirstAid.OnEndApply += method_27;
		BotOwner_0.GetPlayer.BeingHitAction += method_26;
	}

	public override bool ShallUseNow()
	{
		int num;
		if (Gclass440_0 != null)
		{
			bool haveEnemy = BotOwner_0.Memory.HaveEnemy;
			if (haveEnemy)
			{
				Float_51 = Time.time;
			}
			if (haveEnemy)
			{
				num = 1;
				goto IL_0044;
			}
		}
		num = ((Float_51 + 8f > Time.time) ? 1 : 0);
		if (num != 0)
		{
			goto IL_0044;
		}
		goto IL_0074;
		IL_0074:
		return (byte)num != 0;
		IL_0044:
		if (!Bool_14)
		{
			Bool_14 = true;
			BotOwner_0.Brain.BaseBrain.OnLayerChangedTo += OnLayerChanged;
		}
		goto IL_0074;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (method_24() && BotOwner_0.Brain.LastDecision.HasValue && BotOwner_0.Brain.LastDecision != BotLogicDecision.attackMoving)
		{
			if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.IsVisible)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "enemyNear");
			}
			CustomNavigationPoint_0 = FindPointForFight(checkCurrent: false);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "enemyNear");
		}
		Bool_8 = false;
		Bool_10 = false;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (Float_44 + 10f < Time.time && BotOwner_0.Memory.GoalEnemy != null && !BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			if ((goalEnemy == null || (!goalEnemy.IsVisible && BotOwner_0.Brain.LastDecision != BotLogicDecision.heal)) && BotOwner_0.Medecine.FirstAid.Have2Do && BotOwner_0.Memory.IsInCover && BotOwner_0.Memory.LastEnemyTimeSeen + 6f < Time.time)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "HealInCover");
			}
			return method_23("runToEnemy");
		}
		int num = ((BotOwner_0.BotsGroup.MembersCount > 1) ? 1 : 2);
		if (Int_16 >= num)
		{
			Int_16 = 0;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(10f), "bad covers");
		}
		Float_53 = Time.time;
		if (method_14())
		{
			if (method_17())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "loseTarget");
			}
			if (BotOwner_0.WeaponManager.Grenades.HaveGrenadeOfType(ThrowWeapType.smoke_grenade))
			{
				AIGreanageThrowData aIGreanageThrowData = new AIGreanageThrowData();
				aIGreanageThrowData.Direction = BotOwner_0.LookDirection;
				aIGreanageThrowData.Ang = 30f;
				aIGreanageThrowData.Force = 6f;
				aIGreanageThrowData.GrenadeType = ThrowWeapType.smoke_grenade;
				BotOwner_0.WeaponManager.Grenades.SetThrowData(aIGreanageThrowData);
				BotOwner_0.WeaponManager.Grenades.DoThrow();
				Float_30 = Time.time;
				Float_33 = Time.time;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "suppress1");
			}
		}
		if (method_22())
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				BotMemoryClass memory = BotOwner_0.Memory;
				float? secToBeSpotted = 32f;
				memory.Spotted(byHit: false, null, secToBeSpotted);
				Int_16++;
			}
			CustomNavigationPoint customNavigationPoint_ = FindPointForAssault(checkCurrent: false);
			CustomNavigationPoint_0 = customNavigationPoint_;
			if (GClass856.SqrDistance(CustomNavigationPoint_0.Position, BotOwner_0.Memory.GoalEnemy.CurrPosition) < 100f)
			{
				if (method_13() && BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.IsVisible)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "sfps1");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "assault1");
			}
			Float_8 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "assault2");
		}
		if (Bool_5)
		{
			Bool_5 = false;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "Lay");
		}
		if (Boolean_0 && !Gclass440_0.EnoughtHaveGoodCovers && BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "use2covers");
		}
		string cause;
		bool bool_ = method_6(out cause) && method_30();
		Bool_11 = bool_;
		if (Bool_11)
		{
			if (BotOwner_0.Medecine.Using)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.healStimulators, "lastCheckCa");
			}
			Float_31 = GClass856.SqrDistance(BotOwner_0.Position, BotOwner_0.Memory.GoalEnemy.CurrPosition);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, "standart");
		}
		float num2 = Time.time - Float_45;
		Float_45 = Time.time;
		EnemyInfo goalEnemy2 = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy2 == null)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "loseEnemy");
		}
		if (num2 > 1f)
		{
			method_39(anyway: true);
			if (Bool_5)
			{
				Bool_5 = false;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "Lay2");
			}
		}
		if (goalEnemy2.CanShoot && goalEnemy2.IsVisible)
		{
			if (Boolean_0 && !Gclass440_0.EnoughtHaveGoodCovers && Time.time - goalEnemy2.LastChangeVisionTime < 3f)
			{
				Float_37 = 3f;
				Float_40 = Time.time;
				Gclass440_0.SetFightPosition(isGood: true, BotOwner_0);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "SetFightPos");
			}
			if (Time.time - Float_52 < 5f)
			{
				Float_37 = 3f;
				Float_40 = Time.time;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "isUnderHeal");
			}
			if (Time.time - goalEnemy2.FirstTimeSeen < 2f)
			{
				Float_37 = 3f;
				Float_40 = Time.time;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "FIRSTTIMESH");
			}
			if (Time.time - BotOwner_0.Memory.LeaveCoverTime < 3f && Time.time - BotOwner_0.Medecine.FirstAid.LastEndTime < 5f && goalEnemy2.Distance < 20f)
			{
				Float_40 = Time.time;
				Float_37 = 8f;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "shootFromPl");
			}
		}
		float num3 = Time.time - Float_38;
		if (Bool_10 && !BotOwner_0.Boss.IamBoss && method_33(out var posToGo))
		{
			Float_46 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "TryGoBoss", new GClass30(posToGo));
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if (Bool_9 && BotOwner_0.Memory.GoalEnemy != null && !BotOwner_0.Memory.GoalEnemy.CanShoot)
			{
				Bool_9 = false;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "shallUseFir");
			}
			if (Bool_8)
			{
				Bool_8 = false;
				Gclass440_0.SomebodyUseStimulator();
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.healStimulators, "shallUseSti");
			}
			if (Bool_7 && !BotOwner_0.Boss.IamBoss && method_25())
			{
				Vector3 vector = method_20();
				if ((BotOwner_0.Memory.CurCustomCoverPoint.Position - vector).sqrMagnitude > Float_35)
				{
					if ((CustomNavigationPoint_0 == null || (CustomNavigationPoint_0.Position - vector).sqrMagnitude > Float_35) && Float_34 + 2.5f < Time.time)
					{
						method_39();
						Float_34 = Time.time;
						HoldFor(1f);
						Float_36 = Time.time + 3f;
						if (Bool_5)
						{
							Bool_5 = false;
							return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "Lay3");
						}
						Float_31 = GClass856.SqrDistance(BotOwner_0.Position, BotOwner_0.Memory.GoalEnemy.CurrPosition);
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, "nextPosible");
					}
					Float_8 = Time.time;
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "nextPosible");
				}
				Float_31 = GClass856.SqrDistance(BotOwner_0.Position, BotOwner_0.Memory.GoalEnemy.CurrPosition);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, "sDistCloseB");
			}
			if (method_41(BotOwner_0.Memory.CurCustomCoverPoint))
			{
				if (num3 > 3f && BotOwner_0.Medecine.FirstAid.Damaged && BotOwner_0.Medecine.FirstAid.HaveSmth2Use)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "CheckGoodFo");
				}
			}
			else
			{
				if (method_29(Float_42))
				{
					return method_37();
				}
				if ((!method_21() || !method_22()) && !method_24() && BotOwner_0.Memory.IsInCover)
				{
					Float_31 = GClass856.SqrDistance(BotOwner_0.Position, BotOwner_0.Memory.GoalEnemy.CurrPosition);
					FindPointForFight(checkCurrent: false);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, "WannaKill");
				}
			}
		}
		else
		{
			if (method_35())
			{
				BotMemoryClass memory2 = BotOwner_0.Memory;
				float? secToBeSpotted = 32f;
				memory2.Spotted(byHit: true, null, secToBeSpotted);
				FindPointForFight(checkCurrent: false);
				if (method_13() && BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.IsVisible)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "sfps2");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "IsDamaged");
			}
			if (method_29(Float_42))
			{
				return method_37();
			}
		}
		if (goalEnemy2 != null && (goalEnemy2.CanShoot || Float_34 + 2.5f >= Time.time))
		{
			if (Boolean_0 && !Gclass440_0.EnoughtHaveGoodCovers)
			{
				Float_8 = Time.time;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "EnoughtHave");
			}
			float num4 = 5f;
			BotOwner_0.BotLay.DelayPosibleLayFor(num4);
			Float_40 = Time.time;
			Float_37 = num4;
			if (BotOwner_0.Memory.IsInCover)
			{
				BotMemoryClass memory3 = BotOwner_0.Memory;
				float? secToBeSpotted = 32f;
				memory3.Spotted(byHit: false, null, secToBeSpotted);
				Int_16++;
			}
			FindPointForFight(checkCurrent: false);
			Vector3 currPosition = BotOwner_0.Memory.GoalEnemy.CurrPosition;
			if (GClass856.SqrDistance(currPosition, CustomNavigationPoint_0.Position) > GClass856.SqrDistance(currPosition, BotOwner_0.Position) - -49f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "am");
			}
			if (BotOwner_0.BotsGroup.MembersCount > 1 && method_17(includeSelf: false))
			{
				return method_23("assaultGroup");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "periodWait");
		}
		Float_34 = Time.time;
		HoldFor(1f);
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "HoldFor");
	}

	public bool method_13()
	{
		if (BotOwner_0.Brain.LastDecision.HasValue)
		{
			if (BotOwner_0.Brain.LastDecision != BotLogicDecision.goToEnemy)
			{
				return BotOwner_0.Brain.LastDecision == BotLogicDecision.runToEnemy;
			}
			return true;
		}
		return false;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (method_24())
		{
			return new AICoreActionEndStruct("enemy near");
		}
		bool flag = BotOwner_0.Memory.IsInCover || method_3();
		if (Float_8 + 7f < Time.time)
		{
			return new AICoreActionEndStruct("outOfTime");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			method_39();
		}
		if (method_14())
		{
			return new AICoreActionEndStruct("ShootToSmoke");
		}
		if (flag)
		{
			BotOwner_0.BotRun.EndMove();
			return new AICoreActionEndStruct("shallEnd");
		}
		if (method_14() && !method_17())
		{
			return new AICoreActionEndStruct("wantSmoke");
		}
		return AICoreActionEndStruct_1;
	}

	public void SetBossData(GClass440 bossData)
	{
		Gclass440_0 = bossData;
	}

	public bool method_14()
	{
		if (!BotOwner_0.Memory.HaveEnemy && BotOwner_0.Memory.LastEnemy != null)
		{
			if (!BotOwner_0.Memory.HaveEnemy && Float_51 + 22f > Time.time && Float_33 + 120f < Time.time && Float_44 + 20f < Time.time)
			{
				return Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(BotOwner_0.Memory.LastEnemy.Person.ProfileId).ActiveHealthController.IsAlive;
			}
			return false;
		}
		return false;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null && BotOwner_0.Memory.GoalEnemy != null)
		{
			float num = GClass856.SqrDistance(BotOwner_0.Position, CustomNavigationPoint_0.Position);
			float num2 = GClass856.SqrDistance(BotOwner_0.Memory.GoalEnemy.CurrPosition, CustomNavigationPoint_0.Position);
			if (num > num2 * 0.8f)
			{
				CustomNavigationPoint_0 = null;
			}
		}
		if (CustomNavigationPoint_0 != null)
		{
			return CustomNavigationPoint_0;
		}
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			data.CenterPos = BotOwner_0.Position + BotOwner_0.Memory.GoalEnemy.CurrPosition * 0.5f;
		}
		else
		{
			data.CenterPos = BotOwner_0.Position;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public void OnLayerChanged(global::AICoreLayerClass<BotLogicDecision> layer)
	{
		Int_16 = 0;
		if (layer == this)
		{
			Float_44 = Time.time;
		}
		else
		{
			Float_44 = -1000f;
		}
	}

	public void method_15(BotOwner obj)
	{
		Bool_13 = true;
	}

	public void method_16(IPlayer obj)
	{
		Bool_4 = true;
	}

	public bool method_17(bool includeSelf = true)
	{
		bool flag = false;
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			BotOwner botOwner = BotOwner_0.BotsGroup.Member(i);
			if (includeSelf || !(botOwner == BotOwner_0))
			{
				flag = flag || botOwner.Memory.HaveEnemy;
			}
		}
		return flag;
	}

	public void ForceRecalcShootPos()
	{
		CustomNavigationPoint customNavigationPoint = FindPointForFight(checkCurrent: false);
		bool flag = method_41(customNavigationPoint);
		Gclass440_0.SetFightPosition(flag, BotOwner_0);
		if (flag)
		{
			CustomNavigationPoint_0 = customNavigationPoint;
		}
	}

	public CustomNavigationPoint FindPointForStay()
	{
		ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		Vector3 currPosition = BotOwner_0.Memory.GoalEnemy.CurrPosition;
		int value = (Bool_7 ? 6 : 20);
		CoverSearchData coverSearchData = new CoverSearchData(currPosition, BotOwner_0.CoverSearchInfo, CoverShootType.hide, 4f, 0f, CoverSearchType.distToToCenter, shoot2point, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f), PointsArrayType.byShootType, useSelfFindPoint: true, null, value);
		coverSearchData.SearchType = CoverSearchType.distToToCenter;
		coverSearchData.CenterPos = currPosition;
		coverSearchData.UseSelfFindPoint = false;
		coverSearchData.ArrayType = PointsArrayType.both;
		coverSearchData.PointToBeClose = null;
		coverSearchData.shootType = CoverShootType.hide;
		coverSearchData.MinSDistToCarePos = 100f;
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: true);
		if (coverPointMain != null)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(coverPointMain);
		}
		Bool_6 = false;
		return coverPointMain;
	}

	public CustomNavigationPoint FindPointForHeal()
	{
		ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		int value = (Bool_7 ? 8 : 15);
		PointsArrayType arrayType = (method_18() ? PointsArrayType.covers : PointsArrayType.both);
		CoverSearchData coverSearchData = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.hide, Bool_7 ? 256f : 1600f, 0f, CoverSearchType.distToBot, shoot2point, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f), PointsArrayType.both, useSelfFindPoint: true, null, value);
		coverSearchData.SearchType = CoverSearchType.distToBot;
		coverSearchData.CenterPos = BotOwner_0.Position;
		coverSearchData.UseSelfFindPoint = false;
		coverSearchData.ArrayType = arrayType;
		coverSearchData.UseLineCastToCover = true;
		coverSearchData.PointToBeClose = null;
		coverSearchData.shootType = CoverShootType.hide;
		coverSearchData.MinSDistToCarePos = (Bool_7 ? 100f : 144f);
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: true);
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(coverPointMain);
		Bool_6 = false;
		return coverPointMain;
	}

	public CustomNavigationPoint FindPointForFight()
	{
		return FindPointForFight(checkCurrent: false);
	}

	public CustomNavigationPoint FindPointForFight(bool checkCurrent)
	{
		if (CustomNavigationPoint_0 == null)
		{
			checkCurrent = false;
		}
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		CoverShootType shootType = CoverShootType.shoot;
		if (shootPointClass == null)
		{
			shootType = CoverShootType.hide;
		}
		PointsArrayType arrayType = (method_18() ? PointsArrayType.covers : PointsArrayType.both);
		float maxDistSqr = (Bool_7 ? 256f : 1600f);
		int value = (Bool_7 ? 6 : 20);
		CoverSearchData coverSearchData = new CoverSearchData(BotOwner_0.Position + (BotOwner_0.Memory.GoalEnemy.CurrPosition - BotOwner_0.Position) * 0.35f, BotOwner_0.CoverSearchInfo, shootType, maxDistSqr, 0f, CoverSearchType.closerToSelectedPoint, shootPointClass, null, BotOwner_0.Position, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f), PointsArrayType.both, useSelfFindPoint: false, null, value);
		coverSearchData.UseSelfFindPoint = checkCurrent;
		coverSearchData.ArrayType = arrayType;
		coverSearchData.UseLineCastToCover = true;
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent);
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(coverPointMain);
		Bool_6 = true;
		CustomNavigationPoint_0 = coverPointMain;
		return coverPointMain;
	}

	public CustomNavigationPoint FindPointForAssault(bool checkCurrent)
	{
		if (CustomNavigationPoint_0 == null)
		{
			checkCurrent = false;
		}
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		CoverShootType shootType = CoverShootType.shoot;
		if (shootPointClass == null)
		{
			shootType = CoverShootType.hide;
		}
		PointsArrayType arrayType = (method_18() ? PointsArrayType.covers : PointsArrayType.both);
		float maxDistSqr = (Bool_7 ? 256f : 1600f);
		int value = (Bool_7 ? 6 : 20);
		CoverSearchData coverSearchData = new CoverSearchData((BotOwner_0.Position + BotOwner_0.Memory.GoalEnemy.CurrPosition) * 0.5f, BotOwner_0.CoverSearchInfo, shootType, maxDistSqr, 0f, CoverSearchType.distToToCenter, shootPointClass, null, BotOwner_0.Position, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f), PointsArrayType.covers, useSelfFindPoint: false, null, value);
		coverSearchData.UseSelfFindPoint = checkCurrent;
		coverSearchData.ArrayType = arrayType;
		coverSearchData.UseLineCastToCover = true;
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent);
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(coverPointMain);
		CustomNavigationPoint_0 = coverPointMain;
		Bool_6 = true;
		return coverPointMain;
	}

	public virtual HashSet<Vector3> CarePositions()
	{
		List<BotSettingsClass> list = new List<BotSettingsClass>();
		foreach (BotSettingsClass value in BotOwner_0.BotsGroup.Enemies.Values)
		{
			if (value.IsHaveSeen && Time.time - value.EnemyLastSeenTimeReal < LocalBotSettingsProviderClass.Core.CARE_ENEMY_ONLY_TIME)
			{
				list.Add(value);
			}
		}
		bool flag;
		int num = ((flag = BotOwner_0.BewareGrenade.GrenadeDangerPoint != null && BotOwner_0.BewareGrenade.GrenadeDangerPoint.Grenade != null) ? 1 : 0);
		Vector3[] array;
		if (BotOwner_0.Memory.GoalTarget.HavePlaceTarget() && BotOwner_0.Memory.GoalTarget.IsDanger)
		{
			array = new Vector3[list.Count + 1 + num];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = list[i].EnemyLastPosition;
			}
			array[list.Count] = BotOwner_0.Memory.GoalTarget.GoalTarget.BasePoint;
		}
		else
		{
			array = new Vector3[list.Count + num];
			for (int j = 0; j < list.Count; j++)
			{
				array[j] = list[j].EnemyLastPosition;
			}
		}
		if (flag)
		{
			array[^1] = BotOwner_0.BewareGrenade.GrenadeDangerPoint.DangerPoint;
		}
		Vector3 position = BotOwner_0.Transform.position;
		HashSet<Vector3> hashSet = new HashSet<Vector3>();
		foreach (Vector3 vector in array)
		{
			if (!((vector - position).sqrMagnitude > LocalBotSettingsProviderClass.Core.MAX_DANGER_CARE_DIST_SQRT))
			{
				hashSet.Add(vector);
			}
		}
		return hashSet;
	}

	public bool method_18()
	{
		return BotOwner_0.Settings.FileSettings.Boss.SANITAR_ONLY_FIGHT_COVERS;
	}

	public CustomNavigationPoint FindPointNearBoss()
	{
		Vector3 vector = method_20();
		CoverSearchData coverSearchData = new CoverSearchData(vector, BotOwner_0.CoverSearchInfo, CoverShootType.hide, 4f, 0f, CoverSearchType.distToToCenter, null, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f), PointsArrayType.both);
		coverSearchData.SearchType = CoverSearchType.distToToCenter;
		coverSearchData.UseSelfFindPoint = false;
		coverSearchData.ArrayType = PointsArrayType.both;
		coverSearchData.UseLineCastToCover = false;
		coverSearchData.PointToBeClose = null;
		coverSearchData.shootType = CoverShootType.hide;
		coverSearchData.MinSDistToCarePos = (Bool_7 ? 256f : 1600f);
		CustomNavigationPoint customNavigationPoint = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: true);
		if (customNavigationPoint != null)
		{
			float sqrMagnitude = (vector - customNavigationPoint.Position).sqrMagnitude;
			if (sqrMagnitude > Float_35)
			{
				CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(vector);
				if (closestPoint != null)
				{
					sqrMagnitude = (vector - closestPoint.BasePosition).sqrMagnitude;
					if (sqrMagnitude > Float_35)
					{
						Float_36 = Time.time + 3f;
					}
					else
					{
						customNavigationPoint = closestPoint;
					}
				}
				else
				{
					Float_36 = Time.time + 3f;
					Debug.LogError($"Wrong dist to boss: {Mathf.Sqrt(sqrMagnitude)}");
				}
			}
		}
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(customNavigationPoint);
		Bool_6 = false;
		return customNavigationPoint;
	}

	public bool method_19()
	{
		return Float_36 < Time.time;
	}

	public Vector3 method_20()
	{
		if (BotOwner_0.BotFollower.BossToFollow != null)
		{
			return BotOwner_0.BotFollower.BossToFollow.Position;
		}
		return BotOwner_0.Position;
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return new AICoreActionEndStruct("LostEnemy");
		}
		if (BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("CanShoot");
		}
		if (method_14())
		{
			return new AICoreActionEndStruct("ShootToSmoke");
		}
		return base.EndGoToEnemy();
	}

	public override AICoreActionEndStruct EndRunToEnemyZigZag()
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return new AICoreActionEndStruct("LostEnemy");
		}
		if (BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("CanShoot");
		}
		return base.EndGoToEnemy();
	}

	public bool method_21()
	{
		if (BotOwner_0.BotsGroup.MembersCount > 1 && BotOwner_0.Memory.GoalEnemy != null)
		{
			return GClass856.SqrDistance(BotOwner_0.Memory.GoalEnemy.CurrPosition, BotOwner_0.Position) > 625f;
		}
		return false;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		if (method_14())
		{
			return new AICoreActionEndStruct("ShootToSmoke");
		}
		return new AICoreActionEndStruct("inFight");
	}

	public bool method_22()
	{
		if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.TimeLastSeenReal + 8f < Time.time)
		{
			return Float_53 + 10f < Time.time;
		}
		return false;
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("inCover");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new AICoreActionEndStruct("Spotted");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		if (method_21() && method_22())
		{
			Float_32 = Time.time;
			return new AICoreActionEndStruct("need help");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			Float_32 = Time.time;
			return new AICoreActionEndStruct("!IsInCover");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			Float_32 = Time.time;
			return new AICoreActionEndStruct("spotted");
		}
		if (method_30())
		{
			return AICoreActionEndStruct_1;
		}
		if (!BotOwner_0.LookSensor.EnoughDistToShoot(out var _))
		{
			Float_32 = Time.time;
			BotMemoryClass memory = BotOwner_0.Memory;
			float? secToBeSpotted = 32f;
			memory.Spotted(byHit: false, null, secToBeSpotted);
			Int_16++;
			return new AICoreActionEndStruct("EToShoot3");
		}
		if (!BotOwner_0.Memory.CurCustomCoverPoint.CanShootToTargetCast(BotOwner_0, BotOwner_0.Settings.FileSettings.Cover.DELTA_SEEN_FROM_COVE_LAST_POS))
		{
			Float_32 = Time.time;
			BotMemoryClass memory2 = BotOwner_0.Memory;
			float? secToBeSpotted = 32f;
			memory2.Spotted(byHit: false, null, secToBeSpotted);
			Int_16++;
			return new AICoreActionEndStruct("TargetCast2");
		}
		if (BotOwner_0.WeaponManager.Stationary.ShallEndShootFromCurrent())
		{
			Float_32 = Time.time;
			BotMemoryClass memory3 = BotOwner_0.Memory;
			float? secToBeSpotted = 32f;
			memory3.Spotted(byHit: false, null, secToBeSpotted);
			Int_16++;
			return new AICoreActionEndStruct("EndShoot1");
		}
		Int_16 = 0;
		return AICoreActionEndStruct_1;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_23(string label)
	{
		if (BotOwner_0.Memory.GoalEnemy != null && GClass856.SqrDistance(BotOwner_0.Position, BotOwner_0.Memory.GoalEnemy.CurrPosition) < 100f)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, label);
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemy, label);
	}

	public bool method_24()
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			return GClass856.SqrDistance(BotOwner_0.Position, BotOwner_0.Memory.GoalEnemy.CurrPosition) < 16f;
		}
		return false;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return new AICoreActionEndStruct("LostEnemy");
		}
		if (BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("CanShoot");
		}
		if (method_14())
		{
			return new AICoreActionEndStruct("ShootToSmoke");
		}
		if (BotOwner_0.Memory.GoalEnemy != null && GClass856.SqrDistance(BotOwner_0.Position, BotOwner_0.Memory.GoalEnemy.CurrPosition) < 100f)
		{
			return new AICoreActionEndStruct("enemyNear");
		}
		return base.EndRunToEnemy();
	}

	public override AICoreActionEndStruct EndHeal()
	{
		return base.EndHeal();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_14())
		{
			return new AICoreActionEndStruct("ShootToSmoke");
		}
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHoldTime");
		}
		if (method_21() && method_22())
		{
			return new AICoreActionEndStruct("need help");
		}
		if (!Bool_2 && method_29(Float_42))
		{
			return new AICoreActionEndStruct("WannaKill");
		}
		if (method_32())
		{
			Bool_8 = true;
			return new AICoreActionEndStruct("WUseStim");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot) && BotOwner_0.Memory.IsInCover)
		{
			if (goalEnemy.IsVisible && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Cover.END_HOLD_IF_ENEMY_CLOSE_AND_VISIBLE)
			{
				return new AICoreActionEndStruct("Distance");
			}
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("enemy");
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		if (BotOwner_0.GoToSomePointData.IsCome())
		{
			return new AICoreActionEndStruct("IsCome");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		method_39();
		if (Time.time - Float_40 > Float_37)
		{
			if (goalEnemy != null)
			{
				_ = Time.time - goalEnemy.PersonalLastSeenTime > 3f;
			}
			else
				_ = 0;
			if (BotOwner_0.DogFight.ShallStartCauseHavePlace() || goalEnemy == null || !goalEnemy.CanShoot || goalEnemy.Distance < 1f || BotOwner_0.WeaponManager.Stationary.ShallEndShootFromCurrent())
			{
				return new AICoreActionEndStruct("DogFight");
			}
		}
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return new AICoreActionEndStruct("LoseTarget");
		}
		if (!BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("!IsVisible");
		}
		if (!BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("!CanShoot");
		}
		if (BotOwner_0.Memory.LastDamageData != null && BotOwner_0.Memory.LastDamageData.TimeDamage + 0.1f > Time.time)
		{
			return new AICoreActionEndStruct("Damaged");
		}
		return AICoreActionEndStruct_1;
	}

	public bool method_25()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			return Time.time - goalEnemy.TimeLastSeen > 8f;
		}
		return true;
	}

	public void method_26(DamageInfoStruct arg1, EBodyPart arg2, float arg3)
	{
		if (Bool_13)
		{
			Float_52 = Time.time;
		}
	}

	public void method_27(BotOwner obj)
	{
		Bool_13 = false;
	}

	public bool method_28()
	{
		return method_42(CustomNavigationPoint_0);
	}

	public bool method_29(float period)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			return Time.time - goalEnemy.TimeLastSeenReal < period;
		}
		return false;
	}

	public bool method_30()
	{
		if (Float_49 < Time.time)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && !Bool_6 && goalEnemy.VisibleType != EEnemyPartVisibleType.Visible && Time.time - BotOwner_0.Memory.BotCurrentCoverInfo.ComeToCoverTime > 1f)
			{
				method_31();
				Bool_12 = false;
				return Bool_12;
			}
			Float_49 = Time.time + 0.5f;
			if (!method_41(BotOwner_0.Memory.CurCustomCoverPoint))
			{
				Int_14++;
				if (Int_14 >= Int_13)
				{
					method_31();
					Bool_12 = false;
				}
			}
			else
			{
				method_31();
				Bool_12 = true;
			}
		}
		return Bool_12;
	}

	public void method_31()
	{
		Int_13 = (Bool_7 ? GClass856.RandomInclude(4, 7) : GClass856.RandomInclude(8, 30));
		Int_14 = 0;
	}

	public bool method_32()
	{
		float num = Time.time - BotOwner_0.Medecine.Stimulators.LastEndUseTime;
		if (!Gclass440_0.CanStartUseStimulator())
		{
			return false;
		}
		if (num > 3f && Time.time - BotOwner_0.Memory.ComeToCoverTime > 10f && BotOwner_0.Medecine.Stimulators.HaveSmt)
		{
			return true;
		}
		return false;
	}

	public bool method_33(out Vector3 posToGo)
	{
		if (BotOwner_0.BotFollower.HaveBoss && Gclass440_0.PointForBoss.HasValue)
		{
			Vector3 value = Gclass440_0.PointForBoss.Value;
			float y = 1f;
			Vector3 position = BotOwner_0.BotFollower.BossToFollow.Position;
			Vector3 vector = position + new Vector3(1f, y, 1f);
			Vector3 vector2 = position + new Vector3(-1f, y, 1f);
			Vector3 vector3 = position + new Vector3(-1f, y, -1f);
			Vector3 vector4 = position + new Vector3(1f, y, -1f);
			Vector3? vector5 = null;
			float num = float.MaxValue;
			Vector3[] array = new Vector3[4] { vector, vector2, vector3, vector4 };
			for (int i = 0; i < array.Length; i++)
			{
				if (NavMesh.SamplePosition(array[i], out var hit, 2f, -1))
				{
					float magnitude = (hit.position - value).magnitude;
					if (magnitude < num)
					{
						vector5 = hit.position;
						num = magnitude;
					}
				}
			}
			if (vector5.HasValue)
			{
				posToGo = vector5.Value;
				return true;
			}
			posToGo = Vector3.zero;
			return false;
		}
		posToGo = Vector3.zero;
		return false;
	}

	public bool method_34()
	{
		if (Gclass440_0 == null)
		{
			return false;
		}
		Vector3? pointForBoss = Gclass440_0.PointForBoss;
		if (pointForBoss.HasValue && (pointForBoss.Value - BotOwner_0.Position).sqrMagnitude < 4f)
		{
			return true;
		}
		return false;
	}

	public bool method_35()
	{
		return BotOwner_0.Medecine.FirstAid.Damaged;
	}

	public bool method_36(out bool healByAnother, out bool isAmNearBoss)
	{
		if (Float_39 < Time.time)
		{
			Float_39 = Time.time + 5f;
			BotOwner_0.Medecine.FirstAid.CheckParts();
		}
		float num = Time.time - Float_38;
		isAmNearBoss = method_34();
		if (num > 3f && BotOwner_0.Medecine.FirstAid.Damaged)
		{
			float hpPercent = BotOwner_0.Medecine.FirstAid.GetHpPercent(EBodyPart.Chest);
			healByAnother = hpPercent < 0.52f;
			if (DEBUG_ALWAYS_BAD_HEALTH)
			{
				healByAnother = true;
			}
			if (DEBUG_ALWAYS_NOASK_HEAL)
			{
				healByAnother = false;
			}
			float num2 = Time.time - Float_46;
			bool flag = !isAmNearBoss || num2 < 22f;
			if (BotOwner_0.Medecine.FirstAid.HaveSmth2Use)
			{
				if (!flag)
				{
					healByAnother = false;
				}
				return true;
			}
			if (healByAnother && flag)
			{
				return true;
			}
		}
		healByAnother = false;
		return false;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_37()
	{
		if (Boolean_0 && Gclass440_0 != null && !Gclass440_0.EnoughtHaveGoodCovers && method_42(CustomNavigationPoint_0))
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
			if (BotOwner_0.Memory.IsInCover)
			{
				HoldFor(1f);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hold3");
			}
			if (BotOwner_0.Memory.GoalEnemy != null)
			{
				Vector3 currPosition = BotOwner_0.Memory.GoalEnemy.CurrPosition;
				FindPointForFight(checkCurrent: true);
				if (CustomNavigationPoint_0 != null && GClass856.SqrDistance(currPosition, CustomNavigationPoint_0.Position) > GClass856.SqrDistance(currPosition, BotOwner_0.Position) - -49f)
				{
					if (method_13() && BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.IsVisible)
					{
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "sfps3");
					}
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "am");
				}
			}
			Float_8 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run3");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (method_41(CustomNavigationPoint_0))
		{
			if (BotOwner_0.Memory.GoalEnemy != null)
			{
				Vector3 currPosition2 = BotOwner_0.Memory.GoalEnemy.CurrPosition;
				if (GClass856.SqrDistance(currPosition2, CustomNavigationPoint_0.Position) > GClass856.SqrDistance(currPosition2, BotOwner_0.Position) - -49f)
				{
					if (method_13() && BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.IsVisible)
					{
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "sfps4");
					}
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "am2");
				}
			}
			Float_8 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "CheckGoodFo");
		}
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			Float_40 = Time.time;
			Float_37 = 3f;
			BotOwner_0.BotLay.DelayPosibleLayFor(5f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "DelayPosibl");
		}
		method_38();
		if (Bool_5)
		{
			Bool_5 = false;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "Lay");
		}
		if (method_41(CustomNavigationPoint_0))
		{
			if (BotOwner_0.Memory.GoalEnemy != null)
			{
				Vector3 currPosition3 = BotOwner_0.Memory.GoalEnemy.CurrPosition;
				if (GClass856.SqrDistance(currPosition3, CustomNavigationPoint_0.Position) > GClass856.SqrDistance(currPosition3, BotOwner_0.Position) - -49f)
				{
					if (method_13() && BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.IsVisible)
					{
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "sfps5");
					}
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "am4");
				}
			}
			Float_8 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "CheckGoodFo");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			HoldFor(2f);
		}
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			Vector3 currPosition4 = BotOwner_0.Memory.GoalEnemy.CurrPosition;
			if (GClass856.SqrDistance(currPosition4, CustomNavigationPoint_0.Position) > GClass856.SqrDistance(currPosition4, BotOwner_0.Position) - -49f)
			{
				if (method_13() && BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.Memory.GoalEnemy.IsVisible)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "sfps6");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "am4");
			}
		}
		Float_8 = Time.time;
		return method_23("!IsInCover");
	}

	public void method_38()
	{
		if (Float_50 < Time.time)
		{
			Float_50 = Time.time + 0.5f;
			method_43();
		}
	}

	public void method_39(bool anyway = false)
	{
		if (!(Float_43 < Time.time || anyway))
		{
			return;
		}
		Float_43 = Time.time + 3f;
		if (CustomNavigationPoint_0 != null && !CustomNavigationPoint_0.IsSpotted)
		{
			if (!(method_35() ? method_42(CustomNavigationPoint_0) : ((Bool_7 && !BotOwner_0.Boss.IamBoss) ? method_40() : ((!Boolean_0 || Gclass440_0.EnoughtHaveGoodCovers) ? method_41(CustomNavigationPoint_0) : method_42(CustomNavigationPoint_0)))))
			{
				method_43();
			}
		}
		else
		{
			method_43();
		}
	}

	public bool method_40()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null || Time.time - goalEnemy.TimeLastSeen > 8f)
		{
			Vector3 vector = method_20();
			return (BotOwner_0.Position - vector).sqrMagnitude < Float_35;
		}
		return method_41(CustomNavigationPoint_0);
	}

	public bool method_41(CustomNavigationPoint checkPoint)
	{
		if (checkPoint == null)
		{
			return false;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			Bool_6 = true;
			return true;
		}
		if (Time.time - goalEnemy.TimeLastSeen > Float_48)
		{
			return false;
		}
		bool num = GClass369.CanShootToTarget(new ShootPointClass(goalEnemy.EnemyLastPositionReal + BotOwner.STAY_HEIGHT), checkPoint, BotOwner_0.LookSensor.Mask);
		if (num)
		{
			Bool_6 = true;
		}
		return num;
	}

	public bool method_42(CustomNavigationPoint pointOfSearch)
	{
		HashSet<Vector3> positionsIMustCare = CarePositions();
		return pointOfSearch.CanIHide(positionsIMustCare, 0f, useRaycast: true);
	}

	public void method_43()
	{
		Bool_5 = false;
		CustomNavigationPoint customNavigationPoint = ((!method_35()) ? method_44() : FindPointForHeal());
		if (customNavigationPoint != null && GClass856.SqrDistance(customNavigationPoint.Position, BotOwner_0.Position) > 2500f)
		{
			Bool_5 = true;
		}
		CustomNavigationPoint_0 = customNavigationPoint;
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			if (Float_51 + 8f > Time.time)
			{
				return AICoreActionEndStruct_1;
			}
			return new AICoreActionEndStruct("endSmokeShot");
		}
		if (Float_30 + 10f < Time.time)
		{
			return new AICoreActionEndStruct("endSuppress");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("CanShoot");
		}
		return AICoreActionEndStruct_1;
	}

	public CustomNavigationPoint method_44()
	{
		return FindPointForFight(checkCurrent: false);
	}

	public override void Dispose()
	{
		BotOwner_0.HealAnotherTarget.OnHealAsked -= method_16;
		BotOwner_0.GetPlayer.BeingHitAction -= method_26;
		BotOwner_0.Medecine.FirstAid.OnStartApply -= method_15;
		BotOwner_0.Medecine.FirstAid.OnEndApply -= method_27;
		base.Dispose();
	}
}

using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public abstract class GClass146 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public const float Float_3 = 8f;

	[NonSerialized]
	public const int Int_1 = 14;

	[NonSerialized]
	public const int Int_2 = 6;

	[NonSerialized]
	public const int Int_3 = 15;

	[NonSerialized]
	public const int Int_4 = 8;

	[NonSerialized]
	public const float Float_4 = 900f;

	[NonSerialized]
	public const float Float_5 = 256f;

	[NonSerialized]
	public const float Float_6 = 1600f;

	[NonSerialized]
	public const float Float_7 = 144f;

	[NonSerialized]
	public const float Float_8 = 100f;

	[NonSerialized]
	public const float Float_9 = 8f;

	[NonSerialized]
	public const float Float_10 = 3f;

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
	public const float Float_11 = 0.52f;

	[NonSerialized]
	public const float Float_12 = 15f;

	[NonSerialized]
	public const float Float_13 = 2f;

	[NonSerialized]
	public const float Float_14 = 3f;

	[NonSerialized]
	public const float Float_15 = 5f;

	[NonSerialized]
	public const float Float_16 = 2f;

	[NonSerialized]
	public const int Int_11 = 20;

	[NonSerialized]
	public const float Float_17 = 0.5f;

	[NonSerialized]
	public const float Float_18 = 5f;

	[NonSerialized]
	public const float Float_19 = 3f;

	[NonSerialized]
	public const float Float_20 = 22f;

	[NonSerialized]
	public const float Float_21 = 3f;

	[NonSerialized]
	public const float Float_22 = 10f;

	[NonSerialized]
	public const float Float_23 = 4f;

	[NonSerialized]
	public const int Int_12 = 2;

	public bool DEBUG_ALWAYS_BAD_HEALTH;

	public bool DEBUG_ALWAYS_NOASK_HEAL = true;

	[NonSerialized]
	public float Float_24 = 100f;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public GInterface7 Ginterface7_0;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_25;

	[NonSerialized]
	public float Float_26;

	[NonSerialized]
	public int Int_13 = 10;

	[NonSerialized]
	public float Float_27;

	[NonSerialized]
	public float Float_28;

	[NonSerialized]
	public float Float_29;

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public bool Bool_7;

	[NonSerialized]
	public bool Bool_8;

	[NonSerialized]
	public bool Bool_9;

	[NonSerialized]
	public float Float_30;

	[NonSerialized]
	public float Float_31;

	[NonSerialized]
	public float Float_32 = 1.5f;

	[NonSerialized]
	public float Float_33;

	[NonSerialized]
	public float Float_34;

	[NonSerialized]
	public float Float_35;

	[NonSerialized]
	public float Float_36;

	[NonSerialized]
	public float Float_37;

	[NonSerialized]
	public bool Bool_10;

	[NonSerialized]
	public float Float_38;

	[NonSerialized]
	public int Int_14;

	[NonSerialized]
	public float Float_39;

	[NonSerialized]
	public int Int_15;

	[NonSerialized]
	public HashSet<string> HashSet_0 = new HashSet<string> { "ZoneSanatorium1", "ZoneSanatorium2" };

	[NonSerialized]
	public bool Bool_11;

	[NonSerialized]
	public float Float_40;

	public bool Boolean_0 => BotOwner_0.Settings.FileSettings.Boss.SANITAR_TWO_COVER_TACTIC;

	public GClass146(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public void method_13(BotOwner obj)
	{
		Bool_11 = true;
	}

	public override void OnActivate()
	{
		BotOwner_0.GetPlayer.ActiveHealthController.DoPainKiller();
		Bool_5 = HashSet_0.Contains(BotOwner_0.BotsGroup.BotZone.NameZone);
		Float_37 = (Bool_5 ? 3f : 8f);
		BotOwner_0.Medecine.FirstAid.OnStartApply += method_13;
		BotOwner_0.Medecine.FirstAid.OnEndApply += method_19;
		BotOwner_0.GetPlayer.BeingHitAction += method_18;
	}

	public void SetBossData(GInterface7 bossData)
	{
		Ginterface7_0 = bossData;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		Bool_8 = false;
		if (Boolean_0 && !Ginterface7_0.EnoughtHaveGoodCovers && BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "use2covers");
		}
		string cause;
		bool bool_ = method_6(out cause) && method_22();
		Bool_9 = bool_;
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "dog");
		}
		if (Bool_6 && method_24())
		{
			Bool_6 = false;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.healStimulators, "shallUseSti");
		}
		if (Bool_9)
		{
			if (BotOwner_0.Medecine.Using)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.healStimulators, "lastCheckCa");
			}
			if (method_28(out var healByAnother, out var _))
			{
				if (healByAnother)
				{
					Bool_8 = true;
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "healByAnoth");
				}
				Bool_7 = true;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "shallUseFir");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, "standart");
		}
		float num = Time.time - Float_34;
		Float_34 = Time.time;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (num > 1f)
		{
			method_32(anyway: true);
		}
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			if (Boolean_0 && !Ginterface7_0.EnoughtHaveGoodCovers && Time.time - goalEnemy.LastChangeVisionTime < 3f)
			{
				Float_26 = 3f;
				Float_30 = Time.time;
				Ginterface7_0.SetFightPosition(b: true, BotOwner_0);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "SetFightPos");
			}
			if (Time.time - Float_40 < 5f)
			{
				Float_26 = 3f;
				Float_30 = Time.time;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "isUnderHeal");
			}
			if (Time.time - goalEnemy.FirstTimeSeen < 2f)
			{
				Float_26 = 3f;
				Float_30 = Time.time;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "FIRSTTIMESH");
			}
			if (Time.time - BotOwner_0.Memory.LeaveCoverTime < 3f && Time.time - BotOwner_0.Medecine.FirstAid.LastEndTime < 5f && goalEnemy.Distance < 20f)
			{
				Float_30 = Time.time;
				Float_26 = 8f;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "shootFromPl");
			}
		}
		float num2 = Time.time - Float_27;
		bool flag = false;
		if (!BotOwner_0.Boss.IamBoss && method_28(out var healByAnother2, out var isAmNearBoss2) && healByAnother2)
		{
			if (isAmNearBoss2)
			{
				Ginterface7_0.WantAskHeal(BotOwner_0);
				BotOwner_0.HealingBySomebody.StartWait();
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "isAmNearBos");
			}
			flag = true;
		}
		if ((Bool_8 || flag) && !BotOwner_0.Boss.IamBoss && method_25(out var posToGo))
		{
			Float_35 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "TryGoBoss", new GClass30(posToGo));
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if (Bool_7)
			{
				Bool_7 = false;
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "shallUseFir");
			}
			if (Bool_6)
			{
				Bool_6 = false;
				Ginterface7_0.SomebodyUseStimulator();
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.healStimulators, "shallUseSti");
			}
			if (Bool_5 && !BotOwner_0.Boss.IamBoss && method_17())
			{
				Vector3 vector = method_16();
				if ((BotOwner_0.Memory.CurCustomCoverPoint.Position - vector).sqrMagnitude > Float_24)
				{
					if (CustomNavigationPoint_0 != null && (CustomNavigationPoint_0.Position - vector).sqrMagnitude <= Float_24)
					{
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "nextPosible");
					}
					method_32();
					HoldFor(1f);
					Float_25 = Time.time + 3f;
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "nextPosible");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "sDistCloseB");
			}
			if (method_34(BotOwner_0.Memory.CurCustomCoverPoint))
			{
				if (num2 > 3f && BotOwner_0.Medecine.FirstAid.Damaged && BotOwner_0.Medecine.FirstAid.HaveSmth2Use)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "CheckGoodFo");
				}
				if (goalEnemy != null && goalEnemy.CanShoot)
				{
					if (Boolean_0 && !Ginterface7_0.EnoughtHaveGoodCovers)
					{
						return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "EnoughtHave");
					}
					float num3 = 5f;
					BotOwner_0.BotLay.DelayPosibleLayFor(num3);
					Float_30 = Time.time;
					Float_26 = num3;
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "periodWait");
				}
				HoldFor(1f);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "HoldFor");
			}
			if (BotOwner_0.Medecine.FirstAid.Damaged && BotOwner_0.Medecine.FirstAid.HaveSmth2Use)
			{
				if (num2 > 3f)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "HEALPERIODS");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "HEALPERIODS");
			}
			if (method_21(Float_32))
			{
				return method_29(wannaKill: true);
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "WannaKill");
		}
		if (method_27())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "IsDamaged");
		}
		if (method_21(Float_32))
		{
			return method_29(wannaKill: true);
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "NDamaged");
	}

	public void ForceRecalcShootPos()
	{
		CustomNavigationPoint customNavigationPoint = FindPointForFight(checkCurrent: false);
		bool flag = method_34(customNavigationPoint);
		Ginterface7_0.SetFightPosition(flag, BotOwner_0);
		if (flag)
		{
			CustomNavigationPoint_0 = customNavigationPoint;
		}
	}

	public CustomNavigationPoint FindPointForStay()
	{
		ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		Vector3 currPosition = BotOwner_0.Memory.GoalEnemy.CurrPosition;
		int value = (Bool_5 ? 6 : 14);
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
		Bool_4 = false;
		return coverPointMain;
	}

	public CustomNavigationPoint FindPointForHeal()
	{
		ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		int value = (Bool_5 ? 8 : 15);
		PointsArrayType arrayType = (method_14() ? PointsArrayType.covers : PointsArrayType.both);
		CoverSearchData coverSearchData = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.hide, Bool_5 ? 256f : 1600f, 0f, CoverSearchType.distToBot, shoot2point, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f), PointsArrayType.both, useSelfFindPoint: true, null, value);
		coverSearchData.SearchType = CoverSearchType.distToBot;
		coverSearchData.CenterPos = BotOwner_0.Position;
		coverSearchData.UseSelfFindPoint = false;
		coverSearchData.ArrayType = arrayType;
		coverSearchData.UseLineCastToCover = true;
		coverSearchData.PointToBeClose = null;
		coverSearchData.shootType = CoverShootType.hide;
		coverSearchData.MinSDistToCarePos = (Bool_5 ? 100f : 144f);
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: true);
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(coverPointMain);
		Bool_4 = false;
		return coverPointMain;
	}

	public CustomNavigationPoint FindPointNearBoss()
	{
		Vector3 vector = method_16();
		CoverSearchData coverSearchData = new CoverSearchData(vector, BotOwner_0.CoverSearchInfo, CoverShootType.hide, 4f, 0f, CoverSearchType.distToToCenter, null, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f), PointsArrayType.both);
		coverSearchData.SearchType = CoverSearchType.distToToCenter;
		coverSearchData.UseSelfFindPoint = false;
		coverSearchData.ArrayType = PointsArrayType.both;
		coverSearchData.UseLineCastToCover = false;
		coverSearchData.PointToBeClose = null;
		coverSearchData.shootType = CoverShootType.hide;
		coverSearchData.MinSDistToCarePos = (Bool_5 ? 256f : 900f);
		CustomNavigationPoint customNavigationPoint = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: true);
		if (customNavigationPoint != null)
		{
			float sqrMagnitude = (vector - customNavigationPoint.Position).sqrMagnitude;
			if (sqrMagnitude > Float_24)
			{
				CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(vector);
				if (closestPoint != null)
				{
					sqrMagnitude = (vector - closestPoint.BasePosition).sqrMagnitude;
					if (sqrMagnitude > Float_24)
					{
						Float_25 = Time.time + 3f;
					}
					else
					{
						customNavigationPoint = closestPoint;
					}
				}
				else
				{
					Float_25 = Time.time + 3f;
					Debug.LogError($"Wrong dist to boss: {Mathf.Sqrt(sqrMagnitude)}");
				}
			}
		}
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(customNavigationPoint);
		Bool_4 = false;
		return customNavigationPoint;
	}

	public abstract CustomNavigationPoint FindPointForFight(bool checkCurrent);

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

	public bool method_14()
	{
		return BotOwner_0.Settings.FileSettings.Boss.SANITAR_ONLY_FIGHT_COVERS;
	}

	public bool method_15()
	{
		return Float_25 < Time.time;
	}

	public Vector3 method_16()
	{
		if (BotOwner_0.BotFollower.BossToFollow != null)
		{
			return BotOwner_0.BotFollower.BossToFollow.Position;
		}
		return BotOwner_0.Position;
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (method_22())
		{
			return AICoreActionEndStruct_1;
		}
		if (!BotOwner_0.LookSensor.EnoughDistToShoot(out var _))
		{
			return new AICoreActionEndStruct("EnoughDistToShoot");
		}
		if (!BotOwner_0.Memory.CurCustomCoverPoint.CanShootToTargetCast(BotOwner_0, BotOwner_0.Settings.FileSettings.Cover.DELTA_SEEN_FROM_COVE_LAST_POS))
		{
			return new AICoreActionEndStruct("CanShootToTargetCast");
		}
		if (BotOwner_0.WeaponManager.Stationary.ShallEndShootFromCurrent())
		{
			return new AICoreActionEndStruct("ShallEndShootFromCurrent");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (method_3())
		{
			return new AICoreActionEndStruct("dfStart");
		}
		bool isInCover = BotOwner_0.Memory.IsInCover;
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			method_32();
		}
		if (isInCover)
		{
			BotOwner_0.BotRun.EndMove();
			Float_29 = Time.time;
			return new AICoreActionEndStruct("shallEnd");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		if (!method_21(Float_32 + 1f))
		{
			return new AICoreActionEndStruct("!WannaKill");
		}
		return base.EndRunToEnemy();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHoldTime");
		}
		if (!Bool_2 && method_21(Float_32))
		{
			return new AICoreActionEndStruct("WannaKill");
		}
		if (method_28(out var _, out var _))
		{
			Bool_7 = true;
			return new AICoreActionEndStruct("WannaHeal");
		}
		if (method_24())
		{
			Bool_6 = true;
			return new AICoreActionEndStruct("WantUseStim");
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
		if (Time.time - BotOwner_0.HealingBySomebody.StartWaitHeal < 2f)
		{
			return AICoreActionEndStruct_1;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		method_32();
		if (Time.time - Float_30 > Float_26)
		{
			bool flag = goalEnemy != null && Time.time - goalEnemy.PersonalLastSeenTime > 3f;
			if (method_28(out var _, out var _))
			{
				Bool_7 = true;
				return new AICoreActionEndStruct("WannaHeal");
			}
			if ((method_34(CustomNavigationPoint_0) || flag) && Ginterface7_0.EnoughtHaveGoodCovers)
			{
				return new AICoreActionEndStruct("enemyNoMatt");
			}
			if (BotOwner_0.DogFight.ShallStartCauseHavePlace() || goalEnemy == null || !goalEnemy.CanShoot || goalEnemy.Distance < 1f || BotOwner_0.WeaponManager.Stationary.ShallEndShootFromCurrent())
			{
				return new AICoreActionEndStruct("DogFight");
			}
		}
		if (Boolean_0 && Ginterface7_0.EnoughtHaveGoodCovers && method_34(CustomNavigationPoint_0))
		{
			return new AICoreActionEndStruct("use2covers");
		}
		return AICoreActionEndStruct_1;
	}

	public bool method_17()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			return Time.time - goalEnemy.TimeLastSeen > 8f;
		}
		return true;
	}

	public void method_18(DamageInfoStruct arg1, EBodyPart arg2, float arg3)
	{
		if (Bool_11)
		{
			Float_40 = Time.time;
		}
	}

	public void method_19(BotOwner obj)
	{
		Bool_11 = false;
	}

	public bool method_20()
	{
		return method_35(CustomNavigationPoint_0);
	}

	public bool method_21(float period)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			return Time.time - goalEnemy.TimeLastSeenReal < period;
		}
		return false;
	}

	public bool method_22()
	{
		if (Float_38 < Time.time)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && !Bool_4 && goalEnemy.VisibleType != EEnemyPartVisibleType.Visible && Time.time - BotOwner_0.Memory.BotCurrentCoverInfo.ComeToCoverTime > 1f)
			{
				method_23();
				Bool_10 = false;
				return Bool_10;
			}
			Float_38 = Time.time + 0.5f;
			if (!method_34(BotOwner_0.Memory.CurCustomCoverPoint))
			{
				Int_14++;
				if (Int_14 >= Int_13)
				{
					method_23();
					Bool_10 = false;
				}
			}
			else
			{
				method_23();
				Bool_10 = true;
			}
		}
		return Bool_10;
	}

	public void method_23()
	{
		Int_13 = (Bool_5 ? GClass856.RandomInclude(4, 7) : GClass856.RandomInclude(8, 30));
		Int_14 = 0;
	}

	public bool method_24()
	{
		float num = Time.time - BotOwner_0.Medecine.Stimulators.LastEndUseTime;
		if (!Ginterface7_0.CanStartUseStimulator())
		{
			return false;
		}
		if (num > 3f && Time.time - BotOwner_0.Memory.ComeToCoverTime > 10f && BotOwner_0.Medecine.Stimulators.HaveSmt)
		{
			return true;
		}
		return false;
	}

	public bool method_25(out Vector3 posToGo)
	{
		if (BotOwner_0.BotFollower.HaveBoss && Ginterface7_0.PointForBoss.HasValue)
		{
			Vector3 value = Ginterface7_0.PointForBoss.Value;
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

	public bool method_26()
	{
		if (Ginterface7_0 == null)
		{
			return false;
		}
		Vector3? pointForBoss = Ginterface7_0.PointForBoss;
		if (pointForBoss.HasValue && (pointForBoss.Value - BotOwner_0.Position).sqrMagnitude < 4f)
		{
			return true;
		}
		return false;
	}

	public bool method_27()
	{
		if (!BotOwner_0.Medecine.FirstAid.Damaged)
		{
			return Bool_7;
		}
		return true;
	}

	public bool method_28(out bool healByAnother, out bool isAmNearBoss)
	{
		if (Float_28 < Time.time)
		{
			Float_28 = Time.time + 5f;
			BotOwner_0.Medecine.FirstAid.CheckParts();
		}
		float num = Time.time - Float_27;
		isAmNearBoss = method_26();
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
			float num2 = Time.time - Float_35;
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

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_29(bool wannaKill)
	{
		if (!wannaKill && Boolean_0 && !Ginterface7_0.EnoughtHaveGoodCovers && method_35(CustomNavigationPoint_0))
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
			if (BotOwner_0.Memory.IsInCover)
			{
				HoldFor(2f);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hold3");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run3");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (method_34(CustomNavigationPoint_0))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "CheckGoodFo");
		}
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			Float_30 = Time.time;
			Float_26 = 3f;
			BotOwner_0.BotLay.DelayPosibleLayFor(5f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "DelayPosibl");
		}
		method_31();
		if (method_34(CustomNavigationPoint_0))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "CheckGoodFo");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			HoldFor(2f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "HOLDBEFOREA");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "!IsInCover");
	}

	public void method_30()
	{
		CustomNavigationPoint customNavigationPoint_ = FindPointForStay();
		CustomNavigationPoint_0 = customNavigationPoint_;
	}

	public void method_31()
	{
		if (Float_39 < Time.time)
		{
			Float_39 = Time.time + 0.5f;
			method_36();
		}
	}

	public void method_32(bool anyway = false)
	{
		if (!(Float_33 < Time.time || anyway))
		{
			return;
		}
		Float_33 = Time.time + 3f;
		if (CustomNavigationPoint_0 != null && !CustomNavigationPoint_0.IsSpotted)
		{
			if (!(method_27() ? method_35(CustomNavigationPoint_0) : ((Bool_5 && !BotOwner_0.Boss.IamBoss) ? method_33() : ((!Boolean_0 || Ginterface7_0.EnoughtHaveGoodCovers) ? method_34(CustomNavigationPoint_0) : method_35(CustomNavigationPoint_0)))))
			{
				method_36();
			}
		}
		else
		{
			method_36();
		}
	}

	public bool method_33()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null || Time.time - goalEnemy.TimeLastSeen > 8f)
		{
			Vector3 vector = method_16();
			return (BotOwner_0.Position - vector).sqrMagnitude < Float_24;
		}
		return method_34(CustomNavigationPoint_0);
	}

	public bool method_34(CustomNavigationPoint checkPoint)
	{
		if (checkPoint == null)
		{
			return false;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			Bool_4 = true;
			return true;
		}
		if (Time.time - goalEnemy.TimeLastSeen > Float_37)
		{
			return false;
		}
		bool num = GClass369.CanShootToTarget(new ShootPointClass(goalEnemy.EnemyLastPositionReal + BotOwner.STAY_HEIGHT), checkPoint, BotOwner_0.LookSensor.Mask);
		if (num)
		{
			Bool_4 = true;
		}
		return num;
	}

	public bool method_35(CustomNavigationPoint pointOfSearch)
	{
		HashSet<Vector3> positionsIMustCare = CarePositions();
		return pointOfSearch.CanIHide(positionsIMustCare, 0f, useRaycast: true);
	}

	public void method_36()
	{
		CustomNavigationPoint customNavigationPoint_ = (method_27() ? FindPointForHeal() : ((!Bool_5 || BotOwner_0.Boss.IamBoss) ? method_37() : ((!method_17()) ? method_37() : FindPointNearBoss())));
		CustomNavigationPoint_0 = customNavigationPoint_;
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
	}

	public CustomNavigationPoint method_37()
	{
		if (Boolean_0)
		{
			if (Ginterface7_0 != null)
			{
				CustomNavigationPoint customNavigationPoint = FindPointForFight(checkCurrent: false);
				bool b = method_34(customNavigationPoint);
				Ginterface7_0.SetFightPosition(b, BotOwner_0);
				return customNavigationPoint;
			}
			return FindPointForHeal();
		}
		return FindPointForFight(checkCurrent: false);
	}

	public override void Dispose()
	{
		BotOwner_0.GetPlayer.BeingHitAction -= method_18;
		BotOwner_0.Medecine.FirstAid.OnStartApply -= method_13;
		BotOwner_0.Medecine.FirstAid.OnEndApply -= method_19;
		base.Dispose();
	}
}

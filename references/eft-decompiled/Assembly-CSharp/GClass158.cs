using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;
using UnityEngine.AI;

public class GClass158 : GClass157
{
	public enum ETagillaBattleLogic
	{
		Ambush = 1,
		Default,
		MeleeAssault
	}

	[CompilerGenerated]
	public class Class225
	{
		public GClass158 gclass158_0;

		public float d;

		public CustomNavigationPoint p;

		public bool v;

		public void method_0(CustomNavigationPoint point)
		{
			if ((gclass158_0.BotOwner_0.Position - point.Position).sqrMagnitude < d)
			{
				p = point;
				v = true;
			}
		}
	}

	[NonSerialized]
	public const float Float_5 = 3f;

	[NonSerialized]
	public const float Float_6 = 0.35f;

	[NonSerialized]
	public const int Int_2 = 3;

	[NonSerialized]
	public const float Float_7 = 20f;

	[NonSerialized]
	public const float Float_8 = 0.1f;

	[NonSerialized]
	public const float Float_9 = 5f;

	[NonSerialized]
	public const float Float_10 = 10f;

	[NonSerialized]
	public const float Float_11 = 7f;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public List<EnemyInfo> List_1 = new List<EnemyInfo>();

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public float Float_12;

	[NonSerialized]
	public float Float_13;

	[NonSerialized]
	public GClass528 Gclass528_0;

	[NonSerialized]
	public GClass412 Gclass412_0;

	[NonSerialized]
	public float Float_14;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_15 = 999f;

	[NonSerialized]
	public bool Bool_7;

	[NonSerialized]
	public float Float_16 = -1f;

	[NonSerialized]
	public float Float_17;

	[NonSerialized]
	public bool Bool_8;

	[NonSerialized]
	public float Float_18;

	[NonSerialized]
	public float Float_19;

	[NonSerialized]
	public bool Bool_9;

	[NonSerialized]
	public float Float_20;

	[NonSerialized]
	public int Int_4;

	[NonSerialized]
	public float Float_21;

	[NonSerialized]
	public float Float_22;

	[NonSerialized]
	public bool Bool_10;

	[NonSerialized]
	public float Float_23;

	public BotGlobalsBossSettings BotGlobalsBossSettings_0 => BotOwner_0.Settings.FileSettings.Boss;

	public float Single_0 => 14f;

	public float MIDDLE_ATTACK_DIST => BotOwner_0.Settings.FileSettings.Boss.KILLA_MIDDLE_ATTACK_DIST;

	public GClass158(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass528_0 = new GClass528(BotOwner_0);
	}

	public override bool ShallUseNow()
	{
		bool haveEnemy = BotOwner_0.Memory.HaveEnemy;
		if (haveEnemy && BotOwner_0.CallForHelp.WantCallForSavages())
		{
			BotOwner_0.CallForHelp.CallForSavages();
		}
		return haveEnemy;
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
		if (BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogicTagilla();
			if (aICoreActionResultStruct.HasValue)
			{
				return aICoreActionResultStruct.Value;
			}
		}
		if (Nullable_0.HasValue)
		{
			BotLogicDecision value = Nullable_0.Value;
			Nullable_0 = null;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(value, "NextLogic");
		}
		bool anywayAttack;
		int num = method_31(out anywayAttack, resetCache: true);
		if (method_15(num))
		{
			ThrowWeapType grenadeType = ThrowWeapType.frag_grenade;
			if (BotOwner_0.WeaponManager.Grenades.HaveGrenadeOfType(ThrowWeapType.frag_grenade) && method_14(grenadeType, out var decision))
			{
				return decision;
			}
		}
		if (num < 3 && Gclass459_0.WantMeleeAssault())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(Gclass459_0.DoMeleeAssault(), "WantMeleeAs");
		}
		if (!goalEnemy.IsVisible)
		{
			if (BotOwner_0.SmokeGrenade.ShallShoot())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootToSmoke, "Smoke");
			}
			if (BotOwner_0.SmokeGrenade.IsInSmoke)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "IsInSmoke");
			}
		}
		if (Gclass459_0.PathToEnemyLengthByNavMesh() > 999f)
		{
			return Gclass459_0.AttackOrHoldOrShoot();
		}
		if (!anywayAttack && num >= BotOwner_0.Settings.FileSettings.Boss.KILLA_ENEMIES_TO_ATTACK)
		{
			return method_29();
		}
		return method_16();
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26>? InFightLogicTagilla()
	{
		if (Gclass459_0.WantMeleeAssault())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(Gclass459_0.DoMeleeAssault(), "InFightMele");
		}
		if (method_26())
		{
			if (BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0) == BotLogicDecision.holdPosition)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(3f), "InFight1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "InFight1");
		}
		if (Gclass459_0.PathToEnemyLengthByNavMesh() > 999f)
		{
			return Gclass459_0.AttackOrHoldOrShoot();
		}
		return InFightLogic();
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			data = new CoverSearchData(data.CenterPos, data.Bot, CoverShootType.hide, data.MaxDistSqr, 0f, CoverSearchType.distToBot, data.Shoot2Point, null, null, data.CheckShootHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
			return base.FindPoint(data, p, checkCurrent);
		}
		if (!Bool_6)
		{
			if (CustomNavigationPoint_0 != null && !CustomNavigationPoint_0.IsSpotted && CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id))
			{
				return CustomNavigationPoint_0;
			}
		}
		else
		{
			Bool_6 = false;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public bool method_14(ThrowWeapType grenadeType, out global::AICoreActionResultStruct<BotLogicDecision, GClass26> decision)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.WeaponManager.Grenades.HaveGrenadeOfType(grenadeType) && BotOwner_0.BotRequestController.TryActivateThrowGrenadeRequestToPlace(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(goalEnemy.Person.ProfileId)))
		{
			Float_23 = Time.time + 120f;
			decision = new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressGrenade, "AmbushGrenade");
			return true;
		}
		decision = default(global::AICoreActionResultStruct<BotLogicDecision, GClass26>);
		return false;
	}

	public bool method_15(int closeEnemies)
	{
		if (Float_23 < Time.time)
		{
			return false;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (closeEnemies > 0 && goalEnemy != null && BotOwner_0.Memory.IsInCover)
		{
			if (goalEnemy.Person.AIData.PlaceInfo != null)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (Gclass459_0.WantMeleeAssault())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		if (BotOwner_0.Brain.LastDecision.HasValue)
		{
			if (BotOwner_0.Brain.LastDecision.Value == BotLogicDecision.runToEnemy && method_33())
			{
				return new AICoreActionEndStruct("WantSuppres");
			}
			if (BotOwner_0.Brain.LastDecision.Value == BotLogicDecision.runToEnemyZigZag)
			{
				float sqrMagnitude = (Vector3_0 - BotOwner_0.Position).sqrMagnitude;
				Vector3_0 = BotOwner_0.Position;
				if (Gclass528_0.CheckIsBadVal(sqrMagnitude, 0.0001f))
				{
					Float_22 = Time.time + 45f;
					return new AICoreActionEndStruct("ZigZagerror");
				}
			}
		}
		if (Gclass459_0.WantMeleeAssault())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
		}
		if (method_32())
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
		if (Gclass459_0.WantMeleeAssault())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
		}
		return base.EndDogFight();
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
		if (method_32())
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
		if (Gclass459_0.WantMeleeAssault())
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
		if (Gclass459_0.WantMeleeAssault())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
		}
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHoldTime");
		}
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			Bool_2 = false;
			return new AICoreActionEndStruct("CanShoot");
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
		if (Gclass459_0.WantMeleeAssault())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
		}
		return base.EndShootFromPlace();
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		if (Gclass459_0.WantMeleeAssault())
		{
			return new AICoreActionEndStruct("WantMeleeAs");
		}
		return base.EndShootFromCover();
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		if (method_31(out var _, resetCache: true) >= 3)
		{
			return new AICoreActionEndStruct("Enemies>");
		}
		return Gclass459_0.EndOneMeleeAttack();
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_16()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		float num = goalEnemy.Distance / 3f - 1f;
		if (num < 2f)
		{
			num = 2f;
		}
		if (BotOwner_0.WeaponManager.Stationary.CheckWantTakeStationary(BotOwner_0.Settings.FileSettings.Cover.STATIONARY_WEAPON_MAX_DIST_TO_USE) != null)
		{
			BotLogicDecision? currentDecision = BotOwner_0.WeaponManager.Stationary.GetCurrentDecision();
			if (currentDecision.HasValue)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(currentDecision.Value, "AssaultMode");
			}
		}
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		if (shootPointClass == null)
		{
			return method_17();
		}
		shootPointClass.DistCoef = 0.7f;
		Vector3 centerPos = (BotOwner_0.Position + shootPointClass.Point) / 2f;
		float maxDistSqr = num * num;
		CoverSearchData data = new CoverSearchData(centerPos, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, maxDistSqr, 0f, CoverSearchType.distToToCenter, shootPointClass, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
		Bool_6 = true;
		float num2 = Time.time - goalEnemy.TimeLastSeenReal;
		if (!goalEnemy.IsVisible && num2 > BotOwner_0.Settings.FileSettings.Boss.KILLA_START_SEARCH_SEC)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.search, "AssaultMode");
		}
		if (Float_12 < Time.time)
		{
			Float_12 = Time.time + 1f;
			CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
		}
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "AssaultMode");
		}
		if (goalEnemy.Distance < Single_0)
		{
			return method_17();
		}
		if (goalEnemy.Distance < MIDDLE_ATTACK_DIST)
		{
			return method_18();
		}
		return method_20();
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_17()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			Int_3++;
			return method_30();
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0, BotLogicDecision.goToEnemy), "CloseTryMov");
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_18()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return method_30();
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			float num = Time.time - BotOwner_0.Memory.ComeToCoverTime;
			float num2 = BotOwner_0.Settings.FileSettings.Boss.KILLA_HOLD_DELAY - num;
			if (num2 > 0f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(num2), "MidDist1");
			}
		}
		if (Bool_2)
		{
			return method_8(3f, "Mid");
		}
		if (!goalEnemy.IsVisible && Int_3 > BotOwner_0.Settings.FileSettings.Boss.KILLA_CLOSEATTACK_TIMES)
		{
			Int_3 = 0;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(BotOwner_0.Settings.FileSettings.Boss.KILLA_CLOSEATTACK_DELAY), "MidDist2");
		}
		if (!goalEnemy.CanShoot && BotOwner_0.WeaponManager.Reload.BulletCount < BotOwner_0.Settings.FileSettings.Boss.KILLA_BULLET_TO_RELOAD)
		{
			BotOwner_0.WeaponManager.Reload.Reload();
			HoldFor(1f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "MidDist3");
		}
		if (!method_22() && goalEnemy.ShallISuppress())
		{
			return method_23(grenadePriority: false, method_19());
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_19(), "MidDist4");
	}

	public BotLogicDecision method_19()
	{
		BotLogicDecision runDecision = ((Float_22 < Time.time) ? BotLogicDecision.runToEnemyZigZag : BotLogicDecision.runToEnemy);
		return BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0, runDecision);
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_20()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (CustomNavigationPoint_0 != null)
		{
			if (goalEnemy.IsVisible)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "FarDist");
			}
			float num = Time.time - BotOwner_0.Memory.ComeToCoverTime;
			if (BotOwner_0.Memory.IsInCover)
			{
				float num2 = BotOwner_0.Settings.FileSettings.Boss.KILLA_HOLD_DELAY - num;
				if (num2 > 0f)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(num2), "FarDist");
				}
			}
			if (!method_22() && goalEnemy.ShallISuppress())
			{
				if (num > 20f)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0), "fd_delta15");
				}
				return method_23(grenadePriority: false, BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0));
			}
			if (Bool_2)
			{
				return method_8(3f, "Sup1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_21(), "FarDist");
		}
		if (!method_22() && goalEnemy.ShallISuppress())
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return method_30();
			}
			return method_23(grenadePriority: true, BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0));
		}
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return method_30();
		}
		if (Bool_2)
		{
			return method_8(3f, "Sup2");
		}
		if (!goalEnemy.CanShoot && BotOwner_0.WeaponManager.Reload.BulletCount < BotOwner_0.Settings.FileSettings.Boss.KILLA_BULLET_TO_RELOAD)
		{
			BotOwner_0.WeaponManager.Reload.Reload();
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "FarDist");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			float num3 = Time.time - BotOwner_0.Memory.ComeToCoverTime;
			float num4 = BotOwner_0.Settings.FileSettings.Boss.KILLA_HOLD_DELAY - num3;
			if (num4 > 0f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(num4), "FarDist");
			}
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0), "tryMoveEn");
	}

	public BotLogicDecision method_21()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return BotLogicDecision.runToEnemy;
		}
		if (BotOwner_0.CanSprintPlayer)
		{
			return BotLogicDecision.runToCover;
		}
		return BotLogicDecision.attackMoving;
	}

	public bool method_22()
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

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_23(bool grenadePriority, BotLogicDecision doThisIfCant)
	{
		ThrowWeapType? grenadeType = null;
		EnemyInfo enemyInfo = BotOwner_0.Memory.GoalEnemy;
		if (List_1.Count > 1)
		{
			foreach (EnemyInfo item in List_1)
			{
				if (item != BotOwner_0.Memory.GoalEnemy && item.IsSuppressed())
				{
					enemyInfo = item;
					grenadeType = ThrowWeapType.smoke_grenade;
					break;
				}
			}
		}
		if (grenadePriority)
		{
			if (grenadeType.HasValue && BotOwner_0.WeaponManager.Grenades.HaveGrenadeOfType(grenadeType.Value) && BotOwner_0.SuppressGrenade.Init(enemyInfo, grenadeType, null))
			{
				HoldFor(BotOwner_0.Settings.FileSettings.Boss.KILLA_AFTER_GRENADE_SUPPRESS_DELAY);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressGrenade, "Suppress");
			}
			grenadeType = ThrowWeapType.frag_grenade;
			if (BotOwner_0.WeaponManager.Grenades.HaveGrenadeOfType(grenadeType.Value))
			{
				if (BotOwner_0.SuppressGrenade.Init(enemyInfo, grenadeType, null))
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressGrenade, "Suppress");
				}
			}
			else
			{
				grenadeType = ThrowWeapType.stun_grenade;
				if (BotOwner_0.WeaponManager.Grenades.HaveGrenadeOfType(grenadeType.Value) && BotOwner_0.SuppressGrenade.Init(enemyInfo, grenadeType, null))
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressGrenade, "Suppress");
				}
			}
		}
		ShootPointClass shootPointClass = new ShootPointClass(enemyInfo.EnemyLastPositionReal + BotOwner.STAY_HEIGHT, 0.7f);
		if (GClass369.CanShootToTarget(shootPointClass, BotOwner_0.WeaponRoot.position, BotOwner_0.LookSensor.Mask))
		{
			BotOwner_0.SuppressShoot.Init(enemyInfo);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "Suppress");
		}
		if (method_24(shootPointClass, BotOwner_0.Settings.FileSettings.Boss.KILLA_DIST_TO_GO_TO_SUPPRESS, out var pos) && pos != null)
		{
			BotOwner_0.SuppressShoot.Init(enemyInfo, pos);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "Suppress");
		}
		if (doThisIfCant == BotLogicDecision.holdPosition)
		{
			HoldFor(1f);
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(doThisIfCant, "Suppress");
	}

	public bool method_24(ShootPointClass shootPos, float dist, out CustomNavigationPoint pos)
	{
		float d = dist * dist;
		CustomNavigationPoint p = null;
		bool v = false;
		BotOwner_0.BotAttackManager.TryPointGetting(withShoot: true, CoverSearchType.shoot_toCover_toBot_Distances, shootPos, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, delegate(CustomNavigationPoint point)
		{
			if ((BotOwner_0.Position - point.Position).sqrMagnitude < d)
			{
				p = point;
				v = true;
			}
		});
		if (v)
		{
			pos = p;
		}
		else
		{
			pos = null;
		}
		return v;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_25()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return method_30();
		}
		if (CustomNavigationPoint_0 == null)
		{
			if (!goalEnemy.IsVisible)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0), "FarestDist1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "FarestDist4");
		}
		if ((CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude < 1f)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0), "FarestDist2");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "FarestDist3");
	}

	public bool method_26()
	{
		if (BotOwner_0.Brain.LastDecision != BotLogicDecision.oneMeleeAttack && (BotOwner_0.Memory.GoalEnemy.Distance > BotOwner_0.Settings.FileSettings.Boss.TAGILLA_SECOND_ASSAULT_RADIUS || method_27(BotOwner_0.Memory.GoalEnemy) || !BotOwner_0.Memory.GoalEnemy.CanShoot || !BotOwner_0.Memory.IsInCover) && BotOwner_0.Memory.LastEnemyTimeSeen + 10f > Time.time)
		{
			return !BotOwner_0.Memory.GoalEnemy.CanShoot;
		}
		return false;
	}

	public bool method_27(EnemyInfo enemy)
	{
		if (enemy != null)
		{
			if (!enemy.IsSuppressed())
			{
				return Float_20 + 7f > Time.time;
			}
			return true;
		}
		return false;
	}

	public bool method_28()
	{
		if (BotOwner_0.Memory.GoalEnemy != null && !(BotOwner_0.Memory.GoalEnemy.Owner == null) && BotOwner_0.Memory.CurCustomCoverPoint != null)
		{
			return !BotOwner_0.Memory.CurCustomCoverPoint.CanIHide(BotOwner_0.Covers.CarePositions(), 1.5f * GClass856.SqrDistance(BotOwner_0.Position, BotOwner_0.Memory.GoalEnemy.CurrPosition), useRaycast: false);
		}
		return false;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_29()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		float kILLA_DEF_DIST_SQRT = BotOwner_0.Settings.FileSettings.Boss.KILLA_DEF_DIST_SQRT;
		CoverSearchData data = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, kILLA_DEF_DIST_SQRT, 0f, CoverSearchType.distToBot, shoot2point, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
		Bool_6 = true;
		CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "DefenceMode");
		}
		if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_CLOSE_ATTACK_DIST)
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return method_30();
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "DefenceMode");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return method_30();
			}
			return method_8(3f, "Def");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "DefenceMode");
	}

	public override AICoreActionEndStruct EndSearch()
	{
		return AICoreActionEndStruct;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_30()
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

	public int method_31(out bool anywayAttack, bool resetCache = false)
	{
		if (Float_21 + 3f > Time.time && !resetCache)
		{
			anywayAttack = Bool_10;
			return Int_4;
		}
		int num = 0;
		List_1.Clear();
		anywayAttack = false;
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			if (Mathf.Abs(enemyInfo.Value.CurrPosition.y - BotOwner_0.Position.y) < BotOwner_0.Settings.FileSettings.Boss.KILLA_Y_DELTA_TO_BE_ENEMY_BOSS)
			{
				bool num2 = enemyInfo.Value.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_DITANCE_TO_BE_ENEMY_BOSS;
				if (enemyInfo.Value.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_ONE_IS_CLOSE)
				{
					anywayAttack = true;
				}
				if (num2)
				{
					num++;
					List_1.Add(enemyInfo.Value);
				}
			}
		}
		Float_21 = Time.time;
		Int_4 = num;
		Bool_10 = anywayAttack;
		return num;
	}

	public bool method_32()
	{
		if (Time.time - BotOwner_0.Memory.LastEnemyTimeSeen > 20f)
		{
			BotOwner_0.Memory.GoalEnemy = null;
			return true;
		}
		return false;
	}

	public bool method_33()
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
				if (!method_22() && goalEnemy.ShallISuppress())
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

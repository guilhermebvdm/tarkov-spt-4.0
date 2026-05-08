using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class GClass62 : BaseLogicLayerSimpleAbstractClass
{
	[CompilerGenerated]
	public class Class213
	{
		public GClass62 gclass62_0;

		public float d;

		public CustomNavigationPoint p;

		public bool v;

		public void method_0(CustomNavigationPoint point)
		{
			if ((gclass62_0.BotOwner_0.Position - point.Position).sqrMagnitude < d)
			{
				p = point;
				v = true;
			}
		}
	}

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public List<EnemyInfo> List_0 = new List<EnemyInfo>();

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public GClass528 Gclass528_0;

	[NonSerialized]
	public GClass412 Gclass412_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_6;

	public GClass62([NotNull] BotOwner owner, int priority)
		: base(owner, priority)
	{
		Gclass528_0 = new GClass528(owner);
		Gclass412_0 = BotOwner_0.BotsController.Bots.GetConnector();
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

	public override void OnActivate()
	{
		BotOwner_0.GetPlayer.GetPlayer.BeingHitAction += method_15;
		if (BotOwner_0.WeaponManager.Grenades != null)
		{
			BotOwner_0.WeaponManager.Grenades.OnGrenadeThrowStart += method_13;
		}
		if (BotOwner_0.SuppressGrenade != null)
		{
			BotOwner_0.SuppressGrenade.OnSupressComplete += method_14;
		}
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			return aICoreActionResultStruct.Value;
		}
		if (Nullable_0.HasValue)
		{
			BotLogicDecision value = Nullable_0.Value;
			Nullable_0 = null;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(value, "Next logic");
		}
		int num = 0;
		List_0.Clear();
		bool flag = false;
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			if (Mathf.Abs(enemyInfo.Value.CurrPosition.y - BotOwner_0.Position.y) < BotOwner_0.Settings.FileSettings.Boss.KILLA_Y_DELTA_TO_BE_ENEMY_BOSS)
			{
				bool num2 = enemyInfo.Value.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_DITANCE_TO_BE_ENEMY_BOSS;
				if (enemyInfo.Value.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_ONE_IS_CLOSE)
				{
					flag = true;
				}
				if (num2)
				{
					num++;
					List_0.Add(enemyInfo.Value);
				}
			}
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && !goalEnemy.IsVisible)
		{
			if (BotOwner_0.SmokeGrenade.ShallShoot())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootToSmoke, "StM");
			}
			if (BotOwner_0.SmokeGrenade.IsInSmoke)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "IsInSmoke");
			}
		}
		if (!flag && num >= BotOwner_0.Settings.FileSettings.Boss.KILLA_ENEMIES_TO_ATTACK)
		{
			return method_18();
		}
		return method_21();
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.GoalEnemy != null;
	}

	public override string Name()
	{
		return "Kill logic";
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (method_17())
		{
			return new AICoreActionEndStruct("Contusion");
		}
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		if (BotOwner_0.Brain.LastDecision.HasValue)
		{
			if (BotOwner_0.Brain.LastDecision == BotLogicDecision.runToEnemy && method_16())
			{
				return new AICoreActionEndStruct("WandSupress");
			}
			if (BotOwner_0.Brain.LastDecision == BotLogicDecision.runToEnemyZigZag)
			{
				float sqrMagnitude = (Vector3_0 - BotOwner_0.Position).sqrMagnitude;
				Vector3_0 = BotOwner_0.Position;
				if (Gclass528_0.CheckIsBadVal(sqrMagnitude, 0.0001f))
				{
					return new AICoreActionEndStruct("CheckBadVal");
				}
			}
		}
		if (method_17())
		{
			return new AICoreActionEndStruct("Contusion");
		}
		if (BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: false))
		{
			return new AICoreActionEndStruct("IsCome");
		}
		return base.EndRunToEnemy();
	}

	public override AICoreActionEndStruct EndDogFight()
	{
		if (method_17())
		{
			return AICoreActionEndStruct_1;
		}
		return base.EndDogFight();
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool flag = false;
		if (!goalEnemy.IsVisible)
		{
			flag = Time.time - goalEnemy.TimeLastSeen > 4f;
		}
		if (method_3())
		{
			return new AICoreActionEndStruct("StartD");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (flag)
		{
			return new AICoreActionEndStruct("deltaEndAtt");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot))
		{
			if (method_7())
			{
				return new AICoreActionEndStruct("CauseTime");
			}
			if (!BotOwner_0.Memory.IsInCover)
			{
				return new AICoreActionEndStruct("!InCover");
			}
			return AICoreActionEndStruct_1;
		}
		Bool_2 = false;
		return new AICoreActionEndStruct("VisibleCanS");
	}

	public void method_14(BotSuppressGrenade obj)
	{
	}

	public void method_15(DamageInfoStruct damageInfo, EBodyPart bodyPart, float damageReducedByArmor)
	{
		if (bodyPart == EBodyPart.Head)
		{
			Float_3 = Time.time;
		}
	}

	public bool method_16()
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
				if (!method_31() && goalEnemy.ShallISuppress())
				{
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	public bool method_17()
	{
		return Time.time - Float_3 < BotOwner_0.Settings.FileSettings.Boss.KILLA_CONTUTION_TIME;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_18()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		float kILLA_DEF_DIST_SQRT = BotOwner_0.Settings.FileSettings.Boss.KILLA_DEF_DIST_SQRT;
		CoverSearchData data = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, kILLA_DEF_DIST_SQRT, 0f, CoverSearchType.distToBot, shoot2point, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
		CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "BotLogicDec");
		}
		if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_CLOSE_ATTACK_DIST)
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return method_19();
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "DefenceMode");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return method_19();
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "DefenceMode");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "Lastoptiond");
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_19()
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
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "Wannashoot");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "ShootFromPl");
	}

	public CoverSearchData method_20()
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

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null && (data.Bot.Position - CustomNavigationPoint_0.Position).sqrMagnitude > 2f)
		{
			return CustomNavigationPoint_0;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_21()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.WeaponManager.Stationary.CheckWantTakeStationary(BotOwner_0.Settings.FileSettings.Cover.STATIONARY_WEAPON_MAX_DIST_TO_USE) != null)
		{
			BotLogicDecision? currentDecision = BotOwner_0.WeaponManager.Stationary.GetCurrentDecision();
			if (currentDecision.HasValue)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(currentDecision.Value, "stationaryW");
			}
		}
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		if (shootPointClass == null)
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "noTrgHold");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "noTrgCover");
		}
		shootPointClass.DistCoef = 0.7f;
		CoverSearchData coverData = method_20();
		float num = Time.time - goalEnemy.TimeLastSeenReal;
		if (!goalEnemy.IsVisible && num > BotOwner_0.Settings.FileSettings.Boss.KILLA_START_SEARCH_SEC)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.search, "KILLASTARTS");
		}
		method_22(coverData, shootPointClass);
		if (!method_3() && !method_17())
		{
			if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_CLOSE_ATTACK_DIST)
			{
				return method_23();
			}
			if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_MIDDLE_ATTACK_DIST)
			{
				return method_24();
			}
			if (goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Boss.KILLA_LARGE_ATTACK_DIST)
			{
				return method_27();
			}
			return method_28();
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "StartD");
	}

	public void method_22(CoverSearchData coverData, ShootPointClass posibleTarget)
	{
		if (!(Float_5 < Time.time))
		{
			return;
		}
		Float_5 = 1f + Time.time;
		CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverData, checkCurrent: false);
		if (CustomNavigationPoint_0 == null)
		{
			return;
		}
		float magnitude = (CustomNavigationPoint_0.Position - posibleTarget.Point).magnitude;
		if (!((BotOwner_0.Position - posibleTarget.Point).magnitude - 1f < magnitude))
		{
			return;
		}
		coverData.shootType = CoverShootType.hide;
		CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverData, checkCurrent: false);
		if (CustomNavigationPoint_0 != null)
		{
			magnitude = (CustomNavigationPoint_0.Position - posibleTarget.Point).magnitude;
			if ((BotOwner_0.Position - posibleTarget.Point).magnitude < magnitude)
			{
				CustomNavigationPoint_0 = null;
			}
		}
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_23()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			Int_1++;
			return method_19();
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "CloseDist");
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_24()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return method_19();
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			float num = Time.time - BotOwner_0.Memory.ComeToCoverTime;
			float num2 = BotOwner_0.Settings.FileSettings.Boss.KILLA_HOLD_DELAY - num;
			if (num2 > 0f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(num2), "bossKillaHo");
			}
		}
		if (Bool_2)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "MidDistendH");
		}
		if (!goalEnemy.IsVisible && Int_1 > BotOwner_0.Settings.FileSettings.Boss.KILLA_CLOSEATTACK_TIMES)
		{
			Int_1 = 0;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(BotOwner_0.Settings.FileSettings.Boss.KILLA_CLOSEATTACK_DELAY), "KILLA_CLOSEATTACK_TIMES");
		}
		if (!goalEnemy.CanShoot && !goalEnemy.IsVisible && method_9())
		{
			BotOwner_0.WeaponManager.Reload.Reload();
			if (BotOwner_0.Memory.IsInCover)
			{
				HoldFor(2f);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "waitReload");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "KILLABULLET");
		}
		if (!method_31() && goalEnemy.ShallISuppress())
		{
			return method_30(grenadePriority: false, method_25());
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_25(), "EnemiesSupp");
	}

	public BotLogicDecision method_25()
	{
		return BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0, BotLogicDecision.runToEnemyZigZag);
	}

	public BotLogicDecision method_26()
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

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_27()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (CustomNavigationPoint_0 != null)
		{
			if (goalEnemy.IsVisible)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "coverInMidd");
			}
			if (BotOwner_0.Memory.IsInCover)
			{
				float num = Time.time - BotOwner_0.Memory.ComeToCoverTime;
				float num2 = BotOwner_0.Settings.FileSettings.Boss.KILLA_HOLD_DELAY - num;
				if (num2 > 0f)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(num2), "FarDist");
				}
			}
			if (!method_31() && goalEnemy.ShallISuppress())
			{
				return method_30(grenadePriority: false, BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0));
			}
			if (Bool_2)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "endHoldEnab");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_26(), "RunIfCan()1");
		}
		if (!method_31() && goalEnemy.ShallISuppress())
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return method_19();
			}
			return method_30(grenadePriority: true, BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0));
		}
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return method_19();
		}
		if (Bool_2)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "endHoldEnab");
		}
		if (!goalEnemy.CanShoot && method_9())
		{
			BotOwner_0.WeaponManager.Reload.Reload();
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "KILLABULLET");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			float num3 = Time.time - BotOwner_0.Memory.ComeToCoverTime;
			float num4 = BotOwner_0.Settings.FileSettings.Boss.KILLA_HOLD_DELAY - num3;
			if (num4 > 0f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(num4), "bossKillaHo");
			}
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0), "tryMoveEn");
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_28()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return method_19();
		}
		if (CustomNavigationPoint_0 == null)
		{
			if (!goalEnemy.IsVisible)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0), "FarestDistc");
			}
		}
		else
		{
			if ((CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude < 1f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0), "FarestDists");
			}
			if (!BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "FarestDist2");
			}
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "FarestDist4");
	}

	public bool method_29(ShootPointClass shootPos, float dist, out CustomNavigationPoint pos)
	{
		if (Float_6 > Time.time)
		{
			pos = null;
			return false;
		}
		Float_6 = Time.time + 1f;
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

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_30(bool grenadePriority, BotLogicDecision doThisIfCant)
	{
		ThrowWeapType? grenadeType = null;
		EnemyInfo enemyInfo = BotOwner_0.Memory.GoalEnemy;
		if (List_0.Count > 1)
		{
			foreach (EnemyInfo item in List_0)
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
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressGrenade, "SupGrenade");
			}
			grenadeType = ThrowWeapType.frag_grenade;
			if (BotOwner_0.WeaponManager.Grenades.HaveGrenadeOfType(grenadeType.Value))
			{
				if (BotOwner_0.SuppressGrenade.Init(enemyInfo, grenadeType, null))
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressGrenade, "SupGrenade2");
				}
			}
			else
			{
				grenadeType = ThrowWeapType.stun_grenade;
				if (BotOwner_0.WeaponManager.Grenades.HaveGrenadeOfType(grenadeType.Value) && BotOwner_0.SuppressGrenade.Init(enemyInfo, grenadeType, null))
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressGrenade, "SupGrenade3");
				}
			}
		}
		ShootPointClass shootPointClass = new ShootPointClass(enemyInfo.EnemyLastPositionReal + BotOwner.STAY_HEIGHT, 0.7f);
		if (GClass369.CanShootToTarget(shootPointClass, BotOwner_0.WeaponRoot.position, BotOwner_0.LookSensor.Mask))
		{
			BotOwner_0.SuppressShoot.Init(enemyInfo);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "SupFire");
		}
		if (!method_29(shootPointClass, BotOwner_0.Settings.FileSettings.Boss.KILLA_DIST_TO_GO_TO_SUPPRESS, out var pos) && pos != null)
		{
			BotOwner_0.SuppressShoot.Init(enemyInfo, pos);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "SupFire2");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(doThisIfCant, "No Sup");
	}

	public bool method_31()
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

	public override void Dispose()
	{
		BotOwner_0.GetPlayer.GetPlayer.BeingHitAction -= method_15;
		BotOwner_0.SuppressGrenade.OnSupressComplete -= method_14;
		BotOwner_0.WeaponManager.Grenades.OnGrenadeThrowStart -= method_13;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class GClass63 : GClass62
{
	[NonSerialized]
	public const float Float_7 = 4f;

	[NonSerialized]
	public const float Float_8 = 60f;

	[NonSerialized]
	public const float Float_9 = 10f;

	[NonSerialized]
	public const float Float_10 = 10f;

	[NonSerialized]
	public GClass466 Gclass466_0;

	[NonSerialized]
	public List<Vector3> List_1 = new List<Vector3>();

	[NonSerialized]
	public NavMeshPath NavMeshPath_0;

	[NonSerialized]
	public float Float_11;

	[NonSerialized]
	public float Float_12 = -10f;

	[NonSerialized]
	public float Float_13;

	[NonSerialized]
	public Vector3 Vector3_1;

	public GClass63([NotNull] BotOwner owner, int priority)
		: base(owner, priority)
	{
		Gclass466_0 = BotOwner_0.WeaponManager.Selector as GClass466;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (List_1.Count > 0)
		{
			BotOwner_0.SuppressShoot.InitToPoints(List_1.ToList());
			float delay = (float)List_1.Count * 2f;
			foreach (Vector3 item in List_1)
			{
				Singleton<BotEventHandler>.Instance.ArtilleryStart(item, 20f, delay);
			}
			Float_11 = Time.time + 60f;
			List_1.Clear();
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "grSuppress");
		}
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
		return method_33();
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null)
		{
			return CustomNavigationPoint_0;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot))
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("enemy.CanSh");
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (method_3())
		{
			return new AICoreActionEndStruct("StartD");
		}
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemynull");
		}
		if (!goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("!enemy.CanS");
		}
		if (!goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("!vision");
		}
		ShootPointClass posibleTarget = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		method_22(method_20(), posibleTarget);
		if (BotOwner_0.WeaponManager.Reload.Reloading && CustomNavigationPoint_0 != null)
		{
			return new AICoreActionEndStruct("!reloading");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		if (BotOwner_0.SuppressShoot.Complete)
		{
			return new AICoreActionEndStruct("SComplete");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_36())
		{
			return new AICoreActionEndStruct("massSpr");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal < 2f)
		{
			Bool_2 = false;
			return new AICoreActionEndStruct("smb seen");
		}
		return base.EndHoldPosition();
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		if (method_36())
		{
			return new AICoreActionEndStruct("massSpr");
		}
		return base.EndShootFromCover();
	}

	public BotLogicDecision method_32()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		NavMeshPath_0 = new NavMeshPath();
		if (NavMesh.CalculatePath(BotOwner_0.Position, goalEnemy.CurrPosition, -1, NavMeshPath_0))
		{
			float num = GClass371.CalculatePathLength(NavMeshPath_0);
			if (!(goalEnemy.Distance * 2.5f > num))
			{
				return BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0, BotLogicDecision.runToEnemyZigZag);
			}
		}
		float sqrMagnitude = (Vector3_1 - goalEnemy.CurrPosition).sqrMagnitude;
		if (!(Time.time - Float_13 < 20f) && sqrMagnitude >= 4f)
		{
			return method_35(goalEnemy);
		}
		return BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0, BotLogicDecision.runToEnemyZigZag);
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_33()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		ShootPointClass posibleTarget = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		float num = Time.time - goalEnemy.TimeLastSeenReal;
		if (!goalEnemy.IsVisible && num > BotOwner_0.Settings.FileSettings.Boss.KILLA_START_SEARCH_SEC)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.search, "KILLASTARTS");
		}
		method_22(method_20(), posibleTarget);
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "StartD");
		}
		return method_34();
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_34()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "Wannashoot5");
		}
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "ShootFromPl8");
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
		if (!goalEnemy.IsVisible && Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal > 30f)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(BotOwner_0.Settings.FileSettings.Boss.KILLA_CLOSEATTACK_DELAY), "KILLA_CLOSEATTACK_TIMES");
		}
		if (!goalEnemy.CanShoot || !goalEnemy.IsVisible)
		{
			if (method_9())
			{
				if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.CanIShootToEnemy)
				{
					BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "toSFC");
				}
			}
			else if (BotOwner_0.WeaponManager.Reload.TryReload() && BotOwner_0.Memory.IsInCover)
			{
				HoldFor(2f);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "waitReload");
			}
		}
		if (!BotOwner_0.Memory.GoalEnemy.IsSuppressed() && goalEnemy.ShallISuppress())
		{
			return method_30(grenadePriority: false, method_32());
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_32(), "EnemiesSupp");
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return new AICoreActionEndStruct("inFight");
	}

	public BotLogicDecision method_35(EnemyInfo enemy)
	{
		Float_13 = Time.time;
		BotOwner_0.SuppressShoot.Init(enemy);
		Vector3_1 = enemy.CurrPosition;
		return BotLogicDecision.suppressFire;
	}

	public bool method_36()
	{
		if (Float_11 > Time.time)
		{
			return false;
		}
		Float_11 = Time.time + 10f;
		if (Gclass466_0.EquipmentSlot != EquipmentSlot.SecondPrimaryWeapon)
		{
			return false;
		}
		int num = 0;
		List_1.Clear();
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			if (enemyInfo.Value.IsVisible)
			{
				num++;
				List_1.Add(enemyInfo.Value.CurrPosition);
			}
		}
		if (num > BotOwner_0.Settings.FileSettings.Boss.BIG_PIPE_ARTILLERY_COUNT)
		{
			return true;
		}
		List_1.Clear();
		return false;
	}
}

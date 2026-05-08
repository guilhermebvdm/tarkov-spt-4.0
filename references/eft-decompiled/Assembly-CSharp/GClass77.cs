using System;
using EFT;
using UnityEngine;

public class GClass77 : GClass76
{
	[NonSerialized]
	public const string String_0 = "BirdEyeAgro";

	[NonSerialized]
	public const string String_1 = "BirdEyeFight";

	[NonSerialized]
	public GClass483 Gclass483_0;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public string String_2;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public bool Bool_4;

	public GClass77(BotOwner bot, int priority, int preferAttackDist = -1, int minStackDist = -1)
		: base(bot, priority)
	{
		String_2 = "BirdEyeFight";
		if (preferAttackDist > 0)
		{
			Int_2 = preferAttackDist;
		}
		if (minStackDist > 0)
		{
			Int_1 = minStackDist;
		}
		Gclass483_0 = BotOwner_0.FindPlaceToShoot.Register(Int_2, Int_1, 0.2f);
		BotOwner_0.BotsGroup.OnMemberRemove += method_18;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			return aICoreActionResultStruct.Value;
		}
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "toDF");
		}
		if (method_20())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "hl");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool canShoot = goalEnemy.CanShoot;
		bool isVisible = goalEnemy.IsVisible;
		method_21();
		float num = Time.time - BotOwner_0.Memory.GoalEnemy.GroupInfo.EnemyLastSeenTimeReal;
		if (Bool_4 && num < 10f)
		{
			if (canShoot && isVisible)
			{
				return method_14("aggSht");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemy, "aggRun");
		}
		if (canShoot && isVisible && !method_17())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "!TA");
		}
		if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.CanIShootToEnemy)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
			if ((CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude > 2f)
			{
				BotOwner_0.Memory.CheckIsInCover2();
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "tc1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, "sfc1");
		}
		if (canShoot && isVisible && goalEnemy.Distance > 30f)
		{
			return method_14("canVis");
		}
		ShootPointClass shootToPoint = new ShootPointClass(goalEnemy.GetPartToShoot());
		return method_13(replaceShoot: false, Gclass483_0, shootToPoint);
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return String_2;
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		AICoreActionEndStruct result = method_19();
		if (result.Value)
		{
			CustomNavigationPoint_0 = null;
		}
		return result;
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		if (Float_6 > Time.time)
		{
			return AICoreActionEndStruct_1;
		}
		method_15(GetShootPoint());
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("noSh");
		}
		if (!goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("noVs");
		}
		if (!method_17())
		{
			return AICoreActionEndStruct_1;
		}
		if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.CanIShootToEnemy)
		{
			return new AICoreActionEndStruct("HC");
		}
		return AICoreActionEndStruct_1;
	}

	public bool method_17()
	{
		if (BotOwner_0.BotsGroup.MembersCount <= 1)
		{
			return false;
		}
		float num = float.MinValue;
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			BotOwner botOwner = BotOwner_0.BotsGroup.Member(i);
			if (botOwner.Id != BotOwner_0.Id)
			{
				float num2 = Time.time - botOwner.ShootData.LastTriggerPressd;
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return num < 5f;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		if (BotOwner_0.Memory.GoalEnemy.Distance < 30f)
		{
			Float_5 = Time.time + 15f;
			return new AICoreActionEndStruct("tooClose");
		}
		return base.EndRunToEnemy();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal > 10f)
		{
			return AICoreActionEndStruct_1;
		}
		method_21();
		if (Bool_4)
		{
			return new AICoreActionEndStruct("doAggro");
		}
		if (method_20())
		{
			return new AICoreActionEndStruct("wntHeal");
		}
		method_15(GetShootPoint());
		if (method_6(out var _))
		{
			return new AICoreActionEndStruct("cShootFC");
		}
		if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.CanIShootToEnemy && (CustomNavigationPoint_0.Position - BotOwner_0.Position).sqrMagnitude > 2f)
		{
			return new AICoreActionEndStruct("wrongC");
		}
		if (CustomNavigationPoint_0 != null && !CustomNavigationPoint_0.CanIShootToEnemy)
		{
			ShootPointClass shootPoint = GetShootPoint();
			if (Gclass483_0.ManualUpdateSearch(shootPoint, 20f, out var _))
			{
				return new AICoreActionEndStruct("havePoint");
			}
			if (Float_7 < Time.time)
			{
				Float_7 = Time.time + 10f;
				if (Gclass483_0.ShootPositionType == EShootPositionType.stand)
				{
					Gclass483_0.Set(EShootPositionType.lay);
				}
				else
				{
					Gclass483_0.Set(EShootPositionType.stand);
				}
			}
		}
		return AICoreActionEndStruct_1;
	}

	public override ShootPointClass GetShootPoint()
	{
		return BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
	}

	public void method_18(BotOwner obj)
	{
		if (BotOwner_0.BotsGroup.MembersCount <= 1)
		{
			method_22(val: false);
		}
	}

	public AICoreActionEndStruct method_19()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot))
		{
			if (BotOwner_0.GoToSomePointData.IsCome())
			{
				return new AICoreActionEndStruct("Come");
			}
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("Enemy");
	}

	public bool method_20()
	{
		if (Time.time - BotOwner_0.Memory.GoalEnemy.GroupInfo.EnemyLastSeenTimeReal < 30f)
		{
			return false;
		}
		if (!BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return BotOwner_0.Medecine.SurgicalKit.HaveWork;
		}
		return true;
	}

	public void method_21()
	{
		if (Float_8 > Time.time)
		{
			return;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal > 10f)
		{
			if (Bool_4)
			{
				Bool_4 = false;
				String_2 = "BirdEyeFight";
			}
			return;
		}
		Float_8 = Time.time + 1f;
		bool val = BotOwner_0.BotsGroup.MembersCount > 0;
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			BotOwner botOwner = BotOwner_0.BotsGroup.Member(i);
			if (botOwner.Id != BotOwner_0.Id)
			{
				if (!botOwner.Memory.HaveEnemy)
				{
					val = false;
					break;
				}
				if (!botOwner.Memory.GoalEnemy.IsVisible)
				{
					val = false;
					break;
				}
			}
		}
		method_22(val);
	}

	public void method_22(bool val)
	{
		if (Bool_4 != val)
		{
			Bool_4 = val;
			if (Bool_4)
			{
				String_2 = "BirdEyeAgro";
			}
			else
			{
				String_2 = "BirdEyeFight";
			}
		}
	}

	public override void Dispose()
	{
		BotOwner_0.BotsGroup.OnMemberRemove -= method_18;
		base.Dispose();
	}
}

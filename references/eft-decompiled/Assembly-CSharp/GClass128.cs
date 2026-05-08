using System;
using EFT;
using UnityEngine;

public class GClass128 : GClass125
{
	[NonSerialized]
	public GClass453<GClass442> Gclass453_0;

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public AIMinePoint AiminePoint_0;

	[NonSerialized]
	public const float Float_4 = 150f;

	[NonSerialized]
	public const float Float_5 = 45f;

	[NonSerialized]
	public float Float_6 = 10f;

	[NonSerialized]
	public float Float_7 = -90f;

	[NonSerialized]
	public float Float_8 = -90f;

	[NonSerialized]
	public float Float_9 = -90f;

	[NonSerialized]
	public GClass459 Gclass459_0;

	[NonSerialized]
	public bool Bool_4;

	public GClass128(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass453_0 = new GClass453<GClass442>(bot);
		Gclass459_0 = new GClass459(bot);
		Gclass453_0.FindBoss();
	}

	public override void OnActivate()
	{
		base.OnActivate();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "first aid");
			}
			if (method_12(20f))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "goforheal");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal now");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.Distance < Float_6)
		{
			if (Float_9 < Time.time)
			{
				BotLogicDecision botLogicDecision = Gclass459_0.DoMeleeAssault();
				if (botLogicDecision == BotLogicDecision.oneMeleeAttack)
				{
					BotOwner_0.Mover.Stop();
					BotOwner_0.BotTalk.Say(EPhraseTrigger.Greetings, sayImmediately: true);
					Float_8 = Time.time;
					Float_9 = Time.time + 150f;
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(botLogicDecision, "cls");
				}
				BotOwner_0.BotTalk.Say(EPhraseTrigger.Greetings, sayImmediately: true);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(botLogicDecision, "cls");
			}
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				method_14();
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "h6");
			}
		}
		Float_7 = Time.time;
		method_16();
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "fr");
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemynull");
		}
		if (!goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("!S");
		}
		if (!goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("!V");
		}
		return AICoreActionEndStruct_1;
	}

	public void method_16()
	{
		float num = Time.time - BotOwner_0.Memory.GoalEnemy.LastChangeVisionTime;
		if ((!BotOwner_0.Memory.GoalEnemy.IsVisible || !(num > 45f)) && Time.time - BotOwner_0.Memory.LastTimeHit > 30f && Float_9 < Time.time)
		{
			BotOwner_0.Memory.GoalEnemy.SetCanShoot(can: false);
		}
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		method_16();
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		float num = Time.time - Float_7;
		if (goalEnemy.Distance < Float_6 && num > 1f)
		{
			return new AICoreActionEndStruct("cls2trg");
		}
		return base.EndGoToEnemy();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		if (Time.time - Float_8 > 15f)
		{
			Float_9 = Time.time + 150f;
			return new AICoreActionEndStruct("tooLong");
		}
		AICoreActionEndStruct result = Gclass459_0.EndOneMeleeAttack();
		if (result.Value)
		{
			Float_9 = Time.time + 150f;
		}
		return result;
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		if (Time.time - BotOwner_0.Memory.LastTimeHit < 40f)
		{
			return false;
		}
		if (!BotOwner_0.WeaponManager.Melee.HaveMelee)
		{
			return false;
		}
		if (Gclass453_0 != null && Gclass453_0.HaveLogic())
		{
			return Gclass453_0.BossLogic.PartisanEnemyType == EBossPartisanEnemyType.ZeroSavage;
		}
		return false;
	}

	public override string Name()
	{
		return "PrtZrSvg";
	}
}

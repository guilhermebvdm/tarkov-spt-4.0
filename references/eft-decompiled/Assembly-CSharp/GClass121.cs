using System;
using EFT;
using UnityEngine;

public class GClass121 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public GClass453<GClass442> Gclass453_0;

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public AIMinePoint AiminePoint_0;

	public GClass121(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass453_0 = new GClass453<GClass442>(bot);
		Gclass25_0 = new GClass25(5f, method_13);
	}

	public void method_13()
	{
		if (!method_14())
		{
			AIMinePoint aIMinePoint = BotOwner_0.MinesData.FindClosestsUnplanted(BotOwner_0.Position);
			if (!(aIMinePoint == null) && !((aIMinePoint.Position - BotOwner_0.Position).sqrMagnitude > 100f))
			{
				AiminePoint_0 = aIMinePoint;
			}
		}
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
		if (method_14())
		{
			float num = BotOwner_0.SDistTo(AiminePoint_0.Position);
			BotOwner_0.MinesData.SetActive(AiminePoint_0);
			if (num < 2.25f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.plantMine, "putMine2");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "to2mn", new GClass30(AiminePoint_0.Position));
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "toEnemy");
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		if (BotOwner_0.GoToSomePointData.IsCome())
		{
			return new AICoreActionEndStruct("Come");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		Gclass25_0.Update();
		if (method_14())
		{
			return new AICoreActionEndStruct("mnCls");
		}
		return base.EndGoToEnemy();
	}

	public bool method_14()
	{
		if (AiminePoint_0 != null)
		{
			return !BotOwner_0.MinesData.IsPlanted(AiminePoint_0);
		}
		return false;
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

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.IsVisible)
		{
			return false;
		}
		if (Time.time - goalEnemy.TimeLastSeenReal < 30f)
		{
			return false;
		}
		if (Gclass453_0 != null && Gclass453_0.HaveLogic())
		{
			return Gclass453_0.BossLogic.PartisanEnemyType == EBossPartisanEnemyType.BadSavage;
		}
		return false;
	}

	public override string Name()
	{
		return "PrtBadTrg";
	}
}

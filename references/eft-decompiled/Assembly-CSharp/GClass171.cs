using System;
using EFT;
using UnityEngine;

public class GClass171 : GClass168
{
	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	public float Single_1 => BotOwner_0.ShootData.LastTriggerPressd;

	public GClass171(BotOwner bot, int priority)
		: base(bot, priority)
	{
		BotOwner_0.BotPersonalStats.OnKillTarget += method_20;
		BotOwner_0.BotPersonalStats.OnHitTarget += method_19;
	}

	public void method_19(IPlayer arg1, DamageInfoStruct arg2, EBodyPart arg3)
	{
		Bool_4 = true;
		Float_3 = Time.time;
	}

	public void method_20(string arg1, DamageInfoStruct arg2)
	{
		BotOwner_0.Memory.Spotted(byHit: false);
	}

	public override void Dispose()
	{
		BotOwner_0.BotPersonalStats.OnHitTarget -= method_19;
		BotOwner_0.BotPersonalStats.OnKillTarget -= method_20;
		base.Dispose();
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		return new AICoreActionEndStruct("noRun");
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!Bool_4 && goalEnemy.CanShoot)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "CanSh");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "inCover");
		}
		if (!Bool_5)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "runOnStart");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.crawl, "crawl_1");
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "FlZryachFight";
	}

	public override AICoreActionEndStruct EndCrawlNode()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			if (Bool_4)
			{
				Bool_4 = false;
			}
			return new AICoreActionEndStruct("inCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Mover.DistDestination < 10f)
		{
			Bool_5 = true;
			return new AICoreActionEndStruct("Close2Cov");
		}
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		return AICoreActionEndStruct_1;
	}
}

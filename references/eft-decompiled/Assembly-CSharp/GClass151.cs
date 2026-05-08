using System;
using EFT;
using UnityEngine;

public class GClass151 : GClass150
{
	[NonSerialized]
	public const float Float_3 = 10f;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public bool Bool_5;

	public GClass151(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override void OnActivate()
	{
		BotOwner_0.GetPlayer.BeingHitAction += method_15;
		base.OnActivate();
	}

	public void SetInside(bool isInside)
	{
		Bool_5 = isInside;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.GrenadeSuicide.Stay)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "Stay");
		}
		if (method_13())
		{
			Bool_4 = true;
		}
		if (Bool_4)
		{
			Float_4 = Time.time + 5f;
			Bool_4 = false;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.grenadeSuicide, "_doGrenade");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "last");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "last");
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		if (BotOwner_0.Boss.Followers.Count > 0)
		{
			return false;
		}
		return true;
	}

	public override string Name()
	{
		return "GrenSuicide";
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		_ = BotOwner_0.Memory.GoalEnemy;
		if (method_13())
		{
			Bool_4 = true;
		}
		if (Bool_4)
		{
			return new AICoreActionEndStruct("_doGrenade");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("!IsInCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		if (BotOwner_0.GrenadeSuicide.Stay)
		{
			return AICoreActionEndStruct_1;
		}
		return base.EndShootFromPlace();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.GrenadeSuicide.Stay)
		{
			return new AICoreActionEndStruct("Stay");
		}
		if (Bool_4 && method_14())
		{
			return new AICoreActionEndStruct("CanDoSuicid");
		}
		return base.EndRunToCover();
	}

	public bool method_13()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return false;
		}
		if (goalEnemy.Distance < 10f && method_14())
		{
			if (!Bool_5)
			{
				return true;
			}
			if (Mathf.Abs(goalEnemy.CurrPosition.y - BotOwner_0.Position.y) < 1f)
			{
				return true;
			}
		}
		return false;
	}

	public bool method_14()
	{
		return Float_4 < Time.time;
	}

	public void method_15(DamageInfoStruct arg1, EBodyPart arg2, float arg3)
	{
		if (BotOwner_0.Memory.IsInCover || Time.time - BotOwner_0.Memory.BotCurrentCoverInfo.LeaveCoverTime < 0.5f)
		{
			Bool_4 = true;
		}
	}

	public override void Dispose()
	{
		BotOwner_0.GetPlayer.BeingHitAction -= method_15;
		base.Dispose();
	}
}

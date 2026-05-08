using System;
using EFT;
using UnityEngine;

public class Class100(BotOwner bot, int priority) : BaseLogicLayerSimpleAbstractClass(bot, priority)
{
	[NonSerialized]
	public float Float_3 = 5f;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public BotTiltType BotTiltType_0 = BotTiltType.right;

	[NonSerialized]
	public float Float_5;

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			Float_4 = Time.time;
			return aICoreActionResultStruct.Value;
		}
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "BotLogicDec");
		}
		if (BotOwner_0.Memory.IsInCover && BotOwner_0.Memory.BotCurrentCoverInfo.ComeToCoverTime - Time.time > Float_3)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.CanShoot && goalEnemy.IsVisible && GClass856.IsTrue100(25f))
		{
			return method_14(BotLogicDecision.shootFromPlace, "sfp8");
		}
		if (goalEnemy.FirstTimeSeen - Time.time > 100f && GClass856.IsTrue100(25f))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.grenadeSuicide, "chanceSuicide");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "t2h");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(GClass856.IsTrue100(50f) ? BotLogicDecision.runToCover : BotLogicDecision.runToCoverZigZag, "rtc4");
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "ObdolbosFight";
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
		method_13();
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		if (method_15())
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			return new AICoreActionEndStruct("endByT1");
		}
		return base.EndShootFromCover();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (Time.time - BotOwner_0.Memory.BotCurrentCoverInfo.ComeToCoverTime > 15f)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			return new AICoreActionEndStruct("endByT2");
		}
		return base.EndHoldPosition();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		if (method_15())
		{
			return new AICoreActionEndStruct("endses");
		}
		if (Time.time - Float_4 > 15f)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			return new AICoreActionEndStruct("endByT3");
		}
		return base.EndShootFromPlace();
	}

	public void method_13()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.IsVisible)
		{
			if (Float_5 > Time.time)
			{
				BotOwner_0.Tilt.Set(BotTiltType_0);
				return;
			}
			Float_5 = Time.time + GClass856.Random(0.4f, 0.7f);
			BotTiltType_0 = ((BotTiltType_0 == BotTiltType.left) ? BotTiltType.right : BotTiltType.left);
		}
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_14(BotLogicDecision d, string reason)
	{
		Float_4 = Time.time;
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(d, reason);
	}

	public bool method_15()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if ((goalEnemy.IsVisible || !(Time.time - goalEnemy.PersonalLastShootTime > 4f)) && BotOwner_0.WeaponManager.HaveBullets)
		{
			return BotOwner_0.WeaponManager.Reload.Reloading;
		}
		return true;
	}
}

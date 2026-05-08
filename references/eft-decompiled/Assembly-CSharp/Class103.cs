using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class Class103 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public const float Float_3 = 50f;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_5;

	public float Single_0
	{
		[CompilerGenerated]
		get
		{
			return Float_5;
		}
		[CompilerGenerated]
		set
		{
			Float_5 = value;
		}
	}

	public Class103(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (Single_0 < 50f)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "wait");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.leaveMap, "do leave");
	}

	public override bool ShallUseNow()
	{
		BotOwner_0.LeaveData.UpdateFromLayer();
		return BotOwner_0.LeaveData.WannaLeave;
	}

	public override string Name()
	{
		return "Leave Map";
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
		if (Float_4 > Time.time)
		{
			return;
		}
		float num = float.MaxValue;
		Float_4 = Time.time + 3f;
		foreach (KeyValuePair<IPlayer, BotSettingsClass> neutral in BotOwner_0.BotsGroup.Neutrals)
		{
			float sqrMagnitude = (neutral.Key.Transform.position - BotOwner_0.Position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
			}
		}
		foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
		{
			float sqrMagnitude2 = (enemyInfo.Key.Transform.position - BotOwner_0.Position).sqrMagnitude;
			if (sqrMagnitude2 < num)
			{
				num = sqrMagnitude2;
			}
		}
		Single_0 = Mathf.Sqrt(num);
	}

	public override AICoreActionEndStruct EndLeaveMap()
	{
		if (Single_0 < 50f)
		{
			return new AICoreActionEndStruct("too close");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (Single_0 > 50f)
		{
			return new AICoreActionEndStruct("playersfar");
		}
		return AICoreActionEndStruct_1;
	}
}

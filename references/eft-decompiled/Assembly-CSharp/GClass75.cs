using System;
using EFT;
using UnityEngine;

public class GClass75(BotOwner bot, int priority) : BaseLogicLayerSimpleAbstractClass(bot, priority)
{
	[NonSerialized]
	public const float Float_3 = 1f;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_4 = float.MinValue;

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.PatrollingData.ExfiltrationData.HaveActions())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToExfiltrationPointNode, "gotoExit");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "holdExf");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		return AICoreActionEndStruct;
	}

	public bool method_13()
	{
		if (!BotOwner_0.Boss.IamBoss && BotOwner_0.BotFollower != null && BotOwner_0.BotFollower.BossToFollow != null)
		{
			IPlayer player = BotOwner_0.BotFollower.BossToFollow.Player();
			if (player != null && player.AIData != null && !(player.AIData.BotOwner == null) && player.AIData.BotOwner.Exfiltration != null)
			{
				return player.AIData.BotOwner.Exfiltration.WannaLeave();
			}
			return BotOwner_0.Exfiltration.WannaLeave();
		}
		return BotOwner_0.Exfiltration.WannaLeave();
	}

	public bool method_14()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override bool ShallUseNow()
	{
		int num;
		if (method_13() && !method_14() && BotOwner_0.PatrollingData.ExfiltrationData.HaveActions())
		{
			if (!(Time.time < Float_4 + 25f) && !(Time.time > Float_4 + 60f))
			{
				num = (BotOwner_0.Exfiltration.BotInExfiltrationArea() ? 1 : 0);
				if (num == 0)
				{
					goto IL_00a4;
				}
			}
			else
			{
				num = 1;
			}
			if (GClass856.SqrDistance(BotOwner_0.Position, Vector3_0) > 1f)
			{
				Vector3_0 = BotOwner_0.Position;
				Float_4 = Time.time;
			}
			return (byte)num != 0;
		}
		num = 0;
		goto IL_00a4;
		IL_00a4:
		BotOwner_0.Exfiltration.ResetLeaveTime();
		return (byte)num != 0;
	}

	public override string Name()
	{
		return "Exfiltration";
	}

	public override AICoreActionEndStruct EndGoToExfiltrationPointNode()
	{
		return AICoreActionEndStruct_1;
	}
}

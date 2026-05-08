using System;
using EFT;
using UnityEngine;

public class BotAssaultBuildingData(BotOwner owner) : GClass429(owner)
{
	[NonSerialized]
	public bool IsActive_1;

	[NonSerialized]
	public GClass641.IBotTimer Timer;

	[NonSerialized]
	public int TaskId = -1;

	[NonSerialized]
	public float WantAssault;

	[NonSerialized]
	public const float ATTACK_PERIOD = 60f;

	[NonSerialized]
	public const float SDIST_TO_START_ATTACK = 625f;

	public bool IsActive => IsActive_1;

	public void StartFor(float period)
	{
		if (IsActive_1 && TaskId > 0)
		{
			BotOwner_0.AITaskManager.CancelDelayedTask(TaskId);
		}
		IsActive_1 = true;
		TaskId = BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, period, method_1);
	}

	public void PeriodCheckStart()
	{
		if (IsActive_1 || WantAssault > Time.time)
		{
			return;
		}
		WantAssault = Time.time + 3f;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null || Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal > 3f)
		{
			return;
		}
		int num = 0;
		float num2 = float.MinValue;
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			BotOwner botOwner = BotOwner_0.BotsGroup.Member(i);
			if (botOwner.Memory.IsInCover)
			{
				num++;
			}
			for (int j = i; j < BotOwner_0.BotsGroup.MembersCount; j++)
			{
				float sqrMagnitude = (BotOwner_0.BotsGroup.Member(j).Position - botOwner.Position).sqrMagnitude;
				if (sqrMagnitude > num2)
				{
					num2 = sqrMagnitude;
				}
			}
		}
		if (num >= BotOwner_0.BotsGroup.MembersCount - 1 && num2 < 625f)
		{
			method_0();
		}
	}

	public void method_0()
	{
		for (int i = 0; i < BotOwner_0.BotsGroup.MembersCount; i++)
		{
			BotOwner_0.BotsGroup.Member(i).AssaultBuildingData.StartFor(60f);
		}
	}

	public void Stop()
	{
		BotOwner_0.AITaskManager.CancelDelayedTask(TaskId);
		IsActive_1 = false;
	}

	public void method_1()
	{
		IsActive_1 = false;
	}

	public void Dispose()
	{
	}
}

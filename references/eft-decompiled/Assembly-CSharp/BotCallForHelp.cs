using System;
using EFT;
using UnityEngine;

public class BotCallForHelp : GClass429
{
	[NonSerialized]
	public const float COMPUTE_PATH_LENGTH_PERIOD = 0.35f;

	[NonSerialized]
	public const float SAVAGE_CALL_PERIOD = 5f;

	[NonSerialized]
	public PanicType OriginalPanicType;

	[NonSerialized]
	public float PathToEnemyLength = -1f;

	[NonSerialized]
	public float PrevComputePathLengthTime;

	[NonSerialized]
	public float LastSavageCallTime;

	[NonSerialized]
	public bool SavagesCalled;

	[NonSerialized]
	public EnemyInfo PrevTarget;

	public BotGlobalsBossSettings BotGlobalsBossSettings_0 => BotOwner_0.Settings.FileSettings.Boss;

	public BotCallForHelp(BotOwner owner)
		: base(owner)
	{
	}

	public void FreeSavages(BotOwner target)
	{
		if (!method_0())
		{
			return;
		}
		foreach (BotOwner botOwner in BotOwner_0.BotsController.Bots.BotOwners)
		{
			if (botOwner != BotOwner_0)
			{
				method_4(botOwner);
			}
		}
		BotOwner_0.Memory.OnGoalEnemyChanged -= FreeSavages;
		SavagesCalled = false;
	}

	public bool method_0()
	{
		if (!BotOwner_0.IsDead && (BotOwner_0.Memory.GoalEnemy == null || BotOwner_0.Memory.GoalEnemy == PrevTarget || PrevTarget != null) && (BotOwner_0.Memory.GoalEnemy == null || PrevTarget == null || !PrevTarget.Owner.IsDead))
		{
			if (BotOwner_0.Memory.GoalEnemy == null)
			{
				return PrevTarget == null;
			}
			return false;
		}
		return true;
	}

	public bool WantCallForSavages()
	{
		if (LastSavageCallTime + 5f > Time.time)
		{
			return false;
		}
		float num = 20f;
		float num2 = method_1();
		if (num2 > num)
		{
			return false;
		}
		if (BotOwner_0.Memory.GoalEnemy != null && (!(num2 > num) || BotOwner_0.Memory.GoalEnemy.CanShoot))
		{
			PrevTarget = BotOwner_0.Memory.GoalEnemy;
			return true;
		}
		return false;
	}

	public void CallForSavages()
	{
		if (LastSavageCallTime + 5f > Time.time || BotOwner_0.Memory.GoalEnemy == null)
		{
			return;
		}
		LastSavageCallTime = Time.time;
		foreach (BotOwner botOwner in BotOwner_0.BotsController.Bots.BotOwners)
		{
			method_3(botOwner);
		}
		if (!SavagesCalled)
		{
			BotOwner_0.Memory.OnGoalEnemyChanged += FreeSavages;
			SavagesCalled = true;
		}
	}

	public float method_1()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			PathToEnemyLength = float.MaxValue;
			return PathToEnemyLength;
		}
		if (PathToEnemyLength < 0f || PrevComputePathLengthTime + 0.35f < Time.time)
		{
			PathToEnemyLength = BotOwner_0.Mover.ComputePathLengthToPoint(goalEnemy.CurrPosition);
			PrevComputePathLengthTime = Time.time;
		}
		return PathToEnemyLength;
	}

	public bool method_2(BotOwner neutral)
	{
		if (!neutral.IsAI)
		{
			return false;
		}
		if (neutral == BotOwner_0)
		{
			return false;
		}
		if (neutral.BotState != EBotState.Active)
		{
			return false;
		}
		if (!neutral.HealthController.IsAlive)
		{
			return false;
		}
		if (GClass856.SqrDistance(BotOwner_0.Transform.position, neutral.Transform.position) >= BotGlobalsBossSettings_0.TAGILLA_SAVAGE_HELP_SQR_DIST)
		{
			return false;
		}
		if (!neutral.Settings.FileSettings.Mind.MAY_BE_CALLED_FOR_HELP)
		{
			return false;
		}
		if (neutral.Brain?.BaseBrain == null)
		{
			return false;
		}
		if (BotSettingsRepoClass.IsInfected(neutral.Profile.Info.Settings.Role))
		{
			return BotOwner_0.Profile.Info.Settings.Role == WildSpawnType.infectedTagilla;
		}
		return true;
	}

	public bool method_3(BotOwner calledBot)
	{
		try
		{
			if (!method_2(calledBot) || BotOwner_0.Memory.GoalEnemy == null)
			{
				return false;
			}
			calledBot.CallForHelp.OriginalPanicType = calledBot.DangerPointsData.PanicType;
			calledBot.DangerPointsData.PanicType = PanicType.none;
			calledBot.Brain.BaseBrain.CalcActionNextFrame();
			CallEnemyTargetInfo trg = new CallEnemyTargetInfo(BotOwner_0.Memory.GoalEnemy.Person.Position, BotOwner_0.Memory.GoalEnemy.Person.AIData.EnvironmentId);
			calledBot.CalledData.TryCall(trg, BotOwner_0);
		}
		catch (Exception)
		{
			_ = calledBot != null;
		}
		return true;
	}

	public void method_4(BotOwner neutral)
	{
		neutral.CalledData.SetOff();
		neutral.DangerPointsData.PanicType = neutral.CallForHelp.OriginalPanicType;
	}
}

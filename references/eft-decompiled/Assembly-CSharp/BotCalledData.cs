using System;
using EFT;
using UnityEngine;

public class BotCalledData : GClass429
{
	[NonSerialized]
	public bool Called;

	[NonSerialized]
	public BotOwner Caller;

	[NonSerialized]
	public bool HelpOnlyWithEnemy;

	[field: NonSerialized]
	public CallEnemyTargetInfo Target { get; set; }

	public Vector3 CallerPosition => Caller.Position;

	public int TargetEnvId => Target.EnvId;

	public bool MayUse
	{
		get
		{
			if (Called && !Caller.IsDead)
			{
				if (HelpOnlyWithEnemy)
				{
					return Caller.Memory.HaveEnemy;
				}
				return true;
			}
			return false;
		}
	}

	public bool CallerGroupHaveEnemy
	{
		get
		{
			if (Caller.Memory.HaveEnemy)
			{
				return true;
			}
			int num = 0;
			while (true)
			{
				if (num < Caller.BotsGroup.MembersCount)
				{
					if (Caller.BotsGroup.Member(num).Memory.HaveEnemy)
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}
	}

	public bool ShouldComeNeartarget()
	{
		if (Caller == null)
		{
			return true;
		}
		if (!Caller.Memory.HaveEnemy)
		{
			return true;
		}
		if (Caller.Memory.GoalEnemy.Person.AIData.EnvironmentId != Target.EnvId)
		{
			return true;
		}
		return false;
	}

	public bool TryGetPriorityEnemyPosition(out Vector3 position, out int idEnv)
	{
		if (Caller.Memory.HaveEnemy)
		{
			position = Caller.Memory.GoalEnemy.CurrPosition;
			idEnv = Caller.Memory.GoalEnemy.Person.AIData.EnvironmentId;
			return true;
		}
		int num = 0;
		BotOwner botOwner;
		while (true)
		{
			if (num < Caller.BotsGroup.MembersCount)
			{
				botOwner = Caller.BotsGroup.Member(num);
				if (botOwner.Memory.HaveEnemy)
				{
					break;
				}
				num++;
				continue;
			}
			position = Vector3.zero;
			idEnv = -1;
			return false;
		}
		position = botOwner.Memory.GoalEnemy.CurrPosition;
		idEnv = botOwner.Memory.GoalEnemy.Person.AIData.EnvironmentId;
		return true;
	}

	public BotCalledData(BotOwner owner)
		: base(owner)
	{
	}

	public bool TryCall(CallEnemyTargetInfo trg, BotOwner caller, bool helpOnlyWithEnemy = true)
	{
		if (BotOwner_0.EnemiesController.IsEnemy(caller.AIData.Player))
		{
			return false;
		}
		if (Caller != null)
		{
			return false;
		}
		if (caller.Memory.GoalEnemy != null)
		{
			IPlayer person = caller.Memory.GoalEnemy.Person;
			if (!BotOwner_0.BotsGroup.Enemies.ContainsKey(person))
			{
				BotOwner_0.BotsGroup.AddEnemy(person, EBotEnemyCause.callBot);
			}
			BotOwner_0.Memory.GoalEnemy = BotOwner_0.EnemiesController.EnemyInfos[person];
			BotOwner_0.Brain.BaseBrain.CalcActionNextFrame();
		}
		HelpOnlyWithEnemy = helpOnlyWithEnemy;
		Caller = caller;
		Caller.BotsGroup.OnEnemyAdd += method_0;
		Target = trg;
		Caller.OnBotStateChange += method_1;
		Caller.BotsGroup.OnMemberAdd += method_2;
		for (int i = 0; i < caller.BotsGroup.MembersCount; i++)
		{
			BotOwner player = caller.BotsGroup.Member(i);
			BotOwner_0.BotsGroup.RemoveEnemy(player, EBotEnemyCause.callForHelp1);
		}
		Called = true;
		return true;
	}

	public void method_0(IPlayer callerEnemy, EBotEnemyCause cause)
	{
		try
		{
			if (!(BotOwner_0 == null) && callerEnemy != null && BotOwner_0.BotState == EBotState.Active)
			{
				if (!BotOwner_0.BotsGroup.Enemies.ContainsKey(callerEnemy))
				{
					BotOwner_0.BotsGroup.AddEnemy(callerEnemy, EBotEnemyCause.callBot);
				}
				if (BotOwner_0.EnemiesController.EnemyInfos.TryGetValue(callerEnemy, out var value))
				{
					BotOwner_0.Memory.GoalEnemy = value;
				}
				BotOwner_0.Brain.BaseBrain.CalcActionNextFrame();
			}
		}
		catch (Exception)
		{
		}
	}

	public void SetOff()
	{
		if (Caller != null)
		{
			Caller.OnBotStateChange -= method_1;
			if (Caller.BotsGroup != null)
			{
				Caller.BotsGroup.OnMemberAdd -= method_2;
				Caller.BotsGroup.OnEnemyAdd -= method_0;
			}
		}
		Caller = null;
		Called = false;
	}

	public void method_1(EBotState obj)
	{
		if (obj == EBotState.Disposed)
		{
			SetOff();
		}
	}

	public void method_2(BotOwner mem)
	{
		BotOwner_0.BotsGroup.RemoveEnemy(mem, EBotEnemyCause.callForHelp2);
	}

	public void Dispose()
	{
		SetOff();
	}
}

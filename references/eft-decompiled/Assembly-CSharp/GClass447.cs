using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using UnityEngine;

public class GClass447 : GClass445<GClass363, GClass361>, GInterface7, GInterface6
{
	[CompilerGenerated]
	private Action action_0;

	[CompilerGenerated]
	private Action action_1;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	public Vector3 LootPosition
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
	}

	public bool ShallAttack => true;

	public override event Action OnPositionsRecalculated
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnThrowRequest
	{
		[CompilerGenerated]
		add
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass447(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
	}

	public bool CanStartUseStimulator()
	{
		return true;
	}

	public void SetFightPosition(bool b, BotOwner owner)
	{
	}

	public void SomebodyUseStimulator()
	{
	}

	public void WantAskHeal(BotOwner owner)
	{
	}

	public void SupressRequestTaken()
	{
	}

	public bool CanStartSuppress()
	{
		return true;
	}

	public bool ISeeEnemy()
	{
		return BotOwner_0.Memory.GoalEnemy?.IsVisible ?? false;
	}

	public bool TooManyEnemies()
	{
		return false;
	}

	public bool IsRunning()
	{
		return !BotOwner_0.Memory.IsInCover;
	}

	public void SetWannaFight()
	{
	}

	public bool IsNearLoot(EnemyInfo enemy)
	{
		return false;
	}

	public override void SetPatrolMode()
	{
		FindPositionsForFollowers(b: true);
	}

	public override void BossLogicUpdate()
	{
	}

	public override Vector3 GetTargetToLook()
	{
		return BotOwner_0.Position;
	}

	public override void SetLogicToFollower(GClass363 followerLogic)
	{
		followerLogic.SetBoss(this);
	}

	public override void FindPositionsForFollowers(bool b)
	{
	}
}

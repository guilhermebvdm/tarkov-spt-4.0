using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass105 : GClass103
{
	public GClass105([NotNull] BotOwner owner, int priority)
		: base(owner, priority)
	{
	}

	public override string Name()
	{
		return "FolKojEnemy";
	}

	public void SetBossKojaniy(GInterface6 bossKojaniy)
	{
		method_13(bossKojaniy);
		BotOwner_0.Memory.OnGoalEnemyChanged += method_29;
		BotOwner_0.Memory.OnBulletNear += method_28;
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveEnemy)
		{
			return BotOwner_0.BotFollower.HaveBoss;
		}
		return false;
	}

	public override Vector3 GetTargetToLook()
	{
		return base.GInterface6_0.GetTargetToLook();
	}

	public override bool ShallAttack()
	{
		if (base.GInterface6_0 == null)
		{
			return true;
		}
		return base.GInterface6_0.ShallAttack;
	}

	public override bool IsEverybodyRun()
	{
		if (base.GInterface6_0 != null && base.GInterface6_0.IsRunning())
		{
			Dictionary<BotOwner, Vector3> dictionary = base.GInterface6_0.FollowersPositions();
			if (dictionary == null)
			{
				return false;
			}
			foreach (BotOwner key in dictionary.Keys)
			{
				if (!(key == null) && key.Id != BotOwner_0.Id)
				{
					if (key.Memory.IsInCover)
					{
						return false;
					}
					if (!key.Mover.Sprinting)
					{
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}

	public void method_28(BotOwner bot, IPlayer source)
	{
		base.GInterface6_0.SetWannaFight();
	}

	public void method_29(BotOwner obj)
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			Nullable_1 = BotOwner_0.Memory.GoalEnemy.GetPartToShoot();
			Float_9 = Time.time;
		}
		else
		{
			IAmReady = false;
		}
	}

	public override void Dispose()
	{
		BotOwner_0.Memory.OnGoalEnemyChanged -= method_29;
		BotOwner_0.Memory.OnBulletNear -= method_28;
		base.Dispose();
	}
}

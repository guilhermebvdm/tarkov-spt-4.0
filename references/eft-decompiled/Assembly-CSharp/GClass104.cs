using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass104 : GClass103
{
	public GClass104([NotNull] BotOwner owner, int priority)
		: base(owner, priority)
	{
	}

	public override void OnActivate()
	{
		if (!(BotOwner_0.Boss.BossLogic is GInterface6 bossData))
		{
			BotOwner_0.Boss.OnBecomeBoss += method_28;
			Debug.LogError("can't activate BossKojaniyFightLogic");
		}
		else
		{
			method_13(bossData);
		}
		base.OnActivate();
	}

	public void method_28(BotOwner obj)
	{
		GInterface6 bossData = BotOwner_0.Boss.BossLogic as GInterface6;
		method_13(bossData);
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "KojaniyB_Enemy";
	}

	public override bool IsEverybodyRun()
	{
		foreach (BotOwner key in base.GInterface6_0.FollowersPositions().Keys)
		{
			if (!key.Memory.IsInCover)
			{
				if (!key.Mover.Sprinting)
				{
					return false;
				}
				continue;
			}
			return false;
		}
		return true;
	}

	public override Vector3 GetTargetToLook()
	{
		return base.GInterface6_0.GetTargetToLook();
	}

	public bool method_29()
	{
		return false;
	}

	public override bool ShallAttack()
	{
		if (base.GInterface6_0 == null)
		{
			return false;
		}
		return base.GInterface6_0.ShallAttack;
	}

	public override void Dispose()
	{
		if (BotOwner_0.Boss != null)
		{
			BotOwner_0.Boss.OnBecomeBoss -= method_28;
		}
		base.Dispose();
	}
}

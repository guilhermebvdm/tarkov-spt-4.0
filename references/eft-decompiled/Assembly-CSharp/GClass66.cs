using EFT;
using UnityEngine;

public class GClass66 : GClass65
{
	public GClass66(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveEnemy)
		{
			return base.GClass435_0.BossShallAttack;
		}
		return false;
	}

	public override string Name()
	{
		return "BossGlFight";
	}

	public override void OnActivate()
	{
		BotOwner_0.GetPlayer.ActiveHealthController.DoPainKiller();
		if (BotOwner_0.Boss.BossLogic is GClass435 boss)
		{
			SetBoss(boss);
		}
		base.OnActivate();
	}

	public override void ManualUpdate()
	{
		bool securityWannaAttack = method_33();
		base.GClass435_0.SecurityWannaAttack = securityWannaAttack;
	}

	public override AICoreActionEndStruct EndSearch()
	{
		if (!base.GClass435_0.BossShallAttack && BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("BossAt");
		}
		if (CanSearchEnemy())
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("CanSearchEn");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot))
		{
			if (base.GClass435_0.BossShallAttack)
			{
				if (Bool_2)
				{
					if (Float_2 < Time.time)
					{
						Bool_2 = false;
						return new AICoreActionEndStruct("BossAt");
					}
					return AICoreActionEndStruct_1;
				}
				return new AICoreActionEndStruct("endHoldEnab");
			}
			if (Bool_2)
			{
				if (Float_2 < Time.time)
				{
					Bool_2 = false;
					return new AICoreActionEndStruct("endHoldEnab");
				}
				return AICoreActionEndStruct_1;
			}
			if (!BotOwner_0.Memory.IsInCover)
			{
				return new AICoreActionEndStruct("!IsInCover");
			}
			return AICoreActionEndStruct_1;
		}
		Bool_2 = false;
		return new AICoreActionEndStruct("endHoldEnab");
	}

	public void method_32()
	{
		if (base.GClass435_0 != null)
		{
			base.GClass435_0.AssaultFollowersShallAttack = true;
		}
	}

	public bool method_33()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (base.GClass435_0.SeenLessTimeBoss(BotOwner_0.Settings.FileSettings.Mind.ATTACK_ENEMY_IF_PROTECT_DELTA_LAST_TIME_SEEN))
		{
			return true;
		}
		if (base.GClass435_0.SeenLessTimeFollowersSecurity(BotOwner_0.Settings.FileSettings.Mind.ATTACK_ENEMY_IF_PROTECT_DELTA_LAST_TIME_SEEN, out var count) && count > 1)
		{
			return true;
		}
		if (base.GClass435_0.AssaultFollowersShallAttack && goalEnemy != null)
		{
			return goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Boss.GLUHAR_BOSS_DIST_TO_ENEMY_WANT_KILL;
		}
		return false;
	}

	public bool method_34()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return true;
		}
		if (goalEnemy.PersonalLastSeenTime <= 1f)
		{
			return true;
		}
		if (Time.time - goalEnemy.PersonalLastSeenTime < 10f)
		{
			return true;
		}
		return false;
	}

	public override void Dispose()
	{
		BotOwner_0.ShootData.OnTriggerPressed -= method_32;
		base.Dispose();
	}
}

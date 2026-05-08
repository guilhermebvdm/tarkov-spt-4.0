using System;
using EFT;
using UnityEngine;

public class BotTacticData : GClass429
{
	public float AggressionCoef = 1f;

	[NonSerialized]
	public bool ShallReturnToAttack;

	[NonSerialized]
	public float ReturnToAttackTime;

	public BotsGroup.BotCurrentTactic Tactic => SubTactic.Tactic;

	[field: NonSerialized]
	public BotSubTactic SubTactic { get; set; }

	public BotTacticData(BotOwner owner)
		: base(owner)
	{
	}

	public bool IsCurTactic(BotsGroup.BotCurrentTactic tactic)
	{
		return SubTactic.Tactic == tactic;
	}

	public void Activate()
	{
		AggressionCoef = GClass856.Random(BotOwner_0.Settings.FileSettings.Mind.MIN_START_AGGRESION_COEF, BotOwner_0.Settings.FileSettings.Mind.MAX_START_AGGRESION_COEF);
		if (BotOwner_0.Boss.IamBoss)
		{
			switch (BotOwner_0.Profile.Info.Settings.Role)
			{
			case WildSpawnType.bossBully:
				SubTactic = new GClass520(BotOwner_0);
				break;
			case WildSpawnType.bossTest:
			case WildSpawnType.bossKilla:
			case WildSpawnType.cursedAssault:
				SubTactic = new GClass522(BotOwner_0);
				break;
			case WildSpawnType.bossGluhar:
				SubTactic = new GClass521(BotOwner_0);
				break;
			default:
				SubTactic = new BotSubTactic(BotOwner_0);
				break;
			case WildSpawnType.followerGluharAssault:
			case WildSpawnType.followerGluharScout:
				SubTactic = new GClass524(BotOwner_0);
				break;
			}
		}
		if (SubTactic == null)
		{
			SubTactic = new BotSubTactic(BotOwner_0);
		}
	}

	public void AggressionChange(float val)
	{
		AggressionCoef = Mathf.Clamp(AggressionCoef + val, LocalBotSettingsProviderClass.Core.MIN_ARG_COEF, LocalBotSettingsProviderClass.Core.MAX_ARG_COEF);
	}

	public void SetTactic(BotsGroup.BotCurrentTactic tactic, bool shallAutoReturnToAttack = false, float delta = -1f)
	{
		if (!ShallReturnToAttack)
		{
			if (shallAutoReturnToAttack && tactic == BotsGroup.BotCurrentTactic.Ambush)
			{
				ShallReturnToAttack = shallAutoReturnToAttack;
				ReturnToAttackTime = SubTactic.SetTactic(tactic, shallAutoReturnToAttack, delta);
			}
			else
			{
				ReturnToAttackTime = SubTactic.SetTactic(tactic);
			}
		}
	}

	public void UpdateChangeTactics()
	{
		if (ShallReturnToAttack && ReturnToAttackTime < Time.time)
		{
			ShallReturnToAttack = false;
			method_0();
		}
	}

	public void method_0()
	{
		SubTactic.SetLastTactic();
	}

	public void Dispose()
	{
		SubTactic?.Dispose();
	}
}

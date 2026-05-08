using System;
using EFT;
using UnityEngine;

public class BotSubTactic : GClass429
{
	[NonSerialized]
	public BotsGroup.BotCurrentTactic LastTactic;

	[field: NonSerialized]
	public BotsGroup.BotCurrentTactic Tactic { get; set; }

	public event Action<BotsGroup.BotCurrentTactic> OnTacticChange;

	public BotSubTactic(BotOwner owner)
		: base(owner)
	{
	}

	public virtual float SetTactic(BotsGroup.BotCurrentTactic tactic, bool shallAutoReturnToAttack = false, float delta = -1f)
	{
		float result = 0f;
		if (shallAutoReturnToAttack)
		{
			if (delta <= 0f)
			{
				delta = GClass856.Random(BotOwner_0.Settings.FileSettings.Cover.RETURN_TO_ATTACK_AFTER_AMBUSH_MIN, BotOwner_0.Settings.FileSettings.Cover.RETURN_TO_ATTACK_AFTER_AMBUSH_MAX);
			}
			result = Time.time + delta;
		}
		if (Tactic == tactic)
		{
			return 0f;
		}
		if (BotOwner_0.Profile.Info != null && BotOwner_0.Profile.Info.Settings != null && BotOwner_0.Profile.Info.Settings.Role == WildSpawnType.marksman)
		{
			tactic = BotsGroup.BotCurrentTactic.Attack;
		}
		LastTactic = Tactic;
		Tactic = tactic;
		if (this.OnTacticChange != null)
		{
			this.OnTacticChange(tactic);
		}
		return result;
	}

	public void SetLastTactic()
	{
		SetTactic(LastTactic);
	}

	public virtual CoverSearchType SearchTypeGoToCover(CoverShootType shootType)
	{
		return CoverSearchType.distToBotAndToCenter;
	}

	public virtual CoverSearchType SearchTypeAttackMoving(CoverShootType shootType)
	{
		return CoverSearchType.distToBotAndToCenter;
	}

	public virtual CoverSearchType SearchTypeAttack(CoverShootType shootType)
	{
		if (shootType == CoverShootType.hide)
		{
			return CoverSearchType.distToBotAndToCenter;
		}
		return CoverSearchType.shoot_toCover_toBot_Distances;
	}

	public virtual CoverSearchType SearchRunToCover(CoverShootType shootType)
	{
		if (shootType == CoverShootType.hide)
		{
			return CoverSearchType.distToBotAndToCenter;
		}
		return CoverSearchType.shoot_toCover_toBot_Distances;
	}

	public virtual CoverSearchType SearchTypeForCover(CoverShootType shootType)
	{
		return CoverSearchType.distToBotAndToCenter;
	}

	public void Dispose()
	{
		this.OnTacticChange = null;
	}
}

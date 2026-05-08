using System;
using EFT;
using UnityEngine;

public class GClass474 : GClass429
{
	[NonSerialized]
	public BotEnemiesController BotEnemiesController_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1 = 2f;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_2 = 5f;

	public bool Boolean_0 => BotOwner_0.Settings.FileSettings.Shoot.MISS_AFTER_SPRINT;

	public bool Boolean_1 => BotOwner_0.Settings.FileSettings.Shoot.MISS_TO_HEAD;

	public bool Boolean_2 => BotOwner_0.Settings.FileSettings.Shoot.MISS_ON_MOVE;

	public bool Boolean_3 => BotOwner_0.Settings.FileSettings.Shoot.MISS_ON_TRANSITION;

	public float Single_0 => BotOwner_0.Settings.FileSettings.Shoot.MISS_ON_CRITICAL_DIST;

	public GClass474(BotEnemiesController botEnemiesController, BotOwner owner)
		: base(owner)
	{
		BotEnemiesController_0 = botEnemiesController;
	}

	public void Activate()
	{
		BotOwner_0.Mover.OnWayChanged += method_0;
	}

	public void method_0(bool obj)
	{
		if (!BotOwner_0.Mover.HasPathAndNoComplete && BotOwner_0.Mover.Sprinting && !(Time.time - Float_0 < Float_1) && Boolean_0)
		{
			Float_0 = Time.time;
			Int_0 = Mathf.Clamp(Int_0 + 2, 0, 2);
		}
	}

	public void Dispose()
	{
		if (!(BotOwner_0 == null) && BotOwner_0.Mover != null)
		{
			BotOwner_0.Mover.OnWayChanged += method_0;
		}
	}

	public bool ShouldApplyDamage(Player target, DamageInfoStruct damage, EBodyPart bodyPartType)
	{
		if (!(target == null) && damage.Player != null)
		{
			if (target.IsAI)
			{
				return true;
			}
			if (damage.DamageType != EDamageType.Bullet)
			{
				return true;
			}
			if (!Bool_0 && Int_1 == damage.FireIndex)
			{
				return false;
			}
			Int_1 = damage.FireIndex;
			if (!BotOwner_0.IsDead && BotOwner_0.BotState == EBotState.Active)
			{
				if (BotEnemiesController_0 != null && BotEnemiesController_0.EnemyInfos.TryGetValue(target, out var value))
				{
					EPlayerState name = BotOwner_0.GetPlayer.MovementContext.CurrentState.Name;
					if (Boolean_3 && value.Distance > Single_0)
					{
						if (name == EPlayerState.Transit2Prone || name == EPlayerState.Prone2Stand)
						{
							return false;
						}
						if (name == EPlayerState.Transition && BotOwner_0.GetPlayer.PoseLevel <= 1f)
						{
							return false;
						}
					}
					if (Int_0 > 0 && Time.time - Float_0 < Float_1 && value.Distance > Single_0)
					{
						Int_0--;
						return false;
					}
					if (Boolean_1 && bodyPartType == EBodyPart.Head && value.Distance > Single_0)
					{
						return false;
					}
					if (Boolean_2 && BotOwner_0.Mover.HasPathAndNoComplete && value.Distance > Float_2)
					{
						return false;
					}
					if (method_1(value, damage))
					{
						value.MissRemain--;
						if (value.MissRemain < 0)
						{
							value.MissRemain = 0;
						}
						Bool_0 = true;
						return false;
					}
					Bool_0 = false;
					return true;
				}
				return true;
			}
			return true;
		}
		return true;
	}

	public bool method_1(EnemyInfo info, DamageInfoStruct damage)
	{
		if (info.MissRemain <= 0)
		{
			return false;
		}
		if (damage.DamageType != EDamageType.Bullet)
		{
			return false;
		}
		if (info.Distance <= BotOwner_0.Settings.FileSettings.Aiming.MISS_DIST)
		{
			return false;
		}
		if (info.PersonalShoot <= BotOwner_0.Settings.FileSettings.Aiming.MISS_FIRST_SOOTS)
		{
			return true;
		}
		return false;
	}
}

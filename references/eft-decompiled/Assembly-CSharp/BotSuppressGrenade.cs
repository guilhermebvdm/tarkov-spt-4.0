using System;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

public class BotSuppressGrenade : GClass519
{
	public event Action<BotSuppressGrenade> OnSupressComplete;

	public BotSuppressGrenade([NotNull] BotOwner owner)
		: base(owner)
	{
	}

	public bool Init(EnemyInfo enemyToSuppress, ThrowWeapType? grenadeType, CustomNavigationPoint posToDo, AIGreandeAng throwAngle = AIGreandeAng.ang45)
	{
		if (Init(enemyToSuppress, posToDo, throwAngle))
		{
			BotOwner_0.WeaponManager.Grenades.AIGreanageThrowData.GrenadeType = grenadeType;
			return true;
		}
		return false;
	}

	public bool Init(EnemyInfo enemyToSuppress, CustomNavigationPoint fromCurPos, AIGreandeAng throwAngle = AIGreandeAng.ang45)
	{
		method_0(enemyToSuppress, fromCurPos);
		Vector3? point = GetPoint();
		if (!point.HasValue)
		{
			return false;
		}
		AIGreanageThrowData aIGreanageThrowData = GClass577.CanThrowGrenade2(BotOwner_0.Position, point.Value, BotOwner_0.WeaponManager.Grenades, throwAngle, BotOwner_0.Settings.FileSettings.Grenade.MIN_THROW_GRENADE_DIST_SQRT);
		if (!aIGreanageThrowData.CanThrow)
		{
			return false;
		}
		BotOwner_0.WeaponManager.Grenades.SetThrowData(aIGreanageThrowData);
		return true;
	}

	public bool Init(Vector3 targetPosition, AIGreandeAng throwAngle = AIGreandeAng.ang45)
	{
		AIGreanageThrowData aIGreanageThrowData = GClass577.CanThrowGrenade2(BotOwner_0.Position, targetPosition, BotOwner_0.WeaponManager.Grenades, throwAngle, BotOwner_0.Settings.FileSettings.Grenade.MIN_THROW_GRENADE_DIST_SQRT);
		if (!aIGreanageThrowData.CanThrow)
		{
			return false;
		}
		BotOwner_0.WeaponManager.Grenades.SetThrowData(aIGreanageThrowData);
		return true;
	}

	public override void Activate()
	{
		BotOwner_0.WeaponManager.Grenades.OnGrenadeThrowComplete += method_3;
	}

	public void method_3(ThrowWeapItemClass throwWeap)
	{
		if (Bool_0)
		{
			float delta = ((throwWeap.ThrowType == ThrowWeapType.frag_grenade) ? BotOwner_0.Settings.FileSettings.Grenade.SMOKE_SUPPRESS_DELTA : ((throwWeap.ThrowType == ThrowWeapType.stun_grenade || throwWeap.ThrowType == ThrowWeapType.flash_grenade) ? BotOwner_0.Settings.FileSettings.Grenade.STUN_SUPPRESS_DELTA : BotOwner_0.Settings.FileSettings.Grenade.DAMAGE_GRENADE_SUPPRESS_DELTA));
			method_2(delta);
			if (this.OnSupressComplete != null)
			{
				this.OnSupressComplete(this);
			}
		}
	}

	public override void Dispose()
	{
		BotOwner_0.WeaponManager.Grenades.OnGrenadeThrowComplete -= method_3;
	}
}

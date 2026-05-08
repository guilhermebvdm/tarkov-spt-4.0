using System;
using EFT;
using UnityEngine;

public abstract class GClass178 : GClass177<GClass27>
{
	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public GClass275 Gclass275_0;

	public virtual bool _checkIsReady => true;

	public bool IsReady
	{
		get
		{
			if (GetTarget().HasValue)
			{
				return BotOwner_0.AimingManager.CurrentAiming.IsReady;
			}
			return false;
		}
	}

	public GClass178(BotOwner bot)
		: base(bot)
	{
		Gclass275_0 = new GClass275(bot);
	}

	public override void UpdateNodeByBrain(GClass27 data)
	{
		IBotAiming currentAiming = BotOwner_0.AimingManager.CurrentAiming;
		Vector3? trg;
		if (data != null)
		{
			trg = data.PointToShoot;
		}
		else
		{
			trg = GetTarget();
		}
		trg = method_0(trg);
		BotOwner_0.BotLight.TurnOn(currentAiming.AlwaysTurnOnLight);
		if (trg.HasValue)
		{
			Vector3_0 = trg.Value;
			bool flag = true;
			if (_checkIsReady)
			{
				flag = currentAiming.IsReady;
			}
			if (flag)
			{
				ReadyToShoot();
				Gclass275_0.UpdateNodeByBrain(data);
			}
		}
	}

	public virtual void ReadyToShoot()
	{
	}

	public virtual Vector3? GetTarget()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && ((goalEnemy.CanShoot && goalEnemy.IsVisible) || BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive))
		{
			return (!(goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Aiming.DIST_TO_SHOOT_TO_CENTER)) ? goalEnemy.GetPartToShoot() : goalEnemy.GetBodyPartPosition();
		}
		Vector3? result = null;
		if (BotOwner_0.Memory.LastEnemy != null)
		{
			result = BotOwner_0.Memory.LastEnemy.CurrPosition + Vector3.up * BotOwner_0.Settings.FileSettings.Aiming.DANGER_UP_POINT;
		}
		return result;
	}

	public Vector3? method_0(Vector3? trg)
	{
		if (trg.HasValue)
		{
			Vector3_0 = trg.Value;
			if (Float_0 < Time.time)
			{
				Float_0 = Time.time + GClass856.Random(5f, 8f);
				BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnFight, withGroupDelay: true);
			}
			if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive)
			{
				Vector3_0 -= (Vector3_0 - BotOwner_0.Position).normalized;
			}
			BotOwner_0.AimingManager.CurrentAiming.SetTarget(Vector3_0);
			BotOwner_0.AimingManager.NodeUpdate();
			return Vector3_0;
		}
		return null;
	}

	public Vector3 method_1(Vector3 part, Player player)
	{
		return part;
	}
}

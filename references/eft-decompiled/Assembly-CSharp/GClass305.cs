using System;
using EFT;
using UnityEngine;

public class GClass305 : GClass177<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Vector3? Nullable_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public GClass178 Gclass178_0;

	public GClass305(BotOwner bot)
		: base(bot)
	{
		Gclass178_0 = new GClass183(bot);
	}

	public override void Awake()
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (!Nullable_0.HasValue)
		{
			Nullable_0 = BotOwner_0.Position;
			return;
		}
		if (BotOwner_0.WeaponManager.Stationary.CurLink == null)
		{
			method_0();
			return;
		}
		if (BotOwner_0.WeaponManager.Stationary.Taken)
		{
			Gclass178_0.UpdateNodeByBrain(data as GClass27);
		}
		else
		{
			GoToStationary();
		}
		BotOwner_0.Sprint(Bool_0);
		BotOwner_0.SetTargetMoveSpeed(1f);
	}

	public void method_0()
	{
		StationaryWeaponLink stationaryWeaponLink = BotOwner_0.WeaponManager.Stationary.CheckWantTakeStationary(BotOwner_0.Settings.FileSettings.Cover.STATIONARY_WEAPON_MAX_DIST_TO_USE, igoneEnemy: true);
		if (stationaryWeaponLink != null)
		{
			BotOwner_0.WeaponManager.Stationary.SetTargetStationary(stationaryWeaponLink);
		}
	}

	public void method_1()
	{
		if (Float_1 < Time.time)
		{
			Bool_1 = !Bool_1;
			BotOwner_0.ShootData.ShootController.SetTriggerPressed(Bool_1);
			Float_1 = Time.time + (Bool_1 ? 20f : 30f);
		}
	}

	public void method_2()
	{
		if (BotOwner_0.WeaponManager.Stationary.CurLink == null)
		{
			StationaryWeaponLink stationaryWeaponLink = BotOwner_0.WeaponManager.Stationary.CheckWantTakeStationary(BotOwner_0.Settings.FileSettings.Cover.STATIONARY_WEAPON_MAX_DIST_TO_USE, igoneEnemy: true);
			if (stationaryWeaponLink != null)
			{
				BotOwner_0.WeaponManager.Stationary.SetTargetStationary(stationaryWeaponLink);
			}
			return;
		}
		if (!BotOwner_0.WeaponManager.Stationary.CurLink.Weapon.IsOperator(BotOwner_0.Profile.Id))
		{
			BotOwner_0.WeaponManager.Stationary.DropCurWeapon(withDelay: false);
		}
		BotOwner_0.GoToPoint(Nullable_0.Value);
		float magnitude = (Nullable_0.Value - BotOwner_0.Position).magnitude;
		BotOwner_0.Steering.LookToMovingDirection();
		BotOwner_0.SetPose(1f);
		BotOwner_0.Mover.SetTargetMoveSpeed(Mathf.Clamp(magnitude, 0.1f, 1f));
		if (BotOwner_0.WeaponManager.Stationary.CurLink != null && magnitude < 0.8f)
		{
			BotOwner_0.WeaponManager.Stationary.Take();
		}
	}

	public void method_3()
	{
		if (BotOwner_0.WeaponManager.Stationary.CurLink != null && BotOwner_0.WeaponManager.Stationary.CurLink.Weapon.IsOperator(BotOwner_0.Profile.Id))
		{
			BotOwner_0.WeaponManager.Stationary.DropCurWeapon(withDelay: false);
		}
		Float_2 = Time.time + 1f;
	}

	public void method_4()
	{
		if (BotOwner_0.WeaponManager.Stationary.CurLink == null)
		{
			StationaryWeaponLink stationaryWeaponLink = BotOwner_0.WeaponManager.Stationary.CheckWantTakeStationary(BotOwner_0.Settings.FileSettings.Cover.STATIONARY_WEAPON_MAX_DIST_TO_USE, igoneEnemy: true);
			if (stationaryWeaponLink != null)
			{
				BotOwner_0.WeaponManager.Stationary.SetTargetStationary(stationaryWeaponLink);
			}
			return;
		}
		BotOwner_0.StopMove();
		bool locked = BotOwner_0.WeaponManager.Stationary.CurLink.Weapon.Locked;
		if (BotOwner_0.WeaponManager.Stationary.CurLink.Weapon.IsAvailable(BotOwner_0.Profile.Id) && !locked && Float_0 < Time.time)
		{
			Float_0 = Time.time + 1f;
			BotOwner_0.WeaponManager.Stationary.Take();
		}
	}

	public virtual void GoToStationary()
	{
		if (BotOwner_0.WeaponManager.Stationary.CurLink == null)
		{
			StationaryWeaponLink stationaryWeaponLink = BotOwner_0.WeaponManager.Stationary.CheckWantTakeStationary(BotOwner_0.Settings.FileSettings.Cover.STATIONARY_WEAPON_MAX_DIST_TO_USE, igoneEnemy: true);
			if (stationaryWeaponLink != null)
			{
				BotOwner_0.WeaponManager.Stationary.SetTargetStationary(stationaryWeaponLink);
			}
			return;
		}
		float magnitude = (BotOwner_0.WeaponManager.Stationary.CurLink.Weapon.OperatorPosition - BotOwner_0.Position).magnitude;
		BotOwner_0.Steering.LookToMovingDirection();
		BotOwner_0.Mover.SetTargetMoveSpeed(Mathf.Clamp(magnitude, 0.1f, 1f));
		if (magnitude < 0.3f)
		{
			BotOwner_0.WeaponManager.Stationary.Take();
		}
		else
		{
			BotOwner_0.GoToPoint(BotOwner_0.WeaponManager.Stationary.CurLink.Weapon.OperatorPosition);
		}
	}
}

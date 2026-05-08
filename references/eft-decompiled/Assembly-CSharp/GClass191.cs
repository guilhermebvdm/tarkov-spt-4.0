using System;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass191 : GClass177<GClass26>
{
	public ChristmasTreePoI ChristmasTreePoI_0 => BotOwner_0.GameEventsData.KhorovodGameEvent.ChristmasTree;

	public GClass191(BotOwner bot)
		: base(bot)
	{
	}

	public void method_0(DamageInfoStruct di, EBodyPart bp, float t)
	{
		ChristmasTreePoI_0?.ResetEnemiesIgnoreState(di.Player.iPlayer);
	}

	public void method_1(Grenade grenade)
	{
		ChristmasTreePoI_0?.ResetEnemiesIgnoreState(grenade.Player.iPlayer);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.DoorOpener.UpdateDoorInteractionStatus();
		if (!ChristmasTreePoI_0.IsNear(BotOwner_0))
		{
			BotOwner_0.GoToSomePointData.SetPoint(ChristmasTreePoI_0.InitialPosition());
			BotOwner_0.GoToSomePointData.UpdateToGo(sprint: false);
			BotOwner_0.Steering.LookToMovingDirection();
			return;
		}
		if (!BotOwner_0.GameEventsData.KhorovodGameEvent.InKhorovod)
		{
			BotOwner_0.GetPlayer.BeingHitAction += method_0;
			BotBewareGrenade bewareGrenade = BotOwner_0.BewareGrenade;
			bewareGrenade.OnBewareGrenade = (Action<Grenade>)Delegate.Combine(bewareGrenade.OnBewareGrenade, new Action<Grenade>(method_1));
			BotOwner_0.GameEventsData.KhorovodGameEvent.OnEnterKhorovod();
		}
		BotOwner_0.GoToSomePointData.SetPoint(ChristmasTreePoI_0.GetPoint(BotOwner_0));
		BotOwner_0.GoToSomePointData.UpdateToGo(sprint: false, ChristmasTreePoI_0.GetMoveVelocity());
		Vector3 lookDirection = BotOwner_0.Steering.LookDirection;
		Vector3 normalized = (ChristmasTreePoI_0.GetLookPoint() - BotOwner_0.Position).normalized;
		BotOwner_0.Steering.LookToDirection(Vector3.Lerp(lookDirection, normalized, ChristmasTreePoI_0.LookDirectionDelta));
		if (ChristmasTreePoI_0.HaveGesture)
		{
			BotOwner_0.Gesture.TryGestus(ChristmasTreePoI_0.Gesture, withAimingDelay: false);
		}
		if (ChristmasTreePoI_0.HavePhrase(BotOwner_0))
		{
			BotOwner_0.BotTalk.Say(ChristmasTreePoI_0.Phrase);
		}
		if (ChristmasTreePoI_0.Shooting(BotOwner_0))
		{
			BotOwner_0.ShootData.Shoot();
		}
		if (ChristmasTreePoI_0.ForcedAutomaticFireMode)
		{
			BotOwner_0.WeaponManager.CurrentWeapon.FireMode.SetFireMode(Weapon.EFireMode.fullauto);
		}
	}

	public new void Dispose()
	{
		BotOwner_0.GetPlayer.BeingHitAction -= method_0;
	}
}

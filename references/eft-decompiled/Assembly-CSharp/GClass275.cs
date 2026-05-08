using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using UnityEngine;

public class GClass275 : GClass177<GClass26>
{
	public float SilentUntil;

	public bool WithTalk = true;

	[NonSerialized]
	public int Int_0;

	[CompilerGenerated]
	private Action action_0;

	public event Action OnShootDone
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass275(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive)
		{
			if (BotOwner_0.WeaponManager.UnderbarrelLauncherController.NeedToReload() && !BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryReload())
			{
				BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryDisable();
				return;
			}
		}
		else if (!BotOwner_0.WeaponManager.HaveBullets)
		{
			BotOwner_0.WeaponManager.Reload.TryReload();
		}
		Vector3 position = BotOwner_0.GetPlayer.PlayerBones.WeaponRoot.position;
		Vector3 realTargetPoint = BotOwner_0.AimingManager.CurrentAiming.RealTargetPoint;
		if (BotOwner_0.ShootData.CheckFriendlyFire(position, realTargetPoint))
		{
			BotOwner_0.ShootData.EndShoot();
			return;
		}
		if (WithTalk)
		{
			method_0();
		}
		if (BotOwner_0.ShootData.Shoot())
		{
			if (Int_0 > BotOwner_0.WeaponManager.Reload.BulletCount)
			{
				Int_0 = BotOwner_0.WeaponManager.Reload.BulletCount;
			}
			action_0?.Invoke();
		}
	}

	public void method_0()
	{
		if (!(SilentUntil > Time.time))
		{
			if (GClass856.IsTrue100(50f))
			{
				BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnFight, withGroupDelay: true);
			}
			SilentUntil = Time.time + 15f;
		}
	}
}

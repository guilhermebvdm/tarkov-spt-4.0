using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass274 : GClass177<GClass26>
{
	public GClass274(BotOwner bot)
		: base(bot)
	{
	}

	public bool UpdateTryThrow()
	{
		BotGrenadeController grenades = BotOwner_0.WeaponManager.Grenades;
		if (!grenades.HaveGrenade)
		{
			return false;
		}
		if (!BotOwner_0.Settings.FileSettings.Grenade.CAN_THROW_STRAIGHT_CONTACT)
		{
			return false;
		}
		bool flag;
		if ((flag = BotOwner_0.Memory.GoalEnemy != null) && Time.time - BotOwner_0.Memory.EnemySetTime < BotOwner_0.Settings.FileSettings.Grenade.STRAIGHT_CONTACT_DELTA_SEC)
		{
			return false;
		}
		if (grenades.ReadyToThrow && grenades.AIGreanageThrowData.IsUpToDate())
		{
			if (BotOwner_0.Settings.FileSettings.Grenade.STOP_WHEN_THROW_GRENADE)
			{
				BotOwner_0.StopMove();
			}
			BotOwner_0.WeaponManager.Grenades.DoThrow();
			return true;
		}
		if (flag)
		{
			if (Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(BotOwner_0.Memory.GoalEnemy.Person.ProfileId).IsSprintEnabled)
			{
				return false;
			}
			if (Time.time - BotOwner_0.Memory.GoalEnemy.FirstTimeSeen > BotOwner_0.Settings.FileSettings.Grenade.FIRST_TIME_SEEN_DELTA_CAN_THROW)
			{
				grenades.CanThrowGrenade(BotOwner_0.Memory.GoalEnemy.CurrPosition + Vector3.up);
				return false;
			}
		}
		return false;
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
	}
}

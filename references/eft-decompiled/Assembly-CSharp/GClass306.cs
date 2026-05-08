using EFT;

public class GClass306 : GClass305
{
	public GClass306(BotOwner bot)
		: base(bot)
	{
	}

	public override void GoToStationary()
	{
		if (BotOwner_0.WeaponManager.Stationary.CurLink == null)
		{
			StationaryWeaponLink stationaryWeaponLink = BotOwner_0.WeaponManager.Stationary.CheckWantTakeStationary(BotOwner_0.Settings.FileSettings.Cover.STATIONARY_WEAPON_MAX_DIST_TO_USE, igoneEnemy: true);
			if (stationaryWeaponLink != null)
			{
				BotOwner_0.WeaponManager.Stationary.SetTargetStationary(stationaryWeaponLink);
			}
		}
		else
		{
			BotOwner_0.WeaponManager.Stationary.Take();
		}
	}
}

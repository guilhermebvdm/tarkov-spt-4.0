public abstract class GClass2190
{
	public static bool IsBossOrFollower(this ProfileInfoSettingsClass settings)
	{
		return BotSettingsRepoClass.IsBossOrFollower(settings.Role);
	}

	public static bool IsBoss(this ProfileInfoSettingsClass settings)
	{
		return BotSettingsRepoClass.IsBoss(settings.Role);
	}

	public static bool IsFollower(this ProfileInfoSettingsClass settings)
	{
		return BotSettingsRepoClass.IsFollower(settings.Role);
	}
}

using EFT;

[GAttribute29]
public class ProfileInfoSettingsClass
{
	public WildSpawnType Role = WildSpawnType.assault;

	public BotDifficulty BotDifficulty = BotDifficulty.normal;

	public int Experience = -1;

	public double StandingForKill;

	public double AggressorBonus;

	public bool UseSimpleAnimator;

	public void TryChangeRoleToAssaultGroup()
	{
		if (Role == WildSpawnType.assault)
		{
			Role = WildSpawnType.assaultGroup;
		}
	}
}

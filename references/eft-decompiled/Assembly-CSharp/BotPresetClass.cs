using EFT;

public class BotPresetClass
{
	public BotDifficulty BotDifficulty;

	public WildSpawnType Role;

	public bool UseThis;

	public float VisibleAngle;

	public float VisibleDistance;

	public float VISIBILITY_CHANGE_SPEED;

	public float ScatteringPerMeter;

	public float HearingSense;

	public float SCATTERING_DIST_MODIF;

	public float MAX_AIMING_UPGRADE_BY_TIME;

	public float FIRST_CONTACT_ADD_SEC;

	public float COEF_IF_MOVE;

	public string LogDebugInfo()
	{
		return "Difficulty:" + BotDifficulty.ToString() + " Role:" + Role;
	}
}

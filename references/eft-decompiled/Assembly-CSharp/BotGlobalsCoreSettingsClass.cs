public class BotGlobalsCoreSettingsClass
{
	public float VisibleAngle = 110f;

	public float VisibleDistance = 137f;

	public float ScatteringPerMeter = 0.08f;

	public float ScatteringClosePerMeter = 0.12f;

	public float DamageCoeff = 1.2f;

	public float HearingSense = 0.65f;

	public bool CanRun = true;

	public bool CanGrenade = true;

	public AimingType AimingType;

	public float PistolFireDistancePref = 35f;

	public float ShotgunFireDistancePref = 50f;

	public float RifleFireDistancePref = 100f;

	public float AccuratySpeed = 0.3f;

	public float WaitInCoverBetweenShotsSec = 2f;

	public BotGlobalsCoreSettingsClass()
	{
	}

	public BotGlobalsCoreSettingsClass(BotGlobalsCoreSettingsClass core)
	{
		VisibleAngle = core.VisibleAngle;
		VisibleDistance = core.VisibleDistance;
		ScatteringPerMeter = core.ScatteringPerMeter;
		DamageCoeff = core.DamageCoeff;
		HearingSense = core.HearingSense;
		CanRun = core.CanRun;
		CanGrenade = core.CanGrenade;
		AimingType = core.AimingType;
		PistolFireDistancePref = core.PistolFireDistancePref;
		ShotgunFireDistancePref = core.ShotgunFireDistancePref;
		RifleFireDistancePref = core.RifleFireDistancePref;
		AccuratySpeed = core.AccuratySpeed;
	}
}

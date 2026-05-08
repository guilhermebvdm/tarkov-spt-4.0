using UnityEngine;

public class GClass579
{
	public float TimeDamage;

	public Vector3 Postion;

	public GClass579(Vector3 postion)
	{
		Postion = postion;
		TimeDamage = Time.time;
	}

	public bool IsActive()
	{
		return Time.time - TimeDamage < LocalBotSettingsProviderClass.Core.LAST_DAMAGE_ACTIVE;
	}
}

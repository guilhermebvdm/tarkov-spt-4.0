using UnityEngine;

public class GrenadeSettings : ThrowableSettings
{
	public enum CollisionSounds
	{
		frag,
		smoke,
		stun,
		smokeM18,
		stunM7920
	}

	public CollisionSounds CollisionSound;

	public Transform Skoba;

	public string EmmisionEffect => base.name.Replace("(Clone)", "");
}

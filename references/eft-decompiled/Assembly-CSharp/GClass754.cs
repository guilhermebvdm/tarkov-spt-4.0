using UnityEngine;

public abstract class GClass754
{
	public static float GetMassNormalizedKineticEnergy(this Rigidbody rigidbody)
	{
		return (0.5f * rigidbody.mass * Mathf.Pow(rigidbody.velocity.magnitude, 2f) + 0.5f * rigidbody.inertiaTensor.x * Mathf.Pow(rigidbody.angularVelocity.x, 2f) + 0.5f * rigidbody.inertiaTensor.y * Mathf.Pow(rigidbody.angularVelocity.y, 2f) + 0.5f * rigidbody.inertiaTensor.z * Mathf.Pow(rigidbody.angularVelocity.z, 2f)) / rigidbody.mass;
	}
}

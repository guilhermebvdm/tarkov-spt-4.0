using UnityEngine;

public class Trigger : MonoBehaviour
{
	[SerializeField]
	private float _impactForceThreshold;

	public float Single_0 => _impactForceThreshold * _impactForceThreshold;

	public void TriggerDestruction(Vector3 triggerPosition, float magnitude)
	{
		BoxFracture[] components = base.gameObject.GetComponents<BoxFracture>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].Destroy(triggerPosition, magnitude);
		}
	}

	public void OnCollisionEnter(Collision c)
	{
		float sqrMagnitude = c.relativeVelocity.sqrMagnitude;
		if (!(sqrMagnitude < Single_0))
		{
			TriggerDestruction(c.contacts[0].point, sqrMagnitude * 0.1f / Single_0);
		}
	}
}

using UnityEngine;

namespace MirzaBeig.Scripting.Effects;

[AddComponentMenu("Effects/Particle Force Fields/Attraction Particle Force Field")]
public class AttractionParticleForceField : ParticleForceField
{
	[Header("ForceField Controls")]
	[Tooltip("Tether force based on linear inverse particle distance to force field center.")]
	public float arrivalRadius = 1f;

	[Tooltip("Dead zone from force field center in which no additional force is applied.")]
	public float arrivedRadius = 0.5f;

	private float float_4;

	private float float_5;

	public override void Awake()
	{
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void LateUpdate()
	{
		float x = base.transform.lossyScale.x;
		float_4 = arrivalRadius * arrivalRadius * x;
		float_5 = arrivedRadius * arrivedRadius * x;
		base.LateUpdate();
	}

	public override Vector3 GetForce()
	{
		if (parameters.distanceToForceFieldCenterSqr < float_5)
		{
			Vector3 result = default(Vector3);
			result.x = 0f;
			result.y = 0f;
			result.z = 0f;
			return result;
		}
		if (parameters.distanceToForceFieldCenterSqr < float_4)
		{
			float num = 1f - parameters.distanceToForceFieldCenterSqr / float_4;
			return Vector3.Normalize(parameters.scaledDirectionToForceFieldCenter) * num;
		}
		return Vector3.Normalize(parameters.scaledDirectionToForceFieldCenter);
	}

	public override void OnDrawGizmosSelected()
	{
		if (base.enabled)
		{
			base.OnDrawGizmosSelected();
			float x = base.transform.lossyScale.x;
			float num = arrivalRadius * x;
			float num2 = arrivedRadius * x;
			Vector3 vector = base.transform.position + center;
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(vector, num);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(vector, num2);
		}
	}
}

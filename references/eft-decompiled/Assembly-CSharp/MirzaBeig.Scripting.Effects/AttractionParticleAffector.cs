using UnityEngine;

namespace MirzaBeig.Scripting.Effects;

public class AttractionParticleAffector : ParticleAffector
{
	[Header("Affector Controls")]
	public float arrivalRadius = 1f;

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
		if (parameters.distanceToAffectorCenterSqr < float_5)
		{
			Vector3 result = default(Vector3);
			result.x = 0f;
			result.y = 0f;
			result.z = 0f;
			return result;
		}
		if (parameters.distanceToAffectorCenterSqr < float_4)
		{
			float num = 1f - parameters.distanceToAffectorCenterSqr / float_4;
			return Vector3.Normalize(parameters.scaledDirectionToAffectorCenter) * num;
		}
		return Vector3.Normalize(parameters.scaledDirectionToAffectorCenter);
	}

	public override void OnDrawGizmosSelected()
	{
		if (base.enabled)
		{
			base.OnDrawGizmosSelected();
			float x = base.transform.lossyScale.x;
			float num = arrivalRadius * x;
			float num2 = arrivedRadius * x;
			Vector3 center = base.transform.position + offset;
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(center, num);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(center, num2);
		}
	}
}

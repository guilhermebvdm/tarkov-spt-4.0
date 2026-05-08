using UnityEngine;

namespace MirzaBeig.Scripting.Effects;

public class VortexParticleAffector : ParticleAffector
{
	private Vector3 vector3_1;

	[Header("Affector Controls")]
	public Vector3 axisOfRotationOffset = Vector3.zero;

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
		base.LateUpdate();
	}

	public void method_0()
	{
		vector3_1 = Quaternion.Euler(axisOfRotationOffset) * base.transform.up;
	}

	public override void PerParticleSystemSetup()
	{
		method_0();
	}

	public override Vector3 GetForce()
	{
		return Vector3.Normalize(Vector3.Cross(vector3_1, parameters.scaledDirectionToAffectorCenter));
	}

	public override void OnDrawGizmosSelected()
	{
		if (base.enabled)
		{
			base.OnDrawGizmosSelected();
			Gizmos.color = Color.red;
			Vector3 vector;
			if (Application.isPlaying && base.enabled)
			{
				method_0();
				vector = vector3_1;
			}
			else
			{
				vector = Quaternion.Euler(axisOfRotationOffset) * base.transform.up;
			}
			Gizmos.DrawLine(base.transform.position + offset, base.transform.position + offset + vector * base.scaledRadius);
		}
	}
}

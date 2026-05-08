using UnityEngine;

namespace MirzaBeig.Scripting.Effects;

[AddComponentMenu("Effects/Particle Force Fields/Vortex Particle Force Field")]
public class VortexParticleForceField : ParticleForceField
{
	private Vector3 vector3_1;

	[Header("ForceField Controls")]
	[Tooltip("Internal offset for the axis of rotation.\n\nUseful if the force field and particle system are on the same game object, and you need a seperate rotation for the system, and the affector, but don't want to make the two different game objects.")]
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
		return Vector3.Normalize(Vector3.Cross(vector3_1, parameters.scaledDirectionToForceFieldCenter));
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
			Vector3 vector2 = base.transform.position + center;
			Gizmos.DrawLine(vector2, vector2 + vector * base.scaledRadius);
		}
	}
}

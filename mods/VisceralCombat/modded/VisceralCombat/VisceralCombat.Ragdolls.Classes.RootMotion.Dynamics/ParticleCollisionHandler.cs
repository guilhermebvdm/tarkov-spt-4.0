using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public class ParticleCollisionHandler : MonoBehaviour
{
	[Tooltip("PuppetMaster ragdoll layers to hit.")]
	public LayerMask ragdollLayers;

	[Tooltip("Multiplier for unpinning the puppet on particle hit (velocity.magnitude * colliderForce * unpin).")]
	public float unpin = 0.02f;

	private ParticleSystem p;

	private List<ParticleCollisionEvent> particleCollisionEvents = new List<ParticleCollisionEvent>();

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		p = ((Component)this).GetComponent<ParticleSystem>();
		ParticleSystem.CollisionModule collision = p.collision;
		if (!collision.sendCollisionMessages)
		{
		}
		collision = p.collision;
		if (collision.colliderForce <= 0f)
		{
		}
		collision = p.collision;
		if (collision.collidesWith != 0)
		{
		}
	}

	private void OnParticleCollision(GameObject other)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).enabled || !LayerMaskExtensions.Contains(ragdollLayers, other.layer))
		{
			return;
		}
		Collider component = other.GetComponent<Collider>();
		if ((Object)(object)component.attachedRigidbody == (Object)null)
		{
			return;
		}
		MuscleCollisionBroadcaster component2 = ((Component)component.attachedRigidbody).GetComponent<MuscleCollisionBroadcaster>();
		if (!((Object)(object)component2 == (Object)null))
		{
			int collisionEvents = ParticlePhysicsExtensions.GetCollisionEvents(p, other, particleCollisionEvents);
			for (int i = 0; i < collisionEvents; i++)
			{
				ParticleCollisionEvent val = particleCollisionEvents[i];
				Vector3 intersection = val.intersection;
				val = particleCollisionEvents[i];
				Vector3 velocity = val.velocity;
				float magnitude = velocity.magnitude;
				ParticleSystem.CollisionModule collision = p.collision;
				float unPin = magnitude * collision.colliderForce * unpin;
				component2.Hit(unPin, Vector3.zero, intersection);
			}
		}
	}
}

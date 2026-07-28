using System.Collections.Generic;
using Systems.Effects;
using Comfort.Common;
using UnityEngine;

namespace VisceralCombat.Dismemberment.Classes;

public class ParticleFloorPainter : MonoBehaviour
{
	public ParticleSystem ps;

	public List<ParticleCollisionEvent> collisionEvents;

	private void Start()
	{
		ps = ((Component)this).GetComponent<ParticleSystem>();
	}

	private void OnParticleCollision(GameObject collidedObject)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		List<ParticleCollisionEvent> list = new List<ParticleCollisionEvent>();
		ParticlePhysicsExtensions.GetCollisionEvents(ps, collidedObject, list);
		foreach (ParticleCollisionEvent item in list)
		{
			ParticleCollisionEvent current = item;
			if (collidedObject.gameObject.layer != LayerMask.NameToLayer("Player") && collidedObject.gameObject.layer != LayerMask.NameToLayer("HitCollider") && collidedObject.gameObject.layer != LayerMask.NameToLayer("Deadbody"))
			{
				Singleton<Effects>.Instance.EmitBleeding(((ParticleCollisionEvent)(ref current)).intersection, ((ParticleCollisionEvent)(ref current)).normal);
			}
		}
	}
}

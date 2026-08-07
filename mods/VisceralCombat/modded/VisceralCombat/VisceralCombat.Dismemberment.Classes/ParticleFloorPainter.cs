using System.Collections.Generic;
using Systems.Effects;
using Comfort.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Dismemberment.Classes;

public class ParticleFloorPainter : MonoBehaviour
{
	public ParticleSystem ps;

	public List<ParticleCollisionEvent> collisionEvents;

	private static readonly List<ParticleCollisionEvent> SharedCollisionEvents = new List<ParticleCollisionEvent>();
	private static int _playerLayer = -1;
	private static int _hitColliderLayer = -1;
	private static int _deadbodyLayer = -1;

	private void Start()
	{
		ps = GetComponent<ParticleSystem>();
		if (_playerLayer < 0)
		{
			_playerLayer = LayerMask.NameToLayer("Player");
			_hitColliderLayer = LayerMask.NameToLayer("HitCollider");
			_deadbodyLayer = LayerMask.NameToLayer("Deadbody");
		}
	}

	private void OnParticleCollision(GameObject collidedObject)
	{
		if (collidedObject == null || Singleton<Effects>.Instance == null) return;

		int layer = collidedObject.layer;
		if (layer == _playerLayer || layer == _hitColliderLayer || layer == _deadbodyLayer) return;

		SharedCollisionEvents.Clear();
		ParticlePhysicsExtensions.GetCollisionEvents(ps, collidedObject, SharedCollisionEvents);
		for (int i = 0; i < SharedCollisionEvents.Count; i++)
		{
			ParticleCollisionEvent ev = SharedCollisionEvents[i];
			Singleton<Effects>.Instance.EmitBleeding(ev.intersection, ev.normal);
		}
	}
}

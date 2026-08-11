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

	// Cooldown to limit blood decal generation to ~1x per 0.5s per particle system
	private float _nextAllowedEmitTime;
	public float CooldownSeconds = 0.5f;

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
		if (Time.time < _nextAllowedEmitTime) return; // Limit decal creation rate

		int layer = collidedObject.layer;
		if (layer == _playerLayer || layer == _hitColliderLayer || layer == _deadbodyLayer) return;

		SharedCollisionEvents.Clear();
		ParticlePhysicsExtensions.GetCollisionEvents(ps, collidedObject, SharedCollisionEvents);
		if (SharedCollisionEvents.Count > 0)
		{
			ParticleCollisionEvent ev = SharedCollisionEvents[0];
			Vector3 normal = ev.normal;
			if (normal.sqrMagnitude < 0.001f) normal = Vector3.up;

			_nextAllowedEmitTime = Time.time + CooldownSeconds;
			Singleton<Effects>.Instance.EmitBleeding(ev.intersection, normal);
		}
	}
}

using System.Collections.Generic;
using Systems.Effects;
using Comfort.Common;
using UnityEngine;

namespace VisceralCombat.Dismemberment.Classes;

public class ParticleFloorPainter : MonoBehaviour
{
	public ParticleSystem ps;

	private static readonly List<ParticleCollisionEvent> SharedCollisionEvents = new List<ParticleCollisionEvent>();
	private static int _playerLayer = -1;
	private static int _hitColliderLayer = -1;
	private static int _deadbodyLayer = -1;
	private static int _transparentFxLayer = -1;

	// Cooldown to limit blood decal generation to ~1x per 0.15s per particle system
	private float _nextAllowedEmitTime;
	public float CooldownSeconds = 0.15f;

	private void Awake()
	{
		Init();
	}

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (ps == null)
		{
			ps = GetComponent<ParticleSystem>() ?? GetComponentInChildren<ParticleSystem>();
		}
		if (_playerLayer < 0)
		{
			_playerLayer = LayerMask.NameToLayer("Player");
			_hitColliderLayer = LayerMask.NameToLayer("HitCollider");
			_deadbodyLayer = LayerMask.NameToLayer("Deadbody");
			_transparentFxLayer = LayerMask.NameToLayer("TransparentFX");
		}
	}

	private void OnParticleCollision(GameObject collidedObject)
	{
		if (collidedObject == null || Singleton<Effects>.Instance == null) return;
		if (ps == null)
		{
			ps = GetComponent<ParticleSystem>() ?? GetComponentInChildren<ParticleSystem>();
			if (ps == null) return;
		}

		if (Time.time < _nextAllowedEmitTime) return; // Limit decal creation rate

		int layer = collidedObject.layer;
		if (layer == _playerLayer || layer == _hitColliderLayer || layer == _deadbodyLayer || layer == _transparentFxLayer) return;

		SharedCollisionEvents.Clear();
		ParticlePhysicsExtensions.GetCollisionEvents(ps, collidedObject, SharedCollisionEvents);
		if (SharedCollisionEvents.Count > 0)
		{
			ParticleCollisionEvent ev = SharedCollisionEvents[0];
			Vector3 normal = ev.normal;
			if (normal.sqrMagnitude < 0.001f) normal = Vector3.up;

			_nextAllowedEmitTime = Time.time + CooldownSeconds;

			// Emit real blood pools on environment surfaces (floors/walls)
			Singleton<Effects>.Instance.EmitBloodOnEnvironment(ev.intersection, normal);
		}
	}
}

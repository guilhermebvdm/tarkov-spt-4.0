using System.Collections.Generic;
using UnityEngine;

namespace EFT;

public class RocketSettings : ThrowableSettings, GInterface165
{
	[SerializeField]
	private GameObject _ammoObject;

	[SerializeField]
	private List<MeshRenderer> _meshRenderers = new List<MeshRenderer>();

	[SerializeField]
	private float _weight;

	[SerializeField]
	private float _startVelocity;

	[SerializeField]
	private float _rigidbodyDrag;

	[SerializeField]
	private float _collisionDrag;

	[SerializeField]
	private Collider _selfCollider;

	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	private Material _explosionParticleMaterial;

	[SerializeField]
	private Gradient _thermalGradient;

	[SerializeField]
	private Material _explosionDebugMaterial;

	[SerializeField]
	private GameObject _debugObject;

	[Header("Sound")]
	[Space]
	[SerializeField]
	private SoundBank _explosionSoundBank;

	public float StartSpeed
	{
		get
		{
			return _startVelocity;
		}
		set
		{
			_startVelocity = value;
		}
	}

	public float Weight
	{
		get
		{
			return _weight;
		}
		set
		{
			_weight = value;
		}
	}

	public float Drag
	{
		get
		{
			return _rigidbodyDrag;
		}
		set
		{
			_rigidbodyDrag = value;
		}
	}

	public GameObject Ammo => _ammoObject;

	public ICollection<MeshRenderer> MeshRenderers => _meshRenderers;

	public Collider SelfCollider => _selfCollider;

	public Rigidbody Rigidbody => _rigidbody;

	public Material ExplosionParticleMaterial => _explosionParticleMaterial;

	public Gradient ThermalGradient => _thermalGradient;

	public Material ExplosionDebugMaterial => _explosionDebugMaterial;

	public GameObject DebugObject => _debugObject;

	public SoundBank ExplosionSoundBank => _explosionSoundBank;
}

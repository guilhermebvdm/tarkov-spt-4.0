using System;
using UnityEngine;

public class ParticlePoolItem : MonoBehaviour
{
	private Action<ParticlePoolItem> action_0;

	private ParticleSystem particleSystem_0;

	private Transform transform_0;

	private float float_0;

	private float float_1;

	private bool bool_0;

	public void Awake()
	{
		particleSystem_0 = GetComponent<ParticleSystem>();
		if (particleSystem_0 == null)
		{
			base.enabled = false;
		}
		transform_0 = GetComponent<Transform>();
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			float_0 = Mathf.Max(float_0, particleSystem.main.startLifetime.constant + particleSystem.main.startDelay.constant);
		}
	}

	public void Init(Action<ParticlePoolItem> callback)
	{
		action_0 = callback;
	}

	public void UseItem(Vector3 position, Vector3 rotation)
	{
		float_1 = 0f;
		bool_0 = true;
		transform_0.position = position;
		particleSystem_0.Play(withChildren: true);
	}

	public void Stop()
	{
		bool_0 = false;
		particleSystem_0.Stop(withChildren: true);
	}

	public void Update()
	{
		if (bool_0)
		{
			float_1 += Time.deltaTime;
			if (!(float_1 < float_0))
			{
				bool_0 = false;
				particleSystem_0.Stop(withChildren: true);
				action_0(this);
			}
		}
	}
}

using System.Collections.Generic;
using UnityEngine;

public class VolumetricExplosionDemo : MonoBehaviour
{
	private GClass1007 gclass1007_0;

	private GClass1006 gclass1006_0;

	private List<Vector3> list_0;

	private Camera camera_0;

	[SerializeField]
	private float _radius;

	[SerializeField]
	private float _subdivisionN;

	[SerializeField]
	private float _rayLength;

	[SerializeField]
	private float _maxCastLength;

	[SerializeField]
	private ParticleSystem _system;

	public void Awake()
	{
		camera_0 = Camera.main;
	}

	public void Start()
	{
		int maxParticles = _system.main.maxParticles;
		list_0 = new List<Vector3>(maxParticles);
		gclass1007_0 = new GClass1007(_system);
		gclass1006_0 = new GClass1006(_radius, _subdivisionN, maxParticles, _maxCastLength);
	}

	public void OnDestroy()
	{
		gclass1006_0.Dispose();
		gclass1007_0.Dispose();
	}

	public void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Plane plane = new Plane(Vector3.up, 0f);
			Ray ray = camera_0.ScreenPointToRay(Input.mousePosition);
			plane.Raycast(ray, out var enter);
			Vector3 origin = ray.GetPoint(enter) + Vector3.up * 0.5f;
			_system.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
			gclass1006_0.RayCast(list_0, origin);
			gclass1007_0.Play(list_0);
		}
	}
}

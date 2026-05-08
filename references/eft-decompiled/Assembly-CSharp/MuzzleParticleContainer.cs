using UnityEngine;

public class MuzzleParticleContainer : MonoBehaviour
{
	public struct GStruct68
	{
		public ParticleSystem System;

		public ParticleSystemRenderer Renderer;

		public Material Material;
	}

	private ParticleSystem particleSystem_0;

	private GStruct68[] gstruct68_0;

	[SerializeField]
	private EMuzzleParticlePivot _pivot;

	public ParticleSystem RootParticleSystem => particleSystem_0;

	public GStruct68[] MuzzleParticleData => gstruct68_0;

	public EMuzzleParticlePivot Pivot => _pivot;

	public void Awake()
	{
		particleSystem_0 = GetComponent<ParticleSystem>();
		ParticleSystem[] componentsInChildren = particleSystem_0.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			gstruct68_0 = new GStruct68[componentsInChildren.Length];
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				ParticleSystem particleSystem = componentsInChildren[i];
				ParticleSystemRenderer component = particleSystem.GetComponent<ParticleSystemRenderer>();
				gstruct68_0[i] = new GStruct68
				{
					System = particleSystem,
					Renderer = component,
					Material = component.material
				};
			}
		}
	}
}

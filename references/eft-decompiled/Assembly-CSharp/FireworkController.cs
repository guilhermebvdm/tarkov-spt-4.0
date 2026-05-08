using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class FireworkController : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem _particleSystem;

	[SerializeField]
	private ParticleSystem _customSubEmitter;

	public void Update()
	{
		if (_particleSystem.particleCount == 0)
		{
			return;
		}
		NativeArray<ParticleSystem.Particle> particles = new NativeArray<ParticleSystem.Particle>(_particleSystem.particleCount, Allocator.Temp);
		_particleSystem.GetParticles(particles, _particleSystem.particleCount);
		for (int i = 0; i < _particleSystem.particleCount; i++)
		{
			if (particles[i].remainingLifetime <= Time.deltaTime)
			{
				Object.Instantiate(_customSubEmitter, _particleSystem.transform.TransformPoint(particles[i].position), quaternion.identity).Play();
			}
		}
	}
}

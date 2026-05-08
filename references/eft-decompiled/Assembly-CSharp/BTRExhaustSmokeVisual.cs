using UnityEngine;

public class BTRExhaustSmokeVisual : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem _particleSystem;

	public void SetProperties(Vector2 lifetimeMinMax, Vector2 sizeMinMax, float rate, float speed, Color color)
	{
		ParticleSystem.MinMaxCurve lifetime = new ParticleSystem.MinMaxCurve(lifetimeMinMax.x, lifetimeMinMax.y);
		ParticleSystem.MinMaxCurve size = new ParticleSystem.MinMaxCurve(sizeMinMax.x, sizeMinMax.y);
		method_1(lifetime, size, rate, speed, color);
	}

	public void SetProperties(Vector2 lifetimeMinMax, float rate)
	{
		ParticleSystem.MinMaxCurve lifetime = new ParticleSystem.MinMaxCurve(lifetimeMinMax.x, lifetimeMinMax.y);
		method_0(lifetime, rate);
	}

	public void method_0(ParticleSystem.MinMaxCurve lifetime, float rate)
	{
		ParticleSystem.MainModule main = _particleSystem.main;
		main.startLifetime = lifetime;
		ParticleSystem.EmissionModule emission = _particleSystem.emission;
		emission.rateOverTime = rate;
	}

	public void method_1(ParticleSystem.MinMaxCurve lifetime, ParticleSystem.MinMaxCurve size, float rate, float speed, Color color)
	{
		ParticleSystem.MainModule main = _particleSystem.main;
		main.startLifetime = lifetime;
		main.startColor = color;
		main.startSize = size;
		ParticleSystem.EmissionModule emission = _particleSystem.emission;
		emission.rateOverTime = rate;
		ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetime = _particleSystem.limitVelocityOverLifetime;
		limitVelocityOverLifetime.limit = speed;
	}
}

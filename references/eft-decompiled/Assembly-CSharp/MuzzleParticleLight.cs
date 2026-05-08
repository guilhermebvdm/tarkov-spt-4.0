using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class MuzzleParticleLight : MonoBehaviour
{
	private const float float_0 = 0.01f;

	private float float_1;

	private float float_2;

	[SerializeField]
	private Light _light;

	[SerializeField]
	private ParticleSystem _particleSystem;

	[SerializeField]
	private Vector2 _minMaxIntensity = Vector2.up;

	[SerializeField]
	private float _time = 1f;

	public void LateUpdate()
	{
		if (!(_particleSystem == null) && !(_light == null))
		{
			float_1 = (_particleSystem.isPlaying ? _minMaxIntensity.y : _minMaxIntensity.x);
			_light.intensity = Mathf.SmoothDamp(_light.intensity, float_1, ref float_2, Time.deltaTime * _time);
			if (Math.Abs(_light.intensity - float_1) < 0.01f)
			{
				_light.intensity = float_1;
			}
		}
	}
}

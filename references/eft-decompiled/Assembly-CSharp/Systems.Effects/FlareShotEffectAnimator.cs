using System.Runtime.CompilerServices;
using UnityEngine;

namespace Systems.Effects;

public class FlareShotEffectAnimator : MonoBehaviour
{
	private static readonly int int_0 = Shader.PropertyToID("_Intensity");

	[SerializeField]
	private AnimationCurve _fadeCurve;

	[SerializeField]
	private ParticleSystem _flareParticleSystem;

	[SerializeField]
	private ParticleSystem _smokeParticleSystem;

	[SerializeField]
	private CullingLightObject _cullingLightObject;

	[SerializeField]
	private float _lightIntensity = 1f;

	private float float_0;

	private Material material_0;

	[CompilerGenerated]
	private float float_1;

	public float LightIntensity
	{
		set
		{
			_lightIntensity = value;
		}
	}

	public float Frequency
	{
		[CompilerGenerated]
		get
		{
			return float_1;
		}
		[CompilerGenerated]
		set
		{
			float_1 = value;
		}
	}

	public void OnEnable()
	{
		float_0 = 0f;
		if (!(_cullingLightObject == null))
		{
			float num = _fadeCurve.Evaluate(float_0);
			_cullingLightObject.IntensityMultiplier = num * _lightIntensity;
			_flareParticleSystem.Play(withChildren: false);
			_smokeParticleSystem.Play(withChildren: false);
		}
	}

	public void OnDisable()
	{
		_flareParticleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmittingAndClear);
		_smokeParticleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmittingAndClear);
	}

	public void OnDestroy()
	{
		OnDisable();
	}

	public void Update()
	{
		if (!(_cullingLightObject == null))
		{
			base.transform.rotation = Quaternion.identity;
			float num = _fadeCurve.Evaluate(float_0);
			_cullingLightObject.IntensityMultiplier = num * _lightIntensity;
			float_0 += Time.deltaTime * Frequency;
		}
	}
}

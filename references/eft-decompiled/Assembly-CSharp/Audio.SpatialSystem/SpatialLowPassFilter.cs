using System.Runtime.CompilerServices;
using UnityEngine;

namespace Audio.SpatialSystem;

[RequireComponent(typeof(AudioLowPassFilter))]
public class SpatialLowPassFilter : AudioFilter
{
	private const float float_1 = 10f;

	private const float float_2 = 10f;

	private const float float_3 = 22000f;

	[SerializeField]
	private AudioLowPassFilter _filter;

	private float float_4 = 22000f;

	private float float_5 = 5000f;

	private AnimationCurve animationCurve_0 = new AnimationCurve();

	private AnimationCurve animationCurve_1 = new AnimationCurve();

	private AnimationCurve animationCurve_2 = new AnimationCurve();

	private float float_6;

	[CompilerGenerated]
	private float float_7;

	public float CutoffFrequency
	{
		[CompilerGenerated]
		get
		{
			return float_7;
		}
		[CompilerGenerated]
		set
		{
			float_7 = value;
		}
	}

	public override void OnAwake()
	{
		if (_filter == null)
		{
			_filter = GetComponent<AudioLowPassFilter>();
		}
		base.OnAwake();
	}

	public override void SetActive(bool active)
	{
		if (_isActive != active)
		{
			_filter.enabled = active;
			base.SetActive(active);
		}
	}

	public void SetOcclusionCurves(AnimationCurve obstructionCurve, AnimationCurve propagationCurve)
	{
		animationCurve_0 = obstructionCurve;
		animationCurve_1 = propagationCurve;
		animationCurve_2 = propagationCurve;
	}

	public override void ResetFilter()
	{
		if ((object)_filter != null)
		{
			float_4 = 22000f;
			_filter.cutoffFrequency = 22000f;
			base.FilterLevel = 0f;
		}
	}

	public override void SetFilterParams(float value, bool applyImmediately = false, ESoundOcclusionType occlusionType = ESoundOcclusionType.FullOcclusion)
	{
		base.FilterLevel = value;
		animationCurve_2 = ((occlusionType == ESoundOcclusionType.Obstruction) ? animationCurve_0 : animationCurve_1);
		if (applyImmediately)
		{
			CalculateFrequency(1f);
		}
	}

	public override void SetResonance(float value)
	{
		if (base.Initialized)
		{
			_filter.lowpassResonanceQ = value;
		}
	}

	public bool method_0()
	{
		return Mathf.Abs(_filter.cutoffFrequency - float_6) > 10f;
	}

	public void SetHighestFrequency(float frequency)
	{
		float_4 = frequency;
	}

	public void SetLowerFrequency(float frequency)
	{
		if (!(frequency >= float_4))
		{
			float_5 = frequency;
		}
	}

	public override void CalculateFrequency(float time)
	{
		if (base.Initialized)
		{
			float b = Mathf.Clamp(animationCurve_2.Evaluate(base.FilterLevel), float_5, float_4);
			float_6 = Mathf.Lerp(_filter.cutoffFrequency, b, time * 10f);
			if (method_0())
			{
				_filter.cutoffFrequency = float_6;
				CutoffFrequency = float_6;
			}
		}
	}
}

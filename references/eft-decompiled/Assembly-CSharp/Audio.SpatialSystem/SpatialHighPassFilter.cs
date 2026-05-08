using System.Runtime.CompilerServices;
using UnityEngine;

namespace Audio.SpatialSystem;

[RequireComponent(typeof(AudioHighPassFilter))]
public class SpatialHighPassFilter : AudioFilter
{
	private const float float_1 = 10f;

	private const float float_2 = 10f;

	private const float float_3 = 1f;

	[SerializeField]
	private AudioHighPassFilter _filter;

	private float float_4;

	private AnimationCurve animationCurve_0 = new AnimationCurve();

	private float float_5;

	[CompilerGenerated]
	private float float_6;

	public float CutoffFrequency
	{
		[CompilerGenerated]
		get
		{
			return float_6;
		}
		[CompilerGenerated]
		set
		{
			float_6 = value;
		}
	}

	public override void OnAwake()
	{
		if (_filter == null)
		{
			_filter = GetComponent<AudioHighPassFilter>();
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

	public override void ResetFilter()
	{
		if (!(_filter == null))
		{
			float_5 = 10f;
			_filter.cutoffFrequency = 10f;
			base.FilterLevel = 0f;
		}
	}

	public void SetFilterCurve(AnimationCurve filterCurve)
	{
		animationCurve_0 = filterCurve;
	}

	public void SetMaxCutoffFrequency(float frequency)
	{
		if (!(frequency <= 10f))
		{
			float_5 = frequency;
		}
	}

	public override void SetFilterParams(float value, bool applyImmediately = false, ESoundOcclusionType occlusionType = ESoundOcclusionType.FullOcclusion)
	{
		base.FilterLevel = value;
		if (applyImmediately)
		{
			CalculateFrequency(1f);
		}
	}

	public override void SetResonance(float value)
	{
		if (base.Initialized)
		{
			_filter.highpassResonanceQ = value;
		}
	}

	public bool method_0()
	{
		return Mathf.Abs(_filter.cutoffFrequency - float_4) > 1f;
	}

	public override void CalculateFrequency(float time)
	{
		if (base.Initialized)
		{
			float b = Mathf.Clamp(animationCurve_0.Evaluate(base.FilterLevel), 10f, float_5);
			float_4 = Mathf.Lerp(_filter.cutoffFrequency, b, time * 10f);
			if (method_0())
			{
				_filter.cutoffFrequency = float_4;
				CutoffFrequency = float_4;
			}
		}
	}
}

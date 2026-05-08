using UnityEngine;

namespace Audio.Effects;

[RequireComponent(typeof(AudioHighPassFilter), typeof(AudioLowPassFilter))]
public class AudioBuiltinEQFilter : AudioEQFilterBase
{
	[SerializeField]
	private AudioHighPassFilter _highPassFilter;

	[SerializeField]
	private AudioLowPassFilter _lowPassFilter;

	public override void InitializeComponents()
	{
		if (_highPassFilter == null)
		{
			_highPassFilter = GetComponent<AudioHighPassFilter>();
		}
		if (_lowPassFilter == null)
		{
			_lowPassFilter = GetComponent<AudioLowPassFilter>();
		}
	}

	public override void ApplyHighPass(float frequency, float q)
	{
		_highPassFilter.cutoffFrequency = frequency;
		_highPassFilter.highpassResonanceQ = q;
	}

	public override void ApplyLowPass(float frequency, float q)
	{
		_lowPassFilter.cutoffFrequency = frequency;
		_lowPassFilter.lowpassResonanceQ = q;
	}

	public override void OnHighPassEnabled(bool targetState)
	{
		_highPassFilter.enabled = targetState;
		if (targetState)
		{
			ApplyHighPass(_currentHpf, _currentHpfQ);
		}
	}

	public override void OnLowPassEnabled(bool targetState)
	{
		_lowPassFilter.enabled = targetState;
		if (targetState)
		{
			ApplyLowPass(_currentLpf, _currentLpfQ);
		}
	}

	public override void OnSetActive(bool active)
	{
		_lowPassFilter.enabled = active;
		_highPassFilter.enabled = active;
		base.enabled = active;
	}
}

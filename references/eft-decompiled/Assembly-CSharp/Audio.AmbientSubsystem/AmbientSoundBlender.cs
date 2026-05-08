using System;
using Audio.AmbientSubsystem.Data;
using Audio.Data;
using Audio.SpatialSystem.Data;
using CommonAssets.Scripts.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.AmbientSubsystem;

[Serializable]
public class AmbientSoundBlender : MonoBehaviour, IDisposable
{
	private const float MIN_MIXER_VOLUME_DB = -80f;

	private const float MAX_MIXER_PARAM_VALUE = 1f;

	private const float MIN_MIXER_PARAM_VALUE = 0f;

	[Header("Default Transitions")]
	[SerializeField]
	private EAudioFadeType fadeType;

	[SerializeField]
	private float _fadeInDuration = 3f;

	[SerializeField]
	private float _fadeOutDuration = 0.3f;

	[SerializeField]
	private float _fromOutToInTransitionTime = 0.1f;

	[SerializeField]
	private float _fromInToOutTransitionTime = 0.5f;

	[SerializeField]
	private AudioMixerParametersData _outdoorMixerParams;

	[SerializeField]
	private AudioMixerParametersData _indoorMixerParams;

	[SerializeField]
	private AnimationCurve _volumeCurve = new AnimationCurve();

	[SerializeField]
	private AnimationCurve _occlusionCurve = new AnimationCurve();

	[SerializeField]
	private float _blendSpeed = 2f;

	[SerializeField]
	private float _blendDurationMult = 0.3f;

	[SerializeField]
	private float _depthCalculationMult = 1f;

	[Header("Filters Settings")]
	[SerializeField]
	private float _maxHighPassFreq = 700f;

	[SerializeField]
	private float _minHighPassFreq = 30f;

	[SerializeField]
	private float _minLowpassFreq = 500f;

	[SerializeField]
	private float _maxLowpassFreq = 22000f;

	[Header("Effects Send")]
	[SerializeField]
	private float _maxOutdoorReverbSendDb = -2f;

	[SerializeField]
	private float _maxOutdoorDelaySendDb = -5.4f;

	private AudioMixer _masterMixer;

	private GInterface91 _outdoorMixerFader;

	private GInterface91 _indoorMixerFader;

	private float _currentOutdoorFilterValue;

	public void Init(AudioMixer masterMixer)
	{
		_masterMixer = masterMixer;
		_outdoorMixerFader = new GClass1179(this, masterMixer);
		_indoorMixerFader = new GClass1179(this, masterMixer);
	}

	public void CalculateAndApplyMixerParameters(in GStruct108 portalData, Vector3 listenerPos, float maxOutdoorVolume, float dt)
	{
		float t = dt * _blendSpeed;
		PortalData portalData2 = portalData.portalData;
		float distanceToListener = portalData.distanceToListener;
		float f = 1f - GClass1162.GetVerticalMult(portalData2.position, listenerPos);
		distanceToListener += distanceToListener * Mathf.Abs(f);
		float num = method_2(portalData2.depth, distanceToListener, maxOutdoorVolume);
		float value = Mathf.Abs(num - _currentOutdoorFilterValue) * _blendDurationMult;
		value = Mathf.Clamp(value, 0f, _blendDurationMult);
		float value2 = Mathf.Lerp(_currentOutdoorFilterValue, num, t);
		SetOutdoorMixerParams(value2, value);
	}

	public void SetOutdoorMixerParams(float value, float targetBlendDuration = 0.5f, bool force = true)
	{
		float num = _occlusionCurve.Evaluate(value);
		float num2 = GClass2313.ConvertNormalizedVolumeToDB(_volumeCurve.Evaluate(value));
		float value2 = GClass2313.ConvertNormalizedValueToCutoffFrequency(num);
		value2 = Mathf.Clamp(value2, _minLowpassFreq, _maxLowpassFreq);
		float highPassFreq = Mathf.Lerp(_minHighPassFreq, _maxHighPassFreq, num);
		float reverbSendDb = Mathf.Clamp(num2, -80f, _maxOutdoorReverbSendDb);
		float delaySendDb = Mathf.Clamp(num2, -80f, _maxOutdoorDelaySendDb);
		if (!force && targetBlendDuration >= _blendDurationMult)
		{
			method_0(num2, value2, highPassFreq, reverbSendDb, delaySendDb, targetBlendDuration);
		}
		else
		{
			if (_outdoorMixerFader.IsFading)
			{
				_outdoorMixerFader.StopFade();
			}
			method_0(num2, value2, highPassFreq, reverbSendDb, delaySendDb, 0f);
		}
		_currentOutdoorFilterValue = value;
	}

	public void method_0(float volumeDb, float lowPassFreq, float highPassFreq, float reverbSendDb, float delaySendDb, float duration)
	{
		method_1(EMixerParameterType.LowPassFreq, _outdoorMixerParams, _outdoorMixerFader, lowPassFreq, duration);
		method_1(EMixerParameterType.HighPassFreq, _outdoorMixerParams, _outdoorMixerFader, highPassFreq, duration);
		method_1(EMixerParameterType.Volume, _outdoorMixerParams, _outdoorMixerFader, volumeDb, duration);
		method_1(EMixerParameterType.ReverbSend, _outdoorMixerParams, _outdoorMixerFader, reverbSendDb, duration);
		method_1(EMixerParameterType.DelaySend, _outdoorMixerParams, _outdoorMixerFader, delaySendDb, duration);
	}

	public void method_1(EMixerParameterType parameterType, AudioMixerParametersData mixerParamsData, GInterface91 fader, float value, float duration = 0f)
	{
		if (!mixerParamsData.TryGetParameters(parameterType, out var mixerParams))
		{
			return;
		}
		for (int i = 0; i < mixerParams.Count; i++)
		{
			if (duration > 0f)
			{
				fader.StartFade(mixerParams[i], value, fadeType, duration);
			}
			else
			{
				_masterMixer.SetFloat(mixerParams[i], value);
			}
		}
	}

	public void BlendToIndoor()
	{
		SetIndoorMixerParams(1f, _fromOutToInTransitionTime, force: false);
	}

	public void BlendToOutdoor()
	{
		SetOutdoorMixerParams(1f, _fadeOutDuration, force: false);
		SetIndoorMixerParams(0f, _fromInToOutTransitionTime, force: false);
	}

	public void BlendToTargetValue(float value)
	{
		SetOutdoorMixerParams(value, _fadeInDuration, force: false);
	}

	public void SetIndoorMixerParams(float value, float targetBlendDuration = 0.5f, bool force = true)
	{
		float value2 = GClass2313.ConvertNormalizedVolumeToDB(value);
		if (!force)
		{
			method_1(EMixerParameterType.Volume, _indoorMixerParams, _indoorMixerFader, value2, targetBlendDuration);
			return;
		}
		if (_indoorMixerFader.IsFading)
		{
			_indoorMixerFader.StopFade();
		}
		method_1(EMixerParameterType.Volume, _indoorMixerParams, _indoorMixerFader, value2);
	}

	public float method_2(float portalDepth, float currentDistance, float maxVolume)
	{
		float num = Mathf.InverseLerp(portalDepth * _depthCalculationMult, 0f, currentDistance);
		return Mathf.Clamp(num, maxVolume, num);
	}

	public void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		_outdoorMixerFader?.Dispose();
		_outdoorMixerFader = null;
		_indoorMixerFader?.Dispose();
		_indoorMixerFader = null;
	}
}

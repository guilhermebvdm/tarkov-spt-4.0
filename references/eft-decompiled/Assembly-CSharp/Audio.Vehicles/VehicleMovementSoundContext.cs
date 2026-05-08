using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio.Vehicles.BTR;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.Vehicles;

public class VehicleMovementSoundContext : MonoBehaviour, GInterface75, IDisposable, GInterface218, GInterface77
{
	private const float float_0 = 0.2f;

	private const float float_1 = 0.7f;

	private const float float_2 = 1f;

	private const float float_3 = 0.1f;

	[Header("Audio Settings")]
	[Range(0f, 1f)]
	public float OverallVolume = 1f;

	[Range(0f, 1f)]
	public float EngineVolume = 1f;

	[Range(0f, 1f)]
	public float MovementVolume = 1f;

	[Range(0f, 1f)]
	public float AccelerationVolume = 1f;

	[Range(0f, 1f)]
	public float ImpactVolume = 1f;

	[Space]
	[SerializeField]
	private float _pitchMultiplier = 1f;

	[SerializeField]
	private float _lowPitchMax = 6f;

	[SerializeField]
	private float _highPitchMultiplier = 0.25f;

	[SerializeField]
	private float _minRolloffDistance = 5f;

	[SerializeField]
	private float _maxRolloffDistance = 500f;

	[SerializeField]
	private float _dopplerLevel = 1f;

	[SerializeField]
	private float _slopeMultiplier = 10f;

	[SerializeField]
	private float _maxUphillPitchFactor = 0.4f;

	[SerializeField]
	private float _maxDownhillPitchFactor = -0.4f;

	[Space]
	[SerializeField]
	private AudioMixerGroup _mixerGroup;

	[Space]
	[SerializeField]
	private AnimationCurve _enginePitchCurve = new AnimationCurve();

	[SerializeField]
	private AnimationCurve _movementVolumeCurve = new AnimationCurve();

	[SerializeField]
	private AnimationCurve _accelPitchCurve = new AnimationCurve();

	[SerializeField]
	private AnimationCurve _rolloffCurve = new AnimationCurve();

	[Space]
	[SerializeField]
	private VehicleMovementSoundContainer _soundContainer;

	[Space]
	[Header("Vehicle Audio Source")]
	[SerializeField]
	private AudioSource _lowEngineRpmLoopSource;

	[SerializeField]
	private AudioSource _highEngineRpmLoopSource;

	[SerializeField]
	private AudioSource _lowSpeedMovementLoopSource;

	[SerializeField]
	private AudioSource _highSpeedMovementLoopSource;

	[SerializeField]
	private AudioSource _accelerationSoundsSource;

	[SerializeField]
	private AudioSource _oneShotSoundsSource;

	private Class774 class774_0;

	private Class774 class774_1;

	private Class774 class774_2;

	private Class774 class774_3;

	private Class774 class774_4;

	private readonly Dictionary<EVehicleMovementStatus, Class774> dictionary_0 = new Dictionary<EVehicleMovementStatus, Class774>();

	private float float_4;

	private float float_5;

	private float float_6;

	private float float_7;

	private float float_8 = 20f;

	private float float_9;

	private float float_10;

	private float float_11;

	private float float_12;

	private readonly Vector2 vector2_0 = new Vector2(0.2f, 0.8f);

	private readonly Vector2 vector2_1 = new Vector2(0.9f, 1.1f);

	private readonly Vector2 vector2_2 = new Vector2(0.5f, 5f);

	private readonly Vector2 vector2_3 = new Vector2(0.2f, 1f);

	private readonly Vector2 vector2_4 = new Vector2(0.5f, 1f);

	private float float_13;

	private float float_14;

	private float float_15;

	[CompilerGenerated]
	private EVehicleMovementStatus evehicleMovementStatus_0;

	public EVehicleMovementStatus CurrentStatus
	{
		[CompilerGenerated]
		get
		{
			return evehicleMovementStatus_0;
		}
		[CompilerGenerated]
		set
		{
			evehicleMovementStatus_0 = value;
		}
	}

	public float MaxAllowedVolume => float_4 * OverallVolume;

	public void Init()
	{
		SetBaseVolume(0f);
		method_1();
		float_12 = (float_11 = Time.realtimeSinceStartup);
	}

	public void SetSpatialBlend(float spatialBlend)
	{
		_lowEngineRpmLoopSource.spatialBlend = spatialBlend;
		_highEngineRpmLoopSource.spatialBlend = spatialBlend;
		_lowSpeedMovementLoopSource.spatialBlend = spatialBlend;
		_highSpeedMovementLoopSource.spatialBlend = spatialBlend;
		_accelerationSoundsSource.spatialBlend = spatialBlend;
		_oneShotSoundsSource.spatialBlend = spatialBlend;
	}

	public void SetBaseVolume(float volume)
	{
		float_4 = Mathf.Clamp01(volume);
	}

	public void SetMaxSpeed(float speed)
	{
		float_8 = speed;
	}

	public void UpdateSpeed(float speed, float dt)
	{
		float_5 = Mathf.Clamp01(speed);
		method_0(dt);
		float_6 = float_5;
	}

	public void UpdateRotation(float rotationAngle)
	{
		float_13 = rotationAngle;
	}

	public void method_0(float dt)
	{
		float_9 += dt;
		float num = Mathf.Abs(float_5 - float_6);
		float_10 += num;
		if (!(float_9 <= 0.1f))
		{
			float_7 = Mathf.Clamp01(float_10 / float_9);
			float_9 = 0f;
			float_10 = 0f;
		}
	}

	public void method_1()
	{
		dictionary_0.Clear();
		class774_0 = new Class776(this);
		dictionary_0[class774_0.Status] = class774_0;
		class774_1 = new Class775(this);
		dictionary_0[class774_1.Status] = class774_1;
		class774_2 = new Class777(this);
		dictionary_0[class774_2.Status] = class774_2;
		class774_3 = new Class778(this);
		dictionary_0[class774_3.Status] = class774_3;
		class774_4 = class774_0;
	}

	public void method_2()
	{
		if (CurrentStatus == EVehicleMovementStatus.Started)
		{
			_accelerationSoundsSource.volume = MaxAllowedVolume;
			_accelerationSoundsSource.PlayOneShot(_soundContainer.EngineStartClip, MaxAllowedVolume * EngineVolume);
			method_8();
		}
	}

	public void method_3()
	{
		if (CurrentStatus == EVehicleMovementStatus.Running)
		{
			_accelerationSoundsSource.volume = MaxAllowedVolume;
			_accelerationSoundsSource.pitch = 1f;
			_accelerationSoundsSource.PlayOneShot(_soundContainer.EngineIdleAccelClip, MaxAllowedVolume * AccelerationVolume);
		}
	}

	public void method_4()
	{
		if (CurrentStatus == EVehicleMovementStatus.Running && Time.realtimeSinceStartup >= float_11 && !(float_7 < 0.2f) && float_7 <= 0.7f)
		{
			_accelerationSoundsSource.volume = MaxAllowedVolume;
			_accelerationSoundsSource.pitch = _accelPitchCurve.Evaluate(float_7);
			float volumeScale = Mathf.Clamp01(MaxAllowedVolume * float_5) * AccelerationVolume;
			_accelerationSoundsSource.PlayOneShot(_soundContainer.EngineHighAccelClip, volumeScale);
			float_11 = Time.realtimeSinceStartup + _soundContainer.EngineHighAccelClip.length + 1f;
		}
	}

	public void method_5()
	{
		float pitch = UnityEngine.Random.Range(vector2_1.x, vector2_1.y);
		float volumeScale = Mathf.InverseLerp(vector2_4.x, vector2_4.y, float_7);
		_oneShotSoundsSource.volume = MaxAllowedVolume;
		_oneShotSoundsSource.pitch = pitch;
		_oneShotSoundsSource.PlayOneShot(_soundContainer.MovementStopBreakClip, volumeScale);
	}

	public void method_6()
	{
	}

	public EVehicleMovementStatus method_7(EVehicleMovementStatus nextStatus)
	{
		if (!dictionary_0.TryGetValue(nextStatus, out var value))
		{
			Debug.LogError("cant find sound engine state for required status");
			return CurrentStatus;
		}
		if (value.IsCurrent)
		{
			return CurrentStatus;
		}
		class774_4.Exit();
		class774_4 = value;
		CurrentStatus = class774_4.Status;
		return class774_4.Run();
	}

	public void method_8()
	{
		float delay = _soundContainer.EngineStartClip.length / 2f;
		method_13(ref _accelerationSoundsSource, _soundContainer.EngineStartClip, loop: false);
		method_13(ref _oneShotSoundsSource, _soundContainer.MovementStopBreakClip, loop: false);
		method_13(ref _lowSpeedMovementLoopSource, _soundContainer.LowSpeedMovementLoopClip);
		_lowSpeedMovementLoopSource.PlayDelayed(delay);
		method_13(ref _highSpeedMovementLoopSource, _soundContainer.HighSpeedMovementLoopClip);
		_highSpeedMovementLoopSource.PlayDelayed(delay);
		method_13(ref _lowEngineRpmLoopSource, _soundContainer.LowRpmLoopClip);
		_lowEngineRpmLoopSource.PlayDelayed(delay);
		method_13(ref _highEngineRpmLoopSource, _soundContainer.HighRpmLoopClip);
		_highEngineRpmLoopSource.PlayDelayed(delay);
		method_14();
	}

	public void method_9()
	{
		if (CurrentStatus != EVehicleMovementStatus.Stopped)
		{
			float num = method_10();
			float value = _enginePitchCurve.Evaluate(float_5) * _pitchMultiplier + num;
			value = Mathf.Clamp(value, 1f, _lowPitchMax);
			AudioSource lowSpeedMovementLoopSource = _lowSpeedMovementLoopSource;
			float pitch = (_lowEngineRpmLoopSource.pitch = value);
			lowSpeedMovementLoopSource.pitch = pitch;
			AudioSource highSpeedMovementLoopSource = _highSpeedMovementLoopSource;
			pitch = (_highEngineRpmLoopSource.pitch = value * _highPitchMultiplier);
			highSpeedMovementLoopSource.pitch = pitch;
			float num4 = Mathf.InverseLerp(vector2_0.x, vector2_0.y, float_5 + num);
			float value2 = 1f - num4;
			num4 = method_12(num4);
			value2 = method_12(value2);
			float num5 = _movementVolumeCurve.Evaluate(float_5);
			_lowEngineRpmLoopSource.volume = value2 * MaxAllowedVolume * EngineVolume;
			_highEngineRpmLoopSource.volume = num4 * MaxAllowedVolume * EngineVolume;
			_lowSpeedMovementLoopSource.volume = value2 * num5 * MaxAllowedVolume * MovementVolume;
			_highSpeedMovementLoopSource.volume = num4 * num5 * MaxAllowedVolume * MovementVolume;
		}
	}

	public float method_10()
	{
		if (CurrentStatus != EVehicleMovementStatus.Running)
		{
			return 0f;
		}
		float num = float_13;
		if (!(num > 180f))
		{
			return Mathf.Lerp(0f, _maxDownhillPitchFactor, num / 180f * _slopeMultiplier);
		}
		return Mathf.Lerp(0f, _maxUphillPitchFactor, (360f - num) / 180f * _slopeMultiplier);
	}

	public void method_11()
	{
		_lowEngineRpmLoopSource.Stop();
		_highEngineRpmLoopSource.Stop();
		_lowSpeedMovementLoopSource.Stop();
		_highSpeedMovementLoopSource.Stop();
		_accelerationSoundsSource.Stop();
		_oneShotSoundsSource.Stop();
	}

	public float method_12(float value)
	{
		return 1f - (1f - value) * (1f - value);
	}

	public void Update()
	{
		class774_4.Update();
	}

	public void method_13(ref AudioSource source, AudioClip clip, bool loop = true)
	{
		source.outputAudioMixerGroup = _mixerGroup;
		source.clip = clip;
		source.volume = 0f;
		source.loop = loop;
		source.time = UnityEngine.Random.Range(0f, clip.length);
		source.minDistance = _minRolloffDistance;
		source.maxDistance = _maxRolloffDistance;
		source.rolloffMode = AudioRolloffMode.Custom;
		source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _rolloffCurve);
		source.dopplerLevel = 0f;
	}

	public void method_14()
	{
		_highEngineRpmLoopSource.dopplerLevel = _dopplerLevel;
		_lowEngineRpmLoopSource.dopplerLevel = _dopplerLevel;
		_lowSpeedMovementLoopSource.dopplerLevel = _dopplerLevel;
		_highSpeedMovementLoopSource.dopplerLevel = _dopplerLevel;
	}

	public void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		method_7(EVehicleMovementStatus.Stopped);
	}
}

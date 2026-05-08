using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.Vehicles.BTR;

public class VehicleRotationSoundPlayer : MonoBehaviour, GInterface218, GInterface77, IDisposable
{
	[SerializeField]
	private float _rotationPlaySoundCooldown = 0.1f;

	[SerializeField]
	private float _rotationChangeThreshold = 0.01f;

	[SerializeField]
	private float _maxDistance = 60f;

	[SerializeField]
	private AudioClip _startClip;

	[SerializeField]
	private AudioClip _loopClip;

	[SerializeField]
	private AudioClip _endClip;

	[SerializeField]
	private float _volumeMult = 0.9f;

	[SerializeField]
	private AudioMixerGroup _output;

	private float float_0;

	private float float_1;

	private float float_2;

	private float float_3 = 1f;

	private bool bool_0;

	private BetterSource betterSource_0;

	public float Single_0 => float_3 * _volumeMult;

	public void Awake()
	{
		method_0();
	}

	public void Start()
	{
		float_2 = Time.realtimeSinceStartup;
		float_0 = float_1;
	}

	public void UpdateRotation(float rotationAngle)
	{
		float_1 = rotationAngle;
		CheckRotation();
	}

	public void SetBaseVolume(float volume)
	{
		float_3 = Mathf.Clamp01(volume);
		betterSource_0.SetBaseVolume(Single_0);
	}

	public void method_0()
	{
		betterSource_0 = MonoBehaviourSingleton<BetterAudio>.Instance.GetSource(BetterAudio.AudioSourceGroupType.Environment);
		betterSource_0.SetBaseVolume(Single_0);
		betterSource_0.SetMixerGroup(_output);
		betterSource_0.SetRolloff(_maxDistance);
		betterSource_0.SetParent(base.transform, worldPositionStay: false);
		betterSource_0.Loop = true;
	}

	public void method_1()
	{
		bool_0 = true;
		betterSource_0.SetBaseVolume(Single_0);
		betterSource_0.source1.PlayOneShot(_startClip);
		betterSource_0.Loop = true;
		betterSource_0.source1.time = UnityEngine.Random.Range(0f, _loopClip.length);
		betterSource_0.Play(_loopClip, null, 1f, 1f, forceStereo: false, oneShot: false);
	}

	public void method_2()
	{
		bool_0 = false;
		betterSource_0.source1.Stop();
		betterSource_0.source1.PlayOneShot(_endClip, float_3);
	}

	public void CheckRotation()
	{
		if (method_5())
		{
			if (!bool_0 && method_3())
			{
				method_1();
				method_4(_startClip.length);
			}
			float_0 = float_1;
		}
		else if (bool_0 && method_3())
		{
			method_2();
			method_4(_endClip.length);
		}
	}

	public bool method_3()
	{
		return Time.realtimeSinceStartup - float_2 > 0f;
	}

	public void method_4(float clipLength)
	{
		float_2 = Time.realtimeSinceStartup + clipLength + _rotationPlaySoundCooldown;
	}

	public bool method_5()
	{
		return Mathf.Abs(float_1 - float_0) >= _rotationChangeThreshold;
	}

	public void Dispose()
	{
		betterSource_0?.Stop();
		betterSource_0?.Release();
	}
}

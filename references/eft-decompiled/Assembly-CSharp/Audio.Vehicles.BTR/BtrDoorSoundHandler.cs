using EFT;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.Vehicles.BTR;

public class BtrDoorSoundHandler : MonoBehaviour
{
	[SerializeField]
	private HysteresisFilter _filter;

	[SerializeField]
	private Transform _hingeTransform;

	[SerializeField]
	private float _closeEndSoundFilterValue;

	[SerializeField]
	private SoundBank _openSound;

	[SerializeField]
	private SoundBank _closeStartSound;

	[SerializeField]
	private SoundBank _closeEndSound;

	private bool bool_0;

	private AudioMixerGroup audioMixerGroup_0;

	private EnvironmentType environmentType_0;

	public void Awake()
	{
		bool_0 = false;
		_filter.OnValueChanged += method_0;
		_filter.OnEnd += OnFilterEnd;
		if (MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			audioMixerGroup_0 = MonoBehaviourSingleton<BetterAudio>.Instance.CommonAmbientOutEffectsMixer;
		}
	}

	public void method_0(float value)
	{
		if (!_filter.Direction && value <= _closeEndSoundFilterValue && value > 0f && !bool_0)
		{
			bool_0 = true;
			method_1(_closeEndSound);
		}
	}

	public void OnDestroy()
	{
		if (_filter != null)
		{
			_filter.OnValueChanged -= method_0;
			_filter.OnEnd -= OnFilterEnd;
		}
	}

	public void OnFilterEnd(bool onEnabled, bool isOn)
	{
		if (onEnabled && isOn)
		{
			method_1(_openSound);
		}
		else if (onEnabled)
		{
			method_1(_closeStartSound);
		}
		else if (!isOn)
		{
			bool_0 = false;
		}
	}

	public void SetEnvironment(EnvironmentType environmentType)
	{
		environmentType_0 = environmentType;
		audioMixerGroup_0 = method_2();
	}

	public void method_1(SoundBank bank)
	{
		if (MonoBehaviourSingleton<BetterAudio>.Instantiated)
		{
			AudioClip clip = bank.PickSingleClip((int)environmentType_0);
			MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(_hingeTransform.position, clip, BetterAudio.AudioSourceGroupType.Environment, (int)bank.Rolloff, bank.BaseVolume, EOcclusionTest.None, audioMixerGroup_0);
		}
	}

	public AudioMixerGroup method_2()
	{
		BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
		if (environmentType_0 != EnvironmentType.Outdoor)
		{
			return instance.AmbientInMixer;
		}
		return instance.CommonAmbientOutEffectsMixer;
	}
}

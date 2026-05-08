using EFT.Vehicle.Vehicles;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.Vehicles;

public class SoundSuspensionImpactSoundPlayer : MonoBehaviour
{
	private const float float_0 = 0.5f;

	[SerializeField]
	private VehicleSuspensionSpring _suspensionSpring;

	[SerializeField]
	private int _rolloff = 30;

	private GInterface76 ginterface76_0;

	private bool bool_0;

	private float float_1;

	private int int_0;

	public int Int32_0
	{
		get
		{
			return int_0;
		}
		set
		{
			int_0 = value;
		}
	}

	public void Init(GInterface76 suspensionController)
	{
		ginterface76_0 = suspensionController;
		if (_suspensionSpring == null)
		{
			_suspensionSpring = GetComponent<VehicleSuspensionSpring>();
			Debug.LogWarning("suspension spring field is null, try to get component on go: " + base.name);
		}
		if (_suspensionSpring == null)
		{
			Debug.LogError("suspension spring is null on go: " + base.name);
			return;
		}
		Int32_0 = _rolloff * _rolloff;
		float_1 = Time.realtimeSinceStartup;
		bool_0 = true;
	}

	public void TryPlayImpactSound(AudioMixerGroup mixerGroup)
	{
		if (bool_0)
		{
			float num = ginterface76_0.CalculateVolume(_suspensionSpring.SuspensionForce.magnitude);
			bool flag = Time.realtimeSinceStartup - float_1 > ginterface76_0.PlayingTimeThresholdSeconds;
			float randomPitch = ginterface76_0.GetRandomPitch();
			if (num >= 0.5f && flag && !ginterface76_0.IsOutOfRange(base.transform.position, Int32_0))
			{
				method_0(num, randomPitch, mixerGroup);
				float_1 = Time.realtimeSinceStartup;
			}
		}
	}

	public void method_0(float volume, float pitch, AudioMixerGroup mixerGroup)
	{
		if (!(MonoBehaviourSingleton<BetterAudio>.Instance == null))
		{
			AudioClip impactClip = ginterface76_0.GetImpactClip();
			MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(base.transform.position, impactClip, 30f, BetterAudio.AudioSourceGroupType.Impacts, _rolloff, volume, EOcclusionTest.None, mixerGroup).SetPitch(pitch);
		}
	}
}

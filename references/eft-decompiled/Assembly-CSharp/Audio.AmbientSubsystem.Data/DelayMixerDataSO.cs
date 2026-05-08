using System;
using Audio.SpatialSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/DelayMixerDataSO", fileName = "DelayMixerData")]
public class DelayMixerDataSO : ScriptableObject, IEnvironmentMixerParamsHandler
{
	public MixerEnvironmentParameter delayVolume;

	public MixerEnvironmentParameter delay;

	public MixerEnvironmentParameter decayTime;

	public MixerEnvironmentParameter wetMix;

	public MixerEnvironmentParameter lowpassFilterFreq;

	public void Apply(AudioMixer mixer, EAudioRoomTypeMask roomType)
	{
		method_0(delayVolume, mixer, roomType);
		method_0(delay, mixer, roomType);
		method_0(decayTime, mixer, roomType);
		method_0(wetMix, mixer, roomType);
		method_0(lowpassFilterFreq, mixer, roomType);
	}

	public void method_0(MixerEnvironmentParameter parameter, AudioMixer mixer, EAudioRoomTypeMask roomType)
	{
		if (parameter.parametersByRoom.TryGetValue(roomType, out var value))
		{
			mixer.SetFloat(parameter.paramName, value);
		}
	}
}

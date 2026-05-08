using System;
using Audio.SpatialSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/ReverbMixerDataSO", fileName = "ReverbMixerData")]
public class ReverbMixerDataSO : ScriptableObject, IEnvironmentMixerParamsHandler
{
	public MixerEnvironmentParameter reverbVolume;

	public MixerEnvironmentParameter reverbDryLevel;

	public MixerEnvironmentParameter mix;

	public MixerEnvironmentParameter reverbDelay;

	public MixerEnvironmentParameter reflectDelay;

	public MixerEnvironmentParameter reflectMix;

	public MixerEnvironmentParameter roomSize;

	public MixerEnvironmentParameter decayTime;

	public MixerEnvironmentParameter decayHFRatio;

	public MixerEnvironmentParameter roomLowFrequencyGain;

	public MixerEnvironmentParameter lowFrequencyReference;

	public MixerEnvironmentParameter roomHighFrequencyGain;

	public MixerEnvironmentParameter highFrequencyReference;

	public void Apply(AudioMixer mixer, EAudioRoomTypeMask roomType)
	{
		method_0(reverbVolume, mixer, roomType);
		method_0(reverbDryLevel, mixer, roomType);
		method_0(mix, mixer, roomType);
		method_0(reverbDelay, mixer, roomType);
		method_0(reflectDelay, mixer, roomType);
		method_0(reflectMix, mixer, roomType);
		method_0(roomSize, mixer, roomType);
		method_0(decayTime, mixer, roomType);
		method_0(decayHFRatio, mixer, roomType);
		method_0(roomLowFrequencyGain, mixer, roomType);
		method_0(lowFrequencyReference, mixer, roomType);
		method_0(roomHighFrequencyGain, mixer, roomType);
		method_0(highFrequencyReference, mixer, roomType);
	}

	public void method_0(MixerEnvironmentParameter parameter, AudioMixer mixer, EAudioRoomTypeMask roomType)
	{
		if (parameter.parametersByRoom.TryGetValue(roomType, out var value))
		{
			mixer.SetFloat(parameter.paramName, value);
		}
	}
}

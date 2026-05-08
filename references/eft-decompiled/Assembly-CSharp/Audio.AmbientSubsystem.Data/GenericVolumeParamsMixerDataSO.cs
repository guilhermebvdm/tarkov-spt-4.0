using System;
using Audio.SpatialSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/GenericVolumeParamsMixerDataSO", fileName = "GenericVolumeParamsMixerData")]
public class GenericVolumeParamsMixerDataSO : ScriptableObject, IEnvironmentMixerParamsHandler
{
	public MixerEnvironmentParameter[] mixerEnvironmentParameters = Array.Empty<MixerEnvironmentParameter>();

	public void Apply(AudioMixer mixer, EAudioRoomTypeMask roomType)
	{
		MixerEnvironmentParameter[] array = mixerEnvironmentParameters;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Apply(mixer, roomType);
		}
	}
}

using System;
using Audio.SpatialSystem;
using UnityEngine.Audio;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
public class MixerEnvironmentParameter : IEnvironmentMixerParamsHandler
{
	public string paramName;

	public MixerEnvironmentParametersByRoom parametersByRoom = new MixerEnvironmentParametersByRoom
	{
		{
			EAudioRoomTypeMask.IndoorGeneric,
			0f
		},
		{
			EAudioRoomTypeMask.IndoorIsolated,
			0f
		},
		{
			EAudioRoomTypeMask.Outdoor,
			0f
		}
	};

	public void Apply(AudioMixer mixer, EAudioRoomTypeMask roomType)
	{
		if (parametersByRoom.TryGetValue(roomType, out var value))
		{
			mixer.SetFloat(paramName, value);
		}
	}
}

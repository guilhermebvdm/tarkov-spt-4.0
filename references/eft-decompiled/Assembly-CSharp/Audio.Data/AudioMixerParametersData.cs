using System;
using Audio.AmbientSubsystem.Data;
using UnityEngine;

namespace Audio.Data;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/AudioMixerParametersData", fileName = "AudioMixerParametersData")]
public class AudioMixerParametersData : ScriptableObject
{
	[SerializeField]
	private AudioMixerParametersByType MixerParametersByType;

	public bool TryGetParameters(EMixerParameterType parameterType, out AudioMixerParamsContainer mixerParams)
	{
		return MixerParametersByType.TryGetValue(parameterType, out mixerParams);
	}
}

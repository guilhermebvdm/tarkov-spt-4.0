using System;
using UnityEngine;

namespace Audio.AmbientSubsystem.Data;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/MixerEffectsDataSO", fileName = "MixerEffectsData")]
public class MixerEffectsDataSO : ScriptableObject
{
	public ReverbMixerDataSO reverbMixerData;

	public DelayMixerDataSO delayMixerData;

	public GenericVolumeParamsMixerDataSO effectsSendsMixerData;

	public GenericVolumeParamsMixerDataSO effectsVolumesMixerData;
}

using System;
using EFT.ActiveHeadphones;
using UnityEngine;

namespace Audio.ActiveHeadphones.Debug;

[Serializable]
public class EditorHeadphonesTemplate
{
	[HideInInspector]
	public string Name;

	public EHeadphonesType HeadphonesType;

	public float RolloffMultiplier = 1f;

	public float GunsCompressorSendLevel = -80f;

	public float ClientPlayerCompressorSendLevel = -80f;

	public float ObservedPlayerCompressorSendLevel = -80f;

	public float NpcCompressorSendLevel = -80f;

	public float EnvTechnicalCompressorSendLevel = -80f;

	public float EnvNatureCompressorSendLevel = -80f;

	public float EnvCommonCompressorSendLevel = -80f;

	public float AmbientCompressorSendLevel = -80f;

	public float EffectsReturnsCompressorSendLevel = -80f;

	public float HeadphonesMixerVolume;

	public float AmbientVolume;

	public float DryVolume;

	public float EffectsReturnsGroupVolume;

	public float Distortion;

	public float CompressorThreshold;

	public float CompressorAttack;

	public float CompressorRelease;

	public float CompressorGain;

	public int HighpassFreq;

	public float HighpassResonance;

	public int LowpassFreq = 22000;

	public EQBand EQBand1 = new EQBand(200f, 1f, 1f);

	public EQBand EQBand2 = new EQBand(1000f, 1f, 1f);

	public EQBand EQBand3 = new EQBand(8000f, 1f, 1f);

	public EditorHeadphonesTemplate Clone()
	{
		return new EditorHeadphonesTemplate
		{
			Name = Name,
			HeadphonesType = HeadphonesType,
			GunsCompressorSendLevel = GunsCompressorSendLevel,
			ClientPlayerCompressorSendLevel = ClientPlayerCompressorSendLevel,
			ObservedPlayerCompressorSendLevel = ObservedPlayerCompressorSendLevel,
			NpcCompressorSendLevel = NpcCompressorSendLevel,
			EnvTechnicalCompressorSendLevel = EnvTechnicalCompressorSendLevel,
			EnvNatureCompressorSendLevel = EnvNatureCompressorSendLevel,
			EnvCommonCompressorSendLevel = EnvCommonCompressorSendLevel,
			AmbientCompressorSendLevel = AmbientCompressorSendLevel,
			EffectsReturnsCompressorSendLevel = EffectsReturnsCompressorSendLevel,
			Distortion = Distortion,
			CompressorThreshold = CompressorThreshold,
			CompressorAttack = CompressorAttack,
			CompressorRelease = CompressorRelease,
			CompressorGain = CompressorGain,
			HighpassFreq = HighpassFreq,
			LowpassFreq = LowpassFreq,
			HighpassResonance = HighpassResonance,
			HeadphonesMixerVolume = HeadphonesMixerVolume,
			AmbientVolume = AmbientVolume,
			DryVolume = DryVolume,
			RolloffMultiplier = RolloffMultiplier,
			EffectsReturnsGroupVolume = EffectsReturnsGroupVolume,
			EQBand1 = new EQBand(EQBand1.Frequency, EQBand1.Gain, EQBand1.Q),
			EQBand2 = new EQBand(EQBand2.Frequency, EQBand2.Gain, EQBand2.Q),
			EQBand3 = new EQBand(EQBand3.Frequency, EQBand3.Gain, EQBand3.Q)
		};
	}

	public HeadphonesTemplateClass CreateRealTemplate()
	{
		return new HeadphonesTemplateClass
		{
			GunsCompressorSendLevel = GunsCompressorSendLevel,
			ClientPlayerCompressorSendLevel = ClientPlayerCompressorSendLevel,
			ObservedPlayerCompressorSendLevel = ObservedPlayerCompressorSendLevel,
			NpcCompressorSendLevel = NpcCompressorSendLevel,
			EnvTechnicalCompressorSendLevel = EnvTechnicalCompressorSendLevel,
			EnvNatureCompressorSendLevel = EnvNatureCompressorSendLevel,
			EnvCommonCompressorSendLevel = EnvCommonCompressorSendLevel,
			AmbientCompressorSendLevel = AmbientCompressorSendLevel,
			EffectsReturnsCompressorSendLevel = EffectsReturnsCompressorSendLevel,
			Distortion = Distortion,
			CompressorThreshold = CompressorThreshold,
			CompressorAttack = CompressorAttack,
			CompressorRelease = CompressorRelease,
			CompressorGain = CompressorGain,
			HighpassFreq = HighpassFreq,
			LowpassFreq = LowpassFreq,
			HighpassResonance = HighpassResonance,
			HeadphonesMixerVolume = HeadphonesMixerVolume,
			AmbientVolume = AmbientVolume,
			DryVolume = DryVolume,
			RolloffMultiplier = RolloffMultiplier,
			EffectsReturnsGroupVolume = EffectsReturnsGroupVolume,
			EQBand1Frequency = EQBand1.Frequency,
			EQBand1Gain = EQBand1.Gain,
			EQBand1Q = EQBand1.Q,
			EQBand2Frequency = EQBand2.Frequency,
			EQBand2Gain = EQBand2.Gain,
			EQBand2Q = EQBand2.Q,
			EQBand3Frequency = EQBand3.Frequency,
			EQBand3Gain = EQBand3.Gain,
			EQBand3Q = EQBand3.Q
		};
	}
}

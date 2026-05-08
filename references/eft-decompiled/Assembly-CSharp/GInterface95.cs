using System;
using CommonAssets.Scripts.Audio;

public interface GInterface95 : IDisposable
{
	void Play(EAudioMovementState movementState, EnvironmentType environment, float distance = 0f, float baseStepVolume = 1f, float blendParameter = 0f, bool stereo = false);

	void UpdateSoundRolloff(float rolloff);

	void UpdateSurface(BaseBallistic.ESurfaceSound surface);

	void UpdateUnderRoofStatus(bool isUnderRoof);
}

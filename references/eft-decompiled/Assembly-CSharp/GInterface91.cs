using System;
using CommonAssets.Scripts.Audio;
using UnityEngine;

public interface GInterface91 : IDisposable
{
	bool IsFading { get; }

	void StartFade(string mixerParameter, float targetValue, EAudioFadeType fadeType = EAudioFadeType.Linear, float duration = 0f);

	void StartFade(string mixerParameter, float targetValue, AnimationCurve fadeCurve, float duration = 0f);

	void StopFade();
}

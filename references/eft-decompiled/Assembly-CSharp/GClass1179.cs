using System;
using System.Collections;
using System.Collections.Generic;
using CommonAssets.Scripts.Audio;
using UnityEngine;
using UnityEngine.Audio;

public class GClass1179 : GInterface91, IDisposable
{
	[NonSerialized]
	public Dictionary<string, Coroutine> Dictionary_0 = new Dictionary<string, Coroutine>();

	[NonSerialized]
	public AudioMixer AudioMixer_0;

	[NonSerialized]
	public MonoBehaviour MonoBehaviour_0;

	[NonSerialized]
	public short Short_0;

	public bool IsFading => Short_0 > 0;

	public GClass1179(MonoBehaviour behaviour, AudioMixer audioMixer)
	{
		MonoBehaviour_0 = behaviour;
		AudioMixer_0 = audioMixer;
	}

	public void StartFade(string mixerParameter, float targetValue, AnimationCurve fadeCurve, float duration = 0f)
	{
		method_0(mixerParameter, targetValue, duration, fadeCurve.Evaluate);
	}

	public void StartFade(string mixerParameter, float targetValue, EAudioFadeType fadeType = EAudioFadeType.Linear, float duration = 0f)
	{
		method_0(mixerParameter, targetValue, duration, GClass2313.GetEasingFunction(fadeType));
	}

	public void StopFade()
	{
		method_5();
	}

	public void method_0(string mixerParameter, float targetValue, float duration, Func<float, float> easingFunction)
	{
		if (Dictionary_0.TryGetValue(mixerParameter, out var value))
		{
			method_4(ref value);
		}
		if (duration <= 0f)
		{
			method_3(mixerParameter, targetValue);
			return;
		}
		Dictionary_0[mixerParameter] = MonoBehaviour_0.StartCoroutine(method_1(mixerParameter, targetValue, duration, easingFunction));
		Short_0++;
	}

	public IEnumerator method_1(string mixerParameter, float targetValue, float duration, Func<float, float> easingFunction)
	{
		float num = 0f;
		float a = method_2(mixerParameter);
		while (num < duration)
		{
			num += Time.deltaTime;
			float arg = Mathf.Clamp01(num / duration);
			float t = easingFunction(arg);
			float volume = Mathf.Lerp(a, targetValue, t);
			method_3(mixerParameter, volume);
			yield return null;
		}
		method_3(mixerParameter, targetValue);
		if (Short_0 > 0)
		{
			Short_0--;
		}
	}

	public float method_2(string mixerParameter)
	{
		AudioMixer_0.GetFloat(mixerParameter, out var value);
		return value;
	}

	public void method_3(string mixerParameter, float volume)
	{
		AudioMixer_0.SetFloat(mixerParameter, volume);
	}

	public void method_4(ref Coroutine coroutine)
	{
		if (coroutine != null && !(MonoBehaviour_0 == null))
		{
			MonoBehaviour_0.StopCoroutine(coroutine);
			coroutine = null;
			if (Short_0 > 0)
			{
				Short_0--;
			}
		}
	}

	public void method_5()
	{
		foreach (KeyValuePair<string, Coroutine> item in Dictionary_0)
		{
			var (_, coroutine2) = (KeyValuePair<string, Coroutine>)(ref item);
			method_4(ref coroutine2);
		}
		Dictionary_0.Clear();
		Short_0 = 0;
	}

	public void Dispose()
	{
		method_5();
	}
}

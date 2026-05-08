using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using uLipSync;

namespace Cutscene;

public class LipSyncPlayer : MonoBehaviour
{
	private SkinnedMeshRenderer skinnedMeshRenderer_0;

	private BakedData bakedData_0;

	private BakedDataWithCurves.CurveData[] curveData_0;

	private List<uLipSyncBlendShape.BlendShapeInfo> list_0;

	[SerializeField]
	private float minVolume = -2.5f;

	[SerializeField]
	private float maxVolume = -1.5f;

	[SerializeField]
	private float smoothness = 0.05f;

	[SerializeField]
	private float timeOffset = 0.1f;

	[SerializeField]
	private float playVolume = 1f;

	private AudioClip audioClip_0;

	private AudioSource audioSource_0;

	private Animator animator_0;

	private bool bool_0 = true;

	private float float_0;

	private float float_1;

	private bool bool_1;

	private float float_2;

	private float float_3 = 1f / 60f;

	private float float_4;

	private Dictionary<string, int> dictionary_0;

	private Action<string> action_0;

	private Action action_1;

	private Coroutine coroutine_0;

	private LipSyncInfo lipSyncInfo_0;

	[CompilerGenerated]
	private Action action_2;

	[CompilerGenerated]
	private Action action_3;

	[CompilerGenerated]
	private static Action<string> action_4;

	[CompilerGenerated]
	private bool bool_2;

	public bool IsPlaying
	{
		[CompilerGenerated]
		get
		{
			return bool_2;
		}
		[CompilerGenerated]
		set
		{
			bool_2 = value;
		}
	}

	public bool IsAllowPlaySound
	{
		get
		{
			return bool_0;
		}
		set
		{
			bool_0 = value;
			if (!bool_0)
			{
				method_8();
			}
		}
	}

	public event Action OnStartPlay
	{
		[CompilerGenerated]
		add
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnStopPlay
	{
		[CompilerGenerated]
		add
		{
			Action action = action_3;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_3;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static event Action<string> TestOnStartPlay
	{
		[CompilerGenerated]
		add
		{
			Action<string> action = action_4;
			Action<string> action2;
			do
			{
				action2 = action;
				Action<string> value2 = (Action<string>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<string> action = action_4;
			Action<string> action2;
			do
			{
				action2 = action;
				Action<string> value2 = (Action<string>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void ApplySettings(GameObject source, GameObject target)
	{
		uLipSyncBlendShape component = source.GetComponent<uLipSyncBlendShape>();
		if (!(component == null) && !(component.skinnedMeshRenderer == null))
		{
			list_0 = component.blendShapes;
			minVolume = component.minVolume;
			maxVolume = component.maxVolume;
			smoothness = component.smoothness;
			component.updateMethod = UpdateMethod.External;
			skinnedMeshRenderer_0 = base.transform.Find(component.skinnedMeshRenderer.gameObject.name).GetComponent<SkinnedMeshRenderer>();
			uLipSyncBakedDataPlayer component2 = source.GetComponent<uLipSyncBakedDataPlayer>();
			if (component2 != null)
			{
				playVolume = component2.volume;
				timeOffset = component2.timeOffset;
				component2.playOnAwake = false;
				audioSource_0 = component2.audioSource;
			}
			animator_0 = target.GetComponent<Animator>();
			if (audioSource_0 == null)
			{
				audioSource_0 = target.GetComponent<AudioSource>();
			}
			bool_1 = animator_0 != null && list_0 != null;
		}
	}

	public void StartPlay(BakedData bakedData)
	{
		if (bool_1)
		{
			bool num = StopPlay();
			curveData_0 = Array.Empty<BakedDataWithCurves.CurveData>();
			if (bakedData is BakedDataWithCurves { curves: not null } bakedDataWithCurves)
			{
				curveData_0 = bakedDataWithCurves.curves.ToArray();
			}
			bakedData_0 = bakedData;
			audioClip_0 = bakedData.audioClip;
			action_0?.Invoke(audioClip_0.name);
			action_0 = null;
			if (num)
			{
				float_1 = 0f;
				float_0 = 0f;
			}
			float_4 = 0f;
			float_2 = 0f;
			IsPlaying = true;
			if (IsAllowPlaySound)
			{
				method_5();
			}
			method_10();
			UpdatePlay(0f);
			action_2?.Invoke();
			action_4?.Invoke(bakedData.name);
		}
	}

	public void UpdatePlay(float normalizedTime)
	{
		if (bool_1)
		{
			if (Math.Abs(float_2 - normalizedTime) > Mathf.Epsilon)
			{
				float num = Mathf.Abs(float_2 - normalizedTime);
				float_3 = num * bakedData_0.duration;
				float_2 = normalizedTime;
			}
			else
			{
				float_3 = Time.deltaTime;
			}
			BakedFrame frame = bakedData_0.GetFrame(normalizedTime * bakedData_0.duration);
			frame.volume *= playVolume;
			LipSyncInfo lipSyncInfo = BakedData.GetLipSyncInfo(frame);
			method_9(normalizedTime);
			ApplyBakeFrame(lipSyncInfo);
			float_4 = bakedData_0.duration * normalizedTime;
		}
	}

	public void LateUpdate()
	{
		if (lipSyncInfo_0.phonemeRatios != null)
		{
			method_0();
			method_1();
			method_2();
			if (Mathf.Approximately(float_0, 0f))
			{
				method_3();
				lipSyncInfo_0 = default(LipSyncInfo);
			}
		}
	}

	public void ApplyBakeFrame(LipSyncInfo info)
	{
		lipSyncInfo_0 = info;
	}

	public void ContinueDetachedPlay()
	{
		coroutine_0 = StartCoroutine(method_7(float_4));
	}

	public bool StopPlay()
	{
		if (!bool_1)
		{
			return false;
		}
		bool result = false;
		if (coroutine_0 != null)
		{
			StopCoroutine(coroutine_0);
			coroutine_0 = null;
			result = true;
		}
		if (action_1 != null)
		{
			action_1();
			action_1 = null;
		}
		if (IsPlaying)
		{
			action_3?.Invoke();
		}
		method_10();
		method_8();
		float_4 = 0f;
		float_2 = 0f;
		IsPlaying = false;
		return result;
	}

	public void method_0()
	{
		float target = 0f;
		if (lipSyncInfo_0.rawVolume > 0f)
		{
			target = Mathf.Log10(lipSyncInfo_0.rawVolume);
			target = (target - minVolume) / Mathf.Max(maxVolume - minVolume, 0.0001f);
			target = Mathf.Clamp(target, 0f, 1f);
		}
		float_0 = method_4(float_0, target, ref float_1);
	}

	public void method_1()
	{
		float num = 0f;
		Dictionary<string, float> phonemeRatios = lipSyncInfo_0.phonemeRatios;
		foreach (uLipSyncBlendShape.BlendShapeInfo item in list_0)
		{
			float value = 0f;
			if (phonemeRatios != null && !string.IsNullOrEmpty(item.phoneme))
			{
				phonemeRatios.TryGetValue(item.phoneme, out value);
			}
			float velocity = item.weightVelocity;
			item.weight = method_4(item.weight, value, ref velocity);
			item.weightVelocity = velocity;
			num += item.weight;
		}
		foreach (uLipSyncBlendShape.BlendShapeInfo item2 in list_0)
		{
			item2.weight = ((num > 0f) ? (item2.weight / num) : 0f);
		}
	}

	public void method_2()
	{
		if (!skinnedMeshRenderer_0)
		{
			return;
		}
		foreach (uLipSyncBlendShape.BlendShapeInfo item in list_0)
		{
			if (item.index >= 0)
			{
				skinnedMeshRenderer_0.SetBlendShapeWeight(item.index, 0f);
			}
		}
		foreach (uLipSyncBlendShape.BlendShapeInfo item2 in list_0)
		{
			if (item2.index >= 0)
			{
				float blendShapeWeight = skinnedMeshRenderer_0.GetBlendShapeWeight(item2.index);
				blendShapeWeight += item2.weight * item2.maxWeight * float_0 * 100f;
				skinnedMeshRenderer_0.SetBlendShapeWeight(item2.index, blendShapeWeight);
			}
		}
	}

	public void method_3()
	{
		foreach (uLipSyncBlendShape.BlendShapeInfo item in list_0)
		{
			skinnedMeshRenderer_0.SetBlendShapeWeight(item.index, 0f);
		}
	}

	public float method_4(float value, float target, ref float velocity)
	{
		return Mathf.SmoothDamp(value, target, ref velocity, smoothness);
	}

	public void method_5()
	{
		audioSource_0.clip = audioClip_0;
		audioSource_0.volume = playVolume;
		audioSource_0.loop = false;
		audioSource_0.PlayDelayed(0.01f);
	}

	public void method_6(AnimationTrack data, Action<string> onStart, Action onFinish)
	{
		action_0 = onStart;
		StartPlay(data.GetBackedDataForPlay());
		action_1 = onFinish;
		coroutine_0 = StartCoroutine(method_7(0f));
	}

	public IEnumerator method_7(float startTime)
	{
		float_4 = startTime;
		float duration = bakedData_0.duration;
		double dspTime = AudioSettings.dspTime;
		while (float_4 < duration)
		{
			float normalizedTime = float_4 / duration;
			UpdatePlay(normalizedTime);
			float_4 = startTime + (float)(AudioSettings.dspTime - dspTime);
			yield return null;
		}
		coroutine_0 = null;
		StopPlay();
	}

	public void method_8()
	{
		audioSource_0.Stop();
	}

	public void method_9(float normalizedTime)
	{
		if (curveData_0 == null)
		{
			return;
		}
		BakedDataWithCurves.CurveData[] array = curveData_0;
		foreach (BakedDataWithCurves.CurveData curveData in array)
		{
			float value = curveData.curve.Evaluate(normalizedTime * bakedData_0.duration);
			if (curveData.isBlendShape)
			{
				skinnedMeshRenderer_0.SetBlendShapeWeight(curveData.blendShapeIndex, value);
			}
			else
			{
				animator_0.SetFloat(method_11(curveData.paramName), value);
			}
		}
	}

	public void method_10()
	{
		if (curveData_0 == null)
		{
			return;
		}
		BakedDataWithCurves.CurveData[] array = curveData_0;
		foreach (BakedDataWithCurves.CurveData curveData in array)
		{
			if (curveData.isBlendShape)
			{
				skinnedMeshRenderer_0.SetBlendShapeWeight(curveData.blendShapeIndex, curveData.resetValue);
			}
			else
			{
				animator_0.SetFloat(method_11(curveData.paramName), curveData.resetValue);
			}
		}
	}

	public int method_11(string param)
	{
		if (dictionary_0 == null)
		{
			dictionary_0 = new Dictionary<string, int>();
		}
		if (dictionary_0.TryGetValue(param, out var value))
		{
			return value;
		}
		return dictionary_0[param] = Animator.StringToHash(param);
	}

	public static LipSyncPlayer GetPlayer(GameObject target)
	{
		LipSyncPlayer lipSyncPlayer = target.GetComponent<LipSyncPlayer>();
		if (lipSyncPlayer == null)
		{
			lipSyncPlayer = target.AddComponent<LipSyncPlayer>();
			lipSyncPlayer.ApplySettings(target, target);
		}
		return lipSyncPlayer;
	}

	public static void Play(AnimationTrack data, GameObject target, Action<string> onStart, Action onFinish)
	{
		GetPlayer(target).method_6(data, onStart, onFinish);
	}
}

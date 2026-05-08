using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace EFT;

public class AudioArray : MonoBehaviour
{
	[CompilerGenerated]
	public class Class1528
	{
		public bool cancelled;

		public void method_0()
		{
			cancelled = true;
		}
	}

	[CompilerGenerated]
	public class Class1529
	{
		public bool isVolumetric;

		public AudioArray audioArray_0;

		public Action onCancelCallback;

		public Action originalCallback;

		public void method_0(bool volumetric)
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			if (isVolumetric == volumetric)
			{
				audioArray_0.Event_0 -= method_0;
				onCancelCallback?.Invoke();
				originalCallback?.Invoke();
			}
		}
	}

	[CompilerGenerated]
	public class Class1530
	{
		public List<AudioSource> sources;

		public AudioClip sequenceClip;

		public bool cancelled;

		public void method_0(float time, bool init = false)
		{
			foreach (AudioSource item in sources)
			{
				if (init)
				{
					item.DOKill();
				}
				if (!(item == null))
				{
					if (init && item.clip != sequenceClip)
					{
						item.loop = false;
						item.clip = sequenceClip;
					}
					item.time = time;
					if (init)
					{
						item.Play();
					}
					continue;
				}
				break;
			}
		}

		public void method_1()
		{
			cancelled = true;
		}
	}

	[SerializeField]
	private AudioSource _audioSource2D;

	private List<AudioSource> list_0 = new List<AudioSource>();

	[CompilerGenerated]
	private Action<bool> action_0;

	public event Action<bool> Event_0
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Awake()
	{
		Init();
	}

	public void OnDisable()
	{
		action_0?.Invoke(obj: true);
		action_0?.Invoke(obj: false);
	}

	public void OnDestroy()
	{
		action_0?.Invoke(obj: true);
		action_0?.Invoke(obj: false);
	}

	public void Init(float? overlap = null)
	{
		if (list_0.Count == 0)
		{
			list_0 = (from source in base.gameObject.GetComponentsInChildren<AudioSource>()
				where source != _audioSource2D
				select source).ToList();
		}
	}

	public async void PlaySequence(AudioSequence sequence, EAudioSequenceType sequenceType, bool volumetric, Action onCancel = null)
	{
		Class1530 CS_0024_003C_003E8__locals12 = new Class1530();
		Stop(volumetric);
		if (sequence == null || sequence.SequenceClip == null)
		{
			return;
		}
		CS_0024_003C_003E8__locals12.sources = method_1(volumetric);
		if (CS_0024_003C_003E8__locals12.sources.Count == 0)
		{
			return;
		}
		CS_0024_003C_003E8__locals12.sequenceClip = sequence.SequenceClip;
		if (CS_0024_003C_003E8__locals12.sequenceClip.loadState != AudioDataLoadState.Loaded)
		{
			CS_0024_003C_003E8__locals12.sequenceClip.LoadAudioData();
		}
		AudioSequence.GStruct283 sequenceSettings = sequence.GetSequenceSettings(sequenceType);
		bool flag = sequenceSettings.LoopLength >= double.Epsilon;
		if (sequenceSettings.Delay >= float.Epsilon)
		{
			await Task.Delay((int)(sequenceSettings.Delay * 1000f));
		}
		CS_0024_003C_003E8__locals12.cancelled = false;
		method_0(volumetric, delegate
		{
			CS_0024_003C_003E8__locals12.cancelled = true;
		}, onCancel);
		CS_0024_003C_003E8__locals12.method_0((float)sequenceSettings.StartTime, init: true);
		if (sequenceSettings.SkipTime >= double.Epsilon)
		{
			await Task.Delay((int)Math.Ceiling(sequenceSettings.SkipTime));
			if (CS_0024_003C_003E8__locals12.cancelled)
			{
				return;
			}
			CS_0024_003C_003E8__locals12.method_0((float)sequenceSettings.JumpTime);
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		await Task.Delay((int)Math.Ceiling(sequenceSettings.InitialLength * 1000.0));
		if (flag)
		{
			int value = (int)((double)stopwatch.ElapsedMilliseconds - sequenceSettings.InitialLength * 1000.0);
			int num = (int)(sequenceSettings.LoopLength * 1000.0);
			while (!CS_0024_003C_003E8__locals12.cancelled)
			{
				value = Mathf.Clamp(value, 0, num);
				CS_0024_003C_003E8__locals12.method_0((float)(sequenceSettings.LoopStartTime + (double)value / 1000.0));
				stopwatch.Restart();
				await Task.Delay(num - value);
				value = (int)(stopwatch.ElapsedMilliseconds + value - num);
			}
		}
	}

	public async void PlayWithOffset(AudioClip sound, bool volumetric, float offset, Action onFinish = null, Action onCancel = null)
	{
		Stop(volumetric);
		if (sound == null)
		{
			return;
		}
		bool cancelled = false;
		method_0(volumetric, delegate
		{
			cancelled = true;
		}, onCancel);
		List<AudioSource> list = method_1(volumetric);
		if (list.Count == 0)
		{
			return;
		}
		offset = Mathf.Clamp(offset, 0f, sound.length);
		foreach (AudioSource item in list)
		{
			item.clip = sound;
			item.loop = false;
			item.time = offset;
			item.Play();
		}
		await Task.Delay((int)((sound.length - offset) * 1000f));
		if (!cancelled)
		{
			onFinish?.Invoke();
		}
	}

	public async void PlayOneShot(AudioClip sound, bool volumetric, Action onFinish = null)
	{
		await PlayOneShotAsync(sound, volumetric, onFinish);
	}

	public async Task PlayOneShotAsync(AudioClip sound, bool volumetric, Action onFinish = null)
	{
		if (sound == null)
		{
			return;
		}
		List<AudioSource> list = method_1(volumetric);
		if (list.Count == 0)
		{
			return;
		}
		foreach (AudioSource item in list)
		{
			item.PlayOneShot(sound);
		}
		await Task.Delay((int)(sound.length * 1000f));
		onFinish?.Invoke();
	}

	public void PlayLoop(AudioClip sound, bool volumetric)
	{
		Stop(volumetric);
		if (sound == null)
		{
			return;
		}
		List<AudioSource> list = method_1(volumetric);
		if (list.Count == 0)
		{
			return;
		}
		foreach (AudioSource item in list)
		{
			item.clip = sound;
			item.loop = true;
			item.Play();
		}
	}

	public void method_0(bool isVolumetric, Action onCancelCallback, Action originalCallback = null)
	{
		Class1529 CS_0024_003C_003E8__locals9 = new Class1529();
		CS_0024_003C_003E8__locals9.isVolumetric = isVolumetric;
		CS_0024_003C_003E8__locals9.audioArray_0 = this;
		CS_0024_003C_003E8__locals9.onCancelCallback = onCancelCallback;
		CS_0024_003C_003E8__locals9.originalCallback = originalCallback;
		Event_0 += delegate(bool volumetric)
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			if (CS_0024_003C_003E8__locals9.isVolumetric == volumetric)
			{
				CS_0024_003C_003E8__locals9.audioArray_0.Event_0 -= CS_0024_003C_003E8__locals9.method_0;
				CS_0024_003C_003E8__locals9.onCancelCallback?.Invoke();
				CS_0024_003C_003E8__locals9.originalCallback?.Invoke();
			}
		};
	}

	public void Stop(bool volumetric)
	{
		action_0?.Invoke(volumetric);
		foreach (AudioSource item in method_1(volumetric))
		{
			item.Stop();
		}
	}

	public List<AudioSource> method_1(bool volumetric)
	{
		if (volumetric)
		{
			return list_0;
		}
		if (_audioSource2D != null)
		{
			return new List<AudioSource> { _audioSource2D };
		}
		return new List<AudioSource>();
	}

	[CompilerGenerated]
	public bool method_2(AudioSource source)
	{
		return source != _audioSource2D;
	}
}

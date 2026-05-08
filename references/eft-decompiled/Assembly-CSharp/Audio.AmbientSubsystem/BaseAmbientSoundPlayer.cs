using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.AmbientSubsystem;

[RequireComponent(typeof(AudioSource))]
public abstract class BaseAmbientSoundPlayer : MonoBehaviour, ISoundPlayer
{
	[Space]
	[SerializeField]
	[Range(0f, 1f)]
	private float _volume = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _deltaVolume;

	[SerializeField]
	private Vector2 _randomPitchRange = new Vector2(0.9f, 1.1f);

	[SerializeField]
	private float _fadeInDurationSec;

	[SerializeField]
	private float _fadeOutDurationSec;

	[Space]
	[SerializeField]
	private AudioMixerGroup _mixerGroup;

	[SerializeField]
	private bool ForceSetMixerGroup;

	[Space]
	[SerializeField]
	private bool _playOnAwake;

	[Header("Spatial Settings")]
	[Space]
	[SerializeField]
	[Range(0f, 1f)]
	private float _spread = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _spatialBlend = 1f;

	[SerializeField]
	private float _minDistance = 1f;

	[SerializeField]
	private float _maxDistance = 30f;

	[SerializeField]
	private AnimationCurve _rolloffCurve = AnimationCurve.Linear(1f, 1f, 0f, 0f);

	[SerializeField]
	private Vector2 _startPlayingDelay = new Vector2(0f, 20f);

	[SerializeField]
	private AnimationCurve _spreadCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private bool _useCustomSpreadCurve;

	private bool bool_0;

	private Coroutine coroutine_0;

	private Coroutine coroutine_1;

	private AudioSource audioSource_0;

	private Transform transform_0;

	[CompilerGenerated]
	private Action action_0;

	[CompilerGenerated]
	private Action action_1;

	[CompilerGenerated]
	private Action action_2;

	[CompilerGenerated]
	private bool bool_1;

	[CompilerGenerated]
	private GClass1178 gclass1178_0;

	[CompilerGenerated]
	private AudioClip audioClip_0;

	public Vector3 Position
	{
		get
		{
			return transform_0.position;
		}
		set
		{
			transform_0.position = value;
		}
	}

	public bool IsPlaying
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
		[CompilerGenerated]
		set
		{
			bool_1 = value;
		}
	}

	public GClass1178 Fader
	{
		[CompilerGenerated]
		get
		{
			return gclass1178_0;
		}
		[CompilerGenerated]
		set
		{
			gclass1178_0 = value;
		}
	}

	public AudioSource Source
	{
		get
		{
			if (audioSource_0 == null)
			{
				audioSource_0 = GClass6.GetOrAddComponent<AudioSource>(base.gameObject);
			}
			return audioSource_0;
		}
	}

	public AudioClip EmptyClip
	{
		[CompilerGenerated]
		get
		{
			return audioClip_0;
		}
		[CompilerGenerated]
		set
		{
			audioClip_0 = value;
		}
	}

	public event Action Stopped
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action Played
	{
		[CompilerGenerated]
		add
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action StartPlay
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

	public abstract AudioClip GetClip();

	public virtual float GetBaseVolume()
	{
		if (!(_deltaVolume > 0f))
		{
			return _volume;
		}
		return _volume + UnityEngine.Random.Range(0f - _deltaVolume, 0f);
	}

	public virtual float GetPitch()
	{
		return UnityEngine.Random.Range(_randomPitchRange.x, _randomPitchRange.y);
	}

	public float method_0()
	{
		return UnityEngine.Random.Range(_startPlayingDelay.x, _startPlayingDelay.y);
	}

	public void SetMixerGroup(AudioMixerGroup group)
	{
		if (!ForceSetMixerGroup || !(_mixerGroup != null))
		{
			_mixerGroup = group;
			if (Source != null)
			{
				Source.outputAudioMixerGroup = group;
			}
		}
	}

	public virtual void Awake()
	{
		transform_0 = base.transform;
		IsPlaying = false;
		EmptyClip = GClass2313.CreateEmptyMonoClip();
		Fader = new GClass1178();
		Source.playOnAwake = false;
		Source.outputAudioMixerGroup = _mixerGroup;
		Source.minDistance = _minDistance;
		Source.maxDistance = _maxDistance;
		Source.rolloffMode = AudioRolloffMode.Custom;
		Source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _rolloffCurve);
		UpdateSpread(_spread);
		if (_useCustomSpreadCurve)
		{
			Source.SetCustomCurve(AudioSourceCurveType.Spread, _spreadCurve);
		}
		UpdateSpatialBlend(_spatialBlend);
		bool_0 = true;
		if (_playOnAwake)
		{
			Play();
		}
	}

	public void Play()
	{
		GClass7.TryStopCoroutine(this, ref coroutine_0);
		GClass7.TryStopCoroutine(this, ref coroutine_1);
		if (!bool_0)
		{
			Debug.LogWarning(GetType().Name + " of object " + base.gameObject.name + " not initialized!");
		}
		else if (base.gameObject.activeInHierarchy)
		{
			Fader.Reset();
			FadeIn(_fadeInDurationSec);
			coroutine_0 = StartCoroutine(method_1());
			method_3();
		}
	}

	public void FadeIn(float fadeInSec)
	{
		Fader.FadeTo(GetBaseVolume(), fadeInSec);
	}

	public void FadeOut(float fadeOutSec)
	{
		Fader.FadeTo(0f, fadeOutSec);
	}

	public IEnumerator method_1()
	{
		yield return new WaitForSeconds(method_0());
		IsPlaying = true;
		action_2?.Invoke();
		while (IsPlaying)
		{
			float deltaTime = Time.deltaTime;
			Fader.Tick(deltaTime);
			Source.volume = Fader.Volume;
			OnPlay(deltaTime);
			yield return null;
		}
	}

	public virtual void OnPlay(float dt)
	{
		action_1?.Invoke();
	}

	public virtual bool CanPlay()
	{
		if (Source.enabled)
		{
			return Source.gameObject.activeInHierarchy;
		}
		return false;
	}

	public virtual void UpdateSpread(float value)
	{
		value = Mathf.Clamp01(value);
		if (_useCustomSpreadCurve)
		{
			value = _spreadCurve.Evaluate(value);
		}
		SetSpread(value);
	}

	public virtual void SetSpread(float value)
	{
		_spread = value;
		float spread = Mathf.Lerp(180f, 0f, value);
		Source.spread = spread;
	}

	public virtual void UpdateSpatialBlend(float value)
	{
		value = Mathf.Clamp01(value);
		Source.spatialBlend = value;
	}

	public virtual void Stop(bool force = false)
	{
		if (Source == null)
		{
			GClass7.TryStopCoroutine(this, ref coroutine_0);
			GClass7.TryStopCoroutine(this, ref coroutine_1);
			return;
		}
		FadeOut(_fadeOutDurationSec);
		if (force)
		{
			OnStop();
			return;
		}
		GClass7.TryStopCoroutine(this, ref coroutine_1);
		coroutine_1 = StartCoroutine(method_2());
	}

	public IEnumerator method_2(bool force = false)
	{
		while (Fader.Volume > Fader.EndVolume && !force)
		{
			yield return null;
		}
		OnStop();
	}

	public virtual void OnStop()
	{
		IsPlaying = false;
		GClass7.TryStopCoroutine(this, ref coroutine_1);
		GClass7.TryStopCoroutine(this, ref coroutine_0);
		Source.volume = Fader.EndVolume;
		Source.Stop();
		Source.clip = null;
		Fader?.Reset();
		method_4();
		action_0?.Invoke();
	}

	public void ScaleMaxDistance(float value)
	{
		Source.maxDistance = _maxDistance + value;
	}

	public void UpdateFadeOutTime(float value)
	{
		_fadeOutDurationSec = Mathf.Clamp(value, 0f, value);
	}

	public void method_3()
	{
		if (MonoBehaviourSingleton<AmbientAudioSystem>.Instantiated && _spatialBlend > 0f)
		{
			MonoBehaviourSingleton<AmbientAudioSystem>.Instance.RegisterInAudioCulling(Source, Source.loop);
		}
	}

	public void method_4()
	{
		if (MonoBehaviourSingleton<AmbientAudioSystem>.Instantiated)
		{
			MonoBehaviourSingleton<AmbientAudioSystem>.Instance.RemoveFromAudioCulling(Source);
		}
	}

	public virtual void OnDestroy()
	{
		Stop(force: true);
	}

	public BaseAmbientSoundPlayer()
	{
	}
}

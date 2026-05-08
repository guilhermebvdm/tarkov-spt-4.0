using System;
using System.Diagnostics;
using Audio.SpatialSystem;
using EFT;
using UnityEngine;
using UnityEngine.Audio;

public abstract class BetterSource : MonoBehaviour, GClass952.GInterface48, GInterface88, GInterface78
{
	public enum EPlayBackState
	{
		Playing,
		Stopped,
		Released
	}

	public interface GInterface35
	{
		void Release(BetterSource source);
	}

	private const float SQR_TOO_CLOSE_FOR_BINAURAL = 2.25f;

	private const float VOLUME_UPDATE_SPEED = 10f;

	public AudioSource source1;

	protected double EndPlaybackTime;

	protected AudioGroupPreset Preset;

	private Transform _transform;

	private bool _forceStereo;

	protected bool IncludedInOcclusionProcess = true;

	protected BaseSpatialAudioSource Spatializer;

	protected float PreOcclusionVolume = 1f;

	protected bool Initialized;

	public bool checkEveryFrame;

	private readonly GInterface72 _transformTracker = new GClass1111();

	private Action _unsubOnDestroy;

	private string _lastPlayingClipName = string.Empty;

	private GInterface84[] _audioFilters = Array.Empty<GInterface84>();

	private GInterface69[] _updatedComps = Array.Empty<GInterface69>();

	private bool _needUpdate = true;

	private bool _isSubscribed;

	private float _sqrEnableBinauralDist;

	private readonly GClass1178 _audioFader = new GClass1178();

	private bool _isFading;

	private bool _isFadingSubscribe;

	private Action _fadeOnDone;

	public EPlayBackState PlayBackState = EPlayBackState.Released;

	public GInterface35 ReleaseListener;

	public float OcclusionVolumeFactor { get; set; } = 1f;

	public AudioGroupOcclusionSettings SpatialSettings { get; set; }

	public float OcclusionRolloffScale { get; set; } = 1f;

	public bool IsActive { get; set; }

	public float FadeFactor { get; set; } = 1f;

	public Vector3 Position
	{
		get
		{
			return _transform.position;
		}
		set
		{
			_transform.position = value;
		}
	}

	public Vector3 LocalPosition
	{
		get
		{
			return _transform.localPosition;
		}
		set
		{
			_transform.localPosition = value;
		}
	}

	public float MaxDistance { get; set; }

	public bool UpdateVolume { get; set; } = true;

	public virtual bool EnableSpatialization
	{
		get
		{
			return Spatializer.EnableSpatialization;
		}
		set
		{
			Spatializer.EnableSpatialization = value;
		}
	}

	public virtual float HrtfIntensity
	{
		get
		{
			return Spatializer.HrtfIntensity;
		}
		set
		{
			Spatializer.HrtfIntensity = value;
		}
	}

	public virtual float DirectivityIntensity
	{
		get
		{
			return Spatializer.DirectivityIntensity;
		}
		set
		{
			Spatializer.DirectivityIntensity = value;
		}
	}

	public bool CanPlay
	{
		get
		{
			if (source1.enabled && base.gameObject.activeInHierarchy)
			{
				return !source1.mute;
			}
			return false;
		}
	}

	public float BaseVolume { get; set; }

	public virtual bool EnableReverb
	{
		get
		{
			return Spatializer.EnableReverb;
		}
		set
		{
			Spatializer.EnableReverb = value;
		}
	}

	public bool EnableDirectSound
	{
		get
		{
			return Spatializer.EnableDirectSound;
		}
		set
		{
			Spatializer.EnableDirectSound = value;
		}
	}

	public virtual float ReverbSendDB
	{
		get
		{
			return Spatializer.ReverbSendDB;
		}
		set
		{
			Spatializer.ReverbSendDB = value;
		}
	}

	public virtual float EarlyReflectionsSendDB
	{
		get
		{
			return Spatializer.EarlyReflectionsSendDB;
		}
		set
		{
			Spatializer.EarlyReflectionsSendDB = value;
		}
	}

	public virtual float ReverbReach
	{
		get
		{
			return Spatializer.ReverbReach;
		}
		set
		{
			Spatializer.ReverbReach = value;
		}
	}

	public abstract bool Loop { get; set; }

	public event Action<BetterSource> OnReleased;

	public event Action OnPlayed;

	public abstract AudioClip GetClip(int id);

	public abstract void SetClip(int id, AudioClip clip);

	public virtual void UpdateParameters()
	{
		Spatializer.UpdateParameters();
	}

	public virtual void ResetFilters()
	{
		GInterface84[] audioFilters = _audioFilters;
		for (int i = 0; i < audioFilters.Length; i++)
		{
			audioFilters[i].ResetValues();
		}
	}

	public void EnabledEQ(bool targetState)
	{
		GInterface84[] audioFilters = _audioFilters;
		for (int i = 0; i < audioFilters.Length; i++)
		{
			audioFilters[i].SetActive(targetState);
		}
	}

	public virtual void SetLowPassFilterParameters(float cutoffFreq = 1600f, float targetQ = 1f, bool applyImmediately = false)
	{
		if (IncludedInOcclusionProcess)
		{
			GInterface84[] audioFilters = _audioFilters;
			for (int i = 0; i < audioFilters.Length; i++)
			{
				audioFilters[i].SetTargetLowPassSettings(cutoffFreq, targetQ, applyImmediately);
			}
		}
	}

	public virtual void SetHighPassFilterParameters(float cutoffFreq = 100f, float targetQ = 1f, bool applyImmediately = false)
	{
		if (IncludedInOcclusionProcess)
		{
			GInterface84[] audioFilters = _audioFilters;
			for (int i = 0; i < audioFilters.Length; i++)
			{
				audioFilters[i].SetTargetHighPassSettings(cutoffFreq, targetQ, applyImmediately);
			}
		}
	}

	public virtual void EnableStereo(bool stereo = false)
	{
		_forceStereo = stereo;
		float spatialBlend = (stereo ? 0f : Preset.SpatialBlend);
		bool enabledSpat = !stereo && Preset.DirectBinaural && Preset.SteamSpatialize;
		SetSpatialBlend(spatialBlend);
		RefreshSpatialization(enabledSpat);
	}

	public abstract void SetSpatialBlend(float spatialBlend = 1f);

	public abstract void Mute(bool muted);

	public void IncludeInOcclusionProcess(bool included)
	{
		IncludedInOcclusionProcess = included;
	}

	public virtual void SetOcclusionVolumeFactor(float value)
	{
		if (IncludedInOcclusionProcess)
		{
			OcclusionVolumeFactor = Mathf.Clamp01(value);
		}
	}

	public void SetOcclusionRolloffScale(float value)
	{
		if (!IncludedInOcclusionProcess)
		{
			OcclusionRolloffScale = 1f;
			return;
		}
		OcclusionRolloffScale = Mathf.Clamp01(value);
		SetRolloff(MaxDistance);
	}

	public void ResetOcclusion()
	{
		SetOcclusionVolumeFactor(1f);
		ResetFilters();
	}

	public virtual void Release()
	{
		Stop();
		SetBaseVolume(1f);
		PlayBackState = EPlayBackState.Released;
		this.OnReleased?.Invoke(this);
		IncludeInOcclusionProcess(included: true);
		ResetOcclusion();
		_forceStereo = false;
		Mute(muted: false);
		RefreshSpatialization(Preset.SteamSpatialize && Preset.DirectBinaural);
		StopTrackingPosition();
		UpdateVolume = true;
		FadeFactor = 1f;
		_lastPlayingClipName = "";
		ReleaseListener?.Release(this);
	}

	public void Awake()
	{
		PlayBackState = EPlayBackState.Stopped;
		_transform = base.transform;
		OnAwake();
	}

	public virtual void OnAwake()
	{
		_unsubOnDestroy = (Action)Delegate.Combine(_unsubOnDestroy, GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3572>(method_9));
	}

	public void OnEnable()
	{
		if (Initialized)
		{
			method_6();
		}
	}

	public virtual void PlayScheduled(double time)
	{
		method_8();
		CheckBinauralAllowed();
		RefreshSpatialization(Preset.SteamSpatialize && Preset.DirectBinaural);
		PlayBackState = EPlayBackState.Playing;
	}

	public abstract void SetScheduledEndTime(double time);

	public abstract void SetPitch(float p);

	public virtual void Init()
	{
		_transform = base.transform;
		_audioFilters = GetComponentsInChildren<GInterface84>();
		_updatedComps = GetComponentsInChildren<GInterface69>();
		Spatializer = (base.gameObject.TryGetComponent<BaseSpatialAudioSource>(out var component) ? component : base.gameObject.AddComponent<FakeSpatialAudioSource>());
		Initialized = true;
	}

	public virtual void SetActive(bool active)
	{
		if (IsActive != active)
		{
			source1.enabled = active;
			GInterface69[] updatedComps = _updatedComps;
			for (int i = 0; i < updatedComps.Length; i++)
			{
				updatedComps[i].SetActive(active);
			}
			base.enabled = active;
			if (active)
			{
				method_6();
				ManualUpdate();
			}
			else
			{
				Stop();
				method_7();
				ResetOcclusion();
				PlayBackState = EPlayBackState.Stopped;
			}
			IsActive = active;
		}
	}

	public void EnabledUpdate(bool targetState)
	{
		if (_needUpdate == targetState)
		{
			return;
		}
		_needUpdate = targetState;
		if (IsActive)
		{
			if (targetState)
			{
				method_6();
			}
			else
			{
				method_7();
			}
		}
	}

	public virtual void Play(AudioClip clip1, AudioClip clip2, float balance, float volume = 1f, bool forceStereo = false, bool oneShot = true)
	{
		_lastPlayingClipName = ((clip1 == null) ? "" : clip1.name);
		EndPlaybackTime = CalculateEndPlaybackTime(clip1, clip2);
		PlayBackState = EPlayBackState.Playing;
		this.OnPlayed?.Invoke();
		_forceStereo = forceStereo;
		method_8();
		CheckBinauralAllowed();
	}

	public abstract void Clear(float spatial = 1f, float pitch = 1f);

	public abstract void Balance(float p);

	public abstract void Enable(bool p0);

	public abstract void SetRolloff(float distance);

	public abstract void SetMixerGroup(AudioMixerGroup mixerGroup);

	public bool VolumeFadeOut(float time, Action onDone = null)
	{
		if (!Initialized)
		{
			GClass722.Instance.LogWarn(base.gameObject.name + " is not initialized, can't start fade out");
			return false;
		}
		if (IsActive && base.gameObject.activeInHierarchy)
		{
			if (_isFading)
			{
				return false;
			}
			_isFading = true;
			_audioFader.Reset();
			_audioFader.FadeTo(1f, 0f);
			_audioFader.FadeTo(0f, time);
			FadeFactor = 1f;
			_fadeOnDone = onDone;
			method_4();
			return true;
		}
		method_3();
		return false;
	}

	public bool VolumeFadeIn(float time, Action onDone = null)
	{
		if (!Initialized)
		{
			GClass722.Instance.LogWarn(base.gameObject.name + " is not initialized, can't start fade in");
			return false;
		}
		if (IsActive && base.gameObject.activeInHierarchy)
		{
			if (_isFading)
			{
				return false;
			}
			_isFading = true;
			_audioFader.Reset();
			_audioFader.FadeTo(1f, time);
			FadeFactor = 0f;
			UpdateSourceVolume();
			_fadeOnDone = onDone;
			method_4();
			return true;
		}
		method_3();
		return false;
	}

	public virtual void UpdateSourceVolume(float speed = 1f)
	{
		float b = PreOcclusionVolume * OcclusionVolumeFactor * FadeFactor;
		source1.volume = Mathf.Clamp01(Mathf.Lerp(source1.volume, b, speed));
	}

	public virtual void EnabledHighPassFilter(bool enabledFilter)
	{
		GInterface84[] audioFilters = _audioFilters;
		for (int i = 0; i < audioFilters.Length; i++)
		{
			audioFilters[i].SetActiveHighPass(enabledFilter);
		}
	}

	public virtual void EnabledLowPassFilter(bool enabledFilter)
	{
		GInterface84[] audioFilters = _audioFilters;
		for (int i = 0; i < audioFilters.Length; i++)
		{
			audioFilters[i].SetActiveLowPass(enabledFilter);
		}
	}

	public virtual void SetBaseVolume(float volume)
	{
		volume *= Preset.OverallVolume;
		BaseVolume = volume;
	}

	public virtual void SetPriority(int priority)
	{
		source1.priority = priority;
	}

	public virtual void SetPreset(AudioGroupPreset preset)
	{
		Preset = preset;
		SpatialSettings = preset.occlusionSettings;
		GInterface84[] audioFilters = _audioFilters;
		for (int i = 0; i < audioFilters.Length; i++)
		{
			audioFilters[i].Init();
		}
		_sqrEnableBinauralDist = preset.EnableBinauralDist * preset.EnableBinauralDist;
	}

	public void SetParent(Transform parent, bool worldPositionStay = true)
	{
		base.gameObject.transform.SetParent(parent, worldPositionStay);
	}

	public void StartTrackingPosition(Transform tracker, Vector3 positionOffset = default(Vector3))
	{
		_transformTracker.StartTracking(tracker, positionOffset);
		method_0();
	}

	public void StopTrackingPosition()
	{
		_transformTracker.StopTracking();
	}

	public void Stop(float fadeSec = 0f)
	{
		if (!(fadeSec > 0f) || !VolumeFadeOut(fadeSec, OnStop))
		{
			OnStop();
		}
	}

	public virtual void OnStop()
	{
		method_3();
		source1.Stop();
		source1.clip = null;
		PlayBackState = EPlayBackState.Stopped;
	}

	public virtual void RefreshSpatialization(bool enabledSpat)
	{
		EnableSpatialization = enabledSpat;
	}

	[Conditional("UNITY_EDITOR")]
	public void CheckIfClipLoaded(AudioClip clip)
	{
		if (clip != null && clip.loadState != AudioDataLoadState.Loaded && clip.preloadAudioData)
		{
			UnityEngine.Debug.LogWarning("Clip " + clip.name + " trying to load during AudioSource.Play with state " + clip.loadState);
		}
	}

	public virtual float GetPitch(int clipIndex)
	{
		return source1.pitch;
	}

	public virtual double CalculateEndPlaybackTime(AudioClip clip1, AudioClip clip2, double delay = 0.0)
	{
		float pitch = GetPitch(0);
		float pitch2 = GetPitch(1);
		float a = ((clip1 != null) ? (clip1.length / pitch) : 0f);
		float b = ((clip2 != null) ? (clip2.length / pitch2) : 0f);
		float num = Mathf.Max(a, b);
		return AudioSettings.dspTime + (double)num + delay;
	}

	public void ManualUpdate()
	{
		if (!source1.enabled || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		method_0();
		GInterface69[] updatedComps = _updatedComps;
		for (int i = 0; i < updatedComps.Length; i++)
		{
			updatedComps[i].ManualUpdate();
		}
		if (checkEveryFrame || PlayBackState == EPlayBackState.Playing)
		{
			CheckBinauralAllowed();
			if (UpdateVolume && IncludedInOcclusionProcess && !_forceStereo)
			{
				UpdateSourceVolume(10f * Time.deltaTime);
			}
		}
		method_1();
		if (PlayBackState != EPlayBackState.Playing)
		{
			_lastPlayingClipName = string.Empty;
		}
	}

	public void method_0()
	{
		if (_transformTracker.IsPositionTracked)
		{
			_transformTracker.UpdatePosition();
			Position = _transformTracker.Position;
		}
	}

	public void method_1()
	{
		if (PlayBackState == EPlayBackState.Playing && AudioSettings.dspTime > EndPlaybackTime && !source1.isPlaying)
		{
			PlayBackState = EPlayBackState.Stopped;
		}
		else if (PlayBackState != EPlayBackState.Playing && source1.isPlaying)
		{
			PlayBackState = EPlayBackState.Playing;
		}
	}

	public virtual void CheckBinauralAllowed()
	{
		if (_forceStereo)
		{
			if (EnableSpatialization)
			{
				RefreshSpatialization(enabledSpat: false);
			}
		}
		else
		{
			if (!Preset.DirectBinaural || !Preset.DisableBinauralByDist)
			{
				return;
			}
			CameraClass instance = CameraClass.Instance;
			float num = instance.SqrDistance(Position);
			bool enabledSpat = num > _sqrEnableBinauralDist;
			bool num2 = num < 2.25f;
			if (num2)
			{
				enabledSpat = false;
			}
			if (!num2 && instance.Camera != null)
			{
				Transform transform = instance.Camera.transform;
				Vector3 forward = transform.forward;
				Vector3 vector = Position - transform.position;
				float num3 = Vector2.SignedAngle(new Vector2(forward.x, forward.z), new Vector2(vector.x, vector.z));
				if (num3 > 90f || num3 < -90f)
				{
					enabledSpat = true;
				}
				if (Mathf.Abs(num3) < Preset.AngleToAllowBinaural)
				{
					enabledSpat = false;
				}
				if (Mathf.Abs(transform.position.y - Position.y) > Preset.HeightDiffToAllowBinaural)
				{
					enabledSpat = true;
				}
			}
			RefreshSpatialization(enabledSpat);
		}
	}

	public void method_2()
	{
		_audioFader.Tick(Time.deltaTime);
		FadeFactor = _audioFader.Volume;
		UpdateSourceVolume();
		if (Mathf.Abs(FadeFactor - _audioFader.EndVolume) < 0.001f)
		{
			method_3();
		}
	}

	public void method_3()
	{
		if (_isFading)
		{
			method_5();
			_isFading = false;
			_fadeOnDone?.Invoke();
			FadeFactor = 1f;
		}
		_fadeOnDone = null;
	}

	public void method_4()
	{
		if (!_isFadingSubscribe)
		{
			StaticManager instance = StaticManager.Instance;
			if (instance != null)
			{
				instance.StaticUpdate += method_2;
				_isFadingSubscribe = true;
			}
		}
	}

	public void method_5()
	{
		if (_isFadingSubscribe)
		{
			StaticManager instance = StaticManager.Instance;
			if (instance != null)
			{
				instance.StaticUpdate -= method_2;
			}
			_isFadingSubscribe = false;
		}
	}

	public void OnDisable()
	{
		method_7();
		method_3();
	}

	public void OnDestroy()
	{
		StopTrackingPosition();
		SetActive(active: false);
		_unsubOnDestroy?.Invoke();
		_unsubOnDestroy = null;
	}

	public void method_6()
	{
		if (_needUpdate && !_isSubscribed)
		{
			StaticManager instance = StaticManager.Instance;
			if (instance != null)
			{
				instance.StaticUpdate += ManualUpdate;
			}
			_isSubscribed = true;
		}
	}

	public void method_7()
	{
		if (_isSubscribed)
		{
			StaticManager instance = StaticManager.Instance;
			if (instance != null)
			{
				instance.StaticUpdate -= ManualUpdate;
			}
			_isSubscribed = false;
		}
	}

	public void method_8()
	{
	}

	public void method_9(GClass3572 metaXRPluginErrorEvent)
	{
		if (!string.IsNullOrEmpty(_lastPlayingClipName))
		{
			GClass722.Instance.LogWarn("reverb plugin error playing sound: " + _lastPlayingClipName);
		}
	}

	public BetterSource()
	{
	}
}

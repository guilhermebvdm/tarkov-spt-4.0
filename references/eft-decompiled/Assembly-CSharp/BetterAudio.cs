using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Audio;
using Audio.BackendSettings;
using Audio.Effects;
using Audio.ReverbSubsystem;
using Audio.SpatialSystem;
using Comfort.Common;
using DG.Tweening;
using EFT;
using EFT.DataProviding;
using EFT.Interactive;
using EFT.ItemGameSounds;
using JetBrains.Annotations;
using JsonType;
using UnityEngine;
using UnityEngine.Audio;

public class BetterAudio : MonoBehaviourSingleton<BetterAudio>
{
	public enum AudioSourceGroupType
	{
		Gunshots,
		Weaponry,
		Impacts,
		Character,
		Environment,
		Collisions,
		Speech,
		Distant,
		NonspatialBypass,
		Nonspatial,
		Voip,
		Grenades,
		Windows,
		VehicleInSpeech,
		Lamp,
		InteractiveObjects,
		OutEnvironment,
		PropagatedExclusive,
		LightOcclusion
	}

	public interface IAudioSourceGroups
	{
		AudioGroupPreset Preset { get; }

		AudioMixerGroup MixerGroup { get; set; }

		[CanBeNull]
		BetterSource BorrowSource(bool activateSource = true, bool needUpdate = true);

		void RefreshSourceSettings();

		void SetSpatialize(bool val);
	}

	public class GClass886<T> : BetterSource.GInterface35, IAudioSourceGroups where T : BetterSource
	{
		[NonSerialized]
		public T Gparam_0;

		[NonSerialized]
		[CompilerGenerated]
		public AudioGroupPreset AudioGroupPreset_0;

		[NonSerialized]
		[CompilerGenerated]
		public AudioMixerGroup AudioMixerGroup_0;

		public AudioGroupPreset Preset
		{
			[CompilerGenerated]
			get
			{
				return AudioGroupPreset_0;
			}
			[CompilerGenerated]
			set
			{
				AudioGroupPreset_0 = value;
			}
		}

		public AudioMixerGroup MixerGroup
		{
			[CompilerGenerated]
			get
			{
				return AudioMixerGroup_0;
			}
			[CompilerGenerated]
			set
			{
				AudioMixerGroup_0 = value;
			}
		}

		public GClass886(AudioMixerGroup mixerGroup, GameObject prefab, AudioGroupPreset preset)
		{
			MixerGroup = mixerGroup;
			Preset = preset;
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
			AudioSource component = gameObject.GetComponent<AudioSource>();
			component.rolloffMode = AudioRolloffMode.Custom;
			component.SetCustomCurve(AudioSourceCurveType.CustomRolloff, Preset.SoundRolloff);
			component.spatialBlend = 0f;
			component.reverbZoneMix = 0f;
			component.SetCustomCurve(AudioSourceCurveType.Spread, Preset.SpreadCurve);
			component.outputAudioMixerGroup = MixerGroup;
			T val = gameObject.AddComponent<T>();
			val.Init();
			val.SetPreset(Preset);
			val.SetParent(MonoBehaviourSingleton<BetterAudio>.Instance.transform, worldPositionStay: false);
			val.SetPitch(1f);
			val.SetPriority(Preset.AudioSourcePriority);
			val.source1.loop = false;
			val.SetActive(active: false);
			Gparam_0 = val;
		}

		public BetterSource BorrowSource(bool activateSource = true, bool needUpdate = true)
		{
			Gparam_0.EnabledUpdate(needUpdate);
			Gparam_0.SetActive(activateSource);
			Gparam_0.ResetOcclusion();
			Gparam_0.ReleaseListener = this;
			return Gparam_0;
		}

		public void Release(BetterSource source)
		{
			Gparam_0.ReleaseListener = null;
			if (!Gparam_0.source1.isPlaying)
			{
				Gparam_0.SetActive(active: false);
			}
		}

		public void RefreshSourceSettings()
		{
		}

		public void SetSpatialize(bool val)
		{
		}
	}

	public class GClass887<T> : BetterSource.GInterface35, IAudioSourceGroups where T : BetterSource
	{
		[NonSerialized]
		public Stack<T> Stack_0 = new Stack<T>();

		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public GameObject GameObject_0;

		[NonSerialized]
		[CompilerGenerated]
		public AudioGroupPreset AudioGroupPreset_0;

		[NonSerialized]
		[CompilerGenerated]
		public AudioMixerGroup AudioMixerGroup_0;

		public AudioGroupPreset Preset
		{
			[CompilerGenerated]
			get
			{
				return AudioGroupPreset_0;
			}
			[CompilerGenerated]
			set
			{
				AudioGroupPreset_0 = value;
			}
		}

		public AudioMixerGroup MixerGroup
		{
			[CompilerGenerated]
			get
			{
				return AudioMixerGroup_0;
			}
			[CompilerGenerated]
			set
			{
				AudioMixerGroup_0 = value;
			}
		}

		public float Single_0 => Preset.SpatialBlend;

		public GClass887(AudioMixerGroup mixerGroup, GameObject prefab, AudioGroupPreset preset, bool spatialization = false)
		{
			MixerGroup = mixerGroup;
			Preset = preset;
			GameObject_0 = prefab;
			for (int i = 0; i < preset.PreCachedSourcesCount; i++)
			{
				T item = InstantiateNewSource(spatialization);
				Stack_0.Push(item);
			}
		}

		public virtual T InstantiateNewSource(bool spatialization = false)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(GameObject_0);
			AudioSource component = gameObject.GetComponent<AudioSource>();
			component.rolloffMode = AudioRolloffMode.Custom;
			component.SetCustomCurve(AudioSourceCurveType.CustomRolloff, Preset.SoundRolloff);
			component.spatialBlend = Single_0;
			component.SetCustomCurve(AudioSourceCurveType.Spread, Preset.SpreadCurve);
			component.outputAudioMixerGroup = MixerGroup;
			T orAddComponent = GClass6.GetOrAddComponent<T>(gameObject);
			orAddComponent.Init();
			orAddComponent.SetPreset(Preset);
			orAddComponent.SetParent(MonoBehaviourSingleton<BetterAudio>.Instance.transform, worldPositionStay: false);
			orAddComponent.SetPitch(1f);
			orAddComponent.SetPriority(Preset.AudioSourcePriority);
			orAddComponent.SetRolloff(Preset.DefaultMaxDistance);
			orAddComponent.SetMixerGroup(MixerGroup);
			orAddComponent.EnableSpatialization = spatialization;
			orAddComponent.SetActive(active: false);
			return orAddComponent;
		}

		public BetterSource BorrowSource(bool activateSource = true, bool needUpdate = true)
		{
			BetterSource betterSource = null;
			if (Stack_0.Count > 0)
			{
				betterSource = Stack_0.Pop();
			}
			else
			{
				if (Int_0 > Preset.SourcesCountLimit)
				{
					GClass722.Instance.LogWarn($"Instances of {Preset.name} over limit: {Preset.SourcesCountLimit}, instantiate new, total count: {Int_0}");
				}
				betterSource = InstantiateNewSource();
			}
			betterSource.ReleaseListener = this;
			betterSource.SetActive(activateSource);
			betterSource.SetBaseVolume(1f);
			betterSource.ResetOcclusion();
			betterSource.EnabledUpdate(needUpdate);
			return betterSource;
		}

		public virtual void RefreshSourceSettings()
		{
		}

		public virtual void SetSpatialize(bool val)
		{
		}

		public virtual void Release(BetterSource source)
		{
			if (source == null)
			{
				Debug.Log("source == null");
				return;
			}
			if (!Singleton<BetterAudio>.Instantiated)
			{
				UnityEngine.Object.Destroy(source.gameObject);
				return;
			}
			source.ReleaseListener = null;
			source.Clear(Single_0);
			source.SetParent(MonoBehaviourSingleton<BetterAudio>.Instance.transform, worldPositionStay: false);
			source.SetActive(active: false);
			source.EnabledUpdate(targetState: true);
			Stack_0.Push(source as T);
		}
	}

	public class Class510<T>(AudioMixerGroup mixerGroup, GameObject prefab, AudioGroupPreset preset) : GClass887<T>(mixerGroup, prefab, preset, true) where T : BetterSource
	{
		[NonSerialized]
		public List<BaseSpatialAudioSource> List_0 = new List<BaseSpatialAudioSource>();

		public override T InstantiateNewSource(bool spatialization = false)
		{
			T val = base.InstantiateNewSource(spatialization: true);
			BaseSpatialAudioSource[] componentsInChildren = val.GetComponentsInChildren<BaseSpatialAudioSource>();
			foreach (BaseSpatialAudioSource baseSpatialAudioSource in componentsInChildren)
			{
				AudioSource component = baseSpatialAudioSource.GetComponent<AudioSource>();
				component.dopplerLevel = 0f;
				method_0(baseSpatialAudioSource, component);
				val.SetPreset(base.Preset);
				List_0.Add(baseSpatialAudioSource);
			}
			return val;
		}

		public override void RefreshSourceSettings()
		{
			base.RefreshSourceSettings();
			foreach (BaseSpatialAudioSource item in List_0)
			{
				if (!(item == null))
				{
					method_0(item, item.GetComponent<AudioSource>());
				}
			}
		}

		public override void SetSpatialize(bool val)
		{
			foreach (BaseSpatialAudioSource item in List_0)
			{
				if (!(item == null))
				{
					item.EnableSpatialization = val;
				}
			}
		}

		public void method_0(BaseSpatialAudioSource spatialAudioSource, AudioSource audioSource)
		{
			audioSource.spatializePostEffects = false;
			audioSource.spatialBlend = base.Preset.SpatialBlend;
			spatialAudioSource.SetParameters(base.Preset);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class511
	{
		public static readonly Class511 class511_0 = new Class511();

		public static Action action_0;

		public static Action<Player> action_1;

		public void method_0()
		{
		}

		public void method_1(Player player)
		{
		}
	}

	[CompilerGenerated]
	public class Class512
	{
		public BetterSource source;

		public AudioClip clip;

		public float volume;

		public bool spatialize;

		public bool oneShot;

		public EOcclusionTest occlusionTest;

		public void method_0()
		{
			source.Play(clip, null, 1f, volume, !spatialize, oneShot);
			if (occlusionTest != EOcclusionTest.None && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(source, occlusionTest);
			}
		}
	}

	[CompilerGenerated]
	public class Class513
	{
		public BetterSource source;

		public AudioClip clip1;

		public AudioClip clip2;

		public float balance;

		public bool spatialize;

		public SoundBank bank;

		public EOcclusionTest occlusionTest;

		public float targetVolume;

		public void method_0()
		{
			source.Play(clip1, clip2, balance, targetVolume, !spatialize);
			if (!bank.IgnoreOcclusion && occlusionTest != EOcclusionTest.None && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(source, occlusionTest);
			}
		}
	}

	[CompilerGenerated]
	public class Class514
	{
		public BetterAudio betterAudio_0;

		public string mixerKey;

		public float method_0()
		{
			betterAudio_0.Master.GetFloat(mixerKey, out var value);
			return value;
		}

		public void method_1(float x)
		{
			betterAudio_0.Master.SetFloat(mixerKey, x);
		}
	}

	public static bool IsInHideout;

	[CompilerGenerated]
	private Action action_0 = delegate
	{
	};

	public const float FROM_OUT_TO_IN_TRANSITION_TIME = 0.1f;

	public const float FROM_IN_TO_OUT_TRANSITION_TIME = 0.5f;

	public const float SOUND_SPEED = 340f;

	public const float MIN_RELEASE_TIME = 1f;

	[CompilerGenerated]
	private AudioListener audioListener_0;

	public WeaponSounds MiscCollisionSounds;

	public ItemDropSounds ItemDropSounds;

	private SkillManager.SkillBuffClass skillBuffClass = new SkillManager.SkillBuffClass
	{
		Value = 0f
	};

	[CompilerGenerated]
	private Action action_1;

	[CompilerGenerated]
	private Action<Player> action_2 = delegate
	{
	};

	private SkillClass skillClass;

	private float float_0;

	private GameObject gameObject_0;

	private GameObject gameObject_1;

	private GameObject gameObject_2;

	private GameObject gameObject_3;

	private GameObject gameObject_4;

	private GameObject gameObject_5;

	private GameObject gameObject_6;

	private Coroutine coroutine_0;

	private bool bool_0;

	private Action action_3;

	private readonly GClass952 gclass952_0 = new GClass952();

	public IAudioSourceGroups[] SourceGroups;

	public AudioMixer Master;

	public AudioMixerSnapshot[] Snapshots;

	public AudioMixerGroup MasterMixerGroup;

	public AudioMixerGroup GunshotOccludedMixerGroup;

	public AudioMixerGroup SimpleOccludedMixerGroup;

	public AudioMixerGroup MutedGroup;

	public AudioMixerGroup UpperOccluded;

	public AudioMixerGroup LowerOccluded;

	public AudioMixerGroup GunshotMixerGroup;

	public AudioMixerGroup GunsInstrumentalMixer;

	public AudioMixerGroup VeryStandartMixerGroup;

	public AudioMixerGroup SelfSpeechReverb;

	public AudioMixerGroup OutEnvironment;

	public AudioMixerGroup VoipMixer;

	public AudioMixerGroup NonSpatialBypassMixer;

	public AudioMixerGroup VehicleInSpeechMixer;

	public AudioMixerGroup AmbientInMixer;

	public AudioMixerGroup AmbientOutMixer;

	public AudioMixerGroup CommonAmbientInEffectsMixer;

	public AudioMixerGroup CommonAmbientOutEffectsMixer;

	public AudioMixerGroup CommonAmbientEffectsMixer;

	public AudioMixerGroup ClientPlayerMovementMixer;

	public AudioMixerGroup ClientPlayerSpeechMixer;

	public AudioMixerGroup ObservedPlayerMovementMixer;

	public AudioMixerGroup ObservedPlayerSpeechMixer;

	public AudioMixerGroup EnvNatureSoundsGroup;

	public AudioMixerGroup EnvTechnicalSoundsGroup;

	public AudioMixerGroup VehicleInMixer;

	public AudioMixerGroup VehicleOutMixer;

	public AudioMixerGroup WorldMixer;

	private float float_1;

	private float float_2;

	private float float_3;

	public int OcclusionMask;

	public int OcclusionHighPolyMask;

	private CancellationTokenSource cancellationTokenSource_0;

	private readonly List<GClass888> list_0 = new List<GClass888>(15);

	private readonly GClass944<GClass890> gclass944_0 = new GClass944<GClass890>(15, 3);

	private static readonly Dictionary<AudioSourceGroupType, AudioGroupPreset> dictionary_0 = new Dictionary<AudioSourceGroupType, AudioGroupPreset>(GClass866<AudioSourceGroupType>.Count);

	private static Tweener tweener_0 = null;

	private Action action_4;

	private RaycastHit raycastHit_0;

	private GClass2587 gclass2587_0;

	private GClass1180 gclass1180_0 = new GClass1180();

	private GClass894 gclass894_0 = new GClass894();

	private GClass896 gclass896_0 = new GClass896();

	private MetaXRPluginErrorChecker metaXRPluginErrorChecker_0;

	private Action action_5;

	[CompilerGenerated]
	private GClass1174 gclass1174_0;

	[CompilerGenerated]
	private Player player_0;

	[CompilerGenerated]
	private Transform transform_0;

	public AudioListener AudioListener
	{
		[CompilerGenerated]
		get
		{
			return audioListener_0;
		}
		[CompilerGenerated]
		set
		{
			audioListener_0 = value;
		}
	}

	public float ProtagonistHearing => skillBuffClass;

	public GClass1174 AudioMixerData
	{
		[CompilerGenerated]
		get
		{
			return gclass1174_0;
		}
		[CompilerGenerated]
		set
		{
			gclass1174_0 = value;
		}
	}

	public Player ListenerPlayer
	{
		[CompilerGenerated]
		get
		{
			return player_0;
		}
		[CompilerGenerated]
		set
		{
			player_0 = value;
		}
	}

	public Transform ListenerTransform
	{
		[CompilerGenerated]
		get
		{
			return transform_0;
		}
		[CompilerGenerated]
		set
		{
			transform_0 = value;
		}
	}

	public HeadphonesTemplateClass CurrentHeadphonesTemplate => gclass2587_0.CurrentTemplate ?? new HeadphonesTemplateClass();

	public event Action AudioControllerInitialized
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

	public event Action ProtagonistHearingChanged
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

	public event Action<Player> ListenerSpawned
	{
		[CompilerGenerated]
		add
		{
			Action<Player> action = action_2;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Player> action = action_2;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void ApplyHeadphonesTemplate(HeadphonesTemplateClass template)
	{
		gclass2587_0.ApplyTemplate(template);
	}

	public void ApplyCurrentHeadphonesTemplate()
	{
		gclass2587_0.ApplyTemplate(CurrentHeadphonesTemplate);
	}

	public void Update()
	{
		gclass952_0.Update();
		for (int i = 0; i < list_0.Count; i++)
		{
			list_0[i].Update();
		}
	}

	public override void OnDestroy()
	{
		if (SourceGroups != null)
		{
			IAudioSourceGroups[] sourceGroups = SourceGroups;
			for (int i = 0; i < sourceGroups.Length; i++)
			{
				sourceGroups[i]?.SetSpatialize(val: false);
			}
		}
		Singleton<GClass1125>.Release(Singleton<GClass1125>.Instance);
		cancellationTokenSource_0?.Cancel();
		MiscCollisionSounds = null;
		ItemDropSounds = null;
		if (coroutine_0 != null)
		{
			StopCoroutine(coroutine_0);
			coroutine_0 = null;
		}
		action_3 = null;
		dictionary_0.Clear();
		gclass2587_0?.Dispose();
		action_5?.Invoke();
		action_5 = null;
		if (metaXRPluginErrorChecker_0 != null)
		{
			metaXRPluginErrorChecker_0.SetActive(checkEnabled: false);
		}
		base.OnDestroy();
	}

	public async void Preload()
	{
		await PreloadCoroutine();
	}

	public async Task PreloadCoroutine()
	{
		metaXRPluginErrorChecker_0 = GClass6.GetOrAddComponent<MetaXRPluginErrorChecker>(base.gameObject);
		metaXRPluginErrorChecker_0.SetActive(checkEnabled: false);
		if (GClass3670.CreateData<GClass1174>(EDataLifeTime.Raid))
		{
			AudioMixerData = GClass3670.GetData<GClass1174>();
		}
		cancellationTokenSource_0 = new CancellationTokenSource();
		Master = await method_0<AudioMixer>("Audio/MasterMixer", cancellationTokenSource_0);
		if (cancellationTokenSource_0.IsCancellationRequested || Master == null)
		{
			return;
		}
		Snapshots = (from x in Enum.GetNames(typeof(EnvironmentType))
			select Master.FindSnapshot(x)).ToArray();
		method_3();
		gameObject_0 = await method_0<GameObject>("Audio/AudioSourcePrefab", cancellationTokenSource_0);
		gameObject_1 = await method_0<GameObject>("Audio/AudioSourceBypassPrefab", cancellationTokenSource_0);
		gameObject_2 = await method_0<GameObject>("Audio/AudioSourceSpatialBuiltinEQPrefab", cancellationTokenSource_0);
		gameObject_4 = await method_0<GameObject>("Audio/AudioSourceSuperPrefab", cancellationTokenSource_0);
		gameObject_3 = await method_0<GameObject>("Audio/AudioSourceSpatialBuiltinEQReverbPrefab", cancellationTokenSource_0);
		gameObject_5 = await method_0<GameObject>("Audio/AudioSourceSuperSpatialBuiltinEQPrefab", cancellationTokenSource_0);
		gameObject_6 = await method_0<GameObject>("Audio/AudioSourceSuperSpatialBuiltinEQReverbPrefab", cancellationTokenSource_0);
		if (cancellationTokenSource_0.IsCancellationRequested)
		{
			return;
		}
		OcclusionMask = LayerMask.GetMask("LowPolyCollider", "DoorLowPolyCollider");
		OcclusionHighPolyMask = LayerMask.GetMask("HighPolyCollider", "TransparentCollider");
		ResetWorldMixerValues();
		ResetMasterValues();
		GClass722 instance = GClass722.Instance;
		try
		{
			method_4();
		}
		catch (OperationCanceledException)
		{
			instance.LogWarn("BetterAudio loading canceled");
			return;
		}
		catch (Exception arg)
		{
			instance.LogError($"Error loading Audio Mixers - {arg}");
			cancellationTokenSource_0.Cancel();
			return;
		}
		if (!cancellationTokenSource_0.IsCancellationRequested)
		{
			try
			{
				method_5();
			}
			catch (OperationCanceledException)
			{
				instance.LogWarn("BetterAudio loading canceled");
				return;
			}
			catch (Exception arg2)
			{
				instance.LogError($"Error AudioSourceGroup Presets - {arg2}");
				cancellationTokenSource_0.Cancel();
				return;
			}
			if (!cancellationTokenSource_0.IsCancellationRequested)
			{
				method_1();
				cancellationTokenSource_0.Dispose();
				cancellationTokenSource_0 = null;
				instance.LogInfo("Success initialize BetterAudio");
			}
		}
	}

	public async Task<T> method_0<T>(string path, CancellationTokenSource tokenSource) where T : UnityEngine.Object
	{
		GClass722 instance = GClass722.Instance;
		try
		{
			ResourceRequest resourceRequest = Resources.LoadAsync<T>(path);
			while (!resourceRequest.isDone)
			{
				if (tokenSource.Token.IsCancellationRequested)
				{
					tokenSource.Token.ThrowIfCancellationRequested();
				}
				await Task.Yield();
			}
			if (resourceRequest.asset == null)
			{
				instance.LogError($"Failed to load {typeof(T)} at path: {path}");
				tokenSource.Cancel();
				return null;
			}
			return (T)resourceRequest.asset;
		}
		catch (OperationCanceledException)
		{
			instance.LogWarn($"Loading canceled for {typeof(T)} at path: {path}");
			return null;
		}
		catch (Exception ex2)
		{
			instance.LogError($"Error loading {typeof(T)} at path: {path} - {ex2.Message}");
			tokenSource.Cancel();
			return null;
		}
	}

	public void ResetWorldMixerValues()
	{
		if (GClass3670.TryGetData<GClass1174>(out var dataContainer))
		{
			Master.SetFloat(dataContainer.WorldMixerVolume, 0f);
			Master.SetFloat(dataContainer.WorldMixerReverbLevel, -10000f);
			Master.SetFloat(dataContainer.WorldMixerLowpassFreq, (float)AudioSettings.outputSampleRate / 2f);
		}
	}

	public void ApplySpatialSettingsFromBackend(ClientAudioOcclusionSettings audioOcclusionSettings)
	{
		foreach (var (groupType, audioGroupPreset2) in dictionary_0)
		{
			if (audioOcclusionSettings.TryGetSettingsForGroup(groupType, out var jsonSettings))
			{
				audioGroupPreset2.occlusionSettings.ApplyFromJson(jsonSettings);
			}
		}
	}

	public void method_1()
	{
		GClass1096.GClass1107 headphonesSettings = Singleton<GClass1706>.Instance.AudioSettings.HeadphonesSettings;
		GClass3670.CreateData<GClass2596>(EDataLifeTime.Raid);
		HeadphonesTemplateClass defaultTemplate = GClass3670.GetData<GClass2596>().Default;
		GClass1179 mixerFader = new GClass1179(this, Master);
		gclass2587_0 = new GClass2587(defaultTemplate, new GClass2588(), Master, mixerFader, headphonesSettings);
		action_5 = (Action)Delegate.Combine(action_5, GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3566>(method_2));
	}

	public void method_2(GClass3566 initializedEvent)
	{
		ApplyCurrentHeadphonesTemplate();
		if (Singleton<BackendConfigSettingsClass>.Instantiated)
		{
			GClass1096.GClass1106 metaXRPluginSettings = Singleton<GClass1706>.Instance.AudioSettings.MetaXRPluginSettings;
			metaXRPluginErrorChecker_0.checkCooldown = metaXRPluginSettings.OutputVolumeCheckCooldown;
			metaXRPluginErrorChecker_0.SetActive(metaXRPluginSettings.EnabledPluginErrorChecker);
		}
	}

	public void method_3()
	{
		if (!Singleton<GClass1125>.Instantiated)
		{
			Singleton<GClass1125>.Create(new GClass1125());
		}
		action_0();
	}

	public void ResetMasterValues()
	{
		if (GClass3670.TryGetData<GClass1174>(out var dataContainer))
		{
			Master.SetFloat(dataContainer.GunsMixerVolume, 0f);
			Master.SetFloat(dataContainer.MainMixerVolume, (!IsInHideout) ? (-80) : 0);
			Master.SetFloat(dataContainer.GunsMixerTinnitusSendLevel, -80f);
			Master.SetFloat(dataContainer.MainMixerTinnitusSendLevel, -80f);
			Master.SetFloat(dataContainer.InGameVolumeMixer, -80f);
			Master.SetFloat(dataContainer.AmbientInMixerVolume, -80f);
			Master.SetFloat(dataContainer.AmbientOutMixerVolume, -80f);
		}
	}

	public void method_4()
	{
		MasterMixerGroup = FindMixerGroup("Master");
		GunshotOccludedMixerGroup = FindMixerGroup("Guns/Occluded");
		GunshotMixerGroup = FindMixerGroup("Guns/Gunshots");
		GunsInstrumentalMixer = FindMixerGroup("Guns/Instrumental");
		MutedGroup = FindMixerGroup("Main/Mute");
		VeryStandartMixerGroup = FindMixerGroup("Main/Environment/CommonSounds");
		SimpleOccludedMixerGroup = FindMixerGroup("Occlusion/SimpleOccluded");
		UpperOccluded = FindMixerGroup("UpperOccluded");
		LowerOccluded = FindMixerGroup("LowerOccluded");
		SelfSpeechReverb = FindMixerGroup("ClientPlayer/ClientPlayerSelfSpeechReverb");
		OutEnvironment = FindMixerGroup("OutEnvironment");
		VoipMixer = FindMixerGroup("Voip");
		NonSpatialBypassMixer = FindMixerGroup("NonspatialBypass");
		VehicleInSpeechMixer = FindMixerGroup("NPC/VehicleInSpeech");
		AmbientInMixer = FindMixerGroup("AmbientIn");
		AmbientOutMixer = FindMixerGroup("AmbientOut");
		CommonAmbientInEffectsMixer = FindMixerGroup("AmbientIn/CommonAmbInEffects");
		CommonAmbientOutEffectsMixer = FindMixerGroup("AmbientOut/CommonAmbOutEffects");
		CommonAmbientEffectsMixer = FindMixerGroup("Ambient/AmbCommonEffects");
		ClientPlayerSpeechMixer = FindMixerGroup("ClientPlayer/ClientPlayerSpeech");
		ClientPlayerMovementMixer = FindMixerGroup("ClientPlayer/ClientPlayerMovement");
		ObservedPlayerSpeechMixer = FindMixerGroup("ObservedPlayer/ObservedPlayerSpeech");
		ObservedPlayerMovementMixer = FindMixerGroup("ObservedPlayer/ObservedPlayerMovement");
		EnvNatureSoundsGroup = FindMixerGroup("Main/Environment/NatureSounds");
		EnvTechnicalSoundsGroup = FindMixerGroup("Main/Environment/TechnicalSounds");
		VehicleOutMixer = FindMixerGroup("Main/Environment/TechnicalSounds/Vehicles/VehicleOut");
		VehicleInMixer = FindMixerGroup("Main/Environment/TechnicalSounds/Vehicles/VehicleIn");
		WorldMixer = FindMixerGroup("World");
	}

	public void method_5()
	{
		AudioGroupPreset preset = smethod_0("Character");
		AudioGroupPreset preset2 = smethod_0("Collisions");
		AudioGroupPreset preset3 = smethod_0("Distant");
		AudioGroupPreset preset4 = smethod_0("Environment");
		AudioGroupPreset preset5 = smethod_0("Gunshots");
		AudioGroupPreset preset6 = smethod_0("Grenades");
		AudioGroupPreset preset7 = smethod_0("Impacts");
		AudioGroupPreset preset8 = smethod_0("Speech");
		AudioGroupPreset preset9 = smethod_0("Weaponary");
		AudioGroupPreset preset10 = smethod_0("NonspatialBypass");
		AudioGroupPreset preset11 = smethod_0("Nonspatial");
		AudioGroupPreset preset12 = smethod_0("Voip");
		AudioGroupPreset preset13 = smethod_0("Windows");
		AudioGroupPreset preset14 = smethod_0("Lamp");
		AudioGroupPreset preset15 = smethod_0("InteractiveObjects");
		AudioGroupPreset preset16 = smethod_0("PropagatedExclusive");
		AudioGroupPreset preset17 = smethod_0("LightOcclusion");
		SourceGroups = new IAudioSourceGroups[Enum.GetValues(typeof(AudioSourceGroupType)).Length];
		AudioMixerGroup veryStandartMixerGroup = VeryStandartMixerGroup;
		SourceGroups[0] = method_6<ReverbSuperSource>(GunshotMixerGroup, gameObject_6, preset5);
		SourceGroups[11] = method_6<SuperSourceDistant>(GunshotMixerGroup, gameObject_5, preset6);
		SourceGroups[1] = method_6<ReverbSimpleSource>(GunsInstrumentalMixer, gameObject_3, preset9);
		SourceGroups[2] = method_6<ReverbSimpleSource>(veryStandartMixerGroup, gameObject_3, preset7);
		SourceGroups[3] = method_6<ReverbSimpleSource>(veryStandartMixerGroup, gameObject_3, preset);
		SourceGroups[4] = method_6<ReverbSimpleSource>(veryStandartMixerGroup, gameObject_3, preset4);
		SourceGroups[5] = method_6<ReverbSimpleSource>(veryStandartMixerGroup, gameObject_3, preset2);
		SourceGroups[6] = method_6<ReverbSimpleSource>(veryStandartMixerGroup, gameObject_3, preset8);
		SourceGroups[7] = method_6<SuperSourceDistant>(veryStandartMixerGroup, gameObject_5, preset3);
		SourceGroups[8] = new GClass886<SimpleSource>(NonSpatialBypassMixer, gameObject_1, preset10);
		SourceGroups[9] = new GClass887<SimpleSource>(veryStandartMixerGroup, gameObject_1, preset11);
		SourceGroups[10] = method_6<SimpleSource>(VoipMixer, gameObject_3, preset12);
		SourceGroups[12] = method_6<ReverbSimpleSource>(veryStandartMixerGroup, gameObject_3, preset13);
		SourceGroups[13] = method_6<ReverbSimpleSource>(VehicleInSpeechMixer, gameObject_3, preset8);
		SourceGroups[14] = method_6<ReverbSimpleSource>(veryStandartMixerGroup, gameObject_3, preset14);
		SourceGroups[15] = method_6<ReverbSimpleSource>(veryStandartMixerGroup, gameObject_3, preset15);
		SourceGroups[16] = method_6<ReverbSimpleSource>(veryStandartMixerGroup, gameObject_3, preset4);
		SourceGroups[17] = method_6<ReverbSimpleSource>(veryStandartMixerGroup, gameObject_3, preset16);
		SourceGroups[18] = method_6<ReverbSimpleSource>(veryStandartMixerGroup, gameObject_3, preset17);
	}

	public static AudioGroupPreset smethod_0(string groupName)
	{
		AudioGroupPreset audioGroupPreset = GClass861.Load<AudioGroupPreset>("Audio/AudioGroupPresets/" + groupName);
		audioGroupPreset.DirectBinaural = true;
		try
		{
			GClass1096 audioSettings = Singleton<GClass1706>.Instance.AudioSettings;
			if (audioSettings.TryGetPreset((int)audioGroupPreset.Type, out var preset))
			{
				audioGroupPreset.OverallVolume = preset.OverallVolume;
				audioGroupPreset.DisableBinauralByDist = preset.DisabledBinauralByDistance;
				audioGroupPreset.HeightDiffToAllowBinaural = preset.HeightToAllowBinaural;
				audioGroupPreset.AngleToAllowBinaural = preset.AngleToAllowBinaural;
				audioGroupPreset.EnableBinauralDist = preset.DistanceToAllowBinaural;
			}
			if (audioSettings.MetaXRPluginSettings.TryGetSettingsForGroup(audioGroupPreset.Type, out var jsonSettings))
			{
				audioGroupPreset.acousticSettings.ApplyFromJson(jsonSettings);
			}
		}
		catch (Exception arg)
		{
			GClass722.Instance.LogError($"Failed to load settings from backend for : {groupName}, load default values? ex: {arg}");
		}
		dictionary_0[audioGroupPreset.Type] = audioGroupPreset;
		return audioGroupPreset;
	}

	public IAudioSourceGroups method_6<T>(AudioMixerGroup mixerGroup, GameObject sourcePrefab, AudioGroupPreset preset) where T : BetterSource
	{
		return new Class510<T>(mixerGroup, sourcePrefab, preset);
	}

	public AudioMixerGroup FindMixerGroup(string groupName)
	{
		return Master.FindMatchingGroups(groupName).First();
	}

	public void EnableAllSpatialization(bool val)
	{
		if (SourceGroups == null)
		{
			return;
		}
		IAudioSourceGroups[] sourceGroups = SourceGroups;
		foreach (IAudioSourceGroups audioSourceGroups in sourceGroups)
		{
			if (audioSourceGroups != null && !(audioSourceGroups.Preset == null))
			{
				audioSourceGroups.Preset.SteamSpatialize = val;
				audioSourceGroups.RefreshSourceSettings();
				continue;
			}
			break;
		}
	}

	public void EnableAllBinaural(bool val)
	{
		if (SourceGroups == null)
		{
			return;
		}
		IAudioSourceGroups[] sourceGroups = SourceGroups;
		foreach (IAudioSourceGroups audioSourceGroups in sourceGroups)
		{
			if (audioSourceGroups != null && !(audioSourceGroups.Preset == null))
			{
				if (!audioSourceGroups.Preset.DirectBinaural || val)
				{
					audioSourceGroups.Preset.DirectBinaural = val;
					audioSourceGroups.RefreshSourceSettings();
				}
				continue;
			}
			break;
		}
	}

	public void LoadSoundBundles()
	{
		MiscCollisionSounds = GClass1857.InstantiateAsset<WeaponSounds>(Singleton<IEasyAssets>.Instance, "assets/content/audio/prefabs/shells/weaponsounds.bundle");
		ItemDropSounds = GClass1857.InstantiateAsset<ItemDropSounds>(Singleton<IEasyAssets>.Instance, "assets/content/audio/prefabs/items/itemdropsounds.bundle");
	}

	public void SetProtagonist(Player player)
	{
		ListenerPlayer = player;
		skillBuffClass = player.Skills.PerceptionHearing;
		skillClass = player.Skills.Perception;
		action_4 = skillClass.SkillLevelChanged.Subscribe(InvokeProtagonistAction);
		action_2?.Invoke(player);
		ListenerTransform = Singleton<AudioListenerConsistencyManager>.Instance.transform;
	}

	public void UnsubscribeProtagonist()
	{
		if (skillClass != null)
		{
			action_4();
			skillClass = null;
			action_1 = null;
			ListenerPlayer = null;
		}
	}

	public void InvokeProtagonistAction()
	{
		action_1?.Invoke();
	}

	[CanBeNull]
	public GClass890 BorrowWeaponAudioQueue(AudioSourceGroupType groupType)
	{
		GClass887<ReverbSuperSource> gClass = SourceGroups[(int)groupType] as GClass887<ReverbSuperSource>;
		BetterSource betterSource = gClass?.BorrowSource();
		if (betterSource == null)
		{
			return null;
		}
		BetterSource betterSource2 = gClass.BorrowSource();
		if (betterSource2 == null)
		{
			betterSource.Release();
			return null;
		}
		GClass890 gClass2 = gclass944_0.GetObject();
		gClass2.Initialize(betterSource as ReverbSuperSource, betterSource2 as ReverbSuperSource);
		list_0.Add(gClass2);
		return gClass2;
	}

	public void ReleaseQueue(GClass888 queue)
	{
		BetterSource[] audioSources = queue.AudioSources;
		foreach (BetterSource betterSource in audioSources)
		{
			if (betterSource != null)
			{
				betterSource.Release();
			}
		}
		if (queue is GClass890)
		{
			gclass944_0.PutObject(queue as GClass890);
		}
		list_0.Remove(queue);
	}

	public void ReleaseQueueDelayed(GClass888 queue, double delay)
	{
		if (delay <= 0.0)
		{
			ReleaseQueue(queue);
		}
		else
		{
			StartCoroutine(method_7(queue, delay));
		}
	}

	public IEnumerator method_7(GClass888 queue, double delay)
	{
		yield return new WaitForSeconds((float)delay);
		if (queue != null)
		{
			ReleaseQueue(queue);
		}
	}

	public void TransitToEnvironment(EnvironmentType environment)
	{
		int num = ((environment != EnvironmentType.Outdoor) ? 1 : 0);
		float timeToReach = ((environment == EnvironmentType.Outdoor) ? 0.5f : 0.1f);
		Snapshots[num].TransitionTo(timeToReach);
	}

	public void TransitToEnvironment(EnvironmentType environment, float transitionTime)
	{
		Snapshots[(environment != EnvironmentType.Outdoor) ? 1 : 0].TransitionTo(transitionTime);
	}

	public void RegisterVolume(BetterPropagationVolume volume)
	{
		Singleton<GClass1125>.Instance?.RegisterVolume(volume);
	}

	public void RemoveVolume(BetterPropagationVolume volume)
	{
		Singleton<GClass1125>.Instance?.RemoveVolume(volume);
	}

	public void RegisterGroup(BetterPropagationGroups group)
	{
		Singleton<GClass1125>.Instance?.RegisterGroup(group);
	}

	[ContextMenu("погнали ёбанарот")]
	public void RegisterAllVolumes()
	{
		BetterPropagationVolume[] array = UnityEngine.Object.FindObjectsOfType<BetterPropagationVolume>();
		foreach (BetterPropagationVolume volume in array)
		{
			Singleton<GClass1125>.Instance?.RegisterVolume(volume);
		}
	}

	public List<BetterPropagationVolume> GetVolumesByPosition(Vector3 position)
	{
		return Singleton<GClass1125>.Instance?.GetVolumesByPosition(position);
	}

	public BetterPropagationVolume GetVolumeByPosition(Vector3 position)
	{
		return Singleton<GClass1125>.Instance?.GetVolumeByPosition(position);
	}

	public List<BetterPropagationVolume> GetAdjustedAndIsolatedVolumes(Vector3 position, List<BetterPropagationVolume> volumesBuffer)
	{
		return Singleton<GClass1125>.Instance?.GetAdjustedAndIsolatedVolumes(position, volumesBuffer);
	}

	public bool IsSourcesInDifferentVolume(Vector3 firstSourcePos, Vector3 secondSourcePos)
	{
		return Singleton<GClass1125>.Instance.IsPositionsInDifferentVolume(firstSourcePos, secondSourcePos);
	}

	public void StartTinnitusEffect(float time, AudioClip clip = null)
	{
		bool num = float_2 > Time.time;
		float_1 = Mathf.Max(15f, time * 2f);
		float_2 = float_1 + Time.time;
		if (!num)
		{
			StartCoroutine(method_8(clip));
		}
	}

	public void TogglePauseTinnitusEffect(bool pause)
	{
		bool_0 = pause;
		if (!pause && action_3 != null)
		{
			action_3();
			action_3 = null;
		}
	}

	public IEnumerator method_8(AudioClip clip)
	{
		BetterSource source = GetSource(AudioSourceGroupType.Speech);
		AudioMixerGroup outputAudioMixerGroup = source.source1.outputAudioMixerGroup;
		source.source1.outputAudioMixerGroup = MasterMixerGroup;
		bool enableSpatialization = source.EnableSpatialization;
		source.EnableSpatialization = false;
		source.source1.spatialBlend = 0f;
		source.source1.clip = clip;
		source.source1.loop = true;
		Master.GetFloat(AudioMixerData.MainMixerVolume, out var value);
		float tinnitusEffectSpeedMultiplier = EFTHardSettings.Instance.TinnitusEffectSpeedMultiplier;
		source.source1.Play();
		while (Time.time < float_2)
		{
			float num = 1f - (float_2 - Time.time) / float_1;
			float value2 = EFTHardSettings.Instance.TinnitusLowpas.Evaluate(tinnitusEffectSpeedMultiplier * num);
			float num2 = EFTHardSettings.Instance.MainChannelLevel.Evaluate(tinnitusEffectSpeedMultiplier * num);
			float value3 = CurrentHeadphonesTemplate.DryVolume + num2;
			float value4 = value + num2;
			source.source1.volume = EFTHardSettings.Instance.TinnitusSound.Evaluate(num);
			Master.SetFloat(AudioMixerData.GunsMixerVolume, value3);
			Master.SetFloat(AudioMixerData.GunsMixerTinnitusSendLevel, value2);
			Master.SetFloat(AudioMixerData.MainMixerTinnitusSendLevel, value2);
			Master.SetFloat(AudioMixerData.MainMixerVolume, value4);
			yield return null;
		}
		source.source1.loop = false;
		Master.SetFloat(AudioMixerData.GunsMixerVolume, CurrentHeadphonesTemplate.DryVolume);
		Master.SetFloat(AudioMixerData.MainMixerVolume, 0f);
		Master.SetFloat(AudioMixerData.GunsMixerTinnitusSendLevel, -80f);
		Master.SetFloat(AudioMixerData.MainMixerTinnitusSendLevel, -80f);
		source.source1.outputAudioMixerGroup = outputAudioMixerGroup;
		source.EnableSpatialization = enableSpatialization;
		source.source1.Stop();
		source.Release();
		yield return null;
	}

	public void PlayNonspatial(AudioClip clip, AudioSourceGroupType sourceGroup, float panStereo = 0f, float volume = 1f, AudioMixerGroup forcedGroup = null)
	{
		IAudioSourceGroups audioSourceGroups = SourceGroups[(int)sourceGroup];
		BetterSource betterSource = audioSourceGroups.BorrowSource();
		if (!(betterSource == null))
		{
			betterSource.Position = ListenerTransform.position;
			AudioMixerGroup mixerGroup = ((forcedGroup == null) ? audioSourceGroups.MixerGroup : forcedGroup);
			betterSource.EnableStereo(stereo: true);
			betterSource.SetMixerGroup(mixerGroup);
			betterSource.SetBaseVolume(volume);
			betterSource.SetRolloff(audioSourceGroups.Preset.DefaultMaxDistance);
			betterSource.source1.panStereo = panStereo;
			betterSource.Play(clip, null, 1f, volume, forceStereo: true);
			double dspTime = AudioSettings.dspTime;
			gclass952_0.Add(dspTime, dspTime + (double)clip.length, betterSource);
		}
	}

	public BetterSource PlayAtPoint(Vector3 position, AudioClip clip, float distance, AudioSourceGroupType sourceGroup, int rolloff, float volume = 1f, EOcclusionTest occlusionTest = EOcclusionTest.None, AudioMixerGroup forceMixerGroup = null, bool forceStereo = false)
	{
		if (distance > (float)rolloff)
		{
			return null;
		}
		IAudioSourceGroups audioSourceGroups = SourceGroups[(int)sourceGroup];
		BetterSource betterSource = audioSourceGroups.BorrowSource();
		if (betterSource == null)
		{
			return null;
		}
		betterSource.Position = position;
		betterSource.SetRolloff(rolloff);
		betterSource.SetPriority(Mathf.Clamp((int)distance * 2, 64, 128));
		betterSource.ResetOcclusion();
		AudioMixerGroup mixerGroup = ((forceMixerGroup != null) ? forceMixerGroup : audioSourceGroups.MixerGroup);
		betterSource.SetMixerGroup(mixerGroup);
		betterSource.Play(clip, null, 1f, volume, forceStereo);
		if (occlusionTest != EOcclusionTest.None && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
		{
			MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(betterSource, occlusionTest);
		}
		double dspTime = AudioSettings.dspTime;
		float num = Math.Max(clip.length, 1f);
		gclass952_0.Add(dspTime, dspTime + (double)num, betterSource);
		return betterSource;
	}

	public bool TryPlayAtPoint(out BetterSource source, Vector3 position, AudioClip clip, AudioSourceGroupType sourceGroup, int rolloff, float volume = 1f, EOcclusionTest occlusionTest = EOcclusionTest.None, AudioMixerGroup forceMixerGroup = null, bool spatialize = true, bool oneShot = true, bool autoReleaseSource = true, bool enabledHighPassFilter = true)
	{
		source = PlayAtPoint(position, clip, sourceGroup, rolloff, volume, occlusionTest, forceMixerGroup, spatialize, oneShot, autoReleaseSource, enabledHighPassFilter);
		return source != null;
	}

	public BetterSource PlayAtPoint(Vector3 position, AudioClip clip, AudioSourceGroupType sourceGroup, int rolloff, float volume = 1f, EOcclusionTest occlusionTest = EOcclusionTest.None, AudioMixerGroup forceMixerGroup = null, bool spatialize = true, bool oneShot = true, bool autoReleaseSource = true, bool enabledHighPassFilter = true)
	{
		float sqrDistanceToListener = GClass2313.GetSqrDistanceToListener(position);
		if (!GClass2313.IsInRange(sqrDistanceToListener, rolloff))
		{
			return null;
		}
		IAudioSourceGroups audioSourceGroups = SourceGroups[(int)sourceGroup];
		BetterSource betterSource = audioSourceGroups.BorrowSource();
		if (betterSource == null)
		{
			return null;
		}
		int priority = gclass1180_0.CalculatePriority(sqrDistanceToListener, rolloff);
		betterSource.SetPriority(priority);
		betterSource.Position = position;
		betterSource.SetRolloff(rolloff);
		betterSource.ResetOcclusion();
		AudioMixerGroup mixerGroup = ((forceMixerGroup != null) ? forceMixerGroup : audioSourceGroups.MixerGroup);
		betterSource.SetMixerGroup(mixerGroup);
		betterSource.EnabledHighPassFilter(enabledHighPassFilter);
		betterSource.Play(clip, null, 1f, volume, !spatialize, oneShot);
		if (occlusionTest != EOcclusionTest.None && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
		{
			MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(betterSource, occlusionTest);
		}
		if (autoReleaseSource)
		{
			float num = Math.Max(clip.length, 1f);
			double dspTime = AudioSettings.dspTime;
			gclass952_0.Add(dspTime, dspTime + (double)num, betterSource);
		}
		return betterSource;
	}

	public BetterSource PlayAtPointDelayed(Vector3 position, AudioClip clip, AudioSourceGroupType sourceGroup, int rolloff, float volume = 1f, float delay = 0f, EOcclusionTest occlusionTest = EOcclusionTest.None, AudioMixerGroup forceMixerGroup = null, bool spatialize = true, bool oneShot = true)
	{
		Class512 CS_0024_003C_003E8__locals25 = new Class512();
		CS_0024_003C_003E8__locals25.clip = clip;
		CS_0024_003C_003E8__locals25.volume = volume;
		CS_0024_003C_003E8__locals25.spatialize = spatialize;
		CS_0024_003C_003E8__locals25.oneShot = oneShot;
		CS_0024_003C_003E8__locals25.occlusionTest = occlusionTest;
		float sqrDistanceToListener = GClass2313.GetSqrDistanceToListener(position);
		if (!GClass2313.IsInRange(sqrDistanceToListener, rolloff))
		{
			return null;
		}
		IAudioSourceGroups audioSourceGroups = SourceGroups[(int)sourceGroup];
		CS_0024_003C_003E8__locals25.source = audioSourceGroups.BorrowSource();
		if (CS_0024_003C_003E8__locals25.source == null)
		{
			return null;
		}
		int priority = gclass1180_0.CalculatePriority(sqrDistanceToListener, rolloff);
		CS_0024_003C_003E8__locals25.source.SetPriority(priority);
		CS_0024_003C_003E8__locals25.source.Position = position;
		CS_0024_003C_003E8__locals25.source.SetRolloff(rolloff);
		CS_0024_003C_003E8__locals25.source.ResetOcclusion();
		AudioMixerGroup mixerGroup = ((forceMixerGroup != null) ? forceMixerGroup : audioSourceGroups.MixerGroup);
		CS_0024_003C_003E8__locals25.source.SetMixerGroup(mixerGroup);
		if (delay > 0f)
		{
			GClass855.WaitSeconds(CS_0024_003C_003E8__locals25.source, delay, delegate
			{
				CS_0024_003C_003E8__locals25.source.Play(CS_0024_003C_003E8__locals25.clip, null, 1f, CS_0024_003C_003E8__locals25.volume, !CS_0024_003C_003E8__locals25.spatialize, CS_0024_003C_003E8__locals25.oneShot);
				if (CS_0024_003C_003E8__locals25.occlusionTest != EOcclusionTest.None && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
				{
					MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(CS_0024_003C_003E8__locals25.source, CS_0024_003C_003E8__locals25.occlusionTest);
				}
			});
		}
		else
		{
			CS_0024_003C_003E8__locals25.method_0();
		}
		double dspTime = AudioSettings.dspTime;
		gclass952_0.Add(dspTime, dspTime + (double)CS_0024_003C_003E8__locals25.clip.length + (double)delay, CS_0024_003C_003E8__locals25.source);
		return CS_0024_003C_003E8__locals25.source;
	}

	public BetterSource PlayAtPointDelayed(Vector3 position, SoundBank bank, AudioSourceGroupType outputGroup, float distance, float delay, float volume = 1f, float bankBlendValue = -1f, EnvironmentType env = EnvironmentType.Outdoor, EOcclusionTest occlusionTest = EOcclusionTest.None, bool spatialize = false, AudioMixerGroup forceMixerGroup = null)
	{
		Class513 CS_0024_003C_003E8__locals39 = new Class513();
		CS_0024_003C_003E8__locals39.spatialize = spatialize;
		CS_0024_003C_003E8__locals39.bank = bank;
		CS_0024_003C_003E8__locals39.occlusionTest = occlusionTest;
		if (distance > CS_0024_003C_003E8__locals39.bank.Rolloff)
		{
			return null;
		}
		IAudioSourceGroups audioSourceGroups = SourceGroups[(int)outputGroup];
		CS_0024_003C_003E8__locals39.clip1 = null;
		CS_0024_003C_003E8__locals39.clip2 = null;
		CS_0024_003C_003E8__locals39.balance = 1f;
		distance = ((bankBlendValue < 0f) ? distance : bankBlendValue);
		float num = CS_0024_003C_003E8__locals39.bank.PickClips(distance, ref CS_0024_003C_003E8__locals39.clip1, ref CS_0024_003C_003E8__locals39.clip2, ref CS_0024_003C_003E8__locals39.balance, env);
		CS_0024_003C_003E8__locals39.source = null;
		if (num > 0f)
		{
			CS_0024_003C_003E8__locals39.source = audioSourceGroups.BorrowSource();
			if (CS_0024_003C_003E8__locals39.source == null)
			{
				return null;
			}
			int priority = gclass1180_0.CalculatePriority(distance, CS_0024_003C_003E8__locals39.bank.Rolloff);
			CS_0024_003C_003E8__locals39.source.Position = position;
			CS_0024_003C_003E8__locals39.source.SetRolloff(CS_0024_003C_003E8__locals39.bank.Rolloff);
			CS_0024_003C_003E8__locals39.source.SetPriority(priority);
			CS_0024_003C_003E8__locals39.source.ResetOcclusion();
			AudioMixerGroup mixerGroup = ((forceMixerGroup == null) ? audioSourceGroups.MixerGroup : forceMixerGroup);
			CS_0024_003C_003E8__locals39.source.SetMixerGroup(mixerGroup);
			CS_0024_003C_003E8__locals39.targetVolume = volume * CS_0024_003C_003E8__locals39.bank.RandomVolume;
			float randomPitch = CS_0024_003C_003E8__locals39.bank.RandomPitch;
			CS_0024_003C_003E8__locals39.source.SetPitch(randomPitch);
			num /= randomPitch;
			if (delay > 0f)
			{
				GClass855.WaitSeconds(CS_0024_003C_003E8__locals39.source, delay, delegate
				{
					CS_0024_003C_003E8__locals39.source.Play(CS_0024_003C_003E8__locals39.clip1, CS_0024_003C_003E8__locals39.clip2, CS_0024_003C_003E8__locals39.balance, CS_0024_003C_003E8__locals39.targetVolume, !CS_0024_003C_003E8__locals39.spatialize);
					if (!CS_0024_003C_003E8__locals39.bank.IgnoreOcclusion && CS_0024_003C_003E8__locals39.occlusionTest != EOcclusionTest.None && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
					{
						MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(CS_0024_003C_003E8__locals39.source, CS_0024_003C_003E8__locals39.occlusionTest);
					}
				});
			}
			else
			{
				CS_0024_003C_003E8__locals39.method_0();
			}
			double dspTime = AudioSettings.dspTime;
			gclass952_0.Add(dspTime, dspTime + (double)num + (double)delay, CS_0024_003C_003E8__locals39.source);
		}
		return CS_0024_003C_003E8__locals39.source;
	}

	public BetterSource PlayAtPoint(Vector3 position, SoundBank bank, int outputGroup, float distance, float volume = 1f, float bankBlendValue = -1f, EnvironmentType env = EnvironmentType.Outdoor, EOcclusionTest occlusionTest = EOcclusionTest.None, bool oneShot = true, bool needUpdate = true, bool forceStereo = false)
	{
		if (distance > bank.Rolloff)
		{
			return null;
		}
		IAudioSourceGroups audioSourceGroups = SourceGroups[outputGroup];
		AudioClip clip = null;
		AudioClip clip2 = null;
		float proportions = 1f;
		float num = bank.PickClips((bankBlendValue < 0f) ? distance : bankBlendValue, ref clip, ref clip2, ref proportions, env);
		BetterSource betterSource = null;
		if (num > 0f)
		{
			betterSource = audioSourceGroups.BorrowSource(activateSource: true, needUpdate);
			if (betterSource == null)
			{
				return null;
			}
			betterSource.Position = position;
			betterSource.SetRolloff(bank.Rolloff);
			int priority = gclass1180_0.CalculatePriority(distance, bank.Rolloff);
			betterSource.SetPriority(priority);
			betterSource.ResetOcclusion();
			betterSource.SetMixerGroup(audioSourceGroups.MixerGroup);
			float volume2 = volume * bank.RandomVolume;
			float randomPitch = bank.RandomPitch;
			betterSource.SetPitch(randomPitch);
			num /= randomPitch;
			BetterSource betterSource2 = betterSource;
			AudioClip clip3 = clip;
			AudioClip clip4 = clip2;
			float balance = proportions;
			bool oneShot2 = oneShot;
			betterSource2.Play(clip3, clip4, balance, volume2, forceStereo, oneShot2);
			if (!bank.IgnoreOcclusion && occlusionTest != EOcclusionTest.None && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
			{
				MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(betterSource, occlusionTest);
			}
			double dspTime = AudioSettings.dspTime;
			float num2 = Math.Max(num, 1f);
			gclass952_0.Add(dspTime, dspTime + (double)num2, betterSource);
		}
		return betterSource;
	}

	public BetterSource PlayAtPoint(Vector3 position, SoundBank bank, float distance, float volume = 1f, float bankBlendValue = -1f, EnvironmentType env = EnvironmentType.Outdoor, EOcclusionTest occlusionTest = EOcclusionTest.None, bool oneShot = true, bool needUpdate = false)
	{
		return PlayAtPoint(position, bank, (int)bank.SourceType, distance, volume, bankBlendValue, env, occlusionTest, oneShot, needUpdate);
	}

	public BetterSource PlayAtPointDistant(Vector3 position, SoundBank bank, float distance, float volume = 1f, float spatialBlend = 1f, EnvironmentType env = EnvironmentType.Outdoor, EOcclusionTest occlusionTest = EOcclusionTest.None, AudioMixerGroup forceGroup = null, bool needUpdate = true)
	{
		if (distance > bank.Rolloff)
		{
			return null;
		}
		AudioSourceGroupType audioSourceGroupType = ((bank.SourceType == AudioSourceGroupType.Distant || bank.SourceType == AudioSourceGroupType.Grenades) ? bank.SourceType : AudioSourceGroupType.Distant);
		IAudioSourceGroups audioSourceGroups = SourceGroups[(int)audioSourceGroupType];
		AudioClip clip = null;
		AudioClip clip2 = null;
		float proportions = 1f;
		float num = bank.PickClips(distance, ref clip, ref clip2, ref proportions, env);
		if (num <= 0f)
		{
			return null;
		}
		SuperSourceDistant superSourceDistant = (SuperSourceDistant)audioSourceGroups.BorrowSource(activateSource: true, needUpdate);
		if (superSourceDistant == null)
		{
			return null;
		}
		superSourceDistant.ResetOcclusion();
		AudioMixerGroup mixerGroup = ((forceGroup != null) ? forceGroup : SourceGroups[(int)bank.SourceType].MixerGroup);
		if (volume <= 0f)
		{
			superSourceDistant.Release();
			return null;
		}
		superSourceDistant.SetMixerGroup(mixerGroup);
		superSourceDistant.SetRolloff(bank.Rolloff);
		superSourceDistant.Position = position;
		float num2 = (superSourceDistant.Delay = distance / 340f);
		superSourceDistant.SpatialBlend = spatialBlend;
		float volume2 = volume * bank.RandomVolume;
		float randomPitch = bank.RandomPitch;
		num /= randomPitch;
		superSourceDistant.SetPitch(randomPitch);
		superSourceDistant.Play(clip, clip2, proportions, volume2);
		if (occlusionTest != EOcclusionTest.None && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
		{
			MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(superSourceDistant, occlusionTest);
		}
		double dspTime = AudioSettings.dspTime;
		float num3 = Math.Max(num, 1f);
		gclass952_0.Add(dspTime, dspTime + (double)num3 + (double)num2, superSourceDistant);
		return superSourceDistant;
	}

	public bool LimitedPlay(out BetterSource source, Vector3 position, SoundBank bank, float distance, Vector2 gagRadius, float chokeTime, float volume = 1f, float bankBlendValue = -1f, EnvironmentType env = EnvironmentType.Outdoor, EOcclusionTest occlusionTest = EOcclusionTest.None, string key = "", bool needUpdate = true, bool oneShot = true)
	{
		source = null;
		if (distance > bank.Rolloff)
		{
			return false;
		}
		key = (GClass856.IsNullOrEmpty(key) ? bank.name : key);
		if (!gclass896_0.CheckGagsLimit(position, ListenerTransform.position, distance * distance, key))
		{
			return false;
		}
		gclass896_0.InitGag(position, bank.ClipLength, bank.Rolloff, chokeTime, gagRadius, key);
		bool needUpdate2 = needUpdate;
		source = PlayAtPoint(position, bank, distance, volume, bankBlendValue, env, occlusionTest, oneShot, needUpdate2);
		return source != null;
	}

	public void LimitedPlayNonSpatial(AudioClip clip, Vector2 gagRadius, float chokeTime, AudioSourceGroupType sourceGroup, float volume = 1f, float panStereo = 0f, string key = "")
	{
		Vector3 position = ListenerTransform.position;
		if (gclass896_0.CheckGagsLimit(position, position, 0f, key))
		{
			gclass896_0.InitGag(position, clip.length, 0f, chokeTime, gagRadius, key);
			PlayNonspatial(clip, sourceGroup, panStereo, volume);
		}
	}

	public void PlayDropItem(BaseBallistic.ESurfaceSound surfaceSound, EItemDropSoundType dropSoundType, Vector3 position, float energy)
	{
		SoundBank soundBank = ItemDropSounds.GetSoundBank(surfaceSound, dropSoundType);
		if (soundBank != null)
		{
			float volume = (float)Math.Round(ItemDropSounds.EnergyToVolumeCurve.Evaluate(energy), 2);
			float distance = CameraClass.Instance.Distance(position);
			PlayAtPoint(position, soundBank, distance, volume, -1f, EnvironmentType.Outdoor, EOcclusionTest.OneShotPropagation);
		}
	}

	public void PlayDropItem(SoundBank bank, Vector3 position, float energy)
	{
		if (bank != null)
		{
			float volume = (float)Math.Round(ItemDropSounds.EnergyToVolumeCurve.Evaluate(energy), 2);
			float distance = CameraClass.Instance.Distance(position);
			PlayAtPoint(position, bank, distance, volume, -1f, EnvironmentType.Outdoor, EOcclusionTest.OneShotPropagation);
		}
	}

	public void PrecacheGag(string chokeKey)
	{
		gclass896_0.PrecacheGag(chokeKey);
	}

	public bool TryGetGroupPreset(AudioSourceGroupType groupType, out AudioGroupPreset preset)
	{
		return dictionary_0.TryGetValue(groupType, out preset);
	}

	[CanBeNull]
	public T CreateBetterSource<T>(AudioSource parentSource, AudioSourceGroupType groupType, bool enabledLowpass = true, bool enabledHighpass = true) where T : BetterSource
	{
		if (!TryGetGroupPreset(groupType, out var preset))
		{
			Debug.LogError($"Failed to initialize source with type : {groupType}, preset not found");
			return null;
		}
		GClass6.GetOrAddComponent<AudioLowPassFilter>(parentSource.gameObject).enabled = enabledLowpass;
		GClass6.GetOrAddComponent<AudioHighPassFilter>(parentSource.gameObject).enabled = enabledHighpass;
		parentSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, preset.SoundRolloff);
		parentSource.SetCustomCurve(AudioSourceCurveType.Spread, preset.SpreadCurve);
		GClass6.GetOrAddComponent<AudioBuiltinEQFilter>(parentSource.gameObject);
		T orAddComponent = GClass6.GetOrAddComponent<T>(parentSource.gameObject);
		orAddComponent.Init();
		orAddComponent.SetPreset(preset);
		orAddComponent.SetBaseVolume(1f);
		orAddComponent.SetMixerGroup(SourceGroups[(int)groupType].MixerGroup);
		orAddComponent.ReleaseListener = gclass894_0;
		return orAddComponent;
	}

	[CanBeNull]
	public T CreateBetterSourceWithParentSettings<T>(AudioSource parentSource, AudioSourceGroupType groupType, bool enabledLowpass = true, bool enabledHighpass = true) where T : BetterSource
	{
		bool spatialize = parentSource.spatialize;
		float maxDistance = parentSource.maxDistance;
		AnimationCurve customCurve = parentSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
		AnimationCurve customCurve2 = parentSource.GetCustomCurve(AudioSourceCurveType.Spread);
		float volume = parentSource.volume;
		T val = CreateBetterSource<T>(parentSource, groupType, enabledLowpass, enabledHighpass);
		parentSource.rolloffMode = AudioRolloffMode.Custom;
		parentSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customCurve);
		parentSource.SetCustomCurve(AudioSourceCurveType.Spread, customCurve2);
		val.SetMixerGroup(parentSource.outputAudioMixerGroup);
		val.SetRolloff(maxDistance);
		val.SetBaseVolume(volume);
		val.EnableSpatialization = spatialize;
		return val;
	}

	public BetterSource GetSource(SoundBank bank, bool activateSource = true)
	{
		return GetSource(bank.SourceType, activateSource);
	}

	public void AddToAudioSourceQueue(GClass952.GInterface48 queueItem, double endTime)
	{
		gclass952_0.Add(AudioSettings.dspTime, endTime, queueItem);
	}

	public void RemoveFromAudioSourceQueue(GClass952.GInterface48 queueItem)
	{
		gclass952_0.Remove(queueItem);
	}

	[CanBeNull]
	public BetterSource GetSource(AudioSourceGroupType sourceType, bool activateSource = true, bool needUpdate = true)
	{
		return SourceGroups[(int)sourceType].BorrowSource(activateSource, needUpdate);
	}

	public void FadeMixerVolume(string mixerKey, float endValDb, float seconds, bool force = false)
	{
		if (!Master.GetFloat(mixerKey, out var _))
		{
			Debug.LogError(mixerKey + " is not found");
			return;
		}
		if (force)
		{
			tweener_0?.Kill();
		}
		else
		{
			tweener_0?.WaitForKill();
		}
		tweener_0 = DOTween.To(delegate
		{
			Master.GetFloat(mixerKey, out var value2);
			return value2;
		}, delegate(float x)
		{
			Master.SetFloat(mixerKey, x);
		}, endValDb, seconds);
	}

	public void FadeInVolumeBeforeRaid(float seconds)
	{
		FadeMixerVolume(AudioMixerData.MainMixerVolume, 0f, seconds);
		FadeMixerVolume(AudioMixerData.InGameVolumeMixer, 0f, seconds);
		FadeMixerVolume(AudioMixerData.MusicVolumeMixer, -80f, seconds);
	}

	public void FadeOutVolumeAfterRaid()
	{
		int musicVolumeValue = Singleton<SharedGameSettingsClass>.Instance.Sound.Settings.MusicVolumeValue;
		FadeMixerVolume(AudioMixerData.MainMixerVolume, -80f, 0f);
		FadeMixerVolume(AudioMixerData.InGameVolumeMixer, -80f, 0f);
		FadeMixerVolume(AudioMixerData.MusicVolumeMixer, musicVolumeValue, 0f);
	}

	public void ToggleNarrate(bool inScene, float seconds = 1f)
	{
		int musicVolumeValue = Singleton<SharedGameSettingsClass>.Instance.Sound.Settings.MusicVolumeValue;
		FadeMixerVolume(AudioMixerData.MainMixerVolume, inScene ? 0f : (-80f), seconds);
		FadeMixerVolume(AudioMixerData.InGameVolumeMixer, inScene ? 0f : (-80f), seconds);
		FadeMixerVolume(AudioMixerData.MusicVolumeMixer, inScene ? (-80f) : ((float)musicVolumeValue), seconds);
	}

	public void ForceSetCommonVolume(float valDb)
	{
		tweener_0?.Kill();
		Master.SetFloat(AudioMixerData.MainMixerVolume, valDb);
	}

	[CompilerGenerated]
	public AudioMixerSnapshot method_9(string x)
	{
		return Master.FindSnapshot(x);
	}
}

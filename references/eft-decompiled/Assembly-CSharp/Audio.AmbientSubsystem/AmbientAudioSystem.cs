using System;
using System.Runtime.CompilerServices;
using Audio.AmbientSubsystem.AmbientSplineEmitter;
using Audio.AmbientSubsystem.Data;
using Audio.AmbientSubsystem.GameEvents;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class AmbientAudioSystem : MonoBehaviourSingleton<AmbientAudioSystem>, IDisposable
{
	[SerializeField]
	private SeasonAmbientSoundDataSO _ambientData;

	[SerializeField]
	private MixerEffectsDataSO EffectsData;

	[SerializeField]
	private int _frameCountUpdateInterval = 5;

	[SerializeField]
	private bool _useSimpleUnderRoofCheck;

	private AudioGameEventsController audioGameEventsController_0;

	private GInterface101[] ginterface101_0 = Array.Empty<GInterface101>();

	private AmbientSoundPlayerGroup[] ambientSoundPlayerGroup_0 = Array.Empty<AmbientSoundPlayerGroup>();

	private AmbientSplineEmitterController[] ambientSplineEmitterController_0 = Array.Empty<AmbientSplineEmitterController>();

	private GInterface98[] ginterface98_0 = Array.Empty<GInterface98>();

	private GInterface97[] ginterface97_0 = Array.Empty<GInterface97>();

	private GInterface99[] ginterface99_0 = Array.Empty<GInterface99>();

	private GClass1185 gclass1185_0;

	private int int_0;

	private bool bool_0;

	private AudioSourceCulling audioSourceCulling_0;

	private GClass1184 gclass1184_0;

	private Action action_0;

	[CompilerGenerated]
	private ESeasonStatus eseasonStatus_0;

	[CompilerGenerated]
	private SeasonAmbientSoundDataSO seasonAmbientSoundDataSO_0;

	[CompilerGenerated]
	private bool bool_1;

	public ESeasonStatus CurrentSeasonStatus
	{
		[CompilerGenerated]
		get
		{
			return eseasonStatus_0;
		}
		[CompilerGenerated]
		set
		{
			eseasonStatus_0 = value;
		}
	}

	public SeasonAmbientSoundDataSO AmbientSoundData
	{
		[CompilerGenerated]
		get
		{
			return seasonAmbientSoundDataSO_0;
		}
		[CompilerGenerated]
		set
		{
			seasonAmbientSoundDataSO_0 = value;
		}
	}

	public bool UseSimpleUnderRoofCheck => _useSimpleUnderRoofCheck;

	public bool Initialized
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

	public override void Awake()
	{
		base.Awake();
		method_0();
		method_1();
		method_3();
		audioGameEventsController_0 = GClass6.GetOrAddComponent<AudioGameEventsController>(base.gameObject);
		ginterface101_0 = GetComponentsInChildren<GInterface101>();
		ambientSoundPlayerGroup_0 = GetComponentsInChildren<AmbientSoundPlayerGroup>();
		ambientSplineEmitterController_0 = GetComponentsInChildren<AmbientSplineEmitterController>();
		ginterface98_0 = GetComponentsInChildren<GInterface98>();
		ginterface97_0 = GetComponentsInChildren<GInterface97>();
		ginterface99_0 = GetComponentsInChildren<GInterface99>();
	}

	public void Initialize()
	{
		method_7();
	}

	public void method_0()
	{
		AmbientSoundData = ((_ambientData == null) ? ScriptableObject.CreateInstance<SeasonAmbientSoundDataSO>() : _ambientData);
	}

	public void method_1()
	{
		audioSourceCulling_0 = GClass6.GetOrAddComponent<AudioSourceCulling>(base.gameObject);
	}

	public void method_2()
	{
		AudioListenerConsistencyManager instance = Singleton<AudioListenerConsistencyManager>.Instance;
		if (instance != null)
		{
			audioSourceCulling_0.SetListenerTransform(instance.transform);
		}
		else
		{
			GClass722.Instance.LogError("Listener is null!");
		}
	}

	public void method_3()
	{
		GlobalEventHandlerClass instance = GlobalEventHandlerClass.Instance;
		action_0 = (Action)Delegate.Combine(action_0, instance.SubscribeOnEvent<GClass3543>(method_4));
		action_0 = (Action)Delegate.Combine(action_0, instance.SubscribeOnEvent<GClass3569>(method_5));
	}

	public void method_4(GClass3543 rainIntensityChangedEvent)
	{
		GInterface97[] array = ginterface97_0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ApplyPrecipitationsIntensity(rainIntensityChangedEvent.RainIntensity);
		}
	}

	public void method_5(GClass3569 windSpeedChangedEvent)
	{
		GInterface99[] array = ginterface99_0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ApplyWindSpeed(windSpeedChangedEvent.WindSpeed);
		}
	}

	public void method_6()
	{
		if (ambientSplineEmitterController_0.Length == 0)
		{
			GClass722.Instance.LogInfo("AmbientSplineEmitterControllers not found on location");
			return;
		}
		try
		{
			Transform transform = Singleton<AudioListenerConsistencyManager>.Instance.transform;
			if (transform != null)
			{
				AmbientSplineEmitterController[] array = ambientSplineEmitterController_0;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Init(transform);
				}
			}
		}
		catch (Exception arg)
		{
			GClass722.Instance.LogError($"Can't handle AudioListener transform for init spline emitter components! : {arg}");
		}
	}

	public void method_7()
	{
		Class443.OnInitialized -= method_8;
		if (Class443.Controller != null)
		{
			method_8(Class443.Controller);
			return;
		}
		GClass722.Instance.LogWarn("Ambient system initialization failed, try to subscribe");
		Class443.OnInitialized += method_8;
	}

	public void method_8(GInterface29 seasonsController)
	{
		CurrentSeasonStatus = seasonsController.Status;
		Class443.OnInitialized -= method_8;
		GlobalEventHandlerClass.CreateEvent<SeasonChangedEventClass>().Invoke(CurrentSeasonStatus);
		method_9();
	}

	public void method_9()
	{
		try
		{
			method_2();
			gclass1185_0 = new GClass1185();
			gclass1184_0 = new GClass1184(EffectsData.effectsSendsMixerData, EffectsData.delayMixerData, EffectsData.reverbMixerData, EffectsData.effectsVolumesMixerData);
			GInterface101[] array = ginterface101_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Init();
			}
			GInterface98[] array2 = ginterface98_0;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].SetSeasonStatus(CurrentSeasonStatus);
			}
			method_6();
			method_11();
			AmbientSoundPlayerGroup[] array3 = ambientSoundPlayerGroup_0;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].Play();
			}
			GlobalEventHandlerClass.CreateEvent<GClass3566>().Invoke();
			GClass722.Instance.LogInfo("Ambient Audio System successful init");
			Initialized = true;
		}
		catch (Exception arg)
		{
			GClass722.Instance.LogError($"Ambient Audio System init failed, ambient players don't playing! {arg}");
		}
	}

	public void RegisterInAudioCulling(AudioSource source, bool restartSource = false)
	{
		audioSourceCulling_0.Register(source, restartSource);
	}

	public void RemoveFromAudioCulling(AudioSource source)
	{
		if (!(audioSourceCulling_0 == null))
		{
			audioSourceCulling_0.Remove(source);
		}
	}

	public void Update()
	{
		if (!Initialized)
		{
			return;
		}
		if (method_10())
		{
			GInterface101[] array = ginterface101_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ManualUpdate(Time.deltaTime);
			}
		}
		AmbientSplineEmitterController[] array2 = ambientSplineEmitterController_0;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Tick();
		}
	}

	public void LateUpdate()
	{
		if (Initialized)
		{
			AmbientSplineEmitterController[] array = ambientSplineEmitterController_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].LateTick();
			}
		}
	}

	public bool method_10()
	{
		if (bool_0)
		{
			return false;
		}
		if (int_0 < _frameCountUpdateInterval)
		{
			int_0++;
			return false;
		}
		int_0 = 0;
		return true;
	}

	public void method_11()
	{
		BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;
		if (instance != null)
		{
			bool valueOrDefault = Singleton<BackendConfigSettingsClass>.Instance.SeasonActivityConfig?.InfectionHalloweenConfig?.Enabled == true;
			audioGameEventsController_0.Init();
			if (valueOrDefault)
			{
				audioGameEventsController_0.RunEvent(EEventType.Halloween);
			}
			else if (instance.runddansSettings.active)
			{
				audioGameEventsController_0.RunEvent(EEventType.Christmas);
			}
		}
	}

	public override void OnDestroy()
	{
		Dispose();
		base.OnDestroy();
	}

	public void Dispose()
	{
		if (!bool_0)
		{
			action_0?.Invoke();
			action_0 = null;
			if (audioSourceCulling_0 != null)
			{
				audioSourceCulling_0.StopWork();
				audioSourceCulling_0.CleanRegisteredAudioSources();
				audioSourceCulling_0 = null;
			}
			Class443.OnInitialized -= method_8;
			GInterface101[] array = ginterface101_0;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.Dispose();
			}
			AmbientSoundPlayerGroup[] array2 = ambientSoundPlayerGroup_0;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i]?.Stop();
			}
			gclass1185_0?.Dispose();
			if (audioGameEventsController_0 != null)
			{
				audioGameEventsController_0.StopCurrentEvent();
			}
			gclass1184_0?.Dispose();
			bool_0 = true;
			Initialized = false;
		}
	}
}

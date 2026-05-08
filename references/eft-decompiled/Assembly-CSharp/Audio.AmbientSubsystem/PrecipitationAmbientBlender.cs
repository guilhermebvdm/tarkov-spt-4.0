using System;
using Audio.AmbientSubsystem.Data;
using Audio.SpatialSystem;
using Comfort.Common;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.AmbientSubsystem;

public class PrecipitationAmbientBlender : MonoBehaviour, GInterface101, IDisposable, GInterface98
{
	private const float float_0 = 1f;

	[Tooltip("For Calculation by Room PrecipitationVolumeRange")]
	[SerializeField]
	private float _dynamicCrossfadeDuration = 1f;

	[SerializeField]
	private float _dynamicBlendSpeed = 0.33f;

	[Space(10f)]
	[SerializeField]
	private AudioSource _precipitationSource;

	[SerializeField]
	private AudioSource _precipitationMixSource;

	[SerializeField]
	private float _defaultBlendSpeed = 0.33f;

	[SerializeField]
	private float _crossfadeDuration = 1f;

	[SerializeField]
	private EnvironmentType _environment;

	[SerializeField]
	private AudioMixerGroup _outputMixerGroup;

	[SerializeField]
	private float _maxVolumeCheckDelta = 0.05f;

	private SeasonAmbientSoundDataSO seasonAmbientSoundDataSO_0;

	private GInterface100 ginterface100_0;

	private RainController.ERainIntensity erainIntensity_0;

	private EAudioRoomTypeMask eaudioRoomTypeMask_0 = EAudioRoomTypeMask.Outdoor;

	private float float_1 = 1f;

	private float float_2 = 1f;

	private float float_3 = 1f;

	private GClass1188 gclass1188_0;

	private Action action_0;

	private bool bool_0;

	private RoomAmbientData roomAmbientData_0 = new RoomAmbientData();

	private GClass1096.GClass1103 gclass1103_0 = new GClass1096.GClass1103();

	private float float_4 = 1f;

	private ESeasonStatus eseasonStatus_0;

	private int int_0 = -1;

	public float Single_0
	{
		get
		{
			if (!roomAmbientData_0.UseVolumeRange)
			{
				return float_1;
			}
			return float_2;
		}
	}

	public RainController.ERainIntensity ERainIntensity_0 => RainController.IntensityType;

	public void Init()
	{
		seasonAmbientSoundDataSO_0 = MonoBehaviourSingleton<AmbientAudioSystem>.Instance.AmbientSoundData;
		action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3573>(method_1);
		_precipitationSource.outputAudioMixerGroup = _outputMixerGroup;
		_precipitationMixSource.outputAudioMixerGroup = _outputMixerGroup;
		gclass1188_0 = new GClass1188();
		ginterface100_0 = new GClass1186(_precipitationSource, _precipitationMixSource);
		bool_0 = true;
	}

	public void ManualUpdate(float dt = 0f)
	{
		if (roomAmbientData_0.UseVolumeRange && _environment == EnvironmentType.Indoor)
		{
			method_0();
			if (!ginterface100_0.IsCrossfadeStarted && Mathf.Abs(float_3 - Single_0) > _maxVolumeCheckDelta)
			{
				method_2(_dynamicCrossfadeDuration, _dynamicBlendSpeed);
				float_3 = Single_0;
			}
		}
		if (bool_0 && ERainIntensity_0 != erainIntensity_0)
		{
			method_2(_crossfadeDuration, _defaultBlendSpeed);
			erainIntensity_0 = ERainIntensity_0;
		}
	}

	public void SetSeasonStatus(ESeasonStatus targetSeasonStatus)
	{
		try
		{
			gclass1103_0 = Singleton<GClass1706>.Instance.AudioSettings.EnvironmentSettings.GetSeasonSettings(targetSeasonStatus);
		}
		catch (Exception arg)
		{
			GClass722.Instance.LogError($"Can't get ClientSettingsConfig SeasonAmbientSettings, using default values: {arg}");
		}
		method_6();
		eseasonStatus_0 = targetSeasonStatus;
	}

	public void method_0()
	{
		float_2 = gclass1188_0.CalculateNormalizedVolumeByRange();
	}

	public void method_1(GClass3573 roomChangedEvent)
	{
		ISpatialAudioRoom room = roomChangedEvent.Room;
		eaudioRoomTypeMask_0 = room?.Type ?? EAudioRoomTypeMask.Outdoor;
		roomAmbientData_0 = ((room == null) ? roomAmbientData_0 : room.RoomAmbientData);
		EnvironmentType environmentType = method_3();
		float_1 = ((_environment == EnvironmentType.Indoor) ? room.RoomAmbientData.PrecipitationVolume : 1f);
		gclass1188_0.UpdateRoom(room);
		if (_environment == environmentType && !Mathf.Approximately(float_3, Single_0))
		{
			if (!roomAmbientData_0.UseVolumeRange)
			{
				method_2(_crossfadeDuration);
			}
			float_3 = float_1;
		}
	}

	public void method_2(float duration = 1f, float speed = 1f)
	{
		method_6();
		method_4();
		ginterface100_0.Stop();
		ginterface100_0.StartCrossfade(duration, speed, ginterface100_0.CurrentSource.volume, Single_0 * float_4);
	}

	public EnvironmentType method_3()
	{
		if (!GClass1118.Contains(eaudioRoomTypeMask_0, EAudioRoomTypeMask.Outdoor))
		{
			return EnvironmentType.Indoor;
		}
		return EnvironmentType.Outdoor;
	}

	public void method_4()
	{
		AudioSource mixSource = ginterface100_0.MixSource;
		AudioClip clip;
		if (ERainIntensity_0 == RainController.ERainIntensity.None)
		{
			mixSource.clip = null;
			int_0 = -1;
		}
		else if (method_5(out clip))
		{
			mixSource.clip = clip;
			int_0 = clip.GetHashCode();
		}
	}

	public bool method_5(out AudioClip clip)
	{
		int precipitationClip = roomAmbientData_0.GetPrecipitationClip(eseasonStatus_0, ERainIntensity_0, out clip);
		if (precipitationClip == -1)
		{
			return seasonAmbientSoundDataSO_0.TryGetPrecipitationClip(eseasonStatus_0, ERainIntensity_0, _environment, out clip);
		}
		return precipitationClip != int_0;
	}

	public void method_6()
	{
		float_4 = gclass1103_0.GetRainMult(_environment, ERainIntensity_0);
	}

	public void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		action_0?.Invoke();
		action_0 = null;
		ginterface100_0?.Stop();
		gclass1188_0?.Dispose();
	}
}

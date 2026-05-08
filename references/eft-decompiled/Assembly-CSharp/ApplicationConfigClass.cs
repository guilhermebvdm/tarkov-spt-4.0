using System;
using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Profiling;

public class ApplicationConfigClass
{
	[Serializable]
	public class PoolsSettings
	{
		public int AmmoPoolSize = 60;

		public int WeaponsPoolSize = 1;

		public int MagsPoolSize = 1;

		public int ItemsPoolSize = 1;

		public int PlayersPoolSize = 30;

		public Dictionary<ResourceType, int> AutoMaintainMinPoolSize = new Dictionary<ResourceType, int>
		{
			{
				ResourceType.Weapon,
				1
			},
			{
				ResourceType.WeaponModItem,
				1
			},
			{
				ResourceType.Magazine,
				1
			},
			{
				ResourceType.StationaryWeaponMagazine,
				1
			},
			{
				ResourceType.Ammo,
				3
			}
		};

		public int FreeFrameTimeWindow = 80000;

		public int AvgTimeBufferSize = 3600;

		public float MaxAvgFrameLoad = 1.15f;

		public bool ContinuationProfilerEnabled;
	}

	[Serializable]
	public class ProfilingSettings
	{
		public bool EnableInGameSpikeAnalyzer;

		public GClass14.EPauseReason PauseReason = GClass14.EPauseReason.All;

		public GClass14.DetectType DetectType;

		public float TriggerTime = 30f;

		public int FramesCountPerFile = 10;

		public string WorkingDirectory;

		public ProfilerArea[] ProfilingAreas = new ProfilerArea[1];
	}

	[Serializable]
	public class CharacterControllerSettings
	{
		public CharacterControllerSpawner.Mode ObservedPlayerMode = new CharacterControllerSpawner.Mode
		{
			Type = CharacterControllerSpawner.ControllerType.Impostor,
			WithRigidbody = true,
			WithKinematicOption = true
		};

		public CharacterControllerSpawner.Mode ClientPlayerMode = new CharacterControllerSpawner.Mode
		{
			Type = CharacterControllerSpawner.ControllerType.Simple,
			WithRigidbody = true,
			WithKinematicOption = true
		};

		public CharacterControllerSpawner.Mode BotPlayerMode = new CharacterControllerSpawner.Mode
		{
			Type = CharacterControllerSpawner.ControllerType.Simple,
			WithRigidbody = true,
			WithKinematicOption = true,
			WithMovementValidation = true
		};
	}

	[Serializable]
	public class PhysicsSettings
	{
		public bool OverrideMBPSettings;

		public Bounds OverrideMBPBounds;

		public int OverrideMBPSubdivisions;

		public bool ManualUpdate = true;

		public GClass833.GStruct51 CullingForLoot = new GClass833.GStruct51
		{
			check = (GClass833.CheckType.Distance | GClass833.CheckType.Frustrum),
			sqrDistance = 400f
		};

		public GClass833.GStruct51 CullingForGrenade = new GClass833.GStruct51
		{
			check = GClass833.CheckType.Distance,
			sqrDistance = 1600f
		};
	}

	[Serializable]
	public class NetworkConfig
	{
		public int FrameDiffForCorruptedTrigger = 1000;
	}

	[Serializable]
	public class LogConfig
	{
		public bool UnityDebugLogsEnabled;

		public bool DisableAllLogs;

		public float UpdateMetricsInterval = -1f;

		public GClass1357.GClass714.GClass1358 FrameMeasurerLoggerConfig = new GClass1357.GClass714.GClass1358
		{
			AvgStatisticsLogInterval = -1f,
			NetworkQualityLogInterval = -1f,
			BadFrame = float.MaxValue,
			BadFixedUpdate = float.MaxValue,
			BadFrameWithoutFixedUpdates = float.MaxValue
		};

		public int FramesCountForAvgForTimeMeasurers = 60;

		public int FramesCountForAvgForIncrementMeasurers = 5;
	}

	public string BackendUrl;

	public string MatchingVersion;

	public string BuildVersion;

	public ERaidMode RaidMode;

	public PoolsSettings Pools = new PoolsSettings();

	public int TargetFrameRate;

	public bool ResetSettings;

	public bool UseSpiritPlayer;

	public bool UseSpiritFastAnimator;

	public bool UseBodyFastAnimator;

	public bool UseHandsFastAnimator;

	public bool DevelopMode;

	public float FixedFrameRate;

	public int QualitySettingsAsyncUploadTimeSlice = 16;

	public int QualitySettingsAsyncUploadBufferSize = 32;

	public bool LogWhenShaderIsCompiled = true;

	public bool ShowSpiritImage;

	public bool UseSpiritHack;

	public ProfilingSettings Profiling = new ProfilingSettings();

	public float MaximumDeltaTime;

	public int SingleFixedUpdatesMeasurerCountForAVG = 30;

	public CharacterControllerSettings CharacterController = new CharacterControllerSettings();

	public PhysicsSettings Physics = new PhysicsSettings();

	public NetworkConfig Network = new NetworkConfig();

	public float CorpseSyncThreshold = 0.75f;

	public float DelayToOpenContainer = 0.5f;

	public bool NoLootForLocalGame;

	public int IconSaverScaleFactor = 2;

	public float LoadForceModeMultiplier = 3f;

	public LogConfig Log = new LogConfig();

	public bool PatchConfig(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return false;
		}
		try
		{
			ApplicationConfigClass applicationConfigClass = JsonParserClass.ParseJsonTo<ApplicationConfigClass>(json, Array.Empty<JsonConverter>());
			BackendUrl = applicationConfigClass.BackendUrl;
			MatchingVersion = applicationConfigClass.MatchingVersion;
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
	}
}

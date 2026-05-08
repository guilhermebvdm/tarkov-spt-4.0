using System;
using System.Collections.Generic;
using Audio.BackendSettings;
using CommonAssets.Scripts.Audio;
using EFT;
using EFT.Weather;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GClass1096
{
	public class GClass1097
	{
		public float FpVolume = 1f;

		public float TpVolume = 1f;

		public float GetVolume(EPointOfView pointOfView)
		{
			if (!GClass2078.IsFirstPerson(pointOfView))
			{
				return TpVolume;
			}
			return FpVolume;
		}
	}

	public class GClass1098
	{
		public string Name;

		public int GroupType;

		public float OverallVolume;

		public bool DisabledBinauralByDistance;

		public float DistanceToAllowBinaural = 60f;

		public float HeightToAllowBinaural = 10f;

		public float AngleToAllowBinaural = 30f;
	}

	public class GClass1099
	{
		public List<GClass1100> SurfaceMultipliers = new List<GClass1100>();

		public GClass1103 SummerSettings = new GClass1103();

		public GClass1103 AutumnSettings = new GClass1103();

		public GClass1103 WinterSettings = new GClass1103();

		public GClass1103 SpringSettings = new GClass1103();

		public GClass1103 AutumnLateSettings = new GClass1103();

		public GClass1103 SpringEarlySettings = new GClass1103();

		public GClass1103 StormSettings = new GClass1103();

		[NonSerialized]
		public Dictionary<ESeasonStatus, GClass1103> Dictionary_0 = new Dictionary<ESeasonStatus, GClass1103>();

		[NonSerialized]
		public Dictionary<BaseBallistic.ESurfaceSound, float> Dictionary_1 = new Dictionary<BaseBallistic.ESurfaceSound, float>();

		public float GetMultBySurface(BaseBallistic.ESurfaceSound surfaceType)
		{
			if (Dictionary_1.Count == 0)
			{
				method_0();
			}
			return Dictionary_1.GetValueOrDefault(surfaceType, 0f);
		}

		public void method_0()
		{
			foreach (GClass1100 surfaceMultiplier in SurfaceMultipliers)
			{
				Dictionary_1[surfaceMultiplier.SurfaceType] = surfaceMultiplier.VolumeMult;
			}
		}

		public GClass1103 GetSeasonSettings(ESeasonStatus season)
		{
			if (Dictionary_0.Count == 0)
			{
				method_1();
			}
			return Dictionary_0.GetValueOrDefault(season, new GClass1103());
		}

		public void method_1()
		{
			Dictionary_0[ESeasonStatus.Summer] = SummerSettings;
			Dictionary_0[ESeasonStatus.Autumn] = AutumnSettings;
			Dictionary_0[ESeasonStatus.Winter] = WinterSettings;
			Dictionary_0[ESeasonStatus.Spring] = SpringSettings;
			Dictionary_0[ESeasonStatus.AutumnLate] = AutumnLateSettings;
			Dictionary_0[ESeasonStatus.SpringEarly] = SpringEarlySettings;
			Dictionary_0[ESeasonStatus.Storm] = StormSettings;
		}
	}

	public class GClass1100
	{
		public BaseBallistic.ESurfaceSound SurfaceType;

		public float VolumeMult;
	}

	public class GClass1101
	{
		public EWindSpeed WindSpeed;

		public float VolumeMult = 1f;
	}

	public class GClass1102
	{
		public RainController.ERainIntensity RainIntensity;

		public float IndoorVolumeMult = 1f;

		public float OutdoorVolumeMult = 1f;
	}

	public class GClass1103
	{
		public float StepsVolumeMultiplier = 1f;

		public List<GClass1101> WindMultipliers = new List<GClass1101>();

		public List<GClass1102> RainSettings = new List<GClass1102>();

		[NonSerialized]
		public Dictionary<EWindSpeed, float> Dictionary_0 = new Dictionary<EWindSpeed, float>();

		[NonSerialized]
		public Dictionary<RainController.ERainIntensity, GClass1102> Dictionary_1 = new Dictionary<RainController.ERainIntensity, GClass1102>();

		public float GetWindMult(EWindSpeed windSpeed)
		{
			if (Dictionary_0.Count == 0)
			{
				foreach (GClass1101 windMultiplier in WindMultipliers)
				{
					Dictionary_0[windMultiplier.WindSpeed] = windMultiplier.VolumeMult;
				}
			}
			return Dictionary_0.GetValueOrDefault(windSpeed, 1f);
		}

		public float GetRainMult(EnvironmentType environment, RainController.ERainIntensity rainIntensity)
		{
			if (Dictionary_1.Count == 0)
			{
				foreach (GClass1102 rainSetting in RainSettings)
				{
					Dictionary_1[rainSetting.RainIntensity] = rainSetting;
				}
			}
			if (Dictionary_1.TryGetValue(rainIntensity, out var value))
			{
				if (environment != EnvironmentType.Indoor)
				{
					return value.OutdoorVolumeMult;
				}
				return value.IndoorVolumeMult;
			}
			return 1f;
		}
	}

	public class GClass1104
	{
		public float BaseMaxMovementRolloff = 70f;

		public List<GClass1105> MovementRolloffMultipliers = new List<GClass1105>();

		public float MinSpeedVolumeMult = 0.15f;

		public float MinSpeedRolloffMult = 0.8f;

		public float OutdoorRolloffMult = 0.9f;

		public float IndoorRolloffMult = 1f;

		public GClass1097 SearchSoundVolume = new GClass1097();

		[NonSerialized]
		public Dictionary<EAudioMovementState, float> Dictionary_0 = new Dictionary<EAudioMovementState, float>();

		public float GetMultByMovement(EAudioMovementState movementState)
		{
			if (Dictionary_0.Count == 0)
			{
				method_0();
			}
			return Dictionary_0.GetValueOrDefault(movementState, 1f);
		}

		public float GetRolloffMultByEnvironment(EnvironmentType environment)
		{
			if (environment != EnvironmentType.Outdoor)
			{
				return IndoorRolloffMult;
			}
			return OutdoorRolloffMult;
		}

		public void method_0()
		{
			foreach (GClass1105 movementRolloffMultiplier in MovementRolloffMultipliers)
			{
				Dictionary_0[movementRolloffMultiplier.MovementState] = movementRolloffMultiplier.RolloffMultiplier;
			}
		}
	}

	public class GClass1105
	{
		public EAudioMovementState MovementState;

		public float RolloffMultiplier = 1f;
	}

	public class GClass1106
	{
		public bool EnabledPluginErrorChecker = true;

		public float OutputVolumeCheckCooldown = 0.3f;

		public AudioGroupAcousticBackendSettings[] audioGroupAcousticSettings = Array.Empty<AudioGroupAcousticBackendSettings>();

		[NonSerialized]
		public Dictionary<BetterAudio.AudioSourceGroupType, AudioGroupAcousticBackendSettings> Dictionary_0 = new Dictionary<BetterAudio.AudioSourceGroupType, AudioGroupAcousticBackendSettings>();

		public bool TryGetSettingsForGroup(BetterAudio.AudioSourceGroupType groupType, out string jsonSettings)
		{
			jsonSettings = string.Empty;
			if (Dictionary_0.Count == 0 || Dictionary_0.Count != audioGroupAcousticSettings.Length)
			{
				Dictionary_0.Clear();
				AudioGroupAcousticBackendSettings[] array = audioGroupAcousticSettings;
				foreach (AudioGroupAcousticBackendSettings audioGroupAcousticBackendSettings in array)
				{
					Dictionary_0[audioGroupAcousticBackendSettings.groupType] = audioGroupAcousticBackendSettings;
				}
			}
			if (!Dictionary_0.TryGetValue(groupType, out var value))
			{
				return false;
			}
			jsonSettings = value.acousticSettings?.ToString(Formatting.None);
			if (string.IsNullOrEmpty(jsonSettings))
			{
				return false;
			}
			return true;
		}
	}

	[Serializable]
	public class AudioGroupAcousticBackendSettings
	{
		public BetterAudio.AudioSourceGroupType groupType = BetterAudio.AudioSourceGroupType.Environment;

		public JToken acousticSettings;
	}

	public class GClass1107
	{
		public float FadeDuration = 0.7f;

		public EAudioFadeType FadeIn = EAudioFadeType.InverseExponential;

		public EAudioFadeType FadeOut = EAudioFadeType.EaseIn;
	}

	[JsonProperty("AudioGroupPresets")]
	public GClass1098[] Presets;

	[JsonProperty("EnvironmentSettings")]
	public GClass1099 EnvironmentSettings = new GClass1099();

	[JsonProperty("PlayerSettings")]
	public GClass1104 PlayerSettings = new GClass1104();

	[JsonProperty("MetaXRAudioPluginSettings")]
	public GClass1106 MetaXRPluginSettings = new GClass1106();

	[JsonProperty("HeadphonesSettings")]
	public GClass1107 HeadphonesSettings = new GClass1107();

	[JsonProperty("OcclusionSettings")]
	public ClientAudioOcclusionSettings OcclusionSettings = new ClientAudioOcclusionSettings();

	[NonSerialized]
	public Dictionary<int, GClass1098> Dictionary_0;

	public bool TryGetPreset(int type, out GClass1098 preset)
	{
		if (Dictionary_0 != null)
		{
			return Dictionary_0.TryGetValue(type, out preset);
		}
		Dictionary_0 = new Dictionary<int, GClass1098>();
		if (Presets != null)
		{
			GClass1098[] presets = Presets;
			foreach (GClass1098 gClass in presets)
			{
				Dictionary_0.Add(gClass.GroupType, gClass);
			}
		}
		return Dictionary_0.TryGetValue(type, out preset);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.Bots;
using JsonType;
using Newtonsoft.Json;
using UnityEngine;

namespace EFT;

[Serializable]
public class RaidSettings : IRaidSettings<RaidSettings>
{
	[CompilerGenerated]
	public class Class1170
	{
		public RaidSettings raidSettingsCasted;

		public bool method_0(LocationSettingsClass.Location item)
		{
			return item.Id == raidSettingsCasted.LocationId;
		}
	}

	[JsonIgnore]
	public Dictionary<string, bool> SelectedGameModes = new Dictionary<string, bool>();

	[JsonProperty("keyId")]
	public string KeyId;

	[JsonProperty("onlinePveRaidStates")]
	public Dictionary<string, bool> OnlinePveRaidStates = new Dictionary<string, bool>();

	[JsonProperty("location")]
	public string LocationId;

	[JsonProperty("transitionType")]
	public ELocationTransition transitionType;

	[JsonIgnore]
	public bool isInTransition;

	[JsonProperty("timeVariant")]
	public EDateTime SelectedDateTime;

	[JsonProperty("metabolismDisabled")]
	public bool MetabolismDisabled;

	[JsonProperty("timeAndWeatherSettings")]
	public TimeAndWeatherSettings TimeAndWeatherSettings;

	[JsonProperty("botSettings")]
	public BotControllerSettings BotSettings;

	[JsonProperty("wavesSettings")]
	public WavesSettings WavesSettings = new WavesSettings(EBotAmount.AsOnline, EBotDifficulty.AsOnline, isBosses: true, isTaggedAndCursed: false);

	[JsonIgnore]
	public bool IsPveOffline;

	[NonSerialized]
	[JsonIgnore]
	public LocationSettingsClass.Location SelectedLocation_1;

	[NonSerialized]
	public LocationSettingsClass LocationSettings;

	[JsonProperty("side")]
	[field: NonSerialized]
	public ESideType Side { get; set; }

	[JsonIgnore]
	public LocationSettingsClass.Location SelectedLocation
	{
		get
		{
			return SelectedLocation_1;
		}
		set
		{
			SelectedLocation_1 = value;
			LocationId = SelectedLocation_1?.Id;
		}
	}

	[JsonIgnore]
	public bool OnlinePveRaid
	{
		get
		{
			if (SelectedLocation == null)
			{
				return false;
			}
			string id = SelectedLocation.Id;
			if (OnlinePveRaidStates.TryGetValue(id, out var value))
			{
				return value;
			}
			bool boolForProfile = PlayerPrefHelperClass.GetBoolForProfile("OnlinePveRaid_" + id);
			OnlinePveRaidStates[id] = boolForProfile;
			return boolForProfile;
		}
		set
		{
			if (SelectedLocation != null)
			{
				string id = SelectedLocation.Id;
				PlayerPrefHelperClass.SetBoolForProfile("OnlinePveRaid_" + id, value);
				OnlinePveRaidStates[id] = value;
			}
		}
	}

	[JsonIgnore]
	public bool NeedToGoOnlineInPve
	{
		get
		{
			if (!SelectedLocation.ForceOnlineRaidInPVE)
			{
				return OnlinePveRaid;
			}
			return true;
		}
	}

	[JsonProperty("raidMode")]
	[field: NonSerialized]
	public ERaidMode RaidMode { get; set; }

	[JsonProperty("playersSpawnPlace")]
	[field: NonSerialized]
	public EPlayersSpawnPlace PlayersSpawnPlace { get; set; }

	[JsonIgnore]
	public bool Local => RaidMode == ERaidMode.Local;

	[JsonIgnore]
	public bool IsPmc => Side == ESideType.Pmc;

	[JsonIgnore]
	public bool IsScav => Side == ESideType.Savage;

	public bool CanShowGroupPreview
	{
		get
		{
			if (RaidMode == ERaidMode.Coop)
			{
				return PlayersSpawnPlace == EPlayersSpawnPlace.SamePlace;
			}
			return true;
		}
	}

	[JsonIgnore]
	public int MaxGroupCount
	{
		get
		{
			if (Side != ESideType.Savage)
			{
				if (RaidMode != ERaidMode.Online)
				{
					return SelectedLocation.MaxCoopGroup;
				}
				return SelectedLocation.PmcMaxPlayersInGroup;
			}
			return SelectedLocation.ScavMaxPlayersInGroup;
		}
	}

	public void UpdateOnlinePveRaidStates()
	{
		try
		{
			Dictionary<string, LocationSettingsClass.Location>.ValueCollection valueCollection = LocationSettings?.locations?.Values;
			if (valueCollection == null || OnlinePveRaidStates == null)
			{
				return;
			}
			foreach (LocationSettingsClass.Location item in valueCollection)
			{
				if (item != null && item.Id != null)
				{
					bool boolForProfile = PlayerPrefHelperClass.GetBoolForProfile("OnlinePveRaid_" + item.Id);
					OnlinePveRaidStates[item.Id] = boolForProfile;
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public RaidSettings()
	{
	}

	public RaidSettings(ESideType side, EDateTime selectedDateTime, ERaidMode raidMode)
	{
		Side = side;
		SelectedDateTime = selectedDateTime;
		RaidMode = raidMode;
	}

	public RaidSettings(ESideType side, EDateTime selectedDateTime, ERaidMode raidMode, TimeAndWeatherSettings timeAndWeatherSettings, LocationSettingsClass locationSettings)
	{
		Side = side;
		SelectedDateTime = selectedDateTime;
		RaidMode = raidMode;
		TimeAndWeatherSettings = timeAndWeatherSettings;
		LocationSettings = locationSettings;
	}

	public RaidSettings Clone_1()
	{
		return Clone();
	}

	RaidSettings IRaidSettings<RaidSettings>.Clone()
	{
		//ILSpy generated this explicit interface implementation from .override directive in Clone_1
		return this.Clone_1();
	}

	public void Apply_1(RaidSettings raidSettings)
	{
		Apply(raidSettings);
	}

	void IRaidSettings<RaidSettings>.Apply(RaidSettings raidSettings)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Apply_1
		this.Apply_1(raidSettings);
	}

	public void ApplyFromBackend(RaidSettings raidSettings)
	{
		Apply(raidSettings);
		SelectedLocation = LocationSettings.locations.Values.FirstOrDefault((LocationSettingsClass.Location item) => item.Id == raidSettings.LocationId);
	}

	public void Apply(RaidSettings raidSettings)
	{
		Side = raidSettings.Side;
		SelectedLocation = raidSettings.SelectedLocation;
		SelectedDateTime = raidSettings.SelectedDateTime;
		KeyId = raidSettings.KeyId;
		RaidMode = raidSettings.RaidMode;
		MetabolismDisabled = raidSettings.MetabolismDisabled;
		PlayersSpawnPlace = raidSettings.PlayersSpawnPlace;
		TimeAndWeatherSettings = raidSettings.TimeAndWeatherSettings;
		BotSettings = raidSettings.BotSettings;
		WavesSettings = raidSettings.WavesSettings;
		transitionType = raidSettings.transitionType;
	}

	public EMatchingStatus GetMatchingStatus(int groupCount, EMatchingType matchingType)
	{
		EMatchingStatus result = EMatchingStatus.Ready;
		if (RaidMode != ERaidMode.Coop)
		{
			goto IL_0028;
		}
		if (matchingType != EMatchingType.Single && groupCount >= SelectedLocation.MinGroupSize)
		{
			if (matchingType != EMatchingType.GroupPlayer)
			{
				goto IL_0028;
			}
			result = EMatchingStatus.GroupPlayer;
		}
		else
		{
			result = EMatchingStatus.NotEnoughPlayers;
		}
		goto IL_002e;
		IL_002e:
		return result;
		IL_0028:
		if (matchingType == EMatchingType.GroupPlayer)
		{
			result = EMatchingStatus.GroupPlayer;
		}
		goto IL_002e;
	}

	public RaidSettings Clone()
	{
		return new RaidSettings
		{
			Side = Side,
			SelectedLocation = SelectedLocation,
			SelectedDateTime = SelectedDateTime,
			KeyId = KeyId,
			RaidMode = RaidMode,
			MetabolismDisabled = MetabolismDisabled,
			PlayersSpawnPlace = PlayersSpawnPlace,
			TimeAndWeatherSettings = TimeAndWeatherSettings,
			BotSettings = BotSettings,
			WavesSettings = WavesSettings
		};
	}

	public object GetUpdateStatusParams()
	{
		string text = KeyId;
		if (text == null)
		{
			text = string.Empty;
		}
		return new Class0<string, bool, string, string, string, EPlayersSpawnPlace, ELocationTransition>(SelectedLocation.Id, Side == ESideType.Savage, GClass867.ToStringNoBox(SelectedDateTime), text, GClass867.ToStringNoBox(RaidMode), PlayersSpawnPlace, transitionType);
	}

	public object GetCreateRaidGroupParams()
	{
		return new Class1<string, ERaidMode, EPlayersSpawnPlace>(SelectedLocation.Id, RaidMode, PlayersSpawnPlace);
	}
}

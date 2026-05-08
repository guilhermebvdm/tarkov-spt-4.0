using System.Collections.Generic;
using EFT;
using EFT.Bots;
using EFT.UI;
using EFT.UI.Matchmaker;
using UnityEngine;

public class MatchmakerRaidSettingsSummaryView : UIElement
{
	private const string string_0 = "Enabled";

	private const string string_1 = "Disabled";

	[SerializeField]
	private List<CanvasGroup> _labelsCanvasGroups;

	[SerializeField]
	private MatchmakerRaidSettingView _coopEnabledSetting;

	[SerializeField]
	private MatchmakerRaidSettingView _playerSpawnSetting;

	[SerializeField]
	private MatchmakerRaidSettingView _FoodAndWaterSetting;

	[SerializeField]
	private MatchmakerRaidSettingView _weatherSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _dayTimeSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _randomWeatherSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _randomTimeSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _timeFlowSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _freeCameraSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _openedDoorsSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _healthSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _overloadSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _armsStaminaSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _legsStaminaSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _aiAmountSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _aiDifficultySettings;

	[SerializeField]
	private MatchmakerRaidSettingView _silentBotsSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _bossesEnabledSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _bossPickSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _friendlyScavsSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _scavWarSettings;

	[SerializeField]
	private MatchmakerRaidSettingView _cursedSettings;

	public void Show(RaidSettings raidSettings)
	{
		ShowGameObject();
		string text = GClass2348.Localized("Enabled");
		string text2 = GClass2348.Localized("Disabled");
		ERaidMode raidMode = raidSettings.RaidMode;
		foreach (CanvasGroup labelsCanvasGroup in _labelsCanvasGroups)
		{
			GClass856.SetUnlockStatus(labelsCanvasGroup, raidMode != ERaidMode.Online);
		}
		_coopEnabledSetting.Refresh((raidSettings.RaidMode == ERaidMode.Coop) ? text : text2, raidMode == ERaidMode.Coop);
		_playerSpawnSetting.Refresh(GClass2348.Localized("PlayersSpawnPlace/" + GClass867.ToStringNoBox(raidSettings.PlayersSpawnPlace)), raidMode == ERaidMode.Coop);
		_FoodAndWaterSetting.Refresh(raidSettings.MetabolismDisabled ? text : text2, raidMode == ERaidMode.Coop);
		_weatherSettings.Refresh(GClass2348.Localized("CloudinessType/" + GClass867.ToStringNoBox(raidSettings.TimeAndWeatherSettings.CloudinessType)) + "/" + GClass2348.Localized("RainType/" + GClass867.ToStringNoBox(raidSettings.TimeAndWeatherSettings.RainType)) + "/" + GClass2348.Localized("WindSpeed/" + GClass867.ToStringNoBox(raidSettings.TimeAndWeatherSettings.WindType)) + "/" + GClass2348.Localized("FogType/" + GClass867.ToStringNoBox(raidSettings.TimeAndWeatherSettings.FogType)), raidMode == ERaidMode.Coop);
		_dayTimeSettings.Refresh((raidSettings.TimeAndWeatherSettings.HourOfDay == -1) ? GClass2348.Localized("BotAmount/" + GClass867.ToStringNoBox(EBotAmount.AsOnline)) : $"{raidSettings.TimeAndWeatherSettings.HourOfDay:00}:00", raidMode == ERaidMode.Coop);
		_randomWeatherSettings.Refresh(raidSettings.TimeAndWeatherSettings.IsRandomWeather ? text : text2, raidMode == ERaidMode.Local);
		_randomTimeSettings.Refresh(raidSettings.TimeAndWeatherSettings.IsRandomTime ? text : text2, raidMode == ERaidMode.Local);
		_timeFlowSettings.Refresh(GClass2348.Localized("TimeFlowType/" + GClass867.ToStringNoBox(raidSettings.TimeAndWeatherSettings.TimeFlowType)), isActive: false);
		_freeCameraSettings.Refresh(text2, isActive: false);
		_openedDoorsSettings.Refresh(text2, isActive: false);
		_overloadSettings.Refresh(text2, isActive: false);
		_armsStaminaSettings.Refresh(text2, isActive: false);
		_legsStaminaSettings.Refresh(text2, isActive: false);
		_healthSettings.Refresh("100%", isActive: false);
		_aiAmountSettings.Refresh(GClass2348.Localized("BotAmount/" + GClass867.ToStringNoBox(raidSettings.WavesSettings.BotAmount)), raidMode != ERaidMode.Online);
		_aiDifficultySettings.Refresh(GClass2348.Localized("BotDifficulty/" + GClass867.ToStringNoBox(raidSettings.WavesSettings.BotDifficulty)), raidMode != ERaidMode.Online && raidSettings.WavesSettings.BotAmount != EBotAmount.NoBots);
		_silentBotsSettings.Refresh(text2, isActive: false);
		_bossesEnabledSettings.Refresh(raidSettings.WavesSettings.IsBosses ? text : text2, raidMode == ERaidMode.Local);
		_bossPickSettings.Refresh(GClass2348.Localized("BotAmount/" + GClass867.ToStringNoBox(EBotAmount.AsOnline)), isActive: false);
		_friendlyScavsSettings.Refresh(text2, isActive: false);
		_scavWarSettings.Refresh(raidSettings.BotSettings.IsScavWars ? text : text2, raidMode == ERaidMode.Local);
		_cursedSettings.Refresh(raidSettings.WavesSettings.IsTaggedAndCursed ? text : text2, raidMode == ERaidMode.Local);
	}
}

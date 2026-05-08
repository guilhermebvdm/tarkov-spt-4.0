namespace EFT;

public interface IRaidSettings<TRaidSettings> where TRaidSettings : IRaidSettings<TRaidSettings>
{
	ESideType Side { get; }

	LocationSettingsClass.Location SelectedLocation { get; }

	ERaidMode RaidMode { get; }

	EPlayersSpawnPlace PlayersSpawnPlace { get; set; }

	bool CanShowGroupPreview { get; }

	int MaxGroupCount { get; }

	bool Local { get; }

	EMatchingStatus GetMatchingStatus(int groupCount, EMatchingType matchingType);

	TRaidSettings Clone();

	void Apply(TRaidSettings raidSettings);

	void ApplyFromBackend(TRaidSettings raidSettings);

	object GetUpdateStatusParams();

	object GetCreateRaidGroupParams();
}

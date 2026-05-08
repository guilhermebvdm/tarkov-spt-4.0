using System;
using System.Threading;
using System.Threading.Tasks;
using EFT;

public interface IBotCreator
{
	int BotsLoading { get; }

	bool StartProfilesLoaded { get; }

	int BundlesLoading { get; }

	Task<Profile> GenerateProfile(BotCreationDataClass data, CancellationToken cancellationToken, bool withDelete);

	Task ActivateBot(BotCreationDataClass data, BotZone zone, bool shallBeGroup, Func<BotOwner, BotZone, BotsGroup> groupAction, Action<BotOwner> callback, CancellationToken cancellationToken);

	Task ActivateBot(Profile profile, GClass682 position, BotZone zone, bool shallBeGroup, Func<BotOwner, BotZone, BotsGroup> groupAction, Action<BotOwner> callback, CancellationToken cancellationToken);

	void FillBackupProfilesData(GClass406 resultCache);

	void AddToTargetBackup(BotDifficulty difficulty, WildSpawnType role, int count);
}

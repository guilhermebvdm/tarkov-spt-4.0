using System.Threading;
using System.Threading.Tasks;
using EFT;

public interface GInterface21
{
	bool StartProfilesLoaded { get; }

	int BundlesLoading { get; }

	Task<Profile> CreateProfile(BotCreationDataClass data, CancellationToken cancellationToken, bool withDelete);

	void FillBackupProfilesData(GClass406 resultCache);

	void AddToTargetBackup(BotDifficulty difficulty, WildSpawnType role, int count);
}

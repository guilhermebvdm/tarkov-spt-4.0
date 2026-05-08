using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;

public interface IGetProfileData
{
	EPlayerSide? Side { get; }

	BotSpawnParams SpawnParams { get; set; }

	bool KeepZoneOnSpawn { get; }

	bool TryGetRole(out WildSpawnType role, out BotDifficulty difficulty);

	[CanBeNull]
	Profile ChooseProfile(List<Profile> profiles2Select, bool withDelete);

	WaveInfoClass[] PrepareToLoadBackend(int count);

	bool IsValidSpawnType(WildSpawnType wildSpawnType);

	string GetDebugLocalName();

	string GetDebugData();

	bool ShallChooseByData();

	bool IsBossOrFollowerByTime();

	bool IsZeroWave();

	bool IsBossOrFollower();

	bool IsSpawnOnStart();

	bool CanAtZoneByType(BotZone botZone, ZoneLeaveControllerClass botsControllerZonesLeaveController);
}

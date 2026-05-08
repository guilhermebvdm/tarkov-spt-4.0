using System;
using System.Collections.Generic;
using EFT;

public interface GInterface4
{
	Player MyPlayer { get; }

	bool IsDeveloperMode { get; }

	GameWorld GameWorld { get; }

	event Action<string, GClass408> OnPlayerDataAdded;

	event Action<string, GClass408> OnPlayerDataRemoved;

	void Initialize(int playerId = 0);

	List<GStruct21> GetGroupData();

	GClass406 GetProfilesData();

	void SetNetworkDataUpdateKd(float networkDataUpdateKd);
}

using System;
using System.Threading.Tasks;

public interface GInterface29 : IDisposable
{
	ESeason Season { get; }

	ESeasonStatus Status { get; }

	float SnowLevelOnTerrain { get; }

	event Action<ESeasonStatus> StatusChangedEvent;

	event Action<float> SnowLevelOnTerrainChangedEvent;

	Task HandleReconnect(ESeasonStatus seasonStatus, SeasonsSettingsClass seasonsSettings);

	void Register(GInterface49 winterVisual);

	void Unregister(GInterface49 winterVisual);
}

using System;
using EFT;
using EFT.Weather;

public interface IBotGame
{
	GameStatus Status { get; }

	GameDateTime GameDateTime { get; }

	BotsController BotsController { get; }

	IWeatherCurve WeatherCurve { get; }

	BossSpawnScenario BossSpawnScenario { get; }

	event Action UpdateByUnity;

	void BotDespawn(BotOwner bot);
}

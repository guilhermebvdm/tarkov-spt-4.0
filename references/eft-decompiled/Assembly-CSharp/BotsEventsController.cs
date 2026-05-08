using System;
using EFT;

public class BotsEventsController
{
	[field: NonSerialized]
	public BotHalloweenWithZombies BotHalloweenWithZombies { get; }

	[field: NonSerialized]
	public BotsForceAttackEvent ForceAttackEvent { get; }

	[field: NonSerialized]
	public BotsKhorovodEvent KhorovodEvent { get; }

	[field: NonSerialized]
	public FollowPlayerEvent FollowPlayerEvent { get; }

	[field: NonSerialized]
	public BotHalloweenEvent BotHalloweenEvent { get; }

	[field: NonSerialized]
	public BotsMinotaurLabirint BotsMinotaurLabirint { get; }

	[field: NonSerialized]
	public BotsPatrolGeneratorGameEvent BotPatrolGeneratorGameEvent { get; }

	public BotsEventsController(GameDateTime gameDateTime, BotsClass botsList, ZoneLeaveControllerClass botZonesLeaveController, BotSpawner spawner, AIPlaceInfoHolder placesInfo, LocationSettingsClass.Location.EventsDataClass events, AICoversData covers)
	{
		ForceAttackEvent = new BotsForceAttackEvent(gameDateTime, botsList, botZonesLeaveController);
		FollowPlayerEvent = new FollowPlayerEvent(botsList);
		KhorovodEvent = new BotsKhorovodEvent(this, botsList, events?.Khorovod);
		BotHalloweenEvent = new BotHalloweenEvent(this, spawner, placesInfo);
		BotsMinotaurLabirint = new BotsMinotaurLabirint();
		BotHalloweenWithZombies = new BotHalloweenWithZombies(this, botsList, spawner, events?.Halloween2024);
		BotPatrolGeneratorGameEvent = new BotsPatrolGeneratorGameEvent(botsList, covers);
	}

	public void Activate()
	{
		if (LocalBotSettingsProviderClass.Core.ACTIVE_FORCE_ATTACK_EVENTS)
		{
			ForceAttackEvent.Activate();
		}
		if (LocalBotSettingsProviderClass.Core.ACTIVE_FOLLOW_PLAYER_EVENT)
		{
			FollowPlayerEvent.Activate();
		}
		if (LocalBotSettingsProviderClass.Core.ACTIVE_HALLOWEEN_ZOMBIES_EVENT)
		{
			BotHalloweenWithZombies.Activate();
		}
		if (LocalBotSettingsProviderClass.Core.ACTIVE_PATROL_GENERATOR_EVENT)
		{
			BotPatrolGeneratorGameEvent.Activate();
		}
	}

	public void ManualUpdate()
	{
		if (LocalBotSettingsProviderClass.Core.ACTIVE_HALLOWEEN_ZOMBIES_EVENT)
		{
			BotHalloweenWithZombies.ManualUpdate();
		}
		if (LocalBotSettingsProviderClass.Core.ACTIVE_FORCE_KHOROVOD_EVENTS)
		{
			KhorovodEvent.ManualUpdate();
		}
	}

	public void SpawnAction()
	{
		if (LocalBotSettingsProviderClass.Core.ACTIVE_HALLOWEEN_ZOMBIES_EVENT)
		{
			BotHalloweenWithZombies?.SpawnAction();
		}
	}

	public void Dispose()
	{
		BotsMinotaurLabirint.Dispose();
		KhorovodEvent.Dispose();
		ForceAttackEvent.Dispose();
		FollowPlayerEvent.Dispose();
		BotHalloweenWithZombies.Dispose();
		BotPatrolGeneratorGameEvent.Dispose();
	}
}

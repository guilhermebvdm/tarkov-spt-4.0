using System;
using System.Collections.Generic;
using EFT;

public class BotsKhorovodEvent : GClass670
{
	public bool IsActive;

	public readonly LocationSettingsClass.Location.GClass1426 Config;

	[NonSerialized]
	public BotsEventsController BotsEventsController;

	public override List<int> LayersToActivate => new List<int> { 503 };

	public override List<int> LayersToDeActivate => new List<int>();

	public BotsKhorovodEvent(BotsEventsController botsEventsController, BotsClass botsList, LocationSettingsClass.Location.GClass1426 khorovodConfig)
		: base(botsList)
	{
		BotsEventsController = botsEventsController;
		Config = khorovodConfig;
		if (Config == null)
		{
			Config = new LocationSettingsClass.Location.GClass1426();
		}
	}

	public override bool CanActivate(BotOwner bot)
	{
		return true;
	}

	public override void Activate()
	{
		IsActive = true;
		BotsClass.OnBotAdd += method_1;
	}

	public override void Dispose()
	{
		BotsClass.OnBotAdd -= method_1;
		IsActive = false;
	}

	public void ManualUpdate()
	{
		BotsPatrolGeneratorGameEvent botPatrolGeneratorGameEvent = BotsEventsController.BotPatrolGeneratorGameEvent;
		if (!IsActive && botPatrolGeneratorGameEvent.EventObjectCompleteSuccess)
		{
			Activate();
			Start("GeneratorCompleteSuccess");
		}
	}

	public void method_1(BotOwner botOwner)
	{
		if (IsActive)
		{
			method_0(botOwner);
		}
	}
}

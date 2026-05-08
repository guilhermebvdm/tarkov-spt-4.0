using System;
using EFT;

public class BotTradersServices
{
	[NonSerialized]
	public BotLighthouseKeeperServices LighthouseKeeperServices_1;

	[NonSerialized]
	public BotBTRService BtrServices;

	public BotLighthouseKeeperServices LighthouseKeeperServices => LighthouseKeeperServices_1;

	public BotBTRService BTRServices => BtrServices;

	public BotTradersServices(BotsController botController)
	{
		LighthouseKeeperServices_1 = new BotLighthouseKeeperServices(botController);
		BtrServices = new BotBTRService(botController);
	}

	public void Dispose()
	{
		LighthouseKeeperServices_1.Dispose();
		BtrServices.Dispose();
		LighthouseKeeperServices_1 = null;
		BtrServices = null;
	}
}

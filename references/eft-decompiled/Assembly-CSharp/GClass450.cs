using EFT;

public class GClass450 : ABossLogic
{
	public GClass450(BotOwner owner, BotBoss bossLogic)
		: base(owner, bossLogic)
	{
	}

	public override void SetPatrolMode()
	{
	}

	public override void Activate()
	{
		BotOwner_0.GetPlayer.BeingHitAction += method_1;
		BotOwner_0.GetPlayer.OnPlayerDead += method_0;
	}

	public void method_0(Player player, IPlayer lastaggressor, DamageInfoStruct damageinfo, EBodyPart part)
	{
		BotOwner_0.BotsGroup.BotGame.BotsController.EventsController.BotHalloweenEvent.AddToKill(lastaggressor);
	}

	public void method_1(DamageInfoStruct info, EBodyPart arg2, float arg3)
	{
		if (info.HaveOwner && !info.Player.IsAI)
		{
			BotOwner_0.BotsGroup.BotGame.BotsController.EventsController.BotHalloweenEvent.AddToKill(info.Player.iPlayer);
			if (!BotOwner_0.HealthController.IsAlive)
			{
				BotOwner_0.BotsController.EventsController.BotHalloweenEvent.PeaceZryachiyKilled();
			}
		}
	}

	public override void BossLogicUpdate()
	{
	}

	public override void Dispose()
	{
		BotOwner_0.GetPlayer.BeingHitAction -= method_1;
		if (!BotOwner_0.HealthController.IsAlive)
		{
			BotOwner_0.GetPlayer.OnPlayerDead -= method_0;
			BotOwner_0.BotsController.EventsController.BotHalloweenEvent.PeaceZryachiyKilled();
		}
	}
}

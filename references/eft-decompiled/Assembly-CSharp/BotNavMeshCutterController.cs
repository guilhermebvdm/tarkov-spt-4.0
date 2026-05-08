using System;
using EFT;

public class BotNavMeshCutterController : GClass429
{
	public BotNavMeshCutterController(BotOwner owner)
		: base(owner)
	{
	}

	public void Activate()
	{
		GClass636 cutController = BotOwner_0.BotsGroup.BotGame.BotsController.CutController;
		cutController.OnCutComplete = (Action<BotOwner, NavMeshCutElement>)Delegate.Combine(cutController.OnCutComplete, new Action<BotOwner, NavMeshCutElement>(method_0));
	}

	public void method_0(BotOwner bot, NavMeshCutElement element)
	{
		if (bot.BotsGroup.BotZone == BotOwner_0.BotsGroup.BotZone && (element.Position - BotOwner_0.Position).sqrMagnitude < 225f)
		{
			BotOwner_0.Mover.RecalcWay();
		}
	}

	public void Dispose()
	{
		GClass636 cutController = BotOwner_0.BotsGroup.BotGame.BotsController.CutController;
		cutController.OnCutComplete = (Action<BotOwner, NavMeshCutElement>)Delegate.Remove(cutController.OnCutComplete, new Action<BotOwner, NavMeshCutElement>(method_0));
	}
}

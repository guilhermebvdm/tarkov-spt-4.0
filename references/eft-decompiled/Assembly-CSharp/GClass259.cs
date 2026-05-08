using System;
using EFT;

public class GClass259 : GClass177<GClass26>
{
	[NonSerialized]
	public GClass214 Gclass214_0;

	public GClass596 GClass596_0 => BotOwner_0.BotRequestController.CurRequest as GClass596;

	public GClass259(BotOwner bot)
		: base(bot)
	{
		Gclass214_0 = new GClass214(bot);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if ((BotOwner_0.Position - GClass596_0.Door.transform.position).sqrMagnitude < 2f)
		{
			BotOwner_0.DoorOpener.Interact(GClass596_0.Door, EInteractionType.Open);
		}
		else
		{
			Gclass214_0.UpdateNodeByBrain(data);
		}
	}
}

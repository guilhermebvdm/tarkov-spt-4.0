using EFT;
using UnityEngine;

public class GClass272 : GClass177<GClass26>
{
	public GClass272(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.SetPose(0.1f);
		BotOwner_0.StopMove();
		if (!BotOwner_0.MinesData.Planting && BotOwner_0.MinesData.CurrentActive != null)
		{
			Vector3 vector = BotOwner_0.MinesData.CurrentActive.Position - BotOwner_0.Position;
			vector.y = 0f;
			if (vector.sqrMagnitude > 0.01f)
			{
				BotOwner_0.Steering.LookToPoint(BotOwner_0.MinesData.CurrentActive.Position);
			}
			BotOwner_0.MinesData.StartPlant(BotOwner_0.MinesData.CurrentActive);
		}
	}
}

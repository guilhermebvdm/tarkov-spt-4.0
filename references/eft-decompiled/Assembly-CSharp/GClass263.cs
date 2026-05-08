using EFT;
using UnityEngine;

public class GClass263(BotOwner bot) : GClass177<GClass26>(bot)
{
	public readonly float VISIBLE_SECTOR = 0.7660444f;

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.Mover.Stop();
		if (BotOwner_0.Gesture.CurRequestExecuting())
		{
			Vector3 vector = BotOwner_0.Gesture.CurRequest.Requester.Transform.position - BotOwner_0.Transform.position;
			if (BotOwner_0.Gesture.CurRequest.Executing == AIRequestStage.lookAt && GClass855.IsAngLessNormalized(GClass855.NormalizeFastSelf(BotOwner_0.LookDirection), GClass855.NormalizeFastSelf(vector), VISIBLE_SECTOR))
			{
				BotOwner_0.Gesture.StartSecondPart();
			}
			BotOwner_0.Steering.LookToDirection(vector);
		}
		else
		{
			BotOwner_0.Gesture.StartExecuteCur();
		}
	}
}

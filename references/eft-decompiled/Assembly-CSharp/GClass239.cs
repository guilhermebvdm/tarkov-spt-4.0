using EFT;
using UnityEngine;

public class GClass239 : GClass236
{
	public override Vector3 MainTargetPosition => BotOwner_0.GoToSomePointData.Point;

	public GClass239(BotOwner bot)
		: base(bot, 70f, 30f)
	{
	}

	public override void LookSimple()
	{
		BotObserveDataClass.SetVectorToLook(BotOwner_0.Position - BotOwner_0.Mover.CurrentCornerPoint);
		BotObserveDataClass.Update();
	}

	public override void MoveToMainTarget()
	{
		bool num = BotOwner_0.Mover.IsComeTo(BotOwner_0.Settings.FileSettings.Move.REACH_DIST, onCover: true);
		BotOwner_0.SetPose(1f);
		if (num && !BotOwner_0.Memory.ComeToPoint())
		{
			BotOwner_0.BotAttackManager.UpdateNextTick();
		}
		if (!BotOwner_0.HasPathAndNotComplete && BotOwner_0.GoToSomePointData.HaveTarget())
		{
			if ((BotOwner_0.Position - MainTargetPosition).sqrMagnitude > 1f)
			{
				BotOwner_0.GoToPoint(MainTargetPosition);
			}
			else if (!BotOwner_0.Memory.ComeToPoint())
			{
				BotOwner_0.BotAttackManager.UpdateNextTick();
			}
		}
	}

	public override void MoverToMainTarget(CustomNavigationPoint navigationPoint)
	{
		if (!method_6(MainTargetPosition))
		{
			BotOwner_0.GoToPoint(MainTargetPosition);
		}
	}
}

using System;
using EFT;
using UnityEngine;

public class GClass245 : GClass200<GClass26>
{
	[NonSerialized]
	public GClass25 Gclass25_0;

	public GClass245(BotOwner bot)
		: base(bot)
	{
		Gclass25_0 = new GClass25(5f, method_5);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.Sprint(val: false);
		PatrolLootPointsData lootData = BotOwner_0.PatrollingData.LootData;
		AILootPoint cachedLootPoint = lootData.CachedLootPoint;
		float num = Mathf.Abs(cachedLootPoint.Position.y - BotOwner_0.Position.y);
		BotOwner_0.SetPose(1f);
		if (num < 1f && BotOwner_0.SqrDistHorizontal(cachedLootPoint.Position) < 3f)
		{
			BotOwner_0.StopMove();
			BotDeadBodyWork.LookToLoot(BotOwner_0, cachedLootPoint.Position);
			lootData.ComeToLootPoint();
		}
		else
		{
			lootData.ResetLootState();
			BotOwner_0.Steering.LookToMovingDirection();
			Gclass25_0.Update();
			BotOwner_0.LookData.SetLookPointByHearing();
		}
		lootData.UpdateLootState();
		lootData.CheckWantLeave();
	}

	public void method_5()
	{
		BotOwner_0.GoToPoint(BotOwner_0.PatrollingData.LootData.CachedLootPoint.Position);
	}
}

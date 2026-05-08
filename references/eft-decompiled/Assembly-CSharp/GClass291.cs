using System;
using EFT;
using UnityEngine;

public class GClass291 : GClass177<GClass26>
{
	[NonSerialized]
	public float Float_0;

	public GClass291(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.StopMove();
		ManualUpdate();
	}

	public void ManualUpdate()
	{
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 2f;
			BotOwner_0.ItemDropper.RefreshItemToDrop();
			BotOwner_0.ItemDropper.TryDoDrop();
		}
	}
}

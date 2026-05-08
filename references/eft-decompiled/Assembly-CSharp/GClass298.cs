using System;
using EFT;
using UnityEngine;

public class GClass298 : GClass177<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	public GClass298(BotOwner bot)
		: base(bot)
	{
	}

	public override void Awake()
	{
		base.Awake();
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			if (BotOwner_0.Memory.GoalEnemy.Distance > 10f)
			{
				if (Float_1 < Time.time)
				{
					Float_1 = Time.time + 5f;
					BotOwner_0.GoToPoint(BotOwner_0.Position);
				}
			}
			else
			{
				BotOwner_0.StopMove();
			}
		}
		else
		{
			BotOwner_0.StopMove();
		}
		BotOwner_0.SetPose(1f);
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 10f;
			BotOwner_0.Medecine.FirstAid.SetRandomPartToHeal();
			BotOwner_0.Medecine.FirstAid.TryApplyToCurrentPart();
		}
	}
}

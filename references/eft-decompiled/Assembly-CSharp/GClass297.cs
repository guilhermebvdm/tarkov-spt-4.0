using System;
using EFT;
using UnityEngine;

public class GClass297 : GClass177<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public GClass178 Gclass178_0;

	public GClass297(BotOwner bot)
		: base(bot)
	{
		Gclass178_0 = new GClass183(bot);
		Gclass25_0 = new GClass25(2f, method_0);
	}

	public void method_0()
	{
		Bool_0 = !Bool_0;
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		Gclass25_0.Update();
		BotOwner_0.StopMove();
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + GClass856.Random(2f, 4f);
			if (!BotOwner_0.BotLay.IsLay)
			{
				BotOwner_0.BotLay.TryLay();
			}
			else
			{
				BotOwner_0.BotLay.GetUp(withCheck: false);
				BotOwner_0.SetPose(1f);
			}
		}
		if (Bool_0)
		{
			Gclass178_0.UpdateNodeByBrain(data as GClass27);
		}
	}
}

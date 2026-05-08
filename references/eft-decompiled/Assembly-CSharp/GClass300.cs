using System;
using EFT;

public class GClass300(BotOwner bot) : GClass177<GClass26>(bot)
{
	public bool OffHead;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_0 = -15f;

	[NonSerialized]
	public float Float_1;

	public override void UpdateNodeByBrain(GClass26 data)
	{
		BotOwner_0.StopMove();
	}
}

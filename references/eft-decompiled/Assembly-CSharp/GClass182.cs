using EFT;
using UnityEngine;

public class GClass182 : GClass178
{
	public override bool _checkIsReady => false;

	public GClass182(BotOwner bot)
		: base(bot)
	{
	}

	public override Vector3? GetTarget()
	{
		return BotOwner_0.FlashGrenade.PlaceToShoot.Value;
	}
}

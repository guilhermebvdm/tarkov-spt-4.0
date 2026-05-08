using EFT;
using UnityEngine;

public class GClass185 : GClass178
{
	public override bool _checkIsReady => false;

	public GClass185(BotOwner bot)
		: base(bot)
	{
	}

	public override Vector3? GetTarget()
	{
		return BotOwner_0.SmokeGrenade.PlaceToShoot;
	}
}

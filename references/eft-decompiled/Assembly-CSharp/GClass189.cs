using EFT;
using UnityEngine;

public class GClass189 : GClass178
{
	public WarnPlayerRequest WarnPlayerRequest_0 => BotOwner_0.WarnData.WarnPlayerRequest;

	public GClass189(BotOwner bot)
		: base(bot)
	{
	}

	public override Vector3? GetTarget()
	{
		return WarnPlayerRequest_0.PointToShoot();
	}
}

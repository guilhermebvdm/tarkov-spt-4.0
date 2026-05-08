using EFT;
using UnityEngine;

public class GClass238 : GClass236
{
	public override float NEXT_COVER_LONG => 20f;

	public override float NEXT_COVER_SHORT => 15f;

	public override float STAY_AT_COVER_WITH_ENEMY => 15f;

	public override float ENEMY_VISIBLE_RECENTLY => 6f;

	public override float STAY_AT_COVER => 20f;

	public override float CHANCE_TO_COVER_SPRINT_ON_MIN_DIST_OR_LOWER => -1f;

	public override float CHANCE_TO_COVER_SPRINT_ON_MAX_DIST_OR_HIGHER => -1f;

	public virtual float CHANCE_TO_SPRINT_ON_MIN_DIST_OR_LOWER => 0f;

	public virtual float CHANCE_TO_SPRINT_ON_MAX_DIST_OR_HIGHER => 0f;

	public GClass238(BotOwner bot)
		: base(bot, 10f, 10f)
	{
	}

	public override void LookSimple()
	{
		BotObserveDataClass.SetVectorToLook(BotOwner_0.Position - BotOwner_0.Mover.CurrentCornerPoint);
		BotObserveDataClass.Update();
	}

	public override void LookStaingAtSub()
	{
		if (Bool_0 && Time.time - BotOwner_0.Memory.ComeToCoverTime < 3f)
		{
			UpdateLookFromCover();
			return;
		}
		if (GroupPoint_0 != null)
		{
			GroupPoint groupPoint_ = GroupPoint_0;
			BotObserveDataClass.SetVectorToLook(-groupPoint_.WallDirection);
		}
		BotObserveDataClass.Update();
	}
}

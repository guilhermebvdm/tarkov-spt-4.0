using System;
using EFT;
using UnityEngine;

public abstract class GClass222 : GClass200<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	public GClass222(BotOwner bot)
		: base(bot)
	{
	}

	public bool method_5(out Vector3 curTargetPos)
	{
		if (!BotOwner_0.Mover.HasPathAndNoComplete)
		{
			curTargetPos = Vector3.zero;
			return false;
		}
		curTargetPos = BotOwner_0.Mover.CurrentCornerPoint;
		return GClass369.CanShoot(curTargetPos, BotOwner_0.Memory.GoalEnemy);
	}

	public void method_6()
	{
		if (!(Float_1 > Time.time))
		{
			Float_1 = Time.time + 2f;
			if (BotOwner_0.WeaponManager.IsMelee)
			{
				BotOwner_0.WeaponManager.Selector.ChangeToMain();
			}
		}
	}

	public virtual void NotMovingCheck()
	{
		if (!(Float_0 > Time.time))
		{
			Float_0 = Time.time + 3f;
			Vector3 currPosition = BotOwner_0.Memory.GoalEnemy.CurrPosition;
			BotOwner_0.MoveToEnemyData.TryMoveToEnemy(currPosition);
		}
	}
}

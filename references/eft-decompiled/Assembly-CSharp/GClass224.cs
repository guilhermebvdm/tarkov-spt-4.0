using System;
using EFT;
using UnityEngine;

public abstract class GClass224 : GClass222
{
	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public int Int_0 = -1;

	public GClass224(BotOwner bot)
		: base(bot)
	{
	}

	public override void NotMovingCheck()
	{
		if (Float_2 > Time.time)
		{
			return;
		}
		Float_2 = Time.time + 1f;
		int id = BotOwner_0.Memory.GoalEnemy.Person.Id;
		Vector3 vector = (Vector3_0 = BotOwner_0.Memory.GoalEnemy.CurrPosition);
		if (Int_0 == id)
		{
			float sqrMagnitude = (Vector3_0 - vector).sqrMagnitude;
			if (!BotOwner_0.MoveToEnemyData.ShallRecalWay(out var _) && BotOwner_0.HasPathAndNotComplete && (Time.time - BotOwner_0.Mover.LastPathSetTime < 1f || sqrMagnitude < 4f))
			{
				return;
			}
		}
		Int_0 = id;
		if (!BotRun.CalcPathToMoveZigZagAndGo(vector, BotOwner_0, EZigZAgType.allSteps))
		{
			base.NotMovingCheck();
		}
	}

	public void method_7()
	{
		BotOwner_0.StopMove();
	}
}

using System;
using EFT;
using UnityEngine;

public class GClass506 : GClass429, IProductionUpdate
{
	[NonSerialized]
	public BotObserveDataClass BotObserveDataClass;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public PatrollingData Data;

	public GClass506(BotOwner owner, PatrollingData data, BotObserveDataClass botObserveData)
		: base(owner)
	{
		Vector3_0 = owner.LookDirection;
		BotObserveDataClass = botObserveData;
		Data = data;
	}

	public void Update()
	{
		if (Data.Status != PatrolStatus.stay)
		{
			return;
		}
		if (Float_0 < Time.time)
		{
			PatrolPoint subPoint = Data.CurPatrolPoint.GetSubPoint(BotOwner_0.BotFollower.Index);
			if (subPoint.HaveLookSide)
			{
				Vector3_0 = subPoint.LookDir(0);
			}
			else
			{
				Float_0 = Time.time + 10f;
				Vector3 vector = GClass369.Test4Sides(BotOwner_0.LookDirection, BotOwner_0.GetPlayer.PlayerBones.Head.position);
				if (vector == Vector3.one)
				{
					BotObserveDataClass.SetVector(BotOwner_0.Mover.NormDirCurPoint, 25f);
					return;
				}
				vector.y = 0f;
				Vector3_0 = vector;
				if (Vector3_0 == Vector3.zero)
				{
					Vector3_0 = BotOwner_0.LookDirection;
				}
				BotObserveDataClass.SetVector(Vector3_0, 25f);
			}
		}
		BotObserveDataClass.SetVectorToLook(Vector3_0);
		BotObserveDataClass.Update();
	}

	public void SetDefault(bool max)
	{
		Float_0 = 0f;
	}

	public void SetComeToPoint()
	{
	}
}

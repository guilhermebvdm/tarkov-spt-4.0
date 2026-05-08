using EFT;
using UnityEngine;

public class GClass508 : GClass507
{
	public GClass508(BotOwner owner, PatrollingData data, BotObserveDataClass botObserveData)
		: base(owner, data, botObserveData)
	{
	}

	public override void Update()
	{
		if (Data.Status == PatrolStatus.stay)
		{
			BotOwner_0.StopMove();
			if (Float_0 < Time.time)
			{
				Float_0 = method_0(Data.CurPatrolPoint.TargetPoint);
				Int_1 = GClass856.RandomInclude(0, Data.CurPatrolPoint.TargetPoint.LookIndexes - 1);
				if (Int_1 < 0)
				{
					Vector3 vector = method_1(BotOwner_0.LookDirection, BotOwner_0.GetPlayer.PlayerBones.Head.position);
					if (vector == Vector3.one)
					{
						return;
					}
					Vector3_0 = vector;
				}
				else
				{
					Vector3? lookDirection = Data.CurPatrolPoint.TargetPoint.GetLookDirection(Int_1, Data.PointChooser.WasCutted);
					if (lookDirection.HasValue)
					{
						Vector3 vector2 = lookDirection.Value - Data.CurPatrolPoint.Position;
						Debug.DrawRay(BotOwner_0.Position + Vector3.up, vector2, Color.yellow, 6f);
						if (vector2 == Vector3.zero)
						{
							Vector3_0 = BotOwner_0.LookDirection;
						}
						else
						{
							Vector3 vector3 = method_1(vector2, BotOwner_0.GetPlayer.PlayerBones.Head.position);
							if (vector3 == Vector3.one)
							{
								return;
							}
							Vector3_0 = vector3;
						}
					}
					else
					{
						Vector3 lookDirection2 = BotOwner_0.LookDirection;
						Vector3 vector4 = method_1(lookDirection2, BotOwner_0.GetPlayer.PlayerBones.Head.position);
						if (vector4 == Vector3.one)
						{
							return;
						}
						vector4.y = 0f;
						Vector3_0 = vector4;
						if (Vector3_0 == Vector3.zero)
						{
							Vector3_0 = lookDirection2;
						}
					}
				}
				BotObserveDataClass.SetVector(Vector3_0, 25f);
			}
			BotObserveDataClass.SetVectorToLook(Vector3_0);
			BotObserveDataClass.Update();
			if (Data.CurPatrolPoint.TargetPoint.ShallSit)
			{
				BotOwner_0.SetPose(0.01f);
			}
			else
			{
				BotOwner_0.SetPose(1f);
			}
		}
		else
		{
			BotOwner_0.SetPose(1f);
		}
	}
}

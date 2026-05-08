using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass507 : GClass429, IProductionUpdate
{
	[Serializable]
	[CompilerGenerated]
	public class Class257
	{
		public static readonly Class257 class257_0 = new Class257();

		public static Func<int, float> func_0;

		public float method_0(int x)
		{
			return GClass856.Random(0f, 5f);
		}
	}

	[NonSerialized]
	public const int Int_0 = 8;

	public int[] shuffledDirections = new int[8];

	[NonSerialized]
	public BotObserveDataClass BotObserveDataClass;

	[NonSerialized]
	public Vector3 Vector3_0 = Vector3.forward;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1 = 1f;

	[NonSerialized]
	public PatrollingData Data;

	[NonSerialized]
	public int Int_1;

	public GClass507(BotOwner owner, PatrollingData data, BotObserveDataClass botObserveData)
		: base(owner)
	{
		BotObserveDataClass = botObserveData;
		Data = data;
		for (int i = 0; i < 8; i++)
		{
			shuffledDirections[i] = i;
		}
		shuffledDirections = shuffledDirections.OrderBy((int x) => GClass856.Random(0f, 5f)).ToArray();
	}

	public virtual void Update()
	{
		if (Data.Status == PatrolStatus.stay)
		{
			BotOwner_0.SetPose(Float_1);
			BotOwner_0.StopMove();
			if (Float_0 < Time.time)
			{
				Float_0 = method_0(Data.CurPatrolPoint.TargetPoint);
				Int_1--;
				float num = Time.time - Data.ComeToPointTime;
				if (Int_1 < 0 && num > 2f)
				{
					Float_0 = Time.time + 4f;
					Data.PointChooser.FindNextPoint(withSetting: true, withoutNext: false);
					return;
				}
				if (Data.PointChooser.WasCutted)
				{
					Vector3 vector = method_1(BotOwner_0.LookDirection, BotOwner_0.GetPlayer.PlayerBones.Head.position);
					if (vector == Vector3.one)
					{
						Data.PointChooser.FindNextPoint(withSetting: true, withoutNext: true);
						return;
					}
					Vector3_0 = vector;
				}
				else
				{
					Vector3? lookDirection = Data.CurPatrolPoint.TargetPoint.GetLookDirection(Int_1, Data.PointChooser.WasCutted);
					if (lookDirection.HasValue)
					{
						Vector3 value = lookDirection.Value;
						if (value == Vector3.zero)
						{
							Vector3_0 = BotOwner_0.LookDirection;
						}
						else
						{
							Vector3_0 = value;
						}
					}
					else
					{
						Vector3 lookDirection2 = BotOwner_0.LookDirection;
						Vector3 vector2 = method_1(lookDirection2, BotOwner_0.GetPlayer.PlayerBones.Head.position);
						if (vector2 == Vector3.one)
						{
							Data.PointChooser.FindNextPoint(withSetting: true, withoutNext: true);
							return;
						}
						vector2.y = 0f;
						Vector3_0 = vector2;
						if (Vector3_0 == Vector3.zero)
						{
							Vector3_0 = lookDirection2;
						}
					}
					BotObserveDataClass.SetVector(Vector3_0, 25f);
				}
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

	public void SetDefault(bool max)
	{
		if (max)
		{
			if (Data.CurPatrolPoint != null)
			{
				Int_1 = Data.CurPatrolPoint.TargetPoint.LookIndexes;
			}
		}
		else
		{
			Int_1 = 1;
		}
		Float_0 = 0f;
	}

	public void SetComeToPoint()
	{
		Float_1 = (GClass856.IsTrue100(50f) ? 1f : 0f);
	}

	public float method_0(PatrolPoint point)
	{
		float num = BotOwner_0.Settings.FileSettings.Patrol.LOOK_TIME_BASE;
		if (point.PatrolWay.PatrolType == PatrolType.reserved)
		{
			num = point.ActionData.TimeToUse(BotOwner_0);
		}
		return Time.time + num;
	}

	public Vector3 method_1(Vector3 dir, Vector3 headPos)
	{
		dir.y = 0f;
		int num = 0;
		Vector3 vector;
		while (true)
		{
			if (num < 8)
			{
				int num2 = shuffledDirections[num];
				vector = GClass855.RotateOnAngUp(dir, num2 * 360 / 8);
				if (GClass369.TestDir(headPos, vector, LocalBotSettingsProviderClass.Core.PATROL_MIN_LIGHT_DIST))
				{
					break;
				}
				num++;
				continue;
			}
			return Vector3.one;
		}
		return vector;
	}
}

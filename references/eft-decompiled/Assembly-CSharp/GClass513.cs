using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass513 : GClass510
{
	[Serializable]
	[CompilerGenerated]
	public class Class259
	{
		public static readonly Class259 class259_0 = new Class259();

		public static Action action_0;

		public void method_0()
		{
			Debug.LogError("can' find place for bot to stay");
		}
	}

	[NonSerialized]
	public const float Float_2 = 4f;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public IBossLogic IBossLogic;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	public GClass513(BotOwner owner, PatrollingData data, IBossLogic bossLogic)
		: base(owner, data)
	{
		IBossLogic = bossLogic;
		IBossLogic.OnPositionsRecalculated += method_1;
		List<BotOwner> followers = owner.Boss.Followers;
		owner.Boss.OnFollowerStatusChange += method_3;
		foreach (BotOwner item in followers)
		{
			method_2(item);
		}
	}

	public Vector3 GetLookToPoint()
	{
		return IBossLogic.GetTargetToLook();
	}

	public override void Update()
	{
		BotOwner_0.Mover.SetTargetMoveSpeed(1f);
		bool val = true;
		if (CustomNavigationPoint_0 != null)
		{
			Vector3 firePosition = CustomNavigationPoint_0.FirePosition;
			val = !GClass510.IsCome(BotOwner_0, firePosition, extraTarget: false, out var dist) && dist > 4f;
		}
		BotOwner_0.Sprint(val);
		if (BotOwner_0.Memory.IsInCover)
		{
			BotOwner_0.SetPose(0.1f);
			BotOwner_0.StopMove();
			BotOwner_0.LookData.SetLookPointByHearing(CustomNavigationPoint_0);
		}
		else
		{
			if (!IBossLogic.PointForBoss.HasValue)
			{
				return;
			}
			bool useFightLogic = true;
			if (!(Float_3 < Time.time))
			{
				return;
			}
			Float_3 = Time.time + 20f;
			BotOwner_0.BotAttackManager.TryPointGetting(IBossLogic.PointForBoss.Value, CoverShootType.shoot, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, CoverSearchType.distToToCenter, new ShootPointClass(IBossLogic.GetTargetToLook()), delegate(CustomNavigationPoint point)
			{
				CustomNavigationPoint_0 = point;
				Vector3 firePosition2 = point.FirePosition;
				firePosition2.y = point.Position.y;
				if ((point.Position - BotOwner_0.Position).sqrMagnitude > 1f)
				{
					BotOwner_0.GoToPoint(firePosition2);
				}
				BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(point);
			}, delegate
			{
				Debug.LogError("can' find place for bot to stay");
			}, checkCurrent: false, recalcNoTimer: false, useFightLogic);
		}
	}

	public override void DrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Vector3 up = Vector3.up;
		if (IBossLogic.PointForBoss.HasValue)
		{
			Gizmos.DrawLine(BotOwner_0.Position + up, IBossLogic.PointForBoss.Value + up);
		}
		base.DrawGizmosSelected();
	}

	public void method_1()
	{
		IBossLogic.SetFollowersPositions();
		foreach (KeyValuePair<BotOwner, Vector3> item in IBossLogic.FollowersPositions())
		{
			item.Key.BotFollower.PatrolDataFollower.SetTarget(new GStruct25(item.Value), null);
		}
	}

	public void method_2(BotOwner bot)
	{
		bot.BotFollower.PatrolDataFollower.Init(PatrolFollowerType.StayAtPlace, this);
		if (BotOwner_0.PatrollingData.Way == null)
		{
			BotOwner_0.PatrollingData.PointChooser.ChooseStartWay();
		}
		BotOwner_0.PatrollingData.PointChooser.FindPointForFollower(bot, BotOwner_0.PatrollingData.Way);
	}

	public void method_3(BotOwner bot, FollowerStatusChange status)
	{
		if (status == FollowerStatusChange.Add)
		{
			method_2(bot);
		}
	}

	public override void Dispose()
	{
		BotOwner_0.Boss.OnFollowerStatusChange -= method_3;
	}

	[CompilerGenerated]
	public void method_4(CustomNavigationPoint point)
	{
		CustomNavigationPoint_0 = point;
		Vector3 firePosition = point.FirePosition;
		firePosition.y = point.Position.y;
		if ((point.Position - BotOwner_0.Position).sqrMagnitude > 1f)
		{
			BotOwner_0.GoToPoint(firePosition);
		}
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(point);
	}
}

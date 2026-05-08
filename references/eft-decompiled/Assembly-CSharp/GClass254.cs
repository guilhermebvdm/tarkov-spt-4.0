using System;
using EFT;
using UnityEngine;

public class GClass254(BotOwner bot) : GClass200<GClass26>(bot)
{
	public bool WithRestart = true;

	public BotEventDebug TargetPosition;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_0;

	public override void UpdateNodeByBrain(GClass26 data)
	{
		bool val = method_0() == DoorInteractionStatus.CanRun;
		if (TargetPosition == null)
		{
			BotEventDebug botEventDebug = UnityEngine.Object.FindObjectOfType<BotEventDebug>();
			if (botEventDebug != null)
			{
				TargetPosition = botEventDebug;
			}
			return;
		}
		method_5();
		if (!TargetPosition.CanBeUsed)
		{
			CustomNavigationPoint_0 = null;
			BotOwner_0.StopMove();
		}
		else
		{
			if (CustomNavigationPoint_0 == null)
			{
				return;
			}
			GClass369.DebugDrawArc(CustomNavigationPoint_0.Position, BotOwner_0.Position, 1f, 0.1f, Color.green);
			if (WithRestart || !BotOwner_0.Memory.IsInCover)
			{
				BotOwner_0.Sprint(val);
				if (BotOwner_0.Memory.BotCurrentCoverInfo.HaveCover && BotOwner_0.Mover.IsComeTo(-1f, onCover: true))
				{
					BotOwner_0.Sprint(val: false, withDebugCallback: false);
					BotOwner_0.Memory.ComeToPoint();
					BotOwner_0.StopMove();
				}
				else
				{
					BotOwner_0.GoToPoint(CustomNavigationPoint_0.Position, slowAtTheEnd: false, -1f, getUpWithCheck: false, mustHaveWay: false);
				}
			}
		}
	}

	public void method_5()
	{
		if (!(Float_0 > Time.time))
		{
			Float_0 = Time.time + 3f;
			CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position);
			CustomNavigationPoint_0 = closestPoint;
		}
	}

	public void method_6()
	{
		BotOwner_0.SetPose(1f);
		BotOwner_0.StopMove();
	}
}

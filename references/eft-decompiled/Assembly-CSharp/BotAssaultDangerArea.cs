using System;
using EFT;
using UnityEngine;

public class BotAssaultDangerArea : GClass429
{
	[NonSerialized]
	public const float DIST_TO_COME_CLOSE = 20f;

	[NonSerialized]
	public CustomNavigationPoint PointToBeClose;

	[NonSerialized]
	public float LastEndHoldPos;

	[NonSerialized]
	public float LastCHeckHoldPoint;

	[field: NonSerialized]
	public MiniAssaultBotsGroup AssaultGroup { get; set; }

	public bool HaveTarget => AssaultGroup != null;

	public EBotAssaultAreaStatus ShallAssault => AssaultGroup.Status;

	public BotAssaultDangerArea(BotOwner owner)
		: base(owner)
	{
	}

	public void AreaCompleted()
	{
		AssaultGroup = null;
	}

	public void FindCover()
	{
		Vector3 vector = GClass855.NormalizeFastSelf(AssaultGroup.AreaToAssaut.Point - BotOwner_0.Position);
		Vector3 pos = AssaultGroup.AreaToAssaut.Point - vector * 20f;
		CustomNavigationPoint closestPoint = BotOwner_0.Covers.GetClosestPoint(pos);
		PointToBeClose = closestPoint;
	}

	public void Activate()
	{
		AssaultGroup = null;
	}

	public void SetActivaArea(MiniAssaultBotsGroup botZoneDangerArea)
	{
		AssaultGroup = botZoneDangerArea;
	}

	public bool ShallEndHoldPosition()
	{
		if (LastEndHoldPos < Time.time)
		{
			LastEndHoldPos = Time.time + 3f;
			return true;
		}
		return false;
	}

	public CustomNavigationPoint GetCloseCover()
	{
		if (PointToBeClose == null && LastCHeckHoldPoint < Time.time)
		{
			LastCHeckHoldPoint = Time.time + 3f;
			FindCover();
		}
		return PointToBeClose;
	}
}

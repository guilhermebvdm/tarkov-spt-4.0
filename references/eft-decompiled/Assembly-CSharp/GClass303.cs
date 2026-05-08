using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass303 : GClass177<GClass26>
{
	[Serializable]
	[CompilerGenerated]
	public class Class121
	{
		public static readonly Class121 class121_0 = new Class121();

		public static Func<GroupPoint, bool> func_0;

		public bool method_0(GroupPoint x)
		{
			if (x.PointWithNeighborType == PointWithNeighborType.cover)
			{
				return x.CoverLevel == CoverLevel.Stay;
			}
			return false;
		}
	}

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_0;

	public GClass303(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return;
		}
		if (CustomNavigationPoint_0 == null)
		{
			method_0();
			return;
		}
		BotOwner_0.GoToPoint(CustomNavigationPoint_0);
		if ((BotOwner_0.Destination - BotOwner_0.Transform.position).Value.magnitude < BotOwner_0.Settings.FileSettings.Move.REACH_DIST)
		{
			BotOwner_0.StopMove();
			BotOwner_0.Memory.ComeToPoint();
		}
	}

	public void method_0()
	{
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 2f;
			CustomNavigationPoint_0 = BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position, (GroupPoint x) => x.PointWithNeighborType == PointWithNeighborType.cover && x.CoverLevel == CoverLevel.Stay);
		}
	}
}

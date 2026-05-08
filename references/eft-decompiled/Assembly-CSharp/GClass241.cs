using System;
using EFT;
using UnityEngine;

public class GClass241 : GClass236
{
	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_1;

	[NonSerialized]
	public float Float_14;

	public GClass241(BotOwner bot)
		: base(bot, 80f, 60f)
	{
		BotOwner_0.Memory.OnInCoverChange += method_18;
	}

	public void method_18(bool arg1, CustomNavigationPoint arg2)
	{
		if (arg1 && CustomNavigationPoint_1 != null && arg2.Id == CustomNavigationPoint_1.Id)
		{
			CustomNavigationPoint_1 = null;
			method_19();
		}
	}

	public override void MoverToMainTarget(CustomNavigationPoint navigationPoint)
	{
		navigationPoint = method_19();
		base.MoverToMainTarget(navigationPoint);
	}

	public override void LookSimple()
	{
		BotOwner_0.LookData.SetLookPointByHearing();
	}

	public CustomNavigationPoint method_19()
	{
		if (CustomNavigationPoint_1 == null || Time.time - Float_14 > 150f || ((CustomNavigationPoint_1.Position - BotOwner_0.Position).sqrMagnitude < 2f && Time.time - BotOwner_0.Memory.ComeToCoverTime > 5f))
		{
			Vector3 v = new Vector3(GClass856.Random(-1f, 1f), 0f, GClass856.Random(-1f, 1f));
			v = GClass855.NormalizeFastSelf(v);
			Vector3 pos = BotOwner_0.Position + v * 100f;
			CustomNavigationPoint_1 = BotOwner_0.Covers.GetClosestPoint(pos, method_20);
			_ = (BotOwner_0.Position - CustomNavigationPoint_1.Position).magnitude;
			Float_14 = Time.time;
		}
		return CustomNavigationPoint_1;
	}

	public bool method_20(GroupPoint arg)
	{
		if ((BotOwner_0.Position - arg.Position).sqrMagnitude < 400f)
		{
			return false;
		}
		return true;
	}

	public override void Dispose()
	{
		BotOwner_0.Memory.OnInCoverChange -= method_18;
		base.Dispose();
	}
}

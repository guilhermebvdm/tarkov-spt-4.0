using System;
using System.Collections.Generic;
using EFT;

public abstract class BaseBrain : global::AICoreStrategyAbstractClass<BotLogicDecision>
{
	public const int DEBUG_LOGIC = 1000;

	[NonSerialized]
	public BotOwner Owner;

	[NonSerialized]
	public GClass71 Debug;

	public global::AICoreLayerClass<BotLogicDecision> CurLayerInfo => base.GClass35_0;

	public BaseBrain(BotOwner owner)
	{
		Owner = owner;
		bool flag = DebugBotData.UseDebugData && DebugBotData.Instance.DebugBrain;
		Debug = new GClass71(owner, 1000);
		method_0(1000, Debug, flag);
		if (flag)
		{
			ActivateDebug(DebugBotData.Instance.DebugBotDesition);
		}
		method_5(owner);
	}

	public void ActivateDebug(DebugBotDesition logic)
	{
		if (HaveLayer<GClass71>(out var layerTarget))
		{
			if (!method_2(layerTarget))
			{
				method_1(1000);
			}
			layerTarget.Desition = logic;
		}
	}

	public void DeactivateDebug()
	{
		if (HaveLayer<GClass71>())
		{
			method_3(1000);
			Debug.Desition = DebugBotDesition.none;
		}
	}

	public CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> getCoverPointMain2, bool checkCurrent)
	{
		if (!(base.GClass35_0 is BaseLogicLayerSimpleAbstractClass baseLogicLayerSimpleAbstractClass))
		{
			return data.Bot.Covers.FindClosestPoint(data.CenterPos);
		}
		return baseLogicLayerSimpleAbstractClass.FindPoint(data, getCoverPointMain2, checkCurrent);
	}

	public void ActivateLayers(List<int> layers)
	{
		foreach (int layer in layers)
		{
			method_1(layer);
		}
	}

	public void CalcActionNextFrame()
	{
		(base.GClass35_0 as BaseLogicLayerSimpleAbstractClass)?.CalcActionNextFrame();
	}

	public string DebugHandler()
	{
		return ToString();
	}

	public abstract string ShortName();

	public abstract GClass671 EventsPriority();

	public void method_5(BotOwner owner)
	{
		GClass671 gClass = EventsPriority();
		if (gClass != null)
		{
			if (gClass.Khorovod > 0)
			{
				GClass73 layer = new GClass73(owner, gClass.Khorovod);
				method_0(503, layer, activeOnStart: false);
			}
			if (gClass.ForceAttack > 0)
			{
				GClass62 layer2 = new GClass62(owner, gClass.ForceAttack);
				method_0(501, layer2, activeOnStart: false);
			}
			if (gClass.ForcePersuit > 0)
			{
				Class105 layer3 = new Class105(owner, gClass.ForcePersuit);
				method_0(502, layer3, activeOnStart: false);
			}
			if (gClass.FollowPlayer > 0)
			{
				GClass72 layer4 = new GClass72(owner, gClass.FollowPlayer);
				method_0(504, layer4, activeOnStart: false);
			}
			if (gClass.HalloweenHide > 0)
			{
				GClass61 layer5 = new GClass61(owner, gClass.HalloweenHide);
				method_0(506, layer5, activeOnStart: true);
			}
			if (gClass.GoToGenerator > 0)
			{
				GClass136 layer6 = new GClass136(owner, gClass.GoToGenerator, tryFinGreenFirst: false, CoverLevel.Lay, usePeriodForceReched: true);
				method_0(507, layer6, activeOnStart: false);
			}
		}
	}
}

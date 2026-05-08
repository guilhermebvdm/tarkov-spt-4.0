using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public abstract class GClass1320 : CurrentStateAbstractClass
{
	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public FastAnimatorProcessorClass.GClass1342 Gclass1342_0;

	[NonSerialized]
	public GClass1448 Gclass1448_0;

	public override float RealMotionDuration => Gclass1448_0?.ComputeClipTime(Gclass1342_0) ?? 0f;

	public override bool Loop => Gclass1448_0?.IsLooping ?? false;

	public GClass1320(string name, int hash, float speedMultiplier, SpeedAnimParamClass speedAnimParam, List<TransitionClass> outTransitions)
		: base(name, hash, outTransitions, speedMultiplier, speedAnimParam)
	{
	}

	public GClass1320(string name, int hash, float speedMultiplier, SpeedAnimParamClass speedAnimParam)
		: base(name, hash, speedMultiplier, speedAnimParam)
	{
	}

	public void InitParametersCacheAndRootMotionTable(GInterface134 parametersCache, GClass1448 computedNode)
	{
		Bool_4 = true;
		Gclass1342_0 = parametersCache as FastAnimatorProcessorClass.GClass1342;
		Gclass1448_0 = computedNode;
		GClass1448.ProcessParameterRec(Gclass1448_0, delegate(int parameterHash)
		{
			Gclass1342_0.AddDependencyToParameter(parameterHash, this);
		});
	}

	public override void Dispose(bool disposing)
	{
		if (disposing && Gclass1448_0 != null)
		{
			GClass1448.ProcessParameterRec(Gclass1448_0, delegate(int parameterHash)
			{
				Gclass1342_0.RemoveDependencyFromParameter(parameterHash, this);
			});
		}
		base.Dispose(disposing);
	}

	[CompilerGenerated]
	public void method_1(int parameterHash)
	{
		Gclass1342_0.AddDependencyToParameter(parameterHash, this);
	}

	[CompilerGenerated]
	public void method_2(int parameterHash)
	{
		Gclass1342_0.RemoveDependencyFromParameter(parameterHash, this);
	}
}

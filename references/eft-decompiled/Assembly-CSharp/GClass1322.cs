using System;
using System.Collections.Generic;

public class GClass1322 : CurrentStateAbstractClass
{
	public readonly GClass1326 ClipInfo;

	[NonSerialized]
	public bool Bool_4;

	public override float RealMotionDuration => ClipInfo.Duration;

	public override bool Loop => Bool_4;

	public GClass1322(string name, int hash, List<TransitionClass> outTransitions, float speedMultiplier, SpeedAnimParamClass speedAnimParam, GClass1326 clipInfo)
		: base(name, hash, outTransitions, speedMultiplier, speedAnimParam)
	{
		ClipInfo = clipInfo;
		Bool_4 = clipInfo.IsLooped;
	}

	public GClass1322(string name, int hash, float speedMultiplier, SpeedAnimParamClass speedAnimParam, GClass1326 clipInfo)
		: base(name, hash, speedMultiplier, speedAnimParam)
	{
		ClipInfo = clipInfo;
		Bool_4 = clipInfo.IsLooped;
	}

	public override string ToString()
	{
		return $"{base.ToString()}, ClipInfo: [{ClipInfo}]";
	}
}

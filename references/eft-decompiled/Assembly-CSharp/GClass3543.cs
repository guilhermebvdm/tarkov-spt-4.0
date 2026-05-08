using System;
using System.Runtime.CompilerServices;

public class GClass3543 : GClass3542
{
	[NonSerialized]
	[CompilerGenerated]
	public RainController.ERainIntensity ErainIntensity_0;

	public RainController.ERainIntensity RainIntensity
	{
		[CompilerGenerated]
		get
		{
			return ErainIntensity_0;
		}
		[CompilerGenerated]
		set
		{
			ErainIntensity_0 = value;
		}
	}

	public void Invoke(RainController.ERainIntensity rainIntensity)
	{
		RainIntensity = rainIntensity;
		base.Invoke();
	}

	public override void Reset()
	{
		RainIntensity = RainController.ERainIntensity.None;
	}
}

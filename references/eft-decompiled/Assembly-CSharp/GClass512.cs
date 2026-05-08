using EFT;
using UnityEngine;

public class GClass512 : GClass511
{
	public GClass512(BotOwner owner, PatrollingData data)
		: base(owner, data)
	{
	}

	public override void Update()
	{
		method_2(method_6);
	}

	public void method_6()
	{
		if (method_0())
		{
			PatrollingData_0.ComeToPoint();
		}
		else if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 2.5f;
			PatrollingData_0.PatrolPathControl.RefreshGoToPoint();
		}
	}
}

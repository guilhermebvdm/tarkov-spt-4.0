using System;
using EFT;
using UnityEngine;

public class GClass190 : GClass177<GClass26>
{
	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public GClass275 Gclass275_0;

	public GClass190(BotOwner bot)
		: base(bot)
	{
		Gclass275_0 = new GClass275(bot);
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (Bool_0)
		{
			if (Float_0 < Time.time)
			{
				Bool_0 = false;
				Gclass275_0.UpdateNodeByBrain(data);
			}
			return;
		}
		BotOwner_0.SetPose(1f);
		Vector3_0 = BotOwner_0.DeadBodyData.GetPointToShoot();
		IBotAiming currentAiming = BotOwner_0.AimingManager.CurrentAiming;
		currentAiming.SetTarget(Vector3_0);
		currentAiming.NodeUpdate();
		if (currentAiming.IsReady)
		{
			Float_0 = Time.time + 1.7f;
			Bool_0 = true;
		}
	}
}

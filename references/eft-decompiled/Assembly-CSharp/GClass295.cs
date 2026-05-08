using System;
using EFT;
using UnityEngine;

public class GClass295 : GClass177<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	public GClass295(BotOwner bot)
		: base(bot)
	{
	}

	public override void Awake()
	{
		Float_0 = Time.time + 20f;
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 10f;
			if (Bool_0)
			{
				Debug.LogError("ChangeToMain");
				BotOwner_0.WeaponManager.Selector.ChangeToMain();
			}
			else
			{
				Debug.LogError("Melee");
				BotOwner_0.WeaponManager.Selector.ChangeToMelee();
			}
			Bool_0 = !Bool_0;
		}
		BotOwner_0.StopMove();
	}
}

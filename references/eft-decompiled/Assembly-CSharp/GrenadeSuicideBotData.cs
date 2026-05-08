using System;
using EFT;
using UnityEngine;

public class GrenadeSuicideBotData : GClass429
{
	[NonSerialized]
	public AIGreanageThrowData Data;

	[NonSerialized]
	public float LastSetData;

	[field: NonSerialized]
	public bool Stay { get; set; }

	public GrenadeSuicideBotData(BotOwner owner)
		: base(owner)
	{
	}

	public void UpdateByNode()
	{
		if (Data == null || Time.time - LastSetData > 20f)
		{
			Data = GClass577.GetSuicideData(BotOwner_0.Position, BotOwner_0.LookDirection);
			BotOwner_0.WeaponManager.Grenades.SetThrowData(Data);
			if (BotOwner_0.WeaponManager.Grenades.GetGrenadeCount() <= 1)
			{
				Stay = true;
				BotOwner_0.BewareGrenade.IgnoreGrenadesForSec(20f);
			}
		}
		BotOwner_0.WeaponManager.Grenades.DoThrow();
	}
}

using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class BotNeutralsCheskData : GClass429
{
	[NonSerialized]
	public IPlayer ClosestsVisible_1;

	[NonSerialized]
	public float PeriodUpdate;

	[NonSerialized]
	public float SettedTime;

	[NonSerialized]
	public const float PERIOD_CHECK = 5f;

	public IPlayer ClosestsVisible => ClosestsVisible_1;

	public BotNeutralsCheskData(BotOwner owner)
		: base(owner)
	{
	}

	public void ManualUpdate()
	{
		if (PeriodUpdate > Time.time)
		{
			return;
		}
		PeriodUpdate = Time.time + 5f;
		IPlayer player = null;
		float num = float.MaxValue;
		foreach (KeyValuePair<IPlayer, BotSettingsClass> neutral in BotOwner_0.BotsGroup.Neutrals)
		{
			float sqrMagnitude = (neutral.Key.Position - BotOwner_0.Position).sqrMagnitude;
			if (sqrMagnitude < 10000f && sqrMagnitude < num && CanSeeNeautral(neutral.Key))
			{
				player = neutral.Key;
			}
		}
		method_0(player);
	}

	public bool CanSeeNeautral(IPlayer player)
	{
		Vector3 position = BotOwner_0.PlayerBones.Head.position;
		return Physics.Linecast(player.PlayerBones.Head.position, position);
	}

	public void method_0(IPlayer player)
	{
		if (ClosestsVisible_1 != null)
		{
			ClosestsVisible_1.OnIPlayerDeadOrUnspawn -= method_1;
		}
		ClosestsVisible_1 = player;
		if (ClosestsVisible_1 != null)
		{
			SettedTime = Time.time;
			ClosestsVisible_1.OnIPlayerDeadOrUnspawn += method_1;
		}
	}

	public void method_1(IPlayer obj)
	{
		method_0(null);
	}

	public void Dispose()
	{
		if (ClosestsVisible_1 != null)
		{
			ClosestsVisible_1.OnIPlayerDeadOrUnspawn -= method_1;
		}
	}

	public bool IsInPeriod()
	{
		return Time.time - SettedTime < 5.5f;
	}
}

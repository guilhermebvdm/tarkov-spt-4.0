using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotFriendlyTilt(BotOwner owner) : GClass429(owner)
{
	public class Class261
	{
		public int Times;

		public float LastTime;
	}

	[NonSerialized]
	public const float CHANCE_MUMBLE_TILT = 111f;

	[NonSerialized]
	public const float PLAYER_ANSWER_PERIOD_AFTER_MAX = 6f;

	[NonSerialized]
	public const int PLAYER_ANSWER_TIMES_MAX = 6;

	[NonSerialized]
	public const float SIDE_TILT_PERIOD = 0.5f;

	[NonSerialized]
	public const int TILT_LENGHT = 4;

	[NonSerialized]
	public float[] Times = new float[4];

	[NonSerialized]
	public bool PrevTiltSide;

	[NonSerialized]
	public int TiltIndex;

	[NonSerialized]
	public bool Tilting;

	[NonSerialized]
	public IPlayer LookTo;

	[NonSerialized]
	public Dictionary<IPlayer, Class261> TimesRequests = new Dictionary<IPlayer, Class261>();

	public void StartTilt(GClass413 closest)
	{
		StartTilt(closest.GetAnother(BotOwner_0));
	}

	public void StartTilt(IPlayer target)
	{
		if (!BotOwner_0.Settings.FileSettings.Patrol.CAN_FRIENDLY_TILT)
		{
			return;
		}
		method_3();
		LookTo = target;
		if (LookTo != null)
		{
			Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(LookTo.ProfileId);
			if (alivePlayerByProfileID != null)
			{
				alivePlayerByProfileID.OnPlayerDeadOrUnspawn += method_0;
			}
		}
		TiltIndex = 0;
		Tilting = true;
		for (int i = 0; i < 4; i++)
		{
			Times[i] = Time.time + 0.5f * (float)i;
		}
	}

	public void Activate()
	{
		BotOwner_0.Memory.OnPeaceChange += method_1;
	}

	public void ManualUpdate()
	{
		if (!Tilting)
		{
			return;
		}
		try
		{
			Vector3 point = LookTo.Position + Vector3.up * 1.5f;
			BotOwner_0.Steering.LookToPoint(point);
			BotOwner_0.StopMove();
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("{0} - cant process {1}, probably _lookTo [{2}] is unspaned :\n{3}", BotOwner_0.ProfileId, "BotFriendlyTilt", LookTo.ProfileId, ex);
			Tilting = false;
			throw;
		}
		if (Times[TiltIndex] < Time.time)
		{
			TiltIndex++;
			if (TiltIndex >= Times.Length)
			{
				EndTilt();
				return;
			}
			PrevTiltSide = !PrevTiltSide;
			BotOwner_0.Tilt.Set((!PrevTiltSide) ? BotTiltType.right : BotTiltType.left);
		}
	}

	public void EndTilt()
	{
		BotOwner_0.Tilt.Stop();
		method_3();
		LookTo = null;
		Tilting = false;
	}

	public bool HaveActions()
	{
		return Tilting;
	}

	public void TryAddRequest(IPlayer player)
	{
		if (!TimesRequests.TryGetValue(player, out var value) || value.Times <= 6 || !(Time.time - value.LastTime < 6f))
		{
			float sqrMagnitude = (BotOwner_0.Transform.position - player.Transform.position).sqrMagnitude;
			bool flag = BotReceiver.CanReceiveFromPoint(player.Transform.position, BotOwner_0.Transform.position, BotOwner_0.LookDirection, BotOwner_0.LookSensor.VISIBLE_ANGLE);
			if (sqrMagnitude < LocalBotSettingsProviderClass.Core.GESTUS_DIST_ANSWERS_SQRT && flag)
			{
				method_2(player);
			}
			if (flag && GClass856.IsTrue100(111f))
			{
				BotOwner_0.BotTalk.TrySay(EPhraseTrigger.OnMutter, withGroupDelay: true);
			}
		}
	}

	public void method_0(Player player)
	{
		EndTilt();
	}

	public void method_1(bool isPeace)
	{
		if (!isPeace)
		{
			EndTilt();
		}
	}

	public void method_2(IPlayer player)
	{
		if (!TimesRequests.TryGetValue(player, out var value))
		{
			value = new Class261();
			TimesRequests.Add(player, value);
		}
		value.Times++;
		value.LastTime = Time.time;
		StartTilt(player);
	}

	public void method_3()
	{
		if (LookTo != null)
		{
			Player everExistedPlayerByID = Singleton<GameWorld>.Instance.GetEverExistedPlayerByID(LookTo.ProfileId);
			if (everExistedPlayerByID != null)
			{
				everExistedPlayerByID.OnPlayerDeadOrUnspawn -= method_0;
			}
		}
	}

	public void Dispose()
	{
		if (BotOwner_0.Memory != null)
		{
			method_3();
			BotOwner_0.Memory.OnPeaceChange -= method_1;
		}
	}
}

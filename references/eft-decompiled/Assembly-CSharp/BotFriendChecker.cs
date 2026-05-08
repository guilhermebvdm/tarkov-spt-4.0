using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class BotFriendChecker : GClass429
{
	[NonSerialized]
	public const float DIST_TO_AIM = 50f;

	[NonSerialized]
	public const float DIST_TO_LOOK = 50f;

	[NonSerialized]
	public const float DIST_TO_EAT = 50f;

	[NonSerialized]
	public const float DIST_TO_GESTUS = 20f;

	[NonSerialized]
	public const float DIST_TO_TILT = 50f;

	[NonSerialized]
	public const float DIST_TO_WATCH_WEAPON = 50f;

	[NonSerialized]
	public float DistToBeClose = Mathf.Max(20f, 50f, 50f, 50f);

	[NonSerialized]
	public GClass412 BotsConnections;

	[NonSerialized]
	public float NextTimeCheck;

	[NonSerialized]
	public GClass1881<EBotFriendAction> ChancesNoEat;

	[NonSerialized]
	public GClass1881<EBotFriendAction> ChancesTotal;

	public BotFriendChecker(BotOwner owner, GClass412 botsConnections)
		: base(owner)
	{
		BotsConnections = botsConnections;
		ChancesNoEat = new GClass1881<EBotFriendAction>(new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.Tilt, 1f), new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.Aiming, 1f), new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.Sign, 1f), new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.Look, 1f), new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.WatchSecondWeapon, 1f), new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.DoNothing, 1f));
		ChancesTotal = new GClass1881<EBotFriendAction>(new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.Eat, 1f), new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.Aiming, 1f), new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.Tilt, 1f), new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.Sign, 1f), new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.Look, 1f), new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.WatchSecondWeapon, 1f), new KeyValuePair<EBotFriendAction, float>(EBotFriendAction.DoNothing, 1f));
	}

	public void ManualUpdate()
	{
		if (!(NextTimeCheck < Time.time))
		{
			return;
		}
		NextTimeCheck = Time.time + 1f;
		GClass413 closest = BotsConnections.GetClosest(BotOwner_0);
		if (closest == null || !(closest.Dist < DistToBeClose) || !closest.CanDoSign())
		{
			return;
		}
		Vector3 vector = Vector3.up * 1.5f;
		Vector3 start = closest.Person1.Position + vector;
		Vector3 end = closest.Person2.Position + vector;
		if (Physics.Linecast(start, end, LayerMaskClass.HighPolyWithTerrainMaskAI))
		{
			return;
		}
		EBotFriendAction eBotFriendAction = (BotOwner_0.EatDrinkData.HaveEat() ? ChancesTotal : ChancesNoEat).Random();
		if (closest.Dist > 1f)
		{
			switch (eBotFriendAction)
			{
			case EBotFriendAction.Tilt:
				if (closest.Dist < 50f)
				{
					BotOwner_0.FriendlyTilt.StartTilt(closest);
				}
				break;
			case EBotFriendAction.Sign:
				if (closest.Dist < 20f)
				{
					BotOwner_0.PeacefulActions.StartDo(closest);
				}
				break;
			case EBotFriendAction.Eat:
				if (closest.Dist < 50f)
				{
					BotOwner_0.EatDrinkData.StartDo(closest);
				}
				break;
			case EBotFriendAction.Aiming:
				if (closest.Dist < 50f)
				{
					BotOwner_0.PeaceHardAim.StartAim(closest);
				}
				break;
			case EBotFriendAction.Look:
				if (closest.Dist < 50f)
				{
					BotOwner_0.PeaceLook.StartLook(closest);
				}
				break;
			case EBotFriendAction.WatchSecondWeapon:
				if (closest.Dist < 50f)
				{
					BotOwner_0.SecondWeaponData.StartDo(closest);
				}
				break;
			}
		}
		NextTimeCheck = Time.time + LocalBotSettingsProviderClass.Core.PERIOD_NEXT_HELLO_ACTION_SEC;
	}
}

using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class BotTalk : GClass429
{
	[NonSerialized]
	public const float QUERY_WAIT_TIME = 0.15f;

	[NonSerialized]
	public float NextCanSayTime;

	[NonSerialized]
	public bool HaveSomeThingToSay;

	[NonSerialized]
	public bool CanSay;

	[NonSerialized]
	public float EndWaitQueryTime;

	[NonSerialized]
	public GClass535 TryToTalk;

	[NonSerialized]
	public Dictionary<EPhraseTrigger, float> Priority = new Dictionary<EPhraseTrigger, float>();

	[NonSerialized]
	public bool IsSilence;

	[NonSerialized]
	public float SilenceEnds;

	public bool IsSilenced
	{
		get
		{
			if (IsSilence && SilenceEnds < Time.time)
			{
				IsSilence = false;
			}
			return IsSilence;
		}
	}

	public BotTalk(BotOwner owner)
		: base(owner)
	{
		BotOwner_0 = owner;
		method_0();
	}

	public void Say(EPhraseTrigger type, bool sayImmediately = false, ETagStatus? additionalMask = null)
	{
		if (CanSay)
		{
			ETagStatus mask = method_1(additionalMask);
			if (BotOwner_0.Settings.FileSettings.Mind.TALK_WITH_QUERY && !sayImmediately)
			{
				method_2(type, demand: true, 0f, mask);
			}
			else if (NextCanSayTime < Time.time)
			{
				NextCanSayTime = Time.time + GClass856.GreateRandom(LocalBotSettingsProviderClass.Core.TALK_DELAY);
				BotOwner_0.GetPlayer.Say(type, demand: true, 0f, mask);
			}
		}
	}

	public void DropNextSayPeriod()
	{
		NextCanSayTime = -1f;
	}

	public void ManualUpdate()
	{
		method_4();
	}

	public void TrySay(EPhraseTrigger type)
	{
		TrySay(type, null, BotOwner_0.Memory.HaveEnemy);
	}

	public void TrySay(EPhraseTrigger type, bool withGroupDelay)
	{
		TrySay(type, null, withGroupDelay);
	}

	public void TrySay(EPhraseTrigger type, ETagStatus? additionaMask, bool withGroupDelay)
	{
		if (CanSay && (!withGroupDelay || BotOwner_0.BotsGroup.GroupTalk.CanSay(BotOwner_0, type)))
		{
			ETagStatus mask = method_1(additionaMask);
			if (BotOwner_0.Settings.FileSettings.Mind.TALK_WITH_QUERY)
			{
				method_2(type, demand: true, 0f, mask);
			}
			else if (NextCanSayTime < Time.time)
			{
				method_5(type);
			}
		}
	}

	public void Activate()
	{
		CanSay = BotOwner_0.Settings.FileSettings.Mind.CAN_TALK;
	}

	public void method_0()
	{
		Priority.Add(EPhraseTrigger.OnDeath, 199f);
		Priority.Add(EPhraseTrigger.OnAgony, 198f);
		Priority.Add(EPhraseTrigger.Stop, 197f);
		Priority.Add(EPhraseTrigger.FriendlyFire, 176f);
		Priority.Add(EPhraseTrigger.OnFirstContact, 96f);
		Priority.Add(EPhraseTrigger.OnRepeatedContact, 95f);
		Priority.Add(EPhraseTrigger.HurtHeavy, 94f);
		Priority.Add(EPhraseTrigger.OnEnemyGrenade, 93f);
		Priority.Add(EPhraseTrigger.LegBroken, 92f);
		Priority.Add(EPhraseTrigger.HandBroken, 91f);
		Priority.Add(EPhraseTrigger.HurtNearDeath, 90f);
		Priority.Add(EPhraseTrigger.GetInCover, 89f);
		Priority.Add(EPhraseTrigger.OnBeingHurt, 88f);
		Priority.Add(EPhraseTrigger.OnBeingHurtDissapoinment, 87f);
		Priority.Add(EPhraseTrigger.OnGrenade, 86f);
		Priority.Add(EPhraseTrigger.OnSix, 85f);
		Priority.Add(EPhraseTrigger.OnLostVisual, 84f);
		Priority.Add(EPhraseTrigger.OnWeaponReload, 83f);
		Priority.Add(EPhraseTrigger.OnEnemyShot, 82f);
		Priority.Add(EPhraseTrigger.OnOutOfAmmo, 80f);
		Priority.Add(EPhraseTrigger.OnFight, 78f);
		Priority.Add(EPhraseTrigger.Roger, 74f);
		Priority.Add(EPhraseTrigger.Negative, 70f);
		Priority.Add(EPhraseTrigger.OnFriendlyDown, 66f);
		Priority.Add(EPhraseTrigger.OnGoodWork, 62f);
		Priority.Add(EPhraseTrigger.LootBody, 58f);
		Priority.Add(EPhraseTrigger.OnEnemyConversation, 54f);
		Priority.Add(EPhraseTrigger.OnEnemyDown, 50f);
		Priority.Add(EPhraseTrigger.OnBreath, 46f);
		Priority.Add(EPhraseTrigger.OnMutter, 42f);
		Priority.Add(EPhraseTrigger.EnemyHit, 38f);
		Priority.Add(EPhraseTrigger.WeaponBroken, 155f);
		Priority.Add(EPhraseTrigger.NeedWeapon, 156f);
	}

	public ETagStatus method_1(ETagStatus? additionaMask = null)
	{
		ETagStatus eTagStatus = ((BotOwner_0.BotsGroup.MembersCount <= 1) ? ETagStatus.Solo : ETagStatus.Coop);
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			eTagStatus = ((!BotOwner_0.Memory.GoalTarget.HavePlaceTarget()) ? (eTagStatus | ETagStatus.Unaware) : (eTagStatus | ETagStatus.Aware));
		}
		else
		{
			eTagStatus |= ETagStatus.Combat;
			switch (BotOwner_0.Memory.GoalEnemy.Person.Side)
			{
			case EPlayerSide.Usec:
				eTagStatus |= ETagStatus.Usec;
				break;
			case EPlayerSide.Bear:
				eTagStatus |= ETagStatus.Bear;
				break;
			case EPlayerSide.Savage:
				eTagStatus |= ETagStatus.Scav;
				break;
			}
		}
		if (additionaMask.HasValue)
		{
			eTagStatus |= additionaMask.Value;
		}
		return eTagStatus;
	}

	public void method_2(EPhraseTrigger type, bool demand, float delay, ETagStatus mask)
	{
		GClass535 data = new GClass535(type, demand, delay, mask);
		TryToTalk = method_3(data, TryToTalk);
		if (!HaveSomeThingToSay)
		{
			HaveSomeThingToSay = true;
			EndWaitQueryTime = Time.time + 0.15f;
		}
	}

	public GClass535 method_3(GClass535 data, GClass535 tryToTalk)
	{
		if (tryToTalk == null)
		{
			return data;
		}
		if (data == null)
		{
			return tryToTalk;
		}
		float num = (Priority.ContainsKey(data.type) ? Priority[data.type] : 0f);
		float num2 = (Priority.ContainsKey(tryToTalk.type) ? Priority[tryToTalk.type] : 0f);
		if (num > num2)
		{
			return data;
		}
		return tryToTalk;
	}

	public void method_4()
	{
		if (!CanSay || !HaveSomeThingToSay || !(EndWaitQueryTime < Time.time) || TryToTalk == null)
		{
			return;
		}
		HaveSomeThingToSay = false;
		if (NextCanSayTime < Time.time)
		{
			if (BotOwner_0.BotsGroup.GroupTalk.CanSay(BotOwner_0, TryToTalk.type))
			{
				BotOwner_0.BotsGroup.GroupTalk.PhraseSad(BotOwner_0, TryToTalk.type);
				BotOwner_0.GetPlayer.Say(TryToTalk.type, TryToTalk.demand, TryToTalk.delay, TryToTalk.mask);
			}
			NextCanSayTime = Time.time + GClass856.GreateRandom(LocalBotSettingsProviderClass.Core.TALK_DELAY);
		}
		TryToTalk = null;
	}

	public void method_5(EPhraseTrigger type)
	{
		NextCanSayTime = Time.time + GClass856.GreateRandom(LocalBotSettingsProviderClass.Core.TALK_DELAY);
		Say(type);
	}

	public void SetSilence(float period)
	{
		SilenceEnds = Time.time + period;
		IsSilence = true;
	}
}

using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class BotGroupTalk
{
	[NonSerialized]
	public float GroupAnyPhraseDelay = -1f;

	[NonSerialized]
	public float GroupExactlyPhraseDelay = -1f;

	[NonSerialized]
	public Dictionary<EPhraseTrigger, float> NextPossibleTalkTTime = new Dictionary<EPhraseTrigger, float>();

	[NonSerialized]
	public float TotalNextTime;

	public BotGroupTalk()
	{
		EPhraseTrigger[] array = (EPhraseTrigger[])Enum.GetValues(typeof(EPhraseTrigger));
		foreach (EPhraseTrigger key in array)
		{
			NextPossibleTalkTTime.Add(key, 0f);
		}
	}

	public void PhraseSad(BotOwner bot, EPhraseTrigger ePhrase)
	{
		GroupAnyPhraseDelay = bot.Settings.FileSettings.Mind.GROUP_ANY_PHRASE_DELAY;
		GroupExactlyPhraseDelay = GClass856.Random(bot.Settings.FileSettings.Mind.GROUP_EXACTLY_PHRASE_DELAY, bot.Settings.FileSettings.Mind.GROUP_EXACTLY_PHRASE_DELAY_MAX);
		TotalNextTime = Time.time + GroupAnyPhraseDelay;
		NextPossibleTalkTTime[ePhrase] = Time.time + GroupExactlyPhraseDelay;
	}

	public bool CanSay(BotOwner bot, EPhraseTrigger ePhrase)
	{
		if (TotalNextTime > Time.time)
		{
			return false;
		}
		if (NextPossibleTalkTTime.TryGetValue(ePhrase, out var value) && value > Time.time)
		{
			return false;
		}
		return true;
	}
}

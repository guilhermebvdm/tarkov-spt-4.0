using System;
using System.Collections.Generic;
using EFT;

public class BotLoyaltyData(BotOwner owner) : GClass429(owner)
{
	[NonSerialized]
	public Dictionary<IPlayer, bool> ShallApplyFollowRequestByLoyalty = new Dictionary<IPlayer, bool>();

	public void Activate()
	{
		BotOwner_0.BotsGroup.OnAddNeutral += method_0;
		foreach (KeyValuePair<IPlayer, BotSettingsClass> neutral in BotOwner_0.BotsGroup.Neutrals)
		{
			method_0(neutral.Key);
		}
	}

	public bool CanApply(IPlayer person)
	{
		if (BotOwner_0.Settings.FileSettings.Mind.REACT_ADD_DRUNK_ENEMY)
		{
			return person.AIData.IsDrunk;
		}
		if (ShallApplyFollowRequestByLoyalty.TryGetValue(person, out var value))
		{
			return value;
		}
		return true;
	}

	public void method_0(IPlayer neutral)
	{
		if (!ShallApplyFollowRequestByLoyalty.ContainsKey(neutral))
		{
			bool value = GClass856.IsTrue100(neutral.Loyalty.ChanceApplyFollowRequest);
			if (DebugBotData.UseDebugData && DebugBotData.Instance.PhraseRequestAlwaysGood)
			{
				value = true;
			}
			ShallApplyFollowRequestByLoyalty.Add(neutral, value);
		}
	}

	public void Dispose()
	{
		BotOwner_0.BotsGroup.OnAddNeutral -= method_0;
	}
}

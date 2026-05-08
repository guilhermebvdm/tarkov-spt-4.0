using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass173(BotOwner bot, int priority) : GClass167(bot, priority)
{
	public const float DIST_TO_BECOM_ENEMY = 7f;

	[NonSerialized]
	public const float Float_3 = 30f;

	[NonSerialized]
	public const float Float_4 = 10f;

	[NonSerialized]
	public const float Float_5 = 15f;

	[NonSerialized]
	public const float Float_6 = 5f;

	[NonSerialized]
	public const float Float_7 = 8f;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public Dictionary<int, float> Dictionary_0 = new Dictionary<int, float>();

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.gesture, "gestus");
	}

	public override bool ShallUseNow()
	{
		if (Bool_4)
		{
			return true;
		}
		if (Float_8 > Time.time)
		{
			return false;
		}
		float num = 3f;
		Float_8 = Time.time + num;
		foreach (KeyValuePair<IPlayer, BotSettingsClass> neutral in BotOwner_0.BotsGroup.Neutrals)
		{
			float magnitude = (neutral.Key.Position - BotOwner_0.Position).magnitude;
			if (magnitude < 30f)
			{
				num = 1f;
			}
			if (magnitude >= 15f)
			{
				continue;
			}
			if (Dictionary_0.TryGetValue(neutral.Key.Id, out var value) && Time.time - value < 8f)
			{
				method_13().AddEnemy(neutral.Key);
				return false;
			}
			if (magnitude < 10f)
			{
				EInteraction eInteraction = EInteraction.ThereGesture;
				if (BotOwner_0.BotLay.IsLay)
				{
					BotOwner_0.BotLay.GetUp(withCheck: false);
				}
				GClass426 request = new GClass426(neutral.Key, eInteraction, new GClass425(eInteraction));
				BotOwner_0.Gesture.TryAddRequest(request, force: true, withCheckDist: false);
				GClass856.ForceAddValue(Dictionary_0, neutral.Key.Id, Time.time);
				Float_8 = Time.time + 5f;
				Bool_4 = true;
				return true;
			}
			return false;
		}
		Float_8 = Time.time + num;
		return false;
	}

	public override string Name()
	{
		return "CheckZryachiy";
	}

	public override AICoreActionEndStruct EndGestus()
	{
		if (BotOwner_0.Gesture.CurRequestExecuting())
		{
			return AICoreActionEndStruct_1;
		}
		Bool_4 = false;
		return base.EndGestus();
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		return new AICoreActionEndStruct("GetUp");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		return new AICoreActionEndStruct("DoGestus");
	}
}

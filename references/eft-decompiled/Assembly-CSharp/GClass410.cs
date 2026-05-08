using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass410
{
	public class Class134
	{
		[NonSerialized]
		public BotLogicDecision BotLogicDecision_0;

		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		[CompilerGenerated]
		public int Int_1;

		public int Amount
		{
			[CompilerGenerated]
			get
			{
				return Int_1;
			}
			[CompilerGenerated]
			set
			{
				Int_1 = value;
			}
		}

		public void SetDecision(BotLogicDecision decision, int id)
		{
			if (BotLogicDecision_0 == decision && Int_0 == id)
			{
				Amount++;
				return;
			}
			BotLogicDecision_0 = decision;
			Int_0 = id;
			Amount = 0;
		}
	}

	[NonSerialized]
	public const float Float_0 = 10f;

	[NonSerialized]
	public static bool Bool_0;

	[NonSerialized]
	public BotOwner BotOwner_0;

	[NonSerialized]
	public Class134 Class134_0 = new Class134();

	[NonSerialized]
	public float Float_1 = -10f;

	public static void ShowMultipleDecisionLog(bool state)
	{
		Bool_0 = state;
	}

	public GClass410(BotOwner owner)
	{
		BotOwner_0 = owner;
	}

	public BotLogicDecision LogDecision(BotLogicDecision decision, ShortDecision shortDecision = ShortDecision.None, int id = -1, EDecisionMethod method = EDecisionMethod.Unknown)
	{
		Class134_0.SetDecision(decision, id);
		if (Class134_0.Amount > 5 && method_0())
		{
			Float_1 = Time.time;
		}
		return decision;
	}

	public bool method_0()
	{
		if (Bool_0)
		{
			return Float_1 + 10f < Time.time;
		}
		return false;
	}
}

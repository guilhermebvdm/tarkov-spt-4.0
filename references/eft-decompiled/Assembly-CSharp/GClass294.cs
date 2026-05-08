using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class GClass294(BotOwner bot) : GClass177<GClass26>(bot)
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public List<EInteraction> List_0 = new List<EInteraction>();

	public override void Awake()
	{
		foreach (EInteraction value in Enum.GetValues(typeof(EInteraction)))
		{
			List_0.Add(value);
		}
		base.Awake();
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 2f;
			BotOwner_0.GetPlayer.HandsController.ShowGesture(GClass856.RandomElement(List_0));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using UnityEngine;

public class BotsPatrolGeneratorGameEvent : GClass670
{
	public EventObject GeneratorData;

	public bool IsActive;

	[NonSerialized]
	public AICoversData CoversData;

	[field: NonSerialized]
	public int ConnectionGroupId { get; set; } = -1;

	public override List<int> LayersToActivate => new List<int> { 507 };

	[field: NonSerialized]
	public override List<int> LayersToDeActivate { get; }

	public bool EventObjectCompleteSuccess
	{
		get
		{
			if (!IsActive && GeneratorData != null)
			{
				return GeneratorData.State == EventObject.EState.Running;
			}
			return false;
		}
	}

	public BotsPatrolGeneratorGameEvent(BotsClass botsList, AICoversData controller)
		: base(botsList)
	{
		CoversData = controller;
	}

	public override bool CanActivate(BotOwner bot)
	{
		return true;
	}

	public override void Activate()
	{
		if (RunddansControllerAbstractClass.Exist<RunddansControllerAbstractClass>(out var runddansController))
		{
			KeyValuePair<int, EventObject> keyValuePair = runddansController.Objects.FirstOrDefault();
			if (keyValuePair.Key != 0)
			{
				GeneratorData = keyValuePair.Value;
				GeneratorData.OnFinish += method_3;
				IsActive = true;
				BotsClass.OnBotAdd += method_4;
				method_1();
				Vector3 position = GeneratorData.transform.position;
				GroupPoint closest = CoversData.GetClosest(position);
				if (closest != null)
				{
					ConnectionGroupId = closest.ConnectionGroup;
				}
				Start("GeneratorFound");
			}
			else
			{
				IsActive = false;
			}
		}
		else
		{
			IsActive = false;
		}
	}

	public void method_1()
	{
		foreach (BotOwner botOwner in BotsClass.BotOwners)
		{
			botOwner.StandBy.CanDoStandBy = false;
			botOwner.StandBy.Activate();
		}
	}

	public void method_2()
	{
		foreach (BotOwner botOwner in BotsClass.BotOwners)
		{
			botOwner.StandBy.CanDoStandBy = true;
		}
	}

	public void method_3()
	{
		GeneratorData.OnFinish -= method_3;
		Dispose();
	}

	public override void Dispose()
	{
		method_2();
		BotsClass.OnBotAdd -= method_4;
		IsActive = false;
	}

	public void method_4(BotOwner botOwner)
	{
		if (IsActive)
		{
			method_0(botOwner);
		}
	}
}

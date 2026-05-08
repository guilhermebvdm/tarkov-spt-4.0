using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotsForceAttackEvent : GClass670
{
	[NonSerialized]
	public HashSet<string> IdsInteractableToActivate = new HashSet<string>();

	[NonSerialized]
	public HashSet<string> IdsZoneToActivate = new HashSet<string>();

	[NonSerialized]
	public HashSet<string> IdsEventsToActivate = new HashSet<string>();

	[NonSerialized]
	public GClass641.IBotTimer Timer;

	[NonSerialized]
	public GameDateTime GameTime;

	[NonSerialized]
	public BotEventHandler.GDelegate30 OnInteractObject;

	[NonSerialized]
	public Action<string> OnPlayerComeToZone;

	[NonSerialized]
	public Action<string> OnEvent;

	[NonSerialized]
	public ZoneLeaveControllerClass BotZonesLeaveController;

	public override List<int> LayersToActivate => new List<int> { 501, 502 };

	public override List<int> LayersToDeActivate => new List<int>();

	public BotsForceAttackEvent(GameDateTime gameDateTime, BotsClass botsList, ZoneLeaveControllerClass botZonesLeaveController)
		: base(botsList)
	{
		BotZonesLeaveController = botZonesLeaveController;
		GameTime = gameDateTime;
	}

	public override bool Start(string cause)
	{
		if (base.Start(cause))
		{
			BotZonesLeaveController.NoZoneBlocks = true;
			return true;
		}
		return false;
	}

	public override bool CanActivate(BotOwner bot)
	{
		return bot.Settings.FileSettings.Mind.ACTIVE_FORCE_ATTACK_EVENTS;
	}

	public override void BotEventActive(BotOwner bot)
	{
		bot.PriorityAxeTarget.AllPursuit = true;
		bot.LeaveData.BlockLeave();
		base.BotEventActive(bot);
	}

	public void ExternalStart()
	{
		Start("external");
	}

	public override void Activate()
	{
		method_2(IdsInteractableToActivate, LocalBotSettingsProviderClass.Core.ACTIVE_FORCE_ATTACK_EVENTS_INTERACTABLE_IDS);
		method_2(IdsZoneToActivate, LocalBotSettingsProviderClass.Core.ACTIVE_FORCE_ATTACK_EVENTS_ZONE_IDS);
		IdsEventsToActivate = new HashSet<string> { "weatherEvent" };
		BotsClass.OnBotAdd += method_1;
		DateTime dateTime = GameTime.Calculate();
		int aCTIVE_FORCE_ATTACK_EVENTS_HOUR_START = LocalBotSettingsProviderClass.Core.ACTIVE_FORCE_ATTACK_EVENTS_HOUR_START;
		if (aCTIVE_FORCE_ATTACK_EVENTS_HOUR_START > 0)
		{
			int num = dateTime.Hour * 3600 + dateTime.Minute * 60 + dateTime.Second - aCTIVE_FORCE_ATTACK_EVENTS_HOUR_START * 3600;
			Timer = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(num));
			Timer.OnTimer += delegate
			{
				Start("hour");
			};
		}
		OnInteractObject = delegate(string id, Vector3 position)
		{
			if (method_5(id))
			{
				Singleton<BotEventHandler>.Instance.OnInteractObject -= OnInteractObject;
			}
		};
		if (Singleton<BotEventHandler>.Instance != null)
		{
			Singleton<BotEventHandler>.Instance.OnInteractObject += OnInteractObject;
		}
		OnPlayerComeToZone = delegate(string id)
		{
			if (method_4(id))
			{
				Singleton<BotEventHandler>.Instance.OnPlayerComeToPlace -= OnPlayerComeToZone;
			}
		};
		if (Singleton<BotEventHandler>.Instance != null)
		{
			Singleton<BotEventHandler>.Instance.OnPlayerComeToPlace += OnPlayerComeToZone;
		}
		OnEvent = delegate(string id)
		{
			if (method_3(id))
			{
				Singleton<BotEventHandler>.Instance.OnEvent -= OnEvent;
			}
		};
		if (Singleton<BotEventHandler>.Instance != null)
		{
			Singleton<BotEventHandler>.Instance.OnEvent += OnEvent;
		}
		if (LocalBotSettingsProviderClass.Core.START_ACTIVE_FORCE_ATTACK_PLAYER_EVENT)
		{
			Start("OnStart");
		}
	}

	public void method_1(BotOwner bot)
	{
		if (Bool_0)
		{
			method_0(bot);
		}
	}

	public void method_2(HashSet<string> ids, string from)
	{
		string[] array = from.Split(',');
		foreach (string item in array)
		{
			ids.Add(item);
		}
	}

	public bool method_3(string id)
	{
		bool num = IdsEventsToActivate.Contains(id);
		if (num)
		{
			Start("event id:" + id);
		}
		return num;
	}

	public bool method_4(string id)
	{
		bool num = IdsZoneToActivate.Contains(id);
		if (num)
		{
			Start("zone:" + id);
		}
		return num;
	}

	public bool method_5(string doorId)
	{
		bool num = IdsInteractableToActivate.Contains(doorId);
		if (num)
		{
			Start("doorid:" + doorId);
		}
		return num;
	}

	public void DebugActivate()
	{
		Start("debug");
	}

	public new void Dispose()
	{
		BotsClass.OnBotAdd -= method_1;
		Singleton<BotEventHandler>.Instance.OnInteractObject -= OnInteractObject;
		Singleton<BotEventHandler>.Instance.OnEvent -= OnEvent;
		Singleton<BotEventHandler>.Instance.OnPlayerComeToPlace -= OnPlayerComeToZone;
	}

	[CompilerGenerated]
	public void method_6()
	{
		Start("hour");
	}

	[CompilerGenerated]
	public void method_7(string id, Vector3 position)
	{
		if (method_5(id))
		{
			Singleton<BotEventHandler>.Instance.OnInteractObject -= OnInteractObject;
		}
	}

	[CompilerGenerated]
	public void method_8(string id)
	{
		if (method_4(id))
		{
			Singleton<BotEventHandler>.Instance.OnPlayerComeToPlace -= OnPlayerComeToZone;
		}
	}

	[CompilerGenerated]
	public void method_9(string id)
	{
		if (method_3(id))
		{
			Singleton<BotEventHandler>.Instance.OnEvent -= OnEvent;
		}
	}
}

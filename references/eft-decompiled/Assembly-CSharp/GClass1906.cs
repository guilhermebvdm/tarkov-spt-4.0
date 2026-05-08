using System;
using System.Collections.Generic;
using EFT;
using EFT.GlobalEvents;
using EFT.Interactive;
using EFT.UI;
using UnityEngine;

public class GClass1906 : TransitInteractionControllerAbstractClass
{
	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	public Action Action_1;

	[NonSerialized]
	public Action Action_2;

	[NonSerialized]
	public Action Action_3;

	[NonSerialized]
	public Action Action_4;

	[NonSerialized]
	public Action Action_5;

	public GClass1906(BackendConfigSettingsClass.TransitSettingsClass settings, LocationSettingsClass.Location.TransitParameters[] parameters)
		: base(settings, parameters)
	{
		method_19();
		Action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<TransitInitEvent>(method_20);
		Action_1 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<TransitUpdateEvent>(Update);
		Action_3 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<TransitGroupTimerEvent>(method_25);
		Action_4 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<TransitGroupSizeEvent>(method_24);
		Action_2 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<TransitInteractionEvent>(method_22);
		Action_5 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<TransitMessagesEvent>(method_26);
	}

	public void method_19()
	{
		foreach (TransitPoint value in Dictionary_0.Values)
		{
			value.gameObject.SetActive(value: false);
		}
	}

	public static bool smethod_2(int playerId, out Player player)
	{
		player = GamePlayerOwner.MyPlayer;
		if (playerId == -1)
		{
			return true;
		}
		if (player != null)
		{
			return player.PlayerId == playerId;
		}
		return false;
	}

	public void method_20(TransitInitEvent data)
	{
		if (smethod_2(data.PlayerId, out var player))
		{
			summonedTransits[player.ProfileId] = new TransitDataClass(null, data.TransitionCount, null, data.EventPlayer);
			List<TransitPoint> activePoints = method_21(data.Points, player.Side);
			method_8(activePoints, player);
			method_2(activePoints, player);
			Debug.LogError($"[Transit] `{player.ProfileId}` Count:{data.TransitionCount}, EventPlayer:{data.EventPlayer}");
		}
	}

	public void Update(TransitUpdateEvent data)
	{
		if (smethod_2(data.PlayerId, out var player))
		{
			method_8(method_21(data.Points, player.Side), player, data.EventOnly);
		}
	}

	public List<TransitPoint> method_21(Dictionary<int, LocationSettingsClass.Location.TransitParameters> activePoints, EPlayerSide side)
	{
		List<TransitPoint> list = new List<TransitPoint>();
		foreach (var (num2, transitParameters2) in activePoints)
		{
			if (Dictionary_0.TryGetValue(num2, out var value))
			{
				value.parameters.activateAfterSec = transitParameters2.activateAfterSec;
				value.parameters.eventsEnable = transitParameters2.eventsEnable;
				if (side != EPlayerSide.Savage || !method_10(num2, out var _))
				{
					if (!method_7(value))
					{
						HashSet_0.Add(value);
					}
					else
					{
						list.Add(value);
					}
				}
				continue;
			}
			throw new NullReferenceException("TransitPoint not found");
		}
		return list;
	}

	public void method_22(TransitInteractionEvent data)
	{
		if (!smethod_2(data.PlayerId, out var player))
		{
			return;
		}
		switch (data.Type)
		{
		case TransitInteractionEvent.EType.Show:
			method_14(data.PointId, player, method_17());
			break;
		case TransitInteractionEvent.EType.Hide:
			method_18(player);
			break;
		case TransitInteractionEvent.EType.CheckAccess:
		{
			if (method_11(player, data.PointId, out var keyId))
			{
				player.vmethod_3(this, data.PointId, keyId, method_17());
			}
			else
			{
				method_13();
			}
			break;
		}
		case TransitInteractionEvent.EType.InactivePoint:
			InactivePointNotification(player.Id, data.PointId);
			break;
		case TransitInteractionEvent.EType.Confirm:
			method_18(player);
			DisableExits(player);
			break;
		}
		method_23(data.PointId);
	}

	public void method_23(int pointId)
	{
		if (TarkovApplication.Exist(out var tarkovApplication) && Dictionary_0.TryGetValue(pointId, out var value) && (!tarkovApplication.transitionStatus.InTransition || !tarkovApplication.transitionStatus.Location.Equals(value.parameters.location)))
		{
			tarkovApplication.transitionStatus = new TransitionStatusStruct(value.parameters.location, training: false, tarkovApplication.CurrentRaidSettings.Side, tarkovApplication.CurrentRaidSettings.RaidMode, tarkovApplication.CurrentRaidSettings.SelectedDateTime);
		}
	}

	public void method_24(TransitGroupSizeEvent data)
	{
		Sizes(data.Sizes);
	}

	public override void Sizes(Dictionary<int, byte> sizes)
	{
		foreach (var (playerId, size) in sizes)
		{
			if (smethod_2(playerId, out var _))
			{
				MonoBehaviourSingleton<GameUI>.Instance.LocationTransitGroupSize.Display();
				MonoBehaviourSingleton<GameUI>.Instance.LocationTransitGroupSize.Show(size);
			}
		}
	}

	public void method_25(TransitGroupTimerEvent data)
	{
		Timers(data.PointId, data.Timers);
	}

	public override void Timers(int pointId, Dictionary<int, ushort> timers)
	{
		foreach (var (playerId, num3) in timers)
		{
			if (smethod_2(playerId, out var _))
			{
				method_12(pointId);
				MonoBehaviourSingleton<GameUI>.Instance.LocationTransitTimerPanel.Display();
				MonoBehaviourSingleton<GameUI>.Instance.LocationTransitTimerPanel.Show((int)num3);
			}
		}
	}

	public void method_26(TransitMessagesEvent data)
	{
		foreach (var (playerId, eType2) in data.Messages)
		{
			if (smethod_2(playerId, out var _))
			{
				NotificationManagerClass.DisplayWarningNotification(eType2.ToString());
			}
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		Action_0();
		Action_1();
		Action_2();
		Action_3();
		Action_4();
		Action_5();
		CompositeDisposableClass?.Dispose();
	}
}

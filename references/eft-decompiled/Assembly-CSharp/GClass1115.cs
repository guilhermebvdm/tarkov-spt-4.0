using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Audio.Data;
using EFT.GlobalEvents;
using UnityEngine;

public class GClass1115 : GInterface82, IDisposable
{
	[NonSerialized]
	public GInterface81 Ginterface81_0;

	[CompilerGenerated]
	private Action action_0;

	[NonSerialized]
	public Action Action_1;

	public event Action EventReceived
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass1115(GInterface81 speaker)
	{
		Ginterface81_0 = speaker;
		method_0();
	}

	public void method_0()
	{
		Action_1 = (Action)Delegate.Combine(Action_1, GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3544>(method_1));
		Action_1 = (Action)Delegate.Combine(Action_1, GlobalEventHandlerClass.Instance.SubscribeOnEvent<BtrReadyToDepartureEvent>(method_2));
		Action_1 = (Action)Delegate.Combine(Action_1, GlobalEventHandlerClass.Instance.SubscribeOnEvent<BtrIncomingToDestinationGlobalEvent>(method_3));
	}

	public void method_1(GClass3544 goInEvent)
	{
		if (GClass3670.TryGetData<GClass1175>(out var dataContainer) && dataContainer.TryGetGreetingTrigger(goInEvent.PlayerSide, out var trigger))
		{
			method_5(trigger);
		}
	}

	public void method_2(BtrReadyToDepartureEvent readyToDepartureEvent)
	{
		method_5(EBtrDriverPhraseTrigger.ReadyToDepartment);
	}

	public void method_3(BtrIncomingToDestinationGlobalEvent incomingToDestinationEvent)
	{
		string pathID = incomingToDestinationEvent.PathID;
		bool isAnyPassengerInGroupWithAggressor = incomingToDestinationEvent.IsAnyPassengerInGroupWithAggressor;
		bool isAnyEnemyNearBtr = incomingToDestinationEvent.IsAnyEnemyNearBtr;
		EBtrDriverPhraseTrigger trigger = (isAnyPassengerInGroupWithAggressor ? EBtrDriverPhraseTrigger.FarewellAngry : ((!isAnyEnemyNearBtr) ? method_4(pathID) : EBtrDriverPhraseTrigger.FarewellEnemyNear));
		method_5(trigger);
	}

	public EBtrDriverPhraseTrigger method_4(string point)
	{
		float value = UnityEngine.Random.value;
		if (!GClass3670.TryGetData<GClass1175>(out var dataContainer))
		{
			return EBtrDriverPhraseTrigger.FarewellCalm;
		}
		if (!dataContainer.TryGetNamedStationTrigger(point, out var trigger) || value < 0.5f)
		{
			return EBtrDriverPhraseTrigger.FarewellCalm;
		}
		return trigger;
	}

	public void method_5(EBtrDriverPhraseTrigger trigger)
	{
		if (GClass3670.TryGetData<GClass1175>(out var dataContainer))
		{
			if (dataContainer.TryGetBank(trigger, out var bank))
			{
				Ginterface81_0.Play(bank);
			}
			action_0?.Invoke();
		}
	}

	public void method_6()
	{
		Action_1?.Invoke();
		Action_1 = null;
	}

	public void Dispose()
	{
		method_6();
	}
}

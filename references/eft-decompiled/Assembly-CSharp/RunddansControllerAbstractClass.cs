using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using Unity.Mathematics;
using UnityEngine;

public abstract class RunddansControllerAbstractClass : GClass2282, IDisposable
{
	public readonly Dictionary<int, EventObject> Objects = new Dictionary<int, EventObject>();

	[NonSerialized]
	public FlameDamageTrigger[] FlameDamageTrigger_0;

	[NonSerialized]
	public BackendConfigSettingsClass.GClass1748 Gclass1748_0;

	[NonSerialized]
	public LocationSettingsClass.Location Location_0;

	[NonSerialized]
	public Unity.Mathematics.Random Random_0;

	[NonSerialized]
	public const string String_0 = "Buffs_Frostbite";

	[NonSerialized]
	public CancellationTokenSource CancellationTokenSource_0;

	public float InteractionDistanceSqr => Gclass1748_0.interactionDistance * Gclass1748_0.interactionDistance;

	public float GetSecToBreak => Random_0.NextFloat(Gclass1748_0.secToBreak.x, Gclass1748_0.secToBreak.y);

	public float GetDurability => Random_0.NextFloat(Gclass1748_0.durability.x, Gclass1748_0.durability.y);

	public float GrenadeDistanceSqr => Gclass1748_0.grenadeDistanceToBreak * Gclass1748_0.grenadeDistanceToBreak;

	public bool GetKnifeCritToBreak => Random_0.NextFloat(0f, 1f) <= Gclass1748_0.knifeCritChanceToBreak;

	public RunddansControllerAbstractClass(BackendConfigSettingsClass.GClass1748 settings, LocationSettingsClass.Location location)
	{
		Gclass1748_0 = settings;
		Location_0 = location;
		ToggleEventEnvironment(isOn: true);
		method_1();
		FlameDamageTrigger_0 = LocationScene.GetAllObjects<FlameDamageTrigger>().ToArray();
		method_2(flag: true);
		Random_0 = new Unity.Mathematics.Random((uint)DateTime.Now.Millisecond);
		CancellationTokenSource_0 = new CancellationTokenSource();
	}

	public bool method_0(Player player)
	{
		FlameDamageTrigger[] flameDamageTrigger_ = FlameDamageTrigger_0;
		int num = 0;
		while (true)
		{
			if (num < flameDamageTrigger_.Length)
			{
				FlameDamageTrigger flameDamageTrigger = flameDamageTrigger_[num];
				if (!((player.Position - flameDamageTrigger.transform.position).sqrMagnitude >= Gclass1748_0.FireDistanceToHeatSqr))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public static void ToggleEventEnvironment(bool isOn)
	{
		EventEnvironment[] array = LocationScene.GetAllObjects<EventEnvironment>().ToArray();
		foreach (EventEnvironment eventEnvironment in array)
		{
			if (!(eventEnvironment == null))
			{
				eventEnvironment.gameObject.SetActive(isOn);
			}
		}
		if (!isOn)
		{
			smethod_0();
		}
	}

	public static void smethod_0()
	{
		EventObject[] array = LocationScene.GetAllObjects<EventObject>().ToArray();
		foreach (EventObject eventObject in array)
		{
			if (!(eventObject == null))
			{
				eventObject.gameObject.SetActive(value: false);
			}
		}
	}

	public void method_1()
	{
		EventObject[] array = LocationScene.GetAllObjects<EventObject>().ToArray();
		foreach (EventObject eventObject in array)
		{
			if (!(eventObject == null))
			{
				eventObject.Controller = this;
				eventObject.SetState(EventObject.EState.Ready);
				Objects.Add(eventObject.id, eventObject);
			}
		}
	}

	public void method_2(bool flag)
	{
		if (!TransitControllerAbstractClass.Exist<TransitControllerAbstractClass>(out var transitController))
		{
			return;
		}
		foreach (var (_, eventObject2) in Objects)
		{
			if (flag)
			{
				TransitControllerAbstractClass transitControllerAbstractClass = transitController;
				transitControllerAbstractClass.OnSetTimers = (Action<ICollection<TransitPoint>>)Delegate.Combine(transitControllerAbstractClass.OnSetTimers, new Action<ICollection<TransitPoint>>(eventObject2.SetTime));
			}
			else
			{
				TransitControllerAbstractClass transitControllerAbstractClass2 = transitController;
				transitControllerAbstractClass2.OnSetTimers = (Action<ICollection<TransitPoint>>)Delegate.Remove(transitControllerAbstractClass2.OnSetTimers, new Action<ICollection<TransitPoint>>(eventObject2.SetTime));
			}
		}
	}

	public void method_3()
	{
		foreach (KeyValuePair<int, EventObject> @object in Objects)
		{
			@object.Deconstruct(out var _, out var value);
			EventObject eventObject = value;
			eventObject.Handled = true;
			eventObject.OnStateChanged += OnTriggerStateChanged;
		}
	}

	public virtual void OnTriggerStateChanged(EventObject.EState state)
	{
		if (state == EventObject.EState.Broken)
		{
			if (!TransitControllerAbstractClass.Exist<TransitControllerAbstractClass>(out var transitController))
			{
				Debug.LogError("TransitController not found");
			}
			else
			{
				transitController.PauseEventPoints();
			}
		}
	}

	public bool IsValid<T>(Player player, out T transitController, out TransitDataClass info) where T : TransitControllerAbstractClass
	{
		info = null;
		if (!TransitControllerAbstractClass.Exist<T>(out transitController))
		{
			Debug.LogError("TransitController not found");
			return false;
		}
		if (!transitController.summonedTransits.TryGetValue(player.ProfileId, out info))
		{
			Debug.LogError("Player not found in summoned");
			return false;
		}
		return true;
	}

	public void method_4(int objectId, EventObject.EState state)
	{
		if (!Objects.TryGetValue(objectId, out var value))
		{
			Debug.LogError($"EventObject with id {objectId} not found)");
		}
		else
		{
			value.SetState(state);
		}
	}

	public override void InteractWithEventObject(Player player, InteractPacketStruct packet)
	{
	}

	public override InteractPacketStruct GetInteractPacket(int objectId, EventObject.EInteraction interaction)
	{
		return new InteractPacketStruct
		{
			hasInteraction = true,
			objectId = objectId,
			interaction = interaction
		};
	}

	public bool method_5(Player player, out Item item)
	{
		item = null;
		IEnumerable<Item> playerItems = player.InventoryController.Inventory.GetPlayerItems(EPlayerItems.Equipment);
		if (playerItems == null)
		{
			return false;
		}
		item = playerItems.FirstOrDefault((Item i) => Gclass1748_0.consumables.Contains(i.StringTemplateId));
		return item != null;
	}

	public bool TryGetAirplaneId(out int id)
	{
		id = 0;
		if (!Gclass1748_0.sleighLocations.Contains(Location_0.Id))
		{
			return false;
		}
		id = 1;
		return true;
	}

	public bool IsNonExitLocation()
	{
		return Gclass1748_0.nonExitsLocations?.Contains(Location_0.Id) ?? false;
	}

	public static bool Exist<T>(out T runddansController) where T : RunddansControllerAbstractClass
	{
		if (Singleton<GameWorld>.Instance != null && Singleton<GameWorld>.Instance.RunddansController is T val)
		{
			runddansController = val;
			return true;
		}
		runddansController = null;
		return false;
	}

	public virtual void Dispose()
	{
		CancellationTokenSource_0?.Cancel();
		method_2(flag: false);
		foreach (KeyValuePair<int, EventObject> @object in Objects)
		{
			@object.Deconstruct(out var _, out var value);
			value.SetState(EventObject.EState.Disable);
		}
		Objects.Clear();
	}

	[CompilerGenerated]
	public bool method_6(Item i)
	{
		return Gclass1748_0.consumables.Contains(i.StringTemplateId);
	}
}

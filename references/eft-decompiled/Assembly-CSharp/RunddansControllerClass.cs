using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using UnityEngine;

public class RunddansControllerClass : RunddansControllerAbstractClass
{
	[CompilerGenerated]
	public class Class1457
	{
		public Player player;

		public RunddansControllerClass RunddansControllerClass;

		public int objectId;

		public void method_0(bool successful)
		{
			MonoBehaviourSingleton<GameUI>.Instance.EventStatePanel.StopAction();
			GlobalEventHandlerClass.CreateEvent<GClass3574>().Invoke("Generator/Repair", EInteractionStatus.Finished);
			if (successful)
			{
				player.vmethod_5(RunddansControllerClass, objectId, EventObject.EInteraction.Repair);
			}
		}
	}

	[CompilerGenerated]
	public class Class1458
	{
		public RunddansControllerClass RunddansControllerClass;

		public Player player;

		public int objectId;

		public GamePlayerOwner owner;

		public void method_0()
		{
			RunddansControllerClass.method_8(player, objectId);
		}

		public void method_1()
		{
			RunddansControllerClass.method_9(owner, objectId);
		}
	}

	[NonSerialized]
	public const string String_1 = "Generator/FillTheTank";

	[NonSerialized]
	public const string String_2 = "Generator/Repair";

	[NonSerialized]
	public const string String_3 = "Generator/NoRequiredItem";

	[NonSerialized]
	public const string String_4 = "Generator/NeedIdle";

	[NonSerialized]
	public const string String_5 = "Generator/NonInteractive";

	[NonSerialized]
	public const string String_6 = "Repairing generator {0:F1}";

	[NonSerialized]
	public ActionsReturnClass ActionsReturnClass;

	public RunddansControllerClass(BackendConfigSettingsClass.GClass1748 settings, LocationSettingsClass.Location location)
		: base(settings, location)
	{
	}

	public ActionsReturnClass GetAvailableInteraction(GamePlayerOwner owner, int objectId)
	{
		Player player = owner.Player;
		if (IsValid<TransitInteractionControllerAbstractClass>(player, out var _, out var info) && info.events)
		{
			if (!Objects.TryGetValue(objectId, out var value))
			{
				Debug.LogError($"EventObject with id {objectId} not found)");
				return null;
			}
			string empty = string.Empty;
			Action action = null;
			switch (value.State)
			{
			case EventObject.EState.Ready:
				empty = "Generator/FillTheTank";
				action = delegate
				{
					method_8(player, objectId);
				};
				break;
			default:
				return null;
			case EventObject.EState.Broken:
				empty = "Generator/Repair";
				action = delegate
				{
					method_9(owner, objectId);
				};
				break;
			}
			return method_7(empty, action);
		}
		return null;
	}

	public ActionsReturnClass method_7(string name, Action action)
	{
		if (name != null && action != null)
		{
			if (ActionsReturnClass == null)
			{
				ActionsReturnClass = new ActionsReturnClass();
				ActionsReturnClass.Actions.Add(new ActionsTypesClass
				{
					Name = name,
					Action = action
				});
			}
			else
			{
				ActionsReturnClass.Actions[0].Name = name;
				ActionsReturnClass.Actions[0].Action = action;
			}
			return ActionsReturnClass;
		}
		return null;
	}

	public void method_8(Player player, int objectId)
	{
		if (!method_5(player, out var _))
		{
			method_13();
		}
		else
		{
			player.vmethod_5(this, objectId, EventObject.EInteraction.Run);
		}
	}

	public void method_9(GamePlayerOwner owner, int objectId)
	{
		if (!(owner.Player.CurrentState is IdleStateClass))
		{
			method_12();
			return;
		}
		Player player = owner.Player;
		bool hasMultiTool;
		float num = method_11(player, out hasMultiTool);
		GlobalEventHandlerClass.CreateEvent<GClass3574>().Invoke("Generator/Repair", EInteractionStatus.Started);
		MonoBehaviourSingleton<GameUI>.Instance.EventStatePanel.StartAction("Repairing generator {0:F1}", num);
		owner.Player.CurrentManagedState.Plant(enabled: true, hasMultiTool, num, delegate(bool successful)
		{
			MonoBehaviourSingleton<GameUI>.Instance.EventStatePanel.StopAction();
			GlobalEventHandlerClass.CreateEvent<GClass3574>().Invoke("Generator/Repair", EInteractionStatus.Finished);
			if (successful)
			{
				player.vmethod_5(this, objectId, EventObject.EInteraction.Repair);
			}
		});
	}

	public bool method_10(Item item)
	{
		if (!(item?.Parent.Container is StashGridClass stashGridClass))
		{
			return false;
		}
		GStruct154<GClass3413> gStruct = stashGridClass.Remove(item, simulate: false);
		if (!gStruct.Failed)
		{
			return true;
		}
		Debug.LogError(gStruct.Error);
		return false;
	}

	public float method_11(Player player, out bool hasMultiTool)
	{
		IEnumerable<Item> playerItems = player.InventoryController.Inventory.GetPlayerItems(EPlayerItems.Equipment);
		hasMultiTool = playerItems.OfType<MultitoolsItemClass>().FirstOrDefault() != null;
		if (!hasMultiTool)
		{
			return Gclass1748_0.repairSec;
		}
		return Gclass1748_0.multitoolRepairSec;
	}

	public override void OnTriggerStateChanged(EventObject.EState state)
	{
		base.OnTriggerStateChanged(state);
		if (GamePlayerOwner.MyPlayer != null)
		{
			GamePlayerOwner.MyPlayer.ForceInteractionsChanged();
		}
	}

	public void method_12()
	{
		NotificationManagerClass.DisplayMessageNotification(GClass2348.Localized("Generator/NeedIdle"));
	}

	public void method_13()
	{
		NotificationManagerClass.DisplayWarningNotification(GClass2348.Localized("Generator/NoRequiredItem"));
	}

	public void method_14()
	{
		NotificationManagerClass.DisplayWarningNotification(GClass2348.Localized("Generator/NonInteractive"));
	}
}

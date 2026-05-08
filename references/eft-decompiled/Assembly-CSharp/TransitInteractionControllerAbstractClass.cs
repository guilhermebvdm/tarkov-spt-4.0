using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Screens;
using JsonType;

public abstract class TransitInteractionControllerAbstractClass : TransitControllerAbstractClass
{
	[CompilerGenerated]
	public class Class1176
	{
		public TransitInteractionControllerAbstractClass TransitInteractionControllerAbstractClass;

		public Player player;

		public void method_0()
		{
			player.InventoryController.AddItemEvent -= TransitInteractionControllerAbstractClass.method_6;
		}
	}

	[CompilerGenerated]
	public class Class1177
	{
		public Player player;

		public string[] accessKeys;

		public bool method_0(Item equippedItem)
		{
			if (player.InventoryController.Examined(equippedItem))
			{
				return accessKeys.Contains(equippedItem.StringTemplateId);
			}
			return false;
		}
	}

	[CompilerGenerated]
	public class Class1178
	{
		public TransitInteractionControllerAbstractClass TransitInteractionControllerAbstractClass;

		public Player player;

		public int pointId;

		public string keyId;

		public EDateTime time;

		public void method_0()
		{
			TransitInteractionControllerAbstractClass.method_15(player, pointId, keyId, time);
		}

		public void method_1()
		{
			TransitInteractionControllerAbstractClass.method_15(player, pointId, keyId, time);
		}
	}

	[CompilerGenerated]
	public class Class1179
	{
		public GameUI gameUI;

		public Player player;

		public TransitInteractionControllerAbstractClass TransitInteractionControllerAbstractClass;

		public int pointId;

		public string keyId;

		public EDateTime time;

		public void method_0(bool successful)
		{
			gameUI.LocationTransitTimerPanel.Close();
			if (successful)
			{
				player.MovementContext.ResetCanUsePropState();
				TransitInteractionControllerAbstractClass.method_16(player, pointId, keyId, time);
			}
		}
	}

	[CompilerGenerated]
	public class Class1180
	{
		public Player player;

		public TransitInteractionControllerAbstractClass TransitInteractionControllerAbstractClass;

		public int pointId;

		public string keyId;

		public EDateTime time;

		public void method_0()
		{
			player.vmethod_3(TransitInteractionControllerAbstractClass, pointId, keyId, time);
		}
	}

	[NonSerialized]
	public const string String_0 = "Transit/LONG_TAP_TO_INTERACT";

	[NonSerialized]
	public const string String_1 = "Transit/AccessNotGranted";

	[NonSerialized]
	public const string String_2 = "Transit/InactivePoint";

	[NonSerialized]
	public const string String_3 = "Transit/Interaction";

	[NonSerialized]
	public CompositeDisposableClass CompositeDisposableClass = new CompositeDisposableClass();

	[NonSerialized]
	public HashSet<TransitPoint> HashSet_0 = new HashSet<TransitPoint>();

	[NonSerialized]
	public List<TransitPoint> List_0 = new List<TransitPoint>();

	[NonSerialized]
	public ActionsReturnClass ActionsReturnClass;

	[NonSerialized]
	public const float Float_0 = 2f;

	[NonSerialized]
	[CompilerGenerated]
	public ActionsReturnClass ActionsReturnClass_1;

	public ActionsReturnClass AvailableInteractionState
	{
		[CompilerGenerated]
		get
		{
			return ActionsReturnClass_1;
		}
		[CompilerGenerated]
		set
		{
			ActionsReturnClass_1 = value;
		}
	}

	public TransitInteractionControllerAbstractClass(BackendConfigSettingsClass.TransitSettingsClass settings, LocationSettingsClass.Location.TransitParameters[] parameters)
		: base(settings, parameters)
	{
		TransitInteractionControllerAbstractClass TransitInteractionControllerAbstractClass = this;
		Player player = GamePlayerOwner.MyPlayer;
		if (!(player == null))
		{
			player.InventoryController.AddItemEvent += method_6;
			CompositeDisposableClass.AddDisposable(delegate
			{
				player.InventoryController.AddItemEvent -= TransitInteractionControllerAbstractClass.method_6;
			});
		}
	}

	public void method_6(GEventArgs2 addItemEventArgs)
	{
		Player myPlayer = GamePlayerOwner.MyPlayer;
		if (myPlayer == null)
		{
			return;
		}
		List_0.Clear();
		foreach (TransitPoint item in HashSet_0)
		{
			if (method_11(myPlayer, item.parameters.id, out var _))
			{
				List_0.Add(item);
			}
		}
		foreach (TransitPoint item2 in List_0)
		{
			HashSet_0.Remove(item2);
		}
		if (List_0.Count > 0)
		{
			method_8(List_0, myPlayer);
		}
	}

	public bool method_7(TransitPoint transitPoint)
	{
		Player myPlayer = GamePlayerOwner.MyPlayer;
		if (myPlayer == null)
		{
			return true;
		}
		if (!transitPoint.parameters.hideIfNoKey)
		{
			return true;
		}
		if (!method_11(myPlayer, transitPoint.parameters.id, out var _))
		{
			return false;
		}
		return true;
	}

	public void method_8(ICollection<TransitPoint> activePoints, Player player, bool eventOnly = false)
	{
		if (player == null)
		{
			return;
		}
		bool flag = summonedTransits[player.ProfileId]?.events ?? false;
		MonoBehaviourSingleton<GameUI>.Instance.TimerPanel.SetTransits(activePoints, flag);
		if (eventOnly)
		{
			if (flag)
			{
				MonoBehaviourSingleton<GameUI>.Instance.TimerPanel.ShowTimer(showExits: true);
			}
		}
		else
		{
			MonoBehaviourSingleton<GameUI>.Instance.TimerPanel.ShowTimer(showExits: true);
		}
		OnSetTimers?.Invoke(activePoints);
	}

	public bool method_9(string locationId, out LocationSettingsClass.Location location)
	{
		if (!Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession().LocationSettings.locations.TryGetValue(locationId, out location))
		{
			return false;
		}
		return true;
	}

	public bool method_10(int pointId, out string[] accessKeys)
	{
		accessKeys = null;
		if (!method_9(Dictionary_0[pointId].parameters.target, out var location))
		{
			return false;
		}
		accessKeys = location?.AccessKeys;
		if (accessKeys != null)
		{
			return accessKeys.Length != 0;
		}
		return false;
	}

	public bool method_11(Player player, int pointId, out string keyId)
	{
		keyId = string.Empty;
		if (!method_10(pointId, out var accessKeys))
		{
			return true;
		}
		if (player.Side == EPlayerSide.Savage)
		{
			return false;
		}
		Item item = player.InventoryController.Inventory.GetPlayerItems(EPlayerItems.Equipment)?.FirstOrDefault((Item equippedItem) => player.InventoryController.Examined(equippedItem) && accessKeys.Contains(equippedItem.StringTemplateId));
		if (item == null)
		{
			return false;
		}
		keyId = item.Id;
		return true;
	}

	public void method_12(int pointId)
	{
		if (Dictionary_0.TryGetValue(pointId, out var value))
		{
			MonoBehaviourSingleton<GameUI>.Instance.TimerPanel.SwitchTimers(value.Description, showOnePoint: true);
			MonoBehaviourSingleton<GameUI>.Instance.TimerPanel.ShowTimer(showExits: true);
		}
	}

	public override bool DisableExits(Player player)
	{
		if (base.DisableExits(player))
		{
			ExfiltrationControllerClass.Instance.DisableExitsInteraction();
		}
		return true;
	}

	public override bool EnableExits(Player player)
	{
		if (base.EnableExits(player))
		{
			ExfiltrationControllerClass.Instance.EnableExitsInteraction();
		}
		return true;
	}

	public void method_13()
	{
		NotificationManagerClass.DisplayWarningNotification(GClass2348.Localized("Transit/AccessNotGranted"));
	}

	public void method_14(int pointId, Player player, EDateTime time)
	{
		NotificationManagerClass.DisplaySingletonNotification(GClass2348.Localized("Transit/LONG_TAP_TO_INTERACT"));
		if (!method_11(player, pointId, out var keyId))
		{
			method_18(player);
			method_13();
			return;
		}
		method_12(pointId);
		if (ActionsReturnClass == null)
		{
			ActionsReturnClass = new ActionsReturnClass();
			ActionsReturnClass.Actions.Add(new ActionsTypesClass
			{
				Name = "Transit/Interaction",
				Action = delegate
				{
					method_15(player, pointId, keyId, time);
				}
			});
		}
		else
		{
			ActionsReturnClass.Actions[0].Action = delegate
			{
				method_15(player, pointId, keyId, time);
			};
		}
		AvailableInteractionState = ActionsReturnClass;
		player.ForceInteractionsChanged();
	}

	public void method_15(Player player, int pointId, string keyId, EDateTime time)
	{
		GameUI gameUI = MonoBehaviourSingleton<GameUI>.Instance;
		if (!(player.CurrentState is IdleStateClass))
		{
			NotificationManagerClass.DisplayMessageNotification(GClass2348.Localized("NeedIdle"));
			return;
		}
		gameUI.LocationTransitTimerPanel.Show(2f, "Confirmation {0:F1}");
		player.CurrentManagedState.Plant(enabled: true, multitool: true, 2f, delegate(bool successful)
		{
			gameUI.LocationTransitTimerPanel.Close();
			if (successful)
			{
				player.MovementContext.ResetCanUsePropState();
				method_16(player, pointId, keyId, time);
			}
		});
	}

	public void method_16(Player player, int pointId, string keyId, EDateTime time)
	{
		if (player is ClientPlayer clientPlayer)
		{
			clientPlayer.GetTraderServicesDataFromServer("656f0f98d80a697f855d34b1");
		}
		TransferItemsInRaidScreen.GClass3893 gClass = new TransferItemsInRaidScreen.GClass3893(player.Profile, player.InventoryController, player.AbstractQuestControllerClass, new InsuranceCompanyClass(null, player.Profile), base.TransferItemsController);
		gClass.ShowScreen(EScreenState.Queued);
		gClass.OnClose += delegate
		{
			player.vmethod_3(this, pointId, keyId, time);
		};
	}

	public EDateTime method_17()
	{
		if (!TarkovApplication.Exist(out var tarkovApplication))
		{
			return EDateTime.CURR;
		}
		return tarkovApplication.CurrentRaidSettings.SelectedDateTime;
	}

	public void method_18(Player player)
	{
		AvailableInteractionState = null;
		player.ForceInteractionsChanged();
		MonoBehaviourSingleton<GameUI>.Instance.LocationTransitGroupSize.Close();
		MonoBehaviourSingleton<GameUI>.Instance.LocationTransitTimerPanel.Close();
	}

	public override void InactivePointNotification(int playerId, int pointId)
	{
		NotificationManagerClass.DisplayWarningNotification(GClass2348.Localized("Transit/InactivePoint"));
		method_12(pointId);
	}
}

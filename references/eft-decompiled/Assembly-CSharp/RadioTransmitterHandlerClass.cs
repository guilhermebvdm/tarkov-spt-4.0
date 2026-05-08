using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

public class RadioTransmitterHandlerClass : GClass1909
{
	[NonSerialized]
	public const string String_0 = "Lighthouse";

	[NonSerialized]
	public Player Player_0;

	[NonSerialized]
	public RadioTransmitterRecodableComponent RadioTransmitterRecodableComponent_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public List<IPlayer> List_0 = new List<IPlayer>();

	[NonSerialized]
	public List<BotOwner> List_1 = new List<BotOwner>();

	[NonSerialized]
	public WildSpawnType[] WildSpawnType_0 = new WildSpawnType[2]
	{
		WildSpawnType.bossZryachiy,
		WildSpawnType.followerZryachiy
	};

	[CompilerGenerated]
	private Action<Player> action_0;

	[CompilerGenerated]
	private Action action_1;

	public RadioTransmitterStatus Status => RadioTransmitterRecodableComponent_0.Status;

	public RadioTransmitterRecodableComponent RecodableComponent => RadioTransmitterRecodableComponent_0;

	public event Action<Player> OnStatusChanged
	{
		[CompilerGenerated]
		add
		{
			Action<Player> action = action_0;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Player> action = action_0;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnAllTeammatesChangeRadioTransmitterStatus
	{
		[CompilerGenerated]
		add
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public RadioTransmitterHandlerClass(RadioTransmitterRecodableComponent radioTransmitterRecodableComponent, Player player)
	{
		method_0(radioTransmitterRecodableComponent, player);
	}

	public void method_0(RadioTransmitterRecodableComponent radioTransmitterRecodableComponent, Player player)
	{
		AbstractGame instance = Singleton<AbstractGame>.Instance;
		Bool_1 = (object)instance != null && instance.GameType == EGameType.Offline;
		method_3(player);
		method_4(radioTransmitterRecodableComponent);
		method_6();
		method_5();
		method_12();
	}

	public void ReInitInOfflineGame(RadioTransmitterRecodableComponent radioTransmitterRecodableComponent, LocalPlayer player)
	{
		method_0(radioTransmitterRecodableComponent, player);
	}

	~RadioTransmitterHandlerClass()
	{
		method_9();
	}

	public void OnPlayerAddRecodableHandler(Player player)
	{
		method_3(player);
		if (Bool_1)
		{
			method_10();
			method_12();
		}
	}

	public void OnPlayerRemoveRecodableHandler()
	{
		Player_0 = null;
		if (Bool_1)
		{
			List_1.Clear();
		}
	}

	public void method_1()
	{
	}

	public void method_2()
	{
	}

	public void method_3(Player player)
	{
		Player_0 = player;
	}

	public void method_4(RadioTransmitterRecodableComponent recodableComponent)
	{
		RadioTransmitterRecodableComponent_0 = recodableComponent;
	}

	public void method_5()
	{
		bool isEncoded = RadioTransmitterRecodableComponent_0.IsEncoded;
		if (Bool_1)
		{
			if (isEncoded)
			{
				RadioTransmitterRecodableComponent_0.SetStatus(RadioTransmitterStatus.Green);
			}
			else
			{
				RadioTransmitterRecodableComponent_0.SetStatus(RadioTransmitterStatus.Red);
			}
		}
		else if (isEncoded)
		{
			RadioTransmitterRecodableComponent_0.SetStatus(RadioTransmitterStatus.Yellow);
		}
		else
		{
			RadioTransmitterRecodableComponent_0.SetStatus(RadioTransmitterStatus.Red);
		}
	}

	public void SetEncoded(bool isEncoded)
	{
		if (Bool_1)
		{
			RadioTransmitterRecodableComponent_0.SetEncoded(isEncoded);
			method_13();
		}
	}

	public void method_6()
	{
		if (Singleton<GameWorld>.Instance.LocationId == "Lighthouse")
		{
			Bool_2 = true;
		}
		else
		{
			Bool_2 = false;
		}
	}

	public void method_7()
	{
	}

	public void method_8()
	{
	}

	public void method_9()
	{
	}

	public void method_10()
	{
		if (!Bool_1)
		{
			return;
		}
		foreach (Player value in Singleton<GameWorld>.Instance.allAlivePlayersByID.Values)
		{
			if (!value.IsAI)
			{
				continue;
			}
			WildSpawnType role = value.AIData.BotOwner.Profile.Info.Settings.Role;
			WildSpawnType[] wildSpawnType_ = WildSpawnType_0;
			foreach (WildSpawnType wildSpawnType in wildSpawnType_)
			{
				if (role == wildSpawnType)
				{
					List_1.Add(value.AIData.BotOwner);
					break;
				}
			}
		}
	}

	public void method_11()
	{
		string groupId = Player_0.Profile.Info.GroupId;
		List_0.Clear();
		if (string.IsNullOrEmpty(groupId))
		{
			return;
		}
		foreach (Player item2 in Singleton<GameWorld>.Instance.AllPlayersEverExisted)
		{
			if (!item2.IsAI && !(Player_0 == item2) && (object)item2 != null)
			{
				Player item = item2;
				if (item2.GroupId == groupId)
				{
					List_0.Add(item);
				}
			}
		}
	}

	public void method_12()
	{
		method_13();
		method_17();
	}

	public void method_13()
	{
		if (Bool_1)
		{
			method_15();
		}
	}

	public bool method_14()
	{
		foreach (IPlayer item in List_0)
		{
			if (item.HealthController.IsAlive)
			{
				RadioTransmitterRecodableComponent radioTransmitterRecodableComponent = item.FindRadioTransmitter();
				if (radioTransmitterRecodableComponent == null)
				{
					return false;
				}
				if (!radioTransmitterRecodableComponent.IsEncoded)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void method_15()
	{
		if (!Bool_1)
		{
			return;
		}
		if (Player_0 == null)
		{
			RadioTransmitterRecodableComponent_0.SetStatus(RadioTransmitterStatus.NotInitialized);
			return;
		}
		bool isEncoded = RadioTransmitterRecodableComponent_0.IsEncoded;
		bool flag = method_14();
		RadioTransmitterStatus status = RadioTransmitterRecodableComponent_0.Status;
		if (isEncoded)
		{
			if (Bool_2 && flag && status == RadioTransmitterStatus.Green)
			{
				RadioTransmitterRecodableComponent_0.SetStatus(RadioTransmitterStatus.Green);
			}
			else
			{
				RadioTransmitterRecodableComponent_0.SetStatus(RadioTransmitterStatus.Yellow);
			}
		}
		else
		{
			RadioTransmitterRecodableComponent_0.SetStatus(RadioTransmitterStatus.Red);
		}
		if (status != RadioTransmitterRecodableComponent_0.Status)
		{
			action_0?.Invoke(Player_0);
		}
	}

	public void method_16()
	{
	}

	public void OnGetRadioTransmitterStatusFromClient()
	{
		method_17();
	}

	public void method_17()
	{
	}

	public void method_18()
	{
	}

	public void method_19()
	{
	}
}

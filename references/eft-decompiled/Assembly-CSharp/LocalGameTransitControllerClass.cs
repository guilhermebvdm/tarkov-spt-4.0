using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using JetBrains.Annotations;
using UnityEngine;

public class LocalGameTransitControllerClass : TransitInteractionControllerAbstractClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class1181
	{
		public static readonly Class1181 class1181_0 = new Class1181();

		public static Func<string, string, string> func_0;

		public string method_0(string current, string location)
		{
			return current + location + " -> ";
		}
	}

	[CompilerGenerated]
	public class Class1182
	{
		public LocalGameTransitControllerClass LocalGameTransitControllerClass;

		public Player player;

		public void method_0()
		{
			player.InventoryController.AddItemEvent -= LocalGameTransitControllerClass.method_19;
		}
	}

	[NonSerialized]
	public LocalRaidSettings LocalRaidSettings_0;

	public LocalGameTransitControllerClass(BackendConfigSettingsClass.TransitSettingsClass settings, LocationSettingsClass.Location.TransitParameters[] parameters, Profile profile, LocalRaidSettings localRaidSettings)
		: base(settings, parameters)
	{
		LocalGameTransitControllerClass LocalGameTransitControllerClass = this;
		LocalRaidSettings_0 = localRaidSettings;
		OnPlayerEnter = (Action<TransitPoint, Player>)Delegate.Combine(OnPlayerEnter, new Action<TransitPoint, Player>(method_20));
		OnPlayerExit = (Action<TransitPoint, Player>)Delegate.Combine(OnPlayerExit, new Action<TransitPoint, Player>(method_21));
		string[] array = GClass853.EmptyIfNull(localRaidSettings.transition.visitedLocations).Append(localRaidSettings.location).ToArray();
		string empty = string.Empty;
		empty = array.Aggregate(empty, (string current, string location) => current + location + " -> ");
		Debug.LogError("[Transit] " + $"Flag:{localRaidSettings.transition.transitionType}, " + "RaidId:" + localRaidSettings.transition.transitionRaidId + ", " + $"Count:{localRaidSettings.transition.transitionCount}, " + "Locations:" + empty);
		summonedTransits[profile.Id] = new TransitDataClass(localRaidSettings.transition.transitionRaidId, localRaidSettings.transition.transitionCount, array, GClass867.HasFlagNoBox(localRaidSettings.transition.transitionType, ELocationTransition.Event));
		base.TransferItemsController.InitItemControllerServer("656f0f98d80a697f855d34b1", "BTR");
		Player player = GamePlayerOwner.MyPlayer;
		if (!(player == null))
		{
			player.InventoryController.AddItemEvent += method_19;
			CompositeDisposableClass.AddDisposable(delegate
			{
				player.InventoryController.AddItemEvent -= LocalGameTransitControllerClass.method_19;
			});
		}
	}

	public void method_19(GEventArgs2 addItemEventArgs)
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

	public void UpdateTimers()
	{
		List<TransitPoint> list = new List<TransitPoint>();
		foreach (TransitPoint value in Dictionary_0.Values)
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
		method_8(list, GamePlayerOwner.MyPlayer);
	}

	public void HandleExits()
	{
		method_2(Dictionary_0.Values, GamePlayerOwner.MyPlayer);
	}

	public override void Transit(TransitPoint point, int playersCount, string hash, Dictionary<string, ProfileKey> keys, Player player)
	{
		string location = point.parameters.location;
		ERaidMode eRaidMode = ERaidMode.Local;
		if (TarkovApplication.Exist(out var tarkovApplication))
		{
			eRaidMode = ((!tarkovApplication.TryGetLocationById(location, out var location2) || !location2.ForceOnlineRaidInPVE) ? ERaidMode.Local : ERaidMode.Online);
			tarkovApplication.transitionStatus = new TransitionStatusStruct(location, LocalRaidSettings_0.mode == ELocalMode.TRAINING, LocalRaidSettings_0.playerSide, eRaidMode, LocalRaidSettings_0.timeVariant);
		}
		if (method_11(player, point.parameters.id, out var keyId))
		{
			foreach (Item playerItem in player.InventoryController.Inventory.GetPlayerItems(EPlayerItems.Equipment))
			{
				if (playerItem.Id.Equals(keyId))
				{
					InteractionsHandlerClass.Remove(playerItem, player.InventoryController);
					break;
				}
			}
		}
		string profileId = player.ProfileId;
		AlreadyTransitDataClass value = new AlreadyTransitDataClass
		{
			hash = hash,
			playersCount = playersCount,
			ip = "",
			location = location,
			profiles = keys,
			transitionRaidId = summonedTransits[profileId].raidId,
			raidMode = eRaidMode,
			side = ((player.Side == EPlayerSide.Savage) ? ESideType.Savage : ESideType.Pmc),
			dayTime = LocalRaidSettings_0.timeVariant
		};
		alreadyTransits.Add(profileId, value);
		if (Singleton<AbstractGame>.Instance is LocalGame localGame)
		{
			localGame.Stop(profileId, ExitStatus.Transit, point.parameters.name);
		}
	}

	public override void InteractWithTransit(Player player, TransitInteractionPacketStruct packet)
	{
		method_18(player);
		transitPlayers.Add(player.ProfileId, player.Id);
		profileKeys[player.ProfileId] = packet.keyId;
		Dictionary_0[packet.pointId].GroupEnter(player);
		ExfiltrationControllerClass.Instance.BannedPlayers.Add(player.Id);
		ExfiltrationControllerClass.Instance.CancelExtractionForPlayer(player);
		ExfiltrationControllerClass.Instance.DisableExitsInteraction();
	}

	public override void Sizes(Dictionary<int, byte> sizes)
	{
		foreach (var (_, size) in sizes)
		{
			MonoBehaviourSingleton<GameUI>.Instance.LocationTransitGroupSize.Display();
			MonoBehaviourSingleton<GameUI>.Instance.LocationTransitGroupSize.Show(size);
		}
	}

	public override void Timers(int pointId, Dictionary<int, ushort> timers)
	{
		foreach (var (_, num3) in timers)
		{
			method_12(pointId);
			MonoBehaviourSingleton<GameUI>.Instance.LocationTransitTimerPanel.Display();
			MonoBehaviourSingleton<GameUI>.Instance.LocationTransitTimerPanel.Show((int)num3);
		}
	}

	public void method_20([NotNull] TransitPoint point, [NotNull] Player player)
	{
		if (!method_11(player, point.parameters.id, out var _))
		{
			method_13();
		}
		else if (!transitPlayers.ContainsKey(player.ProfileId))
		{
			base.TransferItemsController.InitPlayerStash(player);
			if (player is LocalPlayer localPlayer)
			{
				localPlayer.UpdateBtrTraderServiceData().HandleExceptions();
			}
			method_14(point.parameters.id, player, method_17());
		}
		else
		{
			Dictionary_0[point.parameters.id].GroupEnter(player);
		}
	}

	public void method_21([NotNull] TransitPoint point, [NotNull] Player player)
	{
		if (transitPlayers.ContainsKey(player.ProfileId))
		{
			point.GroupExit(player);
		}
		method_18(player);
	}

	public override void Dispose()
	{
		base.Dispose();
		OnPlayerEnter = (Action<TransitPoint, Player>)Delegate.Remove(OnPlayerEnter, new Action<TransitPoint, Player>(method_20));
		OnPlayerExit = (Action<TransitPoint, Player>)Delegate.Remove(OnPlayerExit, new Action<TransitPoint, Player>(method_21));
	}
}

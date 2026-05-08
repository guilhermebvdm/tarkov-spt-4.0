using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using EFT.Interactive;
using JsonType;

public abstract class TransitControllerAbstractClass : IDisposable
{
	[CompilerGenerated]
	public class Class1183
	{
		public TransitControllerAbstractClass TransitControllerAbstractClass;

		public Player player;

		public Action action_0;

		public void method_0()
		{
			TransitControllerAbstractClass.EnableExits(player);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct304
	{
		public TransitControllerAbstractClass controller;

		public List<ISpawnPoint> spawnPoints;
	}

	[NonSerialized]
	public BackendConfigSettingsClass.TransitSettingsClass TransitSettingsClass;

	[NonSerialized]
	public Dictionary<int, TransitPoint> Dictionary_0 = new Dictionary<int, TransitPoint>();

	public readonly Dictionary<string, int> transitPlayers = new Dictionary<string, int>();

	public readonly Dictionary<string, TransitDataClass> summonedTransits = new Dictionary<string, TransitDataClass>();

	public readonly Dictionary<string, AlreadyTransitDataClass> alreadyTransits = new Dictionary<string, AlreadyTransitDataClass>();

	public readonly Dictionary<string, string> profileKeys = new Dictionary<string, string>();

	public readonly Dictionary<string, EDateTime> profileTime = new Dictionary<string, EDateTime>();

	public readonly List<LocationOrigin> locationOrigins = new List<LocationOrigin>();

	public readonly HashSet<ISpawnPoint> usedSpawnPoints = new HashSet<ISpawnPoint>();

	public Action<TransitPoint, Player> OnPlayerEnter;

	public Action<TransitPoint, Player> OnPlayerExit;

	public Action<ICollection<TransitPoint>> OnSetTimers;

	[NonSerialized]
	[CompilerGenerated]
	public TransferItemsControllerAbstractClass TransferItemsControllerAbstractClass;

	public TransferItemsControllerAbstractClass TransferItemsController
	{
		[CompilerGenerated]
		get
		{
			return TransferItemsControllerAbstractClass;
		}
		[CompilerGenerated]
		set
		{
			TransferItemsControllerAbstractClass = value;
		}
	}

	public TransitControllerAbstractClass(BackendConfigSettingsClass.TransitSettingsClass settings, LocationSettingsClass.Location.TransitParameters[] parameters)
	{
		TransitSettingsClass = settings;
		method_0(parameters);
		TransferItemsController = new GClass1901(Singleton<GameWorld>.Instance, settings, Singleton<AbstractGame>.Instance.GameType == EGameType.Offline);
	}

	public void method_0(LocationSettingsClass.Location.TransitParameters[] parameters)
	{
		TransitPoint[] array = LocationScene.GetAllObjects<TransitPoint>().ToArray();
		foreach (TransitPoint transitPoint in array)
		{
			if (transitPoint == null)
			{
				continue;
			}
			if (parameters != null)
			{
				foreach (LocationSettingsClass.Location.TransitParameters transitParameters in parameters)
				{
					if (transitParameters.id == transitPoint.parameters.id)
					{
						transitPoint.parameters = transitParameters;
						break;
					}
				}
			}
			if (!transitPoint.parameters.active)
			{
				transitPoint.gameObject.SetActive(value: false);
				continue;
			}
			transitPoint.Controller = this;
			Dictionary_0.Add(transitPoint.parameters.id, transitPoint);
		}
	}

	public void method_1()
	{
		LocationOrigin[] array = LocationScene.GetAllObjects<LocationOrigin>().ToArray();
		foreach (LocationOrigin locationOrigin in array)
		{
			if (!(locationOrigin == null))
			{
				locationOrigins.Add(locationOrigin);
			}
		}
	}

	public void method_2(ICollection<TransitPoint> activePoints, Player player)
	{
		TransitDataClass transitDataClass = summonedTransits[player.ProfileId];
		if (transitDataClass == null || !transitDataClass.events || !RunddansControllerAbstractClass.Exist<RunddansControllerAbstractClass>(out var runddansController) || !runddansController.IsNonExitLocation())
		{
			return;
		}
		foreach (TransitPoint activePoint in activePoints)
		{
			if (!activePoint.parameters.events)
			{
				continue;
			}
			DisableExits(player);
			{
				foreach (KeyValuePair<int, EventObject> @object in runddansController.Objects)
				{
					@object.Deconstruct(out var _, out var value);
					value.OnFinish += delegate
					{
						EnableExits(player);
					};
				}
				break;
			}
		}
	}

	public void EnablePoints(bool exceptEvents = true)
	{
		foreach (TransitPoint value in Dictionary_0.Values)
		{
			value.Enabled = !exceptEvents || !value.parameters.events;
			if (!exceptEvents)
			{
				value.parameters.eventsEnable = true;
			}
		}
	}

	public void PauseEventPoints()
	{
		foreach (TransitPoint value in Dictionary_0.Values)
		{
			if (value.parameters.events && value.parameters.eventsEnable)
			{
				value.parameters.eventsEnable = false;
			}
		}
	}

	public static void DisableTransitPoints()
	{
		TransitPoint[] array = LocationScene.GetAllObjects<TransitPoint>().ToArray();
		foreach (TransitPoint transitPoint in array)
		{
			if (!(transitPoint == null))
			{
				transitPoint.gameObject.SetActive(value: false);
			}
		}
	}

	public static bool Exist<T>(out T transitController) where T : TransitControllerAbstractClass
	{
		if (Singleton<GameWorld>.Instance != null && Singleton<GameWorld>.Instance.TransitController is T val)
		{
			transitController = val;
			return true;
		}
		transitController = null;
		return false;
	}

	public static bool IsTransit(string profileId, out int count)
	{
		count = 0;
		if (Exist<TransitControllerAbstractClass>(out var transitController))
		{
			return transitController.IsTransitPlayer(profileId, out count);
		}
		return false;
	}

	public bool IsTransitPlayer(string profileId, out int count)
	{
		if (summonedTransits.TryGetValue(profileId, out var value))
		{
			count = value.count;
			return count > 0;
		}
		count = 0;
		return false;
	}

	public static bool IsTransit(string profileId, out string location)
	{
		location = null;
		if (Exist<TransitControllerAbstractClass>(out var transitController))
		{
			return transitController.IsTransitPlayer(profileId, out location);
		}
		return false;
	}

	public bool IsTransitPlayer(string profileId, out string location)
	{
		if (summonedTransits.TryGetValue(profileId, out var value) && value.maps.Length > 1)
		{
			location = value.maps[value.maps.Length - 2];
			return true;
		}
		location = null;
		return false;
	}

	public static bool TryGetTransitSpawnPoint(string profileId, List<ISpawnPoint> spawnPoints, IReadOnlyCollection<IPlayer> avoidPlayers, out ISpawnPoint spawnPoint, out string log)
	{
		Struct304 struct304_ = default(Struct304);
		struct304_.spawnPoints = spawnPoints;
		spawnPoint = null;
		log = string.Empty;
		if (struct304_.spawnPoints == null)
		{
			return false;
		}
		if (!Exist<TransitControllerAbstractClass>(out struct304_.controller))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(profileId) && struct304_.controller.IsTransitPlayer(profileId, out string location))
		{
			if (!struct304_.controller.method_5(location, out var origin))
			{
				return smethod_1(ref struct304_);
			}
			spawnPoint = struct304_.controller.method_3(struct304_.spawnPoints, origin, avoidPlayers);
			log = "Spawn point " + spawnPoint?.Name + " selected by transition from location: " + location;
			if (spawnPoint != null)
			{
				return struct304_.controller.usedSpawnPoints.Add(spawnPoint);
			}
			return false;
		}
		return smethod_1(ref struct304_);
	}

	public ISpawnPoint method_3(IEnumerable<ISpawnPoint> spawnPoints, LocationOrigin origin, IReadOnlyCollection<IPlayer> avoidPlayers)
	{
		float num = float.MaxValue;
		ISpawnPoint result = null;
		foreach (ISpawnPoint spawnPoint in spawnPoints)
		{
			if (!usedSpawnPoints.Contains(spawnPoint) && !method_4(spawnPoint) && !smethod_0(avoidPlayers, spawnPoint))
			{
				float distanceSqr = origin.GetDistanceSqr(spawnPoint.Position);
				if (!(distanceSqr >= num))
				{
					num = distanceSqr;
					result = spawnPoint;
				}
			}
		}
		return result;
	}

	public bool method_4(ISpawnPoint point)
	{
		foreach (ISpawnPoint usedSpawnPoint in usedSpawnPoints)
		{
			if (usedSpawnPoint.Collider.Contains(point.Position))
			{
				return true;
			}
		}
		return false;
	}

	public static bool smethod_0(IEnumerable<IPlayer> avoidPlayers, ISpawnPoint point)
	{
		foreach (IPlayer avoidPlayer in avoidPlayers)
		{
			if (point.Collider.Contains(avoidPlayer.Position))
			{
				return true;
			}
		}
		return false;
	}

	public bool method_5(string location, out LocationOrigin origin)
	{
		foreach (LocationOrigin locationOrigin in locationOrigins)
		{
			if (locationOrigin.ContainLocation(location))
			{
				origin = locationOrigin;
				return true;
			}
		}
		origin = null;
		return false;
	}

	public bool TryGetMarathon(MongoID profileId, out TransitDataClass transit)
	{
		return summonedTransits.TryGetValue(profileId, out transit);
	}

	public virtual bool DisableExits(Player player)
	{
		return ExfiltrationControllerClass.Instance.BannedPlayers.Add(player.Id);
	}

	public virtual bool EnableExits(Player player)
	{
		return ExfiltrationControllerClass.Instance.BannedPlayers.Remove(player.Id);
	}

	public TransitInteractionPacketStruct GetInteractPacket(int transitPointId, string keyId, EDateTime time)
	{
		return new TransitInteractionPacketStruct
		{
			hasInteraction = true,
			pointId = transitPointId,
			keyId = keyId,
			time = time
		};
	}

	public virtual void InteractWithTransit(Player player, TransitInteractionPacketStruct packet)
	{
	}

	public abstract void InactivePointNotification(int playerId, int pointId);

	public abstract void Sizes(Dictionary<int, byte> sizes);

	public abstract void Timers(int pointId, Dictionary<int, ushort> timers);

	public virtual void Transit(TransitPoint point, int playersCount, string hash, Dictionary<string, ProfileKey> keys, Player player)
	{
	}

	public virtual void Dispose()
	{
		Dictionary_0.Clear();
		transitPlayers.Clear();
		summonedTransits.Clear();
		alreadyTransits.Clear();
		profileKeys.Clear();
		profileTime.Clear();
		locationOrigins.Clear();
		usedSpawnPoints.Clear();
	}

	[CompilerGenerated]
	public static bool smethod_1(ref Struct304 struct304_0)
	{
		foreach (ISpawnPoint usedSpawnPoint in struct304_0.controller.usedSpawnPoints)
		{
			struct304_0.spawnPoints.Remove(usedSpawnPoint);
		}
		return false;
	}
}

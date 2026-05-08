using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Systems.Effects;
using Comfort.Common;
using Diz.LanguageExtensions;
using EFT.AssetsManager;
using EFT.Ballistics;
using EFT.CameraControl;
using EFT.Game.Spawning;
using EFT.GameTriggers;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.MovingPlatforms;
using EFT.NextObservedPlayer;
using EFT.Quests;
using EFT.SynchronizableObjects;
using JetBrains.Annotations;
using UnityEngine;

namespace EFT;

[GAttribute8(-2000)]
public abstract class GameWorld : MonoBehaviour, GInterface169, IPlayersCollection, IEnumerable<IPlayer>, IEnumerable, IDisposable, GInterface278
{
	public struct GStruct162
	{
		public Transform Transform;

		public bool DoNotPerformPickUpValidation;

		public IItemOwner ItemOwner;
	}

	public class GClass1524 : Error
	{
		public readonly string ItemOwnerId;

		public GClass1524(string itemOwnerId)
		{
			ItemOwnerId = itemOwnerId;
		}

		public override string ToString()
		{
			return "Could not find item owner with id: " + ItemOwnerId;
		}
	}

	public class GClass1525 : Error
	{
		[NonSerialized]
		public string String_0;

		[NonSerialized]
		public string String_1;

		public GClass1525(string parentId, string containerId)
		{
			String_0 = parentId;
			String_1 = containerId;
		}

		public override string ToString()
		{
			return "Could not find item address with id. ParentId: " + String_0 + ", ContainerId: " + String_1;
		}
	}

	public class GClass1526 : Error
	{
		public readonly string ItemId;

		public GClass1526(string itemId)
		{
			ItemId = itemId;
		}

		public override string ToString()
		{
			return "Could not find item with id: " + ItemId;
		}
	}

	public class GClass1527 : Error
	{
		public readonly Item Item;

		public readonly Vector3 PlayerPosition;

		public readonly Vector3 ItemPosition;

		public GClass1527(Item item, Vector3 playerPosition, Vector3 itemPosition)
		{
			Item = item;
			PlayerPosition = playerPosition;
			ItemPosition = itemPosition;
		}

		public override string ToString()
		{
			return $"{Item} at {ItemPosition} is too far away from {PlayerPosition}";
		}
	}

	public class GClass1528 : Error
	{
		public readonly Item Item;

		public readonly Player CurrentPlayer;

		[CanBeNull]
		public readonly Player FromPlayer;

		[CanBeNull]
		public readonly Player ToPlayer;

		public readonly bool KnownDirection;

		public GClass1528(Player currentPlayer, Item item, [CanBeNull] Player fromPlayer, [CanBeNull] Player toPlayer)
		{
			Item = item;
			CurrentPlayer = currentPlayer;
			FromPlayer = fromPlayer;
			ToPlayer = toPlayer;
			KnownDirection = true;
		}

		public GClass1528(Player currentPlayer, Player anotherPlayer, Item item)
		{
			Item = item;
			CurrentPlayer = currentPlayer;
			ToPlayer = anotherPlayer;
			KnownDirection = false;
		}

		public override string ToString()
		{
			if (!KnownDirection)
			{
				return $"Player {CurrentPlayer.FullIdInfo} attempts to manipulate with {Item} in {method_0(ToPlayer)} inventory.";
			}
			return $"Player {CurrentPlayer.FullIdInfo} transfers {Item} from {method_0(FromPlayer)} to {method_0(ToPlayer)}.";
		}

		[CompilerGenerated]
		public string method_0(Player player)
		{
			if (!(player == CurrentPlayer))
			{
				if (!(player == null))
				{
					return "alive player " + player.FullIdInfo;
				}
				return "external container";
			}
			return "{SELF}";
		}
	}

	public class GClass1529 : Error
	{
		public readonly Player Player;

		public GClass1529(Player player)
		{
			Player = player;
		}

		public override string ToString()
		{
			return "Player " + Player.FullIdInfo + " transfers his Equipment to another player.";
		}
	}

	public class GClass1530 : Error
	{
		public readonly Player Player;

		public GClass1530(Player player)
		{
			Player = player;
		}

		public override string ToString()
		{
			return "Player " + Player.FullIdInfo + " try to pick up OneOff break weapon.";
		}
	}

	public class GClass1531 : Error
	{
		public readonly IItemOwner ItemOwner;

		public readonly Item Item;

		public readonly Vector3 OwnerPosition;

		public readonly Vector3 ItemPosition;

		public GClass1531(IItemOwner itemOwner, Item item, Vector3 ownerPosition, Vector3 itemPosition)
		{
			ItemOwner = itemOwner;
			Item = item;
			OwnerPosition = ownerPosition;
			ItemPosition = itemPosition;
		}

		public override string ToString()
		{
			return $" {Item}  at {ItemPosition} is too far away from {ItemOwner} at {OwnerPosition}";
		}
	}

	public class GClass1532 : Error
	{
		public readonly IItemOwner ItemOwner;

		public readonly Item Item;

		public readonly Vector3 OwnerPosition;

		public readonly Vector3 ItemPosition;

		public string ColliderName;

		public GClass1532(IItemOwner itemOwner, Item item, Vector3 ownerPosition, Vector3 itemPosition, string colliderName)
		{
			ItemOwner = itemOwner;
			Item = item;
			OwnerPosition = ownerPosition;
			ItemPosition = itemPosition;
			ColliderName = colliderName;
		}

		public override string ToString()
		{
			return $" {Item}  at {ItemPosition} is blocked from {ItemOwner} at {OwnerPosition} with {ColliderName}";
		}
	}

	public class GClass1533 : Error
	{
		public readonly IItemOwner ItemOwner;

		public readonly Item Item;

		public readonly Vector3 OwnerPosition;

		public readonly Vector3 ItemPosition;

		public GClass1533(IItemOwner itemOwner, Item item, Vector3 ownerPosition, Vector3 itemPosition)
		{
			ItemOwner = itemOwner;
			Item = item;
			OwnerPosition = ownerPosition;
			ItemPosition = itemPosition;
		}

		public override string ToString()
		{
			return $" {Item}  at {ItemPosition} cant find by {ItemOwner} at {OwnerPosition}";
		}
	}

	public class GClass1534 : Error
	{
		public readonly Item Item;

		public GClass1534(Item item)
		{
			Item = item;
		}

		public override string ToString()
		{
			return "Owner of " + Item?.ToString() + " is null";
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1111
	{
		public static readonly Class1111 class1111_0 = new Class1111();

		public static Func<IPlayer, bool> func_0;

		public static Func<string, ResourceKey> func_1;

		public static Func<GInterface30, bool> func_2;

		public static Func<Transform, string> func_3;

		public static Func<Item, MongoID> func_4;

		public static Func<QuestClass, bool> func_5;

		public static Func<(Item item, GStruct162 data), Item> func_6;

		public static Func<GClass3735> func_7;

		public static Action<GClass3735> action_0;

		public bool method_0(IPlayer x)
		{
			return x.HealthController.IsAlive;
		}

		public ResourceKey method_1(string x)
		{
			return new ResourceKey
			{
				path = x
			};
		}

		public bool method_2(GInterface30 x)
		{
			return x != null;
		}

		public string method_3(Transform x)
		{
			return x.name;
		}

		public MongoID method_4(Item x)
		{
			return x.TemplateId;
		}

		public bool method_5(QuestClass quest)
		{
			return quest.QuestStatus == EQuestStatus.Started;
		}

		public Item method_6((Item item, GStruct162 data) e)
		{
			return e.item;
		}

		public GClass3735 method_7()
		{
			return new GClass3735();
		}

		public void method_8(GClass3735 calculator)
		{
			calculator.ClearClass();
		}
	}

	[CompilerGenerated]
	public class Class1112
	{
		public string id;

		public bool method_0(LootItem x)
		{
			return x.ItemId.Equals(id);
		}
	}

	[CompilerGenerated]
	public class Class1113
	{
		public string lootId;

		public bool method_0(GInterface30 x)
		{
			return x.Id == lootId;
		}
	}

	[CompilerGenerated]
	public class Class1114
	{
		public GameWorld gameWorld_0;

		public string id;

		public void method_0(LootableContainer container)
		{
			if (!GClass856.IsNullOrEmpty(container.GameObjectsToDestroy))
			{
				foreach (GameObject item in container.GameObjectsToDestroy)
				{
					gameWorld_0.list_2.Add(item);
					item.SetActive(value: false);
				}
				return;
			}
			Transform parent = container.transform.parent;
			gameWorld_0.list_2.Add(parent.gameObject);
			parent.gameObject.SetActive(value: false);
		}

		public bool method_1(LootableContainer x)
		{
			return x.Id.Equals(id);
		}
	}

	[CompilerGenerated]
	public class Class1115
	{
		public GInterface30 staticLootSpawn;

		public bool method_0(LootItemPositionClass lootItem)
		{
			return lootItem.Id != staticLootSpawn.Id;
		}
	}

	[CompilerGenerated]
	public class Class1116
	{
		public string id;

		public bool method_0(StationaryWeapon x)
		{
			return x.Id == id;
		}
	}

	[CompilerGenerated]
	public class Class1117
	{
		public string itemId;

		public bool method_0(StationaryWeapon x)
		{
			return x.Item.Id == itemId;
		}
	}

	[CompilerGenerated]
	public class Class1118
	{
		public string ownerId;

		public bool method_0(KeyValuePair<IItemOwner, GStruct162> x)
		{
			return x.Key.ID == ownerId;
		}
	}

	[CompilerGenerated]
	private static Action action_0;

	[CompilerGenerated]
	private TransitControllerAbstractClass transitControllerAbstractClass;

	[CompilerGenerated]
	private RunddansControllerAbstractClass runddansControllerAbstractClass;

	[CompilerGenerated]
	private ExfiltrationControllerClass exfiltrationControllerClass;

	[CompilerGenerated]
	private BufferZoneControllerClass bufferZoneControllerClass;

	[CompilerGenerated]
	private HalloweenEventControllerClass halloweenEventControllerClass;

	[CompilerGenerated]
	private GInterface29 ginterface29_0;

	[CompilerGenerated]
	private BTRControllerClass bTRControllerClass;

	[CompilerGenerated]
	private TriggersModule triggersModule_0;

	[CompilerGenerated]
	private GClass3592 gclass3592_0;

	[CompilerGenerated]
	private GClass702 gclass702_0;

	public readonly GClass2272 TraderServicesHandler = new GClass2272();

	[CompilerGenerated]
	private ServerShellingControllerClass serverShellingControllerClass;

	[CompilerGenerated]
	private ClientShellingControllerClass clientShellingControllerClass;

	[CompilerGenerated]
	private ClientBroadcastSyncControllerClass clientBroadcastSyncControllerClass;

	[CompilerGenerated]
	private string string_0;

	public GameDateTime GameDateTime;

	public GameWorldUnityTickListener UnityTickListener;

	public bool SpeedLimitsEnabled;

	public GClass2175.Config SpeedLimits = GClass2175.DefaultSpeedLimits;

	public MongoID? CurrentProfileId;

	[CompilerGenerated]
	private Action action_1;

	[CompilerGenerated]
	private Action<IPlayer> action_2;

	[CompilerGenerated]
	private Action<float> action_3;

	[CompilerGenerated]
	private Action<float> action_4;

	private readonly GClass824 gclass824_0 = new GClass824();

	public AudioSourceCulling AudioSourceCulling;

	public GClass835<GClass3735> TrajectoryCalculatorPool = new GClass835<GClass3735>(40, () => new GClass3735(), delegate(GClass3735 calculator)
	{
		calculator.ClearClass();
	});

	[CompilerGenerated]
	private ISharedBallisticsCalculator iSharedBallisticsCalculator;

	private BallisticsCalculator ballisticsCalculator_0;

	public readonly List<IKillableLootItem> LootList = new List<IKillableLootItem>(1000);

	public readonly Dictionary<IItemOwner, GStruct162> ItemOwners = new Dictionary<IItemOwner, GStruct162>(100);

	public readonly GClass818<int, LootItem> LootItems = new GClass818<int, LootItem>(1000);

	public readonly Dictionary<int, ObservedCorpse> ObservedPlayersCorpses = new Dictionary<int, ObservedCorpse>(60);

	public readonly List<LootItemPositionClass> AllLoot = new List<LootItemPositionClass>();

	public readonly List<IPlayer> RegisteredPlayers = new List<IPlayer>(40);

	private readonly Dictionary<string, Player> dictionary_0 = new Dictionary<string, Player>();

	private readonly Dictionary<string, LampController> dictionary_1 = new Dictionary<string, LampController>();

	private readonly Dictionary<ulong, ControlledLampGroup> dictionary_2 = new Dictionary<ulong, ControlledLampGroup>();

	public readonly Dictionary<string, Player> allAlivePlayersByID = new Dictionary<string, Player>();

	public readonly List<Player> AllAlivePlayersList = new List<Player>();

	protected readonly Dictionary<string, IPlayerOwner> AllAlivePlayerBridges = new Dictionary<string, IPlayerOwner>();

	protected readonly Dictionary<Collider, IPlayerOwner> AllAlivePlayerBridgesByCollider = new Dictionary<Collider, IPlayerOwner>();

	protected readonly Dictionary<string, IPlayerOwner> AllPlayerBridgesEverExisted = new Dictionary<string, IPlayerOwner>();

	public Dictionary<string, ObservedPlayerView> allObservedPlayersByID = new Dictionary<string, ObservedPlayerView>();

	public MovingPlatform.GClass3596[] PlatformAdapters = Array.Empty<MovingPlatform.GClass3596>();

	public BorderZone[] BorderZones;

	private MovingPlatform[] movingPlatform_0;

	public Player MainPlayer;

	protected readonly CompositeDisposableClass CompositeDisposable = new CompositeDisposableClass();

	[CompilerGenerated]
	private readonly Dictionary<Collider, IPlayer> dictionary_3 = new Dictionary<Collider, IPlayer>(40);

	private readonly List<AmmoPoolObject> list_0 = new List<AmmoPoolObject>(100);

	private readonly List<LootItemPositionClass> list_1 = new List<LootItemPositionClass>();

	protected static int _interactiveLootMask;

	private static int int_0;

	private static int int_1;

	private static int int_2;

	public SpeakerManager SpeakerManager;

	private static int int_3;

	private EUpdateQueue eupdateQueue_0;

	[CompilerGenerated]
	private SyncObjectProcessorClass syncObjectProcessorClass;

	public GClass705 MineManager;

	[CompilerGenerated]
	private Action<IKillableLootItem> action_5;

	protected readonly Dictionary<int, Turnable> Turnables = new Dictionary<int, Turnable>(512);

	public readonly GClass818<int, WindowBreaker> Windows = new GClass818<int, WindowBreaker>(512);

	public readonly GClass818<int, Throwable> Grenades = new GClass818<int, Throwable>();

	public List<GrenadeDataPacketStruct> GrenadesCriticalStates = new List<GrenadeDataPacketStruct>();

	public List<ArtilleryPacketStruct> ArtilleryProjectilesStates = new List<ArtilleryPacketStruct>();

	private GrenadeFactoryClass grenadeFactoryClass;

	private bool bool_0;

	private global::DependencyGraphClass<IEasyBundle>.GClass1661 gclass1661_0;

	public BotEventHandler.GDelegate15 OnThrowItem;

	public BotEventHandler.GDelegate16 OnTakeItem;

	private readonly List<GameObject> list_2 = new List<GameObject>(100);

	private BaseRestrictableZone[] baseRestrictableZone_0;

	protected EffectsCommutator _effectsCommutator;

	private BallisticCollider ballisticCollider_0;

	internal World world_0;

	private World world_1;

	protected PoolManagerClass ObjectsFactory;

	private List<WorldInteractiveObject> list_3 = new List<WorldInteractiveObject>();

	[CanBeNull]
	public TransitControllerAbstractClass TransitController
	{
		[CompilerGenerated]
		get
		{
			return transitControllerAbstractClass;
		}
		[CompilerGenerated]
		set
		{
			transitControllerAbstractClass = value;
		}
	}

	[CanBeNull]
	public RunddansControllerAbstractClass RunddansController
	{
		[CompilerGenerated]
		get
		{
			return runddansControllerAbstractClass;
		}
		[CompilerGenerated]
		set
		{
			runddansControllerAbstractClass = value;
		}
	}

	public ExfiltrationControllerClass ExfiltrationController
	{
		[CompilerGenerated]
		get
		{
			return exfiltrationControllerClass;
		}
		[CompilerGenerated]
		set
		{
			exfiltrationControllerClass = value;
		}
	}

	public BufferZoneControllerClass BufferZoneController
	{
		[CompilerGenerated]
		get
		{
			return bufferZoneControllerClass;
		}
		[CompilerGenerated]
		set
		{
			bufferZoneControllerClass = value;
		}
	}

	public HalloweenEventControllerClass HalloweenEventController
	{
		[CompilerGenerated]
		get
		{
			return halloweenEventControllerClass;
		}
		[CompilerGenerated]
		set
		{
			halloweenEventControllerClass = value;
		}
	}

	public GInterface29 GInterface29_0
	{
		[CompilerGenerated]
		get
		{
			return ginterface29_0;
		}
		[CompilerGenerated]
		set
		{
			ginterface29_0 = value;
		}
	}

	public BTRControllerClass BtrController
	{
		[CompilerGenerated]
		get
		{
			return bTRControllerClass;
		}
		[CompilerGenerated]
		set
		{
			bTRControllerClass = value;
		}
	}

	public TriggersModule TriggersModule
	{
		[CompilerGenerated]
		get
		{
			return triggersModule_0;
		}
		[CompilerGenerated]
		set
		{
			triggersModule_0 = value;
		}
	}

	public GClass3592 TriggersEmitter
	{
		[CompilerGenerated]
		get
		{
			return gclass3592_0;
		}
		[CompilerGenerated]
		set
		{
			gclass3592_0 = value;
		}
	}

	public GClass702 SyncModule
	{
		[CompilerGenerated]
		get
		{
			return gclass702_0;
		}
		[CompilerGenerated]
		set
		{
			gclass702_0 = value;
		}
	}

	public ServerShellingControllerClass ServerShellingController
	{
		[CompilerGenerated]
		get
		{
			return serverShellingControllerClass;
		}
		[CompilerGenerated]
		set
		{
			serverShellingControllerClass = value;
		}
	}

	public ClientShellingControllerClass ClientShellingController
	{
		[CompilerGenerated]
		get
		{
			return clientShellingControllerClass;
		}
		[CompilerGenerated]
		set
		{
			clientShellingControllerClass = value;
		}
	}

	public ClientBroadcastSyncControllerClass ClientBroadcastSyncController
	{
		[CompilerGenerated]
		get
		{
			return clientBroadcastSyncControllerClass;
		}
		[CompilerGenerated]
		set
		{
			clientBroadcastSyncControllerClass = value;
		}
	}

	public string LocationId
	{
		[CompilerGenerated]
		get
		{
			return string_0;
		}
		[CompilerGenerated]
		set
		{
			string_0 = value;
		}
	}

	public float DeltaTime
	{
		get
		{
			float num = gclass824_0.DeltaTime;
			if (num > 1f)
			{
				num = 1f;
			}
			return num;
		}
	}

	public ISharedBallisticsCalculator ClientBallisticCalculator
	{
		[CompilerGenerated]
		get
		{
			return iSharedBallisticsCalculator;
		}
		[CompilerGenerated]
		set
		{
			iSharedBallisticsCalculator = value;
		}
	}

	public ISharedBallisticsCalculator SharedBallisticsCalculator
	{
		get
		{
			return ballisticsCalculator_0;
		}
		set
		{
			ballisticsCalculator_0 = value as BallisticsCalculator;
		}
	}

	public IEnumerable<Player> AllPlayersEverExisted => dictionary_0.Values;

	public MovingPlatform[] Platforms
	{
		get
		{
			return movingPlatform_0 ?? (movingPlatform_0 = LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<MovingPlatform>().ToArray());
		}
		set
		{
			movingPlatform_0 = value;
		}
	}

	public Dictionary<Collider, IPlayer> PlayersColliders
	{
		[CompilerGenerated]
		get
		{
			return dictionary_3;
		}
	}

	public static int InteractiveLootMaskWPlayer => int_0;

	public static int LootMaskObstruction => int_2;

	public SyncObjectProcessorClass SynchronizableObjectLogicProcessor
	{
		[CompilerGenerated]
		get
		{
			return syncObjectProcessorClass;
		}
		[CompilerGenerated]
		set
		{
			syncObjectProcessorClass = value;
		}
	}

	public EUpdateQueue UpdateQueue => eupdateQueue_0;

	public GrenadeFactoryClass GrenadeFactory => grenadeFactoryClass;

	public BaseRestrictableZone[] RestrictableZones => baseRestrictableZone_0;

	public World World_0 => world_1 ?? (world_1 = GetComponent<World>());

	public virtual ulong TotalOutgoingBytes => 0uL;

	public virtual ulong TotalIngoingBytes => 0uL;

	public static event Action OnDispose
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

	public event Action AfterGameStarted
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

	public event Action<IPlayer> OnPersonAdd
	{
		[CompilerGenerated]
		add
		{
			Action<IPlayer> action = action_2;
			Action<IPlayer> action2;
			do
			{
				action2 = action;
				Action<IPlayer> value2 = (Action<IPlayer>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IPlayer> action = action_2;
			Action<IPlayer> action2;
			do
			{
				action2 = action;
				Action<IPlayer> value2 = (Action<IPlayer>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<float> OnBeforeWorldTick
	{
		[CompilerGenerated]
		add
		{
			Action<float> action = action_3;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<float> action = action_3;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<float> OnLateUpdate
	{
		[CompilerGenerated]
		add
		{
			Action<float> action = action_4;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<float> action = action_4;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<IKillableLootItem> OnLootItemDestroyed
	{
		[CompilerGenerated]
		add
		{
			Action<IKillableLootItem> action = action_5;
			Action<IKillableLootItem> action2;
			do
			{
				action2 = action;
				Action<IKillableLootItem> value2 = (Action<IKillableLootItem>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IKillableLootItem> action = action_5;
			Action<IKillableLootItem> action2;
			do
			{
				action2 = action;
				Action<IKillableLootItem> value2 = (Action<IKillableLootItem>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static TGameWorld Create<TGameWorld>(GameObject gameObject, PoolManagerClass objectsFactory, EUpdateQueue updateQueue, MongoID? currentProfileId) where TGameWorld : GameWorld
	{
		TGameWorld val = gameObject.AddComponent<TGameWorld>();
		val.ObjectsFactory = objectsFactory;
		val.eupdateQueue_0 = updateQueue;
		val.SpeakerManager = gameObject.AddComponent<SpeakerManager>();
		val.ExfiltrationController = new ExfiltrationControllerClass();
		val.BufferZoneController = new BufferZoneControllerClass();
		val.CurrentProfileId = currentProfileId;
		val.UnityTickListener = GameWorldUnityTickListener.Create(gameObject, val);
		val.AudioSourceCulling = GClass6.GetOrAddComponent<AudioSourceCulling>(gameObject);
		return val;
	}

	public Player GetEverExistedPlayerByID(string profileID)
	{
		if (dictionary_0.TryGetValue(profileID, out var value))
		{
			return value;
		}
		return null;
	}

	public ObservedPlayerView GetObservedPlayerByProfileID(string profileID)
	{
		if (allObservedPlayersByID.TryGetValue(profileID, out var value))
		{
			return value;
		}
		return null;
	}

	public bool TryGetObservedPlayer(int id, out ObservedPlayerView observedPlayer)
	{
		foreach (var (_, observedPlayerView2) in allObservedPlayersByID)
		{
			if (observedPlayerView2.Id == id)
			{
				observedPlayer = observedPlayerView2;
				return true;
			}
		}
		observedPlayer = null;
		return false;
	}

	public bool TryGetAlivePlayer(int id, out Player alivePlayer)
	{
		foreach (var (_, player2) in allAlivePlayersByID)
		{
			if (player2.Id == id)
			{
				alivePlayer = player2;
				return true;
			}
		}
		alivePlayer = null;
		return false;
	}

	public void FillLampControllers()
	{
		dictionary_1.Clear();
		foreach (LampController allObject in LocationScene.GetAllObjects<LampController>())
		{
			dictionary_1[allObject.Id] = allObject;
		}
		foreach (ControlledLampGroup allObject2 in LocationScene.GetAllObjects<ControlledLampGroup>())
		{
			dictionary_2[allObject2.ID] = allObject2;
		}
	}

	public bool TryGetAlivePlayerByID(int id, out IPlayer player)
	{
		foreach (var (_, playerOwner2) in AllAlivePlayerBridges)
		{
			if (playerOwner2.iPlayer.Id == id)
			{
				player = playerOwner2.iPlayer;
				return true;
			}
		}
		player = null;
		return false;
	}

	public virtual SyncObjectProcessorClass SyncObjectProcessorFactory()
	{
		return new SyncObjectProcessorClass();
	}

	public IPlayerOwner GetEverExistedBridgeByProfileID(string profileID)
	{
		if (profileID == null)
		{
			return null;
		}
		if (AllPlayerBridgesEverExisted.TryGetValue(profileID, out var value))
		{
			return value;
		}
		return null;
	}

	public IPlayerOwner GetAlivePlayerBridgeByProfileID(string profileID)
	{
		if (profileID == null)
		{
			return null;
		}
		if (AllAlivePlayerBridges.TryGetValue(profileID, out var value))
		{
			return value;
		}
		return null;
	}

	public IPlayerOwner GetAlivePlayerBridgeByCollider(Collider col)
	{
		if (AllAlivePlayerBridgesByCollider.TryGetValue(col, out var value))
		{
			return value;
		}
		return null;
	}

	public Player GetAlivePlayerByProfileID(string profileID)
	{
		if (allAlivePlayersByID.TryGetValue(profileID, out var value))
		{
			return value;
		}
		return null;
	}

	public IEnumerator<IPlayer> GetEnumerator()
	{
		return RegisteredPlayers.Where((IPlayer x) => x.HealthController.IsAlive).GetEnumerator();
	}

	public IEnumerator GetEnumerator_1()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetEnumerator_1
		return this.GetEnumerator_1();
	}

	[CanBeNull]
	public MovingPlatform GetPlatformAtPosition(Vector3 position)
	{
		MovingPlatform[] platforms = Platforms;
		int num = 0;
		MovingPlatform movingPlatform;
		while (true)
		{
			if (num < platforms.Length)
			{
				movingPlatform = platforms[num];
				if (movingPlatform.Area != null && GClass860.ContainsPointConsiderCenter(movingPlatform.Area, position))
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return movingPlatform;
	}

	public void BoardIfOnPlatform(MovingPlatform.GInterface459 transportee, Vector3 position)
	{
		MovingPlatform platformAtPosition = GetPlatformAtPosition(position);
		if ((bool)platformAtPosition)
		{
			transportee.Board(platformAtPosition);
		}
	}

	public AmmoPoolObject SpawnShellInTheWorld(ref AmmoPoolObject shell)
	{
		AmmoPoolObject ammoPoolObject = shell;
		shell = null;
		list_0.Add(ammoPoolObject);
		return ammoPoolObject;
	}

	public ISharedBallisticsCalculator CreateBallisticCalculator(int seed)
	{
		return SharedBallisticsCalculator;
	}

	ISharedBallisticsCalculator GInterface169.CreateBallisticCalculator(int seed)
	{
		//ILSpy generated this explicit interface implementation from .override directive in CreateBallisticCalculator
		return this.CreateBallisticCalculator(seed);
	}

	public void RemoveBallisticCalculator(Item weapon)
	{
	}

	public ISharedBallisticsCalculator CreateClientBallisticsCalculator()
	{
		if (ClientBallisticCalculator == null)
		{
			ClientBallisticCalculator = BallisticsCalculator.Create(base.gameObject, 0, ShotDelegate);
		}
		return ClientBallisticCalculator;
	}

	public void RemoveClientBallisticsCalculator()
	{
	}

	public void RegisterGrenade(Throwable grenade)
	{
		method_0(grenade);
		try
		{
			Grenades.Add(grenade.Id, grenade);
			grenade.DestroyEvent += method_1;
		}
		catch (Exception)
		{
			Debug.LogError($"There is a problem to add the grenade: grenade.Id:{grenade.Id}");
			if (Grenades.TryGetByKey(grenade.Id, out var value))
			{
				Debug.LogError($"Registered: throwable.Id:{value.Id} ");
			}
		}
	}

	public void method_0(Throwable grenade)
	{
		grenade.DestroyEvent -= method_1;
		method_2(grenade);
		Grenades.Remove(grenade.Id);
	}

	public void UnregisterGrenade(string id)
	{
		int stableHashCode = GClass1298.GetStableHashCode(id);
		if (Grenades.ContainsKey(stableHashCode))
		{
			Grenades.Remove(stableHashCode);
		}
	}

	public void method_1(Throwable grenade)
	{
		method_0(grenade);
	}

	public void method_2(Throwable grenade)
	{
		if (grenade.HasNetData)
		{
			GrenadeDataPacketStruct netPacket = grenade.GetNetPacket();
			GrenadesCriticalStates.Add(netPacket);
		}
	}

	public virtual void Awake()
	{
		Grenade.GrenadeRandoms = new GClass3725(128, 1337);
		grenadeFactoryClass = CreateGrenadeFactory();
		foreach (WorldInteractiveObject allObject in LocationScene.GetAllObjects<WorldInteractiveObject>())
		{
			allObject.OnDoorStateChanged += method_13;
		}
	}

	public virtual GrenadeFactoryClass CreateGrenadeFactory()
	{
		return new GrenadeFactoryClass();
	}

	public virtual void vmethod_0(World world)
	{
		world_0 = world;
		ExfiltrationController.StatusChanged += world_0.OnExfiltrationPointUpdate;
	}

	public void RegisterInteractiveObject(Turnable t)
	{
		if (!Turnables.ContainsKey(t.NetId))
		{
			Turnables.Add(t.NetId, t);
		}
		else
		{
			Debug.LogError($"Already registered object - Turnable.NetId:{t.NetId} {t.gameObject.name}");
		}
	}

	public virtual void ChangeLampState(Turnable turnable, Turnable.EState state)
	{
		turnable.Switch(state);
	}

	public void RegisterWindow(WindowBreaker window)
	{
		if (!Windows.TryGetByKey(window.NetId, out var value))
		{
			Windows.Add(window.NetId, window);
			return;
		}
		Debug.LogError("Already registered WindowBreaker's Id:" + window.Id + " Scene:" + value.gameObject.scene.name + " Path:" + GClass6.GetFullPath(value.transform), window);
	}

	public virtual bool IsLocalGame()
	{
		return false;
	}

	public virtual void InitAirdrop(string lootTemplateId = null, bool takeNearbyPoint = false, Vector3 position = default(Vector3))
	{
	}

	public void RegisterWorldInteractionObject(WorldInteractiveObject worldInteractiveObject)
	{
		World_0.RegisterWorldInteractionObject(worldInteractiveObject);
	}

	public virtual async Task InitLevel(ItemFactoryClass itemFactory, ObjectsFactoryDataClass config, bool loadBundlesAndCreatePools = true, List<ResourceKey> resources = null, IProgress<LoadingProgressStruct> progress = null, CancellationToken ct = default(CancellationToken))
	{
		GClass640.LogMemoryConsumption("InitLevel {");
		EftBulletClass.FillPool();
		if (base.transform.parent == null)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		SynchronizableObjectLogicProcessor = SyncObjectProcessorFactory();
		await PreloadAdditionalData();
		await EFTHardSettings.Load();
		GClass2608.Int();
		using (CounterCreatorAbstractClass.StartWithToken("InitializeEffects"))
		{
			await method_3();
		}
		bool_0 = loadBundlesAndCreatePools;
		if (bool_0)
		{
			using (CounterCreatorAbstractClass.StartWithToken("RegisterLoadBundlesAndCreatePools"))
			{
				if (resources == null)
				{
					resources = new List<ResourceKey>();
				}
				resources.AddRange(ResourceKeyManagerAbstractClass.BUNDLES_TO_PRELOAD.Select((string x) => new ResourceKey
				{
					path = x
				}));
				resources.AddRange(ResourceKeyManagerAbstractClass.SynchronizableObjectPath.Values);
				await ObjectsFactory.RegisterLoadBundlesAndCreatePools(PoolManagerClass.PoolsCategory.Raid, base.transform, config, (!IsLocalGame()) ? PoolManagerClass.AssemblyType.Online : PoolManagerClass.AssemblyType.Local, resources.ToArray(), JobPriorityClass.General, progress, ct);
			}
		}
		SharedBallisticsCalculator = BallisticsCalculator.Create(base.gameObject, 0, ShotDelegate);
		ballisticCollider_0 = SharedBallisticsCalculator.DefaultHitBody;
		ballisticCollider_0.transform.SetParent(base.transform, worldPositionStays: true);
		Singleton<BetterAudio>.Instance.LoadSoundBundles();
		GClass640.LogMemoryConsumption("InitLevel }");
		try
		{
			BallisticCalculatorPrewarmer ballisticCalculatorPrewarmer = base.gameObject.AddComponent<BallisticCalculatorPrewarmer>();
			EftBulletClass eftBulletClass = ballisticCalculatorPrewarmer.SimulateShot();
			ShotDelegate(eftBulletClass);
			UnityEngine.Object.Destroy(ballisticCalculatorPrewarmer);
			EftBulletClass.Release(eftBulletClass);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed prewarm shot delegate because " + ex);
		}
		MineManager = new GClass705();
	}

	public async Task method_3()
	{
		gclass1661_0 = GClass1857.Retain(Singleton<IEasyAssets>.Instance, new string[1] { "assets/systems/effects/particlesystems/effects.bundle" });
		await GClass1857.LoadBundles(gclass1661_0);
		GameObject obj = GClass1857.InstantiateAsset<GameObject>(Singleton<IEasyAssets>.Instance, "assets/systems/effects/particlesystems/effects.bundle");
		obj.transform.SetParent(base.transform, worldPositionStays: true);
		obj.transform.position = Vector3.zero;
		obj.transform.rotation = Quaternion.identity;
		if (Singleton<Effects>.Instantiated)
		{
			if (Singleton<Effects>.Instance.gameObject != null)
			{
				UnityEngine.Object.DestroyImmediate(Singleton<Effects>.Instance.gameObject);
			}
			Singleton<Effects>.Release(Singleton<Effects>.Instance);
		}
		Singleton<Effects>.Create(obj.GetComponent<Effects>());
		Singleton<Effects>.Instance.CacheEffects();
	}

	public void RegisterLoot<T>(T loot) where T : InteractableObject
	{
		LootList.Add(loot);
		if (loot is LootItem lootItem)
		{
			ItemOwners.Add(lootItem.ItemOwner, new GStruct162
			{
				Transform = loot.TrackableTransform,
				DoNotPerformPickUpValidation = !lootItem.PerformPickUpValidation,
				ItemOwner = lootItem.ItemOwner
			});
			int netId = lootItem.GetNetId();
			try
			{
				LootItems.Add(netId, lootItem);
			}
			catch (Exception)
			{
				Debug.LogError($"there is a problem to add the loot item: lootItem.ItemId:{lootItem.ItemId} lootItem.Item.Id:{lootItem.Item.Id} netId:{netId} owner:{lootItem.ItemOwner} lootItem:{lootItem}");
				if (LootItems.TryGetByKey(netId, out var value))
				{
					Debug.LogError($"registered: exists.ItemId:{value.ItemId} exists.Item.Id{value.Item.Id} netId:{value.GetNetId()} owner:{value.ItemOwner} exists:{value}");
				}
			}
		}
		if (loot is LootableContainer lootableContainer)
		{
			ItemOwners.Add(lootableContainer.ItemOwner, new GStruct162
			{
				Transform = lootableContainer.TrackableTransform
			});
		}
	}

	public void DestroyAllLoot()
	{
		LootItem[] array = LootList.OfType<LootItem>().ToArray();
		foreach (LootItem loot in array)
		{
			DestroyLoot(loot);
		}
	}

	public void DestroyLoot(string id)
	{
		LootItem lootItem = LootList.OfType<LootItem>().FirstOrDefault((LootItem x) => x.ItemId.Equals(id));
		if (lootItem != null)
		{
			DestroyLoot(lootItem);
		}
		else
		{
			Debug.LogError("Loot item with id (" + id + ") has not been found in a loot list.");
		}
	}

	public void DestroyLoot(IKillableLootItem loot)
	{
		if (action_5 != null)
		{
			action_5(loot);
		}
		LootList.Remove(loot);
		LootItem lootItem = loot as LootItem;
		if (lootItem != null)
		{
			ItemOwners.Remove(lootItem.ItemOwner);
			LootItems.Remove(lootItem.GetNetId());
			OnTakeItem?.Invoke(lootItem);
		}
		if (loot is LootableContainer && ((LootableContainer)loot).ItemOwner != null)
		{
			ItemOwners.Remove(((LootableContainer)loot).ItemOwner);
		}
		foreach (IPlayer registeredPlayer in RegisteredPlayers)
		{
			Player alivePlayerByProfileID = GetAlivePlayerByProfileID(registeredPlayer.ProfileId);
			if (!(alivePlayerByProfileID == null))
			{
				alivePlayerByProfileID.ResetInteractionRaycast(loot);
			}
		}
		loot.Kill();
	}

	public GClass1404 method_4(GClass1404 lootItems)
	{
		GClass1404 gClass = new GClass1404();
		foreach (LootItemPositionClass lootItem in lootItems)
		{
			if (lootItem.Item.QuestItem)
			{
				list_1.Add(lootItem);
			}
			else
			{
				gClass.Add(lootItem);
			}
		}
		return gClass;
	}

	public virtual LootItem CreateStaticLoot(GameObject lootObject, Item item, string lootName, bool randomRotation, [CanBeNull] MongoID[] validProfiles, string staticId = null, Vector3 shift = default(Vector3))
	{
		return LootItem.CreateStaticLoot<LootItem>(lootObject, item, lootName, this, randomRotation, validProfiles, staticId, performPickUpValidation: true, shift);
	}

	public void method_5(GClass1404 lootItems, bool initial)
	{
		Platforms = LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<MovingPlatform>().ToArray();
		Locomotive[] array = Platforms.OfType<Locomotive>().ToArray();
		PlatformAdapters = new MovingPlatform.GClass3596[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			PlatformAdapters[i] = new MovingPlatform.GClass3596
			{
				Platform = array[i],
				Id = (byte)i
			};
		}
		GInterface30[] array2 = LocationScene.GetAllObjects<StaticLoot>().Cast<GInterface30>().Concat(LocationScene.GetAllObjects<LootableContainer>())
			.Concat(LocationScene.GetAllObjects<StationaryWeapon>())
			.ToArray();
		if (initial)
		{
			array2 = method_6(lootItems, array2);
		}
		List<Vector3> list = new List<Vector3>();
		foreach (LootItemPositionClass lootItem2 in lootItems)
		{
			string lootId = lootItem2.Id;
			Item item = lootItem2.Item;
			string shortName = item.ShortName;
			AllLoot.Add(lootItem2);
			if (initial)
			{
				item.SpawnedInSession = true;
				if (item is GClass3248 topLevelCollection)
				{
					foreach (Item item2 in GClass3380.GetAllItemsFromCollection(topLevelCollection))
					{
						item2.SpawnedInSession = true;
					}
				}
			}
			if (lootItem2.IsContainer)
			{
				GInterface30 gInterface = array2.FirstOrDefault((GInterface30 x) => x.Id == lootId);
				if (gInterface != null)
				{
					if (!(gInterface is StaticLoot staticLoot))
					{
						if (!(gInterface is LootableContainer lootableContainer))
						{
							if (gInterface is StationaryWeapon stationaryWeapon)
							{
								LootItem.CreateStationaryWeapon(stationaryWeapon, item, shortName, this, lootId);
								list.Add(stationaryWeapon.transform.position);
							}
						}
						else
						{
							LootItem.CreateLootContainer(lootableContainer, item, shortName, this, lootId);
							list.Add(lootableContainer.transform.position);
						}
					}
					else
					{
						new TraderControllerClass(item, item.Id, shortName);
						CreateStaticLoot(staticLoot.gameObject, item, shortName, lootItem2.randomRotation, lootItem2.ValidProfiles, lootId, lootItem2.Shift);
						list.Add(staticLoot.gameObject.transform.position);
					}
				}
				else
				{
					Debug.LogError(lootId + " is missing");
				}
				continue;
			}
			if (lootItem2 is GClass1402 lootItem)
			{
				SpawnLootCorpse(lootItem);
			}
			else
			{
				if (lootItem2.PlatformId > -1)
				{
					Debug.Log(GClass1673.Yellow("SERIALIZE LOOT IS ON PLATFORM"));
				}
				method_9(lootItem2, initial, null, (lootItem2.PlatformId > -1) ? Platforms[lootItem2.PlatformId] : null);
			}
			list.Add(lootItem2.Position);
		}
		if (Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Instance.SpawnLoot(list);
		}
	}

	public virtual void SpawnLootCorpse(GClass1402 lootItem)
	{
		SpawnLootCorpse<Corpse>(lootItem);
	}

	public void SpawnLootCorpse<T>(GClass1402 corpseJson) where T : Corpse
	{
		Corpse.CreateStillCorpse<T>(this, corpseJson, (corpseJson.PlatformId > -1) ? Platforms[corpseJson.PlatformId] : null);
	}

	public GInterface30[] method_6(GClass1404 lootItems, GInterface30[] staticLootSpawns)
	{
		method_7(lootItems, ref staticLootSpawns);
		return staticLootSpawns.Where((GInterface30 x) => x != null).ToArray();
	}

	public void DevelopSetContainerVisibility(int command, string id)
	{
		Class1114 CS_0024_003C_003E8__locals7 = new Class1114();
		CS_0024_003C_003E8__locals7.gameWorld_0 = this;
		CS_0024_003C_003E8__locals7.id = id;
		switch (command)
		{
		case 0:
			foreach (GameObject item in list_2)
			{
				item.SetActive(value: true);
			}
			list_2.Clear();
			break;
		case 1:
		{
			if (GClass1673.IsNullOrEmpty(CS_0024_003C_003E8__locals7.id))
			{
				Debug.LogError("Can't hide container, empty id");
				break;
			}
			LootableContainer lootableContainer = LootList.OfType<LootableContainer>().FirstOrDefault((LootableContainer x) => x.Id.Equals(CS_0024_003C_003E8__locals7.id));
			if (lootableContainer == null)
			{
				Debug.LogError("Can't find lootable container with id:" + CS_0024_003C_003E8__locals7.id + " to delete");
			}
			else
			{
				CS_0024_003C_003E8__locals7.method_0(lootableContainer);
			}
			break;
		}
		case 2:
		{
			foreach (LootableContainer item2 in LootList.OfType<LootableContainer>())
			{
				CS_0024_003C_003E8__locals7.method_0(item2);
			}
			break;
		}
		}
	}

	public void method_7(GClass1404 lootItems, ref GInterface30[] staticLootSpawns)
	{
		list_2.Clear();
		GInterface30[] array = staticLootSpawns;
		foreach (GInterface30 staticLootSpawn in array)
		{
			if (lootItems.All((LootItemPositionClass lootItem) => lootItem.Id != staticLootSpawn.Id))
			{
				GameObject gameObject = ((MonoBehaviour)staticLootSpawn).gameObject;
				string text = string.Join("/", Enumerable.Repeat(gameObject.scene.name, 1).Concat(from x in smethod_0(gameObject.transform).Reverse()
					select x.name).ToArray());
				Debug.LogWarning("Static loot " + staticLootSpawn.Id + " not found in json. (" + text + ") Destroying the loot component...", gameObject);
				if (staticLootSpawn is LootableContainer lootableContainer && !GClass856.IsNullOrEmpty(lootableContainer.GameObjectsToDestroy))
				{
					list_2.AddRange(lootableContainer.GameObjectsToDestroy);
				}
			}
		}
		foreach (GameObject item in list_2)
		{
			item.SetActive(value: false);
		}
		list_2.Clear();
	}

	public static IEnumerable<Transform> smethod_0(Transform transform)
	{
		while (transform != null)
		{
			yield return transform;
			transform = transform.parent;
		}
	}

	public void method_8(GClass1404 lootItems)
	{
		GInterface30[] staticLootSpawns = LocationScene.GetAllObjects<StaticLoot>().Concat(LocationScene.GetAllObjects<LootableContainer>().Cast<GInterface30>()).ToArray();
		method_7(lootItems, ref staticLootSpawns);
	}

	public virtual LootItem CreateLootWithRigidbody(GameObject lootObject, Item item, string objectName, bool randomRotation, [CanBeNull] MongoID[] validProfiles, out BoxCollider objectCollider, bool syncable, bool performPickUpValidation = true, float makeVisibleAfterDelay = 0f)
	{
		return LootItem.CreateLootWithRigidbody<LootItem>(lootObject, item, objectName, this, randomRotation, validProfiles, out objectCollider, performPickUpValidation, makeVisibleAfterDelay);
	}

	public void method_9(LootItemPositionClass lootItem, bool initial, Player questPlayer = null, MovingPlatform platform = null)
	{
		Item item = ((questPlayer == null) ? lootItem.Item : GClass3380.CloneItem(lootItem.Item));
		new TraderControllerClass(item, item.Id, item.ShortName);
		GameObject gameObject = Singleton<PoolManagerClass>.Instance.CreateLootPrefab(item, ECameraType.Default);
		gameObject.SetActive(value: true);
		if (platform != null)
		{
			gameObject.transform.position = platform.transform.TransformPoint(lootItem.Position);
		}
		else
		{
			gameObject.transform.position = lootItem.Position;
			gameObject.transform.rotation = Quaternion.Euler(lootItem.Rotation);
		}
		BoxCollider objectCollider;
		LootItem lootItem2 = (lootItem.useGravity ? CreateLootWithRigidbody(gameObject, item, item.ShortName, lootItem.randomRotation, lootItem.ValidProfiles, out objectCollider, syncable: false) : CreateStaticLoot(gameObject, item, item.ShortName, lootItem.randomRotation, lootItem.ValidProfiles, null, lootItem.Shift));
		if (platform != null)
		{
			lootItem2.Board(platform);
			gameObject.transform.localRotation = Quaternion.Euler(lootItem.Rotation);
			return;
		}
		PreviewPivot component = gameObject.GetComponent<PreviewPivot>();
		if (!(component == null) && !(component.SpawnPosition == Vector3.zero))
		{
			GameObject gameObject2 = new GameObject("Weapon spawn root");
			gameObject2.transform.position = lootItem.Position;
			gameObject2.transform.rotation = Quaternion.Euler(lootItem.Rotation);
			Transform transform = lootItem2.transform;
			transform.SetParent(gameObject2.transform);
			transform.localPosition = (initial ? (-component.SpawnPosition) : Vector3.zero);
			if (lootItem.randomRotation)
			{
				transform.rotation = Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(0, 360), 0f));
			}
			transform.localScale = Vector3.one;
		}
	}

	public virtual Task PreloadAdditionalData()
	{
		return Task.CompletedTask;
	}

	public LootItem ThrowItem(Item item, IPlayer player, Vector3? direction)
	{
		Vector3 vector = direction ?? (Quaternion.Euler(Mathf.Clamp(player.Rotation.y, -90f, 45f), player.Rotation.x, 0f) * new Vector3(0f, 1f, 1f));
		vector *= 2f;
		Vector3 position = player.PlayerColliderPointOnCenterAxis(0.65f) + player.Velocity * Time.deltaTime;
		Quaternion rotation = player.PlayerBones.WeaponRoot.rotation * Quaternion.Euler(90f, 0f, 0f);
		return ThrowItem(angularVelocity: new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f), 2f * Mathf.Sign(UnityEngine.Random.Range(-1, 2))), item: item, player: player, position: position, rotation: rotation, velocity: vector + player.Velocity / 2f, syncable: true, performPickUpValidation: true, makeVisibleAfterDelay: EFTHardSettings.Instance.ThrowLootMakeVisibleDelay);
	}

	public LootItem ThrowItem(Item item, IPlayer player, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, bool syncable, bool performPickUpValidation = true, float makeVisibleAfterDelay = 0f)
	{
		try
		{
			if (item == null)
			{
				Debug.LogError("item is null");
			}
			new TraderControllerClass(item, item?.Id, item?.ShortName);
			if (Singleton<PoolManagerClass>.Instance == null)
			{
				throw new Exception("Singleton<ObjectsFactory>.Instance is null");
			}
			GameObject gameObject = Singleton<PoolManagerClass>.Instance.CreateLootPrefab(item, Player.GetVisibleToCamera(player));
			if (gameObject == null)
			{
				throw new Exception("Prefab is null");
			}
			gameObject.SetActive(value: true);
			BoxCollider objectCollider;
			LootItem lootItem = CreateLootWithRigidbody(gameObject, item, item?.ShortName, randomRotation: false, null, out objectCollider, syncable, performPickUpValidation, makeVisibleAfterDelay);
			try
			{
				lootItem.transform.SetPositionAndRotation(position, rotation);
				lootItem.RigidBody.velocity = velocity;
				lootItem.RigidBody.angularVelocity = angularVelocity;
				lootItem.LastOwner = player;
				lootItem.CacheParameters();
				OnThrowItem?.Invoke(lootItem);
			}
			catch (Exception ex)
			{
				Debug.LogError($"{item?.Name} {item?.Id} loot: {lootItem}: {ex.Message} \n {ex.StackTrace}");
			}
			return lootItem;
		}
		catch (Exception ex2)
		{
			Debug.LogError(item?.Name + " " + item?.Id + " : " + ex2.Message + " \n " + ex2.StackTrace);
		}
		return null;
	}

	public LootItem SetupItem(Item item, IPlayer player, Vector3 position, Quaternion rotation)
	{
		new TraderControllerClass(item, item.Id, item.ShortName);
		GameObject gameObject = Singleton<PoolManagerClass>.Instance.CreateLootPrefab(item, Player.GetVisibleToCamera(player));
		gameObject.SetActive(value: true);
		BoxCollider objectCollider;
		LootItem lootItem = CreateLootWithRigidbody(gameObject, item, item.ShortName, randomRotation: false, null, out objectCollider, syncable: true);
		lootItem.GetComponent<Rigidbody>().isKinematic = true;
		lootItem.transform.SetPositionAndRotation(position + rotation * lootItem.Shift, rotation);
		lootItem.LastOwner = player;
		return lootItem;
	}

	public virtual void PlantTripwire(Item item, string profileId, Vector3 fromPosition, Vector3 toPosition)
	{
		if (item is ThrowWeapItemClass throwWeap)
		{
			TripwireSynchronizableObject tripwireSynchronizableObject = (TripwireSynchronizableObject)SynchronizableObjectLogicProcessor.TakeFromPool(SynchronizableObjectType.Tripwire);
			tripwireSynchronizableObject.transform.SetPositionAndRotation(fromPosition, Quaternion.identity);
			SynchronizableObjectLogicProcessor.InitSyncObject(tripwireSynchronizableObject, fromPosition, Vector3.forward);
			tripwireSynchronizableObject.SetupGrenade(throwWeap, profileId, fromPosition, toPosition);
			SynchronizableObjectLogicProcessor.TripwireManager.AddTripwire(tripwireSynchronizableObject);
			Vector3 position = (fromPosition + toPosition) * 0.5f;
			Singleton<BotEventHandler>.Instance.PlantTripwire(tripwireSynchronizableObject, position);
		}
	}

	public void SpawnTripwire(ISynchronizableObject tripwire)
	{
		throw new NotImplementedException();
	}

	public virtual void TriggerTripwire(TripwireSynchronizableObject tripwire)
	{
		SynchronizableObjectLogicProcessor.TripwireManager.RemoveTripwire(tripwire);
		tripwire.TriggerTripwire();
	}

	public virtual void DeActivateTripwire(TripwireSynchronizableObject tripwire)
	{
		SynchronizableObjectLogicProcessor.TripwireManager.RemoveTripwire(tripwire);
		tripwire.DisableTripwire();
	}

	public virtual void Start()
	{
		_interactiveLootMask = LayerMask.GetMask("Interactive", "Deadbody", "Loot");
		int_0 = LayerMask.GetMask("Interactive", "Deadbody", "Player", "Loot");
		int_1 = LayerMask.GetMask("Player");
		int_2 = LayerMask.GetMask("HighPolyCollider", "TransparentCollider");
	}

	public virtual void ShotDelegate(EftBulletClass shotResult)
	{
		if (shotResult.IsFlyingOutOfTime || shotResult.AvoidAdditionalDamage)
		{
			return;
		}
		DamageInfoStruct damageInfo = new DamageInfoStruct(EDamageType.Bullet, shotResult);
		ShotIdStruct shotID = new ShotIdStruct(shotResult.Ammo.Id, shotResult.FragmentIndex);
		ShotInfoClass playerHitInfo = shotResult.HittedBallisticCollider.ApplyHit(damageInfo, shotID);
		shotResult.AddClientHitPosition(playerHitInfo);
		ExplosiveItemComponentClass itemComponent = shotResult.Ammo.GetItemComponent<ExplosiveItemComponentClass>();
		if (itemComponent != null && shotResult.TimeSinceShot >= itemComponent.Template.FuzeArmTimeSec)
		{
			string explosionType = itemComponent.Template.ExplosionType;
			if (!string.IsNullOrEmpty(explosionType) && shotResult.IsFirstHit)
			{
				Singleton<Effects>.Instance.EmitGrenade(explosionType, shotResult.HitPoint, shotResult.HitNormal, shotResult.IsForwardHit ? 1 : 0);
			}
			Grenade.Explosion(null, itemComponent, shotResult.HitPoint, shotResult.Player.iPlayer.ProfileId, SharedBallisticsCalculator, shotResult.Weapon, shotResult.HitNormal * 0.08f);
		}
	}

	public virtual void NetworkWorldOnGrenadeHit(Vector3 pos)
	{
	}

	public virtual ShotInfoClass HackShot(DamageInfoStruct damageInfo)
	{
		if (null == damageInfo.HittedBallisticCollider)
		{
			damageInfo.HittedBallisticCollider = ballisticCollider_0;
		}
		return damageInfo.HittedBallisticCollider.ApplyHit(damageInfo, ShotIdStruct.EMPTY_SHOT_ID);
	}

	public void Update()
	{
		gclass824_0.RecordDeltaTime();
		if (CameraClass.Instance.Camera == null)
		{
			return;
		}
		for (int num = list_0.Count - 1; num >= 0; num--)
		{
			AmmoPoolObject ammoPoolObject = list_0[num];
			if (ammoPoolObject.ShouldBeDestroyed)
			{
				list_0.RemoveAt(num);
				AssetPoolObject.ReturnToPool(ammoPoolObject.gameObject);
			}
		}
	}

	public void DoWorldTick(float dt)
	{
		BeforeWorldTick(dt);
		BeforePlayerTick(dt);
		PlayerTick(dt);
		BallisticsTick(dt);
		AfterPlayerTick(dt);
		OtherElseWorldTick(dt);
		AfterWorldTick(dt);
	}

	public void DoOtherWorldTick(float dt)
	{
		method_10(delegate(Player player)
		{
			if (player.UpdateQueue == EUpdateQueue.Update)
			{
				player.FixedUpdateTick();
			}
			else if (player.UpdateQueue == EUpdateQueue.FixedUpdate)
			{
				player.UpdateTick();
			}
		});
	}

	public virtual void BeforeWorldTick(float dt)
	{
		action_3?.Invoke(dt);
	}

	public virtual void BallisticsTick(float dt)
	{
		SharedBallisticsCalculator?.ManualUpdate(dt);
	}

	public virtual void BeforePlayerTick(float dt)
	{
	}

	public virtual void PlayerTick(float dt)
	{
		method_10(delegate(Player player)
		{
			if (player.UpdateQueue == EUpdateQueue.Update)
			{
				player.UpdateTick();
			}
			else if (player.UpdateQueue == EUpdateQueue.FixedUpdate)
			{
				player.FixedUpdateTick();
			}
		});
	}

	public virtual void AfterPlayerTick(float dt)
	{
		method_10(delegate(Player player)
		{
			player.AfterMainTick();
		});
	}

	public virtual void OtherElseWorldTick(float dt)
	{
		SpeakerManager.ManualUpdate(dt);
	}

	public virtual void AfterWorldTick(float dt)
	{
	}

	public virtual void LateUpdateWorld(float dt)
	{
		action_4?.Invoke(dt);
	}

	public void method_10(Action<Player> operation)
	{
		for (int num = AllAlivePlayersList.Count - 1; num >= 0; num--)
		{
			try
			{
				operation(AllAlivePlayersList[num]);
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("[{0}] tick operation exception: {1}", AllAlivePlayersList[num].FullIdInfo, ex);
			}
		}
	}

	public virtual void OnDestroy()
	{
	}

	public virtual void Dispose()
	{
		using (CounterCreatorAbstractClass.StartWithToken("loot.Kill"))
		{
			for (int num = LootList.Count - 1; num >= 0; num--)
			{
				try
				{
					IKillableLootItem killableLootItem = LootList[num];
					if ((UnityEngine.Object)killableLootItem != null)
					{
						killableLootItem.Kill();
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
		foreach (AmmoPoolObject item in list_0)
		{
			AssetPoolObject.ReturnToPool(item.gameObject);
		}
		list_0.Clear();
		LootList.Clear();
		LootItems.Clear();
		if (bool_0)
		{
			using (CounterCreatorAbstractClass.StartWithToken("ClearPools"))
			{
				if (ObjectsFactory != null)
				{
					ObjectsFactory.UnloadTemporaryPools(cleanUselessPools: true);
					ObjectsFactory.UnloadBundles();
				}
			}
		}
		if (gclass1661_0 != null)
		{
			gclass1661_0.Release();
			gclass1661_0 = null;
		}
		ClientBallisticCalculator?.ClearShots();
		SharedBallisticsCalculator?.ClearShots();
		ClientShellingController?.Dispose();
		ServerShellingController?.Dispose();
		ClientBroadcastSyncController?.Dispose();
		CompositeDisposable.Dispose();
		ExfiltrationController.Dispose();
		TransitController?.Dispose();
		RunddansController?.Dispose();
		BufferZoneController.Dispose();
		HalloweenEventController?.Dispose();
		GInterface29_0?.Dispose();
		GInterface29_0 = null;
		BtrController?.Dispose();
		TriggersEmitter?.Dispose();
		GClass3670.Instance.Dispose();
		world_0 = null;
		action_0?.Invoke();
		action_0 = null;
		method_11();
	}

	public void method_11()
	{
		foreach (KeyValuePair<string, IPlayerOwner> item in AllPlayerBridgesEverExisted)
		{
			item.Value?.Dispose();
		}
		allAlivePlayersByID.Clear();
		AllAlivePlayersList.Clear();
		AllAlivePlayerBridges.Clear();
		AllAlivePlayerBridgesByCollider.Clear();
		AllPlayerBridgesEverExisted.Clear();
		allObservedPlayersByID.Clear();
	}

	public static bool InteractionSense(Vector3 origin, Vector3 direction, float radius, float distance)
	{
		RaycastHit hitInfo;
		bool num = Physics.SphereCast(origin, radius, direction, out hitInfo, distance, _interactiveLootMask);
		LootItem lootItem = null;
		Switch obj = null;
		if (num)
		{
			lootItem = hitInfo.collider.gameObject.GetComponentInParent<LootItem>();
			if (lootItem == null)
			{
				Switch component = hitInfo.collider.gameObject.GetComponent<Switch>();
				if (component != null && component.Operatable && component.DoorState == EDoorState.Shut)
				{
					obj = component;
				}
			}
		}
		if (lootItem != null && !(lootItem is Corpse) && (!(lootItem.Item is Weapon { IsOneOff: not false } weapon) || weapon.Repairable.Durability != 0f) && lootItem.enabled)
		{
			return true;
		}
		return obj != null;
	}

	[CanBeNull]
	public static GameObject FindInteractable(Ray ray, out RaycastHit hit)
	{
		GameObject gameObject = (EFTPhysicsClass.Raycast(ray, out hit, Mathf.Max(EFTHardSettings.Instance.LOOT_RAYCAST_DISTANCE, EFTHardSettings.Instance.PLAYER_RAYCAST_DISTANCE + EFTHardSettings.Instance.BEHIND_CAST), int_0) ? hit.collider.gameObject : null);
		if ((bool)gameObject && !Physics.Linecast(ray.origin, hit.point, int_2))
		{
			return gameObject;
		}
		return null;
	}

	public WorldInteractiveObject FindDoor(string doorId)
	{
		return World_0.FindDoor(doorId);
	}

	public IEnumerable<WorldInteractiveObject> WorldInteractiveObjects()
	{
		return World_0.WorldInteractiveObjects();
	}

	public bool TryGetLampController(string id, out LampController lampController)
	{
		return dictionary_1.TryGetValue(id, out lampController);
	}

	public bool TryGetControlledLampGroup(ulong id, out ControlledLampGroup controlledLampGroup)
	{
		return dictionary_2.TryGetValue(id, out controlledLampGroup);
	}

	public StationaryWeapon FindStationaryWeapon(string id)
	{
		return LocationScene.GetAllObjects<StationaryWeapon>().FirstOrDefault((StationaryWeapon x) => x.Id == id);
	}

	public StationaryWeapon FindStationaryWeaponByItemId(string itemId)
	{
		return LocationScene.GetAllObjects<StationaryWeapon>().FirstOrDefault((StationaryWeapon x) => x.Item.Id == itemId);
	}

	public virtual void RegisterPlayer(IPlayer iPlayer)
	{
		RegisteredPlayers.Add(iPlayer);
		Collider collider = iPlayer.CharacterController.GetCollider();
		Player player = iPlayer as Player;
		string profileId = iPlayer.ProfileId;
		Transform transform;
		if (player != null)
		{
			if (!AllAlivePlayersList.Contains(player))
			{
				AllAlivePlayersList.Add(player);
			}
			allAlivePlayersByID[player.ProfileId] = player;
			dictionary_0[player.ProfileId] = player;
			if (iPlayer.IsYourPlayer)
			{
				MainPlayer = player;
			}
			if (BackendConfigAbstractClass.Config.UseSpiritPlayer)
			{
				transform = player.Spirit.GetActiveTransform();
				Collider collider2 = player.Spirit.CharacterController.GetCollider();
				PlayersColliders[collider2] = player;
				Collider collider3 = player.Spirit.PlayerSpiritAura.GetCollider();
				PlayersColliders[collider3] = player;
			}
			else
			{
				transform = player.gameObject.transform;
			}
			player.SetDeltaTimeDelegate(() => DeltaTime);
			AllAlivePlayerBridges[profileId] = new GClass1710(player);
		}
		else
		{
			transform = iPlayer.Transform.Original;
		}
		ItemOwners.Add(iPlayer.InventoryController, new GStruct162
		{
			Transform = transform
		});
		AllAlivePlayerBridgesByCollider[collider] = AllAlivePlayerBridges[profileId];
		AllPlayerBridgesEverExisted[profileId] = AllAlivePlayerBridges[profileId];
		PlayersColliders[collider] = iPlayer;
		action_2?.Invoke(iPlayer);
	}

	public virtual void UnregisterPlayer(IPlayer iPlayer)
	{
		if (!RegisteredPlayers.Contains(iPlayer))
		{
			return;
		}
		RegisteredPlayers.Remove(iPlayer);
		Collider collider = iPlayer.CharacterController.GetCollider();
		PlayersColliders.Remove(collider);
		allAlivePlayersByID.Remove(iPlayer.ProfileId);
		AllAlivePlayerBridges.Remove(iPlayer.ProfileId);
		AllAlivePlayerBridgesByCollider.Remove(collider);
		allObservedPlayersByID.Remove(iPlayer.ProfileId);
		Player player = iPlayer as Player;
		if (player != null)
		{
			AllAlivePlayersList.Remove(player);
			ItemOwners.Remove(player.InventoryController);
			if (BackendConfigAbstractClass.Config.UseSpiritPlayer)
			{
				Collider collider2 = player.Spirit.CharacterController.GetCollider();
				PlayersColliders.Remove(collider2);
				Collider collider3 = player.Spirit.PlayerSpiritAura.GetCollider();
				PlayersColliders.Remove(collider3);
			}
			player.SetDeltaTimeDelegate(null);
		}
	}

	public virtual void RegisterPlayerCollider(Player player, Collider playerCollider)
	{
		PlayersColliders[playerCollider] = player;
	}

	public virtual void UnregisterPlayerCollider(Collider playerCollider)
	{
		PlayersColliders.Remove(playerCollider);
	}

	public async Task method_12(Player player)
	{
		foreach (LootItemPositionClass item in list_1)
		{
			IEnumerable<Item> playerItems = player.Profile.Inventory.GetPlayerItems(EPlayerItems.QuestItems);
			if (playerItems != null)
			{
				item.ValidProfiles = new MongoID[20];
				int num = 0;
				if (playerItems.Select((Item x) => x.TemplateId).Contains(item.Item.TemplateId))
				{
					continue;
				}
				foreach (QuestClass item2 in player.AbstractQuestControllerClass.Quests.Where((QuestClass quest) => quest.QuestStatus == EQuestStatus.Started))
				{
					try
					{
						foreach (ConditionFindItem condition in item2.GetConditions<ConditionFindItem>(EQuestStatus.AvailableForFinish))
						{
							if (condition.target.Contains(item.Item.StringTemplateId) && !item2.ProgressCheckers[condition].Test() && !item2.CompletedConditions.Contains(condition.id))
							{
								item.ValidProfiles[num] = player.ProfileId;
								num++;
							}
						}
					}
					catch (Exception ex)
					{
						Debug.LogError("Something awful happened with a quest: " + item2.Id + ". Exception: " + ex);
					}
				}
				if (num == 0)
				{
					item.ValidProfiles = null;
				}
				method_9(item, initial: true, player);
				continue;
			}
			Debug.LogError("Quest stashes are missing for (" + player.Profile.Nickname + ") !");
			break;
		}
	}

	[CanBeNull]
	public Player GetPlayerByCollider(Collider col)
	{
		if (col == null)
		{
			return null;
		}
		PlayersColliders.TryGetValue(col, out var value);
		if (value is Player result)
		{
			return result;
		}
		return null;
	}

	public GStruct156<ItemAddress> ToItemAddress(GClass1950 descriptor)
	{
		IItemOwner itemOwner;
		if (descriptor is GClass1951)
		{
			MongoID? parentId = descriptor.Container.ParentId;
			itemOwner = FindOwnerById(parentId.HasValue ? ((string)parentId.GetValueOrDefault()) : null);
			if (itemOwner == null)
			{
				parentId = descriptor.Container.ParentId;
				return new GClass1524(parentId.HasValue ? ((string)parentId.GetValueOrDefault()) : null);
			}
		}
		else
		{
			MongoID? parentId = descriptor.Container.ParentId;
			GStruct156<Item> gStruct = FindItemById(parentId.HasValue ? ((string)parentId.GetValueOrDefault()) : null);
			if (gStruct.Failed)
			{
				return gStruct.Error;
			}
			itemOwner = GClass3113.GetOwnerOrNull(gStruct.Value.Parent);
			if (itemOwner == null)
			{
				return new GClass1534(gStruct.Value);
			}
		}
		ItemAddress itemAddress;
		try
		{
			itemAddress = itemOwner.ToItemAddress(descriptor);
		}
		catch (GException16)
		{
			MongoID? parentId = descriptor.Container.ParentId;
			return new GClass1525(parentId.HasValue ? ((string)parentId.GetValueOrDefault()) : null, descriptor.Container.ContainerId);
		}
		return itemAddress;
	}

	[CanBeNull]
	public IItemOwner FindOwnerById(string ownerId)
	{
		return FindOwnerWithWorldData(ownerId).Key;
	}

	public KeyValuePair<IItemOwner, GStruct162> FindOwnerWithWorldData(string ownerId)
	{
		return ItemOwners.FirstOrDefault((KeyValuePair<IItemOwner, GStruct162> x) => x.Key.ID == ownerId);
	}

	public GStruct156<Item> FindItemById(string itemId)
	{
		return FindItemWithWorldData(itemId).Cast(((Item item, GStruct162 data) e) => e.item);
	}

	public GStruct156<(Item item, GStruct162 data)> FindItemWithWorldData(string itemId)
	{
		using (GClass4062.BeginSampleWithToken("GameWorld.FindItemById", "FindItemWithWorldData"))
		{
			foreach (KeyValuePair<IItemOwner, GStruct162> itemOwner2 in ItemOwners)
			{
				itemOwner2.Deconstruct(out var key, out var value);
				IItemOwner itemOwner = key;
				GStruct162 gStruct = value;
				Item item = null;
				try
				{
					if (!itemOwner.TryFindItem(itemId, out item))
					{
						continue;
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				GStruct162 item2 = gStruct;
				item2.ItemOwner = itemOwner;
				return (item, item2);
			}
		}
		return new GClass1526(itemId);
	}

	public int CulledPlayersCount()
	{
		int num = 0;
		foreach (IPlayer registeredPlayer in RegisteredPlayers)
		{
			if (!(registeredPlayer is ObservedPlayerView observedPlayerView))
			{
				if (registeredPlayer is Player player)
				{
					num += (player.IsVisible ? 1 : 0);
				}
			}
			else
			{
				num += (observedPlayerView.IsVisible ? 1 : 0);
			}
		}
		return num;
	}

	public int TotalPlayersCountToCull()
	{
		int num = 0;
		foreach (IPlayer registeredPlayer in RegisteredPlayers)
		{
			if (!registeredPlayer.IsYourPlayer)
			{
				num++;
			}
		}
		return num;
	}

	public void OnSmokeGrenadesDeserialized(List<SmokeGrenadeDataPacketStruct> netGrenadeData)
	{
		foreach (SmokeGrenadeDataPacketStruct netGrenadeDatum in netGrenadeData)
		{
			SmokeGrenade grenade = grenadeFactoryClass.CreateStillSmokeGrenade(netGrenadeDatum.Id, netGrenadeDatum.Template, netGrenadeDatum.Position, netGrenadeDatum.Orientation, netGrenadeDatum.Time, (netGrenadeDatum.PlatformId > -1) ? Platforms[netGrenadeDatum.PlatformId] : null);
			RegisterGrenade(grenade);
		}
	}

	public void method_13(WorldInteractiveObject door, EDoorState stateBefore, EDoorState stateAfter)
	{
		if (stateAfter != EDoorState.Interacting && stateAfter != EDoorState.Breaching)
		{
			list_3.Remove(door);
		}
		else
		{
			list_3.Add(door);
		}
	}

	public int CanPlayerSeepThrough(Collider collider)
	{
		if (collider.gameObject.layer == LayerMaskClass.PlayerLayer)
		{
			foreach (IPlayer registeredPlayer in RegisteredPlayers)
			{
				if (registeredPlayer.CharacterController.GetCollider() == collider)
				{
					return 0;
				}
			}
		}
		else if (collider.gameObject.layer == LayerMaskClass.DoorLayer)
		{
			for (int i = 0; i < list_3.Count; i++)
			{
				if (list_3[i].Collider == collider)
				{
					return 1;
				}
			}
		}
		return -1;
	}

	public void RegisterBorderZones()
	{
		BorderZones = LocationScene.GetAllObjects<BorderZone>().ToArray();
		for (int i = 0; i < BorderZones.Length; i++)
		{
			BorderZones[i].Id = i;
		}
		World_0.SubscribeToBorderZones(BorderZones);
	}

	public void RegisterRestrictableZones()
	{
		baseRestrictableZone_0 = LocationScene.GetAllObjects<BaseRestrictableZone>().ToArray();
	}

	public virtual void OnGameStarted()
	{
		action_1?.Invoke();
	}

	public GameWorld()
	{
	}

	[CompilerGenerated]
	public static void smethod_1(Player player)
	{
		if (player.UpdateQueue == EUpdateQueue.Update)
		{
			player.FixedUpdateTick();
		}
		else if (player.UpdateQueue == EUpdateQueue.FixedUpdate)
		{
			player.UpdateTick();
		}
	}

	[CompilerGenerated]
	public static void smethod_2(Player player)
	{
		if (player.UpdateQueue == EUpdateQueue.Update)
		{
			player.UpdateTick();
		}
		else if (player.UpdateQueue == EUpdateQueue.FixedUpdate)
		{
			player.FixedUpdateTick();
		}
	}

	[CompilerGenerated]
	public static void smethod_3(Player player)
	{
		player.AfterMainTick();
	}

	[CompilerGenerated]
	public float method_14()
	{
		return DeltaTime;
	}
}

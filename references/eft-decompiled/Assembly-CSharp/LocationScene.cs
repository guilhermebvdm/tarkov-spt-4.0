using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Audio.RadioSystem;
using EFT;
using EFT.Airdrop;
using EFT.BufferZone;
using EFT.Game.Spawning;
using EFT.Hideout;
using EFT.Interactive;
using EFT.MovingPlatforms;
using EFT.SpeedTree;
using EFT.SynchronizableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class LocationScene : MonoBehaviour, IBotController
{
	[Serializable]
	[CompilerGenerated]
	public class Class373
	{
		public static readonly Class373 class373_0 = new Class373();

		public static Func<GameObject, bool> func_0;

		public static Func<GameObject, IEnumerable<GInterface457>> func_1;

		public static Func<GInterface457, bool> func_2;

		public static Func<MonoBehaviour, IEnumerable<GInterface104>> func_3;

		public static Func<GInterface457, bool> func_4;

		public static Func<GInterface457, GameObject> func_5;

		public static Func<GInterface104, MonoBehaviour> func_6;

		public bool method_0(GameObject te)
		{
			return te != null;
		}

		public IEnumerable<GInterface457> method_1(GameObject te)
		{
			return te.GetComponents<GInterface457>();
		}

		public bool method_2(GInterface457 te)
		{
			return te != null;
		}

		public IEnumerable<GInterface104> method_3(MonoBehaviour cc)
		{
			return cc.GetComponents<GInterface104>();
		}

		public bool method_4(GInterface457 te)
		{
			if (!te.OutputTriggerIds.Any())
			{
				return te.InputTriggerIds.Any();
			}
			return true;
		}

		public GameObject method_5(GInterface457 te)
		{
			return te.GameObject;
		}

		public MonoBehaviour method_6(GInterface104 cc)
		{
			return cc as MonoBehaviour;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class374<T>
	{
		public static readonly Class374<T> class374_0 = new Class374<T>();

		public static Func<LocationScene, IEnumerable<T>> func_0;

		public IEnumerable<T> method_0(LocationScene scene)
		{
			return scene.method_1<T>();
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class375<T>
	{
		public static readonly Class375<T> class375_0 = new Class375<T>();

		public static Func<LocationScene, IEnumerable<T>> func_0;

		public IEnumerable<T> method_0(LocationScene scene)
		{
			return scene.method_1<T>();
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class376<T> where T : Behaviour
	{
		public static readonly Class376<T> class376_0 = new Class376<T>();

		public static Func<LocationScene, IEnumerable<T>> func_0;

		public IEnumerable<T> method_0(LocationScene x)
		{
			return x.method_1<T>();
		}
	}

	[CompilerGenerated]
	public class Class377<T> where T : Behaviour
	{
		public bool withDisabled;

		public IEnumerable<T> method_0(LocationScene scene)
		{
			return scene.method_1<T>().Where(delegate(T obj)
			{
				if (obj != null)
				{
					if (!withDisabled)
					{
						return obj.isActiveAndEnabled;
					}
					return true;
				}
				Debug.LogError("Object of type " + typeof(T).Name + " on scene " + scene.gameObject.scene.name + " has been deleted. God, please fix it. Is unity null: " + (obj == null && (object)obj != null));
				return false;
			});
		}
	}

	[CompilerGenerated]
	public class Class378<T> where T : Behaviour
	{
		public LocationScene scene;

		public Class377<T> class377_0;

		public bool method_0(T obj)
		{
			if (obj != null)
			{
				if (!class377_0.withDisabled)
				{
					return obj.isActiveAndEnabled;
				}
				return true;
			}
			Debug.LogError("Object of type " + typeof(T).Name + " on scene " + scene.gameObject.scene.name + " has been deleted. God, please fix it. Is unity null: " + (obj == null && (object)obj != null));
			return false;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct73
	{
		public Scene scene;
	}

	[CompilerGenerated]
	public class Class379
	{
		public Scene scene;

		public bool method_0(TreeWind x)
		{
			return x.gameObject.scene == scene;
		}
	}

	[CompilerGenerated]
	public class Class380<T>
	{
		public bool includeInactive;

		public IEnumerable<T> method_0(GameObject x)
		{
			return x.GetComponentsInChildren<T>(includeInactive);
		}
	}

	public StaticLoot[] StaticLoot;

	public LootableContainer[] LootableContainers;

	public WorldInteractiveObject[] WorldInteractiveObjects;

	public ISyncAble[] SyncAbles;

	public ControlledLampGroup[] ControlledLampGroups;

	public GameObject[] TriggerEntities;

	public NavMeshDoorLink[] NavMeshLinks;

	public SpawnPointMarker[] SpawnPointMarkers;

	public BotZone[] BotZones;

	public ExfiltrationPoint[] ExfiltrationPoints;

	public AIPlaceInfo[] AIPlaceInfos;

	public StationaryWeapon[] StationaryWeapons;

	public MovingPlatform[] MovingPlatforms;

	public BorderZone[] BorderZones;

	public BaseRestrictableZone[] RestrictableZones;

	public LampController[] Lamps;

	public WindowBreaker[] Windows;

	public SynchronizableObject[] SynchronizableObjects;

	public AirdropPoint[] AirdropPoints;

	public BufferZoneContainer[] BufferZoneContainers;

	public TransitPoint[] TransitPoints;

	public LocationOrigin[] LocationOrigins;

	public EventObject[] EventObjects;

	public FlameDamageTrigger[] FlameDamages;

	public EventEnvironment[] EventEnvironments;

	public HideoutController[] AreasControllers;

	public AudioSource[] AudioSources;

	public ClientBroadcastPlayer[] BroadcastPlayers;

	public MonoBehaviour[] EventSoundContentChangers;

	[HideInInspector]
	public TreeWind[] treeWinds;

	[HideInInspector]
	public TreeWind.Settings[] treeWindSettingsPresets;

	public static readonly List<LocationScene> LoadedScenes = new List<LocationScene>();

	public static readonly List<Collider> DoorsCollisionColliders = new List<Collider>();

	private readonly Dictionary<Type, Array> dictionary_0 = new Dictionary<Type, Array>();

	public int Int32_0 => treeWinds.Length;

	public void method_0<T>(T[] array)
	{
		dictionary_0.Add(typeof(T), array);
	}

	public T[] method_1<T>()
	{
		if (dictionary_0.TryGetValue(typeof(T), out var value) && value is T[] result)
		{
			return result;
		}
		return Array.Empty<T>();
	}

	public static IEnumerable<T> GetAll<T>()
	{
		return LoadedScenes.SelectMany((LocationScene scene) => scene.method_1<T>());
	}

	public static IEnumerable<T> GetAllObjects<T>(bool withDisabled = false) where T : Behaviour
	{
		return LoadedScenes.SelectMany((LocationScene scene) => scene.method_1<T>().Where(delegate(T obj)
		{
			if (obj != null)
			{
				if (!withDisabled)
				{
					return obj.isActiveAndEnabled;
				}
				return true;
			}
			Debug.LogError("Object of type " + typeof(T).Name + " on scene " + scene.gameObject.scene.name + " has been deleted. God, please fix it. Is unity null: " + (obj == null && (object)obj != null));
			return false;
		}));
	}

	public static IEnumerable<T> GetAllObjectsNoBehaviour<T>()
	{
		return LoadedScenes.SelectMany((LocationScene scene) => scene.method_1<T>());
	}

	public static IEnumerable<T> GetAllObjectsAndWhenISayAllIActuallyMeanIt<T>() where T : Behaviour
	{
		return LoadedScenes.SelectMany((LocationScene x) => x.method_1<T>());
	}

	public void Awake()
	{
		LoadedScenes.Add(this);
		method_0(StaticLoot);
		method_0(LootableContainers);
		method_0(WorldInteractiveObjects);
		if (SyncAbles != null)
		{
			method_0(SyncAbles);
		}
		if (TriggerEntities != null)
		{
			method_0((from te in TriggerEntities.Where((GameObject te) => te != null).SelectMany((GameObject te) => te.GetComponents<GInterface457>())
				where te != null
				select te).Distinct().ToArray());
		}
		if (ControlledLampGroups != null)
		{
			method_0(ControlledLampGroups);
		}
		method_0(NavMeshLinks);
		method_0(SpawnPointMarkers);
		method_0(BotZones);
		method_0(ExfiltrationPoints);
		method_0(AIPlaceInfos);
		method_0(StationaryWeapons);
		method_0(MovingPlatforms);
		method_0(BorderZones);
		method_0(RestrictableZones);
		method_0(Lamps);
		method_0(Windows);
		method_0(SynchronizableObjects);
		method_0(AirdropPoints);
		method_0(BufferZoneContainers);
		method_0(TransitPoints);
		method_0(LocationOrigins);
		method_0(EventObjects);
		method_0(FlameDamages);
		method_0(EventEnvironments);
		method_0(AreasControllers);
		if (AudioSources != null)
		{
			method_0(AudioSources);
		}
		if (BroadcastPlayers != null)
		{
			method_0(BroadcastPlayers);
		}
		if (EventSoundContentChangers != null)
		{
			method_0(EventSoundContentChangers.SelectMany((MonoBehaviour cc) => cc.GetComponents<GInterface104>()).Distinct().ToArray());
		}
		method_0(treeWinds);
		method_0(treeWindSettingsPresets);
		Type typeFromHandle = typeof(Behaviour);
		foreach (var (type2, array2) in dictionary_0)
		{
			if (typeFromHandle.IsAssignableFrom(type2))
			{
				method_2(type2, (Behaviour[])array2);
			}
		}
		WorldInteractiveObject[] worldInteractiveObjects = WorldInteractiveObjects;
		for (int num = 0; num < worldInteractiveObjects.Length; num++)
		{
			if (worldInteractiveObjects[num] is Door door)
			{
				DoorsCollisionColliders.AddRange(door.CollisionColliders);
			}
		}
	}

	public void method_2(Type type, Behaviour[] objects)
	{
		if (objects == null)
		{
			return;
		}
		StringBuilder stringBuilder = null;
		for (int i = 0; i < objects.Length; i++)
		{
			Behaviour behaviour = objects[i];
			bool flag2;
			bool flag = !(flag2 = (object)behaviour == null) && behaviour == null;
			if (flag2 || flag)
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder();
				}
				stringBuilder.AppendLine($"{i}: isRealNull:{flag2} isUnityNull:{flag}");
			}
		}
		if (stringBuilder != null)
		{
			Debug.LogErrorFormat(this, "LocationScene {0} has null {1}:\n{2}", base.gameObject.scene.name, type.Name, stringBuilder);
		}
	}

	public void OnDestroy()
	{
		LoadedScenes.Remove(this);
	}

	public void FillArrays()
	{
		FillArraysFrom(base.gameObject.scene);
	}

	public void FillArraysFrom(Scene scene)
	{
		Struct73 struct73_ = default(Struct73);
		struct73_.scene = scene;
		StaticLoot = smethod_1<StaticLoot>(includeInactive: true, ref struct73_);
		LootableContainers = smethod_1<LootableContainer>(includeInactive: true, ref struct73_);
		WorldInteractiveObjects = smethod_1<WorldInteractiveObject>(includeInactive: true, ref struct73_);
		SyncAbles = smethod_1<ISyncAble>(includeInactive: true, ref struct73_);
		ControlledLampGroups = smethod_1<ControlledLampGroup>(includeInactive: true, ref struct73_);
		TriggerEntities = (from te in smethod_1<GInterface457>(includeInactive: true, ref struct73_)
			where te.OutputTriggerIds.Any() || te.InputTriggerIds.Any()
			select te.GameObject).Distinct().ToArray();
		NavMeshLinks = smethod_1<NavMeshDoorLink>(includeInactive: true, ref struct73_);
		SpawnPointMarkers = smethod_1<SpawnPointMarker>(includeInactive: true, ref struct73_);
		BotZones = smethod_1<BotZone>(includeInactive: true, ref struct73_);
		ExfiltrationPoints = smethod_1<ExfiltrationPoint>(includeInactive: true, ref struct73_);
		AIPlaceInfos = smethod_1<AIPlaceInfo>(includeInactive: true, ref struct73_);
		StationaryWeapons = smethod_1<StationaryWeapon>(includeInactive: true, ref struct73_);
		MovingPlatforms = smethod_1<MovingPlatform>(includeInactive: true, ref struct73_);
		BorderZones = smethod_1<BorderZone>(includeInactive: false, ref struct73_);
		RestrictableZones = smethod_1<BaseRestrictableZone>(includeInactive: true, ref struct73_);
		Lamps = smethod_1<LampController>(includeInactive: true, ref struct73_);
		Windows = smethod_1<WindowBreaker>(includeInactive: false, ref struct73_);
		SynchronizableObjects = smethod_1<SynchronizableObject>(includeInactive: true, ref struct73_);
		AirdropPoints = smethod_1<AirdropPoint>(includeInactive: true, ref struct73_);
		BufferZoneContainers = smethod_1<BufferZoneContainer>(includeInactive: true, ref struct73_);
		TransitPoints = smethod_1<TransitPoint>(includeInactive: true, ref struct73_);
		LocationOrigins = smethod_1<LocationOrigin>(includeInactive: true, ref struct73_);
		EventObjects = smethod_1<EventObject>(includeInactive: true, ref struct73_);
		FlameDamages = smethod_1<FlameDamageTrigger>(includeInactive: false, ref struct73_);
		EventEnvironments = smethod_1<EventEnvironment>(includeInactive: true, ref struct73_);
		AreasControllers = smethod_1<HideoutController>(includeInactive: true, ref struct73_);
		AudioSources = smethod_1<AudioSource>(includeInactive: true, ref struct73_);
		BroadcastPlayers = smethod_1<ClientBroadcastPlayer>(includeInactive: true, ref struct73_);
		EventSoundContentChangers = (from cc in smethod_1<GInterface104>(includeInactive: true, ref struct73_)
			select cc as MonoBehaviour).Distinct().ToArray();
	}

	public void OnPreProcess()
	{
		method_3();
	}

	public void method_3()
	{
		Scene scene = base.gameObject.scene;
		treeWinds = (Application.isPlaying ? smethod_0<TreeWind>(ref scene) : (from x in UnityEngine.Object.FindObjectsOfType<TreeWind>()
			where x.gameObject.scene == scene
			select x).ToArray());
		HashSet<TreeWind.Settings> hashSet = new HashSet<TreeWind.Settings>();
		TreeWind[] array = treeWinds;
		foreach (TreeWind treeWind in array)
		{
			treeWind.FillSettings();
			hashSet.Add(treeWind.settings);
		}
		treeWindSettingsPresets = hashSet.ToArray();
	}

	public static T[] smethod_0<T>(ref Scene scene, bool includeInactive = true)
	{
		return scene.GetRootGameObjects().SelectMany((GameObject x) => x.GetComponentsInChildren<T>(includeInactive)).ToArray();
	}

	[CompilerGenerated]
	public static T[] smethod_1<T>(bool includeInactive = true, ref Struct73 struct73_0)
	{
		return smethod_0<T>(ref struct73_0.scene, includeInactive);
	}
}

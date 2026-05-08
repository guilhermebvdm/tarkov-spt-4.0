using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AnimationEventSystem;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.InventoryLogic;
using Sirenix.Utilities;
using UnityEngine;

public class GClass1041
{
	[Serializable]
	[CompilerGenerated]
	public class Class737
	{
		public static readonly Class737 class737_0 = new Class737();

		public static Func<ResourceKey, string> func_0;

		public static Func<Item, string> func_1;

		public static Func<Slot, Item> func_2;

		public static Func<Item, bool> func_3;

		public string method_0(ResourceKey x)
		{
			return x.path;
		}

		public string method_1(Item x)
		{
			return x.Prefab.path;
		}

		public Item method_2(Slot x)
		{
			return x.ContainedItem;
		}

		public bool method_3(Item x)
		{
			return x != null;
		}
	}

	[NonSerialized]
	public const int Int_0 = -1;

	[NonSerialized]
	public const int Int_1 = 0;

	[NonSerialized]
	public const int Int_2 = 1;

	[NonSerialized]
	public const int Int_3 = 2;

	[NonSerialized]
	public const int Int_4 = 3;

	[NonSerialized]
	public WeaponPrefab WeaponPrefab_0;

	[NonSerialized]
	public IAnimator Ianimator_0;

	[NonSerialized]
	public IViewFilter IViewFilter;

	[NonSerialized]
	public MenuPlayerPoser MenuPlayerPoser_0;

	[NonSerialized]
	public PlayerBody PlayerBody_0;

	[NonSerialized]
	public GameObject GameObject_0;

	[NonSerialized]
	public CustomizationSolverClass CustomizationSolverClass;

	[NonSerialized]
	public IEasyAssets IEasyAssets;

	[NonSerialized]
	public GClass2197 Gclass2197_0;

	[NonSerialized]
	public PoolManagerClass PoolManagerClass;

	[NonSerialized]
	public Animator Animator_0;

	[NonSerialized]
	public HashSet<string> HashSet_0 = new HashSet<string>();

	[NonSerialized]
	public List<global::DependencyGraphClass<IEasyBundle>.GClass1661> List_0 = new List<global::DependencyGraphClass<IEasyBundle>.GClass1661>();

	public void DisposeBody()
	{
		if (WeaponPrefab_0 != null)
		{
			AssetPoolObject.ReturnToPool(WeaponPrefab_0.gameObject);
			WeaponPrefab_0 = null;
		}
		for (int i = 0; i < List_0.Count; i++)
		{
			List_0[i].Release();
		}
		List_0.Clear();
		HashSet_0.Clear();
		if (PlayerBody_0 != null)
		{
			PlayerBody_0.Dispose();
			UnityEngine.Object.DestroyImmediate(PlayerBody_0.gameObject);
			PlayerBody_0 = null;
		}
	}

	public async Task<PlayerBody> Show(GClass932 playerIconRequest)
	{
		IEasyAssets = Singleton<IEasyAssets>.Instance;
		PoolManagerClass = Singleton<PoolManagerClass>.Instance;
		IViewFilter = GClass1856.Default;
		CustomizationSolverClass = Singleton<CustomizationSolverClass>.Instance;
		Gclass2197_0 = IViewFilter.FilterCustomization(playerIconRequest.customization);
		InventoryEquipment equipment = playerIconRequest.equipment;
		await method_0(equipment);
		playerIconRequest.Progress(0.6f);
		method_1();
		Item item = method_2(equipment);
		await PlayerBody_0.Init(Gclass2197_0, equipment, new global::BindableStateClass<Item>(item), LayerMaskClass.WeaponPreview, (EPlayerSide)0);
		playerIconRequest.Progress(0.7f);
		GameObject_0.transform.SetParent(null);
		GameObject_0.SetActive(value: true);
		Ianimator_0 = null;
		TransformLinks transformLinks = null;
		AnimationEventsEmitter eventsEmitter = null;
		if (item != null && !(item is GClass3365))
		{
			WeaponPrefab_0 = PoolManagerClass.CreateItem(item, isAnimated: true).GetComponent<WeaponPrefab>();
		}
		if (WeaponPrefab_0 != null)
		{
			Transform transform = WeaponPrefab_0.transform;
			transform.SetParent(MenuPlayerPoser_0.PBones.Ribcage.Original, worldPositionStays: false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			WeaponPrefab_0.gameObject.SetActive(value: true);
			transformLinks = WeaponPrefab_0.Hierarchy;
			Animator component = transformLinks.GetComponent<Animator>();
			Ianimator_0 = GClass1446.Create(component);
			TransformHelperClass.SetLayersRecursively(WeaponPrefab_0.gameObject, LayerMaskClass.WeaponPreview);
			eventsEmitter = WeaponPrefab_0.AnimationEventsEmitter;
		}
		int num = ((item == null || item is GClass3365) ? (-1) : ((item is PistolItemClass) ? 1 : ((item is ThrowWeapItemClass) ? 2 : ((item.GetItemComponent<KnifeComponent>() != null) ? 3 : 0))));
		Animator_0.SetLayerWeight(11, 1f);
		Animator_0.SetInteger(PlayerAnimator.WEAPON_TYPE, num);
		Animator_0.SetFloat(PlayerAnimator.WEAPON_TYPE_FLOAT_HASH, num);
		Animator_0.SetFloat(PlayerAnimator.UI_WEAPON_TYPE, num);
		Animator_0.Update(0f);
		if (Ianimator_0 != null)
		{
			Ianimator_0.enabled = true;
			Ianimator_0.Play("PATROL", 1, 0f);
			Ianimator_0.Update(3f);
		}
		if (item is Weapon { ChamberAmmoCount: var chamberAmmoCount })
		{
			WeaponAnimationSpeedControllerClass.SetAmmoInChamber(Ianimator_0, chamberAmmoCount);
		}
		MenuPlayerPoser_0.ChangeWeapon(eventsEmitter, Ianimator_0, transformLinks, canReload: false, canCheckChamber: true, animate: false);
		return PlayerBody_0;
	}

	public async Task method_0(InventoryEquipment equipment)
	{
		string[] array = new string[1] { "assets/content/commonprefabs/menuplayer.bundle" }.Concat(from x in Gclass2197_0.Values
			select CustomizationSolverClass.GetBundle(x) into x
			where x != null && !HashSet_0.Contains(x.path)
			select x.path).Concat(from x in Gclass2197_0
			select CustomizationSolverClass.GetWatchBundle(x.Value).WatchPrefab?.path into x
			where !string.IsNullOrEmpty(x) && !HashSet_0.Contains(x)
			select x).Concat(from x in equipment.GetAllVisibleItems()
			select x.Prefab.path into x
			where !string.IsNullOrEmpty(x) && !HashSet_0.Contains(x)
			select x)
			.ToArray();
		global::DependencyGraphClass<IEasyBundle>.GClass1661 gClass = GClass1857.Retain(IEasyAssets, array);
		await GClass1857.LoadBundles(gClass);
		HashSet_0.AddRange(array);
		List_0.Add(gClass);
	}

	public void method_1()
	{
		MenuPlayerPoser_0 = GClass1857.InstantiateAsset<MenuPlayerPoser>(IEasyAssets, "assets/content/commonprefabs/menuplayer.bundle");
		GameObject_0 = MenuPlayerPoser_0.gameObject;
		PlayerBody_0 = GameObject_0.GetComponent<PlayerBody>();
		Animator_0 = MenuPlayerPoser_0.GetComponent<Animator>();
		UnityEngine.Object.DestroyImmediate(MenuPlayerPoser_0.GetComponent<CharacterController>());
	}

	public Item method_2(InventoryEquipment equipment)
	{
		return new List<Slot>
		{
			equipment.GetSlot(EquipmentSlot.FirstPrimaryWeapon),
			equipment.GetSlot(EquipmentSlot.SecondPrimaryWeapon),
			equipment.GetSlot(EquipmentSlot.Holster),
			equipment.GetSlot(EquipmentSlot.Scabbard)
		}.Select((Slot x) => x.ContainedItem).FirstOrDefault((Item x) => x != null);
	}

	[CompilerGenerated]
	public ResourceKey method_3(MongoID x)
	{
		return CustomizationSolverClass.GetBundle(x);
	}

	[CompilerGenerated]
	public bool method_4(ResourceKey x)
	{
		if (x != null)
		{
			return !HashSet_0.Contains(x.path);
		}
		return false;
	}

	[CompilerGenerated]
	public string method_5(KeyValuePair<EBodyModelPart, MongoID> x)
	{
		return CustomizationSolverClass.GetWatchBundle(x.Value).WatchPrefab?.path;
	}

	[CompilerGenerated]
	public bool method_6(string x)
	{
		if (!string.IsNullOrEmpty(x))
		{
			return !HashSet_0.Contains(x);
		}
		return false;
	}

	[CompilerGenerated]
	public bool method_7(string x)
	{
		if (!string.IsNullOrEmpty(x))
		{
			return !HashSet_0.Contains(x);
		}
		return false;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using Diz.Binding;
using Diz.Skinning;
using EFT.AssetsManager;
using EFT.CameraControl;
using EFT.InventoryLogic;
using EFT.Visual;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace EFT;

public class PlayerBody : MonoBehaviour
{
	public interface GInterface233
	{
		void Init(GameObject prefab);
	}

	public class EquipmentSlotClass
	{
		[Serializable]
		[CompilerGenerated]
		public class Class1786
		{
			public static readonly Class1786 class1786_0 = new Class1786();

			public static Func<Item, Item, Item> func_0;

			public static Action action_0;

			public Item method_0(Item item, Item itemInHands)
			{
				if (item != null && itemInHands != null && item.Id == itemInHands.Id)
				{
					return null;
				}
				return item;
			}

			public void method_1()
			{
			}
		}

		[CompilerGenerated]
		public class Class1787
		{
			public EquipmentSlotClass EquipmentSlotClass;

			public EquipmentSlot equipmentSlot;

			public bool loadItemForce;

			public Action<Item> action_0;

			public void method_0(Item item)
			{
				EquipmentSlotClass.method_3();
				EquipmentSlotClass.method_2();
				if (equipmentSlot == EquipmentSlot.Scabbard && item is KnifeItemClass knifeItemClass && !knifeItemClass.KnifeComponent.Template.DisplayOnModel && !loadItemForce)
				{
					item = null;
				}
				if (item != null)
				{
					EquipmentSlotClass.Item_0 = item;
					EquipmentSlotClass.Action_1 = item.ChildrenChanged.Subscribe(delegate
					{
						EquipmentSlotClass.method_3();
						EquipmentSlotClass.method_0();
					});
					EquipmentSlotClass.method_0();
				}
				else
				{
					EquipmentSlotClass.DestroyCurrentModel();
				}
			}

			public void method_1(Item child)
			{
				EquipmentSlotClass.method_3();
				EquipmentSlotClass.method_0();
			}

			public void method_2(Item item)
			{
				if (EquipmentSlotClass.GameObject_0 != null)
				{
					EquipmentSlotClass.method_4(EquipmentSlotClass.PlayerBody_0, EquipmentSlotClass.GameObject_0);
				}
			}
		}

		[CompilerGenerated]
		public class Class1788
		{
			public GameObject result;

			public void method_0()
			{
				if (result != null)
				{
					AssetPoolObject.ReturnToPool(result);
				}
			}
		}

		[NonSerialized]
		public Slot Slot_0;

		[NonSerialized]
		public PlayerBody PlayerBody_0;

		[NonSerialized]
		[CanBeNull]
		public Transform Transform_0;

		[NonSerialized]
		[CanBeNull]
		public Slot Slot_1;

		[NonSerialized]
		[CanBeNull]
		public Transform Transform_1;

		public Task LoadingJob;

		public Dress[] Dresses;

		public readonly global::BindableStateClass<Dress> MainDress = new global::BindableStateClass<Dress>();

		public readonly global::BindableStateClass<GameObject> ParentedModel = new global::BindableStateClass<GameObject>(null);

		[NonSerialized]
		public Item Item_0;

		[NonSerialized]
		public GameObject GameObject_0;

		[NonSerialized]
		public CancellationTokenSource CancellationTokenSource_0;

		[NonSerialized]
		public Action Action_0;

		[NonSerialized]
		public Action Action_1;

		[NonSerialized]
		public Action Action_2;

		[NonSerialized]
		public Renderer[] Renderer_0 = Array.Empty<Renderer>();

		[NonSerialized]
		public GInterface233 Ginterface233_0;

		[NonSerialized]
		[CompilerGenerated]
		public EquipmentSlot EquipmentSlot_0;

		public GameObject Model => GameObject_0;

		public EquipmentSlot EquipmentSlot
		{
			[CompilerGenerated]
			get
			{
				return EquipmentSlot_0;
			}
			[CompilerGenerated]
			set
			{
				EquipmentSlot_0 = value;
			}
		}

		public global::BindableStateClass<Item> ContainedItem => Slot_0.ReactiveContainedItem;

		public Renderer[] Renderers => Renderer_0;

		public EquipmentSlotClass(PlayerBody playerBody, Slot slot, [CanBeNull] Transform bone, EquipmentSlot equipmentSlot, [CanBeNull] Slot backpackSlot = null, [CanBeNull] Transform alternativeHolsterBone = null, bool loadItemForce = false)
		{
			EquipmentSlotClass EquipmentSlotClass = this;
			PlayerBody_0 = playerBody;
			Slot_0 = slot;
			Transform_0 = bone;
			Slot_1 = backpackSlot;
			Transform_1 = alternativeHolsterBone;
			EquipmentSlot = equipmentSlot;
			IBindable<Item> bindable;
			if (playerBody._itemInHands == null)
			{
				IBindable<Item> containedItem = ContainedItem;
				bindable = containedItem;
			}
			else
			{
				IBindable<Item> containedItem = GClass1641.Combine(ContainedItem, playerBody._itemInHands, (Item item, Item itemInHands) => (item != null && itemInHands != null && item.Id == itemInHands.Id) ? null : item);
				bindable = containedItem;
			}
			IBindable<Item> bindable2 = bindable;
			Action_0 = bindable2.Bind(delegate(Item item)
			{
				EquipmentSlotClass.method_3();
				EquipmentSlotClass.method_2();
				if (equipmentSlot == EquipmentSlot.Scabbard && item is KnifeItemClass knifeItemClass && !knifeItemClass.KnifeComponent.Template.DisplayOnModel && !loadItemForce)
				{
					item = null;
				}
				if (item != null)
				{
					EquipmentSlotClass.Item_0 = item;
					EquipmentSlotClass.Action_1 = item.ChildrenChanged.Subscribe(delegate
					{
						EquipmentSlotClass.method_3();
						EquipmentSlotClass.method_0();
					});
					EquipmentSlotClass.method_0();
				}
				else
				{
					EquipmentSlotClass.DestroyCurrentModel();
				}
			});
			if (Slot_1 == null || Slot_1 == Slot_0)
			{
				return;
			}
			Action_2 = Slot_1.ReactiveContainedItem.Bind(delegate
			{
				if (EquipmentSlotClass.GameObject_0 != null)
				{
					EquipmentSlotClass.method_4(EquipmentSlotClass.PlayerBody_0, EquipmentSlotClass.GameObject_0);
				}
			});
		}

		public void SetPositionTuner(GInterface233 tuner)
		{
			Ginterface233_0 = tuner;
			if (GameObject_0 != null)
			{
				Ginterface233_0.Init(GameObject_0);
			}
		}

		public void Dispose()
		{
			method_3();
			method_2();
			DestroyCurrentModel();
			Action_0?.Invoke();
			Action_0 = null;
			Action_2?.Invoke();
			Action_2 = null;
		}

		public void method_0()
		{
			LoadingJob = method_1(Item_0);
		}

		public async Task method_1(Item item)
		{
			CancellationTokenSource_0 = new CancellationTokenSource();
			CancellationToken token = CancellationTokenSource_0.Token;
			await GClass1857.WaitForAllBundlesJob(GClass3380.GetAllBundleTokens(item), delegate
			{
			}, token);
			GameObject result = await Singleton<PoolManagerClass>.Instance.CreateItemAsync(item, ECameraType.Default, null, isAnimated: false, JobPriorityClass.General, token);
			CancellationTokenRegistration cancellationTokenRegistration = token.Register(delegate
			{
				if (result != null)
				{
					AssetPoolObject.ReturnToPool(result);
				}
			});
			CancellationTokenSource_0 = null;
			cancellationTokenRegistration.Dispose();
			if (!token.IsCancellationRequested)
			{
				DestroyCurrentModel();
				method_4(PlayerBody_0, result);
			}
		}

		public void method_2()
		{
			Item_0 = null;
			if (Action_1 != null)
			{
				Action_1();
				Action_1 = null;
			}
		}

		public void method_3()
		{
			if (CancellationTokenSource_0 != null)
			{
				CancellationTokenSource_0.Cancel();
				CancellationTokenSource_0 = null;
			}
		}

		public void DestroyCurrentModel()
		{
			if (Dresses != null)
			{
				Dress[] dresses = Dresses;
				for (int i = 0; i < dresses.Length; i++)
				{
					dresses[i].Unskin();
				}
				Dresses = null;
			}
			if (GameObject_0 != null)
			{
				if (PlayerBody_0._isYourPlayer)
				{
					Renderer[] renderer_ = Renderer_0;
					for (int i = 0; i < renderer_.Length; i++)
					{
						renderer_[i].enabled = true;
					}
				}
				AssetPoolObject.ReturnToPool(GameObject_0);
				GameObject_0 = null;
			}
			ParentedModel.Value = GameObject_0;
			Renderer_0 = Array.Empty<Renderer>();
			MainDress.Value = null;
		}

		public void ChangePositionAfterBackpackChanged()
		{
			if (Slot_1 != null && !(GameObject_0 == null))
			{
				method_4(PlayerBody_0, GameObject_0);
			}
		}

		public void method_4(PlayerBody playerBody, GameObject model)
		{
			Animator componentInChildren = model.GetComponentInChildren<Animator>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = false;
			}
			GameObject_0 = model;
			Renderer_0 = GameObject_0.GetComponentsInChildren<Renderer>(includeInactive: true);
			TransformHelperClass.SetLayersRecursively(GameObject_0, playerBody._layer, "Shells");
			WeaponPrefab component = GameObject_0.GetComponent<WeaponPrefab>();
			TransformLinks transformLinks = ((component != null) ? component._objectInstance.GetComponent<TransformLinks>() : null);
			Transform transform = ((transformLinks != null) ? transformLinks.GetTransform(ECharacterWeaponBones.Weapon_root) : null);
			if (Ginterface233_0 != null)
			{
				Ginterface233_0.Init(GameObject_0);
			}
			else if (transform != null)
			{
				Transform transform2 = ((!(Slot_1?.ContainedItem?.Template is BackpackTemplateClass backpackTemplateClass) || !(Transform_1 != null) || EquipmentSlot == EquipmentSlot.Holster) ? ((!(Transform_1 != null)) ? Transform_0 : ((EquipmentSlot != EquipmentSlot.Holster || !playerBody.HaveHolster || !playerBody.IsRightLegPistolHolster) ? Transform_1 : Transform_0)) : (backpackTemplateClass.LeanWeaponAgainstBody ? Transform_1 : Transform_0));
				bool flag = false;
				GameObject_0.transform.SetParent(transform2, worldPositionStays: false);
				GameObject_0.transform.localRotation = Quaternion.identity;
				GameObject_0.transform.localPosition = Vector3.zero;
				if (transform2 != null)
				{
					Quaternion quaternion = Quaternion.Inverse(transform.rotation) * transform2.rotation;
					GameObject_0.transform.localRotation *= quaternion;
					Vector3 vector = transform2.position - transform.position;
					GameObject_0.transform.position += vector;
					if (Item_0 is Weapon weapon)
					{
						flag = weapon.UseAltMountBone;
					}
					if (flag)
					{
						Transform transform3 = transformLinks.GetTransform(ECharacterWeaponBones.Weapon_root_alt);
						GameObject_0.transform.localPosition += transform3.localPosition;
						GameObject_0.transform.localRotation *= transform3.localRotation;
					}
				}
				else
				{
					Debug.LogError($"PlayerBody.CreateAndParent slotView.Bone == null for {GameObject_0} in {Slot_0}");
				}
				if (PlayerBody_0._isYourPlayer)
				{
					Renderer[] renderer_ = Renderer_0;
					for (int i = 0; i < renderer_.Length; i++)
					{
						renderer_[i].enabled = false;
					}
				}
				GameObject_0.SetActive(value: true);
			}
			else
			{
				DressItem component2 = GameObject_0.GetComponent<DressItem>();
				if (component2 != null)
				{
					GameObject_0.transform.SetParent(playerBody._meshTransform, worldPositionStays: false);
					GameObject_0.SetActive(value: true);
					Dress[] componentsInChildren = component2.DressPrefab.GetComponentsInChildren<Dress>(includeInactive: true);
					component2.EnableLoot(on: false);
					Dress[] array = componentsInChildren;
					foreach (Dress obj in array)
					{
						obj.Init(playerBody);
						obj.Skin(playerBody.PlayerBones.RootJoint, playerBody._meshTransform);
					}
					Dresses = componentsInChildren;
					MainDress.Value = componentsInChildren.FirstOrDefault();
				}
				else
				{
					Dress[] array2 = (Dresses = GameObject_0.GetComponentsInChildren<Dress>(includeInactive: true));
					MainDress.Value = array2.FirstOrDefault();
					Dress[] array = array2;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Init(playerBody);
					}
					GameObject_0.transform.SetParent(Transform_0, worldPositionStays: false);
					GameObject_0.transform.localRotation = new Quaternion(0.5f, -0.5f, -0.5f, -0.5f);
					GameObject_0.transform.localPosition = Vector3.zero;
					GameObject_0.SetActive(value: true);
				}
			}
			ParentedModel.Value = GameObject_0;
			playerBody.OnSlotViewChanged();
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1789
	{
		public static readonly Class1789 class1789_0 = new Class1789();

		public static Func<LoddedSkin, IEnumerable<Renderer>> func_0;

		public static Func<EquipmentSlotClass, bool> func_1;

		public static Func<EquipmentSlotClass, Task> func_2;

		public static Func<LoddedSkin, IEnumerable<Renderer>> func_3;

		public static Func<EquipmentSlotClass, bool> func_4;

		public static Func<EquipmentSlotClass, Task> func_5;

		public IEnumerable<Renderer> method_0(LoddedSkin x)
		{
			return x.GetRenderers();
		}

		public bool method_1(EquipmentSlotClass x)
		{
			Task loadingJob = x.LoadingJob;
			if (loadingJob == null)
			{
				return false;
			}
			return !loadingJob.IsCompleted;
		}

		public Task method_2(EquipmentSlotClass x)
		{
			return x.LoadingJob;
		}

		public IEnumerable<Renderer> method_3(LoddedSkin x)
		{
			return x.GetRenderers();
		}

		public bool method_4(EquipmentSlotClass x)
		{
			Task loadingJob = x.LoadingJob;
			if (loadingJob == null)
			{
				return false;
			}
			return !loadingJob.IsCompleted;
		}

		public Task method_5(EquipmentSlotClass x)
		{
			return x.LoadingJob;
		}
	}

	[SerializeField]
	private Transform _meshTransform;

	public PlayerBones PlayerBones;

	public Skeleton SkeletonRootJoint;

	public Skeleton SkeletonHands;

	public GClass2197 BodyCustomization;

	private int _layer;

	private EPlayerSide _side;

	private bool _active;

	public readonly Dictionary<EBodyModelPart, LoddedSkin> BodySkins = GClass866<EBodyModelPart>.GetDictWith<LoddedSkin>();

	private PluggableBone _watches;

	private BodyRendererDataStruct[] _bodyRenderers;

	private HoodedDress _hoodedDress;

	public bool IsRightLegPistolHolster;

	private InventoryEquipment _equipment;

	public static readonly EquipmentSlot[] SlotNames = new EquipmentSlot[11]
	{
		EquipmentSlot.ArmorVest,
		EquipmentSlot.TacticalVest,
		EquipmentSlot.Backpack,
		EquipmentSlot.Earpiece,
		EquipmentSlot.Eyewear,
		EquipmentSlot.Headwear,
		EquipmentSlot.FaceCover,
		EquipmentSlot.FirstPrimaryWeapon,
		EquipmentSlot.SecondPrimaryWeapon,
		EquipmentSlot.ArmBand,
		EquipmentSlot.Scabbard
	};

	public readonly GClass818<EquipmentSlot, EquipmentSlotClass> SlotViews = new GClass818<EquipmentSlot, EquipmentSlotClass>(SlotNames.Length);

	public EClippingCustoms CustomizationClipping;

	private global::BindableStateClass<Item> _itemInHands;

	public global::BindableStateClass<string> BodyCustomizationId = new global::BindableStateClass<string>();

	public global::BindableStateClass<EPlayerSide> PlayerSide = new global::BindableStateClass<EPlayerSide>();

	public global::BindableStateClass<EPointOfView> PointOfView = new global::BindableStateClass<EPointOfView>(EPointOfView.ThirdPerson);

	private string _playerProfileID;

	private CompositeDisposableClass _dispose;

	private bool _isYourPlayer;

	public bool HaveHolster { get; set; }

	public Transform MeshTransform => _meshTransform;

	public bool HasIntergratedArmor { get; set; }

	public InventoryEquipment Equipment { get; set; }

	public Task Init(GClass2197 customization, InventoryEquipment equipment, [CanBeNull] global::BindableStateClass<Item> itemInHands, int layer, EPlayerSide playerSide, string playerProfileID = "", Dictionary<EquipmentSlot, Transform> alternativeBones = null, bool isYourPlayer = false)
	{
		_playerProfileID = playerProfileID;
		_active = true;
		_layer = layer;
		_itemInHands = itemInHands;
		_equipment = equipment;
		BodyCustomization = customization;
		BodyCustomizationId.Value = customization[EBodyModelPart.Body];
		PlayerSide.Value = playerSide;
		Equipment = equipment;
		_isYourPlayer = isYourPlayer;
		EquipmentSlot[] slotNames = SlotNames;
		foreach (EquipmentSlot equipmentSlot in slotNames)
		{
			Transform bone;
			Transform alternativeHolsterBone;
			if (!GClass856.IsNullOrEmpty(alternativeBones) && alternativeBones.TryGetValue(equipmentSlot, out var value))
			{
				bone = value;
				alternativeHolsterBone = value;
			}
			else
			{
				bone = GetSlotBone(equipmentSlot);
				alternativeHolsterBone = GetAlternativeHolsterBone(equipmentSlot);
			}
			EquipmentSlotClass value2 = new EquipmentSlotClass(this, equipment.GetSlot(equipmentSlot), bone, equipmentSlot, equipment.GetSlot(EquipmentSlot.Backpack), alternativeHolsterBone, !GClass856.IsNullOrEmpty(alternativeBones) && alternativeBones.ContainsKey(EquipmentSlot.Scabbard));
			SlotViews.AddOrReplace(equipmentSlot, value2)?.Dispose();
		}
		CustomizationSolverClass instance = Singleton<CustomizationSolverClass>.Instance;
		HasIntergratedArmor = instance.HasIntegratedArmor(customization[EBodyModelPart.Body]);
		foreach (KeyValuePair<EBodyModelPart, MongoID> item in customization)
		{
			ResourceKey bundle = instance.GetBundle(item.Value);
			if (bundle != null)
			{
				SetSkin(new KeyValuePair<EBodyModelPart, ResourceKey>(item.Key, bundle), (item.Key == EBodyModelPart.Hands) ? SkeletonHands : SkeletonRootJoint);
				continue;
			}
			Debug.LogErrorFormat("No bundle for {0} | id {1}", item.Key, item.Value);
		}
		_bodyRenderers = new BodyRendererDataStruct[1]
		{
			new BodyRendererDataStruct
			{
				DecalType = EDecalTextureType.Blood,
				Renderers = BodySkins.Values.SelectMany((LoddedSkin x) => x.GetRenderers()).ToArray()
			}
		};
		if (BodySkins.TryGetValue(EBodyModelPart.Hands, out var value3))
		{
			GStruct431 watchBundle = instance.GetWatchBundle(customization[EBodyModelPart.Hands]);
			if (watchBundle.HasValidPath())
			{
				method_0(watchBundle, SkeletonHands);
			}
			value3.SetShadowCastingMode(ShadowCastingMode.Off);
		}
		HaveHolster = false;
		if (BodySkins.TryGetValue(EBodyModelPart.Feet, out var value4))
		{
			LegsView component = value4.GetComponent<LegsView>();
			if (component != null)
			{
				component.SetHolster(this);
				Transform value5;
				Transform bone2 = ((!GClass856.IsNullOrEmpty(alternativeBones) && alternativeBones.TryGetValue(EquipmentSlot.Holster, out value5)) ? value5 : (component.IsRightLegHolster ? PlayerBones.HolsterPistol : PlayerBones.LeftLegHolsterPistol));
				SlotViews.AddOrReplace(EquipmentSlot.Holster, new EquipmentSlotClass(this, equipment.GetSlot(EquipmentSlot.Holster), bone2, EquipmentSlot.Holster))?.Dispose();
				HaveHolster = true;
				IsRightLegPistolHolster = component.IsRightLegHolster;
			}
		}
		if (!HaveHolster && SlotViews.ContainsKey(EquipmentSlot.Holster))
		{
			SlotViews.GetByKey(EquipmentSlot.Holster)?.Dispose();
			SlotViews.Remove(EquipmentSlot.Holster);
		}
		_dispose = new CompositeDisposableClass();
		_dispose.AddDisposable(SlotViews.GetByKey(EquipmentSlot.Headwear).ParentedModel.Bind(method_1));
		_dispose.AddDisposable(SlotViews.GetByKey(EquipmentSlot.FaceCover).ParentedModel.Bind(method_1));
		method_1(null);
		return Task.WhenAll(from x in SlotViews.Where(delegate(EquipmentSlotClass x)
			{
				Task loadingJob = x.LoadingJob;
				return loadingJob != null && !loadingJob.IsCompleted;
			})
			select x.LoadingJob);
	}

	public Task Init(GClass2197 customization, int layer, EPlayerSide playerSide)
	{
		_active = true;
		_layer = layer;
		BodyCustomizationId.Value = customization[EBodyModelPart.Body];
		PlayerSide.Value = playerSide;
		CustomizationSolverClass instance = Singleton<CustomizationSolverClass>.Instance;
		HasIntergratedArmor = instance.HasIntegratedArmor(customization[EBodyModelPart.Body]);
		foreach (KeyValuePair<EBodyModelPart, MongoID> item in customization)
		{
			ResourceKey bundle = instance.GetBundle(item.Value);
			if (bundle != null)
			{
				SetSkin(new KeyValuePair<EBodyModelPart, ResourceKey>(item.Key, bundle), (item.Key == EBodyModelPart.Hands) ? SkeletonHands : SkeletonRootJoint);
				continue;
			}
			Debug.LogErrorFormat("No bundle for {0} | id {1}", item.Key, item.Value);
		}
		_bodyRenderers = new BodyRendererDataStruct[1]
		{
			new BodyRendererDataStruct
			{
				DecalType = EDecalTextureType.Blood,
				Renderers = BodySkins.Values.SelectMany((LoddedSkin x) => x.GetRenderers()).ToArray()
			}
		};
		return Task.WhenAll(from x in SlotViews.Where(delegate(EquipmentSlotClass x)
			{
				Task loadingJob = x.LoadingJob;
				return loadingJob != null && !loadingJob.IsCompleted;
			})
			select x.LoadingJob);
	}

	public void method_0(GStruct431 watchBundleInfo, Skeleton handsSkeleton)
	{
		_watches = GClass1857.InstantiateAsset<PluggableBone>(Singleton<IEasyAssets>.Instance, watchBundleInfo.WatchPrefab);
		_watches.Plug(handsSkeleton, watchBundleInfo.WatchPosition, watchBundleInfo.WatchRotation);
		_watches.GetComponent<Watch>().Init(new TimeSpan(0L));
	}

	public void UpdatePlayerRenders(EPointOfView pointOfView, EPlayerSide side)
	{
		foreach (var (eBodyModelPart2, loddedSkin2) in BodySkins)
		{
			switch (eBodyModelPart2)
			{
			case EBodyModelPart.Hands:
				loddedSkin2.EnableRenderers(GClass2078.IsFirstPerson(pointOfView));
				break;
			default:
				loddedSkin2.SetShadowCastingMode((pointOfView == EPointOfView.ThirdPerson) ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly);
				break;
			case EBodyModelPart.Feet:
				break;
			}
		}
		if (_watches != null)
		{
			_watches.gameObject.SetActive(GClass2078.IsFirstPerson(pointOfView));
		}
		PointOfView.Value = pointOfView;
		PlayerSide.Value = side;
	}

	public void OnSlotViewChanged()
	{
		if (!string.IsNullOrEmpty(_playerProfileID))
		{
			GlobalEventHandlerClass.Instance.CreateCommonEvent<GClass3558>().Invoke(_playerProfileID);
		}
	}

	public bool IsVisible()
	{
		if (!BodySkins[EBodyModelPart.Body].IsVisible())
		{
			return BodySkins[EBodyModelPart.Feet].IsVisible();
		}
		return true;
	}

	public void SetSkin(KeyValuePair<EBodyModelPart, ResourceKey> part, Skeleton skeleton)
	{
		LoddedSkin loddedSkin = GClass1857.InstantiateAsset<LoddedSkin>(Singleton<IEasyAssets>.Instance, part.Value);
		loddedSkin.Init(skeleton, this);
		loddedSkin.Skin();
		loddedSkin.SetLayer(_layer);
		loddedSkin.transform.SetParent(_meshTransform, worldPositionStays: false);
		if (loddedSkin.TryGetComponent<ClippingRuleChanger>(out var component))
		{
			CustomizationClipping |= component.GetClippingCustoms;
		}
		if (BodySkins.ContainsKey(part.Key))
		{
			BodySkins[part.Key].Unskin();
			UnityEngine.Object.DestroyImmediate(BodySkins[part.Key].gameObject);
		}
		if (loddedSkin.TryGetComponent<HoodedDress>(out var component2))
		{
			_hoodedDress = component2;
		}
		BodySkins[part.Key] = loddedSkin;
	}

	public void ValidateHoodedDress(EquipmentSlot changedSlot)
	{
		if (changedSlot == EquipmentSlot.Headwear || changedSlot == EquipmentSlot.FaceCover)
		{
			method_1(null);
		}
	}

	public void method_1([CanBeNull] GameObject parentModel)
	{
		if (!(_hoodedDress == null))
		{
			Slot slot = _equipment.GetSlot(EquipmentSlot.Headwear);
			if (slot.ContainedItem != null)
			{
				_hoodedDress.SetHooded(hooded: false);
				return;
			}
			Item containedItem = _equipment.GetSlot(EquipmentSlot.FaceCover).ContainedItem;
			SlotBlockerComponent component;
			bool flag = containedItem != null && (containedItem.ConflictingItems.Any() || (containedItem.TryGetItemComponent<SlotBlockerComponent>(out component) && component.ConflictingSlotNames.Contains(slot.Name)));
			_hoodedDress.SetHooded(!flag);
		}
	}

	public void GetBodyRenderersNonAlloc(List<BodyRendererDataStruct> preAllocatedRenederersList)
	{
		foreach (EquipmentSlotClass item2 in SlotViews.GetValuesEnumerator())
		{
			if (item2.Dresses != null)
			{
				Dress[] dresses = item2.Dresses;
				foreach (Dress dress in dresses)
				{
					preAllocatedRenederersList.Add(dress.GetBodyRenderer());
				}
			}
		}
		BodyRendererDataStruct[] bodyRenderers = _bodyRenderers;
		foreach (BodyRendererDataStruct item in bodyRenderers)
		{
			preAllocatedRenederersList.Add(item);
		}
	}

	public void GetRenderersNonAlloc(List<Renderer> preAllocatedRenederersList)
	{
		foreach (EquipmentSlotClass item in SlotViews.GetValuesEnumerator())
		{
			for (int i = 0; i < item.Renderers.Length; i++)
			{
				preAllocatedRenederersList.Add(item.Renderers[i]);
			}
			if (item.Dresses == null)
			{
				continue;
			}
			Dress[] dresses = item.Dresses;
			for (int j = 0; j < dresses.Length; j++)
			{
				BodyRendererDataStruct bodyRenderer = dresses[j].GetBodyRenderer();
				for (int k = 0; k < bodyRenderer.Renderers.Length; k++)
				{
					preAllocatedRenederersList.Add(bodyRenderer.Renderers[k]);
				}
			}
		}
		if (_bodyRenderers == null)
		{
			return;
		}
		BodyRendererDataStruct[] bodyRenderers = _bodyRenderers;
		for (int j = 0; j < bodyRenderers.Length; j++)
		{
			BodyRendererDataStruct bodyRendererDataStruct = bodyRenderers[j];
			for (int l = 0; l < bodyRendererDataStruct.Renderers.Length; l++)
			{
				preAllocatedRenederersList.Add(bodyRendererDataStruct.Renderers[l]);
			}
		}
	}

	public Transform GetSlotBone(EquipmentSlot slotType)
	{
		return slotType switch
		{
			EquipmentSlot.Holster => PlayerBones.HolsterPistol, 
			EquipmentSlot.Scabbard => PlayerBones.ScabbardTagillaMelee, 
			EquipmentSlot.ArmBand => PlayerBones.RightShoulder.Original, 
			EquipmentSlot.SecondPrimaryWeapon => PlayerBones.HolsterSecondary, 
			EquipmentSlot.FirstPrimaryWeapon => PlayerBones.HolsterPrimary, 
			_ => PlayerBones.Head.Original, 
		};
	}

	public Transform GetAlternativeHolsterBone(EquipmentSlot slotType)
	{
		return slotType switch
		{
			EquipmentSlot.Holster => PlayerBones.LeftLegHolsterPistol, 
			EquipmentSlot.Scabbard => PlayerBones.ScabbardTagillaMelee, 
			EquipmentSlot.SecondPrimaryWeapon => PlayerBones.HolsterSecondaryAlternative, 
			EquipmentSlot.FirstPrimaryWeapon => PlayerBones.HolsterPrimaryAlternative, 
			_ => null, 
		};
	}

	public void SetTemperatureForBody(float tempCelsio)
	{
		if (BodySkins.TryGetValue(EBodyModelPart.Head, out var value))
		{
			value.SetTemperature(tempCelsio);
		}
		if (BodySkins.TryGetValue(EBodyModelPart.Feet, out var value2))
		{
			value2.SetTemperature(tempCelsio);
		}
		if (BodySkins.TryGetValue(EBodyModelPart.Body, out var value3))
		{
			value3.SetTemperature(tempCelsio);
		}
		if (BodySkins.TryGetValue(EBodyModelPart.Hands, out var value4))
		{
			value4.SetTemperature(tempCelsio);
		}
	}

	public void Dispose()
	{
		if (_watches != null)
		{
			UnityEngine.Object.Destroy(_watches.gameObject);
		}
		foreach (EquipmentSlotClass item in SlotViews.GetValuesEnumerator())
		{
			item.Dispose();
		}
		SlotViews.Clear();
		_dispose?.Dispose();
		_bodyRenderers = null;
		foreach (KeyValuePair<EBodyModelPart, LoddedSkin> bodySkin in BodySkins)
		{
			bodySkin.Value.Unskin();
			UnityEngine.Object.DestroyImmediate(bodySkin.Value.gameObject);
		}
		BodySkins.Clear();
		_hoodedDress = null;
		_active = false;
		PointOfView = new global::BindableStateClass<EPointOfView>(EPointOfView.ThirdPerson);
	}

	public void OnApplicationQuit()
	{
		Dispose();
	}

	public void OnDestroy()
	{
		if (_active)
		{
			Debug.LogError("PlayerBody destroyed without being disposed. Please call Dispose before destroying.");
		}
	}

	public EquipmentSlotClass GetSlotViewByItem(Item item)
	{
		foreach (EquipmentSlotClass item2 in SlotViews.GetValuesEnumerator())
		{
			if (item2.ContainedItem.Value == item)
			{
				return item2;
			}
		}
		return null;
	}

	public string GetSlotViewsDebugString()
	{
		string text = "";
		EquipmentSlot[] slotNames = SlotNames;
		foreach (EquipmentSlot equipmentSlot in slotNames)
		{
			if (SlotViews.ContainsKey(equipmentSlot))
			{
				text += $"{equipmentSlot}: {SlotViews.GetByKey(equipmentSlot).ContainedItem.Value}\n";
			}
		}
		return text;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '10.1.0.8386' (yours is '10.0.1.8346')

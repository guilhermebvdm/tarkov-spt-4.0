using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AnimationEventSystem;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.InventoryLogic;
using EFT.Visual;
using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponPrefab : AssetPoolObject, GInterface31
{
	[Serializable]
	public class AimPlane
	{
		public string Name;

		public float Depth;
	}

	[Serializable]
	public class MaterialConfig
	{
		public string renderer;

		public Material material;
	}

	[Serializable]
	public class LODConfig
	{
		public float screenRelativeTransitionHeight;

		public float fadeTransitionWidth;

		public string[] renderers = Array.Empty<string>();
	}

	[Serializable]
	[CompilerGenerated]
	public class Class434
	{
		public static readonly Class434 class434_0 = new Class434();

		public static Func<GInterface137, int> func_0;

		public static Func<GInterface137, GInterface137> func_1;

		public static Func<GInterface137, int> func_2;

		public static Func<GInterface137, GInterface137> func_3;

		public int method_0(GInterface137 x)
		{
			return x.FullNameHash;
		}

		public GInterface137 method_1(GInterface137 x)
		{
			return x;
		}

		public int method_2(GInterface137 x)
		{
			return x.FullNameHash;
		}

		public GInterface137 method_3(GInterface137 x)
		{
			return x;
		}
	}

	public const string BONE_ALT_GRIP = "altpose";

	public const string BONE_SMOKEPORT = "smokeport";

	public const string BONE_FIREPORT = "fireport";

	public const string EXTRACTOR_GO_NAME = "extractor_smoke";

	private const string string_0 = "HIDE_SHADOW";

	[SerializeField]
	public GameObject _weaponObject;

	[SerializeField]
	public GameObject _weaponObjectSimple;

	[SerializeField]
	public RuntimeAnimatorController _originalAnimatorController;

	[SerializeField]
	public RuntimeAnimatorController _animatorSimple;

	[SerializeField]
	public RuntimeAnimatorController _animatorSpirit;

	[SerializeField]
	public TextAsset _fastAnimatorControllerBinaryData;

	[SerializeField]
	private Avatar _avatar;

	[SerializeField]
	public RestSettings RestSettings;

	public GameObject DefaultMuzzlePrefab;

	public GameObject DefaultSmokeport;

	public GameObject DefaultHeatHazeEffect;

	public Vector3 RecoilCenter;

	public Vector3 RotationCenter;

	public Vector3 RotationCenterNoStock;

	public Vector2 DupletAccuracyPenaltyX;

	public Vector2 DupletAccuracyPenaltyY;

	public AimPlane FarPlane = new AimPlane
	{
		Name = "farplane",
		Depth = 0.5f
	};

	public AimPlane DefaultAimPlane = new AimPlane
	{
		Name = "default",
		Depth = 0f
	};

	public AimPlane[] CustomAimPlanes;

	[SerializeField]
	public GameObject _objectInstance;

	[SerializeField]
	private Transform _localWeaponRoot;

	private IPlayer iplayer_0;

	private IAnimator ianimator_0;

	private IAnimator ianimator_1;

	private FirearmsAnimator firearmsAnimator_0;

	private FastAnimatorControllerClass fastAnimatorControllerClass;

	private AnimationEventsEmitter animationEventsEmitter_0;

	private GClass1346.GClass1347 gclass1347_0;

	private Weapon weapon_0;

	private Vector3 vector3_0;

	private Quaternion quaternion_0;

	private Vector3 vector3_1;

	private bool bool_3;

	private bool bool_4;

	private float float_0;

	private List<HotObject> list_0 = new List<HotObject>();

	private List<Material> list_1 = new List<Material>();

	private GunShadowDisabler[] gunShadowDisabler_0 = Array.Empty<GunShadowDisabler>();

	private float float_1 = -1f;

	private Animator animator_0;

	[Header("Extractor params")]
	public string[] RemoveChildrenOf;

	public string[] AnimatedBones;

	public TransformLinks Hierarchy;

	public GClass2086 ObjectInHands;

	public int[] LayersDefaultStates;

	public MaterialConfig[] MaterialsConfig = Array.Empty<MaterialConfig>();

	[Header("Extractor params for LODs")]
	public LODConfig[] LodsConfig = Array.Empty<LODConfig>();

	private Renderer[] renderer_0;

	private List<Mod> list_2 = new List<Mod>();

	public IAnimator Animator => ianimator_0;

	public FirearmsAnimator FirearmsAnimator => firearmsAnimator_0;

	public AnimationEventsEmitter AnimationEventsEmitter => animationEventsEmitter_0;

	public GameObject WeaponObject => _weaponObject;

	public RuntimeAnimatorController AnimatorController => _originalAnimatorController;

	public List<HotObject> HotObjects => list_0;

	public bool IsUnderbarrelWeaponPrefab => ResourceType.ItemTemplate is LauncherTemplateClass;

	public new Renderer[] Renderers
	{
		get
		{
			RecalculateObjectInstanceRenderers();
			return renderer_0 ?? Array.Empty<Renderer>();
		}
	}

	public float CurrentOverheat
	{
		get
		{
			if (weapon_0 == null)
			{
				return 0f;
			}
			if (Math.Abs(weapon_0.MalfState.LastShotTime - float_0) < Mathf.Epsilon)
			{
				return 0f;
			}
			float modsCoolFactor;
			float currentOverheat = weapon_0.GetCurrentOverheat(GClass1891.PastTime, Singleton<BackendConfigSettingsClass>.Instance.Overheat, list_2, out modsCoolFactor);
			if (currentOverheat < Mathf.Epsilon)
			{
				float_0 = weapon_0.MalfState.LastShotTime;
			}
			return currentOverheat;
		}
	}

	public void RecalculateObjectInstanceRenderers()
	{
		renderer_0 = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
	}

	public void SetUnderbarrelFastAnimator(Player player)
	{
		player._underbarrelFastAnimator = ianimator_0;
		ianimator_0.enabled = player.ArmsUpdateMode == Player.EUpdateMode.Auto;
		ianimator_0.updateMode = ((player.ArmsUpdateQueue == EUpdateQueue.FixedUpdate) ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal);
	}

	public void ResetUnderbarrelFastAnimator(Player player)
	{
		player._underbarrelFastAnimator = null;
	}

	public Transform Init(IPlayer player, bool parent)
	{
		iplayer_0 = player;
		CacheInternalObjects();
		if (!parent)
		{
			return null;
		}
		return method_2(player);
	}

	public void InitMalfunctionState(Weapon weapon, bool hasPlayer, bool malfunctionKnown, out AmmoPoolObject ammoPoolObject)
	{
		InitHotObjects(weapon);
		IAnimator animator = ianimator_1 ?? ianimator_0;
		WeaponAnimationSpeedControllerClass.SetMalfunctionType(animator, (int)weapon.MalfState.State);
		if (!malfunctionKnown && (weapon.MalfState.State == Weapon.EMalfunctionState.Misfire || weapon.MalfState.State == Weapon.EMalfunctionState.SoftSlide || weapon.MalfState.State == Weapon.EMalfunctionState.HardSlide))
		{
			WeaponAnimationSpeedControllerClass.SetMisfireSlideUnknown(animator, malfunctionKnown);
		}
		ammoPoolObject = null;
		WeaponAnimationSpeedControllerClass.SetMalfunction(animator, (int)weapon.MalfState.State);
		animator.SetLayerWeight(animator.GetLayerIndex("Malfunction"), 1f);
		AmmoItemClass malfunctionedAmmo = weapon.MalfState.MalfunctionedAmmo;
		GameObject gameObject = Singleton<PoolManagerClass>.Instance.CreateItem(malfunctionedAmmo, isAnimated: true);
		Transform shellParent = TransformHelperClass.FindActiveTransformRecursive(_localWeaponRoot, "shellport");
		ammoPoolObject = gameObject.GetComponent<AmmoPoolObject>();
		ammoPoolObject.SetUsed(weapon.MalfState.State != Weapon.EMalfunctionState.Feed);
		WeaponManagerClass.ParentShellToTransform(ammoPoolObject.gameObject, shellParent);
		if (!hasPlayer)
		{
			animator.SetLayerWeight(animator.GetLayerIndex("Malfunction"), 1f);
			if (weapon.GetFoldable() != null)
			{
				int layerIndex = animator.GetLayerIndex("Stock");
				if (layerIndex >= 0 && layerIndex < animator.layerCount)
				{
					animator.SetLayerWeight(layerIndex, weapon.Folded ? 1 : 0);
				}
				WeaponAnimationSpeedControllerClass.SetStockFolded(animator, weapon.Folded);
			}
			bool activeSelf = base.gameObject.activeSelf;
			base.gameObject.SetActive(value: true);
			animator.Play("IDLE", 1, 0.1f);
			animator.Update(0.01f);
			base.gameObject.SetActive(activeSelf);
		}
		WeaponAnimationSpeedControllerClass.SetMalfunction(animator, -1);
	}

	public void RevertMalfunctionState(Weapon weapon, bool hasPlayer, bool force = false)
	{
		InitHotObjects(weapon);
		if (!force && (hasPlayer || ianimator_0.GetLayerIndex("Malfunction") == -1))
		{
			return;
		}
		WeaponAnimationSpeedControllerClass.SetMalfunction(ianimator_0, 0);
		WeaponAnimationSpeedControllerClass.SetMalfunctionType(ianimator_0, 0);
		if (ianimator_0 == null || (ianimator_0 is GClass1446 gClass && gClass.Animator == null))
		{
			return;
		}
		if (weapon.GetFoldable() != null)
		{
			int layerIndex = ianimator_0.GetLayerIndex("Stock");
			if (layerIndex >= 0 && layerIndex < ianimator_0.layerCount)
			{
				ianimator_0.SetLayerWeight(layerIndex, weapon.Folded ? 1 : 0);
			}
			WeaponAnimationSpeedControllerClass.SetStockFolded(ianimator_0, weapon.Folded);
		}
		bool activeSelf = base.gameObject.activeSelf;
		base.gameObject.SetActive(value: true);
		method_1();
		ianimator_0.SetLayerWeight(ianimator_0.GetLayerIndex("Malfunction"), 0f);
		ianimator_0.Play("IDLE", 1, 0.1f);
		ianimator_0.Update(0.01f);
		base.gameObject.SetActive(activeSelf);
	}

	public void method_1()
	{
		GInterface137[] behaviours = ianimator_0.GetBehaviours<GInterface137>();
		for (int i = 0; i < behaviours.Length; i++)
		{
			behaviours[i].EventsContainer.ResetCache();
		}
	}

	public void InitHotObjects(Weapon weapon)
	{
		weapon_0 = weapon;
		list_2.Clear();
		GClass3380.GetAllItemsNonAlloc(weapon_0, list_2);
		method_6();
		method_7();
		method_8();
	}

	public Transform method_2([CanBeNull] IPlayer player)
	{
		if (player != null)
		{
			base.transform.parent = null;
		}
		base.gameObject.SetActive(value: true);
		if (ObjectInHands != null)
		{
			ObjectInHands.OnWeaponInit();
		}
		_objectInstance.transform.localPosition = Vector3.zero;
		_objectInstance.transform.localRotation = Quaternion.identity;
		firearmsAnimator_0 = new FirearmsAnimator();
		animationEventsEmitter_0 = new AnimationEventsEmitter();
		if (player != null)
		{
			player.SetArmsAnimatorCommon(ianimator_0);
			ianimator_0.enabled = player.ArmsUpdateMode == Player.EUpdateMode.Auto;
			ianimator_0.updateMode = ((player.ArmsUpdateQueue == EUpdateQueue.FixedUpdate) ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal);
			player.PlayerBones.WeaponRoot.Original = Hierarchy.GetTransform(ECharacterWeaponBones.Weapon_root);
			animationEventsEmitter_0.SetAnimator(ianimator_0, AnimationEventsEmitter.EEmitType.EmitOnDemand, player.ProfileId);
			if (BackendConfigAbstractClass.Config.UseSpiritPlayer && player is Player player2)
			{
				method_3(player2);
			}
			firearmsAnimator_0.SetAnimatorGetter(player.GetArmsAnimatorCommon);
		}
		else
		{
			ianimator_0.enabled = true;
			firearmsAnimator_0.SetAnimatorGetter(() => ianimator_0);
			animationEventsEmitter_0.SetAnimator(ianimator_0, AnimationEventsEmitter.EEmitType.EmitOnDemand);
		}
		animationEventsEmitter_0.OnEventAction += firearmsAnimator_0.AnimatorEventHandler;
		bool_3 = true;
		return _objectInstance.transform;
	}

	public void method_3(Player player)
	{
		bool flag = true;
		flag = ianimator_0.enabled;
		player.Spirit.InitArmsAnimator(ianimator_1, _animatorSpirit, ianimator_0, flag);
		Dictionary<int, GInterface137> dictionary = ianimator_0.GetBehaviours<GInterface137>().ToDictionary((GInterface137 x) => x.FullNameHash, (GInterface137 x) => x);
		Dictionary<int, GInterface137> dictionary2 = player.Spirit.ArmsAnimator.GetBehaviours<GInterface137>().ToDictionary((GInterface137 x) => x.FullNameHash, (GInterface137 x) => x);
		foreach (KeyValuePair<int, GInterface137> item in dictionary)
		{
			if (dictionary2.ContainsKey(item.Key))
			{
				dictionary2[item.Key].EventsContainer = item.Value.EventsContainer;
				continue;
			}
			Debug.LogErrorFormat("Fast Animator: key {0} not found for weapon {1}", item.Key, base.gameObject.name);
		}
		foreach (KeyValuePair<int, GInterface137> item2 in dictionary2)
		{
			_ = item2;
		}
	}

	public void RebindAnimator(IPlayer player)
	{
		animationEventsEmitter_0.RemoveBindedAnimator();
		ianimator_0.RebindBones();
		animationEventsEmitter_0.SetAnimator(ianimator_0, AnimationEventsEmitter.EEmitType.EmitOnDemand, player.ProfileId);
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer && player is Player player2)
		{
			method_3(player2);
		}
	}

	public override void ReturnToPool()
	{
		method_9();
		method_10();
		weapon_0 = null;
		if (animationEventsEmitter_0 != null)
		{
			animationEventsEmitter_0.Dispose();
			animationEventsEmitter_0 = null;
		}
		ianimator_0.enabled = false;
		if (BackendConfigAbstractClass.Config.UseSpiritPlayer)
		{
			ianimator_1.enabled = false;
		}
		method_4();
		if ((bool)Hierarchy)
		{
			Hierarchy.Self.localScale = new Vector3(1f, 1f, 1f);
			Hierarchy.ResetPositions();
		}
		if (base.transform.parent != null)
		{
			base.transform.parent = null;
		}
		firearmsAnimator_0 = null;
		base.ReturnToPool();
		bool_3 = false;
	}

	public void method_4()
	{
		if (ianimator_1 != null && !ianimator_1.Reset() && bool_3)
		{
			Debug.LogErrorFormat("Failed to reset _cachedSpiritAnimator for asset id {0}", GetHashCode());
		}
		if (!ianimator_0.Reset() && bool_3)
		{
			Debug.LogErrorFormat("Failed to reset _animator for asset id {0}", GetHashCode());
		}
		ResetStatesToDefault();
	}

	public void ResetStatesToDefault()
	{
		if (LayersDefaultStates == null)
		{
			Debug.LogError("LayersDefaultStates == null: " + base.Name, this);
			return;
		}
		if (ianimator_0 is GClass1446 gClass && !gClass.Animator.gameObject.activeInHierarchy)
		{
			if (bool_3)
			{
				Debug.LogError("Cant reset animator state: Game object with animator is inactive " + base.Name, this);
			}
			return;
		}
		if (LayersDefaultStates.Length != ianimator_0.layerCount)
		{
			if (bool_3)
			{
				Debug.LogErrorFormat(this, "LayersDefaultStates.Length {0} != _animator.layerCount {1} : {2}", LayersDefaultStates.Length, ianimator_0.layerCount, base.Name);
			}
			return;
		}
		for (int i = 0; i < ianimator_0.layerCount; i++)
		{
			int num = LayersDefaultStates[i];
			if (num != 0)
			{
				ianimator_0.Play(num, i, 0f);
			}
		}
		ianimator_0.Update(0f);
	}

	public override void OnCreatePoolRoleModel()
	{
		method_5();
		Hierarchy = _objectInstance.GetComponent<TransformLinks>();
		base.OnCreatePoolRoleModel();
	}

	public override void OnCreatePoolObject<TAssetPoolObject>([CanBeNull] global::AssetPoolAbstractClass<TAssetPoolObject> assetsPoolParent)
	{
		CacheInternalObjects();
		if (!(ResourceType.ItemTemplate is WeaponTemplate) && !(ResourceType.ItemTemplate is LauncherTemplateClass))
		{
			ObjectInHands = new GClass2086();
		}
		else
		{
			ObjectInHands = new WeaponManagerClass();
		}
		RegisterComponent(ObjectInHands);
		ObjectInHands.OnCreatePoolObjectInit(this);
		base.OnCreatePoolObject(assetsPoolParent);
	}

	public override void InheritRoleModelData<TAssetPoolObject>(TAssetPoolObject roleModel)
	{
		base.InheritRoleModelData(roleModel);
		WeaponPrefab weaponPrefab = roleModel as WeaponPrefab;
		if (weaponPrefab.gclass1347_0 != null)
		{
			gclass1347_0 = weaponPrefab.gclass1347_0;
		}
		CacheInternalObjects();
	}

	public void method_5()
	{
		if (!(_objectInstance == null))
		{
			return;
		}
		if (WeaponObject == null)
		{
			Debug.LogError("Invalid _weaponObject: " + base.name);
			return;
		}
		_objectInstance = UnityEngine.Object.Instantiate(WeaponObject);
		Animator component = _objectInstance.GetComponent<Animator>();
		if (component != null)
		{
			component.keepAnimatorStateOnDisable = true;
			component.runtimeAnimatorController = AnimatorController;
			component.enabled = false;
		}
		_objectInstance.transform.SetParent(base.transform);
		_objectInstance.transform.localPosition = Vector3.zero;
		_objectInstance.transform.localRotation = Quaternion.identity;
		if (_localWeaponRoot == null)
		{
			_localWeaponRoot = TransformHelperClass.FindTransform(_objectInstance.transform, "Weapon_root");
			if (_localWeaponRoot == null)
			{
				Debug.LogWarningFormat("Cant find Weapon_root on: {0}", base.gameObject.name);
			}
		}
	}

	public virtual void CacheInternalObjects()
	{
		if (bool_4)
		{
			return;
		}
		method_5();
		if (_objectInstance == null)
		{
			Debug.LogError("_objectInstance == null" + base.name);
			return;
		}
		animator_0 = _objectInstance.GetComponent<Animator>();
		ianimator_0 = GClass1445.CreateAnimator(_objectInstance.GetComponent<Animator>());
		if (BackendConfigAbstractClass.Config != null && BackendConfigAbstractClass.Config.UseSpiritPlayer && ianimator_1 == null)
		{
			GameObject gameObject = new GameObject("SpiritAnimatorGameObject");
			ianimator_1 = GClass1445.CreateAnimator(gameObject.AddComponent<Animator>());
			GClass6.ParentFake(gameObject.transform, _objectInstance.transform);
		}
		bool_4 = true;
	}

	public void method_6()
	{
		list_0.Clear();
		GetComponentsInChildren(includeInactive: true, list_0);
	}

	public void method_7()
	{
		if (iplayer_0 == null || !iplayer_0.IsYourPlayer)
		{
			return;
		}
		gunShadowDisabler_0 = GetComponentsInChildren<GunShadowDisabler>(includeInactive: true);
		for (int i = 0; i < gunShadowDisabler_0.Length; i++)
		{
			gunShadowDisabler_0[i].DisableGunShadow();
		}
		if (!gunShadowDisabler_0.Any())
		{
			return;
		}
		list_1.Clear();
		RecalculateObjectInstanceRenderers();
		for (int j = 0; j < Renderers.Length; j++)
		{
			Renderer renderer = Renderers[j];
			if (!(renderer == null) && renderer.materials != null)
			{
				for (int k = 0; k < renderer.materials.Length; k++)
				{
					Material material = renderer.materials[k];
					list_1.Add(material);
					material.EnableKeyword("HIDE_SHADOW");
				}
			}
		}
	}

	public void method_8()
	{
		VolumetricLight[] componentsInChildren = GetComponentsInChildren<VolumetricLight>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetWeaponFlashlight();
		}
	}

	public void method_9()
	{
		float temperatureCelsio = HotObject.ConvertHeat2Celsio(0f);
		for (int i = 0; i < list_0.Count; i++)
		{
			list_0[i].SetTemperatureToRenderer(temperatureCelsio, force: true);
		}
		list_0.Clear();
		float_1 = -1f;
	}

	public void method_10()
	{
		for (int i = 0; i < list_1.Count; i++)
		{
			list_1[i].DisableKeyword("HIDE_SHADOW");
		}
		list_1.Clear();
		for (int j = 0; j < gunShadowDisabler_0.Length; j++)
		{
			gunShadowDisabler_0[j].EnableGunShadow();
		}
	}

	public void OnEnable()
	{
		GClass862.RegisterInSystem(this);
	}

	public void OnDisable()
	{
		GClass862.UnregisterInSystem(this);
	}

	public void ManualUpdate()
	{
		float currentOverheat = CurrentOverheat;
		if (!(Math.Abs(currentOverheat - float_1) < 0.01f))
		{
			float_1 = currentOverheat;
			float temperatureCelsio = HotObject.ConvertHeat2Celsio(currentOverheat);
			for (int i = 0; i < list_0.Count; i++)
			{
				HotObject hotObject = list_0[i];
				hotObject.SetTemperatureToRenderer(temperatureCelsio);
				hotObject.PrepareForEffects();
			}
		}
	}

	public void UpdateAnimatorHierarchy()
	{
		animator_0.gameObject.SetActive(value: false);
		animator_0.gameObject.SetActive(value: true);
	}

	[CompilerGenerated]
	public IAnimator method_11()
	{
		return ianimator_0;
	}
}

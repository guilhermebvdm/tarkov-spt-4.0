using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.AssetsManager;
using EFT.InventoryLogic;
using UnityEngine;

public class WeaponManagerClass : GClass2086, GInterface210
{
	[Serializable]
	[CompilerGenerated]
	public class Class1382
	{
		public static readonly Class1382 class1382_0 = new Class1382();

		public static Func<Slot, bool> func_0;

		public static Func<GClass3248, IEnumerable<EFT.InventoryLogic.IContainer>> func_1;

		public bool method_0(Slot slot)
		{
			if (slot.ContainedItem != null)
			{
				return slot.ContainedItem.GetItemComponent<MuzzleComponent>() != null;
			}
			return false;
		}

		public IEnumerable<EFT.InventoryLogic.IContainer> method_1(GClass3248 x)
		{
			return x.Containers;
		}
	}

	[CompilerGenerated]
	public class Class1383
	{
		public RainCondensator[] rainCondensators;

		public bool method_0(RainCondensator x)
		{
			return rainCondensators.Contains(x);
		}
	}

	[CompilerGenerated]
	private Action<ESmoothScopeState> action_0;

	[CompilerGenerated]
	private Action<float> action_1;

	public const string SHELLPORT_TRANSFORM_NAME = "shellport";

	public const string PATRON_IN_WEAPON_TRANSFORM_NAME = "patron_in_weapon";

	[NonSerialized]
	public FirearmsEffects FirearmsEffects_0;

	[NonSerialized]
	public List<SightView> List_0 = new List<SightView>();

	[NonSerialized]
	public List<Transform> List_1 = new List<Transform>();

	[NonSerialized]
	public Transform[] Transform_0;

	[NonSerialized]
	public AmmoPoolObject[] AmmoPoolObject_0;

	[NonSerialized]
	public TacticalComboVisualController[] TacticalComboVisualController_0;

	[NonSerialized]
	public SightModVisualControllers[] SightModVisualControllers_0 = Array.Empty<SightModVisualControllers>();

	[NonSerialized]
	public LauncherViauslController[] LauncherViauslController_0;

	[NonSerialized]
	public BipodViewController BipodViewController_0;

	[NonSerialized]
	public List<AmmoPoolObject> List_2 = new List<AmmoPoolObject>();

	[NonSerialized]
	public Transform Transform_1;

	[NonSerialized]
	public WaitForEndOfFrame WaitForEndOfFrame_0 = new WaitForEndOfFrame();

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public AmmoPoolObject[] AmmoPoolObject_1;

	[NonSerialized]
	public List<RainCondensator> List_3;

	[NonSerialized]
	public BifacialTransform BifacialTransform_0;

	[NonSerialized]
	[CompilerGenerated]
	public Player Player_0;

	[NonSerialized]
	public ShellExtractionData ShellExtractionData_0;

	[NonSerialized]
	public Dictionary<Transform, GInterface280> Dictionary_0 = new Dictionary<Transform, GInterface280>();

	public static WeaponSounds WeaponSounds_0 => Singleton<BetterAudio>.Instance.MiscCollisionSounds;

	public Player Player
	{
		[CompilerGenerated]
		get
		{
			return Player_0;
		}
		[CompilerGenerated]
		set
		{
			Player_0 = value;
		}
	}

	public FirearmsEffects FirearmsEffects => FirearmsEffects_0;

	public BipodViewController BipodViewController => BipodViewController_0;

	public event Action<ESmoothScopeState> OnSmoothScopeStateChanged
	{
		[CompilerGenerated]
		add
		{
			Action<ESmoothScopeState> action = action_0;
			Action<ESmoothScopeState> action2;
			do
			{
				action2 = action;
				Action<ESmoothScopeState> value2 = (Action<ESmoothScopeState>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<ESmoothScopeState> action = action_0;
			Action<ESmoothScopeState> action2;
			do
			{
				action2 = action;
				Action<ESmoothScopeState> value2 = (Action<ESmoothScopeState>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<float> OnSmoothSensetivityChange
	{
		[CompilerGenerated]
		add
		{
			Action<float> action = action_1;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<float> action = action_1;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GameObject[] MuzzleTransforms()
	{
		if (!(FirearmsEffects_0 != null))
		{
			return Array.Empty<GameObject>();
		}
		return FirearmsEffects_0.Jets;
	}

	public override void OnCreatePoolObject(AssetPoolObject assetPoolObject)
	{
		base.OnCreatePoolObject(assetPoolObject);
		Bool_3 = WeaponPrefab_0.IsUnderbarrelWeaponPrefab;
		Transform_1 = (Bool_3 ? WeaponPrefab_0.transform : WeaponPrefab_0.Hierarchy.GetTransform(ECharacterWeaponBones.Weapon_root));
		FirearmsEffects_0 = WeaponPrefab_0.gameObject.AddComponent<FirearmsEffects>();
		FirearmsEffects_0.Init(Transform_1);
		List_0.AddRange(WeaponPrefab_0.gameObject.GetComponentsInChildren<SightView>());
		Int_0 = TransformHelperClass.smethod_4(Transform_1, "patron_in_weapon", false).Count;
		Bool_2 = Int_0 > 1;
		method_1();
		method_2();
		GClass985.CreateRainCondensators(Transform_1.GetChild(0).gameObject, enabled: false, cleanOld: false);
		ShellExtractionData_0 = WeaponPrefab_0.GetComponent<ShellExtractionData>();
	}

	public override void AfterGetFromPoolInit(ProceduralWeaponAnimation proceduralWeaponAnimation, GClass768 containerCollectionView, bool isYourPlayer)
	{
		base.AfterGetFromPoolInit(proceduralWeaponAnimation, containerCollectionView, isYourPlayer);
		if (Bool_0)
		{
			List_3 = Transform_1.GetChild(0).gameObject.GetComponentsInChildren<RainCondensator>().ToList();
			GClass985.SetEnabled(List_3, enabled: true);
		}
		method_12();
	}

	public void InitStreamingAnimators()
	{
		Dictionary<EFT.InventoryLogic.IContainer, GClass768.GClass769>.ValueCollection values = Gclass768_0.ContainerBones.Values;
		foreach (KeyValuePair<Transform, GInterface280> item in Dictionary_0)
		{
			item.Value.ClearData();
		}
		Dictionary_0.Clear();
		foreach (GClass768.GClass769 item2 in values)
		{
			if (!(item2.ItemView != null) || !item2.ItemView.TryGetComponent<GInterface280>(out var component))
			{
				continue;
			}
			Dictionary_0.Add(item2.ItemView, component);
			component.SetDonorAnimator(Player.HandsAnimator.Animator);
			if (component.EventBehaviours != null)
			{
				GInterface137[] eventBehaviours = component.EventBehaviours;
				foreach (GInterface137 behaviour in eventBehaviours)
				{
					WeaponPrefab_0.AnimationEventsEmitter.SubscribeToAnimatorEvent(behaviour);
				}
			}
		}
	}

	public override void OnReturnToPool(AssetPoolObject assetPoolObject)
	{
		base.OnReturnToPool(assetPoolObject);
		Player = null;
		List<RainCondensator> list_ = List_3;
		if (list_ != null)
		{
			GClass985.SetEnabled(list_, enabled: false);
		}
		List_3 = null;
		for (int i = 0; i < AmmoPoolObject_1.Length; i++)
		{
			if (AmmoPoolObject_1[i] != null)
			{
				AssetPoolObject.ReturnToPool(AmmoPoolObject_1[i].gameObject);
				AmmoPoolObject_1[i] = null;
			}
		}
		for (int j = 0; j < AmmoPoolObject_0.Length; j++)
		{
			DestroyPatronInWeapon(j);
		}
		ValidateScopeSmoothZoomUpdate(enableUpdate: false);
	}

	public void SetFireport(BifacialTransform fireport)
	{
		BifacialTransform_0 = fireport;
		method_0();
	}

	public void method_0()
	{
		if (BifacialTransform_0 != null)
		{
			GClass768.GClass769 gClass = Gclass768_0.GetBonesWhereSlot((Slot slot) => slot.ContainedItem != null && slot.ContainedItem.GetItemComponent<MuzzleComponent>() != null).FirstOrDefault();
			((gClass == null) ? new List<Transform>() : TransformHelperClass.smethod_4(gClass.Bone, "muzzleflash", true)).Add(BifacialTransform_0.Original);
		}
	}

	public void method_1()
	{
		if (Bool_2)
		{
			List<Transform> childs = new List<Transform>();
			List_1.AddRange(TransformHelperClass.FindChildsByNameRecNoneAlloc(Transform_1, "shellport", childs));
		}
		else if (Bool_3)
		{
			Transform item = TransformHelperClass.FindTransformRecursive(WeaponPrefab_0.transform, "shellport") ?? TransformHelperClass.FindTransformRecursive(WeaponPrefab_0.transform, "patron_in_weapon");
			List_1.Add(item);
		}
		else
		{
			List_1.Add(TransformHelperClass.FindActiveTransformRecursive(Transform_1, "shellport"));
		}
		Vector3_0 = List_1[0].localPosition;
	}

	public void method_2()
	{
		List<Transform> list = TransformHelperClass.smethod_4(Transform_1, "patron_in_weapon", false);
		bool flag;
		if (!(flag = Transform_0 == null))
		{
			foreach (Transform item in list)
			{
				flag |= !Transform_0.Contains(item);
			}
		}
		if (!flag)
		{
			return;
		}
		Transform_0 = list.ToArray();
		AmmoPoolObject_0 = new AmmoPoolObject[Transform_0.Length];
		AmmoPoolObject_1 = (Bool_2 ? new AmmoPoolObject[Int_0] : new AmmoPoolObject[1]);
		for (int i = 0; i < Transform_0.Length; i++)
		{
			Transform transform = Transform_0[i];
			if (transform.childCount > 0)
			{
				AssetPoolObject.ReturnToPool(transform.GetChild(0).gameObject);
			}
		}
		if (Transform_0 == null)
		{
			Debug.LogError("Cant find patron_in_weapon");
		}
	}

	public bool method_3()
	{
		if (!(CameraClass.Instance.Distance(WeaponPrefab_0.transform.position) < EFTHardSettings.Instance.PATRONS_MANIPULATIONS_VISIBLE_DISTANCE))
		{
			return Bool_2;
		}
		return true;
	}

	public bool method_4()
	{
		return CameraClass.Instance.Distance(WeaponPrefab_0.transform.position) < EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE;
	}

	public void SetRoundIntoWeapon(AmmoItemClass ammo, int chamberNumber = 0)
	{
		if (AmmoPoolObject_0[chamberNumber] != null)
		{
			Debug.LogWarning("Already have an ammo in chamber");
			DestroyPatronInWeapon(chamberNumber);
		}
		Transform transform = Transform_0[chamberNumber];
		if (transform.childCount > 0)
		{
			AssetPoolObject.ReturnToPool(transform.GetChild(0).gameObject);
		}
		AmmoPoolObject ammoPoolObject = smethod_0(ammo, Player);
		ammoPoolObject.SetUsed(ammo.IsUsed);
		ParentShellToTransform(ammoPoolObject.gameObject, transform);
		AmmoPoolObject_0[chamberNumber] = ammoPoolObject;
	}

	public bool HasPatronInWeapon(int chamberNumber = 0)
	{
		return AmmoPoolObject_0[chamberNumber] != null;
	}

	public bool HasShellInWeapon(int chamberNumber = 0)
	{
		return AmmoPoolObject_1 != null;
	}

	public bool DestroyPatronInWeapon(int chamberNumber = 0)
	{
		if (AmmoPoolObject_0[chamberNumber] == null)
		{
			return false;
		}
		AssetPoolObject.ReturnToPool(AmmoPoolObject_0[chamberNumber].gameObject);
		AmmoPoolObject_0[chamberNumber] = null;
		return true;
	}

	public void DestroyAllPatronsInWeapon()
	{
		for (int i = 0; i < AmmoPoolObject_0.Length; i++)
		{
			if (!(AmmoPoolObject_0[i] == null))
			{
				AssetPoolObject.ReturnToPool(AmmoPoolObject_0[i].gameObject);
				AmmoPoolObject_0[i] = null;
			}
		}
	}

	public void MoveAmmoFromChamberToShellPort(bool ammoIsUsed, int chamberIndex = 0)
	{
		AmmoPoolObject ammoPoolObject = AmmoPoolObject_0[chamberIndex];
		AmmoPoolObject_0[chamberIndex] = null;
		if (!(ammoPoolObject == null))
		{
			SetPatronInShellPort(ammoPoolObject, chamberIndex);
			if (method_3())
			{
				ammoPoolObject.SetUsed(ammoIsUsed);
			}
		}
	}

	public void CreatePatronInShellPort(AmmoItemClass ammo, int chamberIndex = 0)
	{
		if (method_3())
		{
			SetPatronInShellPort(smethod_0(ammo, Player), chamberIndex);
		}
	}

	public void SetPatronInShellPort(AmmoPoolObject ammoObject, int shellTransformIndex = 0)
	{
		if (AmmoPoolObject_1[shellTransformIndex] != null)
		{
			AssetPoolObject.ReturnToPool(AmmoPoolObject_1[shellTransformIndex].gameObject);
		}
		ammoObject.SetUsed(isUsed: true);
		AmmoPoolObject_1[shellTransformIndex] = ammoObject;
		ParentShellToTransform(ammoObject.gameObject, List_1[shellTransformIndex]);
	}

	public void RemoveAllShells()
	{
		for (int i = 0; i < AmmoPoolObject_1.Length; i++)
		{
			method_5(i);
		}
	}

	public void RemoveShellInWeapon(int shellIndex = 0)
	{
		if (method_3())
		{
			method_5(shellIndex);
		}
	}

	public void method_5(int shellIndex = 0)
	{
		if (!(AmmoPoolObject_1[shellIndex] == null))
		{
			AssetPoolObject.ReturnToPool(AmmoPoolObject_1[shellIndex].gameObject);
			AmmoPoolObject_1[shellIndex] = null;
		}
	}

	public void RemovePatronInWeapon(int chamberIndex = 0)
	{
		if (method_3())
		{
			DestroyPatronInWeapon(chamberIndex);
		}
	}

	public void SetupPatronInWeaponForJam(int chamberNumber = 0)
	{
		if (AmmoPoolObject_0[chamberNumber] != null)
		{
			AmmoPoolObject_0[chamberNumber].SetUsed(isUsed: true);
		}
	}

	public void SetVisiblePatronInWeapon(bool visible, int chamberNumber = 0)
	{
		if (Transform_0[chamberNumber] != null)
		{
			Transform_0[chamberNumber].gameObject.SetActive(visible);
		}
	}

	public void ThrowPatronAsLoot(Item patron, IPlayer player, string source)
	{
		if (patron == null)
		{
			Debug.LogError("Thrown patron is null. Source: " + source);
			return;
		}
		Vector3 angularVelocity = ShellExtractionData_0.PatronExtractionVector();
		Vector3 vector = ShellExtractionData_0.PatronExtractionAdditionalForce();
		Vector3 vector2 = (List_1[0].localPosition - Vector3_0) * ShellExtractionData_0.PatronExtractionForceMultiplier() + vector;
		vector2 = List_1[0].TransformVector(vector2);
		Vector3 position = List_1[0].position;
		Quaternion rotation = List_1[0].rotation;
		rotation *= Quaternion.Euler(90f, 0f, 0f);
		Singleton<GameWorld>.Instance.ThrowItem(patron, player, position, rotation, vector2, angularVelocity, syncable: false, performPickUpValidation: false);
	}

	public void InvokeShellCollision(Vector3 position, BaseBallistic.ESurfaceSound material, ECaliber caliber)
	{
		method_11(position, material, caliber);
	}

	public IEnumerator method_6(Vector3 playerVelocity, int shellPortIndex = 0)
	{
		if (AmmoPoolObject_1 != null)
		{
			AmmoPoolObject ammoPoolObject = AmmoPoolObject_1[shellPortIndex];
			AmmoPoolObject_1[shellPortIndex] = null;
			yield return WaitForEndOfFrame_0;
			if (!(ammoPoolObject == null))
			{
				method_9(playerVelocity, ammoPoolObject).SetUsed(isUsed: true);
			}
		}
	}

	public IEnumerator method_7(Vector3 playerVelocity)
	{
		if (AmmoPoolObject_1 == null)
		{
			yield break;
		}
		List_2.Clear();
		for (int i = 0; i < AmmoPoolObject_1.Length; i++)
		{
			AmmoPoolObject ammoPoolObject = AmmoPoolObject_1[i];
			AmmoPoolObject_1[i] = null;
			if (ammoPoolObject != null)
			{
				List_2.Add(ammoPoolObject);
			}
		}
		if (List_2.Count != 0)
		{
			yield return WaitForEndOfFrame_0;
			for (int j = 0; j < List_2.Count; j++)
			{
				method_9(playerVelocity, List_2[j]).SetUsed(isUsed: true);
			}
			List_2.Clear();
		}
	}

	public IEnumerator method_8(Vector3 playerVelocity)
	{
		if (AmmoPoolObject_1 != null)
		{
			AmmoPoolObject shell = AmmoPoolObject_1[0];
			AmmoPoolObject_1[0] = null;
			if (!(shell == null))
			{
				yield return WaitForEndOfFrame_0;
				AmmoPoolObject ammoPoolObject = Singleton<GameWorld>.Instance.SpawnShellInTheWorld(ref shell);
				ammoPoolObject.transform.parent = null;
				Vector3 misfireRotationVector = ShellExtractionData_0.GetMisfireRotationVector();
				Vector3 misfireAdditionalForce = ShellExtractionData_0.GetMisfireAdditionalForce();
				Vector3 force = (List_1[0].localPosition - Vector3_0) * ShellExtractionData_0.GetMisfireShellForceMultiplier() + misfireAdditionalForce;
				method_10(force, misfireRotationVector, ammoPoolObject, playerVelocity);
				ammoPoolObject.StartAutoDestroyCountDown();
				ammoPoolObject.SetUsed(isUsed: false);
			}
		}
	}

	public void StartSpawnShell(Vector3 playerVelocity, int shellPortIndex = 0)
	{
		if (method_4())
		{
			WeaponPrefab_0.StartCoroutine(method_6(playerVelocity, shellPortIndex));
		}
	}

	public void StartSpawnAllShells(Vector3 playerVelocity)
	{
		if (method_4())
		{
			WeaponPrefab_0.StartCoroutine(method_7(playerVelocity));
		}
	}

	public void StartSpawnMisfiredCartridge(Vector3 playerVelocity)
	{
		if (method_4())
		{
			WeaponPrefab_0.StartCoroutine(method_8(playerVelocity));
		}
	}

	public AmmoPoolObject method_9(Vector3 playerVelocity, AmmoPoolObject shell)
	{
		AmmoPoolObject ammoPoolObject = Singleton<GameWorld>.Instance.SpawnShellInTheWorld(ref shell);
		ammoPoolObject.transform.parent = null;
		Vector3 shotRotationVector = ShellExtractionData_0.GetShotRotationVector();
		Vector3 shotAdditionalForce = ShellExtractionData_0.GetShotAdditionalForce();
		Vector3 force = (List_1[0].localPosition - Vector3_0) * ShellExtractionData_0.GetShotShellForceMultiplier() + shotAdditionalForce;
		method_10(force, shotRotationVector, ammoPoolObject, playerVelocity);
		ammoPoolObject.StartAutoDestroyCountDown();
		return ammoPoolObject;
	}

	public void SpawnShellAfterJam(int chamberNumber = 0)
	{
		if (method_4() && (chamberNumber >= AmmoPoolObject_1.Length || !(AmmoPoolObject_1[chamberNumber] == null)))
		{
			AmmoPoolObject ammoPoolObject = Singleton<GameWorld>.Instance.SpawnShellInTheWorld(ref AmmoPoolObject_1[chamberNumber]);
			ammoPoolObject.transform.parent = null;
			Vector3 jamRotationVector = ShellExtractionData_0.GetJamRotationVector();
			Vector3 jamAdditionalForce = ShellExtractionData_0.GetJamAdditionalForce();
			Vector3 force = (List_1[chamberNumber].localPosition - Vector3_0) * ShellExtractionData_0.GetJamShellForceMultiplier() + jamAdditionalForce;
			method_10(force, jamRotationVector, ammoPoolObject, Vector3.zero);
			ammoPoolObject.SetUsed(isUsed: true);
			ammoPoolObject.StartAutoDestroyCountDown(3f);
		}
	}

	public void method_10(Vector3 force, Vector3 torque, AmmoPoolObject shell, Vector3 parentForce)
	{
		shell.EnablePhysics(force, torque, parentForce, List_1[0].transform.forward);
		shell.gameObject.layer = LayerMaskClass.ShellsLayer;
		shell.Shell.CollisionListener = this;
	}

	public void method_11(Vector3 position, BaseBallistic.ESurfaceSound material, ECaliber caliber)
	{
		SoundBank soundBank = null;
		switch (material)
		{
		case BaseBallistic.ESurfaceSound.Plastic:
			soundBank = caliber switch
			{
				ECaliber.ShellHeavy => WeaponSounds_0.ShellHeavyPlastic, 
				ECaliber.Shell12Cal => WeaponSounds_0.Shell12calPlastic, 
				ECaliber.Shell556Mm => WeaponSounds_0.Shell556mmPlastic, 
				ECaliber.Shell9Mm => WeaponSounds_0.Shell9mmPlastic, 
				_ => null, 
			};
			break;
		case BaseBallistic.ESurfaceSound.Metal:
			soundBank = caliber switch
			{
				ECaliber.ShellHeavy => WeaponSounds_0.ShellHeavyMetal, 
				ECaliber.Shell12Cal => WeaponSounds_0.Shell12calMetal, 
				ECaliber.Shell556Mm => WeaponSounds_0.Shell556mmMetal, 
				ECaliber.Shell9Mm => WeaponSounds_0.Shell9mmMetal, 
				_ => null, 
			};
			break;
		case BaseBallistic.ESurfaceSound.Wood:
			soundBank = caliber switch
			{
				ECaliber.ShellHeavy => WeaponSounds_0.ShellHeavyWood, 
				ECaliber.Shell12Cal => WeaponSounds_0.Shell12calWood, 
				ECaliber.Shell556Mm => WeaponSounds_0.Shell556mmWood, 
				ECaliber.Shell9Mm => WeaponSounds_0.Shell9mmWood, 
				_ => null, 
			};
			break;
		case BaseBallistic.ESurfaceSound.Grass:
			soundBank = caliber switch
			{
				ECaliber.ShellHeavy => WeaponSounds_0.ShellHeavySoil, 
				ECaliber.Shell12Cal => WeaponSounds_0.Shell12calSoil, 
				ECaliber.Shell556Mm => WeaponSounds_0.Shell556mmSoil, 
				ECaliber.Shell9Mm => WeaponSounds_0.Shell9mmSoil, 
				_ => null, 
			};
			break;
		case BaseBallistic.ESurfaceSound.Concrete:
		case BaseBallistic.ESurfaceSound.Asphalt:
			soundBank = caliber switch
			{
				ECaliber.ShellHeavy => WeaponSounds_0.ShellHeavyConcrete, 
				ECaliber.Shell12Cal => WeaponSounds_0.Shell12calConcrete, 
				ECaliber.Shell556Mm => WeaponSounds_0.Shell556mmConcrete, 
				ECaliber.Shell9Mm => WeaponSounds_0.Shell9mmConcrete, 
				_ => null, 
			};
			break;
		case BaseBallistic.ESurfaceSound.Soil:
		case BaseBallistic.ESurfaceSound.Gravel:
			soundBank = caliber switch
			{
				ECaliber.ShellHeavy => WeaponSounds_0.ShellHeavySoil, 
				ECaliber.Shell12Cal => WeaponSounds_0.Shell12calSoil, 
				ECaliber.Shell556Mm => WeaponSounds_0.Shell556mmSoil, 
				ECaliber.Shell9Mm => WeaponSounds_0.Shell9mmSoil, 
				_ => null, 
			};
			break;
		}
		if (!(soundBank == null))
		{
			Vector3 position2 = position + Vector3.up / 4f;
			EOcclusionTest occlusionTest = ((!(Player != null) || !Player.IsYourPlayer) ? EOcclusionTest.OneShotPropagation : EOcclusionTest.None);
			MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(position2, soundBank, (int)soundBank.SourceType, CameraClass.Instance.Distance(position), 1f, -1f, EnvironmentType.Outdoor, occlusionTest, oneShot: false, needUpdate: false);
		}
	}

	public static AmmoPoolObject smethod_0(Item ammo, IPlayer player)
	{
		GameObject gameObject = Singleton<PoolManagerClass>.Instance.CreateItem(ammo, Player.GetVisibleToCamera(player), player, isAnimated: true);
		AmmoPoolObject component = gameObject.GetComponent<AmmoPoolObject>();
		if (component == null)
		{
			throw new Exception("Error: gameobject " + gameObject?.ToString() + " doesn't have AmmoPoolObject component");
		}
		return component;
	}

	public static void ParentShellToTransform(GameObject shell, Transform shellParent)
	{
		shell.transform.position = shellParent.position;
		shell.transform.rotation = shellParent.rotation;
		shell.transform.localRotation *= Quaternion.Euler(90f, 0f, 0f);
		shell.transform.SetParent(shellParent, worldPositionStays: true);
		shell.transform.localScale = Vector3.one;
		shell.SetActive(value: true);
	}

	public void PlayShotEffects(bool isVisible, float sqrCameraDistance)
	{
		FirearmsEffects_0.StartFireEffects(isVisible, sqrCameraDistance);
	}

	public void PlayLauncherEffects()
	{
		FirearmsEffects_0.StartLauncherEffects();
	}

	public void UpdateScopes(bool isAdd)
	{
		foreach (SightView item in List_0)
		{
			if (isAdd)
			{
				item.Close();
			}
			else
			{
				item.Open();
			}
		}
	}

	public override void SetPointOfView(EPointOfView pointOfView)
	{
		base.SetPointOfView(pointOfView);
		if (null != FirearmsEffects_0)
		{
			FirearmsEffects_0.enabled = true;
		}
		else
		{
			Debug.LogWarning("No effects for " + WeaponPrefab_0.transform);
		}
		TacticalComboVisualController[] tacticalComboVisualController_ = TacticalComboVisualController_0;
		for (int i = 0; i < tacticalComboVisualController_.Length; i++)
		{
			tacticalComboVisualController_[i].SetPointOfView(pointOfView);
		}
	}

	public void SetupMod(Slot slot, GameObject modObject)
	{
		GClass768.GClass769 viewForSlot = Gclass768_0.GetViewForSlot(slot);
		viewForSlot.InsertItem(slot.ContainedItem, modObject);
		Gclass768_0.AddChildBones(modObject.transform);
		TransformHelperClass.SetLayersRecursively(modObject, LayerMask.NameToLayer("Player"), "Shells");
		method_1();
		method_2();
		if (Bool_0)
		{
			RainCondensator[] componentsInChildren = modObject.GetComponentsInChildren<RainCondensator>(includeInactive: true);
			GClass985.SetEnabled(componentsInChildren, enabled: true);
			List_3.AddRange(componentsInChildren);
		}
		if (modObject.TryGetComponent<GInterface280>(out var component))
		{
			component.SetDonorAnimator(Player.HandsAnimator.Animator);
			Dictionary_0.Add(viewForSlot.ItemView, component);
			if (component.EventBehaviours != null)
			{
				GInterface137[] eventBehaviours = component.EventBehaviours;
				foreach (GInterface137 behaviour in eventBehaviours)
				{
					WeaponPrefab_0.AnimationEventsEmitter.SubscribeToAnimatorEvent(behaviour);
				}
			}
		}
		SetPointOfView(EpointOfView_0);
		if (slot.ID == EWeaponModType.mod_scope.ToString())
		{
			UpdateScopes(isAdd: true);
		}
		method_12();
		method_0();
		if (slot.ID != EWeaponModType.mod_magazine.ToString())
		{
			base.ProceduralWeaponAnimation.FindAimTransforms();
		}
		if (slot.ID == EWeaponModType.mod_muzzle.ToString() || slot.ID == EWeaponModType.mod_tactical.ToString() || slot.ID == EWeaponModType.mod_muzzle_001.ToString())
		{
			FirearmsEffects_0.UpdateMuzzle();
		}
		if (slot.ID == EWeaponModType.mod_bipod.ToString())
		{
			base.ProceduralWeaponAnimation.FindBipodMountingPoint();
			base.ProceduralWeaponAnimation.UpdateBipodData();
		}
	}

	public void RemoveMod(Slot slot)
	{
		if (slot.ContainedItem is GClass3248 topLevelCollection)
		{
			Gclass768_0.RemoveBones(GClass3380.GetAllItemsFromCollection(topLevelCollection).Concat(new Item[1] { slot.ContainedItem }).OfType<GClass3248>()
				.SelectMany((GClass3248 x) => x.Containers));
		}
		GClass768.GClass769 viewForSlot = Gclass768_0.GetViewForSlot(slot);
		int index = viewForSlot.Bone.childCount - 1;
		Transform child = viewForSlot.Bone.GetChild(index);
		if (Dictionary_0.TryGetValue(viewForSlot.ItemView, out var value))
		{
			value.ClearData();
			Dictionary_0.Remove(viewForSlot.ItemView);
			if (value.EventBehaviours != null)
			{
				GInterface137[] eventBehaviours = value.EventBehaviours;
				foreach (GInterface137 behaviour in eventBehaviours)
				{
					WeaponPrefab_0.AnimationEventsEmitter.UnsubscribeToAnimatorEvent(behaviour);
				}
			}
		}
		if (Bool_0)
		{
			RainCondensator[] rainCondensators = child.gameObject.GetComponentsInChildren<RainCondensator>();
			GClass985.SetEnabled(rainCondensators, enabled: false);
			List_3.RemoveAll((RainCondensator x) => rainCondensators.Contains(x));
		}
		viewForSlot.RemoveItem();
		AssetPoolObject.ReturnToPool(child.gameObject);
		if (slot.ID == EWeaponModType.mod_scope.ToString())
		{
			UpdateScopes(isAdd: false);
		}
		else if (slot.ID == EWeaponModType.mod_muzzle.ToString() || slot.ID == EWeaponModType.mod_tactical.ToString() || slot.ID == EWeaponModType.mod_muzzle_001.ToString())
		{
			FirearmsEffects_0.UpdateMuzzle();
		}
		method_12();
		method_0();
	}

	public void SetAiming(bool aim)
	{
		base.ProceduralWeaponAnimation.IsAiming = aim;
		ValidateScopeSmoothZoomUpdate(!aim);
	}

	public void method_12()
	{
		SightModVisualControllers[] sightModVisualControllers_ = SightModVisualControllers_0;
		for (int i = 0; i < sightModVisualControllers_.Length; i++)
		{
			if (sightModVisualControllers_[i].TryGetZoomHandler(out var zoomHandler))
			{
				zoomHandler.OnSmoothSensetivityChange -= action_1;
				zoomHandler.OnSmoothScopeStateChanged -= action_0;
			}
		}
		TacticalComboVisualController_0 = GClass6.GetComponentsInChildrenActiveIgnoreFirstLevel<TacticalComboVisualController>(Transform_1).ToArray();
		SightModVisualControllers_0 = GClass6.GetComponentsInChildrenActiveIgnoreFirstLevel<SightModVisualControllers>(Transform_1).ToArray();
		LauncherViauslController_0 = GClass6.GetComponentsInChildrenActiveIgnoreFirstLevel<LauncherViauslController>(Transform_1).ToArray();
		BipodViewController_0 = GClass6.GetComponentsInChildrenActiveIgnoreFirstLevel<BipodViewController>(Transform_1).FirstOrDefault();
		sightModVisualControllers_ = SightModVisualControllers_0;
		foreach (SightModVisualControllers sightModVisualControllers in sightModVisualControllers_)
		{
			if (sightModVisualControllers.TryGetZoomHandler(out var zoomHandler2))
			{
				zoomHandler2.OnSmoothSensetivityChange += action_1;
				zoomHandler2.OnSmoothScopeStateChanged += action_0;
				zoomHandler2.Init(sightModVisualControllers.SightMod);
				zoomHandler2.SetUpdateEnable(updateEnable: true);
			}
		}
	}

	public Transform GetModTransform(EWeaponModType modType)
	{
		return TransformHelperClass.FindTransformRecursive(WeaponPrefab_0.Hierarchy.GetTransform(ECharacterWeaponBones.Weapon_root), modType.ToString()).GetChild(0).transform;
	}

	public void UpdateBeams()
	{
		TacticalComboVisualController[] tacticalComboVisualController_ = TacticalComboVisualController_0;
		for (int i = 0; i < tacticalComboVisualController_.Length; i++)
		{
			tacticalComboVisualController_[i].UpdateBeams(Player != null && Player.IsYourPlayer);
		}
	}

	public void ValidateScopeSmoothZoomUpdate(bool enableUpdate)
	{
		SightModVisualControllers[] sightModVisualControllers_ = SightModVisualControllers_0;
		for (int i = 0; i < sightModVisualControllers_.Length; i++)
		{
			if (sightModVisualControllers_[i].TryGetZoomHandler(out var zoomHandler))
			{
				zoomHandler.SetUpdateEnable(enableUpdate);
			}
		}
	}

	public void UpdateScopesMode()
	{
		SightModVisualControllers[] sightModVisualControllers_ = SightModVisualControllers_0;
		foreach (SightModVisualControllers obj in sightModVisualControllers_)
		{
			obj.UpdateSightMode();
			obj.ForceChangeScopeState();
		}
		LauncherViauslController[] launcherViauslController_ = LauncherViauslController_0;
		for (int i = 0; i < launcherViauslController_.Length; i++)
		{
			launcherViauslController_[i].UpdateSightMode();
		}
		base.ProceduralWeaponAnimation.OnScopesModeUpdated();
	}

	public void OpticCalibrationSwitchUp()
	{
		base.ProceduralWeaponAnimation.OpticCalibrationSwitchUp();
	}

	public void OpticCalibrationSwitchDown()
	{
		base.ProceduralWeaponAnimation.OpticCalibrationSwitchDown();
	}

	public override void OnWeaponInit()
	{
		base.OnWeaponInit();
		FirearmsEffects_0.UpdateMuzzle();
	}

	public void ModFinallyRemoved(Slot slot)
	{
		if (slot.ID != EWeaponModType.mod_magazine.ToString())
		{
			base.ProceduralWeaponAnimation.FindAimTransforms();
		}
	}
}

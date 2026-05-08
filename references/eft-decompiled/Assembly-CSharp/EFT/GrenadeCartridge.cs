using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Systems.Effects;
using Comfort.Common;
using EFT.AssetsManager;
using EFT.InventoryLogic;
using UnityEngine;

namespace EFT;

public class GrenadeCartridge : Throwable
{
	public delegate void GDelegate69(Player player, AmmoItemClass grenade, Item weapon, Vector3 position, GrenadeCartridge grenadeCartridge);

	private Player player_0;

	private AmmoItemClass ammoItemClass;

	private Item item_0;

	[CompilerGenerated]
	private GDelegate69 gdelegate69_0;

	private bool bool_2;

	private WeaponSounds weaponSounds_0;

	private const float float_3 = 76f;

	public override int Id => GClass1298.GetStableHashCode(ammoItemClass.Id);

	public override bool HasNetData => false;

	public event GDelegate69 BlowUpEvent
	{
		[CompilerGenerated]
		add
		{
			GDelegate69 gDelegate = gdelegate69_0;
			GDelegate69 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate69 value2 = (GDelegate69)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate69_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate69 gDelegate = gdelegate69_0;
			GDelegate69 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate69 value2 = (GDelegate69)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate69_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public void Awake()
	{
	}

	public void Init(Player player, AmmoItemClass grenadeCartridgeAmmo, Item weapon)
	{
		ammoItemClass = grenadeCartridgeAmmo;
		item_0 = weapon;
		player_0 = player;
		CapsuleCollider capsuleCollider = base.gameObject.AddComponent<CapsuleCollider>();
		capsuleCollider.radius = 0.03f;
		capsuleCollider.center = new Vector3(0f, 0f, -0.005f);
		capsuleCollider.height = 0.12f;
		capsuleCollider.direction = 2;
		Rigidbody = base.gameObject.GetComponent<Rigidbody>();
		if (Rigidbody == null)
		{
			Rigidbody = base.gameObject.AddComponent<Rigidbody>();
		}
		EFTPhysicsClass.GClass745.SupportRigidbody(Rigidbody);
		Rigidbody.isKinematic = false;
		Rigidbody.mass = ammoItemClass.TotalWeight;
		Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		weaponSounds_0 = GClass1857.InstantiateAsset<WeaponSounds>(Singleton<IEasyAssets>.Instance, "assets/content/audio/prefabs/shells/weaponsounds.bundle");
		bool_2 = false;
		base.enabled = true;
	}

	public void Launch()
	{
		Rigidbody.velocity = 76f * base.transform.forward;
	}

	public void method_2()
	{
		Singleton<Effects>.Instance.Emit("Grenade_indoor", base.transform.position, Vector3.up);
		if (gdelegate69_0 != null)
		{
			gdelegate69_0(player_0, ammoItemClass, item_0, base.transform.position, this);
		}
		AssetPoolObject.ReturnToPool(base.gameObject, immediate: false);
		base.enabled = false;
		Singleton<BetterAudio>.Instance.PlayAtPoint(base.transform.position, weaponSounds_0.GrenadeDropSoundBank, CameraClass.Instance.Distance(base.transform.position));
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (base.enabled)
		{
			bool_2 = true;
		}
	}

	public void Update()
	{
		if (bool_2)
		{
			method_2();
			bool_2 = false;
		}
	}
}

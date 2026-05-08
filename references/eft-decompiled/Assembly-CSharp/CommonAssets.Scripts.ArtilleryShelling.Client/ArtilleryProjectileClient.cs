using System;
using System.Runtime.CompilerServices;
using Systems.Effects;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace CommonAssets.Scripts.ArtilleryShelling.Client;

public class ArtilleryProjectileClient : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class1003
	{
		public static readonly Class1003 class1003_0 = new Class1003();

		public ISharedBallisticsCalculator method_0()
		{
			return Singleton<GInterface169>.Instance.CreateBallisticCalculator(0);
		}
	}

	public int id;

	public Action<ArtilleryProjectileClient> ExplosionEvent;

	private Vector3 vector3_0;

	private bool bool_0;

	private const float float_0 = 0.3f;

	private const string string_0 = "Artillery_mine";

	private MineDataClass mineDataClass;

	private bool bool_1;

	private static Lazy<ISharedBallisticsCalculator> lazy_0 = new Lazy<ISharedBallisticsCalculator>(() => Singleton<GInterface169>.Instance.CreateBallisticCalculator(0));

	public ArtilleryProjectileClient()
	{
		mineDataClass = new MineDataClass();
	}

	public void Init(int projectileId, bool hasAuthority)
	{
		bool_1 = hasAuthority;
		id = projectileId;
		bool_0 = true;
		base.gameObject.SetActive(value: true);
	}

	public void Deinit()
	{
		id = 0;
		bool_0 = false;
		vector3_0 = Vector3.zero;
		base.gameObject.SetActive(value: false);
	}

	public void Update()
	{
		if (bool_0)
		{
			method_0();
		}
	}

	public void SyncProjectileState(ref ArtilleryPacketStruct packet)
	{
		vector3_0 = packet.position;
		if (packet.explosion)
		{
			method_1();
		}
	}

	public void method_0()
	{
		Vector3 vector = Vector3.Lerp(base.transform.position, vector3_0, 0.3f);
		base.transform.LookAt(vector);
		base.transform.position = vector;
	}

	public void method_1()
	{
		base.transform.position = vector3_0;
		Singleton<Effects>.Instance.EmitGrenade("Artillery_mine", vector3_0, Vector3.up);
		method_2(mineDataClass, base.transform.position, lazy_0.Value);
		ExplosionEvent?.Invoke(this);
		Deinit();
	}

	public void method_2(MineDataClass mineData, Vector3 explosionPosition, ISharedBallisticsCalculator ballisticsCalculator)
	{
		if (!mineData.IsDummy && !bool_1)
		{
			GClass2085.Explosion(mineData, explosionPosition, null, ballisticsCalculator, null, method_3, mineData.GetDirectionalDamageMultiplier, mineData.GetDirectionalDamageAngle, base.transform.forward, deadlyMinDistance: false);
		}
	}

	public void SetExplosiveItemParams(BackendConfigSettingsClass.ArtilleryProjectileExplosionParams explosiveParams)
	{
		mineDataClass.SetExplosiveItemParams(explosiveParams);
	}

	public DamageInfoStruct method_3()
	{
		return new DamageInfoStruct
		{
			DamageType = EDamageType.Artillery,
			ArmorDamage = mineDataClass.ArmorDamage,
			StaminaBurnRate = mineDataClass.StaminaBurnRate,
			PenetrationPower = mineDataClass.PenetrationPower,
			Direction = Vector3.zero,
			Player = null,
			IsForwardHit = true
		};
	}
}

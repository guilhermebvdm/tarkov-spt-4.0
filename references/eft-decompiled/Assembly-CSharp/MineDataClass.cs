using System;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class MineDataClass : IExplosiveItem
{
	[NonSerialized]
	public static string String_0 = "5485a8684bdc2da71d8b4567";

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1 = new Vector3(2f, 4f, 14f);

	[NonSerialized]
	public Vector3 Vector3_2 = new Vector3(1f, 4f, 29f);

	[NonSerialized]
	public float Float_0 = 4f;

	[NonSerialized]
	public float Float_1 = 4f;

	[NonSerialized]
	public float Float_2 = 6f;

	[NonSerialized]
	public int Int_0 = 75;

	[NonSerialized]
	public float Float_3 = 110f;

	[NonSerialized]
	public string String_1;

	[NonSerialized]
	public float Float_4 = 10f;

	[NonSerialized]
	public float Float_5 = 1f;

	[NonSerialized]
	public float Float_6 = 2f;

	[NonSerialized]
	public string String_2;

	[NonSerialized]
	public string String_3;

	[NonSerialized]
	public WildSpawnType WildSpawnType_0;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	public Vector3 Blindness => Vector3_0;

	public Vector3 Contusion => Vector3_1;

	public Vector3 ArmorDistanceDistanceDamage => Vector3_2;

	public float DeadlyDistance => Float_0;

	public float MinExplosionDistance => Float_1;

	public float MaxExplosionDistance => Float_2;

	public int FragmentsCount => Int_0;

	public float GetStrength => Float_3;

	public string FragmentType => String_2;

	public float ArmorDamage => Float_4;

	public float StaminaBurnRate => Float_5;

	public float PenetrationPower => Float_6;

	public float GetDirectionalDamageAngle => Float_7;

	public float GetDirectionalDamageMultiplier => Float_8;

	public bool IsDummy
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
	}

	public AmmoItemClass CreateFragment()
	{
		if (!Singleton<ItemFactoryClass>.Instance.ItemTemplates.ContainsKey(String_0))
		{
			return null;
		}
		return (AmmoItemClass)Singleton<ItemFactoryClass>.Instance.CreateItem(Guid.NewGuid().ToString(), String_0, null);
	}

	public void SetExplosiveItemParams(BackendConfigSettingsClass.ArtilleryProjectileExplosionParams explosiveParams)
	{
		Vector3_0 = explosiveParams.Blindness;
		Vector3_1 = explosiveParams.Contusion;
		Vector3_2 = explosiveParams.ArmorDistanceDistanceDamage;
		Float_0 = explosiveParams.DeadlyDistance;
		Float_1 = explosiveParams.MinExplosionDistance;
		Float_2 = explosiveParams.MaxExplosionDistance;
		String_2 = explosiveParams.FragmentType;
		Float_3 = explosiveParams.Strength;
		Float_4 = explosiveParams.ArmorDamage;
		Float_5 = explosiveParams.StaminaBurnRate;
		Float_6 = explosiveParams.PenetrationPower;
		Float_7 = explosiveParams.DirectionalDamageAngle;
		Float_8 = explosiveParams.DirectionalDamageMultiplier;
		Int_0 = explosiveParams.FragmentsCount;
	}
}

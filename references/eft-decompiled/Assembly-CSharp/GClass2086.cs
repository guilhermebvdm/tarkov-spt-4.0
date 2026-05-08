using System;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Animations;
using EFT.AssetsManager;

public class GClass2086 : GInterface504
{
	[NonSerialized]
	public WeaponPrefab WeaponPrefab_0;

	[NonSerialized]
	public EPointOfView EpointOfView_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public ProceduralWeaponAnimation ProceduralWeaponAnimation_0;

	[NonSerialized]
	public GClass768 Gclass768_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	public WeaponPrefab WeaponPrefab => WeaponPrefab_0;

	public ProceduralWeaponAnimation ProceduralWeaponAnimation
	{
		[CompilerGenerated]
		get
		{
			return ProceduralWeaponAnimation_0;
		}
		[CompilerGenerated]
		set
		{
			ProceduralWeaponAnimation_0 = value;
		}
	}

	public virtual bool enabled
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public virtual void OnCreatePoolObjectInit(WeaponPrefab weaponPrefab)
	{
		WeaponPrefab_0 = weaponPrefab;
	}

	public virtual void AfterGetFromPoolInit(ProceduralWeaponAnimation proceduralWeaponAnimation, GClass768 containerCollectionView, bool isYourPlayer)
	{
		ProceduralWeaponAnimation = proceduralWeaponAnimation;
		Gclass768_0 = containerCollectionView;
		Bool_0 = isYourPlayer;
	}

	public virtual void SetPointOfView(EPointOfView pointOfView)
	{
		EpointOfView_0 = pointOfView;
	}

	public virtual void OnCreatePoolRoleModel(AssetPoolObject assetPoolObject)
	{
	}

	public virtual void OnCreatePoolObject(AssetPoolObject assetPoolObject)
	{
	}

	public virtual void OnGetFromPool(AssetPoolObject assetPoolObject)
	{
	}

	public virtual void OnReturnToPool(AssetPoolObject assetPoolObject)
	{
		ProceduralWeaponAnimation = null;
		Gclass768_0?.Dispose();
		Gclass768_0 = null;
	}

	public virtual void InheritRoleModelData(GInterface504 roleModel)
	{
	}

	public virtual void OnWeaponInit()
	{
	}
}

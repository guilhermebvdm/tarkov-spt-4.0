using System;
using CommonAssets.Scripts.ArtilleryShelling.Client;
using UnityEngine;

public class GClass1510
{
	[NonSerialized]
	public const string String_0 = "weapon_mine_om_82mm_template";

	[NonSerialized]
	public GClass835<ArtilleryProjectileClient> Gclass835_0;

	[NonSerialized]
	public UnityEngine.Object Object_0;

	[NonSerialized]
	public BackendConfigSettingsClass.ArtilleryShellingGlobalSettings ArtilleryShellingGlobalSettings_0;

	public void InitPool(int capacity, BackendConfigSettingsClass.ArtilleryShellingGlobalSettings artilleryShellingSettings)
	{
		ArtilleryShellingGlobalSettings_0 = artilleryShellingSettings;
		Object_0 = Resources.Load("weapon_mine_om_82mm_template");
		Gclass835_0 = new GClass835<ArtilleryProjectileClient>(capacity, method_0);
	}

	public ArtilleryProjectileClient GetProjectile()
	{
		ArtilleryProjectileClient artilleryProjectileClient = Gclass835_0.Withdraw();
		artilleryProjectileClient.gameObject.SetActive(value: true);
		return artilleryProjectileClient;
	}

	public void ReturnProjectile(ArtilleryProjectileClient projectile)
	{
		projectile.gameObject.SetActive(value: false);
		Gclass835_0.Return(projectile);
	}

	public ArtilleryProjectileClient method_0()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Object_0) as GameObject;
		ArtilleryProjectileClient component = gameObject.GetComponent<ArtilleryProjectileClient>();
		int id = UnityEngine.Random.Range(1, int.MaxValue);
		component.id = id;
		component.SetExplosiveItemParams(ArtilleryShellingGlobalSettings_0.ProjectileExplosionParams);
		gameObject.gameObject.SetActive(value: false);
		return component;
	}
}

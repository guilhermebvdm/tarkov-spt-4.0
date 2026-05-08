using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass1507
{
	[NonSerialized]
	public List<ArtilleryServerProjectileClass> List_0;

	[NonSerialized]
	public BackendConfigSettingsClass.ArtilleryShellingGlobalSettings ArtilleryShellingGlobalSettings_0;

	public void InitPool(int capacity, BackendConfigSettingsClass.ArtilleryShellingGlobalSettings artilleryShellingSettings)
	{
		ArtilleryShellingGlobalSettings_0 = artilleryShellingSettings;
		List_0 = new List<ArtilleryServerProjectileClass>();
		for (int i = 0; i < capacity; i++)
		{
			method_0();
		}
	}

	public ArtilleryServerProjectileClass GetProjectile()
	{
		if (List_0.Count == 0)
		{
			method_0();
		}
		ArtilleryServerProjectileClass artilleryServerProjectileClass = List_0[0];
		List_0.Remove(artilleryServerProjectileClass);
		return artilleryServerProjectileClass;
	}

	public void ReturnProjectile(ArtilleryServerProjectileClass projectile)
	{
		if (!List_0.Contains(projectile))
		{
			projectile.finalPosition = Vector3.zero;
			projectile.startPosition = Vector3.zero;
			projectile.explosion = false;
			List_0.Add(projectile);
		}
	}

	public void method_0()
	{
		ArtilleryServerProjectileClass artilleryServerProjectileClass = new ArtilleryServerProjectileClass
		{
			id = UnityEngine.Random.Range(1, int.MaxValue)
		};
		artilleryServerProjectileClass.SetExplosiveItemParams(ArtilleryShellingGlobalSettings_0.ProjectileExplosionParams);
		List_0.Add(artilleryServerProjectileClass);
	}
}

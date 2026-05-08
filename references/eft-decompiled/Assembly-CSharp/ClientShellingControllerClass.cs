using System;
using System.Collections.Generic;
using Comfort.Common;
using CommonAssets.Scripts.ArtilleryShelling;
using CommonAssets.Scripts.ArtilleryShelling.Client;
using EFT;
using UnityEngine;
using UnityEngine.LowLevel;

public class ClientShellingControllerClass : IDisposable
{
	[NonSerialized]
	public GClass1508 Gclass1508_0 = new GClass1508();

	[NonSerialized]
	public BackendConfigSettingsClass.ArtilleryShellingGlobalSettings ArtilleryShellingGlobalSettings_0;

	[NonSerialized]
	public ArtilleryShellingMapConfiguration ArtilleryShellingMapConfiguration_0;

	[NonSerialized]
	public GClass1510 Gclass1510_0;

	public Dictionary<int, ArtilleryProjectileClient> ActiveClientProjectiles = new Dictionary<int, ArtilleryProjectileClient>();

	[NonSerialized]
	public GameWorld GameWorld_0;

	[NonSerialized]
	public PlayerLoopSystem PlayerLoopSystem_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public GClass1513 Gclass1513_0;

	public ArtilleryShellingMapConfiguration CurrentMapConfiguration => ArtilleryShellingMapConfiguration_0;

	public ClientShellingControllerClass(bool hasAuthority)
	{
		Bool_0 = hasAuthority;
		method_0();
	}

	public void method_0()
	{
		GameWorld_0 = Singleton<GameWorld>.Instance;
		GameWorld_0.OnLateUpdate += method_1;
		Gclass1510_0 = new GClass1510();
		ArtilleryShellingGlobalSettings_0 = Singleton<BackendConfigSettingsClass>.Instance.ArtilleryShelling;
		Gclass1510_0.InitPool(15, ArtilleryShellingGlobalSettings_0);
		if (ArtilleryShellingGlobalSettings_0.ArtilleryMapsConfigs.ContainsKey(GameWorld_0.LocationId))
		{
			ArtilleryShellingMapConfiguration_0 = ArtilleryShellingGlobalSettings_0.ArtilleryMapsConfigs[GameWorld_0.LocationId];
		}
		if (ArtilleryShellingMapConfiguration_0.ShellingZones.Length != 0)
		{
			Gclass1508_0.Init(this);
			Gclass1513_0 = new GClass1513(ActiveClientProjectiles);
			Gclass1513_0.Init().HandleExceptions();
		}
	}

	public void method_1(float deltaTime)
	{
		Gclass1508_0.OnUpdate();
	}

	public void SyncProjectilesStates(ref List<ArtilleryPacketStruct> packets)
	{
		for (int i = 0; i < packets.Count; i++)
		{
			ArtilleryPacketStruct packet = packets[i];
			if (ActiveClientProjectiles.ContainsKey(packet.id))
			{
				ActiveClientProjectiles[packet.id].SyncProjectileState(ref packet);
			}
			else if (!packet.explosion)
			{
				CreateActiveProjectile(packet.id, packet.position);
			}
		}
	}

	public ArtilleryProjectileClient CreateActiveProjectile(int id, Vector3 startPosition = default(Vector3))
	{
		ArtilleryProjectileClient projectile = Gclass1510_0.GetProjectile();
		projectile.Init(id, Bool_0);
		projectile.ExplosionEvent = (Action<ArtilleryProjectileClient>)Delegate.Combine(projectile.ExplosionEvent, new Action<ArtilleryProjectileClient>(method_2));
		ActiveClientProjectiles.Add(id, projectile);
		GlobalEventHandlerClass.CreateEvent<GClass3577>().Invoke(id, startPosition, projectile.transform);
		return projectile;
	}

	public void method_2(ArtilleryProjectileClient projectile)
	{
		GlobalEventHandlerClass.CreateEvent<GClass3578>().Invoke(projectile.id, projectile.transform.position);
		projectile.ExplosionEvent = (Action<ArtilleryProjectileClient>)Delegate.Remove(projectile.ExplosionEvent, new Action<ArtilleryProjectileClient>(method_2));
		ActiveClientProjectiles.Remove(projectile.id);
		Gclass1510_0.ReturnProjectile(projectile);
	}

	public void Dispose()
	{
		GameWorld_0.OnLateUpdate -= method_1;
		Gclass1513_0?.Dispose();
	}
}

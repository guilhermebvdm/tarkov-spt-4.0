using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.NextObservedPlayer;
using EFT.SynchronizableObjects;
using UnityEngine;

public class ClientNetworkGameWorld : ClientGameWorld
{
	public override LootItem CreateLootWithRigidbody(GameObject lootObject, Item item, string objectName, bool randomRotation, MongoID[] validProfiles, out BoxCollider objectCollider, bool syncable = true, bool performPickUpValidation = true, float makeVisibleAfterDelay = 0f)
	{
		if (syncable)
		{
			return ObservedLootItem.CreateLootWithRigidbody(lootObject, item, objectName, this, randomRotation, validProfiles, out objectCollider, performPickUpValidation, makeVisibleAfterDelay);
		}
		return base.CreateLootWithRigidbody(lootObject, item, objectName, randomRotation, validProfiles, out objectCollider, syncable: true, performPickUpValidation, makeVisibleAfterDelay);
	}

	public override void RegisterPlayer(IPlayer iPlayer)
	{
		if (iPlayer is ObservedPlayerView observedPlayerView)
		{
			allObservedPlayersByID[observedPlayerView.ProfileId] = observedPlayerView;
			AllAlivePlayerBridges[observedPlayerView.ProfileId] = new GClass1711(observedPlayerView);
		}
		base.RegisterPlayer(iPlayer);
	}

	public override void PlantTripwire(Item item1, string profileId, Vector3 fromPosition, Vector3 toPosition)
	{
	}

	public override void TriggerTripwire(TripwireSynchronizableObject tripwire)
	{
	}

	public override void DeActivateTripwire(TripwireSynchronizableObject tripwire)
	{
	}

	public override Task InitLevel(ItemFactoryClass itemFactory, ObjectsFactoryDataClass config, bool loadBundlesAndCreatePools = true, List<ResourceKey> resources = null, IProgress<LoadingProgressStruct> progress = null, CancellationToken ct = default(CancellationToken))
	{
		base.TriggersEmitter = new GClass3595();
		return base.InitLevel(itemFactory, config, loadBundlesAndCreatePools, resources, progress, ct);
	}

	public override GrenadeFactoryClass CreateGrenadeFactory()
	{
		return new ClientNetworkGrenadeFactoryClass();
	}

	public override bool IsLocalGame()
	{
		return false;
	}

	public override void OnDestroy()
	{
		GClass3091.instance.Shutdown();
		base.OnDestroy();
	}
}

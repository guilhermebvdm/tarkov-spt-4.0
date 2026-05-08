using System;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using UnityEngine;

public interface GInterface169
{
	GrenadeFactoryClass GrenadeFactory { get; }

	event Action<IKillableLootItem> OnLootItemDestroyed;

	ISharedBallisticsCalculator CreateBallisticCalculator(int seed);

	void RemoveBallisticCalculator(Item weapon);

	void RegisterGrenade(Throwable grenade);

	void UnregisterGrenade(string id);

	LootItem ThrowItem(Item item, IPlayer player, Vector3? fwd);

	LootItem SetupItem(Item item, IPlayer player, Vector3 position, Quaternion rotation);

	void PlantTripwire(Item grenade, string profileId, Vector3 fromPosition, Vector3 toPosition);

	void DestroyLoot(string id);
}

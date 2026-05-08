using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class LootItemPositionClass
{
	public string Id;

	public Vector3 Position;

	public Vector3 Rotation;

	public Item Item;

	public MongoID[] ValidProfiles;

	public bool IsContainer;

	public bool useGravity;

	public bool randomRotation;

	public Vector3 Shift;

	public short PlatformId = -1;
}

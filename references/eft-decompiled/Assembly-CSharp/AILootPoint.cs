using System;
using EFT.Interactive;
using EFT.InventoryLogic;
using UnityEngine;

[Serializable]
public class AILootPoint : IAICorePointLink
{
	[SerializeField]
	public Vector3 _pos;

	[NonSerialized]
	public AICorePoint Core;

	[SerializeField]
	public int _coreId;

	public LootableContainer LootableContainer;

	[SerializeField]
	public string _lootableContainerId;

	[SerializeField]
	public string _lootSpawnId;

	public LootPoint LootSpawn;

	[SerializeField]
	public Vector3 LookPoint;

	public int Id;

	[NonSerialized]
	public float LockedUntile;

	[NonSerialized]
	public bool IsLocked;

	[NonSerialized]
	public int LockerId;

	public int ClusterId;

	[NonSerialized]
	public Item SpawnedItem_1;

	public string LootableContainerId => _lootableContainerId;

	public string LootSpawnId => _lootSpawnId;

	public AICorePoint CorePointInGame => Core;

	public Vector3 Position => _pos;

	public Item SpawnedItem => SpawnedItem_1;

	public EAiLootPointType Type
	{
		get
		{
			if (LootableContainer?.ItemOwner?.RootItem == null)
			{
				if (SpawnedItem == null)
				{
					return EAiLootPointType.Empty;
				}
				return EAiLootPointType.Item;
			}
			return EAiLootPointType.Container;
		}
	}

	public AILootPoint(int id, Vector3 stayPoint, Vector3 lookPoint, LootableContainer lootable, AICorePoint core)
	{
		Id = id;
		_pos = stayPoint;
		LookPoint = lookPoint;
		_lootableContainerId = lootable.Id;
		_lootSpawnId = null;
		Core = core;
		_coreId = Core.Id;
	}

	public AILootPoint(int id, Vector3 stayPoint, Vector3 lookPoint, LootPoint lootable, AICorePoint core)
	{
		Id = id;
		_pos = stayPoint;
		LookPoint = lookPoint;
		_lootableContainerId = null;
		_lootSpawnId = lootable.Id;
		Core = core;
		_coreId = Core.Id;
	}

	public void RestoreLoot(LootItemPositionClass item)
	{
		SpawnedItem_1 = item.Item;
	}

	public void RestoreContainer(LootableContainer container)
	{
		LootableContainer = container;
	}

	public void Restore(AICorePointHolder coresHolder)
	{
		Core = coresHolder.GetCorePoint(_coreId);
	}

	public void DrawGizmos(int index)
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(_pos, 0.1f);
		Vector3 vector = _pos + Vector3.up * (1f + 0.004f * (float)index);
		Gizmos.DrawWireSphere(vector, 0.1f);
		Gizmos.DrawLine(vector, LookPoint);
		Gizmos.DrawLine(vector, _pos);
		Gizmos.DrawWireSphere(LookPoint, 0.1f);
	}

	public bool Empty()
	{
		if (LootableContainer == null)
		{
			return SpawnedItem_1 == null;
		}
		return false;
	}

	public bool IsFree(int idBot)
	{
		if (!IsLocked)
		{
			return true;
		}
		if (idBot == LockerId)
		{
			return true;
		}
		if (LockedUntile < Time.time)
		{
			IsLocked = false;
			return true;
		}
		return false;
	}

	public void LockFor(int ownerId, float period)
	{
		LockerId = ownerId;
		LockedUntile = Time.time + period;
		IsLocked = true;
	}

	public void DisposeSpawnedItem()
	{
		SpawnedItem_1 = null;
	}
}

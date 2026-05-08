using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using UnityEngine;
using UnityEngine.AI;

public class BotItemTaker : GClass429
{
	[CompilerGenerated]
	public class Class264
	{
		public IRaiseEvents possibleAction;

		public IPlayer lootItemLastOwner;

		public Player owner;

		public Item rootItem;

		public BotItemTaker botItemTaker_0;

		public Callback callback_0;

		public void method_0()
		{
			if (!(possibleAction is GClass3411) || lootItemLastOwner == null || !(lootItemLastOwner.ProfileId != owner.ProfileId))
			{
				return;
			}
			if (rootItem is MagazineItemClass magazine)
			{
				owner.InventoryController.StrictCheckMagazine(magazine, status: false);
			}
			if (rootItem is GClass3248 topLevelCollection)
			{
				foreach (MagazineItemClass item in GClass3380.GetAllItemsFromCollection(topLevelCollection).OfType<MagazineItemClass>())
				{
					owner.InventoryController.StrictCheckMagazine(item, status: false);
				}
			}
			botItemTaker_0.BotOwner_0.AITaskManager.RegisterDelayedTask(botItemTaker_0.BotOwner_0, 0.5f, botItemTaker_0.BotOwner_0.Medecine.RefreshCurMeds);
			owner.InventoryController.RunNetworkTransaction(possibleAction, delegate(IResult result)
			{
				if (result.Succeed)
				{
					owner.UpdateInteractionCast();
					botItemTaker_0.OnItemTaken?.Invoke(rootItem, lootItemLastOwner);
				}
				owner.CurrentManagedState.Pickup(enabled: false, null);
			});
		}

		public void method_1(IResult result)
		{
			if (result.Succeed)
			{
				owner.UpdateInteractionCast();
				botItemTaker_0.OnItemTaken?.Invoke(rootItem, lootItemLastOwner);
			}
			owner.CurrentManagedState.Pickup(enabled: false, null);
		}
	}

	[CompilerGenerated]
	public class Class265
	{
		public BotItemTaker botItemTaker_0;

		public LootItem item;

		public void method_0()
		{
			if (botItemTaker_0.PreThrownItems.Contains(item))
			{
				botItemTaker_0.PreThrownItems.Remove(item);
				if ((item.transform.position - botItemTaker_0.BotOwner_0.Position).magnitude < botItemTaker_0.BotGlobalsMindSettings_0.THROW_DIST_TO_SEE)
				{
					botItemTaker_0.ThrownItems.Add(item);
				}
			}
		}
	}

	[NonSerialized]
	public const float DIST_TO_TAKE = 1.5f;

	[NonSerialized]
	public const float LOOK_PERIOD = 1.5f;

	[NonSerialized]
	public const float PERIOD_REFRESH_IF_NO_PLACE = 60f;

	[NonSerialized]
	public const float SDIST_TO_TAKE = 2.25f;

	[NonSerialized]
	public HashSet<LootItem> ThrownItems = new HashSet<LootItem>();

	[NonSerialized]
	public HashSet<LootItem> PreThrownItems = new HashSet<LootItem>();

	[NonSerialized]
	public LootItem ItemToTake;

	[NonSerialized]
	public bool IsCome;

	[NonSerialized]
	public float TimeToTake;

	[NonSerialized]
	public float NextTimeToTake;

	[NonSerialized]
	public float NextRefreshTime;

	[NonSerialized]
	public bool CanPickUpMeds;

	[NonSerialized]
	public bool CanPickUpWeapons;

	[NonSerialized]
	public Vector3 Dir;

	[NonSerialized]
	public float PrevDirUpdateTime;

	[NonSerialized]
	public const float DIR_UPDATE_PERIOD = 2f;

	public BotGlobalsMindSettings BotGlobalsMindSettings_0 => BotOwner_0.Settings.FileSettings.Mind;

	public event Action<Item, IPlayer> OnItemTaken;

	public BotItemTaker(BotOwner owner)
		: base(owner)
	{
	}

	public void Activate()
	{
		if (BotGlobalsMindSettings_0.CAN_TAKE_ITEMS)
		{
			CanPickUpMeds = true;
			CanPickUpWeapons = true;
			GameWorld instance = Singleton<GameWorld>.Instance;
			if (instance != null)
			{
				instance.OnThrowItem = (BotEventHandler.GDelegate15)Delegate.Combine(instance.OnThrowItem, new BotEventHandler.GDelegate15(method_12));
				instance.OnTakeItem = (BotEventHandler.GDelegate16)Delegate.Combine(instance.OnTakeItem, new BotEventHandler.GDelegate16(method_3));
			}
		}
	}

	public bool HaveItemToTake()
	{
		if (!BotGlobalsMindSettings_0.CAN_TAKE_ITEMS)
		{
			return false;
		}
		if (ItemToTake != null)
		{
			return ThrownItems.Contains(ItemToTake);
		}
		return false;
	}

	public void ManualUpdate()
	{
		if (HaveItemToTake())
		{
			if (IsCome)
			{
				method_4();
				return;
			}
			BotOwner_0.Steering.LookToMovingDirection();
			method_6(ItemToTake.transform.position);
		}
		else
		{
			RefreshClosestItems();
		}
	}

	public void RefreshClosestItems()
	{
		if (!(NextRefreshTime < Time.time))
		{
			return;
		}
		NextRefreshTime = Time.time + 2f;
		IsCome = false;
		float num = float.MaxValue;
		LootItem itemToTake = null;
		foreach (LootItem thrownItem in ThrownItems)
		{
			if (!thrownItem.IsPhysicsOn && method_8(thrownItem.transform.position) && method_11(thrownItem) && !method_2(thrownItem))
			{
				float sqrMagnitude = (thrownItem.transform.position - BotOwner_0.Position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					itemToTake = thrownItem;
				}
			}
		}
		ItemToTake = itemToTake;
		if (ItemToTake != null && method_0(ItemToTake.Item) == null)
		{
			ItemToTake = null;
			NextRefreshTime = Time.time + 60f;
		}
	}

	public ItemAddress method_0(Item item)
	{
		InventoryController inventoryController = BotOwner_0.GetPlayer.InventoryController;
		ItemAddress itemAddress = GClass3373.FindSlotToPickUp(inventoryController, item);
		if (itemAddress == null && !(item is Weapon))
		{
			itemAddress = GClass3373.FindGridToPickUp(inventoryController, item);
		}
		return itemAddress;
	}

	public void method_1(Player owner, IRaiseEvents possibleAction, Item rootItem, IPlayer lootItemLastOwner)
	{
		owner.CurrentManagedState.Pickup(enabled: true, delegate
		{
			if (possibleAction is GClass3411 && lootItemLastOwner != null && lootItemLastOwner.ProfileId != owner.ProfileId)
			{
				if (rootItem is MagazineItemClass magazine)
				{
					owner.InventoryController.StrictCheckMagazine(magazine, status: false);
				}
				if (rootItem is GClass3248 topLevelCollection)
				{
					foreach (MagazineItemClass item in GClass3380.GetAllItemsFromCollection(topLevelCollection).OfType<MagazineItemClass>())
					{
						owner.InventoryController.StrictCheckMagazine(item, status: false);
					}
				}
				BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 0.5f, BotOwner_0.Medecine.RefreshCurMeds);
				owner.InventoryController.RunNetworkTransaction(possibleAction, delegate(IResult result)
				{
					if (result.Succeed)
					{
						owner.UpdateInteractionCast();
						this.OnItemTaken?.Invoke(rootItem, lootItemLastOwner);
					}
					owner.CurrentManagedState.Pickup(enabled: false, null);
				});
			}
		});
	}

	public bool method_2(LootItem throwedItem)
	{
		int num = 0;
		while (true)
		{
			if (num < BotOwner_0.BotsGroup.MembersCount)
			{
				BotOwner botOwner = BotOwner_0.BotsGroup.Member(num);
				if (botOwner.Id != BotOwner_0.Id && botOwner.ItemTaker.ItemToTake == throwedItem)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public void method_3(LootItem item)
	{
		PreThrownItems.Remove(item);
		ThrownItems.Remove(item);
		RefreshClosestItems();
	}

	public void method_4()
	{
		if (TimeToTake < Time.time)
		{
			method_5(ItemToTake);
			return;
		}
		Vector3 v = ItemToTake.transform.position - BotOwner_0.Position;
		if (GClass369.SDistHorizontal(v) > 0.0225f)
		{
			v = GClass855.NormalizeFastSelf(v);
			if (v.y > 0f)
			{
				v.y = -0.01f;
				v = GClass855.NormalizeFastSelf(v);
			}
			BotOwner_0.Steering.LookToDirection(v);
		}
	}

	public void method_5(LootItem itemToTake)
	{
		if (NextTimeToTake < Time.time)
		{
			method_9(itemToTake);
			NextTimeToTake = Time.time + 2f;
		}
	}

	public void method_6(Vector3 position)
	{
		if (Time.time > PrevDirUpdateTime + 2f)
		{
			Dir = position - BotOwner_0.Position;
			PrevDirUpdateTime = Time.time;
		}
		float sqrMagnitude = Dir.sqrMagnitude;
		IsCome = sqrMagnitude < 2.25f;
		if (IsCome)
		{
			TimeToTake = Time.time + 1.5f;
			return;
		}
		Vector3 vector = GClass855.NormalizeFastSelf(Dir);
		Vector3 vector2 = position - vector * 1f;
		if (BotOwner_0.GoToPoint(vector2) != NavMeshPathStatus.PathComplete)
		{
			method_7(ItemToTake);
		}
		else if (!BotOwner_0.Mover.HasPathAndNoComplete)
		{
			method_7(ItemToTake);
		}
		else if (BotOwner_0.Mover.TargetPoint.HasValue && (BotOwner_0.Mover.TargetPoint.Value - vector2).magnitude > 0.75f)
		{
			method_7(ItemToTake);
		}
	}

	public void method_7(LootItem item)
	{
		ThrownItems.Remove(item);
		ItemToTake = null;
	}

	public bool method_8(Vector3 transformPosition)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		if (NavMesh.CalculatePath(BotOwner_0.Position, transformPosition, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
		{
			return true;
		}
		return false;
	}

	public void method_9(LootItem item)
	{
		Item rootItem = item.ItemOwner.RootItem;
		Player getPlayer = BotOwner_0.GetPlayer;
		InventoryController inventoryController = getPlayer.InventoryController;
		if (method_0(ItemToTake.Item) == null)
		{
			ItemToTake = null;
			return;
		}
		IPlayer lastOwner = item.LastOwner;
		if (rootItem is InventoryEquipment)
		{
			ItemToTake = null;
			return;
		}
		if (BotOwner_0.Settings.FileSettings.Patrol.PICKUP_ITEMS_TO_BACKPACK_OR_CONTAINER)
		{
			if (!method_10(EquipmentSlot.Backpack, item))
			{
				method_10(EquipmentSlot.SecuredContainer, item);
			}
			return;
		}
		GStruct154<GInterface424> gStruct = InteractionsHandlerClass.QuickFindAppropriatePlace(item.Item, inventoryController, GClass1518.ToEnumerable(inventoryController.Inventory.Equipment), InteractionsHandlerClass.EMoveItemOrder.PickUp, simulate: true);
		if (gStruct.Succeeded)
		{
			method_1(getPlayer, gStruct.Value, rootItem, lastOwner);
		}
	}

	public bool method_10(EquipmentSlot slot, LootItem item)
	{
		Item rootItem = item.ItemOwner.RootItem;
		Player getPlayer = BotOwner_0.GetPlayer;
		InventoryController inventoryController = getPlayer.InventoryController;
		IPlayer lastOwner = item.LastOwner;
		if (!(BotOwner_0.GetPlayer.InventoryController.Inventory.Equipment.GetSlot(slot).ContainedItem is SearchableItemItemClass searchableItemItemClass))
		{
			return false;
		}
		StashGridClass stashGridClass = searchableItemItemClass.Grids.FirstOrDefault();
		if (stashGridClass == null)
		{
			return false;
		}
		GClass3393 gClass = stashGridClass.FindLocationForItem(item.Item);
		if (gClass != null)
		{
			method_1(getPlayer, InteractionsHandlerClass.Move(item.Item, gClass, inventoryController, simulate: true).Value, rootItem, lastOwner);
			return true;
		}
		return false;
	}

	public bool method_11(LootItem thrownItem)
	{
		InventoryController inventoryController = BotOwner_0.GetPlayer.InventoryController;
		return InteractionsHandlerClass.QuickFindAppropriatePlace(thrownItem.Item, inventoryController, GClass1518.ToEnumerable(inventoryController.Inventory.Equipment), InteractionsHandlerClass.EMoveItemOrder.PickUp, simulate: true).Succeeded;
	}

	public void method_12(LootItem item)
	{
		if (item.LastOwner == BotOwner_0.GetPlayer || (!CanPickUpMeds && !CanPickUpWeapons && !BotOwner_0.Settings.FileSettings.Mind.CAN_TAKE_ANY_ITEM))
		{
			return;
		}
		bool num = item.Item is MedsItemClass;
		bool flag = item.Item is Weapon;
		if ((!num || !CanPickUpMeds) && (!flag || !CanPickUpWeapons) && !BotOwner_0.Settings.FileSettings.Mind.CAN_TAKE_ANY_ITEM)
		{
			return;
		}
		PreThrownItems.Add(item);
		BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 1f, delegate
		{
			if (PreThrownItems.Contains(item))
			{
				PreThrownItems.Remove(item);
				if ((item.transform.position - BotOwner_0.Position).magnitude < BotGlobalsMindSettings_0.THROW_DIST_TO_SEE)
				{
					ThrownItems.Add(item);
				}
			}
		});
	}

	public void Dispose()
	{
		GameWorld instance = Singleton<GameWorld>.Instance;
		ItemToTake = null;
		if (instance != null)
		{
			instance.OnThrowItem = (BotEventHandler.GDelegate15)Delegate.Remove(instance.OnThrowItem, new BotEventHandler.GDelegate15(method_12));
			instance.OnTakeItem = (BotEventHandler.GDelegate16)Delegate.Remove(instance.OnTakeItem, new BotEventHandler.GDelegate16(method_3));
		}
	}
}

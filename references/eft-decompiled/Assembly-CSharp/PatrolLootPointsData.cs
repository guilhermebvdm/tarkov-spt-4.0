using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

public class PatrolLootPointsData : GClass429
{
	public enum EAIDropLootPointCause
	{
		Used,
		Dist,
		NotFree,
		Period
	}

	public enum EBotLootState
	{
		AssembleAllItemsToLoot,
		EquipItems,
		LootItems
	}

	public enum EReplaceOperationResult
	{
		Success,
		NoPossibleSlotsToEquip,
		NotSuitableRarityOrPrice,
		MoveOperationFailed
	}

	[Serializable]
	[CompilerGenerated]
	public class Class258
	{
		public static readonly Class258 class258_0 = new Class258();

		public static Predicate<Item> predicate_0;

		public static Predicate<Item> predicate_1;

		public bool method_0(Item item)
		{
			if (!item.QuestItem)
			{
				return !item.CanBePickedUp;
			}
			return true;
		}

		public bool method_1(Item item)
		{
			if (!item.QuestItem)
			{
				return !item.CanBePickedUp;
			}
			return true;
		}
	}

	[NonSerialized]
	public const float MAX_FAR_SDIST = 400f;

	[NonSerialized]
	public const int MIN_COUNT_TO_DROP_VISITED = 12;

	[NonSerialized]
	public float NextCheck;

	[NonSerialized]
	public float DropPointTime;

	[NonSerialized]
	public float LastTimeToPoint;

	[NonSerialized]
	public float TimeDropVisited;

	[NonSerialized]
	public float StayAtPointPeriod;

	[NonSerialized]
	public AILootPoint CachedLootPoint_1;

	[NonSerialized]
	public AILootPoint LastCachedLootPoint;

	[NonSerialized]
	public HashSet<int> Visited = new HashSet<int>();

	[NonSerialized]
	public bool ClusterError;

	[NonSerialized]
	public bool CachedLootPointError;

	[NonSerialized]
	public bool AtPointNow;

	[NonSerialized]
	public EAIDropLootPointCause Cause;

	[NonSerialized]
	public AILootPointsCluster TargetLootCluster_1;

	[NonSerialized]
	public float BeginLootClusterTime_1;

	[NonSerialized]
	public Task<IResult> MoveItemTask;

	[NonSerialized]
	public List<CompoundItem> BotContainersCache = new List<CompoundItem>();

	[NonSerialized]
	public List<Item> NotEquipableItemsToLoot = new List<Item>();

	[NonSerialized]
	public List<Item> AllItemsToLootCache = new List<Item>();

	[NonSerialized]
	public EBotLootState LootState;

	[NonSerialized]
	public int MoveItemIndex;

	[NonSerialized]
	public float UnpauseTime;

	[NonSerialized]
	public bool LootingNow;

	[NonSerialized]
	public bool CurrentPointHasItemsBeforeLooting;

	public AILootPoint CachedLootPoint => CachedLootPoint_1;

	public AILootPointsCluster TargetLootCluster => TargetLootCluster_1;

	public float BeginLootClusterTime => BeginLootClusterTime_1;

	public PatrolLootPointsData(BotOwner owner)
		: base(owner)
	{
	}

	public bool HaveActions(float maxFarSDist = 400f, int voxelsRadius = 1)
	{
		return HaveActions(BotOwner_0.Position, maxFarSDist, voxelsRadius);
	}

	public bool HaveActions(Vector3 positionToSearch, float maxFarSDist = 400f, int voxelsRadius = 1)
	{
		if (CachedLootPoint_1 == null)
		{
			UpdateFindNextLootPoint(positionToSearch, maxFarSDist, voxelsRadius);
			return false;
		}
		if (!CachedLootPoint_1.IsFree(BotOwner_0.Id))
		{
			method_0(EAIDropLootPointCause.NotFree);
			return false;
		}
		if (BotOwner_0.SDistTo(CachedLootPoint_1.Position) > maxFarSDist)
		{
			method_0(EAIDropLootPointCause.Dist);
			return false;
		}
		CachedLootPoint_1.LockFor(BotOwner_0.Id, 3f);
		return true;
	}

	public void CheckWantLeave()
	{
		if (!AtPointNow || !(Time.time - LastTimeToPoint > StayAtPointPeriod))
		{
			return;
		}
		if (CurrentPointHasItemsBeforeLooting)
		{
			if (!LootingNow)
			{
				LootingNow = true;
			}
		}
		else
		{
			method_0(EAIDropLootPointCause.Period);
			NextCheck = Time.time + GClass856.Random(BotOwner_0.Settings.FileSettings.Patrol.STAY_AFTER_LOOT_TIME_MIN, BotOwner_0.Settings.FileSettings.Patrol.STAY_AFTER_LOOT_TIME_MAX);
		}
	}

	public bool ReadyForNextCheck()
	{
		return NextCheck <= Time.time;
	}

	public void method_0(EAIDropLootPointCause cause)
	{
		if (CachedLootPoint_1 != null)
		{
			Visited.Add(CachedLootPoint_1.Id);
			LastCachedLootPoint = CachedLootPoint_1;
		}
		Cause = cause;
		TimeDropVisited = Time.time + 30f;
		CachedLootPoint_1 = null;
		AtPointNow = false;
	}

	public void UpdateFindNextLootPoint(float maxFarSDist = 400f, int voxelsRadius = 1)
	{
		UpdateFindNextLootPoint(BotOwner_0.Position, maxFarSDist, voxelsRadius);
	}

	public void UpdateFindNextLootPoint(Vector3 pointToSearch, float maxFarSDist = 400f, int voxelsRadius = 1)
	{
		if (NextCheck > Time.time)
		{
			return;
		}
		if (TimeDropVisited < Time.time && Visited.Count > 12)
		{
			Visited.Clear();
		}
		NextCheck = Time.time + GClass856.Random(5f, 6f);
		foreach (NavGraphVoxelSimple item in BotOwner_0.VoxelesPersonalData.GetVoxelesExtended(pointToSearch, voxelsRadius, onlyWithPoints: false))
		{
			foreach (AILootPoint lootPoint in item.LootPoints)
			{
				if (!Visited.Contains(lootPoint.Id) && lootPoint.IsFree(BotOwner_0.Id) && lootPoint.CorePointInGame.ConnectionGroupId == BotOwner_0.StartCorePoint.ConnectionGroupId && (!BotOwner_0.Settings.FileSettings.Patrol.USE_REAL_LOOTING || !lootPoint.Empty()) && (Cause != EAIDropLootPointCause.Dist || LastCachedLootPoint == null || !((LastCachedLootPoint.Position - lootPoint.Position).sqrMagnitude < 4f)) && GClass856.SqrDistance(pointToSearch, lootPoint.Position) <= maxFarSDist)
				{
					CachedLootPoint_1 = lootPoint;
					if (CachedLootPoint_1.CorePointInGame.ConnectionGroupId != BotOwner_0.StartCorePoint.ConnectionGroupId && !CachedLootPointError)
					{
						CachedLootPointError = true;
					}
					AtPointNow = false;
					return;
				}
			}
		}
	}

	public void ComeToLootPoint()
	{
		if (!AtPointNow)
		{
			LastTimeToPoint = Time.time;
			AtPointNow = true;
			CurrentPointHasItemsBeforeLooting = method_1(CachedLootPoint_1);
			if (CurrentPointHasItemsBeforeLooting)
			{
				StayAtPointPeriod = (BotOwner_0.Settings.FileSettings.Patrol.USE_REAL_LOOTING ? GClass856.Random(3f, 10f) : GClass856.Random(5f, 15f));
			}
			else
			{
				StayAtPointPeriod = 0.2f;
			}
			Visited.Add(CachedLootPoint_1.Id);
		}
	}

	public void BeginLootCluster()
	{
		int num = 0;
		BotOwner botOwner;
		while (true)
		{
			if (num < BotOwner_0.BotsGroup.MembersCount)
			{
				botOwner = BotOwner_0.BotsGroup.Member(num);
				if (!(botOwner == BotOwner_0) && botOwner.PatrollingData.LootData.ClusterLootingNow() && botOwner.PatrollingData.LootData.TargetLootCluster == TargetLootCluster_1 && !TargetLootCluster_1.Looted)
				{
					break;
				}
				num++;
				continue;
			}
			BeginLootClusterTime_1 = Time.time;
			return;
		}
		BeginLootClusterTime_1 = botOwner.PatrollingData.LootData.BeginLootClusterTime;
	}

	public void StopLootCluster()
	{
		BeginLootClusterTime_1 = float.MaxValue;
	}

	public bool ClusterLootingNow()
	{
		return BeginLootClusterTime_1 < float.MaxValue;
	}

	public void SetTargetLootCluster(AILootPointsCluster targetCluster)
	{
		if (targetCluster != TargetLootCluster_1)
		{
			BeginLootClusterTime_1 = Time.time;
		}
		TargetLootCluster_1 = targetCluster;
		if (TargetLootCluster_1 != null && TargetLootCluster_1.ConnectionGroup != BotOwner_0.StartCorePoint.ConnectionGroupId && !ClusterError)
		{
			ClusterError = true;
		}
	}

	public bool method_1(AILootPoint lootPoint)
	{
		if (CachedLootPoint_1.SpawnedItem != null)
		{
			return true;
		}
		if (CachedLootPoint_1.LootableContainer?.ItemOwner?.RootItem == null)
		{
			return false;
		}
		AllItemsToLootCache.Clear();
		GClass3380.GetAllAssembledItemsNonAlloc(lootPoint.LootableContainer.ItemOwner.RootItem, AllItemsToLootCache);
		AllItemsToLootCache.RemoveAll((Item item) => item.QuestItem || !item.CanBePickedUp);
		return AllItemsToLootCache.Count > 0;
	}

	public void UpdateLootState()
	{
		if (AtPointNow && LootingNow)
		{
			if (BotOwner_0.Settings.FileSettings.Patrol.USE_REAL_LOOTING)
			{
				method_2(CachedLootPoint_1);
			}
			else
			{
				LootingNow = false;
			}
			if (!LootingNow)
			{
				method_0(EAIDropLootPointCause.Period);
				NextCheck = Time.time + GClass856.Random(BotOwner_0.Settings.FileSettings.Patrol.STAY_AFTER_LOOT_TIME_MIN, BotOwner_0.Settings.FileSettings.Patrol.STAY_AFTER_LOOT_TIME_MAX);
			}
		}
	}

	public void method_2(AILootPoint aiLootPoint)
	{
		if (method_5())
		{
			return;
		}
		InventoryController inventoryController = BotOwner_0.GetPlayer.InventoryController;
		InventoryEquipment equipment = inventoryController.Inventory.Equipment;
		switch (LootState)
		{
		case EBotLootState.AssembleAllItemsToLoot:
		{
			AllItemsToLootCache.Clear();
			Item item = method_4(aiLootPoint);
			if (item == null)
			{
				ResetLootState();
				break;
			}
			GClass3380.GetAllAssembledItemsNonAlloc(item, AllItemsToLootCache);
			AllItemsToLootCache.RemoveAll((Item item3) => item3.QuestItem || !item3.CanBePickedUp);
			if (AllItemsToLootCache.Count == 0)
			{
				ResetLootState();
				break;
			}
			MoveItemIndex = 0;
			NotEquipableItemsToLoot.Clear();
			LootState = EBotLootState.EquipItems;
			break;
		}
		case EBotLootState.EquipItems:
			if (method_3(aiLootPoint, ref MoveItemIndex, AllItemsToLootCache))
			{
				break;
			}
			if (MoveItemIndex < AllItemsToLootCache.Count)
			{
				Item item2 = AllItemsToLootCache[MoveItemIndex];
				switch (method_8(item2, inventoryController))
				{
				case EReplaceOperationResult.NoPossibleSlotsToEquip:
				case EReplaceOperationResult.MoveOperationFailed:
					NotEquipableItemsToLoot.Add(item2);
					break;
				case EReplaceOperationResult.Success:
					return;
				}
				MoveItemIndex++;
			}
			else
			{
				MoveItemIndex = 0;
				method_7(equipment);
				LootState = EBotLootState.LootItems;
			}
			break;
		case EBotLootState.LootItems:
			if (method_3(aiLootPoint, ref MoveItemIndex, NotEquipableItemsToLoot))
			{
				break;
			}
			if (MoveItemIndex < NotEquipableItemsToLoot.Count)
			{
				GStruct154<GInterface424> gStruct = InteractionsHandlerClass.QuickFindAppropriatePlace(NotEquipableItemsToLoot[MoveItemIndex], inventoryController, BotContainersCache, InteractionsHandlerClass.EMoveItemOrder.Apply, simulate: true);
				if (gStruct.Succeeded)
				{
					MoveItemTask = inventoryController.TryRunNetworkTransaction(gStruct);
					method_6(BotOwner_0.Settings.FileSettings.Mind.DEADBODYWORK_MOVE_ITEMS_DELAY);
				}
				else
				{
					MoveItemIndex++;
				}
			}
			if (MoveItemTask == null && LootingNow && MoveItemIndex >= NotEquipableItemsToLoot.Count)
			{
				ResetLootState();
			}
			break;
		}
	}

	public bool method_3(AILootPoint aiLootPoint, ref int index, List<Item> itemsList)
	{
		if (MoveItemTask == null)
		{
			return false;
		}
		if (!MoveItemTask.IsCompleted)
		{
			method_6(BotOwner_0.Settings.FileSettings.Mind.DEADBODYWORK_MOVE_ITEMS_DELAY);
			return true;
		}
		if (MoveItemTask.Result.Succeed && aiLootPoint.Type == EAiLootPointType.Item && index < itemsList.Count && itemsList[index] == aiLootPoint.SpawnedItem)
		{
			aiLootPoint.DisposeSpawnedItem();
		}
		index++;
		MoveItemTask = null;
		return false;
	}

	[CanBeNull]
	public Item method_4(AILootPoint aiLootPoint)
	{
		return aiLootPoint.Type switch
		{
			EAiLootPointType.Container => aiLootPoint.LootableContainer.ItemOwner.RootItem, 
			EAiLootPointType.Item => aiLootPoint.SpawnedItem, 
			_ => null, 
		};
	}

	public bool method_5()
	{
		if (UnpauseTime > 0f)
		{
			if (!(Time.time >= UnpauseTime))
			{
				return true;
			}
			UnpauseTime = -1f;
		}
		return false;
	}

	public void method_6(float pTime)
	{
		UnpauseTime = Time.time + pTime;
	}

	public void method_7(InventoryEquipment botEquipment)
	{
		BotContainersCache.Clear();
		if (botEquipment.GetSlot(EquipmentSlot.Backpack).ContainedItem is CompoundItem item)
		{
			BotContainersCache.Add(item);
		}
		_ = botEquipment.GetSlot(EquipmentSlot.Backpack).ContainedItem;
	}

	public EReplaceOperationResult method_8(Item containerItem, InventoryController botInventoryController)
	{
		Item itemToThrow;
		ItemAddress itemAddress = GClass3373.FindEquipmentSlotToReplaceWithBetterItem(botInventoryController, containerItem, withoutPrimaryWeaponSlot: true, out itemToThrow);
		if (itemAddress == null)
		{
			if (itemToThrow != null)
			{
				return EReplaceOperationResult.NotSuitableRarityOrPrice;
			}
			return EReplaceOperationResult.NoPossibleSlotsToEquip;
		}
		if (itemToThrow != null)
		{
			botInventoryController.ThrowItem(itemToThrow);
			method_6(BotOwner_0.Settings.FileSettings.Mind.DEADBODYWORK_DROP_ITEMS_DELAY);
		}
		GStruct154<GClass3411> gStruct = InteractionsHandlerClass.Move(containerItem, itemAddress, botInventoryController, simulate: true);
		if (gStruct.Succeeded)
		{
			MoveItemTask = botInventoryController.TryRunNetworkTransaction(gStruct);
			method_6(BotOwner_0.Settings.FileSettings.Mind.DEADBODYWORK_MOVE_ITEMS_DELAY);
			return EReplaceOperationResult.Success;
		}
		return EReplaceOperationResult.MoveOperationFailed;
	}

	public void ResetLootState()
	{
		CurrentPointHasItemsBeforeLooting = false;
		LootingNow = false;
		LootState = EBotLootState.AssembleAllItemsToLoot;
	}

	public bool AllGroupWantLootCluster()
	{
		AILootPointsCluster targetLootCluster_ = TargetLootCluster_1;
		if (BotOwner_0.BotsGroup.BossGroup == null)
		{
			return true;
		}
		int num = 0;
		while (true)
		{
			if (num < BotOwner_0.BotsGroup.MembersCount)
			{
				if (BotOwner_0.BotsGroup.Member(num).PatrollingData.LootData.TargetLootCluster_1 != targetLootCluster_)
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}
}

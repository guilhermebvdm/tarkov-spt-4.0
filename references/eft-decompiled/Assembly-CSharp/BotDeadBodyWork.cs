using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;
using UnityEngine.AI;

public class BotDeadBodyWork : GClass429
{
	public enum ELookState
	{
		Initial,
		LootWeapon,
		CheckBackpack,
		LootAllCalculations,
		LootAllItemsMoving,
		DropBodyVest,
		DropBodyBackpack,
		Exit
	}

	[Serializable]
	[CompilerGenerated]
	public class Class183
	{
		public static readonly Class183 class183_0 = new Class183();

		public static Comparison<(float Weight, Item Item)> comparison_0;

		public int method_0((float Weight, Item Item) a, (float Weight, Item Item) b)
		{
			return -1 * a.Weight.CompareTo(b.Weight);
		}
	}

	[NonSerialized]
	public const float MOVEITEM_TRANSACTION_DELAY = 1f;

	[NonSerialized]
	public List<GClass386> Bodies;

	[NonSerialized]
	public float NextCheckClose;

	[NonSerialized]
	public float NextPosibleGo;

	[NonSerialized]
	public float NextCheckBody = 20f;

	[NonSerialized]
	public float NextShallStopCheck;

	[NonSerialized]
	public float LastBodyDistCheck;

	[NonSerialized]
	public float LeaveBodyTime;

	[NonSerialized]
	public bool LookInProgress;

	[NonSerialized]
	public bool LastDesicion;

	[NonSerialized]
	public GClass386 ActiveBody;

	[NonSerialized]
	public bool IsStay;

	[NonSerialized]
	public ELookState CurLookState;

	[NonSerialized]
	public float UnpauseTime;

	[NonSerialized]
	public List<Item> ItemsCache = new List<Item>(32);

	[NonSerialized]
	public List<CompoundItem> ContainersCache = new List<CompoundItem>(4);

	[NonSerialized]
	public List<(float Weight, Item Item)> WeightedItems = new List<(float, Item)>(32);

	[NonSerialized]
	public int TakeItemsLeft;

	[NonSerialized]
	public int CurItemIndex;

	[NonSerialized]
	public Task<IResult> MoveItemTask;

	[NonSerialized]
	public Vector3 CachedOffset;

	public const float LOOK_TO_CLOSE_SAFE_SDIST = 0.0225f;

	public float Single_0 => BotOwner_0.Settings.FileSettings.Patrol.DEAD_BODY_LOOK_PERIOD;

	[field: NonSerialized]
	public bool ShallUse { get; set; }

	public bool Boolean_0
	{
		get
		{
			if (IsStay)
			{
				return !LookInProgress;
			}
			return false;
		}
	}

	public event Action<float> OnStartLookToBody;

	public BotDeadBodyWork(BotOwner owner)
		: base(owner)
	{
		float b = 0.3f;
		float x = GClass856.Random(0.15f, b) * Mathf.Sign(GClass856.Random(-1f, 1f));
		float z = GClass856.Random(0.15f, b) * Mathf.Sign(GClass856.Random(-1f, 1f));
		CachedOffset = new Vector3(x, 0f, z);
	}

	public static void LookToLoot(BotOwner bot, Vector3 posLoot)
	{
		Vector3 v = posLoot - bot.Position;
		if (GClass369.SDistHorizontal(v) > 0.0225f)
		{
			v = GClass855.NormalizeFastSelf(v);
			if (v.y > 0f)
			{
				v.y = -0.01f;
				v = GClass855.NormalizeFastSelf(v);
			}
			bot.Steering.LookToDirection(v);
		}
	}

	public void method_0(GClass386 body)
	{
		LookInProgress = true;
		if (body != null)
		{
			switch (body.Side)
			{
			case EPlayerSide.Savage:
			{
				ETagStatus value = (ETagStatus)0;
				if (body.Player != null && body.Player.AIData.IsAI)
				{
					value = BotSettingsRepoClass.GetPhraseTagFromRole(body.Player.AIData.BotOwner.Profile.Info.Settings.Role);
				}
				BotOwner_0.BotTalk.Say(EPhraseTrigger.OnFriendlyDown, sayImmediately: false, value);
				break;
			}
			case EPlayerSide.Usec:
			case EPlayerSide.Bear:
				BotOwner_0.BotTalk.Say(EPhraseTrigger.EnemyDown);
				break;
			}
		}
		EDeadBodyWorkWay hOW_WORK_OVER_DEAD_BODY = (EDeadBodyWorkWay)BotOwner_0.Settings.FileSettings.Mind.HOW_WORK_OVER_DEAD_BODY;
		float num = Single_0 * 2f + 8f;
		switch (hOW_WORK_OVER_DEAD_BODY)
		{
		default:
			method_6();
			CurLookState = ELookState.Exit;
			method_1(num);
			break;
		case EDeadBodyWorkWay.LootBody:
			CurLookState = ELookState.Initial;
			break;
		case EDeadBodyWorkWay.UseSurgicalKit:
			method_7();
			CurLookState = ELookState.Exit;
			method_1(num);
			break;
		}
		this.OnStartLookToBody?.Invoke(num);
	}

	public void method_1(float pTime)
	{
		UnpauseTime = Time.time + pTime;
	}

	public void method_2()
	{
		if (UnpauseTime > 0f)
		{
			if (!(Time.time >= UnpauseTime))
			{
				return;
			}
			UnpauseTime = -1f;
		}
		float dEADBODYWORK_INITIAL_DELAY = BotOwner_0.Settings.FileSettings.Mind.DEADBODYWORK_INITIAL_DELAY;
		float dEADBODYWORK_CHECK_ITEMS_DELAY = BotOwner_0.Settings.FileSettings.Mind.DEADBODYWORK_CHECK_ITEMS_DELAY;
		float dEADBODYWORK_MOVE_ITEMS_DELAY = BotOwner_0.Settings.FileSettings.Mind.DEADBODYWORK_MOVE_ITEMS_DELAY;
		float dEADBODYWORK_DROP_ITEMS_DELAY = BotOwner_0.Settings.FileSettings.Mind.DEADBODYWORK_DROP_ITEMS_DELAY;
		InventoryController inventoryController = BotOwner_0.GetPlayer.InventoryController;
		InventoryEquipment equipment = inventoryController.Inventory.Equipment;
		InventoryEquipment equipment2 = ActiveBody.Player.InventoryController.Inventory.Equipment;
		switch (CurLookState)
		{
		case ELookState.Initial:
			method_1(dEADBODYWORK_INITIAL_DELAY);
			CurLookState = ELookState.LootWeapon;
			break;
		case ELookState.LootWeapon:
		{
			ItemsCache.Clear();
			Item containedItem2 = equipment2.GetSlot(EquipmentSlot.FirstPrimaryWeapon).ContainedItem;
			if (containedItem2 != null)
			{
				ItemsCache.Add(containedItem2);
			}
			Item containedItem3 = equipment2.GetSlot(EquipmentSlot.SecondPrimaryWeapon).ContainedItem;
			if (containedItem3 != null)
			{
				ItemsCache.Add(containedItem3);
			}
			Item containedItem4 = equipment2.GetSlot(EquipmentSlot.Holster).ContainedItem;
			if (containedItem4 != null)
			{
				ItemsCache.Add(containedItem4);
			}
			for (int i = 0; i < ItemsCache.Count; i++)
			{
				Item item = ItemsCache[i];
				ItemAddress itemAddress = GClass3373.FindSlotToPickUp(BotOwner_0.GetPlayer.InventoryController, item);
				if (itemAddress != null)
				{
					GStruct154<GClass3411> gStruct = InteractionsHandlerClass.Move(item, itemAddress, inventoryController, simulate: true);
					if (gStruct.Succeeded)
					{
						inventoryController.TryRunNetworkTransaction(gStruct);
						method_1(dEADBODYWORK_MOVE_ITEMS_DELAY);
						break;
					}
				}
			}
			CurLookState = ELookState.CheckBackpack;
			break;
		}
		case ELookState.CheckBackpack:
		{
			Slot slot = equipment.GetSlot(EquipmentSlot.Backpack);
			if (slot.ContainedItem == null)
			{
				Item containedItem6 = equipment2.GetSlot(EquipmentSlot.Backpack).ContainedItem;
				if (containedItem6 != null)
				{
					GStruct154<GClass3411> gStruct3 = InteractionsHandlerClass.Move(containedItem6, slot.CreateItemAddress(), inventoryController, simulate: true);
					if (gStruct3.Succeeded)
					{
						inventoryController.TryRunNetworkTransaction(gStruct3);
						method_1(dEADBODYWORK_MOVE_ITEMS_DELAY);
					}
				}
			}
			CurLookState = ELookState.LootAllCalculations;
			break;
		}
		case ELookState.LootAllCalculations:
		{
			ItemsCache.Clear();
			ContainersCache.Clear();
			if (equipment2.GetSlot(EquipmentSlot.Backpack).ContainedItem is CompoundItem item2)
			{
				ContainersCache.Add(item2);
			}
			if (equipment2.GetSlot(EquipmentSlot.TacticalVest).ContainedItem is CompoundItem item3)
			{
				ContainersCache.Add(item3);
			}
			if (equipment2.GetSlot(EquipmentSlot.Pockets).ContainedItem is CompoundItem item4)
			{
				ContainersCache.Add(item4);
			}
			for (int j = 0; j < ContainersCache.Count; j++)
			{
				GClass3380.GetAllAssembledItems(ContainersCache[j], ItemsCache);
			}
			for (int num = ItemsCache.Count - 1; num >= 0; num--)
			{
				if (ItemsCache[num] is SearchableItemItemClass)
				{
					ItemsCache.RemoveAt(num);
				}
			}
			if (ItemsCache.Count > 1)
			{
				WeightedItems.Clear();
				for (int k = 0; k < ItemsCache.Count; k++)
				{
					Item item5 = ItemsCache[k];
					XYCellSizeStruct xYCellSizeStruct = item5.CalculateCellSize();
					int num2 = item5.Template.CreditsPrice / (xYCellSizeStruct.X * xYCellSizeStruct.Y);
					float item6 = Mathf.Pow(GClass856.Random(0f, 1f), 1f / (float)num2);
					WeightedItems.Add((item6, item5));
				}
				WeightedItems.Sort(((float Weight, Item Item) a, (float Weight, Item Item) b) => -1 * a.Weight.CompareTo(b.Weight));
				ItemsCache.Clear();
				for (int num3 = 0; num3 < WeightedItems.Count; num3++)
				{
					ItemsCache.Add(WeightedItems[num3].Item);
				}
			}
			TakeItemsLeft = GClass856.RandomInclude(1, ItemsCache.Count);
			if (TakeItemsLeft != 0 && ItemsCache.Count != 0)
			{
				ContainersCache.Clear();
				if (equipment.GetSlot(EquipmentSlot.Backpack).ContainedItem is CompoundItem item7)
				{
					ContainersCache.Add(item7);
				}
				if (equipment.GetSlot(EquipmentSlot.TacticalVest).ContainedItem is CompoundItem item8)
				{
					ContainersCache.Add(item8);
				}
				if (equipment.GetSlot(EquipmentSlot.Pockets).ContainedItem is CompoundItem item9)
				{
					ContainersCache.Add(item9);
				}
				CurItemIndex = 0;
				CurLookState = ELookState.LootAllItemsMoving;
				method_1(dEADBODYWORK_CHECK_ITEMS_DELAY);
			}
			else
			{
				CurLookState = (GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Patrol.DEAD_BODY_DROP_ITEM_PROBABILITY) ? ELookState.DropBodyVest : ELookState.Exit);
			}
			break;
		}
		case ELookState.LootAllItemsMoving:
			if (MoveItemTask != null)
			{
				if (!MoveItemTask.IsCompleted)
				{
					method_1(1f);
					break;
				}
				if (MoveItemTask.Result.Succeed)
				{
					TakeItemsLeft--;
				}
				CurItemIndex++;
				MoveItemTask = null;
			}
			if (CurItemIndex < ItemsCache.Count && TakeItemsLeft > 0)
			{
				GStruct154<GInterface424> gStruct2 = InteractionsHandlerClass.QuickFindAppropriatePlace(ItemsCache[CurItemIndex], inventoryController, ContainersCache, InteractionsHandlerClass.EMoveItemOrder.PickUp, simulate: true);
				if (gStruct2.Succeeded)
				{
					MoveItemTask = inventoryController.TryRunNetworkTransaction(gStruct2);
					method_1(dEADBODYWORK_MOVE_ITEMS_DELAY);
				}
				CurItemIndex++;
			}
			else
			{
				CurLookState = (GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Patrol.DEAD_BODY_DROP_ITEM_PROBABILITY) ? ELookState.DropBodyVest : ELookState.Exit);
			}
			break;
		case ELookState.DropBodyVest:
		{
			Item containedItem5 = equipment2.GetSlot(EquipmentSlot.TacticalVest).ContainedItem;
			CurLookState = ELookState.DropBodyBackpack;
			if (containedItem5 != null)
			{
				inventoryController.ThrowItem(containedItem5);
				method_1(dEADBODYWORK_DROP_ITEMS_DELAY);
			}
			break;
		}
		case ELookState.DropBodyBackpack:
		{
			Item containedItem = equipment2.GetSlot(EquipmentSlot.Backpack).ContainedItem;
			CurLookState = ELookState.Exit;
			if (containedItem != null)
			{
				inventoryController.ThrowItem(containedItem);
				method_1(dEADBODYWORK_DROP_ITEMS_DELAY);
			}
			break;
		}
		case ELookState.Exit:
			LookInProgress = false;
			BotOwner_0.WeaponManager.Grenades.SetDirty();
			break;
		}
	}

	public void Activate()
	{
	}

	public void UpdateCheck()
	{
		if (BotOwner_0.Settings.FileSettings.Patrol.CAN_LOOK_TO_DEADBODIES)
		{
			if (ShallUse)
			{
				method_3();
			}
			else
			{
				method_4();
			}
		}
	}

	public void ManualUpdate()
	{
		if (IsClose(out var _))
		{
			if (!IsStay)
			{
				method_0(ActiveBody);
				IsStay = true;
			}
			else
			{
				method_2();
			}
			BotOwner_0.SetPose(0f);
			LookToLoot(BotOwner_0, ActiveBody.Position);
		}
		else
		{
			BotOwner_0.SetPose(1f);
			BotOwner_0.SetTargetMoveSpeed(1f);
			method_9();
		}
	}

	public bool IsClose(out float dist)
	{
		if (NextCheckClose < Time.time)
		{
			NextCheckClose = Time.time + 2f;
			Vector3 vector = BotOwner_0.Position - ActiveBody.Position;
			float y = vector.y;
			vector.y = 0f;
			dist = vector.magnitude;
			LastBodyDistCheck = dist;
			LastDesicion = dist < 1.7f && Mathf.Abs(y) < 1.3f;
			return LastDesicion;
		}
		dist = LastBodyDistCheck;
		return LastDesicion;
	}

	public void method_3()
	{
		if (Boolean_0)
		{
			BotOwner_0.BotsGroup.DeadBodiesController.RemoveBody(ActiveBody);
			method_5();
		}
		else if (NextShallStopCheck < Time.time)
		{
			NextShallStopCheck = Time.time + 3f;
			IsClose(out var dist);
			if (dist > BotOwner_0.Settings.FileSettings.Patrol.DEAD_BODY_LEAVE_DIST)
			{
				method_5();
			}
		}
	}

	public void method_4()
	{
		if (!(NextCheckBody < Time.time))
		{
			return;
		}
		NextCheckBody = Time.time + 3f;
		Bodies = BotOwner_0.BotsGroup.DeadBodiesController.BodiesByGroup(BotOwner_0.BotsGroup);
		if (Bodies.Count > 0)
		{
			int nearEntity = GClass369.GetNearEntity(BotOwner_0.Position, Bodies);
			GClass386 gClass = Bodies[nearEntity];
			if (!GClass369.IsPointInsideDangerZone(BotOwner_0, gClass.BodyPosition) && gClass.IsOnNavMesh && BotPathFinderClass.FindPath(BotOwner_0.Position, gClass.BodyPosition, out var _) && gClass.IsFreeFor(BotOwner_0) && (gClass.Position - BotOwner_0.Position).magnitude < BotOwner_0.Settings.FileSettings.Patrol.DEAD_BODY_SEE_DIST)
			{
				ActiveBody = gClass;
				ActiveBody.SetUnderWork(BotOwner_0);
				ShallUse = true;
			}
		}
	}

	public void method_5()
	{
		if (ActiveBody != null)
		{
			BotOwner_0.BotsGroup.DeadBodiesController.RemoveBody(ActiveBody);
		}
		ActiveBody = null;
		ShallUse = false;
		IsStay = false;
		LookInProgress = false;
	}

	public void method_6()
	{
	}

	public void method_7()
	{
		if (BotOwner_0.Medecine.SurgicalKit.HaveSmth2Use)
		{
			BotOwner_0.Medecine.SurgicalKit.SetRandomPartToHeal();
			BotOwner_0.Medecine.SurgicalKit.ApplyToCurrentPart(method_8);
		}
	}

	public void method_8()
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			return;
		}
		BotOwner_0.AITaskManager.RegisterDelayedTask(BotOwner_0, 3f, delegate
		{
			if (BotOwner_0.Memory.GoalEnemy == null && BotOwner_0.Medecine.SurgicalKit.HaveSmth2Use)
			{
				BotOwner_0.Medecine.SurgicalKit.SetRandomPartToHeal();
				BotOwner_0.Medecine.SurgicalKit.ApplyToCurrentPart();
			}
		});
	}

	public void method_9()
	{
		if (BotOwner_0.HasPathAndNotComplete)
		{
			BotOwner_0.Steering.LookToMovingDirection();
		}
		if (NextPosibleGo < Time.time)
		{
			NextPosibleGo = Time.time + 6f;
			Vector3 vector = ActiveBody.Position + CachedOffset;
			Vector3 vector2 = GClass855.NormalizeFastSelf(vector - BotOwner_0.Position);
			Vector3 position = vector - vector2;
			if (BotOwner_0.GoToPoint(position, slowAtTheEnd: true, -1f, getUpWithCheck: false, mustHaveWay: false) != NavMeshPathStatus.PathComplete)
			{
				method_5();
			}
		}
	}

	[CompilerGenerated]
	public void method_10()
	{
		if (BotOwner_0.Memory.GoalEnemy == null && BotOwner_0.Medecine.SurgicalKit.HaveSmth2Use)
		{
			BotOwner_0.Medecine.SurgicalKit.SetRandomPartToHeal();
			BotOwner_0.Medecine.SurgicalKit.ApplyToCurrentPart();
		}
	}
}

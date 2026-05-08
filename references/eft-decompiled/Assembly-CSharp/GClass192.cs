using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using JsonType;
using UnityEngine;

public class GClass192(BotOwner bot) : GClass177<GClass26>(bot)
{
	[Serializable]
	[CompilerGenerated]
	public class Class114
	{
		public static readonly Class114 class114_0 = new Class114();

		public static Func<Item, bool> func_0;

		public static Func<Item, bool> func_1;

		public bool method_0(Item x)
		{
			return x.Template.Rarity == ELootRarity.Superrare;
		}

		public bool method_1(Item x)
		{
			return x.Template.Rarity == ELootRarity.Rare;
		}
	}

	[NonSerialized]
	public const float Float_0 = 5f;

	[NonSerialized]
	public const float Float_1 = 2.4f;

	[NonSerialized]
	public EquipmentSlot[] EquipmentSlot_0 = new EquipmentSlot[1] { EquipmentSlot.SecuredContainer };

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4 = float.MaxValue;

	[NonSerialized]
	public Vector3 Vector3_0;

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (Time.time > Float_3 + 5f && !BotOwner_0.GiftData.GiftTarget.TargetLost())
		{
			BotOwner_0.Gesture.TryGestus(EInteraction.ComeWithMeGesture, withAimingDelay: false);
			BotOwner_0.Mover.Stop();
			BotOwner_0.Steering.LookToPoint(BotOwner_0.GiftData.GiftTarget.player.Position + BotOwner.STAY_HEIGHT);
		}
		else
		{
			Float_4 = Time.time + 2.4f;
			BotOwner_0.Gesture.TryGestus(EInteraction.ThereGesture, withAimingDelay: false);
			BotOwner_0.Steering.LookToPoint(Vector3_0);
		}
		if (Time.time > Float_4)
		{
			method_0();
		}
		if (BotOwner_0.GiftData.IsPlayerOnDistance(BotOwner_0.GiftData.GiftTarget.player, 5f) && Float_2 < Time.time)
		{
			method_1();
		}
	}

	public void method_0()
	{
		Float_4 = float.MaxValue;
		Float_2 = Time.time + 5f;
	}

	public void method_1()
	{
		if (!BotOwner_0.GiftData.GiftTarget.CanDoGift())
		{
			return;
		}
		List<Item> list = BotOwner_0.GetPlayer.InventoryController.Inventory.GetItemsInSlots(EquipmentSlot_0).ToList();
		if (BotOwner_0.GiftData.IsGood(BotOwner_0.GiftData.GiftTarget.player))
		{
			List<Item> list2 = list.Where((Item x) => x.Template.Rarity == ELootRarity.Superrare).ToList();
			if (list2.Count > 0)
			{
				list = list2;
			}
			else
			{
				list2 = list.Where((Item x) => x.Template.Rarity == ELootRarity.Rare).ToList();
				if (list2.Count > 0)
				{
					list = list2;
				}
			}
		}
		int count = list.Count;
		int num = UnityEngine.Random.Range(0, count);
		Item item = null;
		for (int num2 = 0; num2 < count; num2++)
		{
			item = list[(num + num2) % count];
			if (!(item is AmmoItemClass) && !BotOwner_0.GiftData.IsItemTaken(item) && !item.IsContainer)
			{
				Vector3_0 = BotOwner_0.Position + BotOwner_0.LookDirection - Vector3.up;
				BotOwner_0.ItemDropper.SetItemToDropNext(item);
				BotOwner_0.ItemDropper.OnItemDrop += method_2;
				break;
			}
		}
		Float_2 = Time.time + 5f;
		BotOwner_0.ItemDropper.TryDoDrop();
		Float_3 = Time.time;
	}

	public void method_2(Item item)
	{
		BotOwner_0.GiftData.GiftTarget.MarkGifted();
		BotOwner_0.ItemDropper.OnItemDrop -= method_2;
	}
}

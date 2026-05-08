using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class BotGiftData : GClass429
{
	public const float DROP_GIFT_CLOSE = 3f;

	public const float DROP_GIFT_DISTANCE = 5f;

	[NonSerialized]
	public const float GIFT_PLAYER_DISTANCE = 25f;

	[NonSerialized]
	public const float GIFT_PLAYER_LOSE_TARGET_DISTANCE = 35f;

	[NonSerialized]
	public const float REPEAT_TRY_GIFT_PLAYER_TIME = 60f;

	[NonSerialized]
	public Vector3 PointToDrop;

	[NonSerialized]
	public float NextCheckTime;

	[NonSerialized]
	public Dictionary<IPlayer, GiftTarget> GifterPlayers = new Dictionary<IPlayer, GiftTarget>();

	[NonSerialized]
	public HashSet<int> BetterGiftsIdPlayer = new HashSet<int>();

	[NonSerialized]
	public EquipmentSlot[] Slots = new EquipmentSlot[1] { EquipmentSlot.SecuredContainer };

	[NonSerialized]
	public int AdditionalGiftsLimit;

	[NonSerialized]
	public List<Item> TakenItems = new List<Item>();

	[field: NonSerialized]
	public GiftTarget GiftTarget { get; set; }

	public BotGiftData(BotOwner owner)
		: base(owner)
	{
		BotOwner_0.ItemTaker.OnItemTaken += method_0;
		AdditionalGiftsLimit = owner.Settings.FileSettings.Mind.GIFTER_ADDITIONAL_GIFTS;
	}

	public void method_0(Item itm, IPlayer pl)
	{
		TakenItems.Add(itm);
	}

	public void AddPlayerForBetterGift(IPlayer player)
	{
		BetterGiftsIdPlayer.Add(player.Id);
	}

	public bool IsItemTaken(Item itm)
	{
		return TakenItems.Contains(itm);
	}

	public bool HaveUngiftedPlayerNear()
	{
		if (GiftTarget != null)
		{
			return GiftTarget.CanDoGift();
		}
		return false;
	}

	public void UpdateGiftTarget()
	{
		if (GiftTarget != null)
		{
			if (!GiftTarget.TargetLost() && !(Vector3.Distance(BotOwner_0.GetPlayer.Position, GiftTarget.player.Position) > 35f) && GiftTarget.CanDoGift())
			{
				return;
			}
			GiftTarget = null;
		}
		if (NextCheckTime > Time.time)
		{
			return;
		}
		NextCheckTime = Time.time + 2f;
		foreach (KeyValuePair<IPlayer, BotSettingsClass> neutral in BotOwner_0.BotsGroup.Neutrals)
		{
			if (IsPlayerOnDistance(neutral.Value.Player, 25f))
			{
				if (!GifterPlayers.ContainsKey(neutral.Key))
				{
					GiftTarget giftTarget = new GiftTarget(AdditionalGiftsLimit);
					giftTarget.player = neutral.Key;
					GifterPlayers.Add(giftTarget.player, giftTarget);
				}
				if (GifterPlayers.TryGetValue(neutral.Key, out var value) && value.CanDoGift() && value.ReadyToNextGift())
				{
					SetGiftTarget(value);
				}
			}
		}
	}

	public void SetGiftTarget(GiftTarget giftTarget)
	{
		GiftTarget = giftTarget;
		GiftTarget.prevBeginGiftNodeTime = Time.time;
	}

	public GiftTarget GetGiftTarget(IPlayer player)
	{
		if (!GifterPlayers.ContainsKey(player))
		{
			GiftTarget giftTarget = new GiftTarget(AdditionalGiftsLimit);
			giftTarget.player = player;
			GifterPlayers.Add(giftTarget.player, giftTarget);
		}
		return GifterPlayers[player];
	}

	public bool IsPlayerOnDistance(IPlayer player, float dist)
	{
		float num = Vector3.Distance(BotOwner_0.GetPlayer.Position, player.Position);
		if (num < 3f)
		{
			return BotOwner_0.NeutralsCheskData.CanSeeNeautral(player);
		}
		if (num < dist)
		{
			return BotOwner_0.Mover.ComputePathLengthToPoint(player.Position) < dist;
		}
		return false;
	}

	public void Dispose()
	{
		if (BotOwner_0 != null && BotOwner_0.ItemTaker != null)
		{
			BotOwner_0.ItemTaker.OnItemTaken -= method_0;
		}
	}

	public void CalcGiftsOnStart()
	{
		List<Item> list = BotOwner_0.GetPlayer.InventoryController.Inventory.GetItemsInSlots(Slots).ToList();
		int count = list.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			Item item = list[i];
			if (!item.IsContainer && !(item is AmmoItemClass) && !BotOwner_0.GiftData.IsItemTaken(item))
			{
				num++;
			}
		}
	}

	public bool IsGood(IPlayer giftTargetPlayer)
	{
		return BetterGiftsIdPlayer.Contains(giftTargetPlayer.Id);
	}
}

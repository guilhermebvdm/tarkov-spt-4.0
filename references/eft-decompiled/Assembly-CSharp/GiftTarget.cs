using System;
using EFT;
using UnityEngine;

public class GiftTarget
{
	[NonSerialized]
	public const float REPEAT_TRY_GIFT_PLAYER_TIME = 60f;

	[NonSerialized]
	public const float TIME_TO_LOSE_GIFT_TARGET = 20f;

	public IPlayer player;

	public float prevBeginGiftNodeTime = -60f;

	[NonSerialized]
	public bool AlreadyGifted;

	[NonSerialized]
	public int AdditionalGiftsGifted;

	[NonSerialized]
	public int AdditionalGiftsLimit = 1;

	[NonSerialized]
	public bool CanDoAdditionalGift;

	public GiftTarget(int additionalGiftsLimit)
	{
		AdditionalGiftsLimit = additionalGiftsLimit;
	}

	public bool CanDoGift()
	{
		if (AlreadyGifted)
		{
			return method_0();
		}
		return true;
	}

	public bool method_0()
	{
		if (CanDoAdditionalGift)
		{
			return AdditionalGiftsGifted < AdditionalGiftsLimit;
		}
		return false;
	}

	public bool ReadyToNextGift()
	{
		if (!(Time.time > prevBeginGiftNodeTime + 60f))
		{
			return method_0();
		}
		return true;
	}

	public bool TargetLost()
	{
		return Time.time > prevBeginGiftNodeTime + 20f;
	}

	public void AllowToDropAdditionalGift()
	{
		CanDoAdditionalGift = true;
	}

	public void MarkGifted()
	{
		if (AlreadyGifted)
		{
			AdditionalGiftsGifted++;
			CanDoAdditionalGift = false;
		}
		AlreadyGifted = true;
	}
}

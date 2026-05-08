using System;
using EFT;
using UnityEngine;

public class BotsGroupGrenade
{
	[NonSerialized]
	public int ThrowCount;

	[NonSerialized]
	public const float STANDART_PERIOD = 2f;

	[NonSerialized]
	public const float START_PERIOD = 5f;

	[NonSerialized]
	public float EndThrow;

	public void ThrowGrenade(BotOwner thrower)
	{
		float num = ((ThrowCount > 1) ? 2f : 5f);
		EndThrow = Time.time + num;
		ThrowCount++;
	}

	public bool CanThrow()
	{
		return Time.time > EndThrow;
	}
}

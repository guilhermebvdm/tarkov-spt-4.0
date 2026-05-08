using System;
using UnityEngine;

public class BotsGroupLaying
{
	[NonSerialized]
	public const float STANDART_PERIOD = 2f;

	[NonSerialized]
	public float EndLay;

	public void StartLay()
	{
		EndLay = Time.time + GClass856.GreateRandom(2f);
	}

	public bool CanLay()
	{
		return Time.time > EndLay;
	}
}

using System;
using System.Globalization;
using UnityEngine;

public class GClass544 : IDisposable
{
	public bool HasLineOfSight;

	public RaycastHit TargetToBotHit;

	public RaycastHit BotToTargetHit;

	public ELookObstacleType BotToTargetObstacleType;

	public ELookObstacleType TargetToBotObstacleType;

	public bool IsGreen;

	public float GreenDistance;

	public float GreenK;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	public bool BotToTargetLineOfSight
	{
		get
		{
			return Bool_0;
		}
		set
		{
			Bool_0 = value;
			HasLineOfSight = Bool_0 && Bool_1;
		}
	}

	public bool TargetToBotLineOfSight
	{
		get
		{
			return Bool_1;
		}
		set
		{
			Bool_1 = value;
			HasLineOfSight = Bool_0 && Bool_1;
		}
	}

	public void Dispose()
	{
		BotToTargetLineOfSight = false;
		TargetToBotHit = default(RaycastHit);
		TargetToBotLineOfSight = false;
		BotToTargetHit = default(RaycastHit);
		BotToTargetObstacleType = ELookObstacleType.None;
		TargetToBotObstacleType = ELookObstacleType.None;
		IsGreen = false;
		Bool_0 = false;
		Bool_1 = false;
	}

	public override string ToString()
	{
		return "los_all: " + HasLineOfSight + " los_b-t: " + BotToTargetLineOfSight + " los_t-b: " + TargetToBotLineOfSight + " isGreen:" + IsGreen + " greenK:" + GreenK.ToString(CultureInfo.InvariantCulture) + " grassDistance:" + GreenDistance.ToString(CultureInfo.InvariantCulture);
	}
}

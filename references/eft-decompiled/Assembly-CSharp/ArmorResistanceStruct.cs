using System;
using UnityEngine;

public struct ArmorResistanceStruct
{
	[NonSerialized]
	public const float Float_0 = 0f;

	[NonSerialized]
	public const float Float_1 = 100f;

	[NonSerialized]
	public const float Float_2 = 15f;

	[NonSerialized]
	public const float Float_3 = -15f;

	[NonSerialized]
	public const float Float_4 = 0.4f;

	[NonSerialized]
	public const float Float_5 = 0.9f;

	public float RealResistance;

	public float ArmorClassResistance;

	public float CF;

	public float GetPenetrationChance(float penetrationPower)
	{
		if (RealResistance >= penetrationPower + 15f)
		{
			return 0f;
		}
		if (RealResistance >= penetrationPower)
		{
			return 0.4f * Mathf.Pow(RealResistance - penetrationPower - 15f, 2f);
		}
		if (RealResistance <= penetrationPower + -15f)
		{
			return 100f;
		}
		return 100f + penetrationPower / (0.9f * RealResistance - penetrationPower);
	}
}

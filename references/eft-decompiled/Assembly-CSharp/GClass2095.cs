using System;
using EFT;
using UnityEngine;

public class GClass2095 : GInterface211
{
	[NonSerialized]
	public const float Float_0 = 6f;

	[NonSerialized]
	public const float Float_1 = 1f;

	[NonSerialized]
	public Player Player_0;

	[NonSerialized]
	public Quaternion Quaternion_0;

	[NonSerialized]
	public float Float_2;

	public bool IsValid
	{
		get
		{
			if (Player_0 != null)
			{
				return Player_0.BtrInteractionSide != null;
			}
			return false;
		}
	}

	public GClass2095(Player player, Quaternion? intermediateRotation = null, float? timeToApply = null)
	{
		Player_0 = player;
		if (IsValid)
		{
			Quaternion_0 = intermediateRotation ?? method_0();
			Float_2 = timeToApply ?? 1f;
		}
	}

	public Quaternion method_0()
	{
		(Vector3, Vector3) tuple = ((Player_0.BtrState >= EPlayerBtrState.Inside) ? Player_0.BtrInteractionSide.GoOutPoints() : Player_0.BtrInteractionSide.GoInPoints());
		return Quaternion.LookRotation(tuple.Item2 - tuple.Item1);
	}

	public Quaternion GetRotation()
	{
		if (Float_2 > 0f)
		{
			Quaternion_0 = Quaternion.Slerp(Quaternion_0, method_0(), Time.deltaTime * 6f);
			Float_2 -= Time.deltaTime;
		}
		else
		{
			Quaternion_0 = method_0();
		}
		return Quaternion_0;
	}
}

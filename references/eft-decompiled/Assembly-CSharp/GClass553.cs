using System;
using EFT;
using UnityEngine;

public class GClass553 : GInterface9, IPositionPoint
{
	[NonSerialized]
	public IAICorePointLink IaicorePointLink_0;

	public Vector3 Position => IaicorePointLink_0.Position;

	public GClass553(IAICorePointLink point)
	{
		IaicorePointLink_0 = point;
	}

	public bool IsSame(GInterface9 point)
	{
		if (point is GClass553 gClass && gClass.IaicorePointLink_0 == IaicorePointLink_0)
		{
			return true;
		}
		if ((double)(Position - point.Position).sqrMagnitude < 0.04)
		{
			return true;
		}
		return false;
	}

	public void BotCome(BotOwner owner)
	{
	}
}

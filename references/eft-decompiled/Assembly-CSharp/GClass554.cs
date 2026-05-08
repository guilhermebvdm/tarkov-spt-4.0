using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass554 : GInterface9, IPositionPoint
{
	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	public Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
	}

	public GClass554(Vector3 target)
	{
		Vector3_0 = target;
	}

	public bool IsSame(GInterface9 point)
	{
		return (double)(point.Position - Position).sqrMagnitude < 0.04;
	}

	public void BotCome(BotOwner owner)
	{
	}
}

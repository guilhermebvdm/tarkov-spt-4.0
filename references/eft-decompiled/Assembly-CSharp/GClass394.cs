using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public abstract class GClass394
{
	[CompilerGenerated]
	public class Class111
	{
		public float sDistToEnemy;

		public Vector3 dirEnemy;

		public Vector3 wallVector;

		public bool method_0()
		{
			if (sDistToEnemy <= 0.1f)
			{
				return true;
			}
			return GClass855.IsAngLessNormalized(GClass855.NormalizeFastSelf(dirEnemy), wallVector.normalized, 0.1736482f);
		}
	}

	public static bool CanIHide(Vector3 posToHide, Vector3 wallVector, IEnumerable<Vector3> positionsIMustCare, float minSDistToEnemy, bool useRaycast, bool useAng = true)
	{
		foreach (Vector3 item in positionsIMustCare)
		{
			if (!CanIHideFromPos(posToHide, wallVector, minSDistToEnemy, useRaycast, useAng, item))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsDangerPositionFarEnough(Vector3 positionToCheck, IEnumerable<Vector3> positionsIMustCare, float minSDistToEnemy)
	{
		foreach (Vector3 item in positionsIMustCare)
		{
			if (!((item - positionToCheck).sqrMagnitude >= minSDistToEnemy))
			{
				return false;
			}
		}
		return true;
	}

	public static bool CanIHideFromPos(Vector3 posToHide, Vector3 wallVector, float minSDistToEnemy, bool useRaycast, bool useAng, Vector3 carePosition)
	{
		Vector3 vector = carePosition + BotOwner.STAY_HEIGHT;
		Vector3 vector2 = posToHide + BotOwner.STAY_HEIGHT;
		Vector3 dirEnemy = vector - vector2;
		float sDistToEnemy = dirEnemy.sqrMagnitude;
		if (sDistToEnemy < minSDistToEnemy)
		{
			return false;
		}
		Func<bool> func = () => sDistToEnemy <= 0.1f || GClass855.IsAngLessNormalized(GClass855.NormalizeFastSelf(dirEnemy), wallVector.normalized, 0.1736482f);
		if (!((useAng && useRaycast) ? (func() && Physics.Linecast(vector2, vector, LayerMaskClass.HighPolyWithTerrainMask)) : (useRaycast ? Physics.Linecast(vector2, vector, LayerMaskClass.HighPolyWithTerrainMask) : (!useAng || func()))))
		{
			return false;
		}
		return true;
	}
}

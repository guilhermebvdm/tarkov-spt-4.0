using System;
using Interpolation;
using UnityEngine;

public abstract class GClass1311
{
	public static EBoundType Next(this EBoundType value)
	{
		switch (value)
		{
		case EBoundType.LessOrEqual:
			return EBoundType.Equals;
		case EBoundType.Less:
			return EBoundType.GreaterOrEqual;
		default:
			Debug.LogException(new ArgumentOutOfRangeException("value", value, "To get next value origin must be < Greater"));
			return EBoundType.Unknown;
		case EBoundType.GreaterOrEqual:
			return EBoundType.Greater;
		case EBoundType.Equals:
			return EBoundType.GreaterOrEqual;
		}
	}
}

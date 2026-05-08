using System;
using UnityEngine;

namespace ChartAndGraph;

[Serializable]
public struct MaterialTiling(bool enable, float value)
{
	public bool EnableTiling = enable;

	public float TileFactor = value;

	public override bool Equals(object obj)
	{
		if (obj is AutoFloat autoFloat)
		{
			if (autoFloat.Automatic && EnableTiling)
			{
				return true;
			}
			if (!autoFloat.Automatic && !EnableTiling)
			{
				return Math.Abs(autoFloat.Value - TileFactor) < Mathf.Epsilon;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (!EnableTiling)
		{
			return TileFactor.GetHashCode();
		}
		return EnableTiling.GetHashCode();
	}
}

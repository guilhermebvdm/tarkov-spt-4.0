using System;
using UnityEngine;

namespace ChartAndGraph;

[Serializable]
public class ChartSubDivisionInfo : ChartDivisionInfo
{
	public override float ValidateTotal(float total)
	{
		return Mathf.Round(total);
	}
}

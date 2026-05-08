using System;
using System.Collections.Generic;
using UnityEngine;
using uLipSync;

namespace Cutscene;

[CreateAssetMenu(menuName = "uLipSync/Backed data with curves")]
public class BakedDataWithCurves : BakedData
{
	[Serializable]
	public class CurveData
	{
		public bool isBlendShape;

		public int blendShapeIndex;

		public string paramName;

		public AnimationCurve curve;

		public float resetValue;
	}

	[SerializeField]
	public List<CurveData> curves;
}

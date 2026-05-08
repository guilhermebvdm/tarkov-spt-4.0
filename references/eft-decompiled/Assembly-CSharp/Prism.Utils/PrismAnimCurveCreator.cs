using UnityEngine;

namespace Prism.Utils;

public class PrismAnimCurveCreator : MonoBehaviour
{
	public float[] curvePointsX;

	public float[] curvePointsY;

	public AnimationCurve thisCurve;

	[ContextMenu("Generate curve")]
	public void method_0()
	{
		thisCurve = new AnimationCurve();
		for (int i = 0; i < curvePointsX.Length; i++)
		{
			thisCurve.AddKey(curvePointsX[i], curvePointsY[i]);
		}
	}
}
